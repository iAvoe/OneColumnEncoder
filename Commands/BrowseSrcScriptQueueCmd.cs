using System.IO;

namespace OneColumnEncoder.Commands;

public class BrowseSrcScriptQueueCmd(
    ToolItemCardVM item,
    SrcFileKind kind,
    AppDataM appDataM,
    string browseKey,
    Action<ToolItemCardVM, SrcFileKind, string, string[]>? afterImport = null,
    Func<string>? getInitialPath = null) : BaseCmd
{
    private readonly ToolItemCardVM _item = item;
    private readonly SrcFileKind _kind = kind;
    private readonly AppDataM _appDataM = appDataM;
    private readonly string _browseKey = browseKey;
    private readonly Action<ToolItemCardVM, SrcFileKind, string, string[]>? _afterImport = afterImport;
    private readonly Func<string>? _getInitialPath = getInitialPath;

    public override void Execute(object? parameter)
    {
        OpenFolderDialog dialog = new()
        {
            Title = UILangProvider.Current["SourceQueue.SelectFolderTitle"],
            InitialDirectory = BrowseHistory.ResolveInitialDirectory(_appDataM, _browseKey, _getInitialPath?.Invoke() ?? _item.P2TextData)
        };

        Window? owner = Application.Current.MainWindow;
        bool? result = owner is null
            ? dialog.ShowDialog()
            : dialog.ShowDialog(owner);
        if (result != true) return;

        string folderPath = dialog.FolderName;
        string[] filePaths = SrcFilePicker.GetSourceFilesInFolder(folderPath, _kind);
        // Extract file names for both short card display and long tooltip display
        string[] fileNames = [.. filePaths.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!)];

        _item.P2TextData = folderPath;
        _item.P1TextData = BrowseSrcQueueCmd.FormatQueueP1Text(fileNames);
        _item.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(fileNames);
        BrowseHistory.Remember(_appDataM, _browseKey, folderPath);
        _afterImport?.Invoke(_item, _kind, folderPath, filePaths);
        Application.Current.MainWindow?.Activate();
    }
}
