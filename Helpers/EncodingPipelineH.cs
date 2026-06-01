using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
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
    string? VspipeY4mArg);

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
        return request.UpstreamExeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => $"-i {input} -f yuv4mpegpipe -an -strict unofficial -",
            "vspipe.exe" => $"{input} {NormalizeRequired(request.VspipeY4mArg, "vspipe Y4M argument")} -",
            "avs2yuv.exe" => $"{input} -",
            "avs2pipemod.exe" => $"{input} -y4mp",
            "one_line_shot_args.exe" => $"{input} --pipe-out",
            _ => throw new InvalidOperationException($"Unsupported upstream tool: {request.UpstreamExeName}")
        };
    }

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
