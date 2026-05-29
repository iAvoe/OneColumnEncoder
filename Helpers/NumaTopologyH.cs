using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers;

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

public static partial class NumaTopologyH
{
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNumaHighestNodeNumber(out uint highestNodeNumber);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNumaNodeProcessorMaskEx(ushort nodeNumber, out GROUP_AFFINITY groupMask);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct GROUP_AFFINITY
    {
        public ulong Mask;
        public ushort Group;
        public ushort Reserved1;
        public ushort Reserved2;
        public ushort Reserved3;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
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
        if (!GetNumaHighestNodeNumber(out uint highestNodeNumber))
            return CreateFallbackNode();

        var nodes = new List<NumaNodeInfo>();

        for (ushort nodeId = 0; nodeId <= highestNodeNumber; nodeId++)
        {
            if (!GetNumaNodeProcessorMaskEx(nodeId, out var groupMask))
                continue;
            if (groupMask.Mask == 0)
                continue;

            int procCount = CountBits(groupMask.Mask);
            if (procCount == 0)
                continue;

            int minBit = LowBitIndex(groupMask.Mask);
            int maxBit = HighBitIndex(groupMask.Mask);
            int globalMin = groupMask.Group * 64 + minBit;
            int globalMax = groupMask.Group * 64 + maxBit;

            nodes.Add(new NumaNodeInfo
            {
                NodeId = nodeId,
                Group = groupMask.Group,
                ProcessorCount = procCount,
                MinThreadNum = globalMin,
                MaxThreadNum = globalMax
            });
        }

        if (nodes.Count == 0)
            return CreateFallbackNode();

        long totalMemory = GetTotalPhysicalMemory();
        if (totalMemory > 0)
            DistributeMemoryByProcessorCount(nodes, totalMemory);

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
        var memStatus = new MEMORYSTATUSEX
        {
            dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
        };
        return GlobalMemoryStatusEx(ref memStatus) ? (long)memStatus.ullTotalPhys : 0;
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
