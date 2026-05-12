using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public class DropdownItemM
    {
        public string Title { get; set; }
        public bool IsSeparator { get; set; }
        public DropdownItemM(string title, bool isSeparator = false)
        {
            Title = title;
            IsSeparator = isSeparator;
        }
    }
}
