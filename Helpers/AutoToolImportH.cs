using OneColumnEncoder.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Helpers;

public static class AutoToolImportH
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

    public static async Task<IReadOnlyList<Candidate>> FindImportableToolsAsync(AppDataM.Importables tools)
    {
        List<Candidate> candidates = [];
        foreach (string exeName in DetectableTools)
        {
            if (!NeedsImport(exeName, tools)) continue;

            Candidate? candidate = await FindTopLevelCandidateAsync(exeName)
                ?? await FindInstalledCandidateAsync(exeName);
            if (candidate != null) candidates.Add(candidate);
        }

        return candidates;
    }

    private static bool NeedsImport(string exeName, AppDataM.Importables tools)
    {
        string? path = exeName.ToLowerInvariant() switch
        {
            "ffmpeg.exe" => tools.FfmpegPath,
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

    private static async Task<Candidate?> FindTopLevelCandidateAsync(string exeName)
    {
        if (!TopLevelScanTools.Contains(exeName)) return null;

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        if (!Directory.Exists(baseDirectory)) return null;

        IEnumerable<FileInfo> matches;
        try
        {
            matches = Directory.EnumerateFiles(baseDirectory, "*.exe", SearchOption.TopDirectoryOnly)
                .Where(path => IsCandidateFileNameMatch(exeName, path))
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.Name.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                .ThenBy(file => Path.GetFileNameWithoutExtension(file.Name).Length)
                .ThenByDescending(file => file.LastWriteTimeUtc)
                .ToArray();
        }
        catch
        {
            return null;
        }

        foreach (FileInfo file in matches)
        {
            Candidate? candidate = await TryBuildCandidateAsync(exeName, file.FullName);
            if (candidate != null) return candidate;
        }

        return null;
    }

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

    private static async Task<Candidate?> TryBuildCandidateAsync(string exeName, string filePath)
    {
        string? version = await ToolVersionDetectH.TryDetectAsync(exeName, filePath);
        return string.IsNullOrWhiteSpace(version)
            ? null
            : new Candidate(exeName, filePath, version);
    }
}
