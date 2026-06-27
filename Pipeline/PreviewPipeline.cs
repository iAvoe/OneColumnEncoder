using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneColumnEncoder.Pipeline;

public enum PreviewEncoder { X264, X265, SvtAv1, Vvenc }

public enum PreviewDisplayMode { Raw, LowToBt709, WcgToBt709, HdrToSdr, HighHdrToSdr }

public static partial class PreviewPipeline
{
    public static string[] BuildSourceArgs(string sourceVideoPath, int previewPositionSeconds, string outputPath, string? displayFilter = null)
    {
        List<string> args =
        [
            "-hide_banner",
            "-y",
            "-strict",
            "unofficial",
            "-ss",
            EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(previewPositionSeconds)),
            "-i",
            sourceVideoPath
        ];

        if (!string.IsNullOrWhiteSpace(displayFilter))
            args.AddRange(["-vf", displayFilter]);

        args.AddRange(["-vframes", "1", "-c:v", "png", outputPath]);
        return [.. args];
    }

    private static string[] BuildVvencEncodeArgs(EncoderConfM model, string sourcePath, string outputPath)
    {
        return
        [
            "-hide_banner",
            "-y",
            "-strict",
            "unofficial",
            "-i",
            sourcePath,
            "-vf",
            "format=yuv420p10le",
            "-c:v",
            "libvvenc",
            "-preset",
            GetVvencPresetName(model.VvencMode),
            "-qp",
            Math.Clamp(model.VvencQp, 0, 63).ToString(CultureInfo.InvariantCulture),
            "-vvenc-params",
            "qpa=1:gopsize=1:intraperiod=1:refreshtype=idr:tier=high",
            "-frames:v",
            "1",
            "-f",
            "vvc",
            outputPath
        ];
    }

    public static string GetVvencPresetName(int presetKey) => presetKey switch
    {
        0 => "medium",
        1 => "slower",
        2 => "slow",
        _ => "medium"
    };

    public static string[] BuildEncodeArgs(PreviewEncoder encoder, EncoderConfM model, string sourcePath, string outputPath)
    {
        if (encoder == PreviewEncoder.Vvenc)
            return BuildVvencEncodeArgs(model, sourcePath, outputPath);

        List<string> args =
        [
            "-hide_banner",
            "-y",
            "-strict",
            "unofficial",
            "-i",
            sourcePath,
            "-c:v",
            GetFfmpegEncoderName(encoder),
            "-crf",
            GetCrfValue(encoder, model).ToString(CultureInfo.InvariantCulture)
        ];

        args.AddRange(SplitArgs(GetCustomParams(encoder, model)));
        args.AddRange(["-frames:v", "1"]);

        if (encoder == PreviewEncoder.X264)
            args.AddRange(["-f", "h264"]);
        else if (encoder == PreviewEncoder.X265)
            args.AddRange(["-f", "hevc"]);

        args.Add(outputPath);
        return [.. args];
    }

    public static string[] BuildDecodeArgs(string inputPath, string outputPath)
    {
        List<string> args =
        [
            "-hide_banner",
            "-y",
            "-strict",
            "unofficial",
            "-i",
            inputPath,
            "-frames:v",
            "1",
            "-c:v",
            "png",
            outputPath
        ];
        return [.. args];
    }

    public static string? BuildDisplayFilter(PreviewDisplayMode displayMode, ColorSpaceAnalysisM colorSpaceAnalysis)
    {
        ColorSpaceStrategy? strategy = displayMode switch
        {
            PreviewDisplayMode.LowToBt709 => ColorSpaceStrategy.LowToHigh,
            PreviewDisplayMode.WcgToBt709 => ColorSpaceStrategy.HighToLow,
            PreviewDisplayMode.HdrToSdr => ColorSpaceStrategy.HdrToSdr,
            PreviewDisplayMode.HighHdrToSdr => ColorSpaceStrategy.HighHdrToSdr,
            _ => null
        };
        if (strategy == null) return null;

        string? filter = ColorSpaceConverter.BuildFfmpegFilter(
            strategy.Value,
            colorSpaceAnalysis.ColorMatrix,
            colorSpaceAnalysis.ColorChromaLocation,
            colorSpaceAnalysis.ColorPrimaries,
            colorSpaceAnalysis.PixelFormat);
        if (string.IsNullOrWhiteSpace(filter)) return null;

        filter = filter.Replace("<nits>", "1000", StringComparison.Ordinal);
        if (strategy == ColorSpaceStrategy.HdrToSdr)
            filter = string.Join(',', filter, "zscale=matrix=bt709:primaries=bt709:transfer=bt709");
        return string.Join(',', filter, "format=rgb24");
    }

    public static string GetDisplayModeFileSuffix(PreviewDisplayMode displayMode) => displayMode switch
    {
        PreviewDisplayMode.LowToBt709 => "low709",
        PreviewDisplayMode.WcgToBt709 => "wcg709",
        PreviewDisplayMode.HdrToSdr => "hdrsdr",
        PreviewDisplayMode.HighHdrToSdr => "highhdrsdr",
        _ => "raw"
    };

    public static string GetDisplayModeTitle(PreviewDisplayMode displayMode, string raw, string lowToBt709, string wcgToBt709, string hdrToSdr, string highHdrToSdr) => displayMode switch
    {
        PreviewDisplayMode.LowToBt709 => lowToBt709,
        PreviewDisplayMode.WcgToBt709 => wcgToBt709,
        PreviewDisplayMode.HdrToSdr => hdrToSdr,
        PreviewDisplayMode.HighHdrToSdr => highHdrToSdr,
        _ => raw
    };

    public static string GetFfmpegEncoderName(PreviewEncoder encoder) => encoder switch
    {
        PreviewEncoder.X264 => "libx264",
        PreviewEncoder.X265 => "libx265",
        PreviewEncoder.Vvenc => "libvvenc",
        _ => "libsvtav1"
    };

    public static string GetEncoderTitle(PreviewEncoder encoder) => encoder switch
    {
        PreviewEncoder.X264 => "libx264",
        PreviewEncoder.X265 => "libx265",
        PreviewEncoder.Vvenc => "libvvenc",
        _ => "libsvtav1"
    };

    public static int GetCrfValue(PreviewEncoder encoder, EncoderConfM model) => encoder switch
    {
        PreviewEncoder.X264 => model.X264Crf,
        PreviewEncoder.X265 => model.X265Crf,
        PreviewEncoder.Vvenc => model.VvencQp,
        _ => model.SvtAv1Crf
    };

    public static string GetCustomParams(PreviewEncoder encoder, EncoderConfM model) => encoder switch
    {
        PreviewEncoder.X264 => model.CustomParamsX264,
        PreviewEncoder.X265 => model.CustomParamsX265,
        PreviewEncoder.Vvenc => "",
        _ => model.CustomParamsSvtAv1
    };

    public static IEnumerable<string> SplitArgs(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) yield break;

        StringBuilder current = new();
        bool inQuotes = false;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    yield return current.ToString();
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
            yield return current.ToString();
    }

    public static bool IsSource12Bit(ColorSpaceAnalysisM colorSpaceAnalysis) =>
        colorSpaceAnalysis.PixelFormat?.Contains("12le", StringComparison.OrdinalIgnoreCase) == true;

    public static string TrimProcessMessage(string message)
    {
        string text = string.IsNullOrWhiteSpace(message) ? "ffmpeg failed." : message.Trim();
        text = text.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
        while (text.Contains("  ", StringComparison.Ordinal))
            text = text.Replace("  ", " ", StringComparison.Ordinal);
        return text.Length <= 700 ? text : text[^700..];
    }

    public static async Task RunFfmpegAsync(string ffmpegPath, string workDirectory, IReadOnlyList<string> args, CancellationToken token)
    {
        ProcessStartInfo psi = new()
        {
            FileName = ffmpegPath,
            WorkingDirectory = workDirectory,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        foreach (string arg in args)
            psi.ArgumentList.Add(arg);

        using Process process = new() { StartInfo = psi, EnableRaisingEvents = true };
        process.Start();
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(token);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(token);

        try
        {
            await process.WaitForExitAsync(token);
        }
        catch (OperationCanceledException)
        {
            TryKillProcess(process);
            throw;
        }

        string stdout = await stdoutTask;
        string stderr = await stderrTask;
        if (process.ExitCode != 0)
        {
            string message = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            throw new InvalidOperationException(TrimProcessMessage(message));
        }
    }

    public static void TryKillProcess(Process process)
    {
        try { if (!process.HasExited) process.Kill(true); }
        catch { }
    }

    public static BitmapImage LoadBitmap(string path)
    {
        BitmapImage bitmap = new();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
