using System.Collections.ObjectModel;

namespace OneColumnEncoder.Commands
{
    public class DeleteToolCmd(ToolItemCardVM item, ObservableCollection<ToolItemCardVM> zone, AppDataM appDataM) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly ObservableCollection<ToolItemCardVM> _zone = zone;
        private readonly AppDataM _appDataM = appDataM;

        public override void Execute(object? parameter)
        {
            // Zone is an ObservableCollection, so Remove is needed to cleanup properly
            _zone.Remove(_item);

            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(_item.Name);
            if (def?.ExeName == null) return;
            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, string.Empty);
            ToolCatalogProviderM.TrySetVersion(def.ExeName, _appDataM.Tools, string.Empty);
            ToolCatalogProviderM.TrySetSize(def.ExeName, _appDataM.Tools, null);

            if (def.Zone == ToolZone.Analytics)
            {
                _item.IsSelected = false;
                _item.IsCancel = false;
            }

            // File level overwrite
            _appDataM.Save();
        }
    }
}
