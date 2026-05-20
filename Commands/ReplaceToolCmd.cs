using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class ReplaceToolCmd(ToolItemVM item, AppDataM appDataM) : AsyncBaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly AppDataM _appDataM = appDataM;

        protected override async Task ExecuteAsync(object? parameter)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(_item.Name);
            if (def?.ExeName == null) return;

            string filter = def.ExeName.Equals("avisynth.dll", StringComparison.OrdinalIgnoreCase)
                ? "DLL files (*.dll)|*.dll"
                : "Executable files (*.exe)|*.exe";

            OpenFileDialog dialog = new()
            {
                Filter = filter,
                Title = $"Replace {_item.Name}",
                CheckFileExists = true,
                CheckPathExists = true
            };

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            string filePath = dialog.FileName;
            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, filePath);
            _item.Path = filePath;
            _appDataM.Save();
        }
    }
}
