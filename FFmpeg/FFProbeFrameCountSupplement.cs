using System.Globalization;
using OneColumnEncoder.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OneColumnEncoder.FFmpeg;

internal static class FFProbeFrameCountSupplement
{
    public static FFProbeFrameCountSupplementResult Supplement(string rawJson)
    {
        using JsonDocument document = JsonDocument.Parse(rawJson);
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("streams", out JsonElement streams)
            || streams.ValueKind != JsonValueKind.Array
            || streams.GetArrayLength() < 1)
            return new(rawJson, 0, false);

        int supplementedCount = 0;
        JsonObject? rootNode = null;
        JsonArray? streamNodes = null;

        for (int i = 0; i < streams.GetArrayLength(); i++)
        {
            JsonElement stream = streams[i];
            if (HasUsableFrameCount(stream) || !TryEstimateFrameCount(root, stream, out long frameCount)) continue;

            rootNode ??= JsonNode.Parse(rawJson)?.AsObject();
            streamNodes ??= rootNode?["streams"]?.AsArray();
            JsonObject? streamNode = streamNodes?[i]?.AsObject();
            if (streamNode == null) continue;

            streamNode["nb_frames"] = frameCount.ToString(CultureInfo.InvariantCulture);
            streamNode["nb_frames_by_1cenc"] = true;
            supplementedCount++;
        }

        return supplementedCount > 0 && rootNode != null
            ? new(rootNode.ToJsonString(FFProbeJsonFormatting.Options), supplementedCount, true)
            : new(rawJson, 0, false);
    }

    private static bool HasUsableFrameCount(JsonElement stream) =>
        JsonElementHelper.TryGetFrameCount(stream) is > 0;

    private static bool TryEstimateFrameCount(JsonElement root, JsonElement stream, out long frameCount)
    {
        frameCount = 0;
        double? duration = JsonElementHelper.TryGetDouble(stream, "duration")
            ?? (root.TryGetProperty("format", out JsonElement format) ? JsonElementHelper.TryGetDouble(format, "duration") : null);
        string? avgFrameRate = JsonElementHelper.TryGetString(stream, "avg_frame_rate");

        if (duration is not > 0
            || !FrameRate.TryParseFrameRate(avgFrameRate, out double fps)
            || fps <= 0d)
            return false;

        double frameCountValue = duration.Value * fps;
        if (frameCountValue <= 0d || double.IsNaN(frameCountValue) || double.IsInfinity(frameCountValue)) return false;

        frameCount = (long)Math.Floor(frameCountValue + 0.5d);
        return frameCount > 0;
    }

}

internal sealed record FFProbeFrameCountSupplementResult(string RawJson, int SupplementedCount, bool IsNbFramesCalculated);
