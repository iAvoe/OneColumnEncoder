namespace OneColumnEncoder.Commands.Browse;

public class BrowseSrcScriptQueueCmd(
    ToolItemCardVM item,
    SrcFileKind kind,
    AppDataM appDataM,
    string browseKey,
    Action<ToolItemCardVM, SrcFileKind, string, string[]>? afterImport = null,
    Func<string>? getInitialPath = null) : BrowseCmdBase(item)
{
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

        if (ShowDialog(dialog) != true) return;

        string folderPath = dialog.FolderName;
        string[] filePaths = SrcFilePicker.GetSourceFilesInFolder(folderPath, _kind);
        SetQueueCardText(folderPath, filePaths);

        Remember(_appDataM, _browseKey, folderPath);
        _afterImport?.Invoke(_item, _kind, folderPath, filePaths);
        ActivateMainWindow();
    }
}
