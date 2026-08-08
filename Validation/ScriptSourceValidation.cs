using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Validation;

public enum ScriptSourceValidationIssueKind
{
    NoMatchingVideoSource,
    NoMatchingScriptFile,
    UnreadableScript,
    SourcePathMismatch
}

public sealed record ScriptSourceValidationIssue(
    ScriptSourceValidationIssueKind Kind,
    string ScriptPath,
    string? EmbeddedPath = null,
    string? ExpectedPath = null);

public static class ScriptSourceValidation
{
    public static IReadOnlyList<ScriptSourceValidationIssue> ValidateQueue(
        SourceFileKind kind,
        IEnumerable<string> scriptPaths,
        IEnumerable<string> videoPaths)
    {
        string ext = GetSupportedScriptExtension(kind);
        if (string.IsNullOrEmpty(ext)) return [];

        Dictionary<string, string> videoByBasename = new(StringComparer.OrdinalIgnoreCase);
        foreach (string videoPath in videoPaths)
            videoByBasename[Path.GetFileNameWithoutExtension(videoPath)] = videoPath;

        List<ScriptSourceValidationIssue> issues = [];

        HashSet<string> scriptBasenames = new(StringComparer.OrdinalIgnoreCase);
        foreach (string scriptPath in scriptPaths)
        {
            string scriptBasename = Path.GetFileNameWithoutExtension(scriptPath);
            scriptBasenames.Add(scriptBasename);

            if (!videoByBasename.TryGetValue(scriptBasename, out string? videoPath))
            {
                issues.Add(new(ScriptSourceValidationIssueKind.NoMatchingVideoSource, scriptPath));
                continue;
            }

            AddMismatchIssueIfNeeded(issues, scriptPath, ext, videoPath);
        }

        foreach (string videoPath in videoPaths)
        {
            string videoBasename = Path.GetFileNameWithoutExtension(videoPath);
            if (!scriptBasenames.Contains(videoBasename))
                issues.Add(new(ScriptSourceValidationIssueKind.NoMatchingScriptFile, videoPath));
        }

        return issues;
    }

    public static ScriptSourceValidationIssue? ValidateSingle(
        SourceFileKind kind,
        string scriptPath,
        string videoPath)
    {
        string ext = GetSupportedScriptExtension(kind);
        if (string.IsNullOrEmpty(ext) || string.IsNullOrWhiteSpace(videoPath)) return null;

        List<ScriptSourceValidationIssue> issues = [];
        AddMismatchIssueIfNeeded(issues, scriptPath, ext, videoPath);
        return issues.FirstOrDefault();
    }

    public static string? ExtractScriptSourcePath(string scriptFilePath, string ext)
    {
        if (!File.Exists(scriptFilePath)) return null;

        try
        {
            string[] lines = File.ReadAllLines(scriptFilePath);
            string pattern = ext.Equals(".vpy", StringComparison.OrdinalIgnoreCase)
                ? @"src\s*=\s*core\.lsmas\.LWLibavSource\(source=r""([^""]+)"""
                : @"LWLibavVideoSource\(""([^""]+)""";

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith(';'))
                    continue;
                Match match = Regex.Match(trimmed, pattern);
                if (match.Success)
                    return match.Groups[1].Value.Trim();
            }
        }
        catch
        {
        }

        return null;
    }

    private static void AddMismatchIssueIfNeeded(
        ICollection<ScriptSourceValidationIssue> issues,
        string scriptPath,
        string ext,
        string expectedVideoPath)
    {
        string? embeddedPath = ExtractScriptSourcePath(scriptPath, ext);
        if (embeddedPath == null)
        {
            issues.Add(new(ScriptSourceValidationIssueKind.UnreadableScript, scriptPath));
            return;
        }

        string normalizedEmbedded = Path.GetFullPath(embeddedPath);
        string normalizedExpected = Path.GetFullPath(expectedVideoPath);
        if (!string.Equals(normalizedEmbedded, normalizedExpected, StringComparison.OrdinalIgnoreCase))
            issues.Add(new(ScriptSourceValidationIssueKind.SourcePathMismatch, scriptPath, embeddedPath, expectedVideoPath));
    }

    private static string GetSupportedScriptExtension(SourceFileKind kind) => kind switch
    {
        SourceFileKind.VapourSynthScript => ".vpy",
        SourceFileKind.AviSynthScript => ".avs",
        _ => string.Empty
    };
}
