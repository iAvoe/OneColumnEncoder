using System.IO;
using System.Runtime.Intrinsics.X86;

namespace OneColumnEncoder.ToolManagement;

public static class AutoToolImport
{
    public sealed record Candidate(string ExeName, string FilePath, string Version);

    /// <summary>
    /// True when the CPU and OS both support AVX-512 (Avx512F.IsSupported validates CPUID flags
    /// and OS XSAVE/ZMM state enablement). Guarded by try/catch: exotic hosts or old OSes must
    /// never make the startup import pass crash, in which case AVX-512 is assumed unsupported.
    /// </summary>
    public static bool SupportsAvx512 { get; } = DetectAvx512Support();

    private static bool DetectAvx512Support()
    {
        try { return Avx512F.IsSupported; }
        catch { return false; }
    }

    /// <summary>
    /// x265 is auto-imported again because version probing now rejects binaries that crash with
    /// 0xC000001D. If the probe crashes, scanning for x265 stops entirely (no silent fallback to
    /// another build) and <see cref="X265RequiresManualImport"/> is set so the user is asked to
    /// import a matching build manually: the compiled instruction set of an x265 binary cannot be
    /// determined reliably from its file name.
    /// </summary>
    private static readonly string[] DetectableTools =
    [
        "ffmpeg.exe",
        "vspipe.exe",
        "avs2yuv.exe",
        "avs2pipemod.exe",
        "one_line_shot_args.exe",
        "x264.exe",
        "x265.exe",
        "svtav1encapp.exe",
        "ffprobe.exe",
        "avisynth.dll"
    ];

    private static readonly HashSet<string> TopLevelScanTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "ffmpeg.exe",
        "ffprobe.exe",
        "x264.exe",
        "x265.exe",
        "svtav1encapp.exe",
        "avs2yuv.exe",
        "avs2pipemod.exe"
    };

    private static readonly HashSet<string> TopLevelScanDllTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "avisynth.dll"
    };

    /// <summary>
    /// Set during the last <see cref="FindImportableToolsAsync"/> pass when an x265 build crashed
    /// while being probed (typically 0xC000001D from an AVX-512 build on a machine without
    /// AVX-512). x265 is then left for manual import instead of trying other candidate builds.
    /// </summary>
    public static bool X265RequiresManualImport { get; private set; }

    /// <summary>
    /// Records that the current x265 scan hit a crashing build; all further x265 candidates are
    /// skipped so a different build (e.g. AVX2) is never chosen automatically.
    /// </summary>
    private static bool _x265CrashDetected;

    /// <summary>
    /// Scans each known tool: skips already-configured ones, then searches for the executable in the app dir,
    /// the upstream tree, and finally the system PATH. Returns everything that was found with a version.
    /// </summary>
    public static async Task<IReadOnlyList<Candidate>> FindImportableToolsAsync(AppDataM.Importables tools)
    {
        X265RequiresManualImport = false;

        List<Candidate> candidates = [];
        foreach (string exeName in DetectableTools)
        {
            if (!NeedsImport(exeName, tools)) continue;

            _x265CrashDetected = false;

            Candidate? candidate = await FindTopLevelCandidateAsync(exeName);
            if (!_x265CrashDetected)
                candidate ??= await FindUpstreamTreeCandidateAsync(exeName);
            if (!_x265CrashDetected)
                candidate ??= await FindInstalledCandidateAsync(exeName);

            if (candidate != null) candidates.Add(candidate);
            if (_x265CrashDetected) X265RequiresManualImport = true;
        }

        return candidates;
    }

    /// <summary>
    /// A tool needs importing when its configured path is empty or no longer points to an existing file.
    /// </summary>
    private static bool NeedsImport(string exeName, AppDataM.Importables tools)
    {
        string? path = exeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => tools.FFmpegPath,
            "vspipe.exe" => tools.VspipePath,
            "avs2yuv.exe" => tools.Avs2yuvPath,
            "avs2pipemod.exe" => tools.Avs2pipemodPath,
            "one_line_shot_args.exe" => tools.OneLineShotArgsPath,
            "x264.exe" => tools.X264Path,
            "x265.exe" => tools.X265Path,
            "svtav1encapp.exe" => tools.SvtAv1Path,
            "ffprobe.exe" => tools.FfprobePath,
            _ => null
        };

        return string.IsNullOrWhiteSpace(path) || !File.Exists(path);
    }

    /// <summary>
    /// Looks up top-level tools (ffmpeg, ffprobe, x264, ...) directly in the app directory
    /// and the config directory (1cenc). Results are ranked: exact-name match first,
    /// then shorter file names, then most recently modified.
    /// </summary>
    private static async Task<Candidate?> FindTopLevelCandidateAsync(string exeName)
    {
        bool isDll = TopLevelScanDllTools.Contains(exeName);
        if (!TopLevelScanTools.Contains(exeName) && !isDll) return null;

        string searchPattern = isDll ? "*.dll" : "*.exe";

        foreach (string directory in GetTopLevelScanDirectories())
        {
            if (!Directory.Exists(directory)) continue;

            IEnumerable<FileInfo> matches;
            try
            {
                matches = [.. Directory.EnumerateFiles(directory, searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(path => IsCandidateFileNameMatch(exeName, path))
                    .Select(path => new FileInfo(path))
                    .OrderByDescending(file => file.Name.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(file => Path.GetFileNameWithoutExtension(file.Name).Length)
                    .ThenByDescending(file => file.LastWriteTimeUtc)];
            }
            catch { continue; }

            foreach (FileInfo file in matches)
            {
                (Candidate? candidate, bool crashed) = await TryBuildCandidateAsync(exeName, file.FullName);
                if (candidate != null) return candidate;
                if (crashed && IsX265(exeName)) return null;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetTopLevelScanDirectories()
    {
        yield return AppDomain.CurrentDomain.BaseDirectory;
        yield return SaveLoadBase<AppConfM>.GetConfigDirectory();
    }

    /// <summary>
    /// Recursively scans the bundled upstream encoder tree (x64/x86 based on process bitness)
    /// in the app directory and the config directory (1cenc).
    /// Results are ranked: exact-name match first, then shorter full paths, then most recently modified.
    /// </summary>
    private static async Task<Candidate?> FindUpstreamTreeCandidateAsync(string exeName)
    {
        bool isDll = TopLevelScanDllTools.Contains(exeName);
        string searchPattern = isDll ? "*.dll" : "*.exe";

        foreach (string rootDirectory in GetUpstreamTreeScanDirectories())
        {
            if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory)) continue;

            IEnumerable<FileInfo> matches;
            try
            {
                matches = [.. Directory.EnumerateFiles(rootDirectory, searchPattern, SearchOption.AllDirectories)
                    .Where(path => IsCandidateFileNameMatch(exeName, path))
                    .Select(path => new FileInfo(path))
                    .OrderByDescending(file => file.Name.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(file => file.FullName.Length)
                    .ThenByDescending(file => file.LastWriteTimeUtc)];
            }
            catch { continue; }

            foreach (FileInfo file in matches)
            {
                (Candidate? candidate, bool crashed) = await TryBuildCandidateAsync(exeName, file.FullName);
                if (candidate != null) return candidate;
                if (crashed && IsX265(exeName)) return null;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetUpstreamTreeScanDirectories()
    {
        string folderName = Environment.Is64BitProcess ? "x64-upstreams-encoders" : "x86-upstreams-encoders";
        yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
        yield return Path.Combine(SaveLoadBase<AppConfM>.GetConfigDirectory(), folderName);
    }

    /// <summary>
    /// Falls back to an already-known install directory from the tool catalog, then scans every
    /// directory on the PATH for the executable.
    /// </summary>
    private static async Task<Candidate?> FindInstalledCandidateAsync(string exeName)
    {
        string? directory = ToolCatalogProviderM.TryFindToolDirectory(exeName);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            (Candidate? knownPathCandidate, bool crashed) = await TryBuildCandidateAsync(exeName, Path.Combine(directory, exeName));
            if (knownPathCandidate != null) return knownPathCandidate;
            if (crashed && IsX265(exeName)) return null;
        }

        foreach (string pathDirectory in GetPathDirectories())
        {
            (Candidate? pathCandidate, bool pathCrashed) = await TryBuildCandidateAsync(exeName, Path.Combine(pathDirectory, exeName));
            if (pathCandidate != null) return pathCandidate;
            if (pathCrashed && IsX265(exeName)) return null;
        }

        return null;
    }

    /// <summary>
    /// Yields each directory on PATH that exists, de-duplicating entries case-insensitively.
    /// </summary>
    private static IEnumerable<string> GetPathDirectories()
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path)) yield break;

        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (string directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Directory.Exists(directory) || !seen.Add(directory)) continue;
            yield return directory;
        }
    }

    /// <summary>
    /// A file counts as a match if its name contains the expected token as a whole word
    /// (e.g. "x264-r3107" matches "x264"; svt-av1 has a more lenient custom pattern).
    /// </summary>
    private static bool IsCandidateFileNameMatch(string exeName, string filePath)
    {
        string name = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
        string token = Path.GetFileNameWithoutExtension(exeName).ToLowerInvariant();
        return RegexProviderM.IsCandidateFileNameMatch(name, token);
    }

    /// <summary>
    /// True when the given tool is x265, whose compiled instruction set cannot be inferred from
    /// its file name and which therefore is never auto-substituted by another candidate build.
    /// </summary>
    private static bool IsX265(string exeName) =>
        exeName.Equals("x265.exe", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Probes the executable for its version. Timeouts and probes that return no version mean the
    /// file is not usable; an abnormal termination (e.g. 0xC000001D) is reported back as a crash so
    /// callers can stop scanning that tool instead of importing it.
    /// </summary>
    private static async Task<(Candidate? Candidate, bool Crashed)> TryBuildCandidateAsync(string exeName, string filePath)
    {
        string? version;
        try
        {
            version = await ToolManagementProviderM.TryDetectAsync(exeName, filePath);
        }
        catch (ToolVersionDetectTimeoutException) { return (null, false); }
        catch (ToolVersionDetectCrashException)
        {
            if (IsX265(exeName)) _x265CrashDetected = true;
            return (null, true);
        }

        return (string.IsNullOrWhiteSpace(version)
            ? null
            : new Candidate(exeName, filePath, version), false);
    }
}
