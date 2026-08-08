using System.IO;
using System.Windows.Threading;

namespace OneColumnEncoder.ViewModels;

public class QueueJobItemVM(QueueJobItemM model) : BaseVM
{
    private readonly EncodingPipelineRequest? _request = DeserializeRequest(model.SerializedRequest);
    private readonly QueueJobItemM _model = model;
    private bool _isSidebarSelected;
    private bool _canMoveUp;
    private bool _canMoveDown;
    private bool _isRecentlyMoved;
    private DispatcherTimer? _moveFlashTimer;

    public QueueJobItemM Model => _model;
    public EncodingPipelineRequest? Request => _request;
    public EncodingPipelineCommand? Command => DeserializeCommand(_model.SerializedCommand);
    public string JobId => _model.JobId;
    public bool IsRepartOutput =>
        _request?.MuxMode == EncodingMuxMode.VideoOnly &&
        _request.Clip != null &&
        _request.IsConcatMode == true;
    public string Name => IsRepartOutput
        ? Path.GetFileName(_model.OutputPath) ?? _model.OutputPath
        : Path.GetFileName(_model.SourcePath) ?? _model.SourcePath;
    public string P1Text
    {
        get => GetFrameCountText();
    }

    public string P1TooltipText => _model.ErrorMessage ?? P1Text;

    public string DisplayR1Text => QueueSidebarLangProvider.Current.QueueItemRemoveText;
    public string R2Text => QueueSidebarLangProvider.Current.QueueItemMoveUpText;
    public string R3Text => QueueSidebarLangProvider.Current.QueueItemMoveDownText;
    public bool R1IsEnabled => _model.Status == "Pending";
    public bool R2IsEnabled => _model.Status == "Pending" && _canMoveUp;
    public bool R3IsEnabled => _model.Status == "Pending" && _canMoveDown;

    public bool IsSelected => _isSidebarSelected || _model.Status == "Encoding";
    public bool IsCancel => _model.Status == "Failed";
    public static bool IsReal => true;
    public static bool EnableRealCheck => false;
    public static bool IsEnabled => true;

    public bool IsRecentlyMoved
    {
        get => _isRecentlyMoved;
        private set
        {
            if (!SetProperty(ref _isRecentlyMoved, value)) return;
            OnPropertyChanged(nameof(IsRecentlyMoved));
        }
    }

    public ICommand? R1Command { get; set; }
    public ICommand? R2Command { get; set; }
    public ICommand? R3Command { get; set; }

    public int UpstreamPid
    {
        get => _model.UpstreamPid;
        set { _model.UpstreamPid = value; OnPropertyChanged(); }
    }

    public int EncoderPid
    {
        get => _model.EncoderPid;
        set { _model.EncoderPid = value; OnPropertyChanged(); }
    }

    public bool IsSidebarSelected
    {
        get => _isSidebarSelected;
        set
        {
            if (!SetProperty(ref _isSidebarSelected, value)) return;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public string Status
    {
        get => _model.Status;
        set
        {
            if (_model.Status != value)
            {
                _model.Status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(DisplayR1Text));
                OnPropertyChanged(nameof(R2Text));
                OnPropertyChanged(nameof(R3Text));
                OnPropertyChanged(nameof(R1IsEnabled));
                OnPropertyChanged(nameof(R2IsEnabled));
                OnPropertyChanged(nameof(R3IsEnabled));
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(IsCancel));
            }
        }
    }

    public int ProgressPercent
    {
        get => _model.ProgressPercent;
        set
        {
            if (_model.ProgressPercent != value)
            {
                _model.ProgressPercent = value;
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }
    }

    public void RefreshBindings()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(P1Text));
        OnPropertyChanged(nameof(P1TooltipText));
        OnPropertyChanged(nameof(DisplayR1Text));
        OnPropertyChanged(nameof(R2Text));
        OnPropertyChanged(nameof(R3Text));
        OnPropertyChanged(nameof(R1IsEnabled));
        OnPropertyChanged(nameof(R2IsEnabled));
        OnPropertyChanged(nameof(R3IsEnabled));
        OnPropertyChanged(nameof(IsSelected));
        OnPropertyChanged(nameof(IsCancel));
        OnPropertyChanged(nameof(Name));
    }

    public void FlashMovedHighlight()
    {
        IsRecentlyMoved = true;
        StopMoveFlashTimer();
        _moveFlashTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(600), DispatcherPriority.Normal, OnMoveFlashTimerTick, Dispatcher.CurrentDispatcher);
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

    public void SetMoveButtonAvailability(bool canMoveUp, bool canMoveDown)
    {
        if (_canMoveUp != canMoveUp)
        {
            _canMoveUp = canMoveUp;
            OnPropertyChanged(nameof(R2IsEnabled));
        }

        if (_canMoveDown != canMoveDown)
        {
            _canMoveDown = canMoveDown;
            OnPropertyChanged(nameof(R3IsEnabled));
        }
    }

    private static EncodingPipelineRequest? DeserializeRequest(string serializedRequest)
    {
        if (string.IsNullOrWhiteSpace(serializedRequest)) return null;
        try
        {
            return JsonSerializer.Deserialize<EncodingPipelineRequest>(serializedRequest);
        }
        catch
        {
            return null;
        }
    }

    private static EncodingPipelineCommand? DeserializeCommand(string serializedCommand)
    {
        if (string.IsNullOrWhiteSpace(serializedCommand)) return null;
        try
        {
            return JsonSerializer.Deserialize<EncodingPipelineCommand>(serializedCommand);
        }
        catch
        {
            return null;
        }
    }

    #region Job Display Queries
    private string GetFrameCountText()
    {
        long? frameCount = _request?.SourceFfprobeJson is { Length: > 0 }
            ? EncodingPipeline.GetExpectedOutputFrames(_request)
            : null;

        if (IsRepartOutput && _request?.Clip is EncodingClipRequest clip)
        {
            string start = clip.StartTime ?? clip.FirstFrame?.ToString() ?? "?";
            string end = clip.EndTime ?? clip.LastFrame?.ToString() ?? "?";
            return frameCount is > 0
                ? $"{start} - {end} | {frameCount:N0}f"
                : $"{start} - {end}";
        }

        if (frameCount is > 0)
            return $"{new ClipRangeSelectorLangProvider(UILangProvider.Current.LanguageCode).SummaryTotalFramesLabel}: {frameCount:N0}";

        return "N/A";
    }
    #endregion

    public override void Dispose()
    {
        StopMoveFlashTimer();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
