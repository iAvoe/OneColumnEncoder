using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers;

public static partial class CpuSetsH
{
    private const int CpuSetInformationType = 0;
    private const byte CpuSetAllocatedFlag = 0x02;
    private const uint ThreadSetLimitedInformation = 0x0400;

    public static int GetNodeProcessorCapacity(int nodeId, bool physicalOnly)
    {
        List<CpuSetInfo> cpuSets = GetCpuSetsForNode(nodeId, physicalOnly, null);
        if (cpuSets.Count > 0) return cpuSets.Count;

        NumaNodeInfo? node = NumaTopologyH.GetNumaNodes().FirstOrDefault(n => n.NodeId == nodeId);
        if (node == null) return Math.Max(1, Environment.ProcessorCount);
        if (!physicalOnly) return Math.Max(1, node.ProcessorCount);

        CpuTopologyH.CacheGroupInfo? topology = CpuTopologyH.GetCacheTopology();
        if (topology is { TotalThreads: > 0, TotalCores: > 0 })
        {
            int estimatedPhysicalCores = (int)Math.Round(node.ProcessorCount * (double)topology.TotalCores / topology.TotalThreads);
            return Math.Max(1, estimatedPhysicalCores);
        }

        return Math.Max(1, node.ProcessorCount);
    }

    public static int ClampThreadCountForNode(int nodeId, bool physicalOnly, int requestedThreadCount)
    {
        int capacity = GetNodeProcessorCapacity(nodeId, physicalOnly);
        return Math.Max(1, Math.Min(capacity, requestedThreadCount));
    }

    public static bool TryApplyProcessDefaultCpuSets(
        Process process,
        int nodeId,
        bool physicalOnly,
        int? maxCpuSets,
        out string message)
    {
        message = string.Empty;
        if (!OperatingSystem.IsWindows())
        {
            message = "CPU Sets are only available on Windows.";
            return false;
        }

        try
        {
            uint[] cpuSetIds = GetCpuSetsForNode(nodeId, physicalOnly, maxCpuSets)
                .Select(c => c.Id)
                .ToArray();
            if (cpuSetIds.Length == 0)
            {
                message = $"No CPU Sets found for NUMA node {nodeId}.";
                return false;
            }

            if (!SetProcessDefaultCpuSets(process.Handle, cpuSetIds, (uint)cpuSetIds.Length))
            {
                message = $"SetProcessDefaultCpuSets failed: {Marshal.GetLastWin32Error()}.";
                return false;
            }

            int updatedThreadCount = ApplyCurrentThreadCpuSets(process, cpuSetIds);
            message = $"Bound process {process.Id} to NUMA node {nodeId} with {cpuSetIds.Length} CPU Set(s); updated {updatedThreadCount} existing thread(s).";
            return true;
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            message = $"CPU Sets binding failed: {ex.Message}";
            return false;
        }
    }

    private static List<CpuSetInfo> GetCpuSetsForNode(int nodeId, bool physicalOnly, int? maxCpuSets)
    {
        List<CpuSetInfo> allCpuSets = GetSystemCpuSets();
        if (allCpuSets.Count == 0) return [];

        List<CpuSetInfo> nodeCpuSets = allCpuSets
            .Where(c => c.NumaNodeIndex == nodeId)
            .ToList();

        if (nodeCpuSets.Count == 0)
        {
            NumaNodeInfo? node = NumaTopologyH.GetNumaNodes().FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                nodeCpuSets = allCpuSets
                    .Where(c => c.Group == node.Group
                        && node.MinThreadNum <= c.GlobalLogicalProcessorIndex
                        && c.GlobalLogicalProcessorIndex <= node.MaxThreadNum)
                    .ToList();
            }
        }

        List<CpuSetInfo> availableCpuSets = nodeCpuSets.Where(c => !c.IsAllocated).ToList();
        if (availableCpuSets.Count > 0) nodeCpuSets = availableCpuSets;

        IEnumerable<CpuSetInfo> selected = physicalOnly
            ? nodeCpuSets
                .GroupBy(c => (c.Group, c.CoreIndex))
                .Select(g => g
                    .OrderByDescending(c => c.EfficiencyClass)
                    .ThenBy(c => c.LogicalProcessorIndex)
                    .First())
            : nodeCpuSets;

        selected = selected
            .OrderByDescending(c => c.EfficiencyClass)
            .ThenBy(c => c.Group)
            .ThenBy(c => c.CoreIndex)
            .ThenBy(c => c.LogicalProcessorIndex);

        if (maxCpuSets is > 0)
            selected = selected.Take(maxCpuSets.Value);

        return selected.ToList();
    }

    private static int ApplyCurrentThreadCpuSets(Process process, uint[] cpuSetIds)
    {
        int updatedCount = 0;
        try
        {
            process.Refresh();
            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr threadHandle = OpenThread(ThreadSetLimitedInformation, false, (uint)thread.Id);
                if (threadHandle == IntPtr.Zero) continue;

                try
                {
                    if (SetThreadSelectedCpuSets(threadHandle, cpuSetIds, (uint)cpuSetIds.Length))
                        updatedCount++;
                }
                finally
                {
                    CloseHandle(threadHandle);
                }
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return updatedCount;
        }

        return updatedCount;
    }

    private static List<CpuSetInfo> GetSystemCpuSets()
    {
        if (!OperatingSystem.IsWindows()) return [];

        uint returnedLength;
        GetSystemCpuSetInformation(IntPtr.Zero, 0, out returnedLength, IntPtr.Zero, 0);
        if (returnedLength == 0) return [];

        IntPtr buffer = Marshal.AllocHGlobal((int)returnedLength);
        try
        {
            if (!GetSystemCpuSetInformation(buffer, returnedLength, out returnedLength, IntPtr.Zero, 0))
                return [];

            List<CpuSetInfo> result = [];
            int offset = 0;
            while (offset < returnedLength)
            {
                IntPtr current = buffer + offset;
                SYSTEM_CPU_SET_INFORMATION info = Marshal.PtrToStructure<SYSTEM_CPU_SET_INFORMATION>(current);
                if (info.Size == 0) break;

                if (info.Type == CpuSetInformationType)
                {
                    result.Add(new CpuSetInfo(
                        info.Id,
                        info.Group,
                        info.LogicalProcessorIndex,
                        info.CoreIndex,
                        info.NumaNodeIndex,
                        info.EfficiencyClass,
                        info.AllFlags));
                }

                offset += (int)info.Size;
            }

            return result;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemCpuSetInformation(
        IntPtr information,
        uint bufferLength,
        out uint returnedLength,
        IntPtr process,
        uint flags);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessDefaultCpuSets(
        IntPtr process,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] cpuSetIds,
        uint cpuSetIdCount);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr OpenThread(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint threadId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetThreadSelectedCpuSets(
        IntPtr thread,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] cpuSetIds,
        uint cpuSetIdCount);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_CPU_SET_INFORMATION
    {
        public uint Size;
        public int Type;
        public uint Id;
        public ushort Group;
        public byte LogicalProcessorIndex;
        public byte CoreIndex;
        public byte LastLevelCacheIndex;
        public byte NumaNodeIndex;
        public byte EfficiencyClass;
        public byte AllFlags;
        public uint Reserved;
        public ulong AllocationTag;
    }

    private readonly record struct CpuSetInfo(
        uint Id,
        ushort Group,
        byte LogicalProcessorIndex,
        byte CoreIndex,
        byte NumaNodeIndex,
        byte EfficiencyClass,
        byte AllFlags)
    {
        public int GlobalLogicalProcessorIndex => Group * 64 + LogicalProcessorIndex;
        public bool IsAllocated => (AllFlags & CpuSetAllocatedFlag) != 0;
    }
}
