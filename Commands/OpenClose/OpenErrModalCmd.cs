using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenErrModalCmd(ModalNavS modalNavS, string windowTitle, string description) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly string _windowTitle = windowTitle;
        private readonly string _description = description;

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
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationModalVM vm =
                ConfirmationModalVM.CreateError(_windowTitle, _description, closeCmd, closeCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }
    }
}
