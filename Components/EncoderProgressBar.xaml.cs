namespace OneColumnEncoder.Components;

public partial class EncoderProgressBar : UserControl
{
    public static readonly DependencyProperty ProgressTitleProperty =
        DependencyProperty.Register(nameof(ProgressTitle), typeof(string), typeof(EncoderProgressBar), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ProgressValueProperty =
        DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(EncoderProgressBar), new PropertyMetadata(0.0));

    public static readonly DependencyProperty ProgressTextProperty =
        DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(EncoderProgressBar), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsProgressTrackingAvailableProperty =
        DependencyProperty.Register(nameof(IsProgressTrackingAvailable), typeof(bool), typeof(EncoderProgressBar), new PropertyMetadata(false));

    public static readonly DependencyProperty IsEncodingActiveProperty =
        DependencyProperty.Register(nameof(IsEncodingActive), typeof(bool), typeof(EncoderProgressBar), new PropertyMetadata(false));

    public string ProgressTitle
    {
        get => (string)GetValue(ProgressTitleProperty);
        set => SetValue(ProgressTitleProperty, value);
    }

    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    public string ProgressText
    {
        get => (string)GetValue(ProgressTextProperty);
        set => SetValue(ProgressTextProperty, value);
    }

    public bool IsProgressTrackingAvailable
    {
        get => (bool)GetValue(IsProgressTrackingAvailableProperty);
        set => SetValue(IsProgressTrackingAvailableProperty, value);
    }

    public bool IsEncodingActive
    {
        get => (bool)GetValue(IsEncodingActiveProperty);
        set => SetValue(IsEncodingActiveProperty, value);
    }

    public EncoderProgressBar()
    {
        InitializeComponent();
    }
}
