using OneColumnEncoder.ToolManagement;

namespace OneColumnEncoder.Commands;

public class ReplaceToolCmd(ToolItemCardVM item, AppDataM appDataM, ModalNavS modalNavS, Action? afterReplace = null) : AsyncBaseCmd
{
    private readonly ToolItemCardVM _item = item;
    private readonly AppDataM _appDataM = appDataM;
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly Action? _afterReplace = afterReplace;

    protected override async Task ExecuteAsync(object? parameter)
    {
        ToolDefinitionM? def = _item.DefinitionKey == null
            ? null
            : ToolDefinitionProviderM.GetByKey(_item.DefinitionKey);
        if (def?.ExeName == null) return;

        string browseKey = BrowseHistoryKeys.ForTool(def.ExeName);
        string? filePath = ImportToolCmd.SelectAndValidateToolPath(
            def.ExeName, "Dialog.ReplaceTitle", _modalNavS,
            BrowseHistory.GetDirectory(_appDataM, browseKey));
        if (string.IsNullOrEmpty(filePath)) return;

        string? version;
        try
        {
            version = await ToolVersionDetect.TryDetectAsync(def.ExeName, filePath);
        }
        catch (ToolVersionDetectTimeoutException)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UILangProvider.Current["Import.VersionDetectTimeoutTitle"],
                string.Format(UILangProvider.Current["Import.VersionDetectTimeoutMessage"], def.ExeName)).Execute(null);
            return;
        }

        await ToolVersionDetect.DetectAndStoreVspipeY4mArgAsync(
            def.ExeName,
            filePath,
            y4mArg => _appDataM.Tools.VspipeY4mArg = y4mArg);
        long? fileSize = ToolCatalogProviderM.GetFileSize(filePath);
        ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, filePath);
        ToolCatalogProviderM.TrySetVersion(def.ExeName, _appDataM.Tools, version ?? string.Empty);
        ToolCatalogProviderM.TrySetSize(def.ExeName, _appDataM.Tools, fileSize);

        _item.SetStoredFingerprint(fileSize);
        _item.P2TextData = filePath;
        _item.P1TextData = version ?? string.Empty;
        _item.IsCancel = false;
        BrowseHistory.Remember(_appDataM, browseKey, filePath);
        _appDataM.Save();
        _afterReplace?.Invoke();
    }
}
