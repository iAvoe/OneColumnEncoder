using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenEncodingMonitorCmd(
        ModalNavS modalNavS,
        EncodingPipelineRequest request,
        EncodingPipelineCommand command,
        bool isSample = false) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly EncodingPipelineRequest _request = request;
        private readonly EncodingPipelineCommand _command = command;
        private readonly bool _isSample = isSample;

        public override void Execute(object? parameter)
        {
            EncodingMonitorModal? existingWindow = Application.Current.Windows
                .OfType<EncodingMonitorModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            EncodingMonitorModal window = new();
            EncodingMonitorVM vm = new(_modalNavS, window.Close, _request, _command, _isSample);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
