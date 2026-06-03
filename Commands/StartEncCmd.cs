using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class StartEncCmd(Func<EncodingPipelineRequest?> buildRequest, ModalNavS modalNavS) : BaseCmd
    {
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override void Execute(object? parameter)
        {
            EncodingPipelineRequest? request = _buildRequest();
            if (request == null) return;

            EncodingPipelineCommand command = EncodingPipelineH.BuildY4mCommand(request);

            ConfirmationModal? existing = Application.Current.Windows
                .OfType<ConfirmationModal>()
                .FirstOrDefault(w => w.DataContext is ConfirmationModalVM &&
                                w.Owner == Application.Current.MainWindow);
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationModalVM vm = ConfirmationModalVM.CreateDebug(
                "Encoding Command", command.CommandLine,
                closeCmd,
                new ActionCmd(_ => { window.DialogResult = true; window.Close(); }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }
    }
}
