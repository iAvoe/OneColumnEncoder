using OneColumnEncoder.Commands.OpenClose;

namespace OneColumnEncoder.Commands.OpenClose.Confirmations;

public class OpenInfoModalCmd(ModalNavS modalNavS, string windowTitle, string description)
    : OpenConfirmationBase(modalNavS, windowTitle, description)
{
    protected override ConfirmationVM CreateViewModel(ConfirmationModal window)
    {
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
        return ConfirmationVM.CreateInfo(WindowTitle, Description, cancelCmd, confirmCmd);
    }
}
