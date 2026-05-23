using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenWarnModalCmd(ModalNavS modalNavS) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        public override void Execute(object? parameter)
        {
            ConfirmationModal? existingWindow = Application.Current.Windows
                .OfType<ConfirmationModal>()
                .FirstOrDefault(w => w.DataContext is ConfirmationModalVM &&
                                w.Owner == Application.Current.MainWindow);

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(_modalNavS, window.Close);
            ConfirmationModalVM vm = ConfirmationModalVM.CreateWarning(
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                UILangProviderM.Current["SrcScribe.Description2"],
                closeCmd, closeCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }
    }
}
