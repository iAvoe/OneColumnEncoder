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
        new("eng - English")      { Tag = "eng" },
        new("zho - Chinese")      { Tag = "zho" },
        new("jpn - Japanese")     { Tag = "jpn" },
        new("kor - Korean")       { Tag = "kor" },
        new("fra - French")       { Tag = "fra" },
        new("deu - German")       { Tag = "deu" },
        new("spa - Spanish")      { Tag = "spa" },
        new("ita - Italian")      { Tag = "ita" },
        new("por - Portuguese")   { Tag = "por" },
        new("rus - Russian")      { Tag = "rus" },
        new("ukr - Ukrainian")    { Tag = "ukr" },
        new("pol - Polish")       { Tag = "pol" },
        new("ces - Czech")        { Tag = "ces" },
        new("hun - Hungarian")    { Tag = "hun" },
        new("ron - Romanian")     { Tag = "ron" },
        new("nld - Dutch")        { Tag = "nld" },
        new("swe - Swedish")      { Tag = "swe" },
        new("dan - Danish")       { Tag = "dan" },
        new("nor - Norwegian")    { Tag = "nor" },
        new("fin - Finnish")      { Tag = "fin" },
        new("ell - Greek")        { Tag = "ell" },
        new("tur - Turkish")      { Tag = "tur" },
        new("heb - Hebrew")       { Tag = "heb" },
        new("ara - Arabic")       { Tag = "ara" },
        new("fas - Persian")      { Tag = "fas" },
        new("hin - Hindi")        { Tag = "hin" },
        new("tha - Thai")         { Tag = "tha" },
        new("vie - Vietnamese")   { Tag = "vie" },
        new("ind - Indonesian")   { Tag = "ind" },
        new("msa - Malay")        { Tag = "msa" },
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
    public static string DurationText => MuxLangProvider.DurationUnknown;
    public bool CanRemove => !_model.IsSourceTrack;
    public string SyncText
    {
        get => _syncText;
        set
        {
            if (!SetProperty(ref _syncText, value)) return;
            if (string.IsNullOrWhiteSpace(value))
            {
                _showError(MuxLangProvider.Current["MuxTracks.InvalidSync"]);
                return;
            }
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sync))
                _model.SyncMilliseconds = sync;
            else
                _showError(MuxLangProvider.Current["MuxTracks.InvalidSync"]);
        }
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
