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

    public RepartDividerItemVM(RepartDividerM model, long totalFrames)
    {
        Model = model;
        Position = totalFrames > 0 ? (double)(model.Frame + 1) / totalFrames : 0d;
    }

    public RepartDividerM Model { get; }
    public long Frame => Model.Frame;
    public bool IsLocked => Model.IsLocked;
    public double Position { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class RepartTimelineSliceVM(
    Guid? outputId,
    string label,
    string tooltip,
    long firstFrame,
    long lastFrame,
    bool isUnallocated,
    int paletteIndex)
{
    public Guid? OutputId { get; } = outputId;
    public string Label { get; } = label;
    public string Tooltip { get; } = tooltip;
    public long FirstFrame { get; } = firstFrame;
    public long LastFrame { get; } = lastFrame;
    public long FrameCount => LastFrame >= FirstFrame ? LastFrame - FirstFrame + 1 : 0;
    public double Weight => Math.Max(1d, FrameCount);
    public bool IsUnallocated { get; } = isUnallocated;
    public int PaletteIndex { get; } = paletteIndex;
}
