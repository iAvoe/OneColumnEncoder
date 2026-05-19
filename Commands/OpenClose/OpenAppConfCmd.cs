using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenAppConfCmd(ModalNavS modalNavS, AppConfM appConfS) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly AppConfM _appConfS = appConfS;

        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<AppConfModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            AppConfModal window = new();
            var vm = new AppConfVM(_modalNavS, _appConfS, window.Close);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}