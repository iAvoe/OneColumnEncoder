using OneColumnEncoder.Persistence;
using OneColumnEncoder.CPU;
using System.IO;

namespace OneColumnEncoder.Models
{
    public class ParallelismConfM : SaveLoadBase<ParallelismConfM>
    {
        private static readonly string ConfigFilePath =
            Path.Combine(GetConfigDirectory(), "parallelismconfig.json");

        protected override string FilePath => ConfigFilePath;

        public int UpstreamNodeId { get; set; } = 0;
        public int DownstreamNodeId { get; set; } = 1;
        public bool PreferUpstreamPhysicalCores { get; set; } = false;
        public bool PreferPhysicalCores { get; set; } = true;
        public int EncoderThreadCount { get; set; } = Environment.ProcessorCount;
        public bool UseLargePipeBuffer { get; set; } = false;

        public static ParallelismConfM LoadEffective()
        {
            ParallelismConfM model = Load();
            List<NumaNodeInfo> nodes = NumaTopology.GetNumaNodes();

            int upstreamNodeId = nodes.FirstOrDefault(n => n.NodeId == model.UpstreamNodeId)?.NodeId
                ?? nodes.FirstOrDefault()?.NodeId
                ?? 0;
            int downstreamNodeId = nodes.FirstOrDefault(n => n.NodeId == model.DownstreamNodeId)?.NodeId
                ?? (nodes.Count > 1 ? nodes[1].NodeId : upstreamNodeId);

            return new ParallelismConfM
            {
                UpstreamNodeId = upstreamNodeId,
                DownstreamNodeId = downstreamNodeId,
                PreferUpstreamPhysicalCores = model.PreferUpstreamPhysicalCores,
                PreferPhysicalCores = model.PreferPhysicalCores,
                EncoderThreadCount = CpuSets.ClampThreadCountForNode(
                    downstreamNodeId,
                    model.PreferPhysicalCores,
                    model.EncoderThreadCount),
                UseLargePipeBuffer = model.UseLargePipeBuffer
            };
        }
    }
}
