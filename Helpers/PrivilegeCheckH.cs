using System.Runtime.InteropServices;

namespace OneColumnEncoder.Helpers;
/// <summary>
/// Currently just try to detect if memory locking priviledge is granted.
/// Windows natively uses 12 bytes for `LUID_AND_ATTRIBUTES`.
/// In C#, the default `StructLayout.Sequential` might be aligned to 8 bytes on x64, causing `Marshal.SizeOf<LUID_AND_ATTRIBUTES>()` to become 16.
/// This resulted in an incorrect step size when enumerating the token privilege array, leading to a misreading of `SeLockMemoryPrivilege`.
/// </summary>
public static partial class PrivilegeCheckH
{
    public static string LastLockMemoryPrivilegeCheckMessage { get; private set; } = string.Empty;

    private enum TOKEN_INFORMATION_CLASS
    {
        TokenPrivileges = 3
    }

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenProcessToken(
        IntPtr ProcessHandle,
        uint DesiredAccess,
        out IntPtr TokenHandle);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetTokenInformation(
        IntPtr TokenHandle,
        TOKEN_INFORMATION_CLASS TokenInformationClass,
        IntPtr TokenInformation,
        uint TokenInformationLength,
        out uint ReturnLength);

    [LibraryImport("advapi32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16, EntryPoint = "LookupPrivilegeValueW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LookupPrivilegeValueW(
        string? lpSystemName,
        string lpName,
        out long lpLuid);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AdjustTokenPrivileges(
        IntPtr TokenHandle,
        [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
        ref TOKEN_PRIVILEGES NewState,
        uint BufferLength,
        IntPtr PreviousState,
        IntPtr ReturnLength);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetCurrentProcess();

    // Also requires TOKEN_ADJUST_PRIVILEGES (0x0020) & TOKEN_QUERY (0x0008)
    private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    private const uint TOKEN_QUERY = 0x0008;
    private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int ERROR_NOT_ALL_ASSIGNED = 1300;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct TOKEN_PRIVILEGES
    {
        public uint PrivilegeCount;
        public long Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct LUID_AND_ATTRIBUTES
    {
        public long Luid;
        public uint Attributes;
    }

    public static bool HasLockMemoryPrivilege()
    {
        LastLockMemoryPrivilegeCheckMessage = string.Empty;

        if (!OperatingSystem.IsWindows())
        {
            LastLockMemoryPrivilegeCheckMessage = "Not running on Windows.";
            return false;
        }

        IntPtr hProcess = GetCurrentProcess();
        // Must require TOKEN_ADJUST_PRIVILEGES
        if (!OpenProcessToken(hProcess, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr hToken))
        {
            LastLockMemoryPrivilegeCheckMessage = $"OpenProcessToken failed. LastWin32Error={Marshal.GetLastWin32Error()}.";
            return false;
        }

        try
        {
            if (!LookupPrivilegeValueW(null, "SeLockMemoryPrivilege", out long luid))
            {
                LastLockMemoryPrivilegeCheckMessage = $"LookupPrivilegeValueW(SeLockMemoryPrivilege) failed. LastWin32Error={Marshal.GetLastWin32Error()}.";
                return false;
            }

            if (!TryGetTokenPrivilege(hToken, luid, out uint attributes, out string tokenPrivilegeError))
            {
                LastLockMemoryPrivilegeCheckMessage = tokenPrivilegeError;
                return false;
            }

            if ((attributes & SE_PRIVILEGE_ENABLED) != 0)
            {
                LastLockMemoryPrivilegeCheckMessage = "SeLockMemoryPrivilege is present and already enabled.";
                return true;
            }

            TOKEN_PRIVILEGES tp = new()
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = SE_PRIVILEGE_ENABLED // Try to enable directly
            };

            // See if we have priviledge
            bool result =
                AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);

            int adjustError = Marshal.GetLastWin32Error();
            if (!result)
            {
                LastLockMemoryPrivilegeCheckMessage = $"AdjustTokenPrivileges failed. LastWin32Error={adjustError}. TokenAttributes=0x{attributes:X8}.";
                return false;
            }

            if (adjustError == ERROR_NOT_ALL_ASSIGNED)
            {
                LastLockMemoryPrivilegeCheckMessage = $"AdjustTokenPrivileges returned ERROR_NOT_ALL_ASSIGNED. TokenAttributes=0x{attributes:X8}.";
                return false;
            }

            LastLockMemoryPrivilegeCheckMessage = $"SeLockMemoryPrivilege is present and was enabled. TokenAttributes=0x{attributes:X8}, AdjustLastWin32Error={adjustError}.";
            return true;
        }
        catch (Exception ex)
        {
            LastLockMemoryPrivilegeCheckMessage = $"Exception while checking SeLockMemoryPrivilege: {ex.GetType().Name}: {ex.Message}";
            return false;
        }
        finally { CloseHandle(hToken); }
    }

    private static bool TryGetTokenPrivilege(IntPtr hToken, long privilegeLuid, out uint attributes, out string errorMessage)
    {
        attributes = 0;
        errorMessage = string.Empty;

        GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrivileges, IntPtr.Zero, 0, out uint requiredLength);
        int lengthError = Marshal.GetLastWin32Error();
        if (requiredLength == 0 || lengthError != ERROR_INSUFFICIENT_BUFFER)
        {
            errorMessage = $"GetTokenInformation length query failed. RequiredLength={requiredLength}, LastWin32Error={lengthError}.";
            return false;
        }

        IntPtr tokenInfo = Marshal.AllocHGlobal((int)requiredLength);
        try
        {
            if (!GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrivileges, tokenInfo, requiredLength, out _))
            {
                errorMessage = $"GetTokenInformation(TokenPrivileges) failed. LastWin32Error={Marshal.GetLastWin32Error()}, RequiredLength={requiredLength}.";
                return false;
            }

            uint privilegeCount = (uint)Marshal.ReadInt32(tokenInfo);
            int privilegeOffset = sizeof(uint);
            int privilegeSize = Marshal.SizeOf<LUID_AND_ATTRIBUTES>();

            for (uint i = 0; i < privilegeCount; i++)
            {
                IntPtr privilegePtr = IntPtr.Add(tokenInfo, privilegeOffset + (int)i * privilegeSize);
                LUID_AND_ATTRIBUTES privilege = Marshal.PtrToStructure<LUID_AND_ATTRIBUTES>(privilegePtr);
                if (privilege.Luid == privilegeLuid)
                {
                    attributes = privilege.Attributes;
                    return true;
                }
            }

            errorMessage = $"Token does not contain SeLockMemoryPrivilege. TokenPrivilegeCount={privilegeCount}.";
            return false;
        }
        finally
        {
            Marshal.FreeHGlobal(tokenInfo);
        }
    }
}
