namespace OneColumnEncoder.ViewModels;

public sealed class ProgressVM : BaseVM
{
    private string _progressTitle = string.Empty;
    private string _statusText = string.Empty;
    private double _progressValue;
    private bool _isProgressTrackingAvailable;
    private bool _isEncodingActive;

    public string ProgressTitle
    {
        get => _progressTitle;
        set => SetProperty(ref _progressTitle, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            double next = Math.Clamp(value, 0d, 100d);
            if (!SetProperty(ref _progressValue, next)) return;
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    public bool IsProgressTrackingAvailable
    {
        get => _isProgressTrackingAvailable;
        set
        {
            if (!SetProperty(ref _isProgressTrackingAvailable, value)) return;
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    public bool IsEncodingActive
    {
        get => _isEncodingActive;
        set => SetProperty(ref _isEncodingActive, value);
    }

    public string ProgressText => IsProgressTrackingAvailable
        ? ProgressValue is > 0d and < 1d
            ? $"{ProgressValue:F2}%"
            : $"{ProgressValue:F0}%"
        : string.Empty;
}
