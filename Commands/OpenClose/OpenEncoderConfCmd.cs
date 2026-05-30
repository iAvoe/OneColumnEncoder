using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenEncoderConfCmd : BaseCmd
    {
        private readonly ModalNavS _modalNavS;
        private readonly ToolItemCardVM? _compressionParamsItem;
        public OpenEncoderConfCmd(ModalNavS modalNavS,
            ToolItemCardVM? compressionParamsItem = null)
        {
            _modalNavS = modalNavS;
            _compressionParamsItem = compressionParamsItem;
        }

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
            EncoderConfVM vm = new(window.Close, _compressionParamsItem);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) =>
            {
                _modalNavS.Close();
                if (_compressionParamsItem != null)
                    EncoderConfVM.ApplySavedSettingsToCard(_compressionParamsItem);
            };
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
