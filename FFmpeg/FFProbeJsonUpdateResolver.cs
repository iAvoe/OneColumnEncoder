using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using OneColumnEncoder.Json;
using OneColumnEncoder.Models;

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

        FFProbeAspectRatio aspectRatio = FFProbeAspectRatioResolver.Resolve(frameStream);
        updatedJson = UpdateSampleAspectRatio(updatedJson, aspectRatio, newWidth, newHeight);

        return updatedJson;
    }

    public static string UpdateResolution(string originalJson, int newWidth, int newHeight)
    {
        if (string.IsNullOrWhiteSpace(originalJson))
            throw new ArgumentException(UILangProviderM.Current["FFProbeJsonUpdate.JsonEmpty"], nameof(originalJson));
        if (newWidth <= 0 || newHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(newWidth), UILangProviderM.Current["FFProbeJsonUpdate.DimensionsNotPositive"]);

        JsonNode? rootNode = JsonNode.Parse(originalJson);
        if (rootNode is not JsonObject rootObject)
            throw new InvalidOperationException(UILangProviderM.Current["FFProbeJsonUpdate.RootNotObject"]);

        JsonObject firstStream = GetFirstVideoStream(rootObject);

        bool changed = false;
        changed |= SetIntProperty(firstStream, "width", newWidth, addIfMissing: true);
        changed |= SetIntProperty(firstStream, "height", newHeight, addIfMissing: true);
        changed |= SetIntProperty(firstStream, "coded_width", newWidth, addIfMissing: false);
        changed |= SetIntProperty(firstStream, "coded_height", newHeight, addIfMissing: false);
        changed |= UpdateDARForSquarePixels(firstStream, newWidth, newHeight);

        return changed
            ? rootNode.ToJsonString(FFProbeJsonFormatting.Options)
            : originalJson;
    }

    private static string UpdateSampleAspectRatio(string originalJson, FFProbeAspectRatio aspectRatio, int newWidth, int newHeight)
    {
        JsonNode? rootNode = JsonNode.Parse(originalJson);
        if (rootNode is not JsonObject rootObject) return originalJson;

        JsonObject firstStream = GetFirstVideoStream(rootObject);
        string sarText = aspectRatio.Sar.ToString();
        bool changed = SetStringProperty(firstStream, "sample_aspect_ratio", sarText, addIfMissing: false);
        changed |= SetStringProperty(firstStream, "sar", sarText, addIfMissing: false);
        if (aspectRatio.HasSquarePixels)
            changed |= SetStringProperty(firstStream, "display_aspect_ratio", $"{newWidth}:{newHeight}", addIfMissing: true);

        return changed
            ? rootNode.ToJsonString(FFProbeJsonFormatting.Options)
            : originalJson;
    }

    private static JsonObject GetFirstVideoStream(JsonObject rootObject)
    {
        if (rootObject["streams"] is not JsonArray streamNodes || streamNodes.Count < 1)
            throw new InvalidOperationException(UILangProviderM.Current["FFProbeJsonUpdate.NoVideoStream"]);

        foreach (JsonNode? node in streamNodes)
        {
            if (node is not JsonObject stream) continue;
            string? codecType = JsonElementHelper.GetString(stream["codec_type"]);
            if (codecType == null || codecType.Equals("video", StringComparison.OrdinalIgnoreCase))
                return stream;
        }

        throw new InvalidOperationException(UILangProviderM.Current["FFProbeJsonUpdate.NoVideoStream"]);
    }

    private static bool SetIntProperty(JsonObject target, string propertyName, int value, bool addIfMissing)
    {
        if (!target.TryGetPropertyValue(propertyName, out JsonNode? oldNode) || oldNode == null)
        {
            if (!addIfMissing) return false;
            target[propertyName] = value;
            return true;
        }

        int? oldValue = JsonElementHelper.GetInt(oldNode);
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

        string? oldValue = JsonElementHelper.GetString(oldNode);
        if (string.Equals(oldValue, value, StringComparison.Ordinal)) return false;

        target[propertyName] = value;
        return true;
    }

    private static bool UpdateDARForSquarePixels(JsonObject stream, int width, int height)
    {
        return FFProbeAspectRatioResolver.HasSquarePixels(stream)
            && SetStringProperty(stream, "display_aspect_ratio", $"{width}:{height}", addIfMissing: true);
    }
}
