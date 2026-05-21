using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class ReplaceToolCmd(ToolItemVM item, AppDataM appDataM, ModalNavS modalNavS) : AsyncBaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly AppDataM _appDataM = appDataM;
        private readonly ModalNavS _modalNavS = modalNavS;

        protected override async Task ExecuteAsync(object? parameter)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(_item.Name);
            if (def?.ExeName == null) return;

            string? filePath = ImportToolCmd.SelectAndValidateToolPath(
                def.ExeName, "Dialog.ReplaceTitle", _modalNavS);
            if (string.IsNullOrEmpty(filePath)) return;

            string? version = await ToolVersionDetector.TryDetectAsync(def.ExeName, filePath);
            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, filePath);
            ToolCatalogProviderM.TrySetVersion(def.ExeName, _appDataM.Tools, version ?? string.Empty);

            if (def.ExeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase))
            {
                string? y4mArg = await ToolVersionDetector.DetectVspipeY4mArgAsync(filePath);
                _appDataM.Tools.VspipeY4mArg = y4mArg;
            }

            _item.Path = filePath;
            _item.VersionText = version ?? string.Empty;
            _appDataM.Save();
        }
    }
}
