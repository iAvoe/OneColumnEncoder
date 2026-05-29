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
        private readonly ToolItemCardVM? _rateControlItem;
        private readonly ToolItemCardVM? _baseParamsItem;
        private readonly ToolItemCardVM? _customParamsItem;
        private readonly int _initialTab;

        public OpenEncoderConfCmd(ModalNavS modalNavS,
            ToolItemCardVM? rateControlItem = null,
            ToolItemCardVM? baseParamsItem = null,
            ToolItemCardVM? customParamsItem = null,
            int initialTab = 0)
        {
            _modalNavS = modalNavS;
            _rateControlItem = rateControlItem;
            _baseParamsItem = baseParamsItem;
            _customParamsItem = customParamsItem;
            _initialTab = initialTab;
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
            EncoderConfVM vm = new(window.Close, _rateControlItem, _baseParamsItem, _customParamsItem, _initialTab);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
