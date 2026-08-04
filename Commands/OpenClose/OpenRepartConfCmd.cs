using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose;

public sealed class OpenRepartConfCmd(
    ModalNavS modalNavS,
    Func<string> getFfprobePath,
    Func<RepartPlanM?> getCurrentPlan,
    Action<RepartPlanM> applyPlan) : BaseCmd
{
    public override async void Execute(object? parameter)
    {
        RepartConfModal? existing = Application.Current.Windows.OfType<RepartConfModal>().FirstOrDefault();
        if (existing != null)
        {
            existing.Activate();
            return;
        }

        RepartPlanM? currentPlan = getCurrentPlan();
        string[] initialPaths = [];
        if (currentPlan == null)
        {
            OpenFolderDialog dialog = new()
            {
                Title = RepartLangProvider.Current["SelectFolder"],
                Multiselect = false
            };
            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;
            string[] folderPaths = SourceFilePicker.GetVideoFilesInFolder(dialog.FolderName);
            if (folderPaths.Length == 0)
            {
                new OpenErrModalCmd(modalNavS, RepartConfVM.WindowTitleText, RepartLangProvider.Current.SourceRequired).Execute(null);
                return;
            }

            try
            {
                initialPaths = await AnalyzeSrcVideoCmd.AnalyzeAndFilterQueueFilePathsForImportAsync(
                    getFfprobePath(),
                    folderPaths,
                    modalNavS);
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(modalNavS, RepartConfVM.WindowTitleText, ex.Message).Execute(null);
                return;
            }
        }

        if (modalNavS.IsOpen) modalNavS.Close();
        RepartConfModal window = new();
        RepartConfVM vm = new(modalNavS, window.Close, getFfprobePath, applyPlan);
        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => modalNavS.Close();
        modalNavS.CurrentModalVM = vm;
        window.Show();
        _ = vm.InitializeAsync(initialPaths, currentPlan);
    }
}
