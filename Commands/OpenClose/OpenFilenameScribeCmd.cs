using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenFilenameScribeCmd(
        ModalNavS modalNavS,
        ToolItemCardVM outputSettingItem) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly ToolItemCardVM _outputSettingItem = outputSettingItem;

        public override bool CanExecute(object? parameter) => true;

        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<FilenameScribeModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            FilenameScribeModal window = new();
            FilenameScribeVM vm = new(window.Close, _outputSettingItem);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
