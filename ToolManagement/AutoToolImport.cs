using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.ToolManagement;

public static class AutoToolImport
{
    public sealed record Candidate(string ExeName, string FilePath, string Version);

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
        "ffprobe.exe"
    ];

    private static readonly HashSet<string> TopLevelScanTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "ffmpeg.exe",
        "ffprobe.exe",
        "x264.exe",
        "x265.exe",
        "svtav1encapp.exe"
    };

    /// <summary>
    /// Scans each known tool: skips already-configured ones, then searches for the executable in the app dir,
    /// the upstream tree, and finally the system PATH. Returns everything that was found with a version.
    /// </summary>
    public static async Task<IReadOnlyList<Candidate>> FindImportableToolsAsync(AppDataM.Importables tools)
    {
        List<Candidate> candidates = [];
        foreach (string exeName in DetectableTools)
        {
            if (!NeedsImport(exeName, tools)) continue;

            Candidate? candidate = await FindTopLevelCandidateAsync(exeName)
                ?? await FindUpstreamTreeCandidateAsync(exeName)
                ?? await FindInstalledCandidateAsync(exeName);
            if (candidate != null) candidates.Add(candidate);
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
        if (!TopLevelScanTools.Contains(exeName)) return null;

        foreach (string directory in GetTopLevelScanDirectories())
        {
            if (!Directory.Exists(directory)) continue;

            IEnumerable<FileInfo> matches;
            try
            {
                matches = [.. Directory.EnumerateFiles(directory, "*.exe", SearchOption.TopDirectoryOnly)
                    .Where(path => IsCandidateFileNameMatch(exeName, path))
                    .Select(path => new FileInfo(path))
                    .OrderByDescending(file => file.Name.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(file => Path.GetFileNameWithoutExtension(file.Name).Length)
                    .ThenByDescending(file => file.LastWriteTimeUtc)];
            }
            catch { continue; }

            foreach (FileInfo file in matches)
            {
                Candidate? candidate = await TryBuildCandidateAsync(exeName, file.FullName);
                if (candidate != null) return candidate;
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
        foreach (string rootDirectory in GetUpstreamTreeScanDirectories())
        {
            if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory)) continue;

            IEnumerable<FileInfo> matches;
            try
            {
                matches = [.. Directory.EnumerateFiles(rootDirectory, "*.exe", SearchOption.AllDirectories)
                    .Where(path => IsCandidateFileNameMatch(exeName, path))
                    .Select(path => new FileInfo(path))
                    .OrderByDescending(file => file.Name.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(file => file.FullName.Length)
                    .ThenByDescending(file => file.LastWriteTimeUtc)];
            }
            catch { continue; }

            foreach (FileInfo file in matches)
            {
                Candidate? candidate = await TryBuildCandidateAsync(exeName, file.FullName);
                if (candidate != null) return candidate;
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
            Candidate? knownPathCandidate = await TryBuildCandidateAsync(exeName, Path.Combine(directory, exeName));
            if (knownPathCandidate != null) return knownPathCandidate;
        }

        foreach (string pathDirectory in GetPathDirectories())
        {
            Candidate? pathCandidate = await TryBuildCandidateAsync(exeName, Path.Combine(pathDirectory, exeName));
            if (pathCandidate != null) return pathCandidate;
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
        string pattern = token switch
        {
            "svtav1encapp" => @"(^|[^a-z0-9])svt[^a-z0-9]*av1[^a-z0-9]*enc[^a-z0-9]*app([^a-z0-9]|$)",
            _ => $@"(^|[^a-z0-9]){Regex.Escape(token)}([^a-z0-9]|$)"
        };

        return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Probes the executable for its version; timeouts (and failed probes) mean the file is not usable.
    /// </summary>
    private static async Task<Candidate?> TryBuildCandidateAsync(string exeName, string filePath)
    {
        string? version;
        try
        {
            version = await ToolVersionDetect.TryDetectAsync(exeName, filePath);
        }
        catch (ToolVersionDetectTimeoutException) { return null; }

        return string.IsNullOrWhiteSpace(version)
            ? null
            : new Candidate(exeName, filePath, version);
    }
}
