using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OneColumnEncoder.FFmpeg;

namespace OneColumnEncoder.Models;

public static class FFProbeResolutionReviseModel
{
    public static string UpdateSingleSourceJson(string rawJson, int width, int height)
    {
        return FFProbeJsonUpdateResolver.UpdateResolution(rawJson, width, height);
    }

    public static (string rawJson, string queueRawJson) UpdateQueueSourceJson(
        string queueFilePath, string rawJson, string queueRawJson, int width, int height)
    {
        string queueFileContent = File.ReadAllText(queueFilePath);
        JsonNode queueRoot = ParseJsonObject(queueFileContent);

        int updated = UpdateFfprobeJsonEntries(queueRoot, width, height);
        if (updated == 0)
            throw new InvalidOperationException("No ffprobe JSON entries found in queue file.");

        string newRawJson = ExtractReferenceFfprobeJson(queueRoot)
            ?? FFProbeJsonUpdateResolver.UpdateResolution(rawJson, width, height);

        string newQueueRawJson = string.IsNullOrWhiteSpace(queueRawJson)
            ? queueRawJson
            : UpdateQueueRawJsonString(queueRawJson, width, height);

        File.WriteAllText(queueFilePath, queueRoot.ToJsonString(FFProbeJsonFormatting.Options), new UTF8Encoding(false));

        return (newRawJson, newQueueRawJson);
    }

    public static (string rawJson, string queueRawJson) UpdateConcatSourceJson(
        string rawJson, string queueRawJson, int width, int height)
    {
        string newRawJson = FFProbeJsonUpdateResolver.UpdateResolution(rawJson, width, height);

        string newQueueRawJson = string.IsNullOrWhiteSpace(queueRawJson)
            ? queueRawJson
            : UpdateQueueRawJsonString(queueRawJson, width, height);

        return (newRawJson, newQueueRawJson);
    }

    private static string UpdateQueueRawJsonString(string queueRawJson, int width, int height)
    {
        JsonNode root = ParseJsonObject(queueRawJson);
        int updated = UpdateFfprobeJsonEntries(root, width, height);
        if (updated == 0)
            throw new InvalidOperationException("No ffprobe JSON entries found in QueueRawJson.");
        return root.ToJsonString(FFProbeJsonFormatting.Options);
    }

    private static int UpdateFfprobeJsonEntries(JsonNode root, int width, int height)
    {
        if (root["Entries"] is not JsonArray entries)
            throw new InvalidOperationException("JSON root is missing 'Entries' array.");

        int count = 0;
        foreach (JsonNode? child in entries)
        {
            if (child is not JsonObject entry || entry["FfprobeJson"] is not JsonNode ffprobeNode)
                continue;

            string raw = ffprobeNode.ToJsonString();
            string revised = FFProbeJsonUpdateResolver.UpdateResolution(raw, width, height);
            entry["FfprobeJson"] = JsonNode.Parse(revised)
                ?? throw new InvalidOperationException("Revised ffprobe JSON resolved to null.");
            count++;
        }

        return count;
    }

    private static string? ExtractReferenceFfprobeJson(JsonNode root)
    {
        if (root is not JsonObject obj || obj["Entries"] is not JsonArray entries)
            return null;

        string? referencePath = GetString(obj["ReferenceFilePath"]);
        JsonNode? fallback = null;

        foreach (JsonNode? child in entries)
        {
            if (child is not JsonObject entry || entry["FfprobeJson"] is not JsonNode ffprobe)
                continue;

            fallback ??= ffprobe;
            string? filePath = GetString(entry["FilePath"]);

            if (!string.IsNullOrWhiteSpace(referencePath)
                && string.Equals(referencePath, filePath, StringComparison.OrdinalIgnoreCase))
                return ffprobe.ToJsonString(FFProbeJsonFormatting.Options);
        }

        return fallback?.ToJsonString(FFProbeJsonFormatting.Options);
    }

    private static JsonNode ParseJsonObject(string json)
    {
        return JsonNode.Parse(json) is JsonObject obj
            ? obj
            : throw new InvalidOperationException("JSON root is not an object.");
    }

    private static string? GetString(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out string? s) ? s : null;
}
