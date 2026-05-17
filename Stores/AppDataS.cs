using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Stores
{
    // Store imported tools (their paths) and version strings
    public class AppDataS
    {
        public Dictionary<string, string> ImportedTools { get; set; } =
            new Dictionary<string, string>();
    }
}
