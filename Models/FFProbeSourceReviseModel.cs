using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.Models;

public static class FFProbeSourceReviseModel
{
    public static string UpdateSingleSourceJson(string rawJson, SourceRevisionRequest request) =>
        ApplyRevision(rawJson, request);

    public static (string rawJson, string queueRawJson) UpdateQueueSourceJson(
        string queueFilePath,
        string rawJson,
        string queueRawJson,
        SourceRevisionRequest request)
    {
        JsonNode queueRoot = ParseJsonObject(File.ReadAllText(queueFilePath));
        int updated = UpdateFfprobeJsonEntries(queueRoot, request);
        if (updated == 0)
            throw new InvalidOperationException("No ffprobe JSON entries found in queue file.");

        string newRawJson = ExtractReferenceFfprobeJson(queueRoot)
            ?? ApplyRevision(rawJson, request);
        string newQueueRawJson = string.IsNullOrWhiteSpace(queueRawJson)
            ? queueRawJson
            : UpdateQueueRawJsonString(queueRawJson, request);

        File.WriteAllText(
            queueFilePath,
            queueRoot.ToJsonString(FFProbeJsonFormatting.Options),
            new UTF8Encoding(false));
        return (newRawJson, newQueueRawJson);
    }

    public static (string rawJson, string queueRawJson) UpdateConcatSourceJson(
        string rawJson,
        string queueRawJson,
        SourceRevisionRequest request)
    {
        string newRawJson = ApplyRevision(rawJson, request);
        string newQueueRawJson = string.IsNullOrWhiteSpace(queueRawJson)
            ? queueRawJson
            : UpdateQueueRawJsonString(queueRawJson, request);
        return (newRawJson, newQueueRawJson);
    }

    public static long CalculateTotalFrames(string queueRawJson)
    {
        if (string.IsNullOrWhiteSpace(queueRawJson)) return 0;

        using JsonDocument document = JsonDocument.Parse(queueRawJson);
        if (!document.RootElement.TryGetProperty("Entries", out JsonElement entries)
            || entries.ValueKind != JsonValueKind.Array)
            return 0;

        long total = 0;
        int count = 0;
        foreach (JsonElement entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty("FfprobeJson", out JsonElement ffprobe)
                || !FrameRate.TryGetFirstVideoStream(ffprobe, out JsonElement stream))
                continue;

            long? frames = TryGetFrameCount(stream);
            if (frames is not > 0) return -1;
            total = checked(total + frames.Value);
            count++;
        }

        return count > 0 ? total : -1;
    }

    private static string ApplyRevision(string rawJson, SourceRevisionRequest request)
        => FFProbeJsonUpdateResolver.UpdateResolution(
            rawJson,
            request.Width,
            request.Height);

    private static string UpdateQueueRawJsonString(string queueRawJson, SourceRevisionRequest request)
    {
        JsonNode root = ParseJsonObject(queueRawJson);
        int updated = UpdateFfprobeJsonEntries(root, request);
        if (updated == 0)
            throw new InvalidOperationException("No ffprobe JSON entries found in QueueRawJson.");
        return root.ToJsonString(FFProbeJsonFormatting.Options);
    }

    private static int UpdateFfprobeJsonEntries(JsonNode root, SourceRevisionRequest request)
    {
        if (root["Entries"] is not JsonArray entries)
            throw new InvalidOperationException("JSON root is missing 'Entries' array.");

        int count = 0;
        foreach (JsonNode? child in entries)
        {
            if (child is not JsonObject entry || entry["FfprobeJson"] is not JsonNode ffprobeNode)
                continue;

            string revised = ApplyRevision(ffprobeNode.ToJsonString(), request);
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

    private static JsonNode ParseJsonObject(string json) =>
        JsonNode.Parse(json) is JsonObject obj
            ? obj
            : throw new InvalidOperationException("JSON root is not an object.");
}
