namespace OneColumnEncoder.Views;

public partial class MuxTracksConfModal : AdaptiveWindow
{
    public MuxTracksConfModal()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is not ViewModels.MuxTracks.MuxTracksConfVM viewModel) return;

            WindowState = WindowState.Normal;
            Width = viewModel.ShowSidebar ? 760 : 540;
        };
    }
}
