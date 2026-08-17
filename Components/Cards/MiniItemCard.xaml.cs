namespace OneColumnEncoder.Components.Cards;

public partial class MiniItemCard : UserControl
{
    public MiniItemCard()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(MiniItemCard),
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
            typeof(MiniItemCard),
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
            typeof(MiniItemCard),
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
            typeof(MiniItemCard),
            new PropertyMetadata(true));

    public bool EnableRealCheck
    {
        get => (bool)GetValue(EnableRealCheckProperty);
        set => SetValue(EnableRealCheckProperty, value);
    }

    public static readonly DependencyProperty IsRecentlyMovedProperty =
        DependencyProperty.Register(
            nameof(IsRecentlyMoved),
            typeof(bool),
            typeof(MiniItemCard),
            new PropertyMetadata(false));

    public bool IsRecentlyMoved
    {
        get => (bool)GetValue(IsRecentlyMovedProperty);
        set => SetValue(IsRecentlyMovedProperty, value);
    }

    public static readonly DependencyProperty ShowActionButtonsProperty =
        DependencyProperty.Register(
            nameof(ShowActionButtons),
            typeof(bool),
            typeof(MiniItemCard),
            new PropertyMetadata(true));

    public bool ShowActionButtons
    {
        get => (bool)GetValue(ShowActionButtonsProperty);
        set => SetValue(ShowActionButtonsProperty, value);
    }

    public static readonly DependencyProperty PaletteIndexProperty =
        DependencyProperty.Register(
            nameof(PaletteIndex),
            typeof(int),
            typeof(MiniItemCard),
            new PropertyMetadata(0));

    public int PaletteIndex
    {
        get => (int)GetValue(PaletteIndexProperty);
        set => SetValue(PaletteIndexProperty, value);
    }
}
