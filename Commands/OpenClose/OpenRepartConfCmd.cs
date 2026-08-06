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
            if (folderPaths.Length < 2)
            {
                new OpenErrModalCmd(modalNavS, RepartConfVM.WindowTitleText, RepartLangProvider.Current.MinFolderSources).Execute(null);
                return;
            }

            try
            {
                // Run the Repart Mode check & filter pass before the window opens.
                // Excluded sources are collected during analysis and reported once,
                // after filtering completes, so the user sees a single summary.
                using CancellationTokenSource cancellation = new();
                ProgressModal progressWindow = new();
                ProgressVM progressVM = new(
                    RepartConfVM.WindowTitleText,
                    RepartLangProvider.Current["StageCheckFiles"],
                    new ActionCmd(_ => cancellation.Cancel()));
                progressWindow.DataContext = progressVM;
                progressWindow.Owner = Application.Current.MainWindow;
                progressWindow.Closed += (_, _) =>
                {
                    cancellation.Cancel();
                    modalNavS.Close();
                };
                modalNavS.CurrentModalVM = progressVM;

                int excludedCount = 0;
                List<RepartExcludedSourceInfo> excludedItems = [];
                Task<RepartAnalysisResult> analysisTask = RepartCompatibilityAnalyzer.AnalyzeAndFilterAsync(
                    ffprobePath: getFfprobePath(),
                    ffmpegPath: getFfmpegPath?.Invoke(),
                    filePaths: folderPaths,
                    confirmDiscardInterlacedSource: source => RepartInterlacedPrompt.Confirm(modalNavS, RepartConfVM.WindowTitleText, source),
                    confirmExpandFrameCountSearch: source => RepartFrameCountPrompt.Confirm(modalNavS, RepartConfVM.WindowTitleText, source),
                    onFileProgress: (stage, index, total, name) =>
                    {
                        progressVM.P1Text = RepartLangProvider.Current[StageKey(stage)];
                        progressVM.P2Text = string.Format(RepartLangProvider.Current["ProcessingFile"], index, total, name);
                    },
                    onExcluded: excluded =>
                    {
                        excludedCount++;
                        excludedItems.Add(excluded);
                        progressVM.P3Text = string.Format(RepartLangProvider.Current["ExcludedCount"], excludedCount);
                    },
                    cancellationToken: cancellation.Token);

                _ = CloseWhenCompletedAsync(analysisTask, progressWindow);
                progressWindow.ShowDialog();

                RepartAnalysisResult result = await analysisTask;

                if (excludedItems.Count > 0)
                {
                    new OpenErrModalCmd(
                        modalNavS,
                        RepartConfVM.WindowTitleText,
                        BuildExcludedSummary(excludedItems, result.FatalMessage)).Execute(null);
                }
                else if (result.Plan == null)
                {
                    new OpenErrModalCmd(
                        modalNavS,
                        RepartConfVM.WindowTitleText,
                        result.FatalMessage ?? RepartLangProvider.Current.SourceRequired).Execute(null);
                }

                if (result.Plan == null)
                {
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
        RepartConfVM vm = new(modalNavS, window.Close, applyPlan);
        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => modalNavS.Close();
        modalNavS.CurrentModalVM = vm;
        window.Show();
        _ = vm.InitializeAsync(initialPlan);
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

    private static string StageKey(RepartAnalysisStage stage) => stage switch
    {
        RepartAnalysisStage.CheckFiles => "StageCheckFiles",
        RepartAnalysisStage.ProbeFiles => "StageProbeFiles",
        RepartAnalysisStage.AnalyzeStreams => "StageAnalyzeStreams",
        RepartAnalysisStage.ValidateStreams => "StageValidateStreams",
        RepartAnalysisStage.ScanFrames => "StageScanFrames",
        _ => "StageCheckFiles"
    };

    private static string BuildExcludedSummary(
        IReadOnlyList<RepartExcludedSourceInfo> excludedItems,
        string? fatalMessage)
    {
        List<string> sections = [];
        if (excludedItems.Count > 0)
        {
            sections.Add(string.Format(RepartLangProvider.Current["ExcludedCount"], excludedItems.Count));
            sections.AddRange(excludedItems.Select(FormatExcludedSummaryLine));
        }

        if (!string.IsNullOrWhiteSpace(fatalMessage))
            sections.Add(fatalMessage.Trim());

        return string.Join(Environment.NewLine + Environment.NewLine, sections);
    }

    private static string FormatExcludedSummaryLine(RepartExcludedSourceInfo info) =>
        string.Join(
            Environment.NewLine,
            info.DisplayName,
            RepartExclusionMessages.FormatReason(info));
}
