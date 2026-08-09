namespace OneColumnEncoder.ChapterTool;

public static class QueueChapterImportPrompt
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
            UILangProvider.Current["SourceQueue.ImportTitle"],
            UILangProvider.Current["SourceQueue.ImportPrompt"],
            cancelCmd,
            confirmCmd);

        OpenCloseBase.ShowModal(modalNavS, window, vm, showDialog: true);
        return confirmed;
    }
}
