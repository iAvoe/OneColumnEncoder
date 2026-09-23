using OneColumnEncoder.Models.Analysis;
using OneColumnEncoder.Models.Encoding;
using System.IO;
using System.Windows.Media.Imaging;

namespace OneColumnEncoder.Pipeline;

public enum PreviewEncoder { X264, X265, SvtAv1, Vvenc }

public enum PreviewDisplayMode { Raw, LowToBt709, WcgToBt709, HdrToSdr, HighHdrToSdr }

public static partial class PreviewPipeline
{
    public static string CreateWorkDirectory(string namePrefix)
    {
        string directory = Path.Combine(Path.GetTempPath(), namePrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    public static void DeleteDirectoryQuietly(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch { }
    }

    public static void EnsureFileExists(string path, string message)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(message, path);
    }

    public static string[] BuildSourceArgs(string sourceVideoPath, int previewPositionSeconds, string outputPath, string? displayFilter = null) =>
        BuildSourceArgs(sourceVideoPath, TimeSpan.FromSeconds(previewPositionSeconds), outputPath, displayFilter);

    public static string[] BuildFFmpegBitmapPipeArgs(
        string srcPath,
        TimeSpan previewPosition,
        string? videoFilter = null)
    {
        List<string> args =
        [
            "-hide_banner",
            "-loglevel", "error",
            "-nostdin",
            "-y",
            "-ss", EncodingPipeline.FormatTimestamp(previewPosition),
            "-i", srcPath
        ];

        if (!string.IsNullOrWhiteSpace(videoFilter))
            args.AddRange(["-vf", videoFilter]);

        args.AddRange(
        [
            "-frames:v", "1",
            "-f", "image2pipe",
            "-c:v", "bmp",
            "pipe:1"
        ]);
        return [.. args];
    }

    public static bool TryExtractVideoFilter(string? ffmpegArgs, out string filter)
    {
        filter = string.Empty;
        if (string.IsNullOrWhiteSpace(ffmpegArgs)) return false;

        string[] options = ["-filter:v", "-vf", "-filter_complex"];
        int optionIndex = -1;
        string? matchedOption = null;
        foreach (string option in options)
        {
            int candidate = ffmpegArgs.IndexOf(option, StringComparison.OrdinalIgnoreCase);
            if (candidate >= 0 && (optionIndex < 0 || candidate < optionIndex))
            {
                optionIndex = candidate;
                matchedOption = option;
            }
        }

        if (optionIndex < 0)
        {
            if (ffmpegArgs.TrimStart().StartsWith('-')) return false;
            filter = ffmpegArgs.Trim();
            return filter.Length > 0;
        }

        int contentStart = optionIndex + matchedOption!.Length;
        while (contentStart < ffmpegArgs.Length && char.IsWhiteSpace(ffmpegArgs[contentStart])) contentStart++;
        if (contentStart >= ffmpegArgs.Length) return false;

        char quote = ffmpegArgs[contentStart] is '"' or '\'' ? ffmpegArgs[contentStart++] : '\0';
        int contentEnd = contentStart;
        if (quote == '\0')
        {
            while (contentEnd < ffmpegArgs.Length && !char.IsWhiteSpace(ffmpegArgs[contentEnd])) contentEnd++;
        }
        else
        {
            bool escaped = false;
            for (; contentEnd < ffmpegArgs.Length; contentEnd++)
            {
                char current = ffmpegArgs[contentEnd];
                if (current == quote && !escaped) break;
                escaped = current == '\\' && !escaped;
                if (current != '\\') escaped = false;
            }
        }

        filter = ffmpegArgs[contentStart..contentEnd].Trim();
        return filter.Length > 0;
    }

    public static string[] BuildSourceArgs(string sourceVideoPath, TimeSpan previewPosition, string outputPath, string? displayFilter = null)
    {
        List<string> args =
        [
            "-hide_banner",
            "-y",
            "-strict", "unofficial",
            "-ss", EncodingPipeline.FormatTimestamp(previewPosition),
            "-i", sourceVideoPath
        ];

        if (!string.IsNullOrWhiteSpace(displayFilter))
            args.AddRange(["-vf", displayFilter]);

        args.AddRange(["-vframes", "1", "-c:v", "png", outputPath]);
        return [.. args];
    }

    public static string[] BuildSourceFrameArgs(
        string sourceVideoPath,
        long firstFrame,
        long lastFrame,
        string outputPattern,
        int targetHeight = 480,
        string scaleFlags = "lanczos")
    {
        long safeFirstFrame = Math.Max(0, firstFrame);
        long safeLastFrame = Math.Max(safeFirstFrame, lastFrame);
        return
        [
            "-hide_banner",
            "-y",
            "-strict", "unofficial",
            "-i", sourceVideoPath,
            "-vf",
            $"select=between(n\\,{safeFirstFrame}\\,{safeLastFrame}),scale=-2:{Math.Max(1, targetHeight)}:flags={scaleFlags}",
            "-fps_mode", "passthrough",
            "-start_number", "0",
            "-frames:v",
            (safeLastFrame - safeFirstFrame + 1).ToString(CultureInfo.InvariantCulture),
            "-c:v", "png",
            outputPattern
        ];
    }

    public static string[] BuildSourceFrameSeekArgs(
        string srcPath,
        double keyframeTime,
        long firstOffsetFrame,
        long lastOffsetFrame,
        string outputPattern,
        int targetHeight = 480,
        string scaleFlags = "lanczos")
    {
        long safeFirstOffset = Math.Max(0, firstOffsetFrame);
        long safeLastOffset = Math.Max(safeFirstOffset, lastOffsetFrame);
        long frameCount = safeLastOffset - safeFirstOffset + 1;
        string keyframeTimestamp = FormatSeekSeconds(keyframeTime);
        return
        [
            "-hide_banner",
            "-y",
            "-strict", "unofficial",
            "-ss", keyframeTimestamp,
            "-seek_timestamp", "1",
            "-i", srcPath,
            "-vf",
            $"select=between(n\\,{safeFirstOffset}\\,{safeLastOffset}),scale=-2:{Math.Max(1, targetHeight)}:flags={scaleFlags}",
            "-fps_mode", "passthrough",
            "-start_number", "0",
            "-frames:v", frameCount.ToString(CultureInfo.InvariantCulture),
            "-c:v", "png",
            outputPattern
        ];
    }

    private static string FormatSeekSeconds(double seconds) =>
        Math.Max(0d, seconds).ToString("0.######", CultureInfo.InvariantCulture);

    private static string[] BuildVvencEncodeArgs(EncoderConfM model, string srcPath, string outputPath)
    {
        return
        [
            "-hide_banner",
            "-y",
            "-strict", "unofficial",
            "-i", srcPath,
            "-vf", "format=yuv420p10le",
            "-c:v", "libvvenc",
            "-preset", GetVvencPresetName(model.VvencMode),
            "-qp", Math.Clamp(model.VvencQp, 0, 63).ToString(CultureInfo.InvariantCulture),
            "-vvenc-params", "qpa=1:gopsize=1:intraperiod=1:refreshtype=idr:tier=high",
            "-frames:v", "1",
            "-f", "vvc",
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

    public static string[] BuildEncodeArgs(PreviewEncoder encoder, EncoderConfM model, string srcPath, string outputPath)
    {
        if (encoder == PreviewEncoder.Vvenc)
            return BuildVvencEncodeArgs(model, srcPath, outputPath);

        List<string> args =
        [
            "-hide_banner",
            "-y",
            "-strict", "unofficial",
            "-i", srcPath,
            "-c:v", GetFFmpegEncoderName(encoder),
            "-crf", GetCrfValue(encoder, model).ToString(CultureInfo.InvariantCulture)
        ];

        args.AddRange(SplitArgs(GetCustomParams(encoder, model)));
        if (encoder == PreviewEncoder.X265 && model.X265Mcstf)
            args.AddRange(["-x265-params", ":selective-mcstf=1:mcstf-ref-range=1"]);
        args.AddRange(["-frames:v", "1"]);

        if (encoder == PreviewEncoder.X264) args.AddRange(["-f", "h264"]);
        else if (encoder == PreviewEncoder.X265) args.AddRange(["-f", "hevc"]);

        args.Add(outputPath);
        return [.. args];
    }

    public static string[] BuildDecodeArgs(string inputPath, string outputPath)
    {
        List<string> args =
        [
            "-hide_banner",
            "-y",
            "-strict", "unofficial",
            "-i", inputPath,
            "-frames:v", "1",
            "-c:v", "png",
            outputPath
        ];
        return [.. args];
    }

    public static string[] BuildVspipeY4mArgs(
        string scriptPath,
        int outputIndex,
        int frame,
        string vspipeY4mArg,
        string outputY4mPath)
    {
        List<string> args =
        [
            scriptPath,
            "-o", outputIndex.ToString(CultureInfo.InvariantCulture),
            "-s", frame.ToString(CultureInfo.InvariantCulture),
            "-e", frame.ToString(CultureInfo.InvariantCulture)
        ];

        args.AddRange(SplitArgs(vspipeY4mArg));
        args.Add(outputY4mPath);
        return [.. args];
    }

    public static string[] BuildAvs2yuvY4mArgs(string scriptPath, int frame) =>
    [
        scriptPath,
        "-seek", Math.Max(0, frame).ToString(CultureInfo.InvariantCulture),
        "-frames", "1",
        "-"
    ];

    public static string[] BuildAvs2pipemodY4mArgs(string scriptPath, int frame) =>
    [
        scriptPath,
        $"-trim={Math.Max(0, frame)},{Math.Max(0, frame)}",
        "-y4mp"
    ];

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

        string? filter = ColorSpaceConverter.BuildFFmpegFilter(
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

    /// <summary>
    /// Maps a MainVM selected video encoder exe name to its preview equivalent.
    /// Returns null for unknown/null so callers fall back to libx264.
    /// </summary>
    public static PreviewEncoder? ResolvePreviewEncoder(string? encoderExeName) => encoderExeName switch
    {
        not null when encoderExeName.Equals("x264.exe", StringComparison.OrdinalIgnoreCase) => PreviewEncoder.X264,
        not null when encoderExeName.Equals("x265.exe", StringComparison.OrdinalIgnoreCase) => PreviewEncoder.X265,
        not null when encoderExeName.Equals("svtav1encapp.exe", StringComparison.OrdinalIgnoreCase) => PreviewEncoder.SvtAv1,
        _ => null,
    };

    public static string GetFFmpegEncoderName(PreviewEncoder encoder) => encoder switch
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

    public static async Task RunFFmpegAsync(string ffmpegPath, string workDirectory, IReadOnlyList<string> args, CancellationToken token)
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
        using CancellationTokenRegistration killRegistration = token.Register(() => TryKillProcess(process));
        try
        {
            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(token);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(token);

            await process.WaitForExitAsync(token);

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                string message = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                throw new InvalidOperationException(TrimProcessMessage(message));
            }
        }
        catch
        {
            TryKillProcess(process);
            throw;
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

    public static BitmapImage LoadBitmap(Stream stream)
    {
        if (stream.CanSeek) stream.Position = 0;

        BitmapImage bitmap = new();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        // Do NOT set IgnoreImageCache here: it requires UriSource to be set.
        // With StreamSource only, EndInit() throws "Value cannot be null (Parameter 'key')"
        // from ImagingCache.RemoveFromCache(null).
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
