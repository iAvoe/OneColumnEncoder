using System.IO;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// ffprobe import gate shared by the multi-file modes. The extension whitelist never
/// rejects anything on its own; this gate turns the ffprobe verdict into the readable
/// subset plus one "name: reason" line per file ffprobe returns no data for.
/// </summary>
public static class SrcProbeGate
{
    private const int MaxFailureDetails = 5;

    internal static AnalyzeSrcVideoCmdLangProvider Lang =>
        new(UILangProvider.Current.LanguageCode);

    /// <summary>
    /// Runs a ffprobe analyzability pass over the selection. Without a usable ffprobe the
    /// gate is a no-op and every file passes through; the later analysis reports the
    /// missing tool, so the import is never silently narrowed here.
    /// </summary>
    public static async Task<SrcProbeVerdict> RunAsync(
        string ffprobePath,
        IReadOnlyList<string> filePaths)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            return new SrcProbeVerdict([.. filePaths], []);

        List<string> readablePaths = [];
        List<string> failures = [];

        foreach (string filePath in filePaths)
        {
            try
            {
                _ = await FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, filePath);
                readablePaths.Add(filePath);
            }
            catch (Exception ex)
            {
                failures.Add(FormatFailure(filePath, ex.Message));
            }
        }

        return new SrcProbeVerdict([.. readablePaths], failures);
    }

    /// <summary>
    /// Formats a compact "skipped failed items" summary: the label with the total count,
    /// up to <see cref="MaxFailureDetails"/> detail lines, then an "...and N more" note.
    /// </summary>
    public static string FormatFailures(IReadOnlyList<string> failures)
    {
        if (failures.Count == 0) return string.Empty;

        List<string> lines = [string.Format(Lang.SkippedItemsLabel, failures.Count)];
        lines.AddRange(failures.Take(MaxFailureDetails).Select(FormatDetail));
        int omitted = failures.Count - Math.Min(failures.Count, MaxFailureDetails);

        if (omitted > 0) lines.Add(string.Format(Lang.AndMoreLabel, omitted));
        return string.Join(Environment.NewLine, lines);
    }

    // One bullet per excluded file, matching the queue analysis summary style.
    internal static string FormatDetail(string failure) =>
        string.Format(Lang.ListItemPrefix, failure);

    // One line per excluded file: "name: reason", first line of the reason only.
    private static string FormatFailure(string filePath, string? reason)
    {
        string name = Path.GetFileName(filePath);
        string detail = (reason ?? string.Empty).Split('\n')[0].Trim();
        return string.IsNullOrWhiteSpace(detail) ? name : $"{name}: {detail}";
    }
}

/// <summary>
/// ffprobe verdict of one import selection: the readable subset and the per-file
/// failures of everything that was excluded.
/// </summary>
public sealed record SrcProbeVerdict(string[] ReadablePaths, IReadOnlyList<string> Failures)
{
    public int RequestedCount => ReadablePaths.Length + Failures.Count;

    /// <summary>Nothing in the selection can be read, so the whole import is refused.</summary>
    public bool IsRefused => ReadablePaths.Length == 0;

    public bool HasFailures => Failures.Count > 0;

    /// <summary>Whole-import refusal message: the count line followed by every failure.</summary>
    public string BuildRefusalMessage()
    {
        List<string> lines = [string.Format(SrcProbeGate.Lang.AllQueueItemsFailed, RequestedCount)];
        lines.AddRange(Failures.Select(SrcProbeGate.FormatDetail));
        return string.Join(Environment.NewLine, lines);
    }

    public string BuildFailureSummary() => SrcProbeGate.FormatFailures(Failures);
}
