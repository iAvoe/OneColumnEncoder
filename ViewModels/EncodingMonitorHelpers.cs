namespace OneColumnEncoder.ViewModels;

/// <summary>
/// Encoding selection, size formatting and metric template replacement for EncodingMonitorVM
/// </summary>
public static partial class EncodingMonitorHelpers
{
    private static readonly Encoding SystemTextEncoding = GetSystemTextEncoding();

    public static Encoding GetSystemTextEncoding()
    {
        try { return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage); }
        catch { try { return Console.OutputEncoding; } catch { return Encoding.UTF8; } }
    }

    public static Encoding GetEncodingForProcess(string exeName)
    {
        return exeName.ToLowerInvariant() switch
        {
            "avs2yuv.exe" or "avs2pipemod.exe" => SystemTextEncoding,
            _ => Encoding.UTF8
        };
    }

    public static string FormatSize(long bytes, string unit = "GB", bool invariantCulture = false, bool includeUnit = true)
    {
        double value = Math.Max(0, bytes);
        string format = "0.0";

        switch (unit.ToUpperInvariant())
        {
            case "GB":
                value /= 1024L * 1024L * 1024L;
                break;
            case "MB":
                value /= 1024L * 1024L;
                format = "N0"; // N0 means whole numbers
                break;
            default: throw new ArgumentException($"!Size unit: {unit}");
        }

        string formattedValue = invariantCulture
            ? value.ToString(format, CultureInfo.InvariantCulture)
            : value.ToString(format);

        return includeUnit ? $"{formattedValue} {unit}" : formattedValue;
    }

    public static string ReplaceMetricValue(string template, string value) =>
        RegexProviderM.FileSizeMetricRegex().Replace(template, value);
}
