namespace OneColumnEncoder.Models;

/// <summary>
/// What needs to be find out about the video source
/// If anything misses, and the encoding mode requires it:
/// - a failure route or case should be reached
/// - Start Encoding button should remain disabled
/// - The user should be informed about this
/// </summary>
public class VideoAnalysisM
{
    public string SrcPath { get; set; } = string.Empty; // Path to the source video file
    // TODO: Wait, why QueueSourcePath, ConcatSourcePath aren't recorded here?
    public string FFprobePath { get; set; } = string.Empty; // Technically should be in a different model, but this modal is small enough to carry it
    public string RawJson { get; set; } = string.Empty; // Pre-req of Single mode
    public string QueueRawJson { get; set; } = string.Empty; // Pre-req of Queue mode (TODO: double check is this required by Repart mode?)
    public long ConcatTotalFrames { get; set; } // Pre-req of Concat and Repart mode

    public void Clear()
    {
        SrcPath = string.Empty;
        FFprobePath = string.Empty;
        RawJson = string.Empty;
        QueueRawJson = string.Empty;
        ConcatTotalFrames = 0;
    }
}
