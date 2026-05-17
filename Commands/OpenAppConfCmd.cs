using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class OpenAppConfCmd(ModalNavS modalNavS, AppConfS appConfS) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly AppConfS _appConfS = appConfS;

        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<AppConfWindow>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            var window = new AppConfWindow
            {
                DataContext = new AppConfVM(_modalNavS, _appConfS)
            };
            window.Show();
        }
    }
}
