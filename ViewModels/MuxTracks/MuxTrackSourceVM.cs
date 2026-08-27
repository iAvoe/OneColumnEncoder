using System.IO;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.ViewModels.MuxTracks;

public sealed class MuxTrackSourceVM : BaseVM
{
    private bool _isSelected;

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
            IsDefault = track.IsDefault,
            IsForced = track.IsForced,
        })];
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

    public void RefreshTracks(IEnumerable<MuxTrackM> tracks)
    {
        Tracks.Clear();
        foreach (MuxTrackM track in tracks) Tracks.Add(track);
        OnPropertyChanged(nameof(TrackSummary));
    }

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

    private static string FormatDuration(double? durationSeconds)
    {
        if (durationSeconds is not > 0) return string.Empty;
        TimeSpan ts = TimeSpan.FromSeconds(durationSeconds.Value);
        return ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");
    }
}
