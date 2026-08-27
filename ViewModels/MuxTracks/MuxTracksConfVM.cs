using System.IO;
using static OneColumnEncoder.Json.JsonElementHelper;

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
        Func<string, string?> getSourceFfprobeJson,
        Action<string, IReadOnlyList<MuxTrackM>> applyTracks)
    {
        _closeAction = closeAction;
        _applyTracks = applyTracks;
        _tracksBySource = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in sourcePaths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tracksBySource[path] = BuildInitialTracks(path, getTracks(path), getSourceFfprobeJson(path));
            SourceItems.Add(new MuxTrackSourceVM(path, _tracksBySource[path]));
        }

        ShowSidebar = SourceItems.Count > 1;

        RemoveTrackCommand = new ActionCmd(item => RemoveTrack(item as MuxTrackEntryVM));
        MoveTrackUpCommand = new ActionCmd(item => MoveTrack(item as MuxTrackEntryVM, -1));
        MoveTrackDownCommand = new ActionCmd(item => MoveTrack(item as MuxTrackEntryVM, 1));
        AddSubtitleCommand = new ActionCmd(_ => BrowseSubtitle(), _ => SelectedSource != null);
        CancelConfirmButtons = ButtonGroupVM.CreateTwoButton(
            Lang.Cancel, Lang.Confirm,
            new ActionCmd(_ => _closeAction()), new ActionCmd(_ => Confirm(), _ => CanConfirm));

        if (SourceItems.Count > 0)
            SelectedSource = SourceItems[0];
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public static MuxLangProvider Lang => MuxLangProvider.Current;
    public static string WindowTitle => MuxLangProvider.WindowTitle;
    public static string SidebarTitle => Lang["MuxTracks.QueueSources"];
    public static string SubtitleHeader => Lang["MuxTracks.SubtitleHeader"];
    public static string AddSubtitleText => Lang["MuxTracks.AddSubtitle"];
    public static string EmptyText => Lang["MuxTracks.Empty"];
    public string CurrentSourceTitle => SelectedSource?.Name ?? string.Empty;
    public static string CurrentSourceDurationText => string.Empty;
    public ObservableCollection<MuxTrackSourceVM> SourceItems { get; } = [];
    public ObservableCollection<MuxTrackEntryVM> Tracks { get; } = [];
    public ButtonGroupVM CancelConfirmButtons { get; }
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
            RefreshTrackList();
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentSourceTitle));
            (AddSubtitleCommand as BaseCmd)?.OnCanExecuteChanged();
        }
    }

    private void BrowseSubtitle()
    {
        if (SelectedSource == null) return;
        OpenFileDialog dialog = new()
        {
            Title = AddSubtitleText,
            Filter = Lang["MuxTracks.FileFilter"],
            InitialDirectory = Path.GetDirectoryName(SelectedSource.FilePath) ?? string.Empty,
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

        List<MuxTrackM> tracks = GetCurrentTracks();
        tracks.Add(new MuxTrackM { FilePath = dialog.FileName });
        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackList();
        RefreshSourceSummary();
    }

    private void RemoveTrack(MuxTrackEntryVM? entry)
    {
        if (entry == null || SelectedSource == null) return;
        List<MuxTrackM> tracks = GetCurrentTracks();
        tracks.Remove(entry.Model);
        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackList();
        RefreshSourceSummary();
    }

    private void MoveTrack(MuxTrackEntryVM? entry, int offset)
    {
        if (entry == null || SelectedSource == null) return;
        int oldIndex = Tracks.IndexOf(entry);
        int newIndex = oldIndex + offset;
        if (oldIndex < 0 || newIndex < 0 || newIndex >= Tracks.Count) return;
        List<MuxTrackM> tracks = GetCurrentTracks();
        (tracks[oldIndex], tracks[newIndex]) = (tracks[newIndex], tracks[oldIndex]);
        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackList();
        Tracks[newIndex].FlashMoved();
    }

    private List<MuxTrackM> GetCurrentTracks() =>
        SelectedSource == null ? [] : [.. _tracksBySource[SelectedSource.FilePath].Select(Clone)];

    private void SaveCurrentTracks()
    {
        if (_selectedSource == null) return;
        _tracksBySource[_selectedSource.FilePath] = [.. Tracks.Select(entry => Clone(entry.Model))];
    }

    private void RefreshTrackList()
    {
        foreach (MuxTrackEntryVM entry in Tracks) entry.Dispose();
        Tracks.Clear();
        if (SelectedSource == null) return;
        foreach (MuxTrackM track in _tracksBySource[SelectedSource.FilePath])
        {
            MuxTrackEntryVM entry = new(track, MoveTrack, RemoveTrack, OnDefaultChanged);
            Tracks.Add(entry);
        }
        RefreshMoveStates();
    }

    private void RefreshMoveStates()
    {
        for (int i = 0; i < Tracks.Count; i++)
        {
            Tracks[i].CanMoveUp = i > 0;
            Tracks[i].CanMoveDown = i < Tracks.Count - 1;
        }
    }

    private void OnDefaultChanged(MuxTrackEntryVM changed, bool isDefault)
    {
        if (!isDefault || SelectedSource == null) return;
        foreach (MuxTrackEntryVM entry in Tracks)
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
        foreach (MuxTrackEntryVM entry in Tracks) entry.RefreshLanguage();
        CancelConfirmButtons.B2_1Text = Lang.Cancel;
        CancelConfirmButtons.B2_2Text = Lang.Confirm;
    }

    private static MuxTrackM Clone(MuxTrackM track) => new()
    {
        FilePath = track.FilePath,
        IsSourceTrack = track.IsSourceTrack,
        SourceStreamIndex = track.SourceStreamIndex,
        SourceSubtitleIndex = track.SourceSubtitleIndex,
        DisplayName = track.DisplayName,
        SyncMilliseconds = track.SyncMilliseconds,
        IsDefault = track.IsDefault,
        IsForced = track.IsForced,
    };

    private static List<MuxTrackM> BuildInitialTracks(string sourcePath, IReadOnlyList<MuxTrackM> savedTracks, string? ffprobeJson)
    {
        List<MuxTrackM> tracks = [.. DetectSourceSubtitleTracks(sourcePath, ffprobeJson)];
        foreach (MuxTrackM detected in tracks)
        {
            MuxTrackM? saved = savedTracks.FirstOrDefault(track =>
                track.IsSourceTrack &&
                track.SourceStreamIndex == detected.SourceStreamIndex);
            if (saved == null) continue;

            detected.SyncMilliseconds = saved.SyncMilliseconds;
            detected.IsDefault = saved.IsDefault;
            detected.IsForced = saved.IsForced;
        }

        tracks.AddRange(savedTracks.Where(track => !track.IsSourceTrack).Select(Clone));
        return tracks;
    }

    private static IEnumerable<MuxTrackM> DetectSourceSubtitleTracks(string sourcePath, string? ffprobeJson)
    {
        if (string.IsNullOrWhiteSpace(ffprobeJson)) yield break;

        using JsonDocument document = JsonDocument.Parse(ffprobeJson);
        if (!document.RootElement.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
            yield break;

        int subtitleIndex = 0;
        foreach (JsonElement stream in streams.EnumerateArray())
        {
            if (!string.Equals(TryGetString(stream, "codec_type"), "subtitle", StringComparison.OrdinalIgnoreCase))
                continue;
            if (!TryGetInt(stream, "index", out int streamIndex))
                continue;

            yield return new MuxTrackM
            {
                FilePath = sourcePath,
                IsSourceTrack = true,
                SourceStreamIndex = streamIndex,
                SourceSubtitleIndex = subtitleIndex,
                DisplayName = BuildSourceSubtitleName(stream, subtitleIndex),
                IsDefault = TryGetDisposition(stream, "default"),
                IsForced = TryGetDisposition(stream, "forced"),
            };
            subtitleIndex++;
        }
    }

    private static string BuildSourceSubtitleName(JsonElement stream, int subtitleIndex)
    {
        string? title = TryGetTag(stream, "title");
        string? language = TryGetTag(stream, "language");
        string prefix = $"Source subtitle {subtitleIndex + 1}";

        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(language))
            return $"{prefix} - {language} - {title}";
        if (!string.IsNullOrWhiteSpace(title)) return $"{prefix} - {title}";
        if (!string.IsNullOrWhiteSpace(language)) return $"{prefix} - {language}";
        return prefix;
    }

    private static bool TryGetDisposition(JsonElement stream, string name)
    {
        return stream.TryGetProperty("disposition", out JsonElement disposition) &&
            disposition.ValueKind == JsonValueKind.Object &&
            TryGetInt(disposition, name, out int value) &&
            value != 0;
    }

    private static string? TryGetTag(JsonElement stream, string name)
    {
        return stream.TryGetProperty("tags", out JsonElement tags) &&
            tags.ValueKind == JsonValueKind.Object &&
            tags.TryGetProperty(name, out JsonElement value) &&
            value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        foreach (MuxTrackEntryVM entry in Tracks) entry.Dispose();
        base.Dispose();
    }
}
