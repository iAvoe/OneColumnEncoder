using System.Diagnostics;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenSavedTextModalCmd(
        ModalNavS modalNavS,
        string windowTitle,
        string description,
        string openTxtText,
        string txtPath) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly string _windowTitle = windowTitle;
        private readonly string _description = description;
        private readonly string _openTxtText = openTxtText;
        private readonly string _txtPath = txtPath;

        public override void Execute(object? parameter)
        {
            ConfirmationModal? existingWindow = Application.Current.Windows
                .OfType<ConfirmationModal>()
                .FirstOrDefault(w => w.DataContext is ConfirmationVM &&
                                w.Owner == Application.Current.MainWindow);

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateSuccess(_windowTitle, _description, closeCmd, closeCmd);
            vm.ContextMenuItems.Add(new(
                _openTxtText,
                new ActionCmd(_ => Process.Start(new ProcessStartInfo(_txtPath) { UseShellExecute = true }))));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }
    }
}
