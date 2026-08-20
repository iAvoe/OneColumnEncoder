using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.FFmpeg;

/// <summary>Represents the ffprobe validation flags for a source video.</summary>
/// <param name="IsProgressive">Source is progressive or unknown.</param>
/// <param name="IsSvtAv1BitDepthSupported">Source supports SVT-AV1 bit depth.</param>
/// <param name="IsMaxBitDepthSupported">Source supports the max target bit depth.</param>
/// <param name="HasConstantFrameRate">Source uses constant frame rate.</param>
/// <param name="HasSquarePixels">Source uses square pixels.</param>
/// <param name="HasColorSpace">Source has known color space metadata.</param>
/// <param name="HasColorTransfer">Source has known transfer metadata.</param>
/// <param name="HasColorPrimaries">Source has known primaries metadata.</param>
/// <param name="HasSupportedChroma">Source chroma format is supported.</param>
/// <param name="IsYuv420">Source pixel format is YUV420.</param>
public readonly record struct FFProbeSrcValResult(
    bool IsProgressive,
    bool IsSvtAv1BitDepthSupported,
    bool IsMaxBitDepthSupported,
    bool HasConstantFrameRate,
    bool HasSquarePixels,
    bool HasColorSpace,
    bool HasColorTransfer,
    bool HasColorPrimaries,
    bool HasSupportedChroma,
    bool IsYuv420);

/// <summary>
/// Evaluates ffprobe metadata for codec, chroma, and frame-rate support checks
/// </summary>
/// <remarks>
/// The biggest problem of the methods are non-standard writing, hopefully ffprobe does not change them...
/// </remarks>
public static class FFProbeSrcVal
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
        catch { return 0; }
    }

    public static FFProbeSrcValResult Analyze(string rawJson)
    {
        using JsonDocument document = JsonDocument.Parse(rawJson);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
            return default;

        return new FFProbeSrcValResult(
            IsProgressive(stream),
            IsSupportedBitDepth(stream, 10),
            IsSupportedBitDepth(stream, 12),
            HasConstantFrameRate(stream),
            HasSquarePixels(stream),
            HasKnownMetadata(stream, "color_space"),
            HasKnownMetadata(stream, "color_transfer"),
            HasKnownMetadata(stream, "color_primaries"),
            FFProbePixelFormatRules.HasSupportedChroma(stream),
            IsYuv420(stream));
    }

    public static bool IsSvtAv1BitDepthSupported(string rawJson) => Analyze(rawJson).IsSvtAv1BitDepthSupported;

    private static bool IsYuv420(JsonElement stream)
    {
        string? pixelFormat = TryGetString(stream, "pix_fmt");
        return !string.IsNullOrWhiteSpace(pixelFormat)
            && pixelFormat.Contains("420", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProgressive(JsonElement stream)
    {
        string? fieldOrder = TryGetString(stream, "field_order");
        // Maybe just match letter p in the JSON field is enough... or not
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
