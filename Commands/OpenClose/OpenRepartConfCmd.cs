using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.RepartManagement;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Threading;
using System.Windows;

namespace OneColumnEncoder.Commands.OpenClose;

public sealed class OpenRepartConfCmd(
    ModalNavS modalNavS,
    Func<string> getFfprobePath,
    Func<string?>? getFfmpegPath,
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
                // Run the Repart Mode check & filter pass before the window opens.
                // Excluded sources are reported immediately, matching queue-mode
                // behavior: once a source fails a filtering stage, it is skipped and
                // never reaches the later ffprobe analysis or frame-count scan.
                using CancellationTokenSource cancellation = new();
                ProgressModal progressWindow = new();
                ProgressVM progressVM = new(
                    RepartConfVM.WindowTitleText,
                    string.Format(RepartLangProvider.Current["AnalyzingProgress"], 0, folderPaths.Length, string.Empty).Trim(),
                    new ActionCmd(_ => cancellation.Cancel()));
                progressWindow.DataContext = progressVM;
                progressWindow.Owner = Application.Current.MainWindow;
                progressWindow.Closed += (_, _) =>
                {
                    cancellation.Cancel();
                    modalNavS.Close();
                };
                modalNavS.CurrentModalVM = progressVM;

                Task<RepartAnalysisResult> analysisTask = RepartCompatibilityAnalyzer.AnalyzeAndFilterAsync(
                    ffprobePath: getFfprobePath(),
                    ffmpegPath: getFfmpegPath?.Invoke(),
                    filePaths: folderPaths,
                    confirmDiscardInterlacedSource: source => RepartInterlacedPrompt.Confirm(modalNavS, RepartConfVM.WindowTitleText, source),
                    onFileProgress: (index, total, name) => progressVM.P1Text =
                        string.Format(RepartLangProvider.Current["AnalyzingProgress"], index, total, name),
                    onExcluded: excluded => new OpenErrModalCmd(
                        modalNavS,
                        RepartConfVM.WindowTitleText,
                        RepartExclusionMessages.FormatExcludedMessage(excluded)).Execute(null),
                    cancellationToken: cancellation.Token);

                _ = CloseWhenCompletedAsync(analysisTask, progressWindow);
                progressWindow.ShowDialog();

                RepartAnalysisResult result = await analysisTask;

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
        RepartConfVM vm = new(modalNavS, window.Close, getFfprobePath, getFfmpegPath, applyPlan);
        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => modalNavS.Close();
        modalNavS.CurrentModalVM = vm;
        window.Show();
        _ = vm.InitializeAsync([], initialPlan);
    }

    private static async Task CloseWhenCompletedAsync(Task task, ProgressModal modal)
    {
        try
        {
            await task;
        }
        catch
        {
        }
        if (modal.IsVisible) modal.Close();
    }
}
