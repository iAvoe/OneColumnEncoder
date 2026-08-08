namespace OneColumnEncoder.Components;

/// <summary>
/// Interaction logic for ToolsImportCard.xaml
/// </summary>
public partial class ToolsImportCard : UserControl
{
    public ToolsImportCard()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ToggleCommandProperty =
        DependencyProperty.Register(
            nameof(ToggleCommand),
            typeof(ICommand),
            typeof(ToolsImportCard),
            new PropertyMetadata(null));

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public static readonly DependencyProperty ToggleTagProperty =
        DependencyProperty.Register(
            nameof(ToggleTag),
            typeof(bool),
            typeof(ToolsImportCard),
            new PropertyMetadata(false));

    public bool ToggleTag
    {
        get => (bool)GetValue(ToggleTagProperty);
        set => SetValue(ToggleTagProperty, value);
    }

    public static readonly DependencyProperty ToggleToolTipProperty =
        DependencyProperty.Register(
            nameof(ToggleToolTip),
            typeof(object),
            typeof(ToolsImportCard),
            new PropertyMetadata(null));

    public object? ToggleToolTip
    {
        get => GetValue(ToggleToolTipProperty);
        set => SetValue(ToggleToolTipProperty, value);
    }
}
