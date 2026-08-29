using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.FFmpeg;

public static partial class SubtitleHelper
{
    [GeneratedRegex(
        @"(\d{1,2}:\d{2}:\d{2}[,.]\d{2,3})\s*-->\s*(\d{1,2}:\d{2}:\d{2}[,.]\d{2,3})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex TimestampRegex();

    [GeneratedRegex(
        @"^Dialogue\s*:\s*[^,]*,(\d{1,2}:\d{2}:\d{2}\.\d{1,2}),(\d{1,2}:\d{2}:\d{2}\.\d{1,2})(?:,|$)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex AssDialogueRegex();

    private static readonly string[] SrtFormats = [@"hh\:mm\:ss\,fff", @"h\:m\:s\,f"];
    private static readonly string[] VttFormats = [@"hh\:mm\:ss\.fff", @"h\:m\:s\.f"];
    private static readonly string[] AssFormats = [@"h\:mm\:ss\.ff", @"h\:m\:s\.ff"];

    /// <summary>
    /// Gets the end time of the last subtitle cue. Returns null for a missing,
    /// unreadable, or unsupported subtitle file.
    /// </summary>
    public static TimeSpan? GetDuration(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        try
        {
            foreach (string line in File.ReadLines(filePath).Reverse())
            {
                Match match = TimestampRegex().Match(line);
                if (match.Success && TryParseTimestamp(match.Groups[2].Value, out TimeSpan result))
                    return result;

                match = AssDialogueRegex().Match(line);
                if (match.Success && TryParseTimestamp(match.Groups[2].Value, out result))
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
