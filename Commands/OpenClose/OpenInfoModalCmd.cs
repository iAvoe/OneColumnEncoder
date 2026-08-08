namespace OneColumnEncoder.Commands.OpenClose;

public class OpenInfoModalCmd(
    ModalNavS modalNavS,
    string windowTitle,
    string description) : BaseCmd
{
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly string _windowTitle = windowTitle;
    private readonly string _description = description;
    public bool? DialogResult { get; private set; }

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
        ActionCmd cancelCmd = new(_ =>
        {
            DialogResult = false;
            window.DialogResult = false;
            window.Close();
        });
        ActionCmd confirmCmd = new(_ =>
        {
            DialogResult = true;
            window.DialogResult = true;
            window.Close();
        });
        ConfirmationVM vm =
            ConfirmationVM.CreateInfo(_windowTitle, _description, cancelCmd, confirmCmd);

        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) =>
        {
            DialogResult ??= window.DialogResult == true;
            _modalNavS.Close();
        };
        _modalNavS.CurrentModalVM = vm;
        window.ShowDialog();
    }
}
