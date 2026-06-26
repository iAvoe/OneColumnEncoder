using System.Globalization;

namespace OneColumnEncoder.Pipeline;

public static class SampleClip
{
    public static double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));

    public static double ClampDuration(double durationSeconds, double totalSeconds, int minClipLengthSeconds, int maxClipLengthSeconds)
    {
        double maxDurationSeconds = Math.Min(maxClipLengthSeconds, totalSeconds);
        double minDurationSeconds = Math.Min(minClipLengthSeconds, maxDurationSeconds);
        return Clamp(durationSeconds, minDurationSeconds, maxDurationSeconds);
    }

    public static (double selectionStart, double selectionEnd)? NormalizeSelectionSeconds(
        double startSeconds,
        double endSeconds,
        bool anchorEnd,
        double totalSeconds,
        int minClipLengthSeconds,
        int maxClipLengthSeconds)
    {
        if (totalSeconds <= 0d
            || double.IsNaN(startSeconds)
            || double.IsNaN(endSeconds)
            || double.IsInfinity(startSeconds)
            || double.IsInfinity(endSeconds))
            return null;

        double durationSeconds = ClampDuration(endSeconds - startSeconds, totalSeconds, minClipLengthSeconds, maxClipLengthSeconds);

        if (anchorEnd)
        {
            endSeconds = Clamp(endSeconds, 0d, totalSeconds);
            startSeconds = endSeconds - durationSeconds;
        }
        else
        {
            startSeconds = Clamp(startSeconds, 0d, totalSeconds);
            endSeconds = startSeconds + durationSeconds;
        }

        if (startSeconds < 0d)
        {
            startSeconds = 0d;
            endSeconds = Math.Min(totalSeconds, durationSeconds);
        }

        if (endSeconds > totalSeconds)
        {
            endSeconds = totalSeconds;
            startSeconds = Math.Max(0d, endSeconds - durationSeconds);
        }

        double start = Clamp(startSeconds / totalSeconds, 0d, 1d);
        double end = Clamp(endSeconds / totalSeconds, 0d, 1d);
        return end <= start ? null : (start, end);
    }

    public static bool TryParseSourceSeconds(string text, double totalSeconds, bool allowSourceEnd, out double seconds)
    {
        try
        {
            seconds = EncodingPipeline.ParseTimestamp(text).TotalSeconds;
            return seconds >= 0d && (allowSourceEnd ? seconds <= totalSeconds : seconds < totalSeconds);
        }
        catch
        {
            seconds = 0d;
            return false;
        }
    }

    public static bool TryParseSourceFrame(string text, long totalFrames, out long frame)
    {
        frame = TryParseNonNegativeLong(text) ?? -1L;
        return frame >= 0L && frame < totalFrames;
    }

    public static long SecondsToFirstFrame(double seconds, double frameRate) =>
        Math.Max(0L, (long)Math.Ceiling(seconds * frameRate));

    public static long SecondsToLastFrame(double seconds, double frameRate) =>
        Math.Max(0L, (long)Math.Ceiling(seconds * frameRate) - 1L);

    public static long? TryParseNonNegativeLong(string text)
    {
        if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
            return null;
        return Math.Max(0, value);
    }

    public static string FormatAxisTimestamp(double seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(Math.Max(0d, seconds));
        return $"{(long)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }
}
