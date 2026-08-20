using System.IO;
using System.Text.RegularExpressions;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.Pipeline;

internal sealed record MuxContext(
    string EncodedVideoPath,
    string OutputPath,
    string FramerateValue,
    long VideoTimescale);

public static partial class MuxPipeline
{
    #region Public API

    /// <summary>
    /// Builds the mux command that merges encoded video with source streams into the final container
    /// </summary>
    /// <remarks>
    /// Expecting format consistency of input sources,
    /// which should be handled by validators before MuxPipeline runs
    /// </remarks>
    /// <param name="request">Encoding request carrying source metadata and output path.</param>
    /// <returns>Mux command for source-stream preservation, or null when muxing is disabled.</returns>
    public static EncodingMuxCommand? BuildMuxCommand(EncodingPipelineRequest request)
    {
        if (request == null
            || request.MuxMode == EncodingMuxMode.Disabled
            || string.IsNullOrWhiteSpace(request.FfmpegPath)) return null;
        if (request.MuxMode == EncodingMuxMode.VideoOnly)
            return BuildRepartMuxCommand(request) ?? BuildVideoOnlyMuxCommand(request);
        if (request.Clip != null) return null;
        if (!request.IsConcatMode.GetValueOrDefault()
            && string.IsNullOrWhiteSpace(request.SourceVideoPath)) return null;

        MuxContext context = BuildMuxContext(request);
        EncodingAudioMuxMode audioMode = EncodingAudioMuxResolver.ResolveAudioMuxMode(request);
        string videoTimescaleArgs = $"-video_track_timescale {context.VideoTimescale}";
        string streamMapArgs = BuildStreamMapArgs(request.SourceFfprobeJson);
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, context.FramerateValue);

        bool isConcatMux =
            request.IsConcatMode.GetValueOrDefault()
            && request.ConcatFileListPath != null;
        bool useConcatSourceInputs = isConcatMux
            && audioMode != EncodingAudioMuxMode.Copy
            && request.ConcatVideoSourcePaths is { Length: > 0 };
        bool splitAudioStep = useConcatSourceInputs
            && EncodingAudioMuxResolver.IsReEncodeMode(audioMode)
            && GetAudioStreamCount(request.SourceFfprobeJson) > 0;
        if (splitAudioStep)
            return BuildConcatAudioSplitMux(request, context, audioMode, videoTimescaleArgs, inputFormatArgs);

        string secondInput = useConcatSourceInputs
            ? string.Join(" ", request.ConcatVideoSourcePaths!.Select(path => $"-i {Quote(path)}"))
            : isConcatMux
                ? $"-f concat -safe 0 -i {Quote(request.ConcatFileListPath!)}"
                : $"-i {Quote(request.SourceVideoPath!)}";

        // Concat mux can either re-encode audio from each fragment input or fall back to the concat demuxer
        // when the user selected Copy.
        string? audioMuxArgs = BuildAudioMuxArgs(audioMode);
        string nonVideoMapAndCodecArgs = useConcatSourceInputs
            ? BuildConcatAudioMuxMapArgs(audioMode)
            : $"{streamMapArgs} -map_metadata 1 -map_chapters 1 {(audioMode == EncodingAudioMuxMode.Disable ? "-an" : audioMuxArgs)} -c:s copy";

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            secondInput,
            $"-map 0:v:0 {nonVideoMapAndCodecArgs} -c:v copy -bsf:v setts=pts=N*DURATION {videoTimescaleArgs}",
            Quote(context.OutputPath));

        return new($"{Quote(request.FfmpegPath)} {args}", args, context.EncodedVideoPath, context.OutputPath);
    }

    /// <summary>
    /// Builds the mux command that merges the encoded video with audio cut from the repart source
    /// timeline (concat list) over the clip's time range. Returns null when audio is disabled or
    /// the clip range is unavailable, so callers can fall back to a video-only mux.
    /// </summary>
    /// <param name="request">Repart encoding request with a clip and concat file list.</param>
    /// <returns>Mux command with re-encoded/copied audio, or null when audio muxing is unavailable.</returns>
    public static EncodingMuxCommand? BuildRepartMuxCommand(EncodingPipelineRequest request)
    {
        EncodingAudioMuxMode audioMode = EncodingAudioMuxResolver.ResolveAudioMuxMode(request);
        if (audioMode == EncodingAudioMuxMode.Disable
            || string.IsNullOrWhiteSpace(request.ConcatFileListPath)
            || string.IsNullOrWhiteSpace(request.FfmpegPath)) return null;
        (string startTime, string endTime)? range = GetRepartClipTimeRange(request);
        if (range == null) return null;

        MuxContext context = BuildMuxContext(request);
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, context.FramerateValue);
        string audioMapArgs = BuildConcatAudioMuxMapArgs(audioMode);
        if (audioMapArgs == "-an") return null;

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            $"-f concat -safe 0 -ss {range.Value.startTime} -to {range.Value.endTime} -i {Quote(request.ConcatFileListPath!)}",
            $"-map 0:v:0 {audioMapArgs} -c:v copy -bsf:v setts=pts=N*DURATION -video_track_timescale {context.VideoTimescale}",
            Quote(context.OutputPath));

        return new($"{Quote(request.FfmpegPath)} {args}", args, context.EncodedVideoPath, context.OutputPath);
    }

    /// <summary>
    /// Builds the stream-map fragment used by full muxing when ffprobe metadata is available.
    /// </summary>
    /// <param name="sourceFfprobeJson">ffprobe JSON for the source container.</param>
    /// <returns>Stream mapping args for ffmpeg.</returns>
    private static string BuildStreamMapArgs(string? sourceFfprobeJson)
    {
        // Prefer explicit stream mapping from ffprobe; otherwise fall back to audio and subtitle streams.
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson))
            return "-map 1:a? -map 1:s?";

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!document.RootElement.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
                return "-map 1:a? -map 1:s?";

            var nonVideoStreams = new List<string>();
            foreach (JsonElement stream in streams.EnumerateArray())
            {
                string? codecType = TryGetString(stream, "codec_type");
                if (string.IsNullOrWhiteSpace(codecType)) continue;
                if (codecType.Equals("video", StringComparison.OrdinalIgnoreCase)) continue;
                if (codecType.Equals("attachment", StringComparison.OrdinalIgnoreCase)) continue;
                if (codecType.Equals("data", StringComparison.OrdinalIgnoreCase)) continue;

                if (!TryGetInt(stream, "index", out int streamIndex)) continue;
                nonVideoStreams.Add($"-map 1:{streamIndex}");
            }

            if (nonVideoStreams.Count > 0)
                return string.Join(" ", nonVideoStreams);

            return "-map 1:a? -map 1:s?";
        }
        catch { return "-map 1:a? -map 1:s?"; }
    }

    /// <summary>
    /// Builds the mux command that keeps only the encoded video stream.
    /// </summary>
    /// <param name="request">Encoding request with source, clip, and mux settings.</param>
    /// <returns>Video-only mux command, or null when muxing is unavailable.</returns>
    public static EncodingMuxCommand? BuildVideoOnlyMuxCommand(EncodingPipelineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FfmpegPath)) return null;

        MuxContext context = BuildMuxContext(request);
        string videoTimescaleArgs = $"-video_track_timescale {context.VideoTimescale}";
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, context.FramerateValue);

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            $"-map 0:v:0 -c:v copy -bsf:v setts=pts=N*DURATION {videoTimescaleArgs}",
            Quote(context.OutputPath));

        return new($"{Quote(request.FfmpegPath)} {args}", args, context.EncodedVideoPath, context.OutputPath);
    }

    public static string ResolveOutputPathWithExtension(string encoderExeName, string outputPath)
    {
        string ext = encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => ".mp4",
            "x265.exe" => ".hevc",
            "svtav1encapp.exe" => ".ivf",
            _ => string.Empty
        };
        return EnsureExtension(outputPath, ext);
    }

    #endregion

    #region Mux Helpers

    /// <summary>
    /// Builds the audio codec argument fragment for a resolved audio mux mode.
    /// Returns null for Disable so callers can emit the bare -an instead.
    /// </summary>
    private static string? BuildAudioMuxArgs(EncodingAudioMuxMode mode) => mode switch
    {
        EncodingAudioMuxMode.Copy => "-c:a copy",
        EncodingAudioMuxMode.ReEncodeAAC320 => "-c:a aac -b:a 320k",
        EncodingAudioMuxMode.ReEncodeAAC256 => "-c:a aac -b:a 256k",
        EncodingAudioMuxMode.ReEncodeAAC128 => "-c:a aac -b:a 128k",
        EncodingAudioMuxMode.ReEncodeOpus320 => "-c:a libopus -b:a 320k -vbr on -compression_level 10 -frame_duration 100",
        EncodingAudioMuxMode.ReEncodeOpus256 => "-c:a libopus -b:a 256k -vbr on -compression_level 10 -frame_duration 100",
        EncodingAudioMuxMode.ReEncodeOpus128 => "-c:a libopus -b:a 128k -vbr on -compression_level 10 -frame_duration 100",
        _ => null
    };

    private static string BuildConcatAudioMuxMapArgs(EncodingAudioMuxMode mode)
    {
        string? audioMuxArgs = BuildAudioMuxArgs(mode);
        if (audioMuxArgs == null) return "-an";

        string audioMapArgs = mode == EncodingAudioMuxMode.Copy
            ? "-map 1:a:0?"
            : "-map 1:a?";
        return $"{audioMapArgs} {audioMuxArgs}";
}

    private static string? BuildConcatAudioFilterArgs(
        string[] sourcePaths,
        string? sourceFfprobeJson)
    {
        int audioStreamCount = GetAudioStreamCount(sourceFfprobeJson);
        if (audioStreamCount <= 0) return null;

        List<string> filterChains = [];
        List<string> audioMaps = [];

        for (int audioIndex = 0; audioIndex < audioStreamCount; audioIndex++)
        {
            string stagePrefix = $"a{audioIndex}";
            string resetInputs = string.Join(";", Enumerable.Range(0, sourcePaths.Length)
                .Select(sourceIndex => $"[{sourceIndex + 1}:a:{audioIndex}]asetpts=PTS-STARTPTS[{stagePrefix}_{sourceIndex}]"));
            string concatInputs = string.Concat(Enumerable.Range(0, sourcePaths.Length)
                .Select(sourceIndex => $"[{stagePrefix}_{sourceIndex}]"));
            filterChains.Add($"{resetInputs};{concatInputs}concat=n={sourcePaths.Length}:v=0:a=1[{stagePrefix}]");
            audioMaps.Add($"-map \"[{stagePrefix}]\"");
        }

        string filterComplex = string.Join(";", filterChains);
        return $"-filter_complex \"{filterComplex}\" {string.Join(" ", audioMaps)}";
    }

    /// <summary>
    /// Builds a two-stage concat mux when the selected audio mode re-encodes: first a standalone
    /// audio encode that concatenates source audio into a temp file, then a mux that copies the
    /// encoded video and the temp audio into the final container.
    /// </summary>
    private static EncodingMuxCommand BuildConcatAudioSplitMux(
        EncodingPipelineRequest request,
        MuxContext context,
        EncodingAudioMuxMode audioMode,
        string videoTimescaleArgs,
        string? inputFormatArgs)
    {
        string[] sourcePaths = request.ConcatVideoSourcePaths!;
        string audioCodecArgs = BuildAudioMuxArgs(audioMode)!;
        int audioStreamCount = GetAudioStreamCount(request.SourceFfprobeJson);
        string audioOutputPath = ResolveTempAudioPath(context.OutputPath, audioMode);

        // Step 1: audio encode — concat source audio streams into a temp file.
        string audioFilterAndMaps = BuildConcatAudioFilterArgs(sourcePaths, request.SourceFfprobeJson)!;
        string audioArgs = JoinArgs(
            "-hide_banner -y",
            string.Join(" ", sourcePaths.Select(path => $"-i {Quote(path)}")),
            audioFilterAndMaps,
            audioCodecArgs,
            Quote(audioOutputPath));
        string audioCommandLine = $"{Quote(request.FfmpegPath!)} {audioArgs}";
        EncodingAudioCommand audioCommand = new(audioCommandLine, audioArgs, audioOutputPath);

        // Step 2: mux — copy encoded video plus the temp audio into the final container.
        string audioMapArgs = string.Join(" ", Enumerable.Range(0, audioStreamCount).Select(i => $"-map 1:a:{i}"));
        string muxArgs = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            $"-i {Quote(audioOutputPath)}",
            $"-map 0:v:0 {audioMapArgs} -c copy -bsf:v setts=pts=N*DURATION {videoTimescaleArgs}",
            Quote(context.OutputPath));
        string muxCommandLine = $"{Quote(request.FfmpegPath!)} {muxArgs}";

        return new(muxCommandLine, muxArgs, context.EncodedVideoPath, context.OutputPath, audioCommand);
    }

    private static string ResolveTempAudioPath(string outputPath, EncodingAudioMuxMode mode)
    {
        string directory = Path.GetDirectoryName(outputPath) ?? string.Empty;
        string baseName = Path.GetFileNameWithoutExtension(outputPath);
        string extension = mode is EncodingAudioMuxMode.ReEncodeOpus320
            or EncodingAudioMuxMode.ReEncodeOpus256
            or EncodingAudioMuxMode.ReEncodeOpus128
            ? ".ogg"
            : ".m4a";
        return Path.Combine(directory, $"{baseName}_audio_temp{extension}");
    }

    /// <summary>
    /// Resolves the normalized start/end timestamps for a repart clip, preferring explicit times
    /// and falling back to frame-based conversion when the plan provides frames and frame rate.
    /// </summary>
    private static (string StartTime, string EndTime)? GetRepartClipTimeRange(EncodingPipelineRequest request)
    {
        EncodingClipRequest? clip = request.Clip;
        if (clip == null) return null;

        string? start = NormalizeTimestamp(clip.StartTime);
        string? end = NormalizeTimestamp(clip.EndTime);

        if (start == null || end == null)
        {
            if (!(clip.FrameRateNumerator is > 0)
                || !(clip.FrameRateDenominator is > 0)
                || clip.FirstFrame == null
                || clip.LastFrame == null)
                return null;
            double frameRate = (double)clip.FrameRateNumerator.Value / clip.FrameRateDenominator.Value;
            start ??= EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(clip.FirstFrame.Value / frameRate));
            end ??= EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds((clip.LastFrame.Value + 1) / frameRate));
        }

        return (start, end);
    }

    private static string? NormalizeTimestamp(string? timestamp) =>
        string.IsNullOrWhiteSpace(timestamp)
            ? null
            : EncodingPipeline.FormatTimestamp(EncodingPipeline.ParseTimestamp(timestamp));

    private static int GetAudioStreamCount(string? sourceFfprobeJson)
    {
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return 0;

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!document.RootElement.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
                return 0;

            int count = 0;
            foreach (JsonElement stream in streams.EnumerateArray())
            {
                string? codecType = TryGetString(stream, "codec_type");
                if (!string.IsNullOrWhiteSpace(codecType) && codecType.Equals("audio", StringComparison.OrdinalIgnoreCase))
                    count++;
            }

            return count;
        }
        catch { return 0; }
    }

    public static string ResolveMuxOutputPath(string outputPath) =>
        RemoveRawVideoExtension(outputPath) + ".mkv";

    private static MuxContext BuildMuxContext(EncodingPipelineRequest request)
    {
        string framerateValue = request.Clip?.FrameRateNumerator is > 0 && request.Clip.FrameRateDenominator is > 0
            ? $"{request.Clip.FrameRateNumerator.Value}/{request.Clip.FrameRateDenominator.Value}"
            : GetMuxFramerateValue(request.SourceFfprobeJson, request.FfmpegFilterArgs);

        return new(
            ResolveOutputPathWithExtension(request.EncoderExeName, request.OutputPath),
            ResolveMuxOutputPath(request.OutputPath),
            framerateValue,
            GetSourceVideoTimescale(request.SourceFfprobeJson));
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Expect different file input format depends on encoder selection
    /// </summary>
    /// <param name="encoderExeName">i.e., 264.exe</param>
    /// <param name="framerateValue">helping hevc mux, which does not have fps written</param>
    /// <returns>The -f command for ffmpeg mux</returns>
    private static string? GetMuxInputFormatArgs(string encoderExeName, string? framerateValue)
    {
        string? fmt = encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => null,
            "x265.exe" => "hevc",
            "svtav1encapp.exe" => "ivf",
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(framerateValue))
        {
            if (fmt == "hevc") return $"-f hevc -framerate {framerateValue}";
            if (fmt != null) return $"-f {fmt}";
            return null;
        }

        return fmt != null ? $"-f {fmt}" : null;
    }

    private static string RemoveRawVideoExtension(string outputPath)
    {
        string ext = Path.GetExtension(outputPath);
        return ext.Equals(".hevc", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".h265", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".h264", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".264", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".265", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".ivf", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(Path.GetDirectoryName(outputPath) ?? string.Empty, Path.GetFileNameWithoutExtension(outputPath))
            : outputPath;
    }

    private static string GetMuxFramerateValue(string? sourceFfprobeJson, string? filterArgs = null)
    {
        if (!string.IsNullOrWhiteSpace(filterArgs))
        {
            var match = FpsRegex().Match(filterArgs);
            if (match.Success)
            {
                string fps = match.Groups[1].Value;
                if (IsUsableFrameRate(fps)) return fps;
            }
        }

        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return string.Empty;

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(sourceFfprobeJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return string.Empty;
            string? frameRate = TryGetFrameRateString(stream);
            return FrameRate.TryParseFrameRate(frameRate, out _)
                ? frameRate!
                : string.Empty;
        }
        catch { return string.Empty; }
    }

    private static long GetSourceVideoTimescale(string? sourceFfprobeJson)
    {
        const long fallbackTimescale = 90000;
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return fallbackTimescale;

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return fallbackTimescale;

            string? timeBase = TryGetString(stream, "time_base");
            if (string.IsNullOrWhiteSpace(timeBase)) return fallbackTimescale;

            string[] parts = timeBase.Trim().Split('/');
            return parts.Length == 2 &&
                   long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out long denominator) &&
                   denominator > 0
                ? denominator
                : fallbackTimescale;
        }
        catch { return fallbackTimescale; }
    }

    private static bool IsUsableFrameRate(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Equals("0/0", StringComparison.OrdinalIgnoreCase)
        && !value.Equals("N/A", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"fps=(\d+/\d+)")]
    private static partial Regex FpsRegex();

    private static string? TryGetFrameRateString(JsonElement stream)
    {
        string? fps = TryGetString(stream, "avg_frame_rate");
        if (IsUsableFrameRate(fps)) return fps;

        fps = TryGetString(stream, "r_frame_rate");
        return IsUsableFrameRate(fps) ? fps : null;
    }

    private static string Quote(string value) =>
        $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string JoinArgs(params string?[] parts) =>
        string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static string EnsureExtension(string outputPath, string extension) =>
        string.IsNullOrEmpty(extension) || outputPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : outputPath + extension;

    #endregion
}
