using OneColumnEncoder.ViewModels.Cards;
using System.Collections.ObjectModel;

namespace OneColumnEncoder.Commands
{
    public class RemoveZoneItemCmd(ToolItemCardVM item, ObservableCollection<ToolItemCardVM> zone) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly ObservableCollection<ToolItemCardVM> _zone = zone;

        public override void Execute(object? parameter)
        {
            _zone.Remove(_item);
        }
    }
}
