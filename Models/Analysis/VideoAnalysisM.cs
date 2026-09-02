namespace OneColumnEncoder.Models.Analysis;

/// <summary>
/// What needs to be find out about the video source
/// If anything misses, and the encoding mode requires it:
/// - a failure route or case should be reached
/// - Start Encoding button should remain disabled
/// - The user should be informed about this
///
/// The model only holds the analysis *results* (representative JSON, batch JSON,
/// aggregate frame count) plus provenance (which route produced them, which tool/path).
/// The actual source file sets per route live in their route state owners
/// (VideoSrcQueueState / VideoSrcConcatState / VideoSrcRepartState), not here.
/// </summary>
public class VideoAnalysisM
{
    /// <summary>
    /// Route that produced the current analysis. Null when no analysis has been recorded.
    /// </summary>
    public SrcRouteKind? Route { get; set; }

    /// <summary>
    /// Representative file path the analysis refers to:
    /// - Single → the source file
    /// - Queue → the reference candidate
    /// - Concat → the first fragment
    /// - Repart → the first source.
    /// </summary>
    public string SrcPath { get; set; } = string.Empty;

    /// <summary>
    /// ffprobe exe path used. Kept here (instead of a separate model) to help invalidate stale analysis upon tool path changes.
    /// It is tool configuration, not an analysis result.
    /// </summary>
    public string FFprobePath { get; set; } = string.Empty;

    /// <summary>
    /// Representative raw ffprobe JSON. Pre-req of Single mode; for Queue/Concat/Repart it is the
    /// reference/first-fragment/source JSON that drives previews and FilterScribe helpers.
    /// </summary>
    public string RawJson { get; set; } = string.Empty;

    /// <summary>
    /// Serialized <see cref="RawAnalysisBatchM"/> of every analyzed file. Pre-req of Queue and Concat
    /// modes; Repart stores its source set here as well so CopyRawAnalysisCmd can copy the full batch.
    /// </summary>
    public string BatchRawJson { get; set; } = string.Empty;

    /// <summary>
    /// Aggregate frame count over the whole batch. Pre-req of Concat and Repart mode; 0 when unknown.
    /// </summary>
    public long ConcatTotalFrames { get; set; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(RawJson);

    /// <summary>
    /// Whether the recorded analysis satisfies the preconditions of the given route.
    /// Multi-file routes additionally require the batch payload; Repart additionally requires a frame count.
    /// </summary>
    public bool IsCompleteFor(SrcRouteKind route) => route switch
    {
        SrcRouteKind.Queue => !IsEmpty && !string.IsNullOrWhiteSpace(BatchRawJson),
        SrcRouteKind.Concat => !IsEmpty && !string.IsNullOrWhiteSpace(BatchRawJson),
        SrcRouteKind.Repart => !IsEmpty && ConcatTotalFrames > 0,
        _ => !IsEmpty
    };

    /// <summary>
    /// Recomputes <see cref="ConcatTotalFrames"/> from <see cref="BatchRawJson"/>. Needed after a
    /// source revision rewrites the batch payload.
    /// </summary>
    public void UpdateConcatTotalFramesFromQueueJson() =>
        ConcatTotalFrames = FFProbeSrcRevisionModel.CalculateTotalFrames(BatchRawJson);

    public void Clear()
    {
        Route = null;
        SrcPath = string.Empty;
        FFprobePath = string.Empty;
        RawJson = string.Empty;
        BatchRawJson = string.Empty;
        ConcatTotalFrames = 0;
    }
}
