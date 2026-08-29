using System.IO;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.ViewModels.MuxTracks;

/// <summary>
/// View model for a single source row in the mux tracks modal, holding its track collection.
/// </summary>
public sealed class MuxTrackSourceVM : BaseVM
{
    private bool _isSelected;

    /// <summary>
    /// Initializes a source row, cloning its tracks and computing the source duration.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="tracks">The track models to clone into this source.</param>
    /// <param name="ffprobeJson">ffprobe JSON used to derive the source duration.</param>
    public MuxTrackSourceVM(string filePath, IEnumerable<MuxTrackM> tracks, string? ffprobeJson)
    {
        FilePath = filePath;
        Tracks = [.. tracks.Select(track => new MuxTrackM
        {
            FilePath = track.FilePath,
            IsSourceTrack = track.IsSourceTrack,
            SourceStreamIndex = track.SourceStreamIndex,
            SourceSubtitleIndex = track.SourceSubtitleIndex,
            DisplayName = track.DisplayName,
            SyncMilliseconds = track.SyncMilliseconds,
            LanguageCode = track.LanguageCode,
            DurationSeconds = track.DurationSeconds,
            IsDefault = track.IsDefault,
        })];
        // This summary uses the container-level ffprobe duration for the source file itself.
        DurationText = FormatDuration(ParseDurationFromFfprobeJson(ffprobeJson));
    }

    public string FilePath { get; }
    public string Name => Path.GetFileName(FilePath);
    public ObservableCollection<MuxTrackM> Tracks { get; }
    public string DurationText { get; }
    public string TrackSummary => DurationText.Length > 0
        ? $"{Tracks.Count} tracks | {DurationText}"
        : $"{Tracks.Count} tracks";
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Replaces the source's track collection and updates its summary.
    /// </summary>
    /// <param name="tracks">The new track models to populate.</param>
    public void RefreshTracks(IEnumerable<MuxTrackM> tracks)
    {
        Tracks.Clear();
        foreach (MuxTrackM track in tracks) Tracks.Add(track);
        OnPropertyChanged(nameof(TrackSummary));
    }

    /// <summary>
    /// Extracts the container or stream duration from an ffprobe JSON string.
    /// </summary>
    /// <param name="rawJson">The ffprobe JSON, or null/empty.</param>
    /// <returns>Duration in seconds, or null when undeterminable.</returns>
    private static double? ParseDurationFromFfprobeJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(rawJson);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("streams", out JsonElement streams) &&
                streams.ValueKind == JsonValueKind.Array &&
                streams.GetArrayLength() > 0)
            {
                double? fromStream = TryGetDouble(streams[0], "duration");
                if (fromStream is > 0) return fromStream;
            }
            if (root.TryGetProperty("format", out JsonElement format))
                return TryGetDouble(format, "duration");
            return null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Formats a duration in seconds as a compact "h:mm:ss" or "m:ss" string.
    /// </summary>
    /// <param name="durationSeconds">The duration in seconds, or null/zero.</param>
    /// <returns>A formatted duration string, or empty when invalid.</returns>
    private static string FormatDuration(double? durationSeconds)
    {
        if (durationSeconds is not > 0) return string.Empty;
        TimeSpan ts = TimeSpan.FromSeconds(durationSeconds.Value);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");
    }
}
