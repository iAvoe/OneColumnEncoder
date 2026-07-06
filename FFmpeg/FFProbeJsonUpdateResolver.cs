using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;

namespace OneColumnEncoder.FFmpeg;

internal static class FFProbeJsonUpdateResolver
{
    public static string? UpdateResolution(string? originalJson, string? frameFfprobeJson)
    {
        if (string.IsNullOrWhiteSpace(originalJson)) return originalJson;
        if (string.IsNullOrWhiteSpace(frameFfprobeJson)) return originalJson;

        using JsonDocument frameDoc = JsonDocument.Parse(frameFfprobeJson);
        if (!FrameRate.TryGetFirstVideoStream(frameDoc.RootElement, out JsonElement frameStream))
            return originalJson;

        if (!frameStream.TryGetProperty("width", out JsonElement wEl) || !frameStream.TryGetProperty("height", out JsonElement hEl))
            return originalJson;

        if (!wEl.TryGetInt32(out int newWidth) || !hEl.TryGetInt32(out int newHeight))
            return originalJson;

        string updatedJson = UpdateResolution(originalJson, newWidth, newHeight);

        if (frameStream.TryGetProperty("sample_aspect_ratio", out JsonElement sarEl))
        {
            string? sar = sarEl.GetString();
            if (!string.IsNullOrWhiteSpace(sar))
                updatedJson = UpdateSampleAspectRatio(updatedJson, sar, newWidth, newHeight);
        }

        return updatedJson;
    }

    public static string UpdateResolution(string originalJson, int newWidth, int newHeight)
    {
        if (string.IsNullOrWhiteSpace(originalJson))
            throw new ArgumentException("ffprobe JSON is empty.", nameof(originalJson));
        if (newWidth <= 0 || newHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(newWidth), "Resolution dimensions must be positive.");

        JsonNode? rootNode = JsonNode.Parse(originalJson);
        if (rootNode is not JsonObject rootObject)
            throw new InvalidOperationException("ffprobe JSON root is not an object.");

        JsonObject firstStream = GetFirstVideoStream(rootObject);

        bool changed = false;

        changed |= SetIntProperty(firstStream, "width", newWidth, addIfMissing: true);
        changed |= SetIntProperty(firstStream, "height", newHeight, addIfMissing: true);
        changed |= SetIntProperty(firstStream, "coded_width", newWidth, addIfMissing: false);
        changed |= SetIntProperty(firstStream, "coded_height", newHeight, addIfMissing: false);
        changed |= UpdateDisplayAspectRatioForSquarePixels(firstStream, newWidth, newHeight);

        return changed
            ? rootNode.ToJsonString(FFProbeJsonFormatting.Options)
            : originalJson;
    }

    private static string UpdateSampleAspectRatio(string originalJson, string sar, int newWidth, int newHeight)
    {
        JsonNode? rootNode = JsonNode.Parse(originalJson);
        if (rootNode is not JsonObject rootObject) return originalJson;

        JsonObject firstStream = GetFirstVideoStream(rootObject);
        bool changed = SetStringProperty(firstStream, "sample_aspect_ratio", sar, addIfMissing: false);
        if (sar == "1:1")
            changed |= SetStringProperty(firstStream, "display_aspect_ratio", $"{newWidth}:{newHeight}", addIfMissing: true);

        return changed
            ? rootNode.ToJsonString(FFProbeJsonFormatting.Options)
            : originalJson;
    }

    private static JsonObject GetFirstVideoStream(JsonObject rootObject)
    {
        if (rootObject["streams"] is not JsonArray streamNodes || streamNodes.Count < 1)
            throw new InvalidOperationException("No video stream found in ffprobe JSON.");

        foreach (JsonNode? node in streamNodes)
        {
            if (node is not JsonObject stream) continue;
            string? codecType = GetString(stream["codec_type"]);
            if (codecType == null || codecType.Equals("video", StringComparison.OrdinalIgnoreCase))
                return stream;
        }

        throw new InvalidOperationException("No video stream found in ffprobe JSON.");
    }

    private static bool SetIntProperty(JsonObject target, string propertyName, int value, bool addIfMissing)
    {
        if (!target.TryGetPropertyValue(propertyName, out JsonNode? oldNode) || oldNode == null)
        {
            if (!addIfMissing) return false;
            target[propertyName] = value;
            return true;
        }

        int? oldValue = GetInt(oldNode);
        if (oldValue == value) return false;

        target[propertyName] = value;
        return true;
    }

    private static bool SetStringProperty(JsonObject target, string propertyName, string value, bool addIfMissing)
    {
        if (!target.TryGetPropertyValue(propertyName, out JsonNode? oldNode) || oldNode == null)
        {
            if (!addIfMissing) return false;
            target[propertyName] = value;
            return true;
        }

        string? oldValue = GetString(oldNode);
        if (string.Equals(oldValue, value, StringComparison.Ordinal)) return false;

        target[propertyName] = value;
        return true;
    }

    private static bool UpdateDisplayAspectRatioForSquarePixels(JsonObject stream, int width, int height)
    {
        string? sar = GetString(stream["sample_aspect_ratio"]);
        return sar == "1:1"
            && SetStringProperty(stream, "display_aspect_ratio", $"{width}:{height}", addIfMissing: true);
    }

    private static int? GetInt(JsonNode? node)
    {
        if (node is not JsonValue value) return null;
        if (value.TryGetValue<int>(out int intValue)) return intValue;
        if (value.TryGetValue<long>(out long longValue)
            && longValue >= int.MinValue
            && longValue <= int.MaxValue)
            return (int)longValue;
        if (value.TryGetValue<string>(out string? stringValue)
            && int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            return parsed;
        return null;
    }

    private static string? GetString(JsonNode? node)
    {
        if (node is not JsonValue value) return null;
        return value.TryGetValue<string>(out string? stringValue) ? stringValue : null;
    }
}
