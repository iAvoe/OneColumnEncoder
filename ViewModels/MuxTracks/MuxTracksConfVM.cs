using System.IO;
using static OneColumnEncoder.Models.JsonProviderM;

namespace OneColumnEncoder.ViewModels.MuxTracks;

/// <summary>
/// View model for the mux tracks configuration modal that manages subtitles per source.
/// </summary>
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
    private readonly Func<string, bool> _confirmNoDefaultSubtitle;
    private readonly Dictionary<string, List<MuxTrackM>> _tracksBySource;
    private readonly Dictionary<string, List<MuxTrackM>> _initialTracksBySource;
    private readonly HashSet<string> _sourcesWithDroppedMissingExternalTracks;
    private MuxTrackSourceVM? _selectedSource;

    /// <summary>
    /// Builds the per-source track lists, sidebar, and bottom button commands for the modal
    /// </summary>
    /// <param name="closeAction">Callback that closes the modal.</param>
    /// <param name="sourcePaths">Source file paths to configure tracks for.</param>
    /// <param name="getTracks">Resolves saved tracks for a given source path.</param>
    /// <param name="ffprobeJsonByPath">ffprobe JSON keyed by source path.</param>
    /// <param name="applyTracks">Callback to persist edited tracks back to a source.</param>
    /// <param name="showError">Callback used to surface errors.</param>
    public MuxTracksConfVM(
        Action closeAction,
        IEnumerable<string> sourcePaths,
        Func<string, IReadOnlyList<MuxTrackM>> getTracks,
        IReadOnlyDictionary<string, string?> ffprobeJsonByPath,
        Action<string, IReadOnlyList<MuxTrackM>> applyTracks,
        Action<string> showError,
        Func<string, bool> confirmNoDefaultSubtitle)
    {
        _closeAction = closeAction;
        _applyTracks = applyTracks;
        _showError = showError;
        _confirmNoDefaultSubtitle = confirmNoDefaultSubtitle;
        _tracksBySource = new(StringComparer.OrdinalIgnoreCase);
        _initialTracksBySource = new(StringComparer.OrdinalIgnoreCase);
        _sourcesWithDroppedMissingExternalTracks = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in sourcePaths.Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            ffprobeJsonByPath.TryGetValue(path, out string? ffprobeJson);
            IReadOnlyList<MuxTrackM> savedTracks = getTracks(path);
            if (savedTracks.Any(track => !track.IsSourceTrack && !File.Exists(track.FilePath)))
                _sourcesWithDroppedMissingExternalTracks.Add(path);

            _tracksBySource[path] = BuildInitialTracks(path, savedTracks, ffprobeJson);
            _initialTracksBySource[path] = [.. _tracksBySource[path].Select(Clone)];
            SourceItems.Add(new MuxTrackSourceVM(path, _tracksBySource[path], ffprobeJson));
        }

        ShowSidebar = SourceItems.Count > 1;

        RemoveTrackCommand = new ActionCmd(item => RemoveTrack(item as MuxTrackEntryVM));
        AddSubtitleCommand = new ActionCmd(_ => BrowseSubtitle(), _ => SelectedSource != null);
        BottomButtons = ButtonGroupVM.CreateThreeButton(
            AddSubtitleText, Lang.Cancel, Lang.Confirm,
            AddSubtitleCommand,
            new ActionCmd(_ => _closeAction()),
            new ActionCmd(_ => Confirm(), _ => CanConfirm));

        if (SourceItems.Count > 0) SelectedSource = SourceItems[0];
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public static MuxLangProvider Lang => MuxLangProvider.Current;
    public static string WindowTitle => MuxLangProvider.WindowTitle;
    public static string SidebarTitle => Lang["MuxTracks.QueueSources"];
    public static string SubtitleHeader => Lang["MuxTracks.SubtitleHeader"];
    public static string AddSubtitleText => Lang["MuxTracks.AddSubtitle"];
    public static string EmptyText => Lang["MuxTracks.NoSub"];
    public static string CannotDeleteSrcSubsHint => Lang["Hint.CannotDeleteSrcSubs"];
    public static string FFmpegSubtitleDefaultHint => Lang["Hint.FFmpegSubtitleDefault"];
    public string CurrentSourceTitle => SelectedSource?.Name ?? string.Empty;
    public string CurrentSourceDurationText => SelectedSource?.TrackSummary ?? string.Empty;
    public ObservableCollection<MuxTrackSourceVM> SourceItems { get; } = [];
    public ObservableCollection<MuxTrackEntryVM> Tracks { get; } = [];
    public ButtonGroupVM BottomButtons { get; }
    public ActionCmd AddSubtitleCommand { get; }
    public ActionCmd RemoveTrackCommand { get; }
    private bool _showSidebar;
    public bool ShowSidebar
    {
        get => _showSidebar;
        private set => SetProperty(ref _showSidebar, value);
    }
    public bool CanConfirm => SourceItems.Count > 0;
    private bool _hasNoSubtitles = true;
    public bool HasNoSubtitles
    {
        get => _hasNoSubtitles;
        private set => SetProperty(ref _hasNoSubtitles, value);
    }

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

    /// <summary>
    /// Opens a file dialog and appends a validated external subtitle track to the selected source
    /// </summary>
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
            Multiselect = true,
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

        List<MuxTrackM> tracks = GetCurrentTracks();
        bool addedAny = false;
        foreach (string fileName in dialog.FileNames)
        {
            // External subtitle imports are validated by parsing cue timestamps, not by ffprobe
            TimeSpan? duration = SubtitleHelper.GetDuration(fileName);
            if (duration == null)
            {
                _showError($"Unable to import subtitle file: {Path.GetFileName(fileName)}");
                continue;
            }

            tracks.Add(new MuxTrackM
            {
                FilePath = fileName,
                DurationSeconds = duration.Value.TotalSeconds,
            });
            addedAny = true;
        }

        if (!addedAny) return;

        _tracksBySource[SelectedSource.FilePath] = tracks;
        RefreshTrackList();
        RefreshSourceSummary();
    }

    /// <summary>
    /// Removes the given track entry from the selected source's track list
    /// </summary>
    /// <param name="entry">The entry to remove, or null to ignore.</param>
    private void RemoveTrack(MuxTrackEntryVM? entry)
    {
        if (entry == null || SelectedSource == null) return;
        List<MuxTrackM> tracks = _tracksBySource[SelectedSource.FilePath];
        tracks.Remove(entry.Model);
        RefreshTrackList();
        RefreshSourceSummary();
    }

    /// <summary>
    /// Returns a cloned snapshot of the selected source's current tracks
    /// </summary>
    /// <returns>A new list of cloned track models, or empty if none selected.</returns>
    private List<MuxTrackM> GetCurrentTracks() =>
        SelectedSource == null ? [] : [.. _tracksBySource[SelectedSource.FilePath].Select(Clone)];

    /// <summary>
    /// Writes the live track list back into the per-source working dictionary
    /// </summary>
    private void SaveCurrentTracks()
    {
        if (_selectedSource == null) return;
        _tracksBySource[_selectedSource.FilePath] = [.. Tracks.Select(entry => Clone(entry.Model))];
    }

    /// <summary>
    /// Rebuilds the visible entry list for the selected source.
    /// </summary>
    private void RefreshTrackList()
    {
        foreach (MuxTrackEntryVM entry in Tracks) entry.Dispose();
        Tracks.Clear();
        if (SelectedSource == null)
        {
            HasNoSubtitles = true;
            return;
        }
        foreach (MuxTrackM track in _tracksBySource[SelectedSource.FilePath])
        {
            MuxTrackEntryVM entry = new(track, RemoveTrack, OnDefaultChanged);
            Tracks.Add(entry);
        }
        HasNoSubtitles = Tracks.Count == 0;
    }

    /// <summary>
    /// Ensures only one track per source is flagged as the default
    /// </summary>
    /// <param name="changed">The entry whose default flag changed.</param>
    /// <param name="isDefault">True when the entry became the default.</param>
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

    /// <summary>
    /// Pushes the updated track list into the source's summary display
    /// </summary>
    private void RefreshSourceSummary() =>
        SelectedSource?.RefreshTracks(_tracksBySource[SelectedSource.FilePath]);

    /// <summary>
    /// Applies edited tracks to changed sources and closes the modal.
    /// </summary>
    private void Confirm()
    {
        SaveCurrentTracks();

        List<(MuxTrackSourceVM Source, List<MuxTrackM> Current)> changedSources = [];
        foreach (MuxTrackSourceVM source in SourceItems)
        {
            List<MuxTrackM> current = _tracksBySource[source.FilePath];
            if (_sourcesWithDroppedMissingExternalTracks.Contains(source.FilePath)
                || TracksDiffer(current, _initialTracksBySource[source.FilePath]))
                changedSources.Add((source, current));
        }

        List<(MuxTrackSourceVM Source, int OrigDefId, int FirstSubId)> needsConfirmation = [];
        foreach (var (source, current) in changedSources)
        {
            bool userChanged = TracksDiffer(current, _initialTracksBySource[source.FilePath]);
            bool staleTracksOnly = _sourcesWithDroppedMissingExternalTracks.Contains(source.FilePath)
                && !userChanged;
            if (!staleTracksOnly && !current.Any(track => track.IsDefault))
                needsConfirmation.Add((source, source.OriginalSrcDefSubTrackId, source.FirstSrcSubTrackId));
        }

        if (needsConfirmation.Count > 0)
        {
            List<string> blocks = [];
            foreach (var (source, origDefId, firstSubId) in needsConfirmation)
            {
                blocks.Add(origDefId >= 0
                    ? string.Join(Environment.NewLine,
                        string.Format(Lang["MuxTracks.NoDefault.SourceLine"], source.CurrentSourceTitle),
                        Lang["MuxTracks.NoDefault.NoDefaultMarked"],
                        string.Format(Lang["MuxTracks.NoDefault.OrigDefTrackId"], origDefId),
                        Lang["MuxTracks.NoDefault.ClearAndContinue"])
                    : string.Join(Environment.NewLine,
                        string.Format(Lang["MuxTracks.NoDefault.SourceLine"], source.CurrentSourceTitle),
                        Lang["MuxTracks.NoDefault.NoDefaultMarked"],
                        string.Format(Lang["MuxTracks.NoDefault.OrigDefTrackId"], origDefId),
                        firstSubId >= 0
                            ? string.Format(Lang["MuxTracks.NoDefault.FirstSubTrackId"], firstSubId)
                            : Lang["MuxTracks.NoDefault.NoSourceSubs"],
                        Lang["MuxTracks.NoDefault.ConsiderAdding"]));
            }
            if (!_confirmNoDefaultSubtitle(string.Join(Environment.NewLine + Environment.NewLine, blocks)))
                return;
        }

        foreach (var (source, current) in changedSources)
            _applyTracks(source.FilePath, [.. current.Select(Clone)]);
        _closeAction();
    }

    /// <summary>
    /// Compares two track lists for any meaningful field difference
    /// </summary>
    /// <param name="a">First track list.</param>
    /// <param name="b">Sec track list.</param>
    /// <returns>True when the lists differ in count or any compared field.</returns>
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

    /// <summary>
    /// Refreshes all localized strings and button labels when the UI language changes
    /// </summary>
    private void OnLanguageChanged()
    {
        OnPropertyChanged(string.Empty);
        foreach (MuxTrackEntryVM entry in Tracks) entry.RefreshLanguage();
        BottomButtons.B3_1Text = AddSubtitleText;
        BottomButtons.B3_2Text = Lang.Cancel;
        BottomButtons.B3_3Text = Lang.Confirm;
    }

    /// <summary>
    /// Produces a deep copy of a track model for working snapshots
    /// </summary>
    /// <param name="track">The track model to clone.</param>
    /// <returns>A new track model with copied field values.</returns>
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

    /// <summary>
    /// Merges detected source subtitles with saved external tracks for a source
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="savedTracks">Previously saved tracks to merge in.</param>
    /// <param name="ffprobeJson">ffprobe JSON for the source, or null.</param>
    /// <returns>The combined initial track list for the source.</returns>
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

        tracks.AddRange(savedTracks
            .Where(track => !track.IsSourceTrack && File.Exists(track.FilePath))
            .Select(Clone));
        return tracks;
    }

    /// <summary>
    /// Yields source subtitle tracks parsed from an ffprobe JSON document
    /// </summary>
    /// <param name="sourcePath">The source file path used as the track file path.</param>
    /// <param name="ffprobeJson">ffprobe JSON, or null/empty to skip.</param>
    /// <returns>An enumerable of detected source subtitle track models.</returns>
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
                // Source subtitles keep their ffprobe-derived duration so rows can show a real length.
                DurationSeconds = TryGetSubtitleDurationSeconds(document.RootElement, stream),
                IsDefault = isOriginalDefault,
                OriginalIsDefault = isOriginalDefault,
            };
            subtitleIndex++;
        }
    }

    /// <summary>
    /// Resolves a subtitle duration from stream/time-base/tag/format fallbacks.
    /// </summary>
    /// <param name="root">The ffprobe document root element.</param>
    /// <param name="stream">The subtitle stream element.</param>
    /// <returns>Duration in seconds, or null when undeterminable.</returns>
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

    /// <summary>
    /// Parses an "hh:mm:ss" duration tag into seconds.
    /// </summary>
    /// <param name="text">The duration tag text.</param>
    /// <param name="seconds">Receives the parsed duration in seconds.</param>
    /// <returns>True when parsing succeeded and the value is positive.</returns>
    private static bool TryParseDurationTag(string? text, out double seconds)
    {
        seconds = 0d;
        if (string.IsNullOrWhiteSpace(text)) return false;

        string[] parts = text.Split(':');
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int hh)) return false;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int mm)) return false;
        if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double ss)) return false;

        seconds = hh * 3600d + mm * 60d + ss;
        return seconds > 0d;
    }

    /// <summary>
    /// Parses a "numerator/denominator" fraction string into a double
    /// </summary>
    /// <param name="text">The fraction string, or null/empty.</param>
    /// <returns>The parsed value, or null when invalid.</returns>
    private static double? ParseFraction(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        string[] parts = text.Split('/');
        if (parts.Length != 2) return null;
        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double numerator)) return null;
        if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double denominator) || denominator <= 0d)
            return null;
        return numerator / denominator;
    }

    /// <summary>
    /// Builds a display name for a source subtitle from its title/language metadata
    /// </summary>
    /// <param name="stream">The subtitle stream element.</param>
    /// <param name="subtitleIndex">Zero-based index of the subtitle stream.</param>
    /// <returns>The composed display name string.</returns>
    private static string BuildSourceSubtitleName(JsonElement stream, int subtitleIndex)
    {
        string? title = TryGetTag(stream, "title");
        string? language = TryGetTag(stream, "language");
        string prefix = $"*Source #{subtitleIndex + 1}";

        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(language))
            return $"{prefix} - {language} - {title}";
        if (!string.IsNullOrWhiteSpace(title)) return $"{prefix} - {title}";
        if (!string.IsNullOrWhiteSpace(language)) return $"{prefix} - {language}";
        return prefix;
    }

    /// <summary>
    /// Reads a boolean disposition flag from an ffprobe stream element
    /// </summary>
    /// <param name="stream">The stream element.</param>
    /// <param name="name">The disposition flag name.</param>
    /// <returns>True when the flag is present and non-zero.</returns>
    private static bool TryGetDisposition(JsonElement stream, string name)
    {
        return stream.TryGetProperty("disposition", out JsonElement disposition) &&
            disposition.ValueKind == JsonValueKind.Object &&
            TryGetInt(disposition, name, out int value) &&
            value != 0;
    }

    /// <summary>
    /// Reads a string tag value from an ffprobe stream's tags object
    /// </summary>
    /// <param name="stream">The stream element.</param>
    /// <param name="name">The tag name.</param>
    /// <returns>The tag string, or null when absent.</returns>
    private static string? TryGetTag(JsonElement stream, string name)
    {
        return stream.TryGetProperty("tags", out JsonElement tags) &&
            tags.ValueKind == JsonValueKind.Object &&
            tags.TryGetProperty(name, out JsonElement value) &&
            value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    /// <summary>
    /// Detaches the language-changed handler and disposes track entries
    /// </summary>
    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        foreach (MuxTrackEntryVM entry in Tracks) entry.Dispose();
        base.Dispose();
    }
}
