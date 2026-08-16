using OneColumnEncoder.CPU;
using OneColumnEncoder.Models.Encoding;
using System.IO;
using System.Text.RegularExpressions;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.Pipeline;

public record EncodingPipelineRequest(
    string UpstreamExeName,
    string UpstreamPath,
    string UpstreamInputPath,
    string EncoderExeName,
    string EncoderPath,
    string? FfmpegPath,
    string? SourceVideoPath,
    string OutputPath,
    EncoderConfM EncoderConf,
    string? VspipeY4mArg,
    EncodingClipRequest? Clip = null,
    string? SourceFfprobeJson = null,
    ParallelismConfM? ParallelismConf = null,
    string? SvfiIniPath = null,
    string? SvfiTaskId = null,
    string? FfmpegFilterArgs = null,
    bool? IsConcatMode = null,
    string? ConcatFileListPath = null,
    long? ConcatTotalFrames = null,
    EncodingMuxMode MuxMode = EncodingMuxMode.Auto,
    string[]? ConcatVideoSourcePaths = null);

public enum EncodingMuxMode
{
    Auto,
    Disabled,
    VideoOnly,
    SourceStreams
}

// For clip sampler
public record EncodingClipRequest(
    string? StartTime = null,
    string? EndTime = null,
    long? FirstFrame = null,
    long? LastFrame = null,
    double? FrameRate = null,
    int? FrameRateNumerator = null,
    int? FrameRateDenominator = null);

public record EncodingPipelineCommand(
    string CommandLine,
    string UpstreamArgs,
    string EncoderArgs,
    EncodingMuxCommand? MuxCommand = null)
{
    public string DisplayCommandLine => MuxCommand == null
        ? CommandLine
        : $"{CommandLine}{Environment.NewLine}{Environment.NewLine}{MuxCommand.CommandLine}";
}

public record EncodingMuxCommand(
    string CommandLine,
    string Arguments,
    string EncodedVideoPath,
    string OutputPath);

public static partial class EncodingPipeline
{
    public static EncodingPipelineCommand BuildY4mCommand(EncodingPipelineRequest request)
    {
        string upstreamArgs = BuildUpstreamArgs(request);
        string encoderArgs = BuildEncoderArgs(request);
        string commandLine = $"{Quote(request.UpstreamPath)} {upstreamArgs} | {Quote(request.EncoderPath)} {encoderArgs}";
        return new(commandLine, upstreamArgs, encoderArgs, BuildMuxCommand(request));
    }

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

    private static EncodingMuxCommand? BuildMuxCommand(EncodingPipelineRequest request)
    {
        if (request.MuxMode == EncodingMuxMode.Disabled) return null;
        if (request.MuxMode == EncodingMuxMode.VideoOnly) return BuildVideoOnlyMuxCommand(request);
        if (request.Clip != null) return null;
        if (string.IsNullOrWhiteSpace(request.FfmpegPath)) return null;
        if (!request.IsConcatMode.GetValueOrDefault() && string.IsNullOrWhiteSpace(request.SourceVideoPath)) return null;

        string encodedVideoPath = ResolveOutputPathWithExtension(request.EncoderExeName, request.OutputPath);
        string outputPath = ResolveMuxOutputPath(request.OutputPath);
        string framerateValue = request.Clip?.FrameRateNumerator is > 0 && request.Clip.FrameRateDenominator is > 0
            ? $"{request.Clip.FrameRateNumerator.Value}/{request.Clip.FrameRateDenominator.Value}"
            : GetMuxFramerateValue(request.SourceFfprobeJson, request.FfmpegFilterArgs);
        string videoTimescaleArgs = GetMuxVideoTrackTimescaleArgs(request.SourceFfprobeJson);
        string streamMapArgs = BuildStreamMapArgs(request.SourceFfprobeJson);
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, framerateValue);

        bool isConcatMux = request.IsConcatMode.GetValueOrDefault() && request.ConcatFileListPath != null;
        string secondInput = isConcatMux
            ? $"-f concat -safe 0 -i {Quote(request.ConcatFileListPath!)}"
            : $"-i {Quote(request.SourceVideoPath!)}";
        string nonVideoMapAndCodecArgs = isConcatMux
            ? "-map 1:a? -c:a copy"
            : $"{streamMapArgs} -map_metadata 1 -map_chapters 1 -c:a copy -c:s copy";

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(encodedVideoPath)}",
            secondInput,
            $"-map 0:v:0 {nonVideoMapAndCodecArgs} -c:v copy -bsf:v setts=pts=N*DURATION {videoTimescaleArgs}",
            Quote(outputPath));

        return new($"{Quote(request.FfmpegPath)} {args}", args, encodedVideoPath, outputPath);
    }

    private static string BuildStreamMapArgs(string? sourceFfprobeJson)
    {
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

    private static string BuildUpstreamArgs(EncodingPipelineRequest request)
    {
        string input = Quote(request.UpstreamInputPath);
        bool isConcat = request.IsConcatMode == true && request.ConcatFileListPath != null;
        bool isFrameExactRepart = isConcat
            && request.MuxMode == EncodingMuxMode.VideoOnly
            && request.Clip?.FirstFrame is long
            && request.Clip.LastFrame is long;
        bool isFfmpeg = request.UpstreamExeName.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase);
        string clipArgs = isFrameExactRepart && isFfmpeg
            ? string.Empty
            : BuildUpstreamClipArgs(request.UpstreamExeName, request.Clip);
        string? ffmpegFilterArgs = isFrameExactRepart && isFfmpeg
            ? BuildFrameExactRepartFilter(request)
            : request.FfmpegFilterArgs;
        if (isFrameExactRepart && isFfmpeg && request.ConcatVideoSourcePaths is { Length: > 0 })
            return BuildFfmpegRepartArgs(request);
        return request.UpstreamExeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => isConcat
                ? JoinArgs("-hide_banner", "-f concat -safe 0", $"-i {Quote(request.ConcatFileListPath!)}", clipArgs, ffmpegFilterArgs, "-f yuv4mpegpipe -an -strict unofficial -")
                : JoinArgs($"-hide_banner", clipArgs, $"-i {input}", ffmpegFilterArgs, "-f yuv4mpegpipe -an -strict unofficial -"), // unofficial allows 10bit pipe
            "vspipe.exe" => JoinArgs(input, clipArgs, NormalizeRequired(request.VspipeY4mArg, "vspipe Y4M argument"), "-"),
            "avs2yuv.exe" => JoinArgs(input, clipArgs, "-"),
            "avs2pipemod.exe" => JoinArgs(input, clipArgs, "-y4mp"),
            "one_line_shot_args.exe" => JoinArgs(
                $"--input {input}",
                request.SvfiIniPath != null ? $"--config {Quote(request.SvfiIniPath)}" : null,
                request.SvfiTaskId != null ? $"--task-id {request.SvfiTaskId}" : null,
                "--pipe-out"),
            _ => throw new InvalidOperationException($"Unsupported upstream tool: {request.UpstreamExeName}")
        };
    }

    private static string BuildFfmpegRepartArgs(EncodingPipelineRequest request)
    {
        string[] paths = request.ConcatVideoSourcePaths!;
        string inputs = string.Join(" ", paths.Select(path => $"-i {Quote(path)}"));
        EncodingClipRequest clip = request.Clip!;
        string setPts = BuildRepartSetPts(request);
        string? extraFilter = ExtractVideoFilter(request.FfmpegFilterArgs);
        if (paths.Length == 1)
        {
            string filter = JoinFilterChain(
                $"trim=start_frame={clip.FirstFrame!.Value}:end_frame={clip.LastFrame!.Value + 1}",
                $"setpts={setPts}",
                extraFilter);
            return JoinArgs(
                "-hide_banner",
                inputs,
                $"-vf \"{filter}\" -fps_mode passthrough",
                "-f yuv4mpegpipe -an -strict unofficial -");
        }

        string resetInputs = string.Join(";", Enumerable.Range(0, paths.Length)
            .Select(index => $"[{index}:v:0]setpts=PTS-STARTPTS[rv{index}]"));
        string concatInputs = string.Concat(Enumerable.Range(0, paths.Length).Select(index => $"[rv{index}]"));
        string filterComplex =
            $"{resetInputs};{concatInputs}concat=n={paths.Length}:v=1:a=0," +
            $"trim=start_frame={clip.FirstFrame!.Value}:end_frame={clip.LastFrame!.Value + 1}," +
            $"setpts={setPts}" +
            (string.IsNullOrWhiteSpace(extraFilter) ? string.Empty : $",{extraFilter}") +
            "[repartv]";
        return JoinArgs(
            "-hide_banner",
            inputs,
            $"-filter_complex \"{filterComplex}\" -map \"[repartv]\" -fps_mode passthrough",
            "-f yuv4mpegpipe -an -strict unofficial -");
    }

    private static string JoinFilterChain(params string?[] filters) =>
        string.Join(",", filters.Where(filter => !string.IsNullOrWhiteSpace(filter)));

    private static string? ExtractVideoFilter(string? filterArgs)
    {
        if (string.IsNullOrWhiteSpace(filterArgs)) return null;

        Match match = FFmpegFilterVScaleRegex().Match(filterArgs);
        if (!match.Success) return null;

        return match.Groups["quoted"].Success
            ? match.Groups["quoted"].Value
            : match.Groups["single"].Success
                ? match.Groups["single"].Value
                : match.Groups["plain"].Value;
    }

    private static string BuildFrameExactRepartFilter(EncodingPipelineRequest request)
    {
        EncodingClipRequest clip = request.Clip!;
        string setPts = BuildRepartSetPts(request);
        return $"-vf \"trim=start_frame={clip.FirstFrame!.Value}:end_frame={clip.LastFrame!.Value + 1},setpts={setPts}\" -fps_mode passthrough";
    }

    private static string BuildRepartSetPts(EncodingPipelineRequest request)
    {
        EncodingClipRequest clip = request.Clip!;
        (int num, int den)? rate = clip.FrameRateNumerator is > 0 && clip.FrameRateDenominator is > 0
            ? (clip.FrameRateNumerator.Value, clip.FrameRateDenominator.Value)
            : string.IsNullOrWhiteSpace(request.SourceFfprobeJson)
                ? null
                : FrameRate.GetRFrameRate(request.SourceFfprobeJson!);
        return rate is { num: > 0, den: > 0 }
            ? $"N*{rate.Value.den}/({rate.Value.num}*TB)"
            : "N/FRAME_RATE/TB";
    }

    public static string BuildUpstreamClipArgs(string upstreamExeName, EncodingClipRequest? clip)
    {
        if (clip == null ||
            clip.StartTime == null && clip.EndTime == null && clip.FirstFrame == null && clip.LastFrame == null)
            return string.Empty;

        return upstreamExeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => BuildFfmpegClipArgs(BuildClipRange(clip, needsTimes: true, needsFrames: false)),
            "vspipe.exe" => BuildVspipeClipArgs(BuildClipRange(clip, needsTimes: false, needsFrames: true)),
            "avs2yuv.exe" => BuildAvs2yuvClipArgs(BuildClipRange(clip, needsTimes: false, needsFrames: true)),
            "avs2pipemod.exe" => BuildAvs2pipemodClipArgs(BuildClipRange(clip, needsTimes: false, needsFrames: true)),
            "one_line_shot_args.exe" => string.Empty,
            _ => throw new InvalidOperationException($"Unsupported upstream tool: {upstreamExeName}")
        };
    }

    private static string BuildFfmpegClipArgs(EncodingClipRequest? clip) =>
        clip == null
            ? string.Empty
            : JoinArgs(
                clip.StartTime == null ? null : $"-ss {clip.StartTime}",
                clip.EndTime == null ? null : $"-to {clip.EndTime}");

    private static string BuildVspipeClipArgs(EncodingClipRequest? clip)
    {
        if (clip == null) return string.Empty;
        long? firstFrame = clip.FirstFrame ?? (clip.LastFrame.HasValue ? 0 : null);
        return JoinArgs(
            firstFrame == null ? null : $"-s {firstFrame}",
            clip.LastFrame == null ? null : $"-e {clip.LastFrame}");
    }

    private static string BuildAvs2yuvClipArgs(EncodingClipRequest? clip)
    {
        if (clip == null) return string.Empty;
        long? firstFrame = clip.FirstFrame;
        long? lastFrame = clip.LastFrame;
        long? frameCount = lastFrame == null
            ? null
            : lastFrame.Value - (firstFrame ?? 0) + 1;

        return JoinArgs(
            firstFrame == null ? null : $"-seek {firstFrame}",
            frameCount == null ? null : $"-frames {frameCount}");
    }

    private static string BuildAvs2pipemodClipArgs(EncodingClipRequest? clip)
    {
        if (clip == null || clip.FirstFrame == null && clip.LastFrame == null) return string.Empty;
        long firstFrame = clip.FirstFrame ?? 0;
        if (clip.LastFrame == null) return $"-trim={firstFrame}";
        return $"-trim={firstFrame},{clip.LastFrame}";
    }

    #region Sample clip modal stuffs
    private static EncodingClipRequest? BuildClipRange(EncodingClipRequest? clip, bool needsTimes, bool needsFrames)
    {
        if (clip == null) return null;
        if (clip.StartTime == null && clip.EndTime == null && clip.FirstFrame == null && clip.LastFrame == null)
            return null;

        string? startTime = NormalizeTimestamp(clip.StartTime);
        string? endTime = NormalizeTimestamp(clip.EndTime);
        long? firstFrame = ValidateFrame(clip.FirstFrame, nameof(clip.FirstFrame));
        long? lastFrame = ValidateFrame(clip.LastFrame, nameof(clip.LastFrame));

        double? frameRate = clip.FrameRate;
        bool needsFrameRate = needsFrames && (startTime != null && firstFrame == null || endTime != null && lastFrame == null)
            || needsTimes && (firstFrame != null && startTime == null || lastFrame != null && endTime == null);

        if (needsFrameRate && frameRate == null)
            throw new InvalidOperationException("Clip time/frame conversion requires a frame rate.");

        if (frameRate != null) ValidateFrameRate(frameRate.Value);

        if (needsFrames && firstFrame == null && startTime != null && frameRate != null)
            firstFrame = TimestampToFirstFrame(startTime, frameRate.Value);

        if (needsFrames && lastFrame == null && endTime != null && frameRate != null)
            lastFrame = TimestampToLastFrame(endTime, frameRate.Value);

        if (needsTimes && startTime == null && firstFrame != null && frameRate != null)
            startTime = FrameToTimestamp(firstFrame.Value, frameRate.Value);

        if (needsTimes && endTime == null && lastFrame != null && frameRate != null)
            endTime = LastFrameToEndTimestamp(lastFrame.Value, frameRate.Value);

        if (firstFrame != null && lastFrame != null && lastFrame < firstFrame)
            throw new InvalidOperationException("Clip last frame must be greater than or equal to first frame.");

        if (startTime != null && endTime != null && ParseTimestamp(endTime) <= ParseTimestamp(startTime))
            throw new InvalidOperationException("Clip end time must be greater than start time.");

        return new(startTime, endTime, firstFrame, lastFrame);
    }

    public static long TimestampToFirstFrame(string timestamp, double frameRate)
    {
        ValidateFrameRate(frameRate);
        return (long)Math.Ceiling(ParseTimestamp(timestamp).TotalSeconds * frameRate);
    }

    public static long TimestampToLastFrame(string timestamp, double frameRate)
    {
        ValidateFrameRate(frameRate);
        long lastFrame = (long)Math.Ceiling(ParseTimestamp(timestamp).TotalSeconds * frameRate) - 1;
        if (lastFrame < 0)
            throw new InvalidOperationException("Clip end time must include at least one frame.");
        return lastFrame;
    }

    public static string FrameToTimestamp(long frame, double frameRate)
    {
        ValidateFrame(frame, nameof(frame));
        ValidateFrameRate(frameRate);
        return FormatTimestamp(TimeSpan.FromSeconds(frame / frameRate));
    }

    public static string LastFrameToEndTimestamp(long lastFrame, double frameRate)
    {
        ValidateFrame(lastFrame, nameof(lastFrame));
        ValidateFrameRate(frameRate);
        return FormatTimestamp(TimeSpan.FromSeconds((lastFrame + 1) / frameRate));
    }

    public static TimeSpan ParseTimestamp(string timestamp)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
            throw new InvalidOperationException("Clip timestamp cannot be empty.");

        string[] parts = timestamp.Trim().Split(':');
        if (parts.Length is < 2 or > 3)
            throw new InvalidOperationException("Clip timestamp must use [HH:]MM:SS[.sss].");

        if (!long.TryParse(parts.Length == 3 ? parts[0] : "0", NumberStyles.None, CultureInfo.InvariantCulture, out long hours) || hours < 0)
            throw new InvalidOperationException("Clip timestamp hour must be a non-negative integer.");

        if (!int.TryParse(parts[^2], NumberStyles.None, CultureInfo.InvariantCulture, out int minutes) || minutes is < 0 or > 59)
            throw new InvalidOperationException("Clip timestamp minute must be 0-59.");

        if (!double.TryParse(parts[^1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double seconds) || seconds < 0 || seconds >= 60)
            throw new InvalidOperationException("Clip timestamp second must be 0-59.999...");

        return TimeSpan.FromSeconds(hours * 3600d + minutes * 60d + seconds);
    }

    public static string FormatTimestamp(TimeSpan timestamp)
    {
        if (timestamp < TimeSpan.Zero)
            throw new InvalidOperationException("Clip timestamp cannot be negative.");

        long totalHours = (long)timestamp.TotalHours;
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:00}:{1:00}:{2:00}.{3:000}",
            totalHours,
            timestamp.Minutes,
            timestamp.Seconds,
            timestamp.Milliseconds);
    }

    private static string? NormalizeTimestamp(string? timestamp) =>
        string.IsNullOrWhiteSpace(timestamp)
            ? null
            : FormatTimestamp(ParseTimestamp(timestamp));

    private static long? ValidateFrame(long? frame, string name)
    {
        if (frame < 0)
            throw new InvalidOperationException($"Clip {name} must be non-negative.");
        return frame;
    }

    private static void ValidateFrameRate(double frameRate)
    {
        if (double.IsNaN(frameRate) || double.IsInfinity(frameRate) || frameRate <= 0)
            throw new InvalidOperationException("Clip frame rate must be a positive number.");
    }
    #endregion

    private static string BuildEncoderArgs(EncodingPipelineRequest request)
    {
        string y4mInputArgs = GetEncoderY4mInputArgs(request.EncoderExeName);
        string encodeParams = BuildEncoderParams(request.EncoderExeName, request.EncoderConf);
        string encoderCustomParams = request.EncoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => request.EncoderConf?.CustomParamsX264 ?? "",
            "x265.exe" => request.EncoderConf?.CustomParamsX265 ?? "",
            "svtav1encapp.exe" => request.EncoderConf?.CustomParamsSvtAv1 ?? "",
            _ => ""
        };
        string customParams = FilterCustomParamsForEncoder(encoderCustomParams, request.EncoderExeName);
        string autoParams = BuildAutoGeneratedEncoderParams(request, encodeParams, customParams);
        string parallelismParams = BuildParallelismEncoderParams(request);
        string outputArgs = BuildEncoderOutputArgs(request.EncoderExeName, request.OutputPath);
        return JoinArgs(y4mInputArgs, encodeParams, autoParams, parallelismParams, customParams, outputArgs);
    }

    private static string GetEncoderY4mInputArgs(string encoderExeName) =>
        encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => "--demuxer y4m -",
            "x265.exe" => "--y4m -",
            "svtav1encapp.exe" => "-i -",
            _ => throw new InvalidOperationException($"Unsupported encoder: {encoderExeName}")
        };

    public static string BuildEncoderParams(string encoderExeName, EncoderConfM model)
    {
        bool useAbr = model.RateControlMode.Equals("ABR", StringComparison.OrdinalIgnoreCase);
        return encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => BuildX264Params(model, useAbr),
            "x265.exe" => BuildX265Params(model, useAbr),
            "svtav1encapp.exe" => BuildSvtAv1Params(model, useAbr),
            _ => throw new InvalidOperationException($"Unsupported encoder: {encoderExeName}")
        };
    }

    private static string BuildParallelismEncoderParams(EncodingPipelineRequest request)
    {
        ParallelismConfM? parallelismConf = request.ParallelismConf;
        if (parallelismConf == null) return string.Empty;

        int threads = CpuSets.ClampThreadCountForNode(
            parallelismConf.DownstreamNodeId,
            parallelismConf.PreferPhysicalCores,
            parallelismConf.EncoderThreadCount);

        return request.EncoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => $"--threads {threads}",
            "x265.exe" => BuildX265PoolsParam(parallelismConf.DownstreamNodeId, threads),
            _ => string.Empty
        };
    }

    private static string BuildX265PoolsParam(int nodeId, int threads)
    {
        if (nodeId <= 0) return $"--pools {threads}";

        IEnumerable<string> pools = Enumerable.Repeat("-", nodeId)
            .Append(threads.ToString(CultureInfo.InvariantCulture));
        return $"--pools {string.Join(",", pools)}";
    }

    private static string FilterCustomParamsForEncoder(string customParams, string encoderExeName)
    {
        if (string.IsNullOrWhiteSpace(customParams)) return string.Empty;

        // Strip the 3rd party parameters that are unfit to an encoder
        HashSet<string> stripPrefixes = encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => ["--aq-auto", "--aq-bias-strength", "--aq-strength-edge", "--enable-dlf", "--auto-tiling"],
            "x265.exe" => ["--fgo", "--enable-dlf", "--auto-tiling"],
            "svtav1encapp.exe" => ["--fgo", "--aq-auto", "--aq-bias-strength", "--aq-strength-edge"],
            _ => []
        };

        if (stripPrefixes.Count == 0) return customParams;

        var tokens = customParams.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();
        bool skipNext = false;

        foreach (string token in tokens)
        {
            if (skipNext) { skipNext = false; continue; }
            if (stripPrefixes.Contains(token))
            {
                if (token != "--fgo") skipNext = true;
                continue;
            }
            result.Add(token);
        }

        return string.Join(" ", result);
    }

    public static long? GetSourceTotalFrames(string? sourceFfprobeJson, long? concatTotalFrames = null)
    {
        if (concatTotalFrames < 0) return null;
        if (concatTotalFrames > 0) return concatTotalFrames.Value;

        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return null;

            long? frameCount = TryGetFrameCount(stream);
            if (frameCount is > 0) return frameCount.Value;

            double? durationSeconds = TryGetDouble(stream, "duration")
                ?? (document.RootElement.TryGetProperty("format", out JsonElement format)
                    ? TryGetDouble(format, "duration")
                    : null);
            if (durationSeconds is null || durationSeconds <= 0d) return null;

            double? frameRate = null;
            if (FrameRate.TryParseFrameRate(TryGetString(stream, "avg_frame_rate"), out double avgFrameRate))
                frameRate = avgFrameRate;
            else if (FrameRate.TryParseFrameRate(TryGetString(stream, "r_frame_rate"), out double rFrameRate))
                frameRate = rFrameRate;

            if (frameRate is null || frameRate <= 0d) return null;
            return Math.Max(0L, (long)Math.Round(durationSeconds.Value * frameRate.Value));
        }
        catch { return null; }
    }

    public static long? GetExpectedOutputFrames(EncodingPipelineRequest request)
    {
        long? clipFrames = GetClipFrameCount(request.Clip);
        return clipFrames is > 0
            ? clipFrames
            : GetSourceTotalFrames(request.SourceFfprobeJson, request.ConcatTotalFrames);
    }

    private static string BuildX264Params(EncoderConfM model, bool useAbr)
    {
        string preset = EncoderPresetsM.GetX264Preset(model.X264Mode)?.Params ?? string.Empty;
        string rateControl = useAbr ? $"--bitrate {model.X264Abr * 1000}" : $"--crf {model.X264Crf}";
        return JoinArgs(
            preset.Replace("$crfParam", rateControl, StringComparison.Ordinal),
            $"--keyint {model.X264Keyframe * 24}",
            model.X264Mod ? "--fgo" : string.Empty);
    }

    private static string BuildX265Params(EncoderConfM model, bool useAbr)
    {
        string preset = EncoderPresetsM.GetX265Preset(model.X265Mode)?.Params ?? string.Empty;
        string rateControl = useAbr ? $"--bitrate {model.X265Abr * 1000}" : $"--crf {model.X265Crf}";
        return JoinArgs(
            preset.Replace("$crfParam", rateControl, StringComparison.Ordinal),
            $"--keyint {model.X265Keyframe * 24}",
            model.X265Aq ? "--aq-auto 10" : string.Empty,
            model.X265Dark ? "--aq-bias-strength 1.3" : string.Empty,
            model.X265Texture ? "--aq-strength-edge 1.4" : string.Empty);
    }

    private static string BuildSvtAv1Params(EncoderConfM model, bool useAbr)
    {
        string preset = EncoderPresetsM.GetSvtAv1Preset(model.SvtAv1Mode)?.Params ?? string.Empty;
        string rateControl = useAbr ? $"--rc 1 --tbr {model.SvtAv1Abr * 1000}" : $"--crf {model.SvtAv1Crf}";
        return JoinArgs(
            preset.Replace("$crfParam", rateControl, StringComparison.Ordinal)
                .Replace("$deblock", model.SvtAv1Dl2 ? "--enable-dlf 2" : "--enable-dlf 1", StringComparison.Ordinal),
            $"--keyint {model.SvtAv1Keyframe * 24}",
            model.SvtAv1AutoTile ? "--auto-tiling 1" : string.Empty);
    }

    #region Auto Params From FFprobe
    private static string BuildAutoGeneratedEncoderParams(
        EncodingPipelineRequest request,
        string baseEncoderParams,
        string customParams)
    {
        if (string.IsNullOrWhiteSpace(request.SourceFfprobeJson)) return string.Empty;

        using JsonDocument document = JsonDocument.Parse(request.SourceFfprobeJson);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return string.Empty;
        FFProbeHdrInfo hdrInfo = FFProbeHdrInfoReader.Read(document.RootElement);

        string encoder = request.EncoderExeName.ToLowerInvariant();
        bool isX264 = encoder == "x264.exe";
        bool isX265 = encoder == "x265.exe";
        bool isSvtAv1 = encoder == "svtav1encapp.exe";
        if (!isX264 && !isX265 && !isSvtAv1) return string.Empty;

        string? fpsString = TryGetFrameRateString(stream);
        string? pixelFormat = TryGetString(stream, "pix_fmt");
        string? width = TryGetString(stream, "width");
        string? height = TryGetString(stream, "height");

        // Y4M pipeline skips upstream-only or Y4M-header-provided parameters:
        // Get-ffmpegCSP (-pix_fmt), Get-InputResolution, Get-FPSParam,
        // and Get-EncoderAVSRawCSPBits are intentionally not emitted here.
        // The frame-count parameter (--frames / -n) is deliberately NOT emitted:
        // nb_frames is unreliable and can produce wrong output length.
        _ = (pixelFormat, width, height);

        return encoder switch
        {
            "x264.exe" => JoinArgs(
                GetRateControlLookahead(fpsString, TryGetIntegerArg(baseEncoderParams, "--bframes") ?? 0),
                GetColorSpaceSei(stream, isX264: true, isX265: false, isSvtAv1: false),
                GetRangeChromaLocation(stream, isX264: true, isX265: false, isSvtAv1: false),
                GetHdrParams(encoder, hdrInfo, customParams)),
            "x265.exe" => JoinArgs(
                GetRateControlLookahead(fpsString, TryGetIntegerArg(baseEncoderParams, "--bframes") ?? 0),
                GetX265MeRange(stream),
                GetX265SubmotionEstimation(fpsString),
                GetColorSpaceSei(stream, isX264: false, isX265: true, isSvtAv1: false),
                GetRangeChromaLocation(stream, isX264: false, isX265: true, isSvtAv1: false),
                GetHdrParams(encoder, hdrInfo, customParams)),
            "svtav1encapp.exe" => JoinArgs(
                GetColorSpaceSei(stream, isX264: false, isX265: false, isSvtAv1: true),
                GetRangeChromaLocation(stream, isX264: false, isX265: false, isSvtAv1: true),
                GetHdrParams(encoder, hdrInfo, customParams)),
            _ => string.Empty
        };
    }

    private static string GetHdrParams(string encoder, FFProbeHdrInfo hdrInfo, string customParams)
    {
        if (!hdrInfo.HasHdr10 || hdrInfo.MasteringDisplay is not { } masteringDisplay)
            return string.Empty;

        bool hasDisplayOption = encoder switch
        {
            "x264.exe" => HasAnyOption(customParams, "--mastering-display", "--master-display"),
            "x265.exe" => HasAnyOption(customParams, "--master-display", "--mastering-display"),
            "svtav1encapp.exe" => HasAnyOption(customParams, "--mastering-display", "--master-display"),
            _ => true
        };

        bool hasCllOption = encoder switch
        {
            "x264.exe" => HasAnyOption(customParams, "--cll", "--max-cll", "--content-light"),
            "x265.exe" => HasAnyOption(customParams, "--max-cll", "--cll", "--content-light"),
            "svtav1encapp.exe" => HasAnyOption(customParams, "--content-light", "--cll", "--max-cll"),
            _ => true
        };

        string hdrDisplay = string.Empty;
        if (!hasDisplayOption)
        {
            hdrDisplay = encoder switch
            {
                "x264.exe" => BuildHdrDisplayParam("--mastering-display", FFProbeHdrInfoReader.ToX264MasteringDisplay(masteringDisplay)),
                "x265.exe" => BuildHdrDisplayParam("--master-display", FFProbeHdrInfoReader.ToX265MasterDisplay(masteringDisplay)),
                "svtav1encapp.exe" => BuildHdrDisplayParam("--mastering-display", FFProbeHdrInfoReader.ToSvtAv1MasteringDisplay(masteringDisplay)),
                _ => string.Empty
            };
        }

        string hdrCll = string.Empty;
        if (!hasCllOption && hdrInfo.ContentLightLevel is { HasValue: true } contentLightLevel)
        {
            hdrCll = encoder switch
            {
                "x264.exe" => BuildHdrDisplayParam("--cll", FFProbeHdrInfoReader.ToX264ContentLight(contentLightLevel)),
                "x265.exe" => BuildHdrDisplayParam("--max-cll", FFProbeHdrInfoReader.ToX265ContentLight(contentLightLevel)),
                "svtav1encapp.exe" => BuildHdrDisplayParam("--content-light", FFProbeHdrInfoReader.ToSvtAv1ContentLight(contentLightLevel)),
                _ => string.Empty
            };
        }

        return JoinArgs(hdrDisplay, hdrCll);

        string BuildHdrDisplayParam(string optionName, string value) =>
            $"{optionName} \"{value}\"";
    }

    private static bool HasAnyOption(string parameters, params string[] optionNames)
    {
        if (string.IsNullOrWhiteSpace(parameters)) return false;
        return optionNames.Any(optionName => parameters.Contains(optionName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetRateControlLookahead(string? fpsString, int bframes)
    {
        if (!FrameRate.TryParseFrameRate(fpsString, out double fps)) return string.Empty;
        int frames = Math.Max((int)Math.Round(fps * 1.8d), bframes + 1);
        return $"--rc-lookahead {frames}";
    }

    private static string GetX265MeRange(JsonElement stream)
    {
        if (!TryGetInt(stream, "width", out int width) || !TryGetInt(stream, "height", out int height))
            return string.Empty;

        long pixels = (long)width * height;
        int merange = pixels switch
        {
            >= 8294400 => 56,
            >= 3686400 => 52,
            >= 2073600 => 48,
            >= 921600 => 40,
            _ => 36
        };
        return $"--merange {merange}";
    }

    private static string GetX265SubmotionEstimation(string? fpsString)
    {
        if (!FrameRate.TryParseFrameRate(fpsString, out double fps)) return string.Empty;
        int subme = fps < 25 ? 3 : fps < 49 ? 4 : fps < 61 ? 5 : 6;
        return $"--subme {subme}";
    }

    private static long? GetClipFrameCount(EncodingClipRequest? clip)
    {
        if (clip == null) return null;

        long? firstFrame = clip.FirstFrame;
        long? lastFrame = clip.LastFrame;

        if (firstFrame == null && clip.StartTime != null && clip.FrameRate != null)
            firstFrame = TimestampToFirstFrame(clip.StartTime, clip.FrameRate.Value);
        if (lastFrame == null && clip.EndTime != null && clip.FrameRate != null)
            lastFrame = TimestampToLastFrame(clip.EndTime, clip.FrameRate.Value);

        if (lastFrame == null) return null;
        long first = firstFrame ?? 0;
        return lastFrame >= first ? lastFrame - first + 1 : null;
    }

    private static string GetColorSpaceSei(JsonElement stream, bool isX264, bool isX265, bool isSvtAv1)
    {
        string? colorMatrix = NormalizeMetadata(TryGetString(stream, "color_space"));
        string? transfer = NormalizeMetadata(TryGetString(stream, "color_transfer"));
        string? primaries = NormalizeMetadata(TryGetString(stream, "color_primaries"));
        if (colorMatrix == null || transfer == null || primaries == null) return string.Empty;

        if (isX264)
        {
            string matrixArg = colorMatrix is "unknown" or "unspec"
                ? "--colormatrix undef"
                : $"--colormatrix {colorMatrix}";
            string transferArg = transfer == "unknown"
                ? "--transfer undef"
                : $"--transfer {transfer}";
            string primariesArg = primaries is "unknown" or "unspec"
                ? "--colorprim undef"
                : $"--colorprim {primaries}";
            return JoinArgs(matrixArg, transferArg, primariesArg);
        }

        if (isX265)
        {
            string matrixArg = colorMatrix is "unknown" or "unspec"
                ? "--colormatrix unknown"
                : $"--colormatrix {colorMatrix}";
            string primariesArg = primaries is "unknown" or "unspec"
                ? "--colorprim unknown"
                : $"--colorprim {primaries}";
            return JoinArgs(matrixArg, $"--transfer {transfer}", primariesArg);
        }

        if (!isSvtAv1) return string.Empty;
        return JoinArgs(
            $"--matrix-coefficients {MapSvtAv1Matrix(colorMatrix)}",
            $"--transfer-characteristics {MapSvtAv1Transfer(transfer)}",
            $"--color-primaries {MapSvtAv1Primaries(primaries)}");
    }

    private static string GetRangeChromaLocation(JsonElement stream, bool isX264, bool isX265, bool isSvtAv1)
    {
        string pixelFormat = NormalizeMetadata(TryGetString(stream, "pix_fmt")) ?? string.Empty;
        string range = NormalizeMetadata(TryGetString(stream, "color_range")) ?? string.Empty;
        string chromaLocation = NormalizeMetadata(TryGetString(stream, "chroma_location")) ?? string.Empty;

        List<string> result = [];
        if (range is "tv" or "pc")
        {
            if (isX264 && range == "pc") result.Add("--fullrange");
            else if (isX265) result.Add(range == "pc" ? "--range full" : "--range limited");
            else if (isSvtAv1) result.Add(range == "pc" ? "--color-range 1" : "--color-range 0");
        }

        int chromaDepth = FFProbePixelFormatRules.GetChromaSubsamplingDepth(pixelFormat);
        if (chromaDepth <= 0) return JoinArgs([.. result]);

        if (isX264 || isX265)
        {
            int? chromaloc = chromaLocation switch
            {
                "left" => 0,
                "center" => 1,
                "topleft" => 2,
                "top" => 3,
                "bottomleft" => 4,
                "bottom" => 5,
                _ => null
            };
            if (chromaloc != null) result.Add($"--chromaloc {chromaloc}");
        }
        else if (isSvtAv1)
        {
            string? chromaSamplePosition = chromaLocation switch
            {
                "left" => "left",
                "topleft" => "topleft",
                "unknown" => "unknown",
                _ => null
            };
            if (chromaSamplePosition != null)
                result.Add($"--chroma-sample-position {chromaSamplePosition}");
        }

        return JoinArgs([.. result]);
    }

    private static int MapSvtAv1Matrix(string value) => value switch
    {
        "identity" => 0,
        "bt709" => 1,
        "unspec" => 2,
        "fcc" => 4,
        "bt470bg" => 5,
        "bt601" => 6,
        "smpte240m" => 7,
        "ycgco" => 8,
        "bt2020nc" => 9,
        "bt2020-ncl" => 9,
        "bt2020c" => 10,
        "bt2020-cl" => 10,
        "smpte2085" => 11,
        "chroma-ncl" => 12,
        "chroma-cl" => 13,
        "ictcp" => 14,
        _ => 1
    };

    private static int MapSvtAv1Transfer(string value) => value switch
    {
        "bt709" => 1,
        "unspec" => 2,
        "bt470m" => 4,
        "bt470bg" => 5,
        "bt601" => 6,
        "smpte240m" => 7,
        "linear" => 8,
        "log100" => 9,
        "log100-sqrt10" => 10,
        "iec61966-2-4" => 11,
        "iec61966-2-1" => 13,
        "bt2020-10" => 14,
        "bt2020-12" => 15,
        "smpte2084" => 16,
        "smpte428" => 17,
        "arib-std-b67" => 18,
        "hlg" => 18,
        _ => 1
    };

    private static int MapSvtAv1Primaries(string value) => value switch
    {
        "bt709" => 1,
        "unspec" => 2,
        "unknown" => 2,
        "bt470m" => 4,
        "bt470bg" => 5,
        "bt601" => 6,
        "smpte240m" => 7,
        "film" => 8,
        "bt2020" => 9,
        "xyz" => 10,
        "smpte431" => 11,
        "smpte432" => 12,
        "ebu3213" => 22,
        _ => 1
    };

    private static string? TryGetFrameRateString(JsonElement stream)
    {
        string? fps = TryGetString(stream, "avg_frame_rate");
        if (IsUsableFrameRate(fps)) return fps;

        fps = TryGetString(stream, "r_frame_rate");
        return IsUsableFrameRate(fps) ? fps : null;
    }

    private static bool IsUsableFrameRate(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Equals("0/0", StringComparison.OrdinalIgnoreCase)
        && !value.Equals("N/A", StringComparison.OrdinalIgnoreCase);

    private static int? TryGetIntegerArg(string args, string name)
    {
        Match match = Regex.Match(args, $@"(?:^|\s){Regex.Escape(name)}\s+(-?\d+)(?=\s|$)");
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : null;
    }

    private static string? NormalizeMetadata(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    #endregion

    public static EncodingMuxCommand? BuildVideoOnlyMuxCommand(EncodingPipelineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FfmpegPath)) return null;

        string encodedVideoPath = ResolveOutputPathWithExtension(request.EncoderExeName, request.OutputPath);
        string outputPath = ResolveMuxOutputPath(request.OutputPath);
        string framerateValue = request.Clip?.FrameRateNumerator is > 0 && request.Clip.FrameRateDenominator is > 0
            ? $"{request.Clip.FrameRateNumerator.Value}/{request.Clip.FrameRateDenominator.Value}"
            : GetMuxFramerateValue(request.SourceFfprobeJson, request.FfmpegFilterArgs);
        string videoTimescaleArgs = GetMuxVideoTrackTimescaleArgs(request.SourceFfprobeJson);
        string? inputFormatArgs = GetMuxInputFormatArgs(request.EncoderExeName, framerateValue);

        string args = JoinArgs(
            "-hide_banner -y",
            inputFormatArgs,
            $"-i {Quote(encodedVideoPath)}",
            "-map 0:v:0 -c:v copy -bsf:v setts=pts=N*DURATION " + videoTimescaleArgs,
            Quote(outputPath));

        return new($"{Quote(request.FfmpegPath)} {args}", args, encodedVideoPath, outputPath);
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

    public static string ResolveMuxOutputPath(string outputPath) =>
        RemoveRawVideoExtension(outputPath) + ".mkv";

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
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return string.Empty;
            string? frameRate = TryGetFrameRateString(stream);
            return FrameRate.TryParseFrameRate(frameRate, out _) ? frameRate! : string.Empty;
        }
        catch { return string.Empty; }
    }

    private static string GetMuxVideoTrackTimescaleArgs(string? sourceFfprobeJson) =>
        $"-video_track_timescale {GetSourceVideoTimescale(sourceFfprobeJson)}";

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

    private static string BuildEncoderOutputArgs(string encoderExeName, string outputPath) =>
        encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => $"-o {Quote(ResolveOutputPathWithExtension(encoderExeName, outputPath))}",
            "x265.exe" => $"-o {Quote(ResolveOutputPathWithExtension(encoderExeName, outputPath))}",
            "svtav1encapp.exe" => $"-b {Quote(ResolveOutputPathWithExtension(encoderExeName, outputPath))}",
            _ => throw new InvalidOperationException($"Unsupported encoder: {encoderExeName}")
        };

    private static string EnsureExtension(string outputPath, string extension) =>
        string.IsNullOrEmpty(extension) || outputPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : outputPath + extension;

    private static string NormalizeRequired(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing {name}.");
        return value.Trim();
    }

    [GeneratedRegex(@"gui_inputs\s*=\s*""((?:[^""\\]|\\.)*)""")]
    private static partial Regex SvfiIniRegex();

    public static (string inputPath, string taskId) ParseSvfiIni(string iniPath)
    {
        if (string.IsNullOrWhiteSpace(iniPath) || !File.Exists(iniPath))
            return (string.Empty, string.Empty);

        string iniContent = File.ReadAllText(iniPath);
        Match match = SvfiIniRegex().Match(iniContent);
        if (!match.Success)
            return (string.Empty, string.Empty);

        string jsonString = match.Groups[1].Value;
        jsonString = jsonString.Replace("\\\"", "\"");
        jsonString = jsonString.Replace("\\\\", "\\");

        using JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement inputs = doc.RootElement.GetProperty("inputs");
        if (inputs.GetArrayLength() == 0)
            return (string.Empty, string.Empty);

        JsonElement firstInput = inputs[0];
        string inputPath = firstInput.GetProperty("input_path").GetString() ?? string.Empty;
        string taskId = firstInput.GetProperty("task_id").GetString() ?? string.Empty;
        return (inputPath, taskId);
    }

    private static string JoinArgs(params string?[] parts) =>
        string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

    private static string Quote(string value) =>
        $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    [GeneratedRegex(@"fps=(\d+/\d+)")]
    private static partial Regex FpsRegex();
    [GeneratedRegex("(?:-filter(?::v)?|-vf)\\s+(?:\"(?<quoted>[^\"]+)\"|'(?<single>[^']+)'|(?<plain>\\S+))", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex FFmpegFilterVScaleRegex();
}
