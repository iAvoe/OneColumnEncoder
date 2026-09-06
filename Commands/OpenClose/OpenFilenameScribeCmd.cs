namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the filename scribe modal for configuring the output filename pattern.
/// </summary>
public class OpenFilenameScribeCmd(
    ModalNavS modalNavS,
    ToolItemCardVM outputSettingItem) : OpenCloseBase(modalNavS)
{
    private readonly ToolItemCardVM _outputSettingItem = outputSettingItem;

    /// <summary>
    /// Always allows execution.
    /// </summary>
    public override bool CanExecute(object? parameter) => true;

    /// <summary>
    /// Brings an already-open window to the front; otherwise opens the filename editor and path selector.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<FilenameScribeModal>())
            return;

        FilenameScribeModal window = new();
        FilenameScribeVM vm = new(window.Close, _outputSettingItem);
        PositionFilenameWindow(window);
        window.Loaded += (_, _) => PositionFilenameWindow(window);
        AttachModal(window, vm, closeOpenStack: true);
        _ = window.Dispatcher.BeginInvoke(window.Show);
        new BrowseOutputDirectoryCmd(_outputSettingItem).Execute(null);
    }

    private static void PositionFilenameWindow(Window window)
    {
        Rect workArea = SystemParameters.WorkArea;
        Window? owner = GetSafeOwnerWindow();
        double ownerCenterX = owner == null
            ? workArea.Left + workArea.Width / 2
            : owner.Left + owner.ActualWidth / 2;
        double ownerCenterY = owner == null
            ? workArea.Top + workArea.Height / 2
            : owner.Top + owner.ActualHeight / 2;
        double windowWidth = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
        double windowHeight = window.ActualHeight > 0
            ? window.ActualHeight
            : owner?.ActualHeight > 0 ? owner.ActualHeight : 600;

        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = Math.Clamp(
            ownerCenterX - windowWidth / 2,
            workArea.Left,
            workArea.Right - windowWidth);
        window.Top = Math.Clamp(
            ownerCenterY - windowHeight / 2,
            workArea.Top,
            workArea.Bottom - windowHeight);
    }
}
