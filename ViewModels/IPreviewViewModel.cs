namespace OneColumnEncoder.ViewModels;

/// <summary>
/// Fixing image centering issue for the first image rendered in XxxPreviewerPanel
/// </summary>
/// <remarks>
/// ImgPreviewer.xaml.cs cast DataContext to ImgPreviewerVM specifically,
/// so when hosted inside FrameServerPreviewerPanel with a VpyPreviewerVM DataContext,
/// the cast silently returned null.
/// No PropertyChanged subscription was ever established,
/// so QueueFitImage() was never called and the first image rendered at (0,0)
/// </remarks>
public interface IPreviewViewModel : INotifyPropertyChanged
{
    bool IsFitMode { get; }
    bool IsBusy { get; }
    void SetFitMode(bool isFitMode);
    void SetZoomPercent(int percent);
}
