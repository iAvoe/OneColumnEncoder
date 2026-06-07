using OneColumnEncoder.Commands;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.ViewModels;

public class ConfirmationVM(string windowTitle, string message, ImageSource image, ICommand cancelCmd, ICommand confirmCmd) : BaseVM
{
    public string WindowTitle { get; set; } = windowTitle;
    public string P1Text { get; } = message;
    public static string CopyText => UILangProviderM.Current["ConfirmDialog.CopyText"];
    public static string CopyHint => UILangProviderM.Current["ConfirmDialog.CopyHint"];
    public ImageSource I1Source { get; } = image;
    public ButtonGroupVM FinishWarnErrButtons { get; } =
        ButtonGroupVM.CreateTwoButton(
            UILangProviderM.Current["ConfirmDialog.Cancel"],
            UILangProviderM.Current["ConfirmDialog.Confirm"],
            cancelCmd, confirmCmd);

    public static ConfirmationVM CreateWarning(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd) =>
        new(UILangProviderM.Current["ConfirmDialog.WarningPrefix"] + title, p1Text, SvgIconProviderH.GlobeWarning, cancelCmd, confirmCmd);

    public static ConfirmationVM CreateError(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd) =>
        new(UILangProviderM.Current["ConfirmDialog.ErrorPrefix"] + title, p1Text, SvgIconProviderH.GlobeError, cancelCmd, confirmCmd);

    public static ConfirmationVM CreateDebug(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd) =>
        new(UILangProviderM.Current["ConfirmDialog.DebugPrefix"] + title, p1Text, SvgIconProviderH.Troubleshoot, cancelCmd, confirmCmd);

    public static ConfirmationVM CreateInfo(string title, string p1Text, ICommand cancelCmd, ICommand confirmCmd) =>
        new(UILangProviderM.Current["ConfirmDialog.InfoPrefix"] + title, p1Text, SvgIconProviderH.AzureConsortium, cancelCmd, confirmCmd);
}
