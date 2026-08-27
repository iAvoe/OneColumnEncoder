namespace OneColumnEncoder.ViewModels.MuxTracks;

public sealed class MuxTrackEntryVM : BaseVM
{
    private readonly MuxTrackM _model;
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _isRecentlyMoved;
    private string _syncText;
    private System.Windows.Threading.DispatcherTimer? _flashTimer;

    public MuxTrackEntryVM(MuxTrackM model, Action<MuxTrackEntryVM, int> move, Action<MuxTrackEntryVM> remove, Action<MuxTrackEntryVM, bool> defaultChanged)
    {
        _model = model;
        _syncText = model.SyncMilliseconds.ToString(CultureInfo.InvariantCulture);
        MoveUpCommand = new ActionCmd(_ => move(this, -1), _ => CanMoveUp);
        MoveDownCommand = new ActionCmd(_ => move(this, 1), _ => CanMoveDown);
        RemoveCommand = new ActionCmd(_ => remove(this));
        DefaultChanged = defaultChanged;
    }

    public MuxTrackM Model => _model;
    public string Name => _model.Name;
    public string DurationText => MuxTracksConfModalLangProvider.Current["MuxTracks.DurationUnknown"];
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

    public string MoveUpText => LangProviderBase.MoveUpText;
    public string MoveDownText => LangProviderBase.MoveDownText;
    public string RemoveText => LangProviderBase.RemoveText;
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand RemoveCommand { get; }
    private Action<MuxTrackEntryVM, bool> DefaultChanged { get; }

    public void FlashMoved()
    {
        IsRecentlyMoved = true;
        _flashTimer?.Stop();
        _flashTimer = new System.Windows.Threading.DispatcherTimer(TimeSpan.FromMilliseconds(600), System.Windows.Threading.DispatcherPriority.Normal, (_, _) =>
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
