using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.Models;

public static partial class LibImportProviderM
{
    private const uint TH32CS_SNAPPROCESS = 0x00000002;
    private static readonly IntPtr InvalidHandleValue = new(-1);

    public static int CompareLogical(string? x, string? y)
    {
        string xName = Path.GetFileName(x ?? string.Empty);
        string yName = Path.GetFileName(y ?? string.Empty);
        int result = StrCmpLogicalWNative(xName, yName);
        return result != 0
            ? result
            : StringComparer.OrdinalIgnoreCase.Compare(x, y);
    }

    public static long GetPageFaultCount(Process process)
    {
        try
        {
            if (process.HasExited) return 0L;
            process.Refresh();

            PROCESS_MEMORY_COUNTERS counters = new()
            {
                cb = (uint)Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>()
            };

            return GetProcessMemoryInfo(process.Handle, ref counters, counters.cb)
                ? counters.PageFaultCount
                : 0L;
        }
        catch
        {
            return 0L;
        }
    }

    public static bool TryGetLogicalProcessorInformationEx(int relationshipType, IntPtr buffer, ref uint returnedLength) =>
        GetLogicalProcessorInformationExNative((LOGICAL_PROCESSOR_RELATIONSHIP)relationshipType, buffer, ref returnedLength);

    public static bool TryGetNumaHighestNodeNumber(out uint highestNodeNumber) =>
        GetNumaHighestNodeNumberNative(out highestNodeNumber);

    public static bool TryGetNumaNodeProcessorMaskEx(ushort nodeNumber, out int group, out ulong mask)
    {
        if (GetNumaNodeProcessorMaskExNative(nodeNumber, out GROUP_AFFINITY groupMask) && groupMask.Mask != 0)
        {
            group = groupMask.Group;
            mask = groupMask.Mask;
            return true;
        }

        group = 0;
        mask = 0;
        return false;
    }

    public static bool TryGetTotalPhysicalMemoryBytes(out long totalPhysicalBytes)
    {
        totalPhysicalBytes = 0;
        if (!OperatingSystem.IsWindows()) return false;

        MEMORYSTATUSEX memStatus = new()
        {
            dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
        };

        if (!GlobalMemoryStatusExNative(ref memStatus)) return false;

        totalPhysicalBytes = (long)memStatus.ullTotalPhys;
        return true;
    }

    public static bool TryGetSystemCpuSetInformation(
        IntPtr information,
        uint bufferLength,
        out uint returnedLength,
        IntPtr process,
        uint flags) =>
        GetSystemCpuSetInformationNative(information, bufferLength, out returnedLength, process, flags);

    public static bool TrySetProcessDefaultCpuSets(IntPtr process, uint[] cpuSetIds, uint cpuSetIdCount) =>
        SetProcessDefaultCpuSetsNative(process, cpuSetIds, cpuSetIdCount);

    public static IntPtr OpenThread(uint desiredAccess, bool inheritHandle, uint threadId) =>
        OpenThreadNative(desiredAccess, inheritHandle, threadId);

    public static bool TrySetThreadSelectedCpuSets(IntPtr thread, uint[] cpuSetIds, uint cpuSetIdCount) =>
        SetThreadSelectedCpuSetsNative(thread, cpuSetIds, cpuSetIdCount);

    public static bool TryCloseHandle(IntPtr handle) =>
        CloseHandleNative(handle);

    public static bool TryGetSystemPowerStatus(out byte acLineStatus, out byte batteryFlag)
    {
        acLineStatus = 0;
        batteryFlag = 0;

        if (!GetSystemPowerStatusNative(out SYSTEM_POWER_STATUS status))
            return false;

        acLineStatus = status.ACLineStatus;
        batteryFlag = status.BatteryFlag;
        return true;
    }

    public static bool TryGetSystemTimes(out ulong idleTime, out ulong kernelTime, out ulong userTime) =>
        GetSystemTimesNative(out idleTime, out kernelTime, out userTime);

    public static bool TryGetSystemProcessorPerformanceInformation(
        ushort processorGroup,
        IntPtr processorInformation,
        uint byteLength,
        out uint returnedLength)
        => GetSystemProcessorPerformanceInformationNative(processorGroup, processorInformation, byteLength, out returnedLength);

    public static uint GetActiveProcessorCount(ushort groupNumber) =>
        GetActiveProcessorCountNative(groupNumber);

    public static IntPtr MonitorFromWindow(IntPtr hwnd, int flags) =>
        MonitorFromWindowNative(hwnd, flags);

    public static bool TryGetMonitorInfo(IntPtr hMonitor, out int left, out int top, out int right, out int bottom)
    {
        MONITORINFO info = new()
        {
            cbSize = Marshal.SizeOf<MONITORINFO>()
        };

        if (!GetMonitorInfoNative(hMonitor, ref info))
        {
            left = top = right = bottom = 0;
            return false;
        }

        left = info.rcWork.Left;
        top = info.rcWork.Top;
        right = info.rcWork.Right;
        bottom = info.rcWork.Bottom;
        return true;
    }

    public static IntPtr GetSystemMenu(IntPtr hWnd, bool revert) =>
        GetSystemMenuNative(hWnd, revert);

    public static bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable) =>
        EnableMenuItemNative(hMenu, uIDEnableItem, uEnable);

    public static bool DrawMenuBar(IntPtr hWnd) =>
        DrawMenuBarNative(hWnd);

    public static bool TryGetOpenClPlatformCount(out uint platformCount)
    {
        platformCount = 0;

        if (!OperatingSystem.IsWindows())
            return false;

        if (!NativeLibrary.TryLoad("OpenCL.dll", out IntPtr library))
            return false;

        try
        {
            int result = ClGetPlatformIDsNative(0, IntPtr.Zero, out platformCount);
            return result == 0 && platformCount > 0;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
        finally
        {
            NativeLibrary.Free(library);
        }
    }

    public static Dictionary<int, List<int>> GetChildProcessMap()
    {
        Dictionary<int, List<int>> childIdsByParentId = [];
        if (!OperatingSystem.IsWindows()) return childIdsByParentId;

        IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (snapshot == InvalidHandleValue) return childIdsByParentId;

        try
        {
            PROCESSENTRY32 entry = new()
            {
                dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>()
            };

            if (!Process32First(snapshot, ref entry)) return childIdsByParentId;

            do
            {
                int parentProcessId = unchecked((int)entry.th32ParentProcessID);
                int processId = unchecked((int)entry.th32ProcessID);
                if (!childIdsByParentId.TryGetValue(parentProcessId, out List<int>? childIds))
                {
                    childIds = [];
                    childIdsByParentId[parentProcessId] = childIds;
                }

                childIds.Add(processId);
                entry.dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>();
            }
            while (Process32Next(snapshot, ref entry));

            return childIdsByParentId;
        }
        finally
        {
            CloseHandle(snapshot);
        }
    }

    public static MemoryStatusSnapshot GetMemoryStatusSnapshot()
    {
        if (!OperatingSystem.IsWindows()) return default;

        MEMORYSTATUSEX memoryStatus = new()
        {
            dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
        };

        if (!GlobalMemoryStatusExNative(ref memoryStatus)) return default;

        long totalPhysicalBytes = ToNonNegativeLong(memoryStatus.ullTotalPhys);
        long availablePhysicalBytes = Math.Min(totalPhysicalBytes, ToNonNegativeLong(memoryStatus.ullAvailPhys));
        long commitLimitBytes = ToNonNegativeLong(memoryStatus.ullTotalPageFile);
        long commitAvailableBytes = ToNonNegativeLong(memoryStatus.ullAvailPageFile);
        long committedBytes = Math.Max(0, commitLimitBytes - commitAvailableBytes);
        long systemCacheBytes = 0;

        PERFORMANCE_INFORMATION performanceInfo = new()
        {
            cb = (uint)Marshal.SizeOf<PERFORMANCE_INFORMATION>()
        };

        if (GetPerformanceInfo(ref performanceInfo, performanceInfo.cb) && performanceInfo.PageSize != 0)
        {
            ulong pageSize = performanceInfo.PageSize;
            totalPhysicalBytes = ToNonNegativeLong(performanceInfo.PhysicalTotal * pageSize);
            availablePhysicalBytes = Math.Min(totalPhysicalBytes, ToNonNegativeLong(performanceInfo.PhysicalAvailable * pageSize));
            commitLimitBytes = ToNonNegativeLong(performanceInfo.CommitLimit * pageSize);
            committedBytes = Math.Min(commitLimitBytes, ToNonNegativeLong(performanceInfo.CommitTotal * pageSize));
            systemCacheBytes = Math.Min(totalPhysicalBytes, ToNonNegativeLong(performanceInfo.SystemCache * pageSize));
        }

        return new MemoryStatusSnapshot(
            totalPhysicalBytes,
            availablePhysicalBytes,
            commitLimitBytes,
            committedBytes,
            systemCacheBytes,
            Math.Clamp((int)memoryStatus.dwMemoryLoad, 0, 100));
    }

    private static long ToNonNegativeLong(ulong value) =>
        value > long.MaxValue ? long.MaxValue : (long)value;

    [LibraryImport("shlwapi.dll", EntryPoint = "StrCmpLogicalW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int StrCmpLogicalWNative(string x, string y);

    [LibraryImport("kernel32.dll", EntryPoint = "GetLogicalProcessorInformationEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetLogicalProcessorInformationExNative(
        LOGICAL_PROCESSOR_RELATIONSHIP relationshipType,
        IntPtr buffer,
        ref uint returnedLength);

    [LibraryImport("kernel32.dll", EntryPoint = "GetNumaHighestNodeNumber", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNumaHighestNodeNumberNative(out uint highestNodeNumber);

    [LibraryImport("kernel32.dll", EntryPoint = "GetNumaNodeProcessorMaskEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNumaNodeProcessorMaskExNative(ushort nodeNumber, out GROUP_AFFINITY groupMask);

    [LibraryImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalMemoryStatusExNative(ref MEMORYSTATUSEX lpBuffer);

    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemCpuSetInformation", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemCpuSetInformationNative(
        IntPtr information,
        uint bufferLength,
        out uint returnedLength,
        IntPtr process,
        uint flags);

    [LibraryImport("kernel32.dll", EntryPoint = "SetProcessDefaultCpuSets", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessDefaultCpuSetsNative(
        IntPtr process,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] cpuSetIds,
        uint cpuSetIdCount);

    [LibraryImport("kernel32.dll", EntryPoint = "OpenThread", SetLastError = true)]
    private static partial IntPtr OpenThreadNative(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint threadId);

    [LibraryImport("kernel32.dll", EntryPoint = "SetThreadSelectedCpuSets", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetThreadSelectedCpuSetsNative(
        IntPtr thread,
        [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] uint[] cpuSetIds,
        uint cpuSetIdCount);

    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandleNative(IntPtr handle);

    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemPowerStatus", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemPowerStatusNative(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemTimes", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemTimesNative(out ulong lpIdleTime, out ulong lpKernelTime, out ulong lpUserTime);

    [LibraryImport("kernel32.dll", EntryPoint = "GetActiveProcessorCount", SetLastError = true)]
    private static partial uint GetActiveProcessorCountNative(ushort groupNumber);

    [LibraryImport("user32.dll", EntryPoint = "GetSystemMenu")]
    private static partial IntPtr GetSystemMenuNative(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    [LibraryImport("user32.dll", EntryPoint = "EnableMenuItem")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableMenuItemNative(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [LibraryImport("user32.dll", EntryPoint = "DrawMenuBar")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DrawMenuBarNative(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "MonitorFromWindow")]
    private static partial IntPtr MonitorFromWindowNative(IntPtr hwnd, int dwFlags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfoNative(IntPtr hMonitor, ref MONITORINFO lpmi);

    [LibraryImport("OpenCL", EntryPoint = "clGetPlatformIDs")]
    private static partial int ClGetPlatformIDsNative(
        uint numEntries,
        IntPtr platforms,
        out uint numPlatforms);

    [LibraryImport("kernel32.dll", EntryPoint = "GetSystemProcessorPerformanceInformation")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemProcessorPerformanceInformationNative(
        ushort processorGroup,
        IntPtr processorInformation,
        uint byteLength,
        out uint returnedLength);

    [LibraryImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetProcessMemoryInfo(IntPtr Process, ref PROCESS_MEMORY_COUNTERS ppsmemCounters, uint cb);

    [LibraryImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [LibraryImport("kernel32.dll", EntryPoint = "Process32FirstW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [LibraryImport("kernel32.dll", EntryPoint = "Process32NextW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORYSTATUSEX
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_MEMORY_COUNTERS
    {
        public uint cb;
        public uint PageFaultCount;
        public nuint PeakWorkingSetSize;
        public nuint WorkingSetSize;
        public nuint QuotaPeakPagedPoolUsage;
        public nuint QuotaPagedPoolUsage;
        public nuint QuotaPeakNonPagedPoolUsage;
        public nuint QuotaNonPagedPoolUsage;
        public nuint PagefileUsage;
        public nuint PeakPagefileUsage;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PERFORMANCE_INFORMATION
    {
        public uint cb;
        public nuint CommitTotal;
        public nuint CommitLimit;
        public nuint CommitPeak;
        public nuint PhysicalTotal;
        public nuint PhysicalAvailable;
        public nuint SystemCache;
        public nuint KernelTotal;
        public nuint KernelPaged;
        public nuint KernelNonpaged;
        public nuint PageSize;
        public uint HandleCount;
        public uint ProcessCount;
        public uint ThreadCount;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        public fixed char szExeFile[260];
    }

    public readonly record struct MemoryStatusSnapshot(
        long TotalPhysicalBytes,
        long AvailablePhysicalBytes,
        long CommitLimitBytes,
        long CommittedBytes,
        long SystemCacheBytes,
        int MemoryLoadPercent)
    {
        public long UsedPhysicalBytes => Math.Max(0, TotalPhysicalBytes - AvailablePhysicalBytes);
    }

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
    private struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte Reserved1;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESSOR_POWER_INFORMATION
    {
        public uint CurrentFrequency;
        public uint ThermalLimitFrequency;
        public ulong IdleTime;
        public ulong KernelTime;
        public ulong UserTime;
        public ulong DpcTime;
        public ulong InterruptTime;
        public uint IsIdle;
    }

    private enum LOGICAL_PROCESSOR_RELATIONSHIP : int
    {
        RelationProcessorCore = 0,
        RelationNumaNode = 1,
        RelationCache = 2,
        RelationProcessorPackage = 3,
        RelationGroup = 4
    }
}
