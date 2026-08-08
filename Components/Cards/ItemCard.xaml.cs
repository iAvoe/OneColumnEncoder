namespace OneColumnEncoder.Components;

/// <summary>
/// Interaction logic for ItemCard.xaml
/// </summary>
public partial class ItemCard : UserControl
{
    public ItemCard()
    {
        InitializeComponent();
    }

    // Register IsSelected depending property
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(ItemCard),
            new PropertyMetadata(false));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsCancelProperty =
        DependencyProperty.Register(
            nameof(IsCancel),
            typeof(bool),
            typeof(ItemCard),
            new PropertyMetadata(false));

    public bool IsCancel
    {
        get => (bool)GetValue(IsCancelProperty);
        set => SetValue(IsCancelProperty, value);
    }

    public static readonly DependencyProperty IsRealProperty =
        DependencyProperty.Register(
            nameof(IsReal),
            typeof(bool),
            typeof(ItemCard),
            new PropertyMetadata(true));

    public bool IsReal
    {
        get => (bool)GetValue(IsRealProperty);
        set => SetValue(IsRealProperty, value);
    }

    public static readonly DependencyProperty EnableRealCheckProperty =
        DependencyProperty.Register(
            nameof(EnableRealCheck),
            typeof(bool),
            typeof(ItemCard),
            new PropertyMetadata(true));

    public bool EnableRealCheck
    {
        get => (bool)GetValue(EnableRealCheckProperty);
        set => SetValue(EnableRealCheckProperty, value);
    }

    public static readonly DependencyProperty PaletteIndexProperty =
        DependencyProperty.Register(
            nameof(PaletteIndex),
            typeof(int),
            typeof(ItemCard),
            new PropertyMetadata(0));

    public int PaletteIndex
    {
        get => (int)GetValue(PaletteIndexProperty);
        set => SetValue(PaletteIndexProperty, value);
    }
}
