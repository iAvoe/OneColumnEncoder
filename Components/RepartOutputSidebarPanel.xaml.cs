namespace OneColumnEncoder.Components;

public partial class RepartOutputSidebarPanel : UserControl
{
    public RepartOutputSidebarPanel()
    {
        InitializeComponent();

        foreach (var listBox in new[] { WaitingListBox, UnfinishedListBox, CompletedListBox })
        {
            listBox.PreviewMouseWheel += ListBox_PreviewMouseWheel;
        }
    }

    private void Sidebar_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ForwardWheelToScrollViewer(e);
    }

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ForwardWheelToScrollViewer(e);
    }

    private void ForwardWheelToScrollViewer(MouseWheelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        var newOffset = SidebarScrollViewer.VerticalOffset - e.Delta;
        if (newOffset < 0)
        {
            newOffset = 0;
        }
        else if (newOffset > SidebarScrollViewer.ScrollableHeight)
        {
            newOffset = SidebarScrollViewer.ScrollableHeight;
        }

        SidebarScrollViewer.ScrollToVerticalOffset(newOffset);
        e.Handled = true;
    }
}
