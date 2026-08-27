using System.IO;

namespace OneColumnEncoder.ViewModels.MuxTracks;

public sealed class MuxTracksConfVM : BaseVM
{
    private readonly Action _closeAction;
    private readonly Action<string, IReadOnlyList<MuxTrackM>> _applyTracks;
    private readonly Dictionary<string, List<MuxTrackM>> _tracksBySource;
    private MuxTrackSourceVM? _selectedSource;

    public MuxTracksConfVM(
        Action closeAction,
        IEnumerable<string> sourcePaths,
        Func<string, IReadOnlyList<MuxTrackM>> getTracks,
        Action<string, IReadOnlyList<MuxTrackM>> applyTracks)
    {
        _closeAction = closeAction;
        _applyTracks = applyTracks;
        _tracksBySource = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in sourcePaths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tracksBySource[path] = [.. getTracks(path).Select(Clone)];
            SourceItems.Add(new MuxTrackSourceVM(path, _tracksBySource[path]));
        }

        ShowSidebar = SourceItems.Count > 1;

        RemoveTrackCommand = new ActionCmd(item => RemoveTrack(item as MuxTrackEntryVM));
        MoveTrackUpCommand = new ActionCmd(item => MoveTrack(item as MuxTrackEntryVM, -1));
        MoveTrackDownCommand = new ActionCmd(item => MoveTrack(item as MuxTrackEntryVM, 1));
        AddAudioCommand = new ActionCmd(_ => BrowseTrack(MuxTrackKind.Audio), _ => SelectedSource != null);
        AddSubtitleCommand = new ActionCmd(_ => BrowseTrack(MuxTrackKind.Subtitle), _ => SelectedSource != null);
        CancelConfirmButtons = ButtonGroupVM.CreateTwoButton(
            Lang["MuxTracks.Cancel"], Lang["MuxTracks.Confirm"],
            new ActionCmd(_ => _closeAction()), new ActionCmd(_ => Confirm(), _ => CanConfirm));

        if (SourceItems.Count > 0)
            SelectedSource = SourceItems[0];
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public static MuxTracksConfModalLangProvider Lang => MuxTracksConfModalLangProvider.Current;
    public static string WindowTitle => MuxTracksConfModalLangProvider.WindowTitle;
    public static string SidebarTitle => Lang["MuxTracks.QueueSources"];
    public static string AudioHeader => Lang["MuxTracks.AudioHeader"];
    public static string SubtitleHeader => Lang["MuxTracks.SubtitleHeader"];
    public static string AddAudioText => Lang["MuxTracks.AddAudio"];
    public static string AddSubtitleText => Lang["MuxTracks.AddSubtitle"];
    public static string EmptyText => Lang["MuxTracks.Empty"];
    public string CurrentSourceTitle => SelectedSource?.Name ?? string.Empty;
    public static string CurrentSourceDurationText => string.Empty;
    public ObservableCollection<MuxTrackSourceVM> SourceItems { get; } = [];
    public ObservableCollection<MuxTrackEntryVM> AudioTracks { get; } = [];
    public ObservableCollection<MuxTrackEntryVM> SubtitleTracks { get; } = [];
    public ButtonGroupVM CancelConfirmButtons { get; }
    public ActionCmd AddAudioCommand { get; }
    public ActionCmd AddSubtitleCommand { get; }
    public ActionCmd RemoveTrackCommand { get; }
    public ActionCmd MoveTrackUpCommand { get; }
    public ActionCmd MoveTrackDownCommand { get; }
    private bool _showSidebar;
    public bool ShowSidebar
    {
        get => _showSidebar;
        private set => SetProperty(ref _showSidebar, value);
    }
    public bool CanConfirm => SourceItems.Count > 0;

    public MuxTrackSourceVM? SelectedSource
    {
        get => _selectedSource;
        set
        {
            if (_selectedSource == value) return;
            SaveCurrentTracks();
            _selectedSource = value;
            RefreshTrackLists();
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentSourceTitle));
            (AddAudioCommand as BaseCmd)?.OnCanExecuteChanged();
            (AddSubtitleCommand as BaseCmd)?.OnCanExecuteChanged();
        }
    }

    private void BrowseTrack(MuxTrackKind kind)
    {
        if (SelectedSource == null) return;
        OpenFileDialog dialog = new()
        {
            Title = kind == MuxTrackKind.Audio ? AddAudioText : AddSubtitleText,
            Filter = Lang["MuxTracks.FileFilter"],
            InitialDirectory = Path.GetDirectoryName(SelectedSource.FilePath) ?? string.Empty,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

        List<MuxTrackM> tracks = GetCurrentTracks();
        tracks.Add(new MuxTrackM { FilePath = dialog.FileName, Kind = kind });
        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackLists();
        RefreshSourceSummary();
    }

    private void RemoveTrack(MuxTrackEntryVM? entry)
    {
        if (entry == null || SelectedSource == null) return;
        List<MuxTrackM> tracks = GetCurrentTracks();
        tracks.Remove(entry.Model);
        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackLists();
        RefreshSourceSummary();
    }

    private void MoveTrack(MuxTrackEntryVM? entry, int offset)
    {
        if (entry == null || SelectedSource == null) return;
        ObservableCollection<MuxTrackEntryVM> list = entry.Model.Kind == MuxTrackKind.Audio ? AudioTracks : SubtitleTracks;
        int oldIndex = list.IndexOf(entry);
        int newIndex = oldIndex + offset;
        if (oldIndex < 0 || newIndex < 0 || newIndex >= list.Count) return;
        List<MuxTrackM> tracks = GetCurrentTracks();
        MuxTrackM[] sameKind = [.. tracks.Where(track => track.Kind == entry.Model.Kind)];
        (sameKind[oldIndex], sameKind[newIndex]) = (sameKind[newIndex], sameKind[oldIndex]);
        int kindIndex = 0;
        for (int i = 0; i < tracks.Count; i++)
            if (tracks[i].Kind == entry.Model.Kind) tracks[i] = sameKind[kindIndex++];
        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackLists();
        list[newIndex].FlashMoved();
    }

    private List<MuxTrackM> GetCurrentTracks() =>
        SelectedSource == null ? [] : [.. _tracksBySource[SelectedSource.FilePath].Select(Clone)];

    private void SaveCurrentTracks()
    {
        if (_selectedSource == null) return;
        _tracksBySource[_selectedSource.FilePath] = [.. AudioTracks.Concat(SubtitleTracks).Select(entry => Clone(entry.Model))];
    }

    private void RefreshTrackLists()
    {
        foreach (MuxTrackEntryVM entry in AudioTracks.Concat(SubtitleTracks)) entry.Dispose();
        AudioTracks.Clear();
        SubtitleTracks.Clear();
        if (SelectedSource == null) return;
        foreach (MuxTrackM track in _tracksBySource[SelectedSource.FilePath])
        {
            MuxTrackEntryVM entry = new(track, MoveTrack, RemoveTrack, OnDefaultChanged);
            (track.Kind == MuxTrackKind.Audio ? AudioTracks : SubtitleTracks).Add(entry);
        }
        RefreshMoveStates(AudioTracks);
        RefreshMoveStates(SubtitleTracks);
    }

    private static void RefreshMoveStates(ObservableCollection<MuxTrackEntryVM> tracks)
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].CanMoveUp = i > 0;
            tracks[i].CanMoveDown = i < tracks.Count - 1;
        }
    }

    private void OnDefaultChanged(MuxTrackEntryVM changed, bool isDefault)
    {
        if (!isDefault || SelectedSource == null) return;
        foreach (MuxTrackEntryVM entry in (changed.Model.Kind == MuxTrackKind.Audio ? AudioTracks : SubtitleTracks))
        {
            if (ReferenceEquals(entry, changed) || !entry.IsDefault) continue;
            entry.Model.IsDefault = false;
            entry.RefreshDefaultBinding();
        }
    }

    private void RefreshSourceSummary()
    {
        SelectedSource?.RefreshTracks(_tracksBySource[SelectedSource.FilePath]);
    }

    private void Confirm()
    {
        SaveCurrentTracks();
        foreach (MuxTrackSourceVM source in SourceItems)
            _applyTracks(source.FilePath, [.. _tracksBySource[source.FilePath].Select(Clone)]);
        _closeAction();
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(string.Empty);
        foreach (MuxTrackEntryVM entry in AudioTracks.Concat(SubtitleTracks)) entry.RefreshLanguage();
        CancelConfirmButtons.B2_1Text = Lang["MuxTracks.Cancel"];
        CancelConfirmButtons.B2_2Text = Lang["MuxTracks.Confirm"];
    }

    private static MuxTrackM Clone(MuxTrackM track) => new()
    {
        FilePath = track.FilePath,
        Kind = track.Kind,
        SyncMilliseconds = track.SyncMilliseconds,
        IsDefault = track.IsDefault,
    };

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        foreach (MuxTrackEntryVM entry in AudioTracks.Concat(SubtitleTracks)) entry.Dispose();
        base.Dispose();
    }
}
