namespace OneColumnEncoder.Commands.OpenClose;

public class OpenDebugModalCmd(ModalNavS modalNavS, string windowTitle, string description) : BaseCmd
{
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly string _windowTitle = windowTitle;
    private readonly string _description = description;

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
        ConfirmationVM vm =
            ConfirmationVM.CreateDebug(_windowTitle, _description, closeCmd, closeCmd);

        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => _modalNavS.Close();
        _modalNavS.CurrentModalVM = vm;
        window.ShowDialog();
    }
}
