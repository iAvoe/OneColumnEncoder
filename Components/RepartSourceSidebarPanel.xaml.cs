using System.Windows.Controls;
using System.Windows.Input;

namespace OneColumnEncoder.Components;

public partial class RepartSourceSidebarPanel : UserControl
{
    public RepartSourceSidebarPanel()
    {
        InitializeComponent();
        SourceListBox.PreviewMouseWheel += SourceListBox_PreviewMouseWheel;
    }

    private void RepartSourceSidebarPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ForwardWheelToScrollViewer(e);
    }

    private void SourceListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ForwardWheelToScrollViewer(e);
    }

    private void ForwardWheelToScrollViewer(MouseWheelEventArgs e)
    {
        if (e.Handled) return;

        SourceScrollViewer.ScrollToVerticalOffset(Math.Clamp(
            SourceScrollViewer.VerticalOffset - e.Delta,
            0,
            SourceScrollViewer.ScrollableHeight));
        e.Handled = true;
    }
}
