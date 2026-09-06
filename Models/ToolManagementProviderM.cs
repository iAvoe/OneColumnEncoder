using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Models;

public class ToolVersionDetectTimeoutException(string exeName)
    : TimeoutException($"Tool version detection timed out for {exeName}.")
{
}

public static class ToolManagementProviderM
{
    private static readonly TimeSpan VersionDetectTimeout = TimeSpan.FromSeconds(3);

    #region ToolCompatibility methods

    public static void RefreshDependencySelectionState(
        IEnumerable<ToolItemCardVM> upstreamsZone,
        IEnumerable<ToolItemCardVM> dependenciesZone,
        Action updateEncodingStartButtons)
    {
        ToolItemCardVM? avs2pipemod = upstreamsZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedToolByKey(t.DefinitionKey, "avs2pipemod.exe"));
        ToolItemCardVM? avisynth = dependenciesZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedToolByKey(t.DefinitionKey, "avisynth.dll"));

        bool avsSelected = avs2pipemod?.IsSelected ?? false;
        bool aviSelected = avisynth?.IsSelected ?? false;
        bool bothSelectedOrNeither = avsSelected == aviSelected;

        if (avs2pipemod != null)
            avs2pipemod.IsCancel = avsSelected && !bothSelectedOrNeither;

        if (avisynth != null)
            avisynth.IsCancel = aviSelected && !bothSelectedOrNeither;

        foreach (ToolItemCardVM upstream in upstreamsZone.Where(t => !ToolDefinitionProviderM.IsImportedToolByKey(t.DefinitionKey, "avs2pipemod.exe") && t.IsCancel))
        {
            upstream.IsCancel = false;
        }

        updateEncodingStartButtons();
    }

    public static void RefreshSrcSelectState(
        IEnumerable<ToolItemCardVM> upstreamsZone,
        IEnumerable<ToolItemCardVM> scriptSrcImportZone,
        Action refreshSelectedSourceStatus)
    {
        ToolItemCardVM? upstream = upstreamsZone.FirstOrDefault(t => t.IsSelected);

        string? allowedName = null;
        bool allDisabled = false;

        switch (upstream)
        {
            case null:
                break;
            case var u when ToolDefinitionProviderM.IsImportedToolByKey(u.DefinitionKey, "ffmpeg.exe"):
                allDisabled = true;
                break;
            case var u when ToolDefinitionProviderM.IsImportedToolByKey(u.DefinitionKey, "vspipe.exe"):
                allowedName = ResolveScriptSourceName(scriptSrcImportZone, "Tool.Source.VapourSynth", "Tool.Source.VapourSynthQueue");
                break;
            case var u when ToolDefinitionProviderM.IsImportedToolByKey(u.DefinitionKey, "avs2yuv.exe")
                       || ToolDefinitionProviderM.IsImportedToolByKey(u.DefinitionKey, "avs2pipemod.exe"):
                allowedName = ResolveScriptSourceName(scriptSrcImportZone, "Tool.Source.AviSynth", "Tool.Source.AviSynthQueue");
                break;
            case var u when ToolDefinitionProviderM.IsImportedToolByKey(u.DefinitionKey, "one_line_shot_args.exe"):
                allowedName = ResolveScriptSourceName(scriptSrcImportZone, "Tool.Source.Svfi", "Tool.Source.SvfiQueue");
                if (allowedName == null) allDisabled = true;
                break;
        }

        foreach (ToolItemCardVM item in scriptSrcImportZone)
        {
            bool shouldEnable = allDisabled switch
            {
                true => false,
                _ when allowedName == null => true,
                _ => item.DefinitionKey == allowedName
            };

            if (!shouldEnable) item.IsSelected = false;
            item.IsEnabled = shouldEnable;
        }

        refreshSelectedSourceStatus();
    }

    public static void RefreshVideoSrcSelectState(
        IEnumerable<ToolItemCardVM> upstreamsZone,
        IList<ToolItemCardVM> videoSrcImportZone,
        bool hasFfprobe)
    {
        if (videoSrcImportZone.Count < 3) return;

        ToolItemCardVM singleVideoCard = videoSrcImportZone[0];
        ToolItemCardVM queueCard = videoSrcImportZone[1];
        ToolItemCardVM concatCard = videoSrcImportZone[2];
        ToolItemCardVM? repartCard = videoSrcImportZone.Count > 3 ? videoSrcImportZone[3] : null;

        if (!hasFfprobe)
        {
            foreach (ToolItemCardVM item in videoSrcImportZone)
            {
                item.IsSelected = false;
                item.IsEnabled = false;
                item.IsCancel = false;
            }
            return;
        }

        foreach (ToolItemCardVM item in videoSrcImportZone)
            item.IsCancel = false;

        ToolItemCardVM? upstream = upstreamsZone.FirstOrDefault(t => t.IsSelected);

        singleVideoCard.IsEnabled = true;

        bool oneLineShotMode = upstream != null && (
            ToolDefinitionProviderM.IsImportedToolByKey(upstream.DefinitionKey, "one_line_shot_args.exe"));

        if (oneLineShotMode)
        {
            queueCard.IsSelected = false;
            queueCard.IsEnabled = false;
            concatCard.IsSelected = false;
            concatCard.IsEnabled = false;
            if (repartCard != null)
            {
                repartCard.IsSelected = false;
                repartCard.IsEnabled = false;
            }
        }
        else
        {
            queueCard.IsEnabled = true;
            concatCard.IsEnabled = true;
            if (repartCard != null) repartCard.IsEnabled = true;
        }
    }

    private static string? ResolveScriptSourceName(
        IEnumerable<ToolItemCardVM> scriptSrcImportZone,
        string primaryKey,
        string queueKey)
    {
        bool hasPrimary = scriptSrcImportZone.Any(t => t.DefinitionKey == primaryKey);
        if (hasPrimary)
            return primaryKey;
        bool hasQueue = scriptSrcImportZone.Any(t => t.DefinitionKey == queueKey);
        if (hasQueue)
            return queueKey;

        return null;
    }

    #endregion

    #region ToolVersionDetect methods

    private static string RemoveToolNamePrefix(string version, string toolName)
    {
        string prefix = toolName + " ";
        return version.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? version[prefix.Length..]
            : version;
    }

    public static async Task<string?> TryDetectAsync(string exeName, string filePath)
    {
        if (string.IsNullOrWhiteSpace(exeName)
            || string.IsNullOrWhiteSpace(filePath)
            || !File.Exists(filePath)) return null;

        if (exeName.Equals("avisynth.dll", StringComparison.OrdinalIgnoreCase)
            || exeName.Equals("one_line_shot_args.exe", StringComparison.OrdinalIgnoreCase))
            return TryReadProductVersion(filePath);

        string exeArgs = exeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => "-version",
            "ffprobe.exe" => "-version",
            "vspipe.exe" => "-v",
            "x264.exe" => "-V",
            "x265.exe" => "-V",
            "svtav1encapp.exe" => "--version",
            "avs2yuv.exe" => "",
            "avs2pipemod.exe" => "",
            _ => "",
        };

        Stopwatch stopwatch = Stopwatch.StartNew();
        string exePrints = await RunAndCaptureAsync(filePath, exeArgs, GetRemainingTimeout(stopwatch), outputEncoding: GetSystemTextEncoding());
        string? version = ParseVersion(exeName, exePrints);
        if (version != null) return version;

        exePrints = await RunAndCaptureAsync(filePath, exeArgs, GetRemainingTimeout(stopwatch), useUtf8: true);
        return ParseVersion(exeName, exePrints);
    }

    public static string? TryReadProductVersion(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        try
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

            if (!string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
                return versionInfo.ProductVersion.Trim();

            if (!string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                return versionInfo.FileVersion.Trim();
        }
        catch { }
        return null;
    }

    public static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, bool useUtf8 = false, System.Text.Encoding? outputEncoding = null)
    {
        return await RunAndCaptureAsync(filePath, exeArgs, TimeSpan.FromSeconds(5), false, useUtf8, outputEncoding);
    }

    public static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, TimeSpan timeout, bool useUtf8 = false, System.Text.Encoding? outputEncoding = null)
    {
        return await RunAndCaptureAsync(filePath, exeArgs, timeout, true, useUtf8, outputEncoding);
    }

    private static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, TimeSpan timeout, bool throwOnTimeout, bool useUtf8 = false, System.Text.Encoding? outputEncoding = null)
    {
        ProcessStartInfo psi = new()
        {
            FileName = filePath,
            Arguments = exeArgs,
            WorkingDirectory = Path.GetDirectoryName(filePath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (outputEncoding != null)
        {
            psi.StandardOutputEncoding = outputEncoding;
            psi.StandardErrorEncoding = outputEncoding;
        }
        else if (useUtf8)
        {
            psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
            psi.StandardErrorEncoding = System.Text.Encoding.UTF8;
        }

        using Process process = new() { StartInfo = psi };
        process.Start();
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        using CancellationTokenSource cts = new(timeout);
        try { await process.WaitForExitAsync(cts.Token); }
        catch (OperationCanceledException)
        {
            try { if (!process.HasExited) process.Kill(true); }
            catch { }
            if (throwOnTimeout)
                throw new ToolVersionDetectTimeoutException(Path.GetFileName(filePath));
        }

        string stdout = await stdoutTask;
        string stderr = await stderrTask;
        return string.Join(
            Environment.NewLine,
            new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static System.Text.Encoding? GetSystemTextEncoding()
    {
        try { return Console.OutputEncoding; }
        catch { return null; }
    }

    private static TimeSpan GetRemainingTimeout(Stopwatch stopwatch)
    {
        TimeSpan remaining = VersionDetectTimeout - stopwatch.Elapsed;
        if (remaining <= TimeSpan.Zero) throw new ToolVersionDetectTimeoutException("tool");
        return remaining;
    }

    public static string? ParseVersion(string exeName, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        string[] lines = text
            .Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries
                       | StringSplitOptions.TrimEntries);
        string firstLine = lines.FirstOrDefault() ?? text.Trim();

        switch (exeName.ToLowerInvariant())
        {
            case "ffmpeg.exe":
                return firstLine.StartsWith("ffmpeg version", StringComparison.OrdinalIgnoreCase)
                    ? RemoveToolNamePrefix(firstLine[..Math.Min(25, firstLine.Length)], "ffmpeg")
                    : null;
            case "ffprobe.exe":
                return firstLine.StartsWith("ffprobe version", StringComparison.OrdinalIgnoreCase)
                    ? RemoveToolNamePrefix(firstLine[..Math.Min(26, firstLine.Length)], "ffprobe")
                    : null;
            case "vspipe.exe":
                return lines.FirstOrDefault(l =>
                    l.Contains("Core R", StringComparison.OrdinalIgnoreCase));
            case "avs2yuv.exe":
                return text.Contains("avs2yuv", StringComparison.OrdinalIgnoreCase) ? firstLine : null;
            case "avs2pipemod.exe":
                {
                    if (!text.Contains("avs2pipemod", StringComparison.OrdinalIgnoreCase)) return null;
                    Match m = RegexProviderM.Avs2pipemodVersionRegex().Match(firstLine);
                    return m.Success ? m.Value : firstLine;
                }

            case "x264.exe":
                return text.Contains("x264", StringComparison.OrdinalIgnoreCase) ? firstLine : null;
            case "x265.exe":
                {
                    if (!text.Contains("x265", StringComparison.OrdinalIgnoreCase)) return null;
                    Match m = RegexProviderM.X265VersionRegex().Match(text);
                    return m.Success ? m.Groups[1].Value : firstLine;
                }
            case "svtav1encapp.exe":
                return text.Contains("svt", StringComparison.OrdinalIgnoreCase) ? firstLine : null;

            default:
                return null;
        }
    }

    public static async Task<string?> DetectVspipeY4mArgAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        string[][] testArgs =
        [
            ["-c", "y4m"],
            ["--container", "y4m"],
            ["--y4m"]
        ];

        foreach (var args in testArgs)
        {
            string argString = string.Join(" ", args);
            string output = await RunAndCaptureAsync(filePath, argString, useUtf8: true);

            if (output.Contains("No script file specified", StringComparison.OrdinalIgnoreCase))
                return argString;
        }

        return null;
    }

    public static bool HasValidVspipeY4mArg(string? vspipePath, string? vspipeY4mArg)
    {
        return !string.IsNullOrWhiteSpace(vspipePath) &&
               !string.IsNullOrWhiteSpace(vspipeY4mArg);
    }

    public static async Task DetectAndStoreVspipeY4mArgAsync(
        string exeName,
        string filePath,
        Action<string?> store)
    {
        if (!exeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase)) return;
        store(await DetectVspipeY4mArgAsync(filePath));
    }

    #endregion
}
