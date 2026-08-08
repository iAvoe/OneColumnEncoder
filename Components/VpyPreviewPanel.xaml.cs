namespace OneColumnEncoder.Components;

public partial class VpyPreviewPanel : UserControl
{
    public VpyPreviewPanel()
    {
        InitializeComponent();
    }

    private void ZoomFineOut_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomFineOut();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomOut();
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomIn();
    private void ZoomFineIn_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomFineIn();
    private void ZoomFit_Click(object sender, RoutedEventArgs e) => PreviewViewer.Fit();
    private void Zoom100_Click(object sender, RoutedEventArgs e) => PreviewViewer.SetActualSize();
    private void Zoom200_Click(object sender, RoutedEventArgs e) => PreviewViewer.SetDoubleSize();
}
