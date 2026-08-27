namespace OneColumnEncoder.Components;

/// <summary>
/// A settings row that shows a mode label on the left and three encoder
/// x264/x265/SVT-AV1 auto-mux checkboxes on the right.
/// </summary>
public class AutoMuxRow : Control
{
    static AutoMuxRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AutoMuxRow),
            new FrameworkPropertyMetadata(typeof(AutoMuxRow)));
    }

    public string ModeText
    {
        get => (string)GetValue(ModeTextProperty);
        set => SetValue(ModeTextProperty, value);
    }
    public static readonly DependencyProperty ModeTextProperty =
        DependencyProperty.Register(
            nameof(ModeText),
            typeof(string),
            typeof(AutoMuxRow),
            new PropertyMetadata(string.Empty));

    public bool IsX264
    {
        get => (bool)GetValue(IsX264Property);
        set => SetValue(IsX264Property, value);
    }
    public static readonly DependencyProperty IsX264Property =
        DependencyProperty.Register(
            nameof(IsX264),
            typeof(bool),
            typeof(AutoMuxRow),
            new PropertyMetadata(false));

    public bool IsX265
    {
        get => (bool)GetValue(IsX265Property);
        set => SetValue(IsX265Property, value);
    }
    public static readonly DependencyProperty IsX265Property =
        DependencyProperty.Register(
            nameof(IsX265),
            typeof(bool),
            typeof(AutoMuxRow),
            new PropertyMetadata(false));

    public bool IsSvtAv1
    {
        get => (bool)GetValue(IsSvtAv1Property);
        set => SetValue(IsSvtAv1Property, value);
    }
    public static readonly DependencyProperty IsSvtAv1Property =
        DependencyProperty.Register(
            nameof(IsSvtAv1),
            typeof(bool),
            typeof(AutoMuxRow),
            new PropertyMetadata(false));
}