using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class ReplaceToolCmd(ToolItemCardVM item, AppDataM appDataM, ModalNavS modalNavS, Action? afterReplace = null) : AsyncBaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly AppDataM _appDataM = appDataM;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Action? _afterReplace = afterReplace;

        protected override async Task ExecuteAsync(object? parameter)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(_item.Name);
            if (def?.ExeName == null) return;

            string? filePath = ImportToolCmd.SelectAndValidateToolPath(
                def.ExeName, "Dialog.ReplaceTitle", _modalNavS);
            if (string.IsNullOrEmpty(filePath)) return;

            string? version = await ToolVersionDetectH.TryDetectAsync(def.ExeName, filePath);
            long? fileSize = ToolCatalogProviderM.GetFileSize(filePath);
            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, filePath);
            ToolCatalogProviderM.TrySetVersion(def.ExeName, _appDataM.Tools, version ?? string.Empty);
            ToolCatalogProviderM.TrySetSize(def.ExeName, _appDataM.Tools, fileSize);

            if (def.ExeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase))
            {
                string? y4mArg = await ToolVersionDetectH.DetectVspipeY4mArgAsync(filePath);
                _appDataM.Tools.VspipeY4mArg = y4mArg;
            }

            _item.SetStoredFingerprint(fileSize);
            _item.P2TextData = filePath;
            _item.P1TextData = version ?? string.Empty;
            _appDataM.Save();
            _afterReplace?.Invoke();
        }
    }
}
