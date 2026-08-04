using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.RepartManagement;
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
        RepartPlanM? initialPlan = currentPlan;
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
                // Run the full Repart Mode check & filter pass before the window opens:
                // every rejected source is reported here (between the per-source ffprobe
                // failure modal and the completion summary) instead of being silently
                // dropped after the window opens. The resulting plan is pre-loaded.
                RepartAnalysisResult result = await RepartCompatibilityAnalyzer.AnalyzeAndFilterAsync(
                    getFfprobePath(),
                    folderPaths,
                    source => RepartInterlacedPrompt.Confirm(modalNavS, RepartConfVM.WindowTitleText, source));

                foreach (RepartExcludedSourceInfo excluded in result.Excluded)
                {
                    new OpenErrModalCmd(
                        modalNavS,
                        RepartConfVM.WindowTitleText,
                        RepartExclusionMessages.FormatExcludedMessage(excluded)).Execute(null);
                }

                if (result.Plan == null)
                {
                    new OpenErrModalCmd(
                        modalNavS,
                        RepartConfVM.WindowTitleText,
                        result.FatalMessage ?? RepartLangProvider.Current.SourceRequired).Execute(null);
                    return;
                }

                new OpenSuccModalCmd(
                    modalNavS,
                    RepartConfVM.WindowTitleText,
                    string.Format(
                        RepartLangProvider.Current["ImportSummary"],
                        result.Plan.Sources.Count,
                        result.Excluded.Count)).Execute(null);
                initialPlan = result.Plan;
            }
            catch (OperationCanceledException)
            {
                return;
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
        _ = vm.InitializeAsync([], initialPlan);
    }
}
