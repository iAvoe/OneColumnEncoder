using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OneColumnEncoder.Helpers;

public record EncodingPipelineRequest(
    string UpstreamExeName,
    string UpstreamPath,
    string UpstreamInputPath,
    string EncoderExeName,
    string EncoderPath,
    string OutputPath,
    EncoderConfM EncoderConf,
    string? VspipeY4mArg,
    EncodingClipRequest? Clip = null);

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
    string EncoderArgs);

public static class EncodingPipelineH
{
    public static EncodingPipelineCommand BuildY4mCommand(EncodingPipelineRequest request)
    {
        string upstreamArgs = BuildUpstreamArgs(request);
        string encoderArgs = BuildEncoderArgs(request);
        string commandLine = $"{Quote(request.UpstreamPath)} {upstreamArgs} | {Quote(request.EncoderPath)} {encoderArgs}";
        return new(commandLine, upstreamArgs, encoderArgs);
    }

    private static string BuildUpstreamArgs(EncodingPipelineRequest request)
    {
        string input = Quote(request.UpstreamInputPath);
        string clipArgs = BuildUpstreamClipArgs(request.UpstreamExeName, request.Clip);
        return request.UpstreamExeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => JoinArgs(clipArgs, $"-i {input}", "-f yuv4mpegpipe -an -strict unofficial -"), // unofficial allows 10bit pipe
            "vspipe.exe" => JoinArgs(input, clipArgs, NormalizeRequired(request.VspipeY4mArg, "vspipe Y4M argument"), "-"),
            "avs2yuv.exe" => JoinArgs(input, clipArgs, "-"),
            "avs2pipemod.exe" => JoinArgs(input, clipArgs, "-y4mp"),
            "one_line_shot_args.exe" => $"{input} --pipe-out",
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
        string outputArgs = BuildEncoderOutputArgs(request.EncoderExeName, request.OutputPath);
        return JoinArgs(y4mInputArgs, encodeParams, outputArgs);
    }

    private static string GetEncoderY4mInputArgs(string encoderExeName) =>
        encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => "--demuxer y4m -",
            "x265.exe" => "--y4m -",
            "svtav1encapp.exe" => "-i stdin",
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

    private static string BuildEncoderOutputArgs(string encoderExeName, string outputPath) =>
        encoderExeName.ToLowerInvariant() switch
        {
            "x264.exe" => $"-o {Quote(EnsureExtension(outputPath, ".mp4"))}",
            "x265.exe" => $"-o {Quote(EnsureExtension(outputPath, ".hevc"))}",
            "svtav1encapp.exe" => $"-b {Quote(EnsureExtension(outputPath, ".ivf"))}",
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

    private static string JoinArgs(params string?[] parts) =>
        string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

    private static string Quote(string value) =>
        $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
