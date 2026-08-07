using OneColumnEncoder.Commands;
using OneColumnEncoder.Models.Lang;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Windows;

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

        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => modalNavS.Close();
        modalNavS.CurrentModalVM = vm;
        window.ShowDialog();
        return confirmed;
    }
}
