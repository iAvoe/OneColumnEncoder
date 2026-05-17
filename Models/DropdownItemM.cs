using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public class DropdownItemM(string title, bool isSeparator = false)
    {
        public string Title { get; set; } = title;
        public bool IsSeparator { get; set; } = isSeparator;
    }
}
