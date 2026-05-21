using OneColumnEncoder.ViewModels;
using System.Collections.ObjectModel;

namespace OneColumnEncoder.Commands
{
    public class RemoveZoneItemCmd(ToolItemVM item, ObservableCollection<ToolItemVM> zone) : BaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly ObservableCollection<ToolItemVM> _zone = zone;

        public override void Execute(object? parameter)
        {
            _zone.Remove(_item);
        }
    }
}
