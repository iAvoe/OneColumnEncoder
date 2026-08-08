using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.FFmpeg;

public readonly record struct FFProbeSourceStats(
    double DurationSeconds,
    double FrameRate,
    long TotalFrames,
    string FieldOrderKind,
    string FrameRateKind);

public static class FFProbeSourceStatsReader
{
    private const double FallbackDuration = 600d;
    private const double FallbackFrameRate = 30d;

    public static FFProbeSourceStats Read(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            return CreateFallback();

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            JsonElement root = document.RootElement;
            if (!FrameRate.TryGetFirstVideoStream(root, out JsonElement stream))
                return CreateFallback();

            double duration = TryGetDouble(stream, "duration")
                ?? (root.TryGetProperty("format", out JsonElement format) ? TryGetDouble(format, "duration") : null)
                ?? FallbackDuration;

            double frameRate = ParseFrameRate(TryGetString(stream, "avg_frame_rate"))
                ?? ParseFrameRate(TryGetString(stream, "r_frame_rate"))
                ?? FallbackFrameRate;

            long totalFrames = TryGetFrameCount(stream)
                ?? Math.Max(0L, (long)Math.Round(duration * frameRate));

            return new FFProbeSourceStats(
                duration,
                frameRate,
                totalFrames,
                GetFieldOrderKind(stream),
                GetFrameRateKind(stream));
        }
        catch
        {
            return CreateFallback();
        }
    }

    private static FFProbeSourceStats CreateFallback() =>
        new(FallbackDuration, FallbackFrameRate, (long)(FallbackDuration * FallbackFrameRate), "unknown", "unknown");

    private static string GetFieldOrderKind(JsonElement stream)
    {
        string? fieldOrder = TryGetString(stream, "field_order");
        if (string.IsNullOrWhiteSpace(fieldOrder) || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            return "unknown";
        return fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
            ? "progressive"
            : "interlaced";
    }

    private static string GetFrameRateKind(JsonElement stream)
    {
        string? avg = TryGetString(stream, "avg_frame_rate");
        string? r = TryGetString(stream, "r_frame_rate");
        return !string.IsNullOrWhiteSpace(avg) && !avg.Equals("0/0", StringComparison.OrdinalIgnoreCase)
            ? string.Equals(avg, r, StringComparison.OrdinalIgnoreCase) ? "constant" : "variable"
            : "unknown";
    }

    private static double? ParseFrameRate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Equals("0/0", StringComparison.OrdinalIgnoreCase))
            return null;

        string[] parts = text.Split('/');
        if (parts.Length == 2
            && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double n)
            && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double d)
            && d > 0)
            return n / d;

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : null;
    }
}
