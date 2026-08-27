using System.IO;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Runs ffprobe for a video source and returns normalized analysis JSON
/// </summary>
public static class FFProbeVideoAnalysis
{
    private static FFProbeVideoAnalysisLangProvider Lang =>
        new(UILangProvider.Current.LanguageCode);

    /// <summary>
    /// Analyze source by ffprobe video, returns normalized JSON containing the requested entries
    /// </summary>
    /// <param name="ffprobePath">Path to ffprobe</param>
    /// <param name="videoSource">Path to video source</param>
    /// <param name="showEntries">
    /// The ffprobe entries to include in the output. Defaults to <c>"stream:frame:stream_side_data:frame_side_data"</c>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>
    /// A Task that represents the async operation,
    /// Task result contains the normalized JSON output produced by ffprobe
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Either <paramref name="ffprobePath"/> or <paramref name="videoSource"/> is null/empty/moved
    /// </exception>
    /// <exception cref="TimeoutException">FFprobe timed out, somehow</exception>
    /// <exception cref="InvalidOperationException">
    /// FFprobe fails or returns invalid JSON. Usually because of bad source video (especially BluRay m2ts')
    /// </exception>
    public static async Task<string> AnalyzeAsync(
        string ffprobePath,
        string videoSource,
        string showEntries = "stream:frame:stream_side_data:frame_side_data",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            throw new FileNotFoundException(string.Format(Lang.FfprobeNotFound, ffprobePath));
        if (string.IsNullOrWhiteSpace(videoSource) || !File.Exists(videoSource))
            throw new FileNotFoundException(string.Format(Lang.InputVideoNotFound, videoSource));

        string[] arguments =
        [
            "-v", "quiet", "-hide_banner", "-select_streams", "v:0",
            "-show_entries", showEntries, "-show_frames", "-read_intervals", "%+#1",
            "-show_format", "-of", "json", videoSource
        ];

        FFprobeProcessResult result;
        try
        {
            result = await FFprobeProcessRunner.RunAsync(
                ffprobePath,
                arguments,
                TimeSpan.FromSeconds(30),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested) throw;
            throw new TimeoutException(Lang.FfprobeTimedOut);
        }

        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Stdout))
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Stderr)
                ? Lang.FfprobeFailedOrEmpty
                : result.Stderr.Trim());

        ValidateJson(result.Stdout);

        string mergedJson = await TryMergeAllStreamsAsync(ffprobePath, videoSource, result.Stdout, cancellationToken);
        return FFProbeJsonFormatting.Normalize(mergedJson);
    }

    // Check if source video metadata corrupted, or ffprobe was mad (unlikely)
    private static void ValidateJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("streams", out JsonElement streams)
            || streams.ValueKind != JsonValueKind.Array
            || streams.GetArrayLength() < 1)
            throw new InvalidOperationException(Lang.NoVideoStreamInfo);
    }

    private static async Task<string> TryMergeAllStreamsAsync(
        string ffprobePath,
        string videoSource,
        string primaryJson,
        CancellationToken cancellationToken)
    {
        try
        {
            FFprobeProcessResult streamsResult = await FFprobeProcessRunner.RunAsync(
                ffprobePath,
                ["-v", "quiet", "-hide_banner", "-show_streams", "-show_format", "-of", "json", videoSource],
                TimeSpan.FromSeconds(30),
                cancellationToken);

            if (streamsResult.ExitCode != 0 || string.IsNullOrWhiteSpace(streamsResult.Stdout))
                return primaryJson;

            using JsonDocument primaryDoc = JsonDocument.Parse(primaryJson);
            using JsonDocument streamsDoc = JsonDocument.Parse(streamsResult.Stdout);
            if (!streamsDoc.RootElement.TryGetProperty("streams", out JsonElement allStreams) || allStreams.ValueKind != JsonValueKind.Array)
                return primaryJson;

            using var output = new MemoryStream();
            using (Utf8JsonWriter writer = new(output, new JsonWriterOptions { Indented = false }))
            {
                writer.WriteStartObject();
                foreach (JsonProperty property in primaryDoc.RootElement.EnumerateObject())
                {
                    if (property.NameEquals("streams"))
                        continue;

                    property.WriteTo(writer);
                }

                writer.WritePropertyName("streams");
                allStreams.WriteTo(writer);
                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(output.ToArray());
        }
        catch
        {
            return primaryJson;
        }
    }
}
