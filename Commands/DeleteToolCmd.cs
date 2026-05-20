using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace OneColumnEncoder.Commands
{
    public class DeleteToolCmd(ToolItemVM item, ObservableCollection<ToolItemVM> zone, AppDataM appDataM) : BaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly ObservableCollection<ToolItemVM> _zone = zone;
        private readonly AppDataM _appDataM = appDataM;

        public override void Execute(object? parameter)
        {
            // Zone is an ObservableCollection, so Remove is needed to cleanup properly
            _zone.Remove(_item);

            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(_item.Name);
            if (def?.ExeName == null) return;
            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, string.Empty);
            ToolCatalogProviderM.TrySetVersion(def.ExeName, _appDataM.Tools, string.Empty);

            // File level overwrite
            _appDataM.Save();
        }
    }
}
