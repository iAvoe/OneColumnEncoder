using System;
using System.IO;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Models
{
    public class ParallelismConfM : SaveLoadBaseH<ParallelismConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(GetConfigDirectory(), "parallelismconfig.json");

        protected override string FilePath => ConfigFilePath;

        public int UpstreamNodeId { get; set; } = 0;
        public int DownstreamNodeId { get; set; } = 1;
        public bool PreferPhysicalCores { get; set; } = true;
        public bool PreferPCoreCompute { get; set; } = false; // May be too hard for normal users since source code mod needed
        public bool PreferECoreLookahead { get; set; } = false;
        public bool UseLargePages { get; set; } = true;
        public int EncoderThreadCount { get; set; } = Environment.ProcessorCount;
    }
}
