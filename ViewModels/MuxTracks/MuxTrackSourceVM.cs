using System.IO;

namespace OneColumnEncoder.ViewModels.MuxTracks;

public sealed class MuxTrackSourceVM : BaseVM
{
    private bool _isSelected;

    public MuxTrackSourceVM(string filePath, IEnumerable<MuxTrackM> tracks)
    {
        FilePath = filePath;
        Tracks = [.. tracks.Select(track => new MuxTrackM
        {
            FilePath = track.FilePath,
            SyncMilliseconds = track.SyncMilliseconds,
            IsDefault = track.IsDefault,
        })];
    }

    public string FilePath { get; }
    public string Name => Path.GetFileName(FilePath);
    public ObservableCollection<MuxTrackM> Tracks { get; }
    public string TrackSummary => Tracks.Count.ToString();
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
}
