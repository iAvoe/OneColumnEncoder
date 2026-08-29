namespace OneColumnEncoder.ViewModels.MuxTracks;

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
        new("English")      { Tag = "eng" },
        new("Chinese")      { Tag = "zho" },
        new("Japanese")     { Tag = "jpn" },
        new("Korean")       { Tag = "kor" },
        new("French")       { Tag = "fra" },
        new("German")       { Tag = "deu" },
        new("Spanish")      { Tag = "spa" },
        new("Italian")      { Tag = "ita" },
        new("Portuguese")   { Tag = "por" },
        new("Russian")      { Tag = "rus" },
        new("Ukrainian")    { Tag = "ukr" },
        new("Polish")       { Tag = "pol" },
        new("Czech")        { Tag = "ces" },
        new("Hungarian")    { Tag = "hun" },
        new("Romanian")     { Tag = "ron" },
        new("Dutch")        { Tag = "nld" },
        new("Swedish")      { Tag = "swe" },
        new("Danish")       { Tag = "dan" },
        new("Norwegian")    { Tag = "nor" },
        new("Finnish")      { Tag = "fin" },
        new("Greek")        { Tag = "ell" },
        new("Turkish")      { Tag = "tur" },
        new("Hebrew")       { Tag = "heb" },
        new("Arabic")       { Tag = "ara" },
        new("Persian")      { Tag = "fas" },
        new("Hindi")        { Tag = "hin" },
        new("Thai")         { Tag = "tha" },
        new("Vietnamese")   { Tag = "vie" },
        new("Indonesian")   { Tag = "ind" },
        new("Malay")        { Tag = "msa" },
    ];

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

    public void ValidateSyncText()
    {
        if (!string.IsNullOrWhiteSpace(_syncText) &&
            int.TryParse(_syncText, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            return;
        _syncText = "0";
        _model.SyncMilliseconds = 0;
        OnPropertyChanged(nameof(SyncText));
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

    private void OnLanguageChanged()
    {
        _model.LanguageCode = LanguageDropdown.SelectedItem?.Tag as string;
    }

    private static string FormatDuration(double? durationSeconds) =>
        durationSeconds is > 0d
            ? EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(durationSeconds.Value))
            : MuxLangProvider.DurationUnknown;

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

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(MoveUpText));
        OnPropertyChanged(nameof(MoveDownText));
        OnPropertyChanged(nameof(RemoveText));
    }

    public void RefreshDefaultBinding() => OnPropertyChanged(nameof(IsDefault));

    public override void Dispose()
    {
        _flashTimer?.Stop();
        _flashTimer = null;
        base.Dispose();
    }
}
