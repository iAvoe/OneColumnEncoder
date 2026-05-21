using OneColumnEncoder.ViewModels;
using System;

namespace OneColumnEncoder.Commands
{
    public class ClearToolItemCmd(ToolItemVM item, Action? afterClear = null) : BaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly Action? _afterClear = afterClear;

        public override void Execute(object? parameter)
        {
            _item.Path = string.Empty;
            _item.VersionText = string.Empty;
            _item.IsSelected = false;
            _afterClear?.Invoke();
        }
    }
}
