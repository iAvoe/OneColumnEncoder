using static OneColumnEncoder.Json.JsonElementHelper;
using System.Text.Json;

namespace OneColumnEncoder.FFmpeg;

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

public static class FFProbeSourceValidation
{
    public static int ReadBitDepthFromJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return 0;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return 0;

            return FFProbePixelFormatRules.GetBitDepth(stream);
        }
        catch
        {
            return 0;
        }
    }

    public static FFProbeSourceValidationResult Analyze(string rawJson)
    {
        using JsonDocument document = JsonDocument.Parse(rawJson);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
            return default;

        return new FFProbeSourceValidationResult(
            IsProgressive(stream),
            IsSupportedBitDepth(stream, 10),
            IsSupportedBitDepth(stream, 12),
            HasConstantFrameRate(stream),
            HasSquarePixels(stream),
            HasKnownMetadata(stream, "color_space"),
            HasKnownMetadata(stream, "color_transfer"),
            HasKnownMetadata(stream, "color_primaries"),
            FFProbePixelFormatRules.HasSupportedChroma(stream));
    }

    public static bool IsSvtAv1BitDepthSupported(string rawJson) => Analyze(rawJson).IsSvtAv1BitDepthSupported;

    private static bool IsProgressive(JsonElement stream)
    {
        string? fieldOrder = TryGetString(stream, "field_order");
        return string.IsNullOrWhiteSpace(fieldOrder)
            || fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
            || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSupportedBitDepth(JsonElement stream, int max)
    {
        int bitDepth = FFProbePixelFormatRules.GetBitDepth(stream);
        return bitDepth == 8 || bitDepth == 10 || bitDepth == max;
    }

    private static bool HasConstantFrameRate(JsonElement stream)
    {
        string? avg = TryGetString(stream, "avg_frame_rate");
        string? r = TryGetString(stream, "r_frame_rate");
        return !string.IsNullOrWhiteSpace(avg)
            && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
            && string.Equals(avg, r, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasSquarePixels(JsonElement stream)
    {
        return FFProbeAspectRatioResolver.HasSquarePixels(stream);
    }

    private static bool HasKnownMetadata(JsonElement stream, string propertyName)
    {
        string? value = TryGetString(stream, propertyName);
        return !string.IsNullOrWhiteSpace(value)
            && !value.Equals("unknown", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("unspecified", StringComparison.OrdinalIgnoreCase)
            && !value.Equals("reserved", StringComparison.OrdinalIgnoreCase);
    }

}
