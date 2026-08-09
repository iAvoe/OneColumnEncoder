namespace OneColumnEncoder.ChapterTool;

public static class RepartChapterImportPrompt
{
    public static bool Confirm(ModalNavS modalNavS)
    {
        ConfirmationModal window = new();
        bool confirmed = false;
        ActionCmd cancelCmd = new(_ => window.Close());
        ActionCmd confirmCmd = new(_ =>
        {
            confirmed = true;
            window.Close();
        });
        ConfirmationVM vm = ConfirmationVM.CreateInfo(
            RepartConfVM.WindowTitleText,
            RepartLangProvider.Current["ChapterFileImportPrompt"],
            cancelCmd,
            confirmCmd);

        OpenCloseBase.ShowModal(modalNavS, window, vm, showDialog: true);
        return confirmed;
    }
}
