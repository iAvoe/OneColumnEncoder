using OneColumnEncoder.Models;
using OneColumnEncoder.Pipeline;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartOutputItemVM : BaseVM
{
    private bool _isSelected;

    public RepartOutputItemVM(RepartOutputSegmentM model, int frameRateNumerator, int frameRateDenominator)
    {
        Model = model;
        double start = (double)model.FirstFrame * frameRateDenominator / frameRateNumerator;
        double end = (double)(model.LastFrame + 1) * frameRateDenominator / frameRateNumerator;
        P1Text = $"{EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(start))} - {EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(end))}  |  {model.FrameCount:N0}f";
    }

    public RepartOutputSegmentM Model { get; }
    public string Name => Model.BaseName;
    public string P1Text { get; }
    public string FrameRangeText => $"{Model.FirstFrame:N0} - {Model.LastFrame:N0}";
    public string FrameCountText => $"{Model.FrameCount:N0} {RepartLangProvider.Current["FrameFormat"]}";
    public string DisplayR1Text => string.Empty;
    public string R2Text => string.Empty;
    public string R3Text => string.Empty;
    public bool R1IsEnabled => false;
    public bool R2IsEnabled => false;
    public bool R3IsEnabled => false;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class RepartDividerItemVM : BaseVM
{
    private bool _isSelected;
    private RepartDividerM _model;
    private double _position;

    public RepartDividerItemVM(RepartDividerM model, long totalFrames)
    {
        _model = model;
        _position = GetPosition(model, totalFrames);
    }

    public RepartDividerM Model => _model;
    public long Frame => _model.Frame;
    public bool IsLocked => _model.IsLocked;
    public double Position
    {
        get => _position;
        private set => SetProperty(ref _position, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public void Update(RepartDividerM model, long totalFrames)
    {
        _model = model;
        Position = GetPosition(model, totalFrames);
        OnPropertyChanged(nameof(Model));
        OnPropertyChanged(nameof(Frame));
        OnPropertyChanged(nameof(IsLocked));
    }

    private static double GetPosition(RepartDividerM model, long totalFrames) =>
        totalFrames > 0 ? (double)(model.Frame + 1) / totalFrames : 0d;
}
