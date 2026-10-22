using System.IO;

namespace OneColumnEncoder.Views;

/// <summary>
/// Interaction logic for MainUI.xaml
///
/// Handles the drag &amp; drop of video files onto the main window:
/// the view owns the OLE drag events and the static "release to import" feedback,
/// the ViewModel owns the import rules.
/// </summary>
public partial class MainUI : UserControl
{
    private string[]? _dragPaths;
    private string[] _dragExistingFiles = [];

    public MainUI()
    {
        InitializeComponent();
        // Just inherit DataContext from parent (MainVM), so no need to set it here
    }

    private MainVM? MainViewModel => DataContext as MainVM;

    #region Drag & Drop
    private void OnDragOver(object sender, DragEventArgs e) => HandleDragOver(e);

    private void OnDragLeave(object sender, DragEventArgs e) => HandleDragLeave(e);

    private void OnDrop(object sender, DragEventArgs e) => HandleDrop(e);

    /// <summary>
    /// Entry point for hosts that own the drag events (e.g. the window's ScrollViewer).
    /// </summary>
    public void ForwardDragOver(DragEventArgs e) => HandleDragOver(e);

    public void ForwardDragLeave(DragEventArgs e) => HandleDragLeave(e);

    public void ForwardDrop(DragEventArgs e) => HandleDrop(e);

    private void HandleDragOver(DragEventArgs e)
    {
        e.Handled = true;

        MainVM? vm = MainViewModel;
        if (vm == null)
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        string[]? paths = GetDroppedPaths(e);
        string[] existingFiles = GetExistingFiles(paths);
        bool canDrop = vm.CanAcceptDroppedFiles(existingFiles);

        e.Effects = canDrop ? DragDropEffects.Copy : DragDropEffects.None;
        vm.SetDropTargetHighlight(canDrop, existingFiles.Length);
    }

    private void HandleDragLeave(DragEventArgs e)
    {
        e.Handled = true;
        ResetDragCache();
        MainViewModel?.SetDropTargetHighlight(false);
    }

    private void HandleDrop(DragEventArgs e)
    {
        e.Handled = true;

        MainVM? vm = MainViewModel;
        vm?.SetDropTargetHighlight(false);

        if (vm == null) return;

        string[]? paths = GetDroppedPaths(e);
        ResetDragCache();
        if (paths == null || paths.Length == 0) return;

        // The OLE drag source is still waiting for the drop to complete, so the import
        // (which may open a modal) is deferred until the drop handler returns.
        Dispatcher.InvokeAsync(new Action(() => vm.ImportDroppedFiles(paths)));
    }

    private static string[]? GetDroppedPaths(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return null;
        return e.Data.GetData(DataFormats.FileDrop) as string[];
    }

    // The payload does not change while dragging, so the file system lookups are cached.
    private string[] GetExistingFiles(string[]? paths)
    {
        if (paths == null) return [];

        if (_dragPaths != null && _dragPaths.SequenceEqual(paths))
            return _dragExistingFiles;

        _dragPaths = paths;
        _dragExistingFiles = [.. paths.Where(File.Exists)];
        return _dragExistingFiles;
    }

    private void ResetDragCache()
    {
        _dragPaths = null;
        _dragExistingFiles = [];
    }
    #endregion
}
