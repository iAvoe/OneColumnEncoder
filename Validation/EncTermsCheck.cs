using OneColumnEncoder.CPU;
using System.IO;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.Validation;

public static partial class EncTermsCheck
{
    private const double NumaCpuUsageHighThreshold = 0.5;
    private static DateTime _lastNumaCpuCheck = DateTime.MinValue;
    private static StatusType _lastNumaCpuStatus = StatusType.Waiting;
    private static ulong _lastIdleTicks;
    private static ulong _lastKernelTicks;
    private static ulong _lastUserTicks;

    private static int _lastNumaNodeId = -1;
    private static StatusType _lastNumaNodeCpuStatus = StatusType.Waiting;
    private static ulong[] _lastNodeIdleTicks = [];
    private static ulong[] _lastNodeKernelTicks = [];
    private static ulong[] _lastNodeUserTicks = [];

    #region Win32 P/Invoke
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

    private const byte AC_LINE_ONLINE = 1;
    private const byte AC_LINE_UNKNOWN = 255;
    private const byte BATTERY_FLAG_NO_BATTERY = 128;
    private const byte BATTERY_FLAG_CHARGING = 8;
    #endregion

    #region NUMA node CPU usage check (on interaction trigger)

    public static StatusType EvaluateNumaNodeCpuUsage()
    {
        if (!OperatingSystem.IsWindows())
            return StatusType.Success;

        if (!LibImportProvider.TryGetSystemTimes(out ulong idle, out ulong kernel, out ulong user))
            return StatusType.Success;

        if (_lastNumaCpuCheck == DateTime.MinValue)
        {
            _lastIdleTicks = idle;
            _lastKernelTicks = kernel;
            _lastUserTicks = user;
            _lastNumaCpuCheck = DateTime.UtcNow;
            return StatusType.Waiting;
        }

        ulong totalDelta = (kernel - _lastKernelTicks) + (user - _lastUserTicks);
        ulong idleDelta = idle - _lastIdleTicks;

        _lastIdleTicks = idle;
        _lastKernelTicks = kernel;
        _lastUserTicks = user;
        _lastNumaCpuCheck = DateTime.UtcNow;

        if (totalDelta == 0)
            return _lastNumaCpuStatus;

        double usage = (double)(totalDelta - idleDelta) / totalDelta;
        usage = Math.Clamp(usage, 0, 1);

        _lastNumaCpuStatus = usage > NumaCpuUsageHighThreshold
            ? StatusType.Warning
            : StatusType.Success;
        return _lastNumaCpuStatus;
    }

    #endregion

    #region Per-node CPU usage check (delta between successive calls)

    /// <summary>
    /// Evaluates recent CPU usage of the given NUMA node by sampling per-core
    /// idle/kernel/user counters on the first call and computing the delta on
    /// subsequent calls. Falls back to the system-wide measurement when the
    /// per-core counters are unavailable (e.g. non-Windows or legacy OS).
    /// </summary>
    public static StatusType EvaluateNumaNodeCpuUsage(int nodeId)
    {
        if (!OperatingSystem.IsWindows())
            return StatusType.Success;

        if (!NumaTopology.TryGetNodeGroupMask(nodeId, out int group, out ulong mask))
            return EvaluateNumaNodeCpuUsage();

        if (!TryReadGroupProcessorCounters((ushort)group, out ulong[] idle, out ulong[] kernel, out ulong[] user))
            return EvaluateNumaNodeCpuUsage();

        if (_lastNumaNodeId != nodeId || _lastNodeIdleTicks.Length == 0)
        {
            _lastNumaNodeId = nodeId;
            _lastNodeIdleTicks = idle;
            _lastNodeKernelTicks = kernel;
            _lastNodeUserTicks = user;
            return StatusType.Waiting;
        }

        double busySum = 0;
        int counted = 0;

        for (int bit = 0; bit < 64; bit++)
        {
            if ((mask & (1UL << bit)) == 0) continue;
            if (bit >= idle.Length || bit >= _lastNodeIdleTicks.Length) continue;

            ulong idleDelta = idle[bit] - _lastNodeIdleTicks[bit];
            ulong kernelDelta = kernel[bit] - _lastNodeKernelTicks[bit];
            ulong userDelta = user[bit] - _lastNodeUserTicks[bit];
            ulong coreTotal = kernelDelta + userDelta;
            if (coreTotal == 0) continue;

            double busy = Math.Clamp(1.0 - (double)idleDelta / coreTotal, 0, 1);
            busySum += busy;
            counted++;
        }

        _lastNodeIdleTicks = idle;
        _lastNodeKernelTicks = kernel;
        _lastNodeUserTicks = user;

        if (counted == 0)
            return _lastNumaNodeCpuStatus;

        double usage = busySum / counted;

        _lastNumaNodeCpuStatus = usage > NumaCpuUsageHighThreshold
            ? StatusType.Warning
            : StatusType.Success;
        return _lastNumaNodeCpuStatus;
    }

    private static bool TryReadGroupProcessorCounters(ushort group, out ulong[] idle, out ulong[] kernel, out ulong[] user)
    {
        idle = [];
        kernel = [];
        user = [];
        if (!OperatingSystem.IsWindows()) return false;

        try
        {
            uint processorCount = LibImportProvider.GetActiveProcessorCount(group);
            if (processorCount == 0) return false;

            int structSize = Marshal.SizeOf<PROCESSOR_POWER_INFORMATION>();
            uint byteLength = processorCount * (uint)structSize;

            IntPtr buffer = Marshal.AllocHGlobal((int)byteLength);
            try
            {
                if (!LibImportProvider.TryGetSystemProcessorPerformanceInformation(group, buffer, byteLength, out _))
                    return false;

                idle = new ulong[processorCount];
                kernel = new ulong[processorCount];
                user = new ulong[processorCount];
                for (int i = 0; i < processorCount; i++)
                {
                    PROCESSOR_POWER_INFORMATION info =
                        Marshal.PtrToStructure<PROCESSOR_POWER_INFORMATION>(buffer + i * structSize);
                    idle[i] = info.IdleTime;
                    kernel[i] = info.KernelTime;
                    user[i] = info.UserTime;
                }
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException)
        {
            return false;
        }
    }

    #endregion

    #region Battery check with caching
    private static DateTime _lastBatteryCheck = DateTime.MinValue;
    private static bool _lastBatteryResult;
    private static readonly TimeSpan BatteryCacheDuration = TimeSpan.FromSeconds(5);

    public static bool IsOnAcPower()
    {
        if ((DateTime.UtcNow - _lastBatteryCheck) < BatteryCacheDuration)
            return _lastBatteryResult;

        bool result = CheckBatteryStatus();

        _lastBatteryCheck = DateTime.UtcNow;
        _lastBatteryResult = result;
        return result;
    }

    private static bool CheckBatteryStatus()
    {
        if (!OperatingSystem.IsWindows()) return true;

        if (!LibImportProvider.TryGetSystemPowerStatus(out byte acLineStatus, out byte batteryFlag))
            return true;

        if (acLineStatus == AC_LINE_ONLINE)
            return true;

        if (acLineStatus == AC_LINE_UNKNOWN)
            return batteryFlag == BATTERY_FLAG_NO_BATTERY;

        return false;
    }
    #endregion

    /// <summary>
    /// Returns the size of the source video file in bytes. Returns -1 if unavailable.
    /// </summary>
    public static long GetSourceVideoFileSize(string? srcPath)
    {
        if (string.IsNullOrWhiteSpace(srcPath) || !File.Exists(srcPath))
            return -1;

        try { return new FileInfo(srcPath).Length; }
        catch { return -1; }
    }

    /// <summary>
    /// Returns the available free space on the drive of the given path in bytes.
    /// Returns -1 if the drive cannot be determined.
    /// </summary>
    public static long GetAvailableDiskSpaceBytes(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return -1;

        try
        {
            string? root = Path.GetPathRoot(path);
            if (string.IsNullOrWhiteSpace(root)) return -1;

            DriveInfo drive = new(root);
            return drive.IsReady ? drive.AvailableFreeSpace : -1;
        }
        catch { return -1; }
    }

    public static bool HasWritePermission(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return false;

        string testFile = Path.Combine(directory, $".1cenc_write_test_{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(testFile, string.Empty);
            File.Delete(testFile);
            return true;
        }
        catch { return false; }
    }

    #region Overwrite
    public static bool OutputFileExists(string? outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath)) return false;
        return File.Exists(outputPath);
    }
    #endregion

    #region Lsmash plugin detection
    /// <summary>
    /// Returns true if LSMASHSource.dll can be found in known AviSynth+ plugin locations.
    /// </summary>
    public static bool HasLsmashPlugin(string? avisynthDllPath)
    {
        if (string.IsNullOrWhiteSpace(avisynthDllPath)) return false;

        try
        {
            string installDir = Directory.Exists(avisynthDllPath)
                ? avisynthDllPath
                : Path.GetDirectoryName(avisynthDllPath) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(installDir)) return false;

            if (CheckLsmashInDir(installDir)
                || CheckLsmashInDir(Path.Combine(installDir, "plugins"))
                || CheckLsmashInDir(Path.Combine(installDir, "plugins64"))
                || CheckLsmashInDir(Path.Combine(installDir, "plugins64+"))
                || CheckLsmashInDir(Path.Combine(installDir, "..", "plugins"))
                || CheckLsmashInDir(Path.Combine(installDir, "..", "plugins64"))
                || CheckLsmashInDir(Path.Combine(installDir, "..", "plugins64+")))
                return true;

            string? programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            return CheckLsmashInDir(Path.Combine(programFiles, "AviSynth+", "plugins"))
                || CheckLsmashInDir(Path.Combine(programFiles, "AviSynth+", "plugins64"))
                || CheckLsmashInDir(Path.Combine(programFiles, "AviSynth+", "plugins64+"))
                || CheckLsmashInDir(Path.Combine(programFilesX86, "AviSynth+", "plugins"))
                || CheckLsmashInDir(Path.Combine(programFilesX86, "AviSynth+", "plugins64"))
                || CheckLsmashInDir(Path.Combine(programFilesX86, "AviSynth+", "plugins64+"));
        }
        catch
        {
            return false;
        }
    }

    private static bool CheckLsmashInDir(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return false;

        return File.Exists(Path.Combine(directory, "LSMASHSource.dll"));
    }
    #endregion
}
