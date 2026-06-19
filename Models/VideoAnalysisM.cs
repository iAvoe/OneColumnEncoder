namespace OneColumnEncoder.Models
{
    public class VideoAnalysisM
    {
        public string SourcePath { get; set; } = string.Empty;
        public string FfprobePath { get; set; } = string.Empty;
        public string RawJson { get; set; } = string.Empty;
        public string QueueRawJson { get; set; } = string.Empty;

        public void Clear()
        {
            SourcePath = string.Empty;
            FfprobePath = string.Empty;
            RawJson = string.Empty;
            QueueRawJson = string.Empty;
        }
    }
}
