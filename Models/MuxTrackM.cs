using System.IO;

namespace OneColumnEncoder.Models;

/// <summary>
/// An external subtitle stream to add during the final mux step.
/// </summary>
public sealed class MuxTrackM
{
    public string FilePath { get; set; } = string.Empty;
    public bool IsSourceTrack { get; set; }
    public int? SourceStreamIndex { get; set; }
    public int? SourceSubtitleIndex { get; set; }
    public string? DisplayName { get; set; }
    public int SyncMilliseconds { get; set; }
    public string? LanguageCode { get; set; }
    public bool IsDefault { get; set; }
    public bool OriginalIsDefault { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string Name => string.IsNullOrWhiteSpace(DisplayName) ? Path.GetFileName(FilePath) : DisplayName!;
}
