namespace OneColumnEncoder.Commands.OpenClose.Confirmations;

public class OpenDebugModalCmd(ModalNavS modalNavS, string windowTitle, string description)
    : OpenConfirmationBase(modalNavS, windowTitle, description)
{
    protected override ConfirmationVM CreateViewModel(ConfirmationModal window)
    {
        CloseModalCmd closeCmd = new(window.Close);
        return ConfirmationVM.CreateDebug(WindowTitle, Description, closeCmd, closeCmd);
    }
}
