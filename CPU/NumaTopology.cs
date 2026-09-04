namespace OneColumnEncoder.CPU;

public class NumaNodeInfo
{
    public int NodeId { get; set; }
    public int Group { get; set; }
    public int ProcessorCount { get; set; }
    public int MinThreadNum { get; set; }
    public int MaxThreadNum { get; set; }
    public long TotalMemoryBytes { get; set; }
    public int HasMemGB => (int)(TotalMemoryBytes / (1024L * 1024 * 1024));
}

public static partial class NumaTopology
{
    /// <summary>
    /// Resolves the processor group and affinity mask for a given NUMA node.
    /// Returns false when the node does not exist or has no processors.
    /// </summary>
    public static bool TryGetNodeGroupMask(int nodeId, out int group, out ulong mask)
    {
        if (LibImportProviderM.TryGetNumaNodeProcessorMaskEx((ushort)nodeId, out int nodeGroup, out ulong nodeMask) && nodeMask != 0)
        {
            group = nodeGroup;
            mask = nodeMask;
            return true;
        }

        group = 0;
        mask = 0;
        return false;
    }

    /// <summary>
    /// Enumerates all NUMA nodes in the system.
    /// Thread ranges are derived from the GROUP_AFFINITY mask bit positions
    /// plus group offset, giving correct global logical-processor IDs.
    /// Memory per node is computed by distributing total physical RAM
    /// proportionally to each node's processor count (the most reliable
    /// method, since no public API returns per-node *total* memory).
    /// </summary>
    public static List<NumaNodeInfo> GetNumaNodes()
    {
        if (!LibImportProviderM.TryGetNumaHighestNodeNumber(out uint highestNodeNumber))
            return CreateFallbackNode();

        List<NumaNodeInfo> nodes = [];

        for (ushort nodeId = 0; nodeId <= highestNodeNumber; nodeId++)
        {
            if (!LibImportProviderM.TryGetNumaNodeProcessorMaskEx(nodeId, out int groupId, out ulong mask))
                continue;
            if (mask == 0) continue;

            int procCount = CountBits(mask);
            if (procCount == 0) continue;

            int minBit = LowBitIndex(mask);
            int maxBit = HighBitIndex(mask);
            int globalMin = groupId * 64 + minBit;
            int globalMax = groupId * 64 + maxBit;

            nodes.Add(new NumaNodeInfo
            {
                NodeId = nodeId,
                Group = groupId,
                ProcessorCount = procCount,
                MinThreadNum = globalMin,
                MaxThreadNum = globalMax
            });
        }

        if (nodes.Count == 0) return CreateFallbackNode();

        long totalMemory = GetTotalPhysicalMemory();
        if (totalMemory > 0) DistributeMemoryByProcessorCount(nodes, totalMemory);

        return nodes;
    }

    private static List<NumaNodeInfo> CreateFallbackNode()
    {
        int procCount = Environment.ProcessorCount;
        long totalMemory = GetTotalPhysicalMemory();
        return
        [
            new NumaNodeInfo
            {
                NodeId = 0,
                Group = 0,
                ProcessorCount = procCount,
                MinThreadNum = 0,
                MaxThreadNum = procCount - 1,
                TotalMemoryBytes = totalMemory > 0 ? totalMemory : 0
            }
        ];
    }

    private static long GetTotalPhysicalMemory()
    {
        return LibImportProviderM.TryGetTotalPhysicalMemoryBytes(out long totalPhysicalBytes)
            ? totalPhysicalBytes
            : 0;
    }

    private static void DistributeMemoryByProcessorCount(List<NumaNodeInfo> nodes, long totalMemory)
    {
        int totalProcs = nodes.Sum(n => n.ProcessorCount);
        if (totalProcs <= 0) return;

        long remaining = totalMemory;
        for (int i = 0; i < nodes.Count; i++)
        {
            long share = i == nodes.Count - 1
                ? remaining
                : totalMemory * nodes[i].ProcessorCount / totalProcs;
            nodes[i].TotalMemoryBytes = Math.Max(0, share);
            remaining -= share;
        }
    }

    private static int CountBits(ulong value)
    {
        int count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }
        return count;
    }

    private static int LowBitIndex(ulong value)
    {
        if (value == 0) return 0;
        int index = 0;
        while ((value & 1) == 0)
        {
            value >>= 1;
            index++;
        }
        return index;
    }

    private static int HighBitIndex(ulong value)
    {
        if (value == 0) return 0;
        int index = 0;
        while (value != 0)
        {
            value >>= 1;
            index++;
        }
        return index - 1;
    }
}
