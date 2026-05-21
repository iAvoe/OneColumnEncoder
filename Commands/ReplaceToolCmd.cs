using Microsoft.Win32;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System;
using System.Threading.Tasks;
using static OneColumnEncoder.Models.ConfirmationProviderM;

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

            string filter = def.ExeName.Equals("avisynth.dll", StringComparison.OrdinalIgnoreCase)
                ? UILangProviderM.Current["Dialog.Filter.Dll"]
                : UILangProviderM.Current["Dialog.Filter.Exe"];

            OpenFileDialog dialog = new()
            {
                Filter = filter,
                Title = string.Format(UILangProviderM.Current["Dialog.ReplaceTitle"], _item.Name),
                CheckFileExists = true,
                CheckPathExists = true
            };

            string? detectedDir = ToolCatalogProviderM.TryFindToolDirectory(def.ExeName);
            if (detectedDir != null)
            {
                dialog.InitialDirectory = detectedDir;
            }

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            string filePath = dialog.FileName;

            if (!ImportToolCmd.IsFileNameMatch(def.ExeName, filePath))
            {
                if (!ImportToolCmd.ShowDoubleCheckConfirmation(
                    _modalNavS,
                    ConfirmForceImport.GetSuspiciousImportTitle(_item.Name),
                    ConfirmForceImport.GetWrongToolMessage(filePath, _item.Name),
                    def.ExeName,
                    filePath)) return;
            }

            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, filePath);
            _item.Path = filePath;
            _appDataM.Save();
        }
    }
}
