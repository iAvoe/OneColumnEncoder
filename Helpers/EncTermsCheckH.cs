using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers;

public static partial class EncTermsCheckH
{
    #region Win32 P/Invoke
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

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

    private const byte AC_LINE_ONLINE = 1;
    private const byte AC_LINE_UNKNOWN = 255;
    private const byte BATTERY_FLAG_NO_BATTERY = 128;
    private const byte BATTERY_FLAG_CHARGING = 8;
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

        if (!GetSystemPowerStatus(out SYSTEM_POWER_STATUS status))
            return true;

        if (status.ACLineStatus == AC_LINE_ONLINE)
            return true;

        if (status.ACLineStatus == AC_LINE_UNKNOWN)
            return status.BatteryFlag == BATTERY_FLAG_NO_BATTERY;

        return false;
    }
    #endregion

    /// <summary>
    /// Returns the size of the source video file in bytes. Returns -1 if unavailable.
    /// </summary>
    public static long GetSourceVideoFileSize(string? sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            return -1;

        try { return new FileInfo(sourcePath).Length; }
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
    /// Returns true if libvslsmashsource.dll can be found in known AviSynth+ plugin locations.
    /// </summary>
    public static bool HasLsmashPlugin(string? avisynthDllPath)
    {
        if (string.IsNullOrWhiteSpace(avisynthDllPath)) return false;

        try
        {
            string? installDir = Path.GetDirectoryName(avisynthDllPath);
            if (string.IsNullOrWhiteSpace(installDir)) return false;

            if (CheckLsmashInDir(installDir)
                || CheckLsmashInDir(Path.Combine(installDir, "plugins"))
                || CheckLsmashInDir(Path.Combine(installDir, "plugins64"))
                || CheckLsmashInDir(Path.Combine(installDir, "..", "plugins"))
                || CheckLsmashInDir(Path.Combine(installDir, "..", "plugins64")))
                return true;

            string? programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            return CheckLsmashInDir(Path.Combine(programFiles, "AviSynth+", "plugins"))
                || CheckLsmashInDir(Path.Combine(programFiles, "AviSynth+", "plugins64"))
                || CheckLsmashInDir(Path.Combine(programFilesX86, "AviSynth+", "plugins"))
                || CheckLsmashInDir(Path.Combine(programFilesX86, "AviSynth+", "plugins64"));
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

        return File.Exists(Path.Combine(directory, "libvslsmashsource.dll"));
    }
    #endregion
}
