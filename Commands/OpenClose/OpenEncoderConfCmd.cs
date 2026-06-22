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
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            EncoderConfModal window = new();
            EncoderConfVM vm = new(
                window.Close,
                _compressionParamsItem,
                _modalNavS,
                _getFfmpegPath?.Invoke(),
                _getSourceVideoPath?.Invoke(),
                _getSourceFfprobeJson?.Invoke());
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;

            window.Closed += (_, _) =>
            {
                _modalNavS.Close();
            };
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
