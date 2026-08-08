namespace OneColumnEncoder.Components
{
    public partial class QueueSidebarPanel : UserControl
    {
        public QueueSidebarPanel()
        {
            InitializeComponent();

            foreach (var listBox in new[] { WaitingListBox, UnfinishedListBox, CompletedListBox })
            {
                listBox.PreviewMouseWheel += ListBox_PreviewMouseWheel;
            }
        }

        private void QueueSidebarPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
                return;

            double newOffset = QueueScrollViewer.VerticalOffset - e.Delta;
            if (newOffset < 0)
                newOffset = 0;
            else if (newOffset > QueueScrollViewer.ScrollableHeight)
                newOffset = QueueScrollViewer.ScrollableHeight;

            QueueScrollViewer.ScrollToVerticalOffset(newOffset);
            e.Handled = true;
        }
    }
}
