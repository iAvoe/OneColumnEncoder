using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenEncoderConfCmd : BaseCmd
    {
        private readonly ModalNavS _modalNavS;
        private readonly ToolItemCardVM? _compressionParamsItem;
        private readonly Func<string?>? _getFfmpegPath;
        private readonly Func<string?>? _getSourceVideoPath;
        private readonly Func<string?>? _getSourceFfprobeJson;
        public OpenEncoderConfCmd(ModalNavS modalNavS,
            ToolItemCardVM? compressionParamsItem = null,
            Func<string?>? getFfmpegPath = null,
            Func<string?>? getSourceVideoPath = null,
            Func<string?>? getSourceFfprobeJson = null)
        {
            _modalNavS = modalNavS;
            _compressionParamsItem = compressionParamsItem;
            _getFfmpegPath = getFfmpegPath;
            _getSourceVideoPath = getSourceVideoPath;
            _getSourceFfprobeJson = getSourceFfprobeJson;
        }

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
            PositionPreviewWindow(window, previewWindow);
            previewWindow.Show();
        }

        private static void PositionPreviewWindow(Window owner, Window preview)
        {
            const double gap = 8d;
            Rect workArea = SystemParameters.WorkArea;
            double left = owner.Left + owner.ActualWidth + gap;
            double ownerWidth = owner.ActualWidth > 0 ? owner.ActualWidth : owner.Width;
            left = owner.Left + ownerWidth + gap;
            if (left + preview.Width > workArea.Right)
                left = Math.Max(workArea.Left, owner.Left - preview.Width - gap);

            preview.Left = Math.Max(workArea.Left, Math.Min(workArea.Right - preview.Width, left));
            preview.Top = Math.Max(workArea.Top, Math.Min(workArea.Bottom - preview.Height, owner.Top));
        }
    }
}
