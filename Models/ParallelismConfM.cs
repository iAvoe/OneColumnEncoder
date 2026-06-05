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

        public static ParallelismConfM LoadEffective()
        {
            ParallelismConfM model = Load();
            List<NumaNodeInfo> nodes = NumaTopologyH.GetNumaNodes();

            int upstreamNodeId = nodes.FirstOrDefault(n => n.NodeId == model.UpstreamNodeId)?.NodeId
                ?? nodes.FirstOrDefault()?.NodeId
                ?? 0;
            int downstreamNodeId = nodes.FirstOrDefault(n => n.NodeId == model.DownstreamNodeId)?.NodeId
                ?? (nodes.Count > 1 ? nodes[1].NodeId : upstreamNodeId);

            return new ParallelismConfM
            {
                UpstreamNodeId = upstreamNodeId,
                DownstreamNodeId = downstreamNodeId,
                PreferPhysicalCores = model.PreferPhysicalCores,
                PreferPCoreCompute = model.PreferPCoreCompute,
                PreferECoreLookahead = model.PreferECoreLookahead,
                UseLargePages = model.UseLargePages,
                EncoderThreadCount = CpuSetsH.ClampThreadCountForNode(
                    downstreamNodeId,
                    model.PreferPhysicalCores,
                    model.EncoderThreadCount)
            };
        }
    }
}
