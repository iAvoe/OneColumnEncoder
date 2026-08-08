namespace OneColumnEncoder.Components;

public partial class ValidationActionGroup : UserControl
{
    public static readonly DependencyProperty CardMarginProperty =
        DependencyProperty.Register(
            nameof(CardMargin),
            typeof(Thickness),
            typeof(ValidationActionGroup),
            new PropertyMetadata(new Thickness(5, 5, 5, 0)));

    public static readonly DependencyProperty MiddleContentProperty =
        DependencyProperty.Register(
            nameof(MiddleContent),
            typeof(object),
            typeof(ValidationActionGroup),
            new PropertyMetadata(null));

    public Thickness CardMargin
    {
        get => (Thickness)GetValue(CardMarginProperty);
        set => SetValue(CardMarginProperty, value);
    }

    public object? MiddleContent
    {
        get => GetValue(MiddleContentProperty);
        set => SetValue(MiddleContentProperty, value);
    }

    public ValidationActionGroup()
    {
        InitializeComponent();
    }
}
