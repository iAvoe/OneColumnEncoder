using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Applies pixel-format rules used by ffprobe-based validation and filtering
/// </summary>
public static class FFProbePixelFormatRules
{
    public static bool IsYuvRgbOrGray(string? pixelFormat)
    {
        if (string.IsNullOrWhiteSpace(pixelFormat)) return false;

        return pixelFormat.Contains("yuv", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("rgb", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gbr", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gray", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasSupportedChroma(JsonElement stream)
    {
        string? pixelFormat = TryGetString(stream, "pix_fmt");
        if (string.IsNullOrWhiteSpace(pixelFormat)) return false;

        if (pixelFormat.Contains("444", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("rgb", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gbr", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gray", StringComparison.OrdinalIgnoreCase))
            return true;

        string? chromaLocation = TryGetString(stream, "chroma_location");
        return pixelFormat.Contains("yuv", StringComparison.OrdinalIgnoreCase)
            && (chromaLocation?.Equals("left", StringComparison.OrdinalIgnoreCase) == true
                || chromaLocation?.Equals("topleft", StringComparison.OrdinalIgnoreCase) == true);
    }

    public static int GetBitDepth(JsonElement stream)
    {
        if (TryGetInt(stream, "bits_per_raw_sample", out int rawBits)) return rawBits;
        if (TryGetInt(stream, "bits_per_sample", out int sampleBits)) return sampleBits;

        string pixFmt = TryGetString(stream, "pix_fmt") ?? string.Empty;
        if (pixFmt.Contains("10", StringComparison.OrdinalIgnoreCase)) return 10;
        if (pixFmt.Contains("12", StringComparison.OrdinalIgnoreCase)) return 12;
        if (pixFmt.Contains("14", StringComparison.OrdinalIgnoreCase)) return 14;
        if (pixFmt.Contains("16", StringComparison.OrdinalIgnoreCase)) return 16;
        return string.IsNullOrWhiteSpace(pixFmt) ? 0 : 8;
    }

    public static int GetChromaSubsamplingDepth(string? pixelFormat)
    {
        if (string.IsNullOrWhiteSpace(pixelFormat)) return -2;
        if (pixelFormat.Contains("444", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("rgb", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gbr", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gray", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("400", StringComparison.OrdinalIgnoreCase))
            return 0;
        if (pixelFormat.Contains("420", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("422", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv12", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv16", StringComparison.OrdinalIgnoreCase))
            return 1;
        return -2;
    }
}
