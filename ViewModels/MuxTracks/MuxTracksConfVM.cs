using System.IO;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.ViewModels.MuxTracks;

public sealed class MuxTracksConfVM : BaseVM
{
    private static readonly Dictionary<string, string> Iso6391To6392B = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = "eng", ["zh"] = "zho", ["ja"] = "jpn", ["ko"] = "kor",
        ["fr"] = "fra", ["de"] = "deu", ["es"] = "spa", ["it"] = "ita",
        ["pt"] = "por", ["ru"] = "rus", ["uk"] = "ukr", ["pl"] = "pol",
        ["cs"] = "ces", ["hu"] = "hun", ["ro"] = "ron", ["nl"] = "nld",
        ["sv"] = "swe", ["da"] = "dan", ["no"] = "nor", ["fi"] = "fin",
        ["el"] = "ell", ["tr"] = "tur", ["he"] = "heb", ["ar"] = "ara",
        ["fa"] = "fas", ["hi"] = "hin", ["th"] = "tha", ["vi"] = "vie",
        ["id"] = "ind", ["ms"] = "msa",
        ["chi"] = "zho", ["fre"] = "fra", ["ger"] = "deu",
        ["dut"] = "nld", ["gre"] = "ell", ["ice"] = "isl",
    };

    private readonly Action _closeAction;
    private readonly Action<string, IReadOnlyList<MuxTrackM>> _applyTracks;
    private readonly Action<string> _showError;
    private readonly Dictionary<string, List<MuxTrackM>> _tracksBySource;
    private readonly Dictionary<string, List<MuxTrackM>> _initialTracksBySource;
    private MuxTrackSourceVM? _selectedSource;

    public MuxTracksConfVM(
        Action closeAction,
        IEnumerable<string> sourcePaths,
        Func<string, IReadOnlyList<MuxTrackM>> getTracks,
        IReadOnlyDictionary<string, string?> ffprobeJsonByPath,
        Action<string, IReadOnlyList<MuxTrackM>> applyTracks,
        Action<string> showError)
    {
        _closeAction = closeAction;
        _applyTracks = applyTracks;
        _showError = showError;
        _tracksBySource = new(StringComparer.OrdinalIgnoreCase);
        _initialTracksBySource = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in sourcePaths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ffprobeJsonByPath.TryGetValue(path, out string? ffprobeJson);
            _tracksBySource[path] = BuildInitialTracks(path, getTracks(path), ffprobeJson);
            _initialTracksBySource[path] = [.. _tracksBySource[path].Select(Clone)];
            SourceItems.Add(new MuxTrackSourceVM(path, _tracksBySource[path], ffprobeJson));
        }

        ShowSidebar = SourceItems.Count > 1;

        RemoveTrackCommand = new ActionCmd(item => RemoveTrack(item as MuxTrackEntryVM));
        MoveTrackUpCommand = new ActionCmd(item => MoveTrack(item as MuxTrackEntryVM, -1));
        MoveTrackDownCommand = new ActionCmd(item => MoveTrack(item as MuxTrackEntryVM, 1));
        AddSubtitleCommand = new ActionCmd(_ => BrowseSubtitle(), _ => SelectedSource != null);
        BottomButtons = ButtonGroupVM.CreateThreeButton(
            AddSubtitleText, Lang.Cancel, Lang.Confirm,
            AddSubtitleCommand,
            new ActionCmd(_ => _closeAction()),
            new ActionCmd(_ => Confirm(), _ => CanConfirm));

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
    public static string CannotDeleteSrcSubsHint => Lang["Hint.CannotDeleteSrcSubs"];
    public string CurrentSourceTitle => SelectedSource?.Name ?? string.Empty;
    public string CurrentSourceDurationText => SelectedSource?.TrackSummary ?? string.Empty;
    public ObservableCollection<MuxTrackSourceVM> SourceItems { get; } = [];
    public ObservableCollection<MuxTrackEntryVM> Tracks { get; } = [];
    public ButtonGroupVM BottomButtons { get; }
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
        List<MuxTrackM> tracks = _tracksBySource[SelectedSource.FilePath];
        tracks.Remove(entry.Model);
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
            MuxTrackEntryVM entry = new(track, MoveTrack, RemoveTrack, OnDefaultChanged, _showError);
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
        {
            List<MuxTrackM> current = _tracksBySource[source.FilePath];
            List<MuxTrackM> initial = _initialTracksBySource[source.FilePath];
            if (TracksDiffer(current, initial))
                _applyTracks(source.FilePath, [.. current.Select(Clone)]);
        }
        _closeAction();
    }

    private static bool TracksDiffer(List<MuxTrackM> a, List<MuxTrackM> b)
    {
        if (a.Count != b.Count) return true;
        for (int i = 0; i < a.Count; i++)
        {
            MuxTrackM x = a[i], y = b[i];
            if (x.IsSourceTrack != y.IsSourceTrack ||
                x.SourceStreamIndex != y.SourceStreamIndex ||
                x.SyncMilliseconds != y.SyncMilliseconds ||
                !string.Equals(x.LanguageCode, y.LanguageCode, StringComparison.OrdinalIgnoreCase) ||
                x.IsDefault != y.IsDefault ||
                !string.Equals(x.FilePath, y.FilePath, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(string.Empty);
        foreach (MuxTrackEntryVM entry in Tracks) entry.RefreshLanguage();
        BottomButtons.B3_1Text = AddSubtitleText;
        BottomButtons.B3_2Text = Lang.Cancel;
        BottomButtons.B3_3Text = Lang.Confirm;
    }

    private static MuxTrackM Clone(MuxTrackM track) => new()
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
        OriginalIsDefault = track.OriginalIsDefault,
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
            detected.LanguageCode = saved.LanguageCode;
            detected.IsDefault = saved.IsDefault;
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

            bool isOriginalDefault = TryGetDisposition(stream, "default");
            string? lang = TryGetTag(stream, "language");
            if (lang != null && Iso6391To6392B.TryGetValue(lang, out string? mapped))
                lang = mapped;
            yield return new MuxTrackM
            {
                FilePath = sourcePath,
                IsSourceTrack = true,
                SourceStreamIndex = streamIndex,
                SourceSubtitleIndex = subtitleIndex,
                DisplayName = BuildSourceSubtitleName(stream, subtitleIndex),
                LanguageCode = lang,
                DurationSeconds = TryGetSubtitleDurationSeconds(document.RootElement, stream),
                IsDefault = isOriginalDefault,
                OriginalIsDefault = isOriginalDefault,
            };
            subtitleIndex++;
        }
    }

    private static double? TryGetSubtitleDurationSeconds(JsonElement root, JsonElement stream)
    {
        double? streamDuration = TryGetDouble(stream, "duration");
        if (streamDuration is > 0d) return streamDuration;

        long? durationTs = TryGetLong(stream, "duration_ts");
        if (durationTs is > 0 && TryGetString(stream, "time_base") is string timeBase)
        {
            double? secondsPerTick = ParseFraction(timeBase);
            if (secondsPerTick is > 0d)
                return durationTs.Value * secondsPerTick.Value;
        }

        string? tagDuration = TryGetTag(stream, "DURATION");
        if (TryParseDurationTag(tagDuration, out double tagSeconds)) return tagSeconds;

        return root.TryGetProperty("format", out JsonElement format)
            ? TryGetDouble(format, "duration")
            : null;
    }

    private static bool TryParseDurationTag(string? text, out double seconds)
    {
        seconds = 0d;
        if (string.IsNullOrWhiteSpace(text)) return false;

        string[] parts = text.Split(':');
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int hours)) return false;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes)) return false;
        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double secs)) return false;

        seconds = hours * 3600d + minutes * 60d + secs;
        return seconds > 0d;
    }

    private static double? ParseFraction(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        string[] parts = text.Split('/');
        if (parts.Length != 2) return null;
        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double num)) return null;
        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double den) || den <= 0d) return null;
        return num / den;
    }

    private static string BuildSourceSubtitleName(JsonElement stream, int subtitleIndex)
    {
        string? title = TryGetTag(stream, "title");
        string? language = TryGetTag(stream, "language");
        string prefix = $"Source sub #{subtitleIndex + 1}";

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
