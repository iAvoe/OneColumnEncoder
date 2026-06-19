using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.Commands
{
    public class ClearToolItemCmd(ToolItemCardVM item, Action? afterClear = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly Action? _afterClear = afterClear;

        public override void Execute(object? parameter)
        {
            _item.P2TextData = string.Empty;
            _item.P1TextData = string.Empty;
            _item.P1TooltipText = null; // Reset tooltip to fall back to P1TextData
            _item.IsSelected = false;
            _afterClear?.Invoke();
        }
    }
}
