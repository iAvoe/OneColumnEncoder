using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class SelectDropdownCmd : BaseCmd
    {
        public override void Execute(object? parameter)
        {
            if (parameter is DropdownItemM selectedItem && !selectedItem.IsSeparator)
            {
                System.Diagnostics.Debug.WriteLine($"Selected: {selectedItem.Title}");
            }
        }
    }
}
