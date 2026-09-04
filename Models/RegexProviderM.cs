using System.Text.RegularExpressions;

namespace OneColumnEncoder.Models;

public static partial class RegexProviderM
{
    [GeneratedRegex(@"(?<start>(?:(?:\d{1,2}:)?\d{1,2}:\d{2}[,.]\d{1,3}))\s*-->\s*(?<end>(?:(?:\d{1,2}:)?\d{1,2}:\d{2}[,.]\d{1,3}))", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    public static partial Regex TimestampRegex();

    [GeneratedRegex(@"^Dialogue\s*:\s*[^,]*,(\d{1,2}:\d{2}:\d{2}\.\d{1,2}),(\d{1,2}:\d{2}:\d{2}\.\d{1,2})(?:,|$)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    public static partial Regex AssDialogueRegex();

    [GeneratedRegex(@"X+(?:\.X+)?\s*(?:GBps|GB|MB|%)?", RegexOptions.CultureInvariant)]
    public static partial Regex FileSizeMetricRegex();

    [GeneratedRegex(@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex ReservedFilenamesRegex();

    [GeneratedRegex(@"\bver\s+\S+", RegexOptions.IgnoreCase)]
    public static partial Regex Avs2pipemodVersionRegex();

    [GeneratedRegex(@"version\s+(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)]
    public static partial Regex X265VersionRegex();

    [GeneratedRegex(@"frame=\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    public static partial Regex FFmpegTotalFramesRegex();

    [GeneratedRegex(@"fps=(\d+/\d+)")]
    public static partial Regex FpsRegex();

    [GeneratedRegex(@"gui_inputs\s*=\s*""((?:[^""\\]|\\.)*)""")]
    public static partial Regex SvfiIniRegex();

    [GeneratedRegex("(?:-filter(?::v)?|-vf)\\s+(?:\"(?<quoted>[^\"]+)\"|'(?<single>[^']+)'|(?<plain>\\S+))", RegexOptions.IgnoreCase, "zh-CN")]
    public static partial Regex FFmpegFilterVScaleRegex();

    [GeneratedRegex(@"(?<![\d.])\d{1,3}(?:\.\d+)?\s*%")]
    public static partial Regex ProgressLineRegex();

    [GeneratedRegex(@"(?<![\d.])(\d{1,3})(?:\.\d+)?\s*%")]
    public static partial Regex ProgressPercentRegex();

    [GeneratedRegex(@"(?:^|\s)(?:frame|fps|size|time|bitrate|speed|dup|drop|progress)\s*=\s*[^\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex FFmpegProgressFieldRegex();

    [GeneratedRegex(@"(?:^|\s)(?:frame|fps|size|time|bitrate|speed|dup|drop)\s*[=:]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex FFmpegProgressKeyRegex();

    [GeneratedRegex(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.CultureInvariant)]
    public static partial Regex AnsiEscapeRegex();

    [GeneratedRegex(@"(?:^|\D)frame\s*=\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex FFmpegFrameRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s*:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex X264FrameRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s*/\s*\d+\s+frames?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex SlashFrameRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s+@", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex FramesAtRegex();

    [GeneratedRegex(@"\bencoding\s+frame\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex EncodingFrameRegex();

    [GeneratedRegex(@"(?:^|\D)encoded\s+(\d+)\s+frames?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex EncodedFrameRegex();

    [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s+encoded", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex FramesEncodedRegex();

    public static Match MatchIntegerArg(string args, string name) =>
        Regex.Match(args, $@"(?:^|\s){Regex.Escape(name)}\s+(-?\d+)(?=\s|$)");

    public static bool IsCandidateFileNameMatch(string name, string token)
    {
        string pattern = token switch
        {
            "svtav1encapp" => @"(^|[^a-z0-9])svt[^a-z0-9]*av1[^a-z0-9]*enc[^a-z0-9]*app([^a-z0-9]|$)",
            _ => $@"(^|[^a-z0-9]){Regex.Escape(token)}([^a-z0-9]|$)"
        };

        return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
    }

    public static Match MatchScriptSourcePath(string trimmed, string ext)
    {
        string pattern = ext.Equals(".vpy", StringComparison.OrdinalIgnoreCase)
            ? "src\\s*=\\s*core\\.lsmas\\.LWLibavSource\\(source=r\"([^\"]+)\""
            : "LWLibavVideoSource\\(\"([^\"]+)\"";

        return Regex.Match(trimmed, pattern);
    }
}
