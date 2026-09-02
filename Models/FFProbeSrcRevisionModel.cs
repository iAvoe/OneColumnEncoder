using OneColumnEncoder.Models.Analysis;
using OneColumnEncoder.Models.Lang;
using System.IO;
using System.Text.Json.Nodes;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.Models;

/// <summary>
/// Updates stored ffprobe JSON after a source revision.
/// </summary>
public static class FFProbeSrcRevisionModel
{
    public static string UpdateSingleSourceJson(string rawJson, SrcRevisionRequest request) =>
        ApplyRevision(rawJson, request);

    public static (string rawJson, string batchRawJson) UpdateQueueSourceJson(
        string queueFilePath,
        string rawJson,
        string batchRawJson,
        SrcRevisionRequest request)
    {
        JsonNode queueRoot = ParseJsonObject(File.ReadAllText(queueFilePath));
        int updated = UpdateFfprobeJsonEntries(queueRoot, request);
        if (updated == 0)
            throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.NoEntriesInQueue);

        string newRawJson = ExtractReferenceFfprobeJson(queueRoot)
            ?? ApplyRevision(rawJson, request);
        string newBatchRawJson = string.IsNullOrWhiteSpace(batchRawJson)
            ? batchRawJson
            : UpdateBatchRawJsonString(batchRawJson, request);

        File.WriteAllText(
            queueFilePath,
            queueRoot.ToJsonString(FFProbeJsonFormatting.Options),
            new UTF8Encoding(false));
        return (newRawJson, newBatchRawJson);
    }

    public static (string rawJson, string batchRawJson) UpdateConcatSourceJson(
        string rawJson,
        string batchRawJson,
        SrcRevisionRequest request)
    {
        string newRawJson = ApplyRevision(rawJson, request);
        string newBatchRawJson = string.IsNullOrWhiteSpace(batchRawJson)
            ? batchRawJson
            : UpdateBatchRawJsonString(batchRawJson, request);
        return (newRawJson, newBatchRawJson);
    }

    public static long CalculateTotalFrames(string batchRawJson)
    {
        if (string.IsNullOrWhiteSpace(batchRawJson)) return 0;

        RawAnalysisBatchM data = JsonSerializer.Deserialize<RawAnalysisBatchM>(batchRawJson)
            ?? throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.FailedToParseBatch);
        if (data.Entries.Count == 0) return -1;

        long total = 0;
        int count = 0;
        foreach (SourceRawAnalysisM entry in data.Entries)
        {
            if (!FrameRate.TryGetFirstVideoStream(entry.FfprobeJson, out JsonElement stream))
                continue;

            long? frames = TryGetFrameCount(stream);
            if (frames is not > 0) return -1;
            total = checked(total + frames.Value);
            count++;
        }

        return count > 0 ? total : -1;
    }

    private static string ApplyRevision(string rawJson, SrcRevisionRequest request)
        => FFProbeJsonUpdateResolver.UpdateResolution(
            rawJson,
            request.Width,
            request.Height);

    private static string UpdateBatchRawJsonString(string batchRawJson, SrcRevisionRequest request)
    {
        RawAnalysisBatchM data = JsonSerializer.Deserialize<RawAnalysisBatchM>(batchRawJson)
            ?? throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.FailedToParseBatch);

        List<SourceRawAnalysisM> entries = [.. data.Entries
            .Select(entry => entry with
            {
                FfprobeJson = JsonDocument.Parse(
                    ApplyRevision(entry.FfprobeJson.GetRawText(), request)).RootElement.Clone()
            })];
        if (entries.Count == 0)
            throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.NoEntriesInBatch);
        return JsonSerializer.Serialize(new RawAnalysisBatchM(entries), FFProbeJsonFormatting.Options);
    }

    private static int UpdateFfprobeJsonEntries(JsonNode root, SrcRevisionRequest request)
    {
        if (root["Entries"] is not JsonArray entries)
            throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.MissingEntriesArray);

        int count = 0;
        foreach (JsonNode? child in entries)
        {
            if (child is not JsonObject entry || entry["FfprobeJson"] is not JsonNode ffprobeNode)
                continue;

            string revised = ApplyRevision(ffprobeNode.ToJsonString(), request);
            entry["FfprobeJson"] = JsonNode.Parse(revised)
                ?? throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.RevisedJsonNull);
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

    private static JsonObject ParseJsonObject(string json) =>
        JsonNode.Parse(json) is JsonObject obj
            ? obj
            : throw new InvalidOperationException(FFProbeSrcRevisionLangProvider.Current.JsonRootNotObject);
}
