namespace OneColumnEncoder.Commands.OpenClose.Confirmations;

public class OpenSuccModalCmd(ModalNavS modalNavS, string windowTitle, string description)
    : OpenConfirmationBase(modalNavS, windowTitle, description)
{
    protected override ConfirmationVM CreateViewModel(ConfirmationModal window)
    {
        CloseModalCmd closeCmd = new(window.Close);
        return ConfirmationVM.CreateSuccess(WindowTitle, Description, closeCmd, closeCmd);
    }
}
