using OneColumnEncoder.Commands;
using OneColumnEncoder.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    public partial class EncoderPreviewPanel : UserControl
    {
        public EncoderPreviewPanel()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ImageABPreviewVM vm) return;

            vm.ZoomPresetButtons.Cmd1 = new ActionCmd(_ => PreviewViewer.Fit());
            vm.ZoomPresetButtons.Cmd2 = new ActionCmd(_ => PreviewViewer.SetActualSize());
            vm.ZoomPresetButtons.Cmd3 = new ActionCmd(_ => PreviewViewer.SetDoubleSize());
        }

        private void ZoomFineOut_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomFineOut();
        private void ZoomOut_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomOut();
        private void ZoomIn_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomIn();
        private void ZoomFineIn_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomFineIn();
    }
}
