using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenUsagesCmd(ModalNavS modelNavS, AppConfM appConfM) : BaseCmd
    {
        private readonly ModalNavS _modelNavS = modelNavS;
        private readonly AppConfM _appConfM = appConfM;
        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<AppUsageModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modelNavS.IsOpen)
                _modelNavS.Close();

            var window = new AppUsageModal();
            var vm = new AppUsageVM(_appConfM, window.Close);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modelNavS.Close();
            _modelNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}