using OneColumnEncoder.Commands;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.ViewModels;

public class ConfirmationModalVM(string windowTitle, string message, ImageSource image, ICommand cancelCmd, ICommand confirmCmd) : BaseVM
{
    public string WindowTitle { get; set; } = windowTitle;
    public string P1Text { get; } = message;
    public ImageSource I1Source { get; } = image;
    public ButtonGroupVM FinishWarnErrButtons { get; } =
        ButtonGroupVM.CreateTwoButton(
            UILangProviderM.Current["ConfirmDialog.Cancel"],
            UILangProviderM.Current["ConfirmDialog.Confirm"],
            cancelCmd, confirmCmd);

    public static ConfirmationModalVM CreateWarning(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd)
    {
        return new ConfirmationModalVM(UILangProviderM.Current["ConfirmDialog.WarningPrefix"] + title, p1Text, SvgIconProvider.GlobeWarning, cancelCmd, confirmCmd);
    }

    public static ConfirmationModalVM CreateError(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd)
    {
        return new ConfirmationModalVM(UILangProviderM.Current["ConfirmDialog.ErrorPrefix"] + title, p1Text, SvgIconProvider.GlobeError, cancelCmd, confirmCmd);
    }

    public static ConfirmationModalVM CreateDebug(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd)
    {
        return new ConfirmationModalVM(UILangProviderM.Current["ConfirmDialog.DebugPrefix"] + title, p1Text, SvgIconProvider.Troubleshoot, cancelCmd, confirmCmd);
    }
}
