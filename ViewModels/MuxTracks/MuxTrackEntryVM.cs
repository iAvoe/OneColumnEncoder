namespace OneColumnEncoder.ViewModels.MuxTracks;

/// <summary>
/// View model for a single mux track row (source subtitle or imported subtitle file).
/// </summary>
public sealed class MuxTrackEntryVM : BaseVM
{
    private readonly MuxTrackM _model;
    private readonly Action<string> _showError;
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _isRecentlyMoved;
    private string _syncText;
    private System.Windows.Threading.DispatcherTimer? _flashTimer;

    private static readonly DropdownItemM[] Languages =
    [
        new("(none)", isPlaceholder: true) { Tag = null },
        new("eng (English)")    { Tag = "eng" },
        new("zho (Chinese)")    { Tag = "zho" },
        new("jpn (Japanese)")   { Tag = "jpn" },
        new("kor (Korean)")     { Tag = "kor" },
        new("fra (French)")     { Tag = "fra" },
        new("deu (German)")     { Tag = "deu" },
        new("spa (Spanish)")    { Tag = "spa" },
        new("ita (Italian)")    { Tag = "ita" },
        new("por (Portuguese)") { Tag = "por" },
        new("rus (Russian)")    { Tag = "rus" },
        new("ukr (Ukrainian)")  { Tag = "ukr" },
        new("pol (Polish)")     { Tag = "pol" },
        new("ces (Czech)")      { Tag = "ces" },
        new("hun (Hungarian)")  { Tag = "hun" },
        new("ron (Romanian)")   { Tag = "ron" },
        new("nld (Dutch)")      { Tag = "nld" },
        new("swe (Swedish)")    { Tag = "swe" },
        new("dan (Danish)")     { Tag = "dan" },
        new("nor (Norwegian)")  { Tag = "nor" },
        new("fin (Finnish)")    { Tag = "fin" },
        new("ell (Greek)")      { Tag = "ell" },
        new("tur (Turkish)")    { Tag = "tur" },
        new("heb (Hebrew)")     { Tag = "heb" },
        new("ara (Arabic)")     { Tag = "ara" },
        new("fas (Persian)")    { Tag = "fas" },
        new("hin (Hindi)")      { Tag = "hin" },
        new("tha (Thai)")       { Tag = "tha" },
        new("vie (Vietnamese)") { Tag = "vie" },
        new("ind (Indonesian)") { Tag = "ind" },
        new("msa (Malay)")      { Tag = "msa" },
    ];

    /// <summary>
    /// Wires up commands and initializes the language dropdown from the underlying track model.
    /// </summary>
    /// <param name="model">The track model this entry binds to.</param>
    /// <param name="move">Callback to move this entry by a given offset.</param>
    /// <param name="remove">Callback to remove this entry.</param>
    /// <param name="defaultChanged">Callback invoked when the default flag changes.</param>
    /// <param name="showError">Callback used to surface validation errors.</param>
    public MuxTrackEntryVM(MuxTrackM model, Action<MuxTrackEntryVM, int> move, Action<MuxTrackEntryVM> remove, Action<MuxTrackEntryVM, bool> defaultChanged, Action<string> showError)
    {
        _model = model;
        _showError = showError;
        _syncText = model.SyncMilliseconds.ToString(CultureInfo.InvariantCulture);
        MoveUpCommand = new ActionCmd(_ => move(this, -1), _ => CanMoveUp);
        MoveDownCommand = new ActionCmd(_ => move(this, 1), _ => CanMoveDown);
        RemoveCommand = new ActionCmd(_ => remove(this));
        DefaultChanged = defaultChanged;

        LanguageDropdown = new DropdownMenuVM
        {
            SelectionChangedCommand = new ActionCmd(_ => OnLanguageChanged()),
        };
        foreach (DropdownItemM item in Languages)
            LanguageDropdown.Items.Add(item);
        LanguageDropdown.SelectedItem = Languages.FirstOrDefault(
            item => string.Equals(item.Tag as string, model.LanguageCode, StringComparison.OrdinalIgnoreCase))
            ?? Languages[0];
    }

    public MuxTrackM Model => _model;
    public string Name => _model.Name;
    // Imported subtitle files and ffprobe source subtitles both surface their parsed duration here.
    public string DurationText => FormatDuration(_model.DurationSeconds);
    public bool CanRemove => !_model.IsSourceTrack;
    public string SyncText
    {
        get => _syncText;
        set
        {
            if (!SetProperty(ref _syncText, value)) return;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sync))
                _model.SyncMilliseconds = sync;
        }
    }

    /// <summary>
    /// Shows an error if the sync offset text is non-empty but not a valid integer.
    /// </summary>
    public void ValidateSyncText()
    {
        if (!string.IsNullOrWhiteSpace(_syncText) &&
            int.TryParse(_syncText, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            return;
        _showError(MuxLangProvider.Current["MuxTracks.InvalidSync"]);
    }

    public bool IsDefault
    {
        get => _model.IsDefault;
        set
        {
            if (_model.IsDefault == value) return;
            _model.IsDefault = value;
            DefaultChanged(this, value);
            OnPropertyChanged();
        }
    }

    public bool CanMoveUp
    {
        get => _canMoveUp;
        set
        {
            if (SetProperty(ref _canMoveUp, value))
                (MoveUpCommand as BaseCmd)?.OnCanExecuteChanged();
        }
    }

    public bool CanMoveDown
    {
        get => _canMoveDown;
        set
        {
            if (SetProperty(ref _canMoveDown, value))
                (MoveDownCommand as BaseCmd)?.OnCanExecuteChanged();
        }
    }

    public bool IsRecentlyMoved
    {
        get => _isRecentlyMoved;
        private set => SetProperty(ref _isRecentlyMoved, value);
    }

    public static string MoveUpText => LangProviderBase.MoveUpText;
    public static string MoveDownText => LangProviderBase.MoveDownText;
    public static string RemoveText => LangProviderBase.RemoveText;
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand RemoveCommand { get; }
    public DropdownMenuVM LanguageDropdown { get; }
    private Action<MuxTrackEntryVM, bool> DefaultChanged { get; }

    /// <summary>
    /// Persists the selected dropdown language code back to the track model.
    /// </summary>
    private void OnLanguageChanged()
    {
        _model.LanguageCode = LanguageDropdown.SelectedItem?.Tag as string;
    }

    /// <summary>
    /// Formats a duration in seconds as a timestamp, or a localized "unknown" label.
    /// </summary>
    /// <param name="durationSeconds">The duration in seconds, or null/zero.</param>
    /// <returns>A formatted timestamp string, or the unknown-duration label.</returns>
    private static string FormatDuration(double? durationSeconds) =>
        durationSeconds is > 0d
            ? EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(durationSeconds.Value))
            : MuxLangProvider.DurationUnknown;

    /// <summary>
    /// Highlights the row briefly after a move, then clears the highlight.
    /// </summary>
    public void FlashMoved()
    {
        IsRecentlyMoved = true;
        _flashTimer?.Stop();
        _flashTimer =
            new System.Windows.Threading.DispatcherTimer(TimeSpan.FromMilliseconds(600),
            System.Windows.Threading.DispatcherPriority.Normal, (_, _) =>
        {
            IsRecentlyMoved = false;
            _flashTimer?.Stop();
        }, System.Windows.Threading.Dispatcher.CurrentDispatcher);
        _flashTimer.Start();
    }

    /// <summary>
    /// Refreshes language-dependent display strings for this entry.
    /// </summary>
    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(MoveUpText));
        OnPropertyChanged(nameof(MoveDownText));
        OnPropertyChanged(nameof(RemoveText));
    }

    /// <summary>
    /// Notifies the view that the default-track binding may have changed.
    /// </summary>
    public void RefreshDefaultBinding() => OnPropertyChanged(nameof(IsDefault));

    /// <summary>
    /// Stops the flash timer and releases base resources.
    /// </summary>
    public override void Dispose()
    {
        _flashTimer?.Stop();
        _flashTimer = null;
        base.Dispose();
    }
}
