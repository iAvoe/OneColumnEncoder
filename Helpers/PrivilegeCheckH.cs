using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers;

public static partial class PrivilegeCheckH
{
    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenProcessToken(
        IntPtr ProcessHandle,
        uint DesiredAccess,
        out IntPtr TokenHandle);

    [LibraryImport("advapi32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "LookupPrivilegeValueW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LookupPrivilegeValueW(
        string? lpSystemName,
        string lpName,
        out long lpLuid);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetTokenInformation(
        IntPtr TokenHandle,
        uint TokenInformationClass,
        IntPtr TokenInformation,
        uint TokenInformationLength,
        out uint ReturnLength);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetCurrentProcess();

    private const uint TOKEN_QUERY = 0x0008;
    private const uint TokenPrivileges = 3;
    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;

    public static bool HasLockMemoryPrivilege()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        IntPtr hProcess = GetCurrentProcess();
        if (!OpenProcessToken(hProcess, TOKEN_QUERY, out IntPtr hToken))
            return false;

        try
        {
            if (!LookupPrivilegeValueW(null, "SeLockMemoryPrivilege", out long luid))
                return false;

            GetTokenInformation(hToken, TokenPrivileges, IntPtr.Zero, 0, out uint returnLength);
            if (returnLength == 0)
                return false;

            IntPtr buffer = Marshal.AllocHGlobal((int)returnLength);
            try
            {
                if (!GetTokenInformation(hToken, TokenPrivileges, buffer, returnLength, out _))
                    return false;

                int privilegeCount = Marshal.ReadInt32(buffer);
                int offset = 4;

                for (int i = 0; i < privilegeCount; i++)
                {
                    long currentLuid = Marshal.ReadInt64(buffer, offset);
                    uint attributes = (uint)Marshal.ReadInt32(buffer, offset + 8);

                    if (currentLuid == luid)
                        return (attributes & SE_PRIVILEGE_ENABLED) != 0;

                    offset += 12;
                }

                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            CloseHandle(hToken);
        }
    }
}
