using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.ViewModels;

public class WarnErrModalVM(string message, string emoji, ICommand cancelCmd, ICommand confirmCmd) : BaseVM
{
    public string P1Text { get; } = message;
    public string I1Text { get; } = emoji;
    public ButtonGroupVM FinishWarnErrButtons { get; } =
        ButtonGroupVM.CreateTwoButton("No", "Yes", cancelCmd, confirmCmd);

    public static WarnErrModalVM CreateImportWarning(string p1Text, ICommand cancelCmd, ICommand confirmCmd)
    {
        return new WarnErrModalVM(p1Text, "\u1F42", cancelCmd, confirmCmd);
    }

    public static WarnErrModalVM CreateCustomModal(string p1Text, string emoji, ICommand cancelCmd, ICommand confirmCmd)
    {
        return new WarnErrModalVM(p1Text, emoji, cancelCmd, confirmCmd);
    }
}
