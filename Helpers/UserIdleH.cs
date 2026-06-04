using System;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers;

public static class UserIdleH
{
    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint CbSize;
        public uint DwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo lastInputInfo);

    public static TimeSpan GetIdleTime()
    {
        LastInputInfo info = new()
        {
            CbSize = (uint)Marshal.SizeOf<LastInputInfo>()
        };

        if (!GetLastInputInfo(ref info)) return TimeSpan.Zero;

        uint tickCount = unchecked((uint)Environment.TickCount);
        uint elapsedMs = tickCount - info.DwTime;
        return TimeSpan.FromMilliseconds(elapsedMs);
    }
}
