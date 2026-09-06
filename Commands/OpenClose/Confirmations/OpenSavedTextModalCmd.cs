namespace OneColumnEncoder.Commands.OpenClose.Confirmations;

public class OpenSavedTextModalCmd(
    ModalNavS modalNavS,
    string windowTitle,
    string description,
    string openTxtText,
    string txtPath) : OpenConfirmationBase(modalNavS, windowTitle, description)
{
    private readonly string _openTxtText = openTxtText;
    private readonly string _txtPath = txtPath;

    protected override ConfirmationVM CreateViewModel(ConfirmationModal window)
    {
        CloseModalCmd closeCmd = new(window.Close);
        return ConfirmationVM.CreateSuccess(WindowTitle, Description, closeCmd, closeCmd);
    }

    protected override void ConfigureViewModel(ConfirmationVM vm)
    {
        vm.ContextMenuItems.Add(new(
            _openTxtText,
            new ActionCmd(_ => Process.Start(new ProcessStartInfo(_txtPath) { UseShellExecute = true }))));
    }
}
