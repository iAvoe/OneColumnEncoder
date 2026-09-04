using System.IO;
using OneColumnEncoder.Models;
using System.Text.RegularExpressions;
using static OneColumnEncoder.Models.JsonProviderM;

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
            || !request.AutoMuxEnabled
            || request.MuxMode == EncodingMuxMode.Disabled
            || string.IsNullOrWhiteSpace(request.FFmpegPath)) return null;
        if (request.MuxMode == EncodingMuxMode.VideoOnly)
            return BuildRepartMuxCommand(request) ?? BuildVideoOnlyMuxCommand(request);
        if (request.Clip != null) return null;
        bool hasExternalTracks = request.MuxTracks is { Count: > 0 };
        if (!request.IsConcatMode.GetValueOrDefault()
            && string.IsNullOrWhiteSpace(request.SourceVideoPath)
            && !hasExternalTracks) return null;

        MuxContext context = BuildMuxContext(request);
        EncodingAudioMuxMode audioMode = EncodingAudioMuxResolver.ResolveAudioMuxMode(request);
        string videoTimescaleArgs = $"-video_track_timescale {context.VideoTimescale}";
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, context.FramerateValue);

        bool isConcatMux =
            request.IsConcatMode.GetValueOrDefault()
            && request.ConcatFileListPath != null;
        // The concat demuxer is used only for the source timeline. Its video is not
        // mapped because the encoded video is the sole video stream in the output.
        // Attachment streams (fonts) are skipped to avoid duplicates when multiple
        // concat sources share the same embedded fonts.
        string streamMapArgs = isConcatMux
            ? BuildConcatStreamMapArgs(request.SourceFfprobeJson)
            : BuildStreamMapArgs(request.SourceFfprobeJson);

        bool hasSourceInput = isConcatMux || !string.IsNullOrWhiteSpace(request.SourceVideoPath);
        string? secondInput = isConcatMux
            ? $"-f concat -safe 0 -i {Quote(request.ConcatFileListPath!)}"
            : hasSourceInput ? $"-i {Quote(request.SourceVideoPath!)}" : null!;
        int externalInputIndex = hasSourceInput ? 2 : 1;
        string externalInputs = BuildExternalInputArgs(request.MuxTracks);

        string? audioMuxArgs = BuildAudioMuxArgs(audioMode);
        string sourceMapAndCodecArgs = hasSourceInput
            ? $"{streamMapArgs} -map_metadata 1 -map_chapters 1 {(audioMode == EncodingAudioMuxMode.Disable ? "-an" : audioMuxArgs)} -c:s copy"
            : (audioMode == EncodingAudioMuxMode.Disable ? "-an" : audioMuxArgs ?? string.Empty);
        int sourceSubtitleCount = isConcatMux ? 0 : GetStreamCount(request.SourceFfprobeJson, "subtitle");
        string externalMapAndCodecArgs = BuildExternalTrackMapArgs(request.MuxTracks, externalInputIndex, sourceSubtitleCount);
        string sourceDispositionArgs = isConcatMux
            ? string.Empty
            : BuildSourceSubtitleDispositionArgs(request.MuxTracks);

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            secondInput,
            externalInputs,
            $"-map 0:v:0 {sourceMapAndCodecArgs} {externalMapAndCodecArgs} {sourceDispositionArgs} -c:v copy -bsf:v setts=pts=N*DURATION {videoTimescaleArgs}",
            Quote(context.OutputPath));

        return new($"{Quote(request.FFmpegPath)} {args}", args, context.EncodedVideoPath, context.OutputPath);
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
            || string.IsNullOrWhiteSpace(request.FFmpegPath)) return null;
        (string startTime, string endTime)? range = GetRepartClipTimeRange(request);
        if (range == null) return null;

        MuxContext context = BuildMuxContext(request);
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, context.FramerateValue);
        if (EncodingAudioMuxResolver.IsReEncodeMode(audioMode))
            return BuildRepartAudioSplitMux(request, context, audioMode, range.Value, inputFormatArgs);

        string audioMapArgs = BuildConcatAudioMuxMapArgs(audioMode);
        if (audioMapArgs == "-an") return null;

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            $"-f concat -safe 0 -ss {range.Value.startTime} -to {range.Value.endTime} -i {Quote(request.ConcatFileListPath!)}",
            $"-map 0:v:0 {audioMapArgs} -c:v copy -bsf:v setts=pts=N*DURATION -video_track_timescale {context.VideoTimescale}",
            Quote(context.OutputPath));

        return new($"{Quote(request.FFmpegPath)} {args}", args, context.EncodedVideoPath, context.OutputPath);
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
    /// Builds stream-map args for concat mode.
    /// Skips attachment streams to avoid duplicate fonts when multiple concat
    /// sources share the same embedded fonts. Also skips data streams.
    /// </summary>
    private static string BuildConcatStreamMapArgs(string? sourceFfprobeJson)
    {
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson))
            return "-map 1:a?";

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!document.RootElement.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
                return "-map 1:a?";

            var mapArgs = new List<string>();
            foreach (JsonElement stream in streams.EnumerateArray())
            {
                string? codecType = TryGetString(stream, "codec_type");
                if (string.IsNullOrWhiteSpace(codecType)) continue;
                if (codecType.Equals("video", StringComparison.OrdinalIgnoreCase)) continue;
                if (codecType.Equals("attachment", StringComparison.OrdinalIgnoreCase)) continue;
                if (codecType.Equals("data", StringComparison.OrdinalIgnoreCase)) continue;

                if (!TryGetInt(stream, "index", out int streamIndex)) continue;
                mapArgs.Add($"-map 1:{streamIndex}");
            }

            if (mapArgs.Count > 0)
                return string.Join(" ", mapArgs);

            return "-map 1:a?";
        }
        catch { return "-map 1:a?"; }
    }

    /// <summary>
    /// Builds the mux command that keeps only the encoded video stream.
    /// </summary>
    /// <param name="request">Encoding request with source, clip, and mux settings.</param>
    /// <returns>Video-only mux command, or null when muxing is unavailable.</returns>
    public static EncodingMuxCommand? BuildVideoOnlyMuxCommand(EncodingPipelineRequest request)
    {
        if (!request.AutoMuxEnabled || string.IsNullOrWhiteSpace(request.FFmpegPath)) return null;

        MuxContext context = BuildMuxContext(request);
        string videoTimescaleArgs = $"-video_track_timescale {context.VideoTimescale}";
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, context.FramerateValue);

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            $"-map 0:v:0 -c:v copy -bsf:v setts=pts=N*DURATION {videoTimescaleArgs}",
            Quote(context.OutputPath));

        return new($"{Quote(request.FFmpegPath)} {args}", args, context.EncodedVideoPath, context.OutputPath);
    }

    /// <summary>
    /// Builds the repart mux as a split audio encode followed by a final mux when audio is re-encoded.
    /// </summary>
    private static EncodingMuxCommand BuildRepartAudioSplitMux(
        EncodingPipelineRequest request,
        MuxContext context,
        EncodingAudioMuxMode audioMode,
        (string StartTime, string EndTime) range,
        string? inputFormatArgs)
    {
        string audioCodecArgs = BuildAudioMuxArgs(audioMode)!;
        string audioOutputPath = ResolveTempAudioPath(context.OutputPath, audioMode);

        string audioArgs = JoinArgs(
            "-hide_banner -y",
            $"-f concat -safe 0 -ss {range.StartTime} -to {range.EndTime} -i {Quote(request.ConcatFileListPath!)}",
            $"-map 0:a? {audioCodecArgs}",
            Quote(audioOutputPath));
        string audioCommandLine = $"{Quote(request.FFmpegPath!)} {audioArgs}";
        EncodingAudioCommand audioCommand = new(audioCommandLine, audioArgs, audioOutputPath);

        string muxArgs = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(context.EncodedVideoPath)}",
            $"-i {Quote(audioOutputPath)}",
            $"-map 0:v:0 -map 1:a? -c copy -bsf:v setts=pts=N*DURATION -video_track_timescale {context.VideoTimescale}",
            Quote(context.OutputPath));
        string muxCommandLine = $"{Quote(request.FFmpegPath!)} {muxArgs}";

        return new(muxCommandLine, muxArgs, context.EncodedVideoPath, context.OutputPath, audioCommand);
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

    private static string BuildExternalInputArgs(IReadOnlyList<MuxTrackM>? tracks)
    {
        if (tracks is not { Count: > 0 }) return string.Empty;
        var externalTracks = tracks.Where(t => !t.IsSourceTrack).ToList();
        if (externalTracks.Count == 0) return string.Empty;
        return string.Join(" ", externalTracks.Select((track, index) =>
        {
            string offset = track.SyncMilliseconds == 0
                ? string.Empty
                : $"-itsoffset {track.SyncMilliseconds.ToString(CultureInfo.InvariantCulture)}ms";
            return JoinArgs(offset, $"-i {Quote(track.FilePath)}");
        }));
    }

    private static string BuildExternalTrackMapArgs(
        IReadOnlyList<MuxTrackM>? tracks,
        int inputIndex,
        int sourceSubtitleCount)
    {
        if (tracks is not { Count: > 0 }) return string.Empty;

        var externalTracks = tracks.Where(t => !t.IsSourceTrack).ToList();
        int subtitleIndex = sourceSubtitleCount;

        List<string> args = [];
        foreach (MuxTrackM track in externalTracks)
        {
            string meta = $"-map {inputIndex}:s:0 -c:s copy -metadata:s:s:{subtitleIndex} title={Quote(track.Name)}";
            if (!string.IsNullOrWhiteSpace(track.LanguageCode))
                meta += $" -metadata:s:s:{subtitleIndex} language={track.LanguageCode}";
            args.Add(meta);
            subtitleIndex++;
            inputIndex++;
        }

        MuxTrackM? defaultTrack = externalTracks.FirstOrDefault(track => track.IsDefault);
        if (defaultTrack != null)
        {
            subtitleIndex = sourceSubtitleCount;
            foreach (MuxTrackM track in externalTracks)
            {
                if (track.IsDefault)
                {
                    args.Add($"-disposition:s:{subtitleIndex} default");
                    break;
                }
                subtitleIndex++;
            }
        }

        return string.Join(" ", args);
    }

    private static string BuildSourceSubtitleDispositionArgs(IReadOnlyList<MuxTrackM>? tracks)
    {
        if (tracks is not { Count: > 0 }) return string.Empty;

        List<string> args = [];
        foreach (MuxTrackM track in tracks.Where(t => t.IsSourceTrack))
        {
            if (track.SourceSubtitleIndex == null) continue;
            int subtitleIndex = track.SourceSubtitleIndex.Value;

            if (track.IsDefault)
                args.Add($"-disposition:s:{subtitleIndex} default");
            else if (track.OriginalIsDefault)
                args.Add($"-disposition:s:{subtitleIndex} 0");

            if (!string.IsNullOrWhiteSpace(track.LanguageCode))
                args.Add($"-metadata:s:s:{subtitleIndex} language={track.LanguageCode}");
        }

        return string.Join(" ", args);
    }

    private static int GetStreamCount(string? sourceFfprobeJson, string codecType)
    {
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return 0;
        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!document.RootElement.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
                return 0;
            return streams.EnumerateArray().Count(stream =>
                string.Equals(TryGetString(stream, "codec_type"), codecType, StringComparison.OrdinalIgnoreCase));
        }
        catch { return 0; }
    }

    private static string BuildConcatAudioMuxMapArgs(EncodingAudioMuxMode mode)
    {
        string? audioMuxArgs = BuildAudioMuxArgs(mode);
        if (audioMuxArgs == null) return "-an";

        string audioMapArgs = mode == EncodingAudioMuxMode.Copy
            ? "-map 1:a:0?"
            : "-map 1:a?";
        return $"{audioMapArgs} {audioMuxArgs}";
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

    public static string ResolveMuxOutputPath(string outputPath) =>
        RemoveRawVideoExtension(outputPath) + ".mkv";

    private static MuxContext BuildMuxContext(EncodingPipelineRequest request)
    {
        string framerateValue = request.Clip?.FrameRateNumerator is > 0 && request.Clip.FrameRateDenominator is > 0
            ? $"{request.Clip.FrameRateNumerator.Value}/{request.Clip.FrameRateDenominator.Value}"
            : GetMuxFramerateValue(request.SourceFfprobeJson, request.FFmpegFilterArgs);

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
            var match = RegexProviderM.FpsRegex().Match(filterArgs);
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
