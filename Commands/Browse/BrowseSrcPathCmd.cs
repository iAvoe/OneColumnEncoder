using System.IO;

namespace OneColumnEncoder.Commands.Browse;

public class BrowseSrcPathCmd(ToolItemCardVM item,
                                 SrcFileKind fileKind,
                                 AppDataM appDataM,
                                 ModalNavS modalNavS,
                                 string browseKey,
                                 Action<ToolItemCardVM, SrcFileKind, string, bool>? afterImport = null,
                                 Func<string>? getFoundPath = null) : BrowseCmdBase(item)
{
    private readonly SrcFileKind _fileKind = fileKind;
    private readonly AppDataM _appDataM = appDataM;
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly string _browseKey = browseKey;
    private readonly Action<ToolItemCardVM, SrcFileKind, string, bool>? _afterImport = afterImport;
    private readonly Func<string>? _getFoundPath = getFoundPath;

    public override void Execute(object? parameter)
    {
        bool wasReplaced = !string.IsNullOrWhiteSpace(_item.P2TextData);
        string dialogTitle =
            string.Format(UILangProvider.Current["Dialog.SelectTitle"], _item.Name);

        string? foundPath = _fileKind == SrcFileKind.SvfiIni && !string.IsNullOrWhiteSpace(_appDataM.Tools.OneLineShotArgsPath)
            ? Path.Combine(Path.GetDirectoryName(_appDataM.Tools.OneLineShotArgsPath) ?? "", "Configs")
            : _getFoundPath?.Invoke();

        if (_fileKind == SrcFileKind.Video)
            foundPath = null;

        string? currentPath = _fileKind == SrcFileKind.Video ? _item.P2TextData : null;
        string? fallbackPath = string.IsNullOrWhiteSpace(currentPath) ? foundPath : currentPath;

        string? filePath = SrcFilePicker.GetSource(
            _fileKind,
            dialogTitle,
            foundPath: BrowseHistory.ResolveInitialDirectory(_appDataM, _browseKey, fallbackPath),
            currentPath: null);

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        _item.P2TextData = filePath;
        _item.P1TextData = SrcFilePicker.GetPrimaryText(_fileKind, filePath);
        Remember(_appDataM, _browseKey, filePath);
        _afterImport?.Invoke(_item, _fileKind, filePath, wasReplaced);
    }
}
