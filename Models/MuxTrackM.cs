using System.IO;

namespace OneColumnEncoder.Models;

/// <summary>
/// An external subtitle stream to add during the final mux step.
/// </summary>
public sealed class MuxTrackM
{
    public string FilePath { get; set; } = string.Empty;
    public int SyncMilliseconds { get; set; }
    public bool IsDefault { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string Name => Path.GetFileName(FilePath);
}
