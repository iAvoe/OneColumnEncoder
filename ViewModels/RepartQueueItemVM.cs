using System.Windows.Threading;

namespace OneColumnEncoder.ViewModels;

/// <summary>
/// Queue-editor representation of a committed Repart output segment.
/// </summary>
public sealed class RepartQueueItemVM : BaseVM, IQueueEditorItem
{
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _canRemove;
    private bool _isSelected;
    private bool _isRecentlyMoved;
    private string _p1Text = string.Empty;
    private string _r2Text = string.Empty;
    private string _r3Text = string.Empty;
    private DispatcherTimer? _moveFlashTimer;

    public RepartQueueItemVM(RepartOutputSegmentM model, int frameRateNumerator, int frameRateDenominator)
    {
        Model = model;
        FrameRateNumerator = frameRateNumerator;
        FrameRateDenominator = frameRateDenominator;
        RefreshLanguage();
    }

    public RepartOutputSegmentM Model { get; }
    public int FrameRateNumerator { get; }
    public int FrameRateDenominator { get; }
    public string Name => Model.BaseName;
    public string P1Text => _p1Text;
    public string DisplayR1Text => LangProviderBase.RemoveText;
    public string R2Text => _r2Text;
    public string R3Text => _r3Text;
    public bool R1IsEnabled => _canRemove;
    public bool R2IsEnabled => _canMoveUp;
    public bool R3IsEnabled => _canMoveDown;
    public bool IsCancel => false;
    public bool IsRecentlyMoved
    {
        get => _isRecentlyMoved;
        private set => SetProperty(ref _isRecentlyMoved, value);
    }
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    public bool CanMoveUp
    {
        get => _canMoveUp;
        set
        {
            if (SetProperty(ref _canMoveUp, value))
                OnPropertyChanged(nameof(R2IsEnabled));
        }
    }
    public bool CanMoveDown
    {
        get => _canMoveDown;
        set
        {
            if (SetProperty(ref _canMoveDown, value))
                OnPropertyChanged(nameof(R3IsEnabled));
        }
    }
    public bool CanRemove
    {
        get => _canRemove;
        set
        {
            if (SetProperty(ref _canRemove, value))
                OnPropertyChanged(nameof(R1IsEnabled));
        }
    }
    public long SortSize => Model.FrameCount;
    public string SortName => Name;
    public Guid? OutputId => Model.Id;
    public ICommand? R1Command { get; set; }
    public ICommand? R2Command { get; set; }
    public ICommand? R3Command { get; set; }

    public void RefreshLanguage()
    {
        double start = (double)Model.FirstFrame * FrameRateDenominator / FrameRateNumerator;
        double end = (double)(Model.LastFrame + 1) * FrameRateDenominator / FrameRateNumerator;
        _p1Text = $"{EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(start))} - "
            + $"{EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(end))}  |  {Model.FrameCount:N0} "
            + RepartLangProvider.Current["FrameFormat"];
        _r2Text = LangProviderBase.MoveUpText;
        _r3Text = LangProviderBase.MoveDownText;
        OnPropertyChanged(nameof(P1Text));
        OnPropertyChanged(nameof(R2Text));
        OnPropertyChanged(nameof(R3Text));
    }

    public void FlashMovedHighlight()
    {
        IsRecentlyMoved = true;
        StopMoveFlashTimer();
        _moveFlashTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(600),
            DispatcherPriority.Normal,
            OnMoveFlashTimerTick,
            Dispatcher.CurrentDispatcher);
        _moveFlashTimer.Start();
    }

    private void OnMoveFlashTimerTick(object? sender, EventArgs e)
    {
        IsRecentlyMoved = false;
        StopMoveFlashTimer();
    }

    private void StopMoveFlashTimer()
    {
        if (_moveFlashTimer == null) return;
        _moveFlashTimer.Stop();
        _moveFlashTimer.Tick -= OnMoveFlashTimerTick;
        _moveFlashTimer = null;
    }

    public override void Dispose()
    {
        StopMoveFlashTimer();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
