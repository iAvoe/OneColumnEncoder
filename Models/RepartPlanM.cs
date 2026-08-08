using System.IO;

namespace OneColumnEncoder.Models;

public sealed record RepartVideoFormatSignature(
    string CodecName,
    string Profile,
    string CodecTag,
    string Level,
    int Width,
    int Height,
    int CodedWidth,
    int CodedHeight,
    string PixelFormat,
    string BitsPerRawSample,
    string FieldOrder,
    string SampleAspectRatio,
    string AverageFrameRate,
    string RealFrameRate,
    string ColorRange,
    string ColorSpace,
    string ColorTransfer,
    string ColorPrimaries,
    string ChromaLocation,
    string ExtradataHash)
{
    public string Display => string.Join(", ",
        $"{CodecName}/{Profile}",
        $"{Width}x{Height}",
        PixelFormat,
        $"fps={AverageFrameRate}");
}

public sealed record RepartSourceM(
    string FilePath,
    string RawJson,
    long TotalFrames,
    long FirstFrame,
    long LastFrame,
    long FileLength,
    long LastWriteUtcTicks)
{
    public string DisplayName => Path.GetFileName(FilePath);
    public long FrameCount => LastFrame >= FirstFrame ? LastFrame - FirstFrame + 1 : 0;

    public bool MatchesCurrentFile()
    {
        try
        {
            FileInfo file = new(FilePath);
            return file.Exists && file.Length == FileLength && file.LastWriteTimeUtc.Ticks == LastWriteUtcTicks;
        }
        catch { return false; }
    }
}

public sealed record RepartOutputSegmentM(
    Guid Id,
    string BaseName,
    long FirstFrame,
    long LastFrame)
{
    public long FrameCount => LastFrame >= FirstFrame ? LastFrame - FirstFrame + 1 : 0;

    public bool Overlaps(RepartOutputSegmentM other) =>
        FirstFrame <= other.LastFrame && other.FirstFrame <= LastFrame;

    public bool IsAdjacentTo(RepartOutputSegmentM other) =>
        LastFrame + 1 == other.FirstFrame || other.LastFrame + 1 == FirstFrame;
}

public sealed record RepartDividerM(
    Guid Id,
    long Frame,
    bool IsLocked);

public sealed record RepartTimelineRangeM(
    Guid? OutputId,
    string BaseName,
    long FirstFrame,
    long LastFrame,
    bool IsUnallocated)
{
    public long FrameCount => LastFrame >= FirstFrame ? LastFrame - FirstFrame + 1 : 0;
}

public sealed class RepartPlanM
{
    public Guid PlanId { get; init; } = Guid.NewGuid();
    public string FfprobePath { get; init; } = string.Empty;
    public string ReferenceRawJson { get; init; } = string.Empty;
    public RepartVideoFormatSignature? FormatSignature { get; init; }
    public int FrameRateNumerator { get; init; }
    public int FrameRateDenominator { get; init; }
    public long TotalFrames { get; init; }
    public List<RepartSourceM> Sources { get; init; } = [];
    public List<RepartOutputSegmentM> Outputs { get; init; } = [];
    public List<RepartDividerM> Dividers { get; init; } = [];

    public bool IsConfigured =>
        Sources.Count > 0 && Outputs.Count > 0 && TotalFrames > 0 &&
        FrameRateNumerator > 0 && FrameRateDenominator > 0;

    public double FrameRate => FrameRateDenominator > 0
        ? (double)FrameRateNumerator / FrameRateDenominator
        : 0d;

    public double TotalSeconds => FrameRateNumerator > 0
        ? (double)TotalFrames * FrameRateDenominator / FrameRateNumerator
        : 0d;

    public RepartPlanM Clone() => new()
    {
        PlanId = PlanId,
        FfprobePath = FfprobePath,
        ReferenceRawJson = ReferenceRawJson,
        FormatSignature = FormatSignature,
        FrameRateNumerator = FrameRateNumerator,
        FrameRateDenominator = FrameRateDenominator,
        TotalFrames = TotalFrames,
        Sources = [.. Sources],
        Outputs = [.. Outputs],
        Dividers = [.. Dividers]
    };

    public IReadOnlyList<RepartTimelineRangeM> BuildTimelineRanges(
        IEnumerable<RepartOutputSegmentM> outputs,
        long totalFrames)
    {
        var ranges = new List<RepartTimelineRangeM>();
        long cursor = 0;
        foreach (RepartOutputSegmentM output in outputs.OrderBy(output => output.FirstFrame))
        {
            if (output.FirstFrame > cursor)
                ranges.Add(new RepartTimelineRangeM(null, string.Empty, cursor, output.FirstFrame - 1, true));
            ranges.Add(new RepartTimelineRangeM(output.Id, output.BaseName, output.FirstFrame, output.LastFrame, false));
            cursor = output.LastFrame + 1;
        }
        if (cursor < totalFrames)
            ranges.Add(new RepartTimelineRangeM(null, string.Empty, cursor, totalFrames - 1, true));
        return ranges;
    }
}
