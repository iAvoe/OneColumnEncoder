using System.IO;
using OneColumnEncoder.Models;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Validation;

public enum ScriptSrcValIssueKind
{
    NoMatchingVideoSrc,
    NoMatchingScriptFile,
    UnreadableScript,
    srcPathMismatch
}

public sealed record ScriptSrcValIssue(
    ScriptSrcValIssueKind Kind,
    string ScriptPath,
    string? EmbeddedPath = null,
    string? ExpectedPath = null);

public static class ScriptSrcValidator
{
    public static IReadOnlyList<ScriptSrcValIssue> ValidateQueue(
        SrcFileKind kind,
        IEnumerable<string> scriptPaths,
        IEnumerable<string> videoPaths)
    {
        string ext = GetSupportedScriptExtension(kind);
        if (string.IsNullOrEmpty(ext)) return [];

        Dictionary<string, string> videoByBasename = new(StringComparer.OrdinalIgnoreCase);
        foreach (string videoPath in videoPaths)
            videoByBasename[Path.GetFileNameWithoutExtension(videoPath)] = videoPath;

        List<ScriptSrcValIssue> issues = [];

        HashSet<string> scriptBasenames = new(StringComparer.OrdinalIgnoreCase);
        foreach (string scriptPath in scriptPaths)
        {
            string scriptBasename = Path.GetFileNameWithoutExtension(scriptPath);
            scriptBasenames.Add(scriptBasename);

            if (!videoByBasename.TryGetValue(scriptBasename, out string? videoPath))
            {
                issues.Add(new(ScriptSrcValIssueKind.NoMatchingVideoSrc, scriptPath));
                continue;
            }

            AddMismatchIssueIfNeeded(issues, scriptPath, ext, videoPath);
        }

        foreach (string videoPath in videoPaths)
        {
            string videoBasename = Path.GetFileNameWithoutExtension(videoPath);
            if (!scriptBasenames.Contains(videoBasename))
                issues.Add(new(ScriptSrcValIssueKind.NoMatchingScriptFile, videoPath));
        }

        return issues;
    }

    public static ScriptSrcValIssue? ValidateSingle(
        SrcFileKind kind,
        string scriptPath,
        string videoPath)
    {
        string ext = GetSupportedScriptExtension(kind);
        if (string.IsNullOrEmpty(ext) || string.IsNullOrWhiteSpace(videoPath)) return null;

        List<ScriptSrcValIssue> issues = [];
        AddMismatchIssueIfNeeded(issues, scriptPath, ext, videoPath);
        return issues.FirstOrDefault();
    }

    public static string? ExtractScriptSrcPath(string scriptFilePath, string ext)
    {
        if (!File.Exists(scriptFilePath)) return null;

        try
        {
            string[] lines = File.ReadAllLines(scriptFilePath);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || trimmed.StartsWith(';'))
                    continue;
                Match match = RegexProvider.MatchScriptSourcePath(trimmed, ext);
                if (match.Success)
                    return match.Groups[1].Value.Trim();
            }
        }
        catch {}
        return null;
    }

    private static void AddMismatchIssueIfNeeded(
        List<ScriptSrcValIssue> issues,
        string scriptPath,
        string ext,
        string expectedVideoPath)
    {
        string? embeddedPath = ExtractScriptSrcPath(scriptPath, ext);
        if (embeddedPath == null)
        {
            issues.Add(new(ScriptSrcValIssueKind.UnreadableScript, scriptPath));
            return;
        }

        string normalizedEmbedded = Path.GetFullPath(embeddedPath);
        string normalizedExpected = Path.GetFullPath(expectedVideoPath);
        if (!string.Equals(normalizedEmbedded, normalizedExpected, StringComparison.OrdinalIgnoreCase))
            issues.Add(new(ScriptSrcValIssueKind.srcPathMismatch, scriptPath, embeddedPath, expectedVideoPath));
    }

    private static string GetSupportedScriptExtension(SrcFileKind kind) => kind switch
    {
        SrcFileKind.VapourSynthScript => ".vpy",
        SrcFileKind.AviSynthScript => ".avs",
        _ => string.Empty
    };
}
