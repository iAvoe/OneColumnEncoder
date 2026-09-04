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

        return LibImportProviderM.TryGetOpenClPlatformCount(out uint platformCount) && platformCount > 0;
    }
}
