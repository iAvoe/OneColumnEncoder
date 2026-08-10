namespace OneColumnEncoder.Views;

public partial class QueueEditorModal : AdaptiveWindow
{
    public QueueEditorModal()
    {
        InitializeComponent();
        QueueItemsListBox.PreviewMouseWheel += QueueItemsListBox_PreviewMouseWheel;
    }

    private void QueueItemsListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;

        double newOffset = QueueEditorScrollViewer.VerticalOffset - e.Delta;
        QueueEditorScrollViewer.ScrollToVerticalOffset(
            Math.Clamp(newOffset, 0, QueueEditorScrollViewer.ScrollableHeight));
        e.Handled = true;
    }
}
