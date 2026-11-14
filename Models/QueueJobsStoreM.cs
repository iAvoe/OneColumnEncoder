using System.IO;

namespace OneColumnEncoder.Models;

/// <summary>
/// Persisted queue job list and queue state.
/// </summary>
/// <remarks>The path should be config directory, since this is a config file, not a history log</remarks>
public class QueueJobsStoreM : SaveLoadBase<QueueJobsStoreM>
{
    public List<QueueJobItemM> Jobs { get; set; } = [];
    public bool IsBatchActive { get; set; }
    public int MaxConcurrent { get; set; } = 1;

    protected override string FilePath =>
        Path.Combine(GetConfigDirectory(), "queue-store.json");
}
