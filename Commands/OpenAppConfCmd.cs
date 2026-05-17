using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands
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

            // First create the window
            AppConfModal window = new();

            // Now window.Close is ready
            window.DataContext = new AppConfVM(_modalNavS, _appConfS, window.Close);
            window.Show();
        }
    }
}