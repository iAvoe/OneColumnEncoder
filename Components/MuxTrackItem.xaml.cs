namespace OneColumnEncoder.Components;

public partial class MuxTrackItem : UserControl
{
    public MuxTrackItem() => InitializeComponent();

    public static readonly DependencyProperty IsRecentlyMovedProperty =
        DependencyProperty.Register(
            nameof(IsRecentlyMoved),
            typeof(bool),
            typeof(MuxTrackItem),
            new PropertyMetadata(false));

    public bool IsRecentlyMoved
    {
        get => (bool)GetValue(IsRecentlyMovedProperty);
        set => SetValue(IsRecentlyMovedProperty, value);
    }

}
