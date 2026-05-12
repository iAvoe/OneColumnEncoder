using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public class EncItemM
    {
        public Guid Id = Guid.NewGuid();
        public string Name { get; set; }
        public string Path { get; set; } = ""; // Only available in UpstreamTools, Encoders, AnalyticTools, Import zones
        public EncItemM(string name) => Name = name;
    }
}