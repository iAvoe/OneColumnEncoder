using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenUsagesCmd(ModalNavS modelNavS) : BaseCmd
    {
        private readonly ModalNavS _modelNavS = modelNavS;
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

            // First create the window
            var window = new AppUsageModal();

            // Now window.Close is ready
            window.DataContext = new UsageComplianceVM(_modelNavS, window.Close);
            window.Show();
        }
    }
}