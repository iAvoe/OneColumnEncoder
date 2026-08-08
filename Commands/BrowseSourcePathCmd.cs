using System.IO;

namespace OneColumnEncoder.Commands;

public class BrowseSourcePathCmd(ToolItemCardVM item,
                                 SourceFileKind fileKind,
                                 AppDataM appDataM,
                                 ModalNavS modalNavS,
                                 string browseKey,
                                 Action<ToolItemCardVM, SourceFileKind, string, bool>? afterImport = null,
                                 Func<string>? getFoundPath = null) : BaseCmd
{
    private readonly ToolItemCardVM _item = item;
    private readonly SourceFileKind _fileKind = fileKind;
    private readonly AppDataM _appDataM = appDataM;
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly string _browseKey = browseKey;
    private readonly Action<ToolItemCardVM, SourceFileKind, string, bool>? _afterImport = afterImport;
    private readonly Func<string>? _getFoundPath = getFoundPath;

    public override void Execute(object? parameter)
    {
        bool wasReplaced = !string.IsNullOrWhiteSpace(_item.P2TextData);
        string dialogTitle =
            string.Format(UILangProvider.Current["Dialog.SelectTitle"], _item.Name);

        string? foundPath = _fileKind == SourceFileKind.SvfiIni && !string.IsNullOrWhiteSpace(_appDataM.Tools.OneLineShotArgsPath)
            ? Path.Combine(Path.GetDirectoryName(_appDataM.Tools.OneLineShotArgsPath) ?? "", "Configs")
            : _getFoundPath?.Invoke();

        if (_fileKind == SourceFileKind.Video)
            foundPath = null;

        string? currentPath = _fileKind == SourceFileKind.Video ? _item.P2TextData : null;
        string? fallbackPath = string.IsNullOrWhiteSpace(currentPath) ? foundPath : currentPath;

        string? filePath = SourceFilePicker.GetSource(
            _fileKind,
            dialogTitle,
            foundPath: BrowseHistory.ResolveInitialDirectory(_appDataM, _browseKey, fallbackPath),
            currentPath: null);

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        _item.P2TextData = filePath;
        _item.P1TextData = SourceFilePicker.GetPrimaryText(_fileKind, filePath);
        BrowseHistory.Remember(_appDataM, _browseKey, filePath);
        _afterImport?.Invoke(_item, _fileKind, filePath, wasReplaced);
    }
}
