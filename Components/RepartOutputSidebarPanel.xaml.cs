using System.Windows.Controls;
using System.Windows.Input;

namespace OneColumnEncoder.Components;

public partial class RepartOutputSidebarPanel : UserControl
{
    public RepartOutputSidebarPanel() => InitializeComponent();

    private void Sidebar_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;
        SidebarScrollViewer.ScrollToVerticalOffset(Math.Clamp(
            SidebarScrollViewer.VerticalOffset - e.Delta,
            0,
            SidebarScrollViewer.ScrollableHeight));
        e.Handled = true;
    }
}
