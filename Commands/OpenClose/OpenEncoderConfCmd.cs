using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenEncoderConfCmd(ModalNavS modalNavS,
        ToolItemCardVM? compressionParamsItem = null,
        Func<string?>? getFfmpegPath = null,
        Func<string?>? getSourceVideoPath = null,
        Func<string?>? getSourceFfprobeJson = null) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly ToolItemCardVM? _compressionParamsItem = compressionParamsItem;
        private readonly Func<string?>? _getFfmpegPath = getFfmpegPath;
        private readonly Func<string?>? _getSourceVideoPath = getSourceVideoPath;
        private readonly Func<string?>? _getSourceFfprobeJson = getSourceFfprobeJson;

        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<EncoderConfModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                Application.Current.Windows
                    .OfType<ImageABPreviewModal>()
                    .FirstOrDefault()
                    ?.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            EncoderConfModal window = new();
            EncoderConfVM vm = new(window.Close, _compressionParamsItem);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;

            ImageABPreviewModal previewWindow = new();
            ImageABPreviewVM previewVM = new(
                vm,
                _getFfmpegPath?.Invoke(),
                _getSourceVideoPath?.Invoke(),
                _getSourceFfprobeJson?.Invoke());
            previewWindow.DataContext = previewVM;
            previewWindow.Owner = Application.Current.MainWindow;

            window.Closed += (_, _) =>
            {
                previewWindow.CloseFromOwner();
                _modalNavS.Close();
            };
            window.LocationChanged += (_, _) => PositionPreviewWindow(window, previewWindow);
            window.SizeChanged += (_, _) => PositionPreviewWindow(window, previewWindow);
            _modalNavS.CurrentModalVM = vm;
            window.Show();
            previewWindow.Show();
            PositionPreviewWindow(window, previewWindow);
        }

        // Open ImageABViewer beside EncoderConfModal
        private static void PositionPreviewWindow(Window owner, Window preview)
        {
            Rect workArea = SystemParameters.WorkArea;
            double ownerWidth = owner.Width > 0 && !double.IsNaN(owner.Width) ? owner.Width : owner.ActualWidth;
            double left = owner.Left + ownerWidth;
            double rightSpace = workArea.Right - left;

            if (rightSpace >= preview.MinWidth)
                preview.Width = Math.Min(Math.Max(preview.Width, preview.MinWidth), rightSpace);
            else if (left + preview.Width > workArea.Right)
                left = Math.Max(workArea.Left, workArea.Right - preview.Width);

            preview.Left = Math.Max(workArea.Left, Math.Min(workArea.Right - preview.Width, left));
            preview.Top = Math.Max(workArea.Top, Math.Min(workArea.Bottom - preview.Height, owner.Top));
        }
    }
}
