using System.Runtime.InteropServices;

namespace OneColumnEncoder.Hardware;

public static partial class OpenCLDetector
{
    private const int CL_SUCCESS = 0;
    private static readonly Lazy<bool> _isAvailable = new(CheckOpenCLAvailability);

    public static bool IsOpenCLAvailable() => _isAvailable.Value;

    private static bool CheckOpenCLAvailability()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        if (!NativeLibrary.TryLoad("OpenCL.dll", out IntPtr library))
            return false;

        try
        {
            int result = clGetPlatformIDs(0, IntPtr.Zero, out uint platformCount);
            return result == CL_SUCCESS && platformCount > 0;
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

    [LibraryImport("OpenCL", EntryPoint = "clGetPlatformIDs")]
    private static partial int clGetPlatformIDs(
        uint numEntries,
        IntPtr platforms,
        out uint numPlatforms);
}
