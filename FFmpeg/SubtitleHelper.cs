using System.IO;
using OneColumnEncoder.Models;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.FFmpeg;

public static class SubtitleHelper
{
    private static readonly string[] SrtFormats = [@"hh\:mm\:ss\,fff", @"h\:m\:s\,f"];
    private static readonly string[] VttFormats =
    [
        @"hh\:mm\:ss\.fff", @"h\:m\:s\.f",
        @"mm\:ss\.fff", @"m\:s\.f"
    ];
    private static readonly string[] AssFormats = [@"h\:mm\:ss\.ff", @"h\:m\:s\.ff"];
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ass", ".ssa", ".srt", ".vtt", ".webvtt"
    };

    /// <summary>
    /// Gets the end time of the last subtitle cue. Returns null for a missing,
    /// unreadable, or unsupported subtitle file.
    /// </summary>
    public static TimeSpan? GetDuration(string filePath)
    {
        if (!File.Exists(filePath) || !SupportedExtensions.Contains(Path.GetExtension(filePath))) return null;

        try
        {
            foreach (string line in File.ReadLines(filePath).Reverse())
            {
                Match match = RegexProviderM.TimestampRegex().Match(line);
                if (match.Success
                    && TryParseTimestamp(match.Groups["start"].Value, out TimeSpan start)
                    && TryParseTimestamp(match.Groups["end"].Value, out TimeSpan result)
                    && result > start)
                    return result;

                match = RegexProviderM.AssDialogueRegex().Match(line);
                if (match.Success
                    && TryParseTimestamp(match.Groups[1].Value, out start)
                    && TryParseTimestamp(match.Groups[2].Value, out result)
                    && result > start)
                    return result;
            }
        }
        catch {} // return null
        return null;
    }

    private static bool TryParseTimestamp(string timestamp, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        if (TimeSpan.TryParseExact(timestamp, SrtFormats, CultureInfo.InvariantCulture, out result)) return true;
        if (TimeSpan.TryParseExact(timestamp, VttFormats, CultureInfo.InvariantCulture, out result)) return true;
        if (TimeSpan.TryParseExact(timestamp, AssFormats, CultureInfo.InvariantCulture, out result)) return true;

        string normalized = timestamp.Replace(',', '.');
        return TimeSpan.TryParse(normalized, CultureInfo.InvariantCulture, out result);
    }
}
