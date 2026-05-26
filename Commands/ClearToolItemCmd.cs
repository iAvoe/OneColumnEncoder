using OneColumnEncoder.ViewModels.Cards;
using System;

namespace OneColumnEncoder.Commands
{
    public class ClearToolItemCmd(ToolItemCardVM item, Action? afterClear = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly Action? _afterClear = afterClear;

        public override void Execute(object? parameter)
        {
            _item.Path = string.Empty;
            _item.PrimaryValueText = string.Empty;
            _item.IsSelected = false;
            _afterClear?.Invoke();
        }
    }
}
