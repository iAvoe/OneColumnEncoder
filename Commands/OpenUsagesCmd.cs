using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class OpenUsagesCmd(ModalNavS modelNavS) : BaseCmd
    {
        private readonly ModalNavS _modelNavS = modelNavS;
        public override void Execute(object? parameter)
        {
            var existingWindow = Application.Current.Windows
                .OfType<UsageComplianceWindow>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modelNavS.IsOpen)
                _modelNavS.Close();

            var window = new UsageComplianceWindow
            {
                DataContext = new UsageComplianceVM(_modelNavS)
            };
            window.Show();
        }
    }
}
