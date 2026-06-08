using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Helpers;

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
    string? SvfiTaskId = null);

public record EncodingClipRequest(
    string? StartTime = null,
    string? EndTime = null,
    long? FirstFrame = null,
    long? LastFrame = null,
    double? FrameRate = null);

internal record ClipRange(
    string? StartTime,
    string? EndTime,
    long? FirstFrame,
    long? LastFrame);

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

public static partial class EncodingPipelineH
{
    public static EncodingPipelineCommand BuildY4mCommand(EncodingPipelineRequest request)
    {
        string upstreamArgs = BuildUpstreamArgs(request);
        string encoderArgs = BuildEncoderArgs(request);
        string commandLine = $"{Quote(request.UpstreamPath)} {upstreamArgs} | {Quote(request.EncoderPath)} {encoderArgs}";
        return new(commandLine, upstreamArgs, encoderArgs, BuildMuxCommand(request));
    }

    private static EncodingMuxCommand? BuildMuxCommand(EncodingPipelineRequest request)
    {
        if (request.Clip != null) return null;
        if (string.IsNullOrWhiteSpace(request.FfmpegPath) || string.IsNullOrWhiteSpace(request.SourceVideoPath)) return null;

        string encodedVideoPath = ResolveOutputPathWithExtension(request.EncoderExeName, request.OutputPath);
        string outputPath = ResolveMuxOutputPath(request.OutputPath);
        string framerateValue = GetMuxFramerateValue(request.SourceFfprobeJson);
        string streamMapArgs = BuildStreamMapArgs(request.SourceFfprobeJson);
        string args = JoinArgs(
            "-hide_banner -y",
            string.IsNullOrWhiteSpace(framerateValue) ? null : $"-f hevc -framerate {framerateValue}",
            $"-i {Quote(encodedVideoPath)}",
            $"-i {Quote(request.SourceVideoPath)}",
            $"-map 0:v:0 {streamMapArgs} -map_metadata 1 -map_chapters 1 -c:v copy -bsf:v setts=pts=N*DURATION -c:a copy -c:s copy",
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
        catch
        {
            return "-map 1:a? -map 1:s?";
        }
    }

    private static string BuildUpstreamArgs(EncodingPipelineRequest request)
    {
        string input = Quote(request.UpstreamInputPath);
        string clipArgs = BuildUpstreamClipArgs(request.UpstreamExeName, request.Clip);
        return request.UpstreamExeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => JoinArgs($"-hide_banner", clipArgs, $"-i {input}", "-f yuv4mpegpipe -an -strict unofficial -"), // unofficial allows 10bit pipe
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

    private static string BuildFfmpegClipArgs(ClipRange? clip) =>
        clip == null
            ? string.Empty
            : JoinArgs(
                clip.StartTime == null ? null : $"-ss {clip.StartTime}",
                clip.EndTime == null ? null : $"-to {clip.EndTime}");

    private static string BuildVspipeClipArgs(ClipRange? clip)
    {
        if (clip == null) return string.Empty;
        long? firstFrame = clip.FirstFrame ?? (clip.LastFrame.HasValue ? 0 : null);
        return JoinArgs(
            firstFrame == null ? null : $"-s {firstFrame}",
            clip.LastFrame == null ? null : $"-e {clip.LastFrame}");
    }

    private static string BuildAvs2yuvClipArgs(ClipRange? clip)
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

    private static string BuildAvs2pipemodClipArgs(ClipRange? clip)
    {
        if (clip == null || clip.FirstFrame == null && clip.LastFrame == null) return string.Empty;
        long firstFrame = clip.FirstFrame ?? 0;
        if (clip.LastFrame == null) return string.Empty;
        return $"-trim={firstFrame},{clip.LastFrame}";
    }

    #region Sample clip modal stuffs
    private static ClipRange? BuildClipRange(EncodingClipRequest? clip, bool needsTimes, bool needsFrames)
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
        string autoParams = BuildAutoGeneratedEncoderParams(request, encodeParams);
        string parallelismParams = BuildParallelismEncoderParams(request);
        string encoderCustomParams = request.EncoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => request.EncoderConf?.CustomParamsX264 ?? "",
            "x265.exe" => request.EncoderConf?.CustomParamsX265 ?? "",
            "svtav1encapp.exe" => request.EncoderConf?.CustomParamsSvtAv1 ?? "",
            _ => ""
        };
        string customParams = FilterCustomParamsForEncoder(encoderCustomParams, request.EncoderExeName);
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

        int threads = CpuSetsH.ClampThreadCountForNode(
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

    public static long? GetSourceTotalFrames(string? sourceFfprobeJson)
    {
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return null;

            long? frameCount = TryGetLong(stream, "nb_frames")
                ?? TryGetLong(stream, "NUMBER_OF_FRAMES", "tags");

            if (frameCount is > 0) return frameCount;

            double? duration = TryGetDouble(stream, "duration")
                ?? (document.RootElement.TryGetProperty("format", out JsonElement format) ? TryGetDouble(format, "duration") : null);
            string? fpsString = TryGetFrameRateString(stream);
            if (duration is > 0 && TryParseFrameRate(fpsString, out double fps))
            {
                long estimated = (long)Math.Round(duration.Value * fps);
                return estimated > 0 ? estimated : null;
            }

            return null;
        }
        catch
        {
            return null;
        }
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
    private static string BuildAutoGeneratedEncoderParams(EncodingPipelineRequest request, string baseEncoderParams)
    {
        if (string.IsNullOrWhiteSpace(request.SourceFfprobeJson)) return string.Empty;

        using JsonDocument document = JsonDocument.Parse(request.SourceFfprobeJson);
        if (!TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return string.Empty;

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
        _ = (pixelFormat, width, height);

        return encoder switch
        {
            "x264.exe" => JoinArgs(
                GetFrameCount(stream, request.Clip, isSvtAv1: false),
                GetRateControlLookahead(fpsString, TryGetIntegerArg(baseEncoderParams, "--bframes") ?? 0),
                GetColorSpaceSei(stream, isX264: true, isX265: false, isSvtAv1: false),
                GetRangeChromaLocation(stream, isX264: true, isX265: false, isSvtAv1: false)),
            "x265.exe" => JoinArgs(
                GetFrameCount(stream, request.Clip, isSvtAv1: false),
                GetRateControlLookahead(fpsString, TryGetIntegerArg(baseEncoderParams, "--bframes") ?? 0),
                GetX265MeRange(stream),
                GetX265SubmotionEstimation(fpsString),
                GetColorSpaceSei(stream, isX264: false, isX265: true, isSvtAv1: false),
                GetRangeChromaLocation(stream, isX264: false, isX265: true, isSvtAv1: false)),
            "svtav1encapp.exe" => JoinArgs(
                GetFrameCount(stream, request.Clip, isSvtAv1: true),
                GetColorSpaceSei(stream, isX264: false, isX265: false, isSvtAv1: true),
                GetRangeChromaLocation(stream, isX264: false, isX265: false, isSvtAv1: true)),
            _ => string.Empty
        };
    }

    private static string GetRateControlLookahead(string? fpsString, int bframes)
    {
        if (!TryParseFrameRate(fpsString, out double fps)) return string.Empty;
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
        if (!TryParseFrameRate(fpsString, out double fps)) return string.Empty;
        int subme = fps < 25 ? 3 : fps < 49 ? 4 : fps < 61 ? 5 : 6;
        return $"--subme {subme}";
    }

    private static string GetFrameCount(JsonElement stream, EncodingClipRequest? clip, bool isSvtAv1)
    {
        long? frameCount = GetClipFrameCount(clip)
            ?? TryGetLong(stream, "nb_frames")
            ?? TryGetLong(stream, "NUMBER_OF_FRAMES", "tags");

        return frameCount is > 0
            ? isSvtAv1 ? $"-n {frameCount}" : $"--frames {frameCount}"
            : string.Empty;
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
            string matrixArg = colorMatrix is "unknown" or "bt2020nc"
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
            string matrixArg = colorMatrix == "bt2020nc"
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

        int chromaDepth = GetChromaSubsamplingDepth(pixelFormat);
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

    private static int GetChromaSubsamplingDepth(string pixelFormat)
    {
        if (string.IsNullOrWhiteSpace(pixelFormat)) return -2;
        if (pixelFormat.Contains("444", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("rgb", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gbr", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gray", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("400", StringComparison.OrdinalIgnoreCase))
            return 0;
        if (pixelFormat.Contains("420", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("422", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv12", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv16", StringComparison.OrdinalIgnoreCase))
            return 1;
        return -2;
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
        "bt2020-ncl" => 9,
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

    private static bool TryGetFirstVideoStream(JsonElement root, out JsonElement stream)
    {
        stream = default;
        if (!root.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
            return false;

        foreach (JsonElement item in streams.EnumerateArray())
        {
            if (TryGetString(item, "codec_type") is null or "video")
            {
                stream = item;
                return true;
            }
        }

        return false;
    }

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

    private static bool TryParseFrameRate(string? value, out double frameRate)
    {
        frameRate = 0d;
        if (!IsUsableFrameRate(value)) return false;

        string fps = value!.Trim();
        string[] fraction = fps.Split('/');
        if (fraction.Length == 2)
        {
            if (!double.TryParse(fraction[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double numerator)
                || !double.TryParse(fraction[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double denominator)
                || denominator == 0d)
                return false;

            frameRate = numerator / denominator;
            return frameRate > 0d;
        }

        return double.TryParse(fps, NumberStyles.Float, CultureInfo.InvariantCulture, out frameRate)
            && frameRate > 0d;
    }

    private static double? TryGetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out double value)) return value;
        return double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            ? value
            : null;
    }

    private static int? TryGetIntegerArg(string args, string name)
    {
        Match match = Regex.Match(args, $@"(?:^|\s){Regex.Escape(name)}\s+(-?\d+)(?=\s|$)");
        return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return false;
        if (property.ValueKind == JsonValueKind.Number) return property.TryGetInt32(out value);
        return property.ValueKind == JsonValueKind.String
            && int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static long? TryGetLong(JsonElement element, string propertyName, string? parentPropertyName = null)
    {
        JsonElement container = element;
        if (parentPropertyName != null
            && (!element.TryGetProperty(parentPropertyName, out container) || container.ValueKind != JsonValueKind.Object))
            return null;

        if (!container.TryGetProperty(propertyName, out JsonElement property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out long number)) return number;
        if (property.ValueKind != JsonValueKind.String) return null;

        string? text = property.GetString();
        return !string.IsNullOrWhiteSpace(text)
            && !text.Equals("N/A", StringComparison.OrdinalIgnoreCase)
            && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
                ? parsed
                : null;
    }

    private static string? NormalizeMetadata(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    #endregion

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
        Path.ChangeExtension(RemoveRawVideoExtension(outputPath), ".mkv");

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

    private static string GetMuxFramerateValue(string? sourceFfprobeJson)
    {
        if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return string.Empty;

        try
        {
            using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
            if (!TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return string.Empty;
            string? frameRate = TryGetFrameRateString(stream);
            return TestFrameRateValid(frameRate) ? frameRate! : string.Empty;
        }
        catch { return string.Empty; }
    }

    private static bool TestFrameRateValid(string? frameRate)
    {
        if (string.IsNullOrWhiteSpace(frameRate)) return false;
        string value = frameRate.Trim();
        if (value.Equals("0", StringComparison.OrdinalIgnoreCase) || value.Equals("0/0", StringComparison.OrdinalIgnoreCase)) return false;
        if (value.Contains('/', StringComparison.Ordinal))
        {
            string[] parts = value.Split('/');
            return parts.Length == 2 &&
                   long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long numerator) &&
                   long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out long denominator) &&
                   numerator > 0 && denominator > 0;
        }

        return double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parsed) && parsed > 0;
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
        string.IsNullOrWhiteSpace(Path.GetExtension(outputPath))
            ? outputPath + extension
            : outputPath;

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
}
