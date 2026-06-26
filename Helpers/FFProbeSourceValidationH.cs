using System.Text.Json;

namespace OneColumnEncoder.Helpers;

public readonly record struct FFProbeSourceValidationResult(
    bool IsProgressive,
    bool IsSvtAv1BitDepthSupported,
    bool IsMaxBitDepthSupported,
    bool HasConstantFrameRate,
    bool HasSquarePixels,
    bool HasColorSpace,
    bool HasColorTransfer,
    bool HasColorPrimaries,
    bool HasSupportedChroma);

public static class FFProbeSourceValidationH
{
    public static FFProbeSourceValidationResult Analyze(string rawJson)
    {
        using JsonDocument document = JsonDocument.Parse(rawJson);
        if (!FrameRateH.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
            throw new InvalidOperationException("ffprobe returned no video stream information.");

        return new FFProbeSourceValidationResult(
            IsProgressive(stream),
            IsSupportedBitDepth(stream, 10),
            IsSupportedBitDepth(stream, 12),
            HasConstantFrameRate(stream),
            HasSquarePixels(stream),
            HasKnownMetadata(stream, "color_space"),
            HasKnownMetadata(stream, "color_transfer"),
            HasKnownMetadata(stream, "color_primaries"),
            HasSupportedChroma(stream));
    }

    public static bool IsSvtAv1BitDepthSupported(string rawJson) => Analyze(rawJson).IsSvtAv1BitDepthSupported;

    private static bool IsProgressive(JsonElement stream)
    {
        string? fieldOrder = JsonElementHelper.TryGetString(stream, "field_order");
        return string.IsNullOrWhiteSpace(fieldOrder)
            || fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
            || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSupportedBitDepth(JsonElement stream, int max)
    {
        int bitDepth = GetBitDepth(stream);
        return bitDepth == 8 || bitDepth == 10 || bitDepth == max;
    }

    private static int GetBitDepth(JsonElement stream)
    {
        if (JsonElementHelper.TryGetInt(stream, "bits_per_raw_sample", out int rawBits)) return rawBits;
        if (JsonElementHelper.TryGetInt(stream, "bits_per_sample", out int sampleBits)) return sampleBits;

        string pixFmt = JsonElementHelper.TryGetString(stream, "pix_fmt") ?? string.Empty;
        if (pixFmt.Contains("10", StringComparison.OrdinalIgnoreCase)) return 10;
        if (pixFmt.Contains("12", StringComparison.OrdinalIgnoreCase)) return 12;
        if (pixFmt.Contains("14", StringComparison.OrdinalIgnoreCase)) return 14;
        if (pixFmt.Contains("16", StringComparison.OrdinalIgnoreCase)) return 16;
        return string.IsNullOrWhiteSpace(pixFmt) ? 0 : 8;
    }

    private static bool HasConstantFrameRate(JsonElement stream)
    {
        string? avg = JsonElementHelper.TryGetString(stream, "avg_frame_rate");
        string? r = JsonElementHelper.TryGetString(stream, "r_frame_rate");
        return !string.IsNullOrWhiteSpace(avg)
            && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
            && string.Equals(avg, r, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasSquarePixels(JsonElement stream)
    {
        string? sar = JsonElementHelper.TryGetString(stream, "sample_aspect_ratio");
        return string.Equals(sar, "1:1", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasKnownMetadata(JsonElement stream, string propertyName)
    {
        string? value = JsonElementHelper.TryGetString(stream, propertyName);
        return !string.IsNullOrWhiteSpace(value)
            && !value.Equals("unknown", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("unspecified", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("reserved", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasSupportedChroma(JsonElement stream)
    {
        string pixFmt = JsonElementHelper.TryGetString(stream, "pix_fmt") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(pixFmt)) return false;
        if (pixFmt.Contains("444", StringComparison.OrdinalIgnoreCase)
            || pixFmt.Contains("rgb", StringComparison.OrdinalIgnoreCase)
            || pixFmt.Contains("gbr", StringComparison.OrdinalIgnoreCase)
            || pixFmt.Contains("gray", StringComparison.OrdinalIgnoreCase))
            return true;

        string? chromaLocation = JsonElementHelper.TryGetString(stream, "chroma_location");
        return pixFmt.Contains("yuv", StringComparison.OrdinalIgnoreCase)
            && (chromaLocation?.Equals("left", StringComparison.OrdinalIgnoreCase) == true
                || chromaLocation?.Equals("topleft", StringComparison.OrdinalIgnoreCase) == true);
    }
}
