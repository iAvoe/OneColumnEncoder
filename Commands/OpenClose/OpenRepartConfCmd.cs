using Microsoft.Win32;
using OneColumnEncoder.ChapterTool;
using OneColumnEncoder.RepartManagement;
using System.IO;
using System.Threading;

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
            bool importAsChapterFile = RepartChapterImportPrompt.Confirm(modalNavS);
            initialPlan = importAsChapterFile
                ? await ImportChapterFolderAsync()
                : await ImportFolderAsync();
            if (initialPlan == null) return;
        }

        if (modalNavS.IsOpen) modalNavS.Close();
        RepartConfModal window = new();
        RepartConfVM vm = new(modalNavS, window.Close, applyPlan, getFfmpegPath?.Invoke(), getFfprobePath());
        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => modalNavS.Close();
        modalNavS.CurrentModalVM = vm;
        window.Show();
        _ = vm.InitializeAsync(initialPlan);
    }

    private async Task<RepartPlanM?> ImportFolderAsync()
    {
        OpenFolderDialog dialog = new()
        {
            Title = RepartLangProvider.Current["SelectFolder"],
            Multiselect = false
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return null;
        string[] folderPaths = SourceFilePicker.GetVideoFilesInFolder(dialog.FolderName);
        if (folderPaths.Length < 2)
        {
            new OpenErrModalCmd(modalNavS, RepartConfVM.WindowTitleText, RepartLangProvider.Current.MinFolderSources).Execute(null);
            return null;
        }

        RepartAnalysisResult? result = await RunAnalysisAsync(folderPaths);
        if (result?.Plan == null) return null;

        new OpenSuccModalCmd(
            modalNavS,
            RepartConfVM.WindowTitleText,
            string.Format(
                RepartLangProvider.Current["ImportSummary"],
                result.Plan.Sources.Count,
                result.Excluded.Count)).Execute(null);
        return result.Plan;
    }

    private async Task<RepartPlanM?> ImportChapterFolderAsync()
    {
        PlaylistImportResult? import = await PlaylistImportService.ImportAsync(
            modalNavS,
            new PlaylistImportStrings(
                RepartLangProvider.Current["SelectPlaylistFolder"],
                RepartConfVM.WindowTitleText,
                BuildPlaylistScanFailureMessage,
                fileName => string.Format(RepartLangProvider.Current["ChapterImportFailed"], fileName),
                RepartLangProvider.Current["ChapterSourcesMissing"]));
        if (import == null) return null;

        RepartAnalysisResult? result = await RunAnalysisAsync(import.SourcePaths, requireMultipleSources: false);
        if (result?.Plan == null)
            return null;

        ApplyChapterDividers(result.Plan, import.Chapter);
        new OpenSuccModalCmd(
            modalNavS,
            RepartConfVM.WindowTitleText,
            string.Format(
                RepartLangProvider.Current["ImportSummary"],
                result.Plan.Sources.Count,
                result.Excluded.Count)).Execute(null);
        return result.Plan;
    }

    private static string BuildPlaylistScanFailureMessage(string folderPath, IReadOnlyList<string> diagnostics)
    {
        List<string> lines = [$"No usable MPLS playlists were found in: {folderPath}"];
        lines.AddRange(diagnostics.Take(8));
        return string.Join(Environment.NewLine, lines);
    }

    private async Task<RepartAnalysisResult?> RunAnalysisAsync(
        IReadOnlyList<string> filePaths,
        bool requireMultipleSources = true)
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
            filePaths: filePaths,
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
            cancellationToken: cancellation.Token,
            requireMultipleSources: requireMultipleSources);

        _ = CloseWhenCompletedAsync(analysisTask, progressWindow);
        progressWindow.ShowDialog();

        RepartAnalysisResult result;
        try
        {
            result = await analysisTask;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            new OpenErrModalCmd(modalNavS, RepartConfVM.WindowTitleText, ex.Message).Execute(null);
            return null;
        }

        if (result.Plan == null)
        {
            if (excludedItems.Count > 0)
            {
                new OpenErrModalCmd(
                    modalNavS,
                    RepartConfVM.WindowTitleText,
                    BuildExcludedSummary(excludedItems, result.FatalMessage)).Execute(null);
            }
            else
            {
                new OpenErrModalCmd(
                    modalNavS,
                    RepartConfVM.WindowTitleText,
                    result.FatalMessage ?? RepartLangProvider.Current.SourceRequired).Execute(null);
            }
            return null;
        }

        return result;
    }

    private static void ApplyChapterDividers(RepartPlanM plan, DiscChapterReadResult chapters)
    {
        if (plan.FrameRate <= 0d || plan.TotalFrames <= 0) return;

        List<(long Frame, string Name)> chapterMarkers = chapters.Chapters
            .Where(chapter => !chapter.IsSeparator)
            .Select(chapter => (
                Frame: (long)Math.Round(chapter.StartTime.TotalSeconds * plan.FrameRate),
                Name: chapter.Name))
            .Where(marker => marker.Frame >= 0 && marker.Frame < plan.TotalFrames)
            .DistinctBy(marker => marker.Frame)
            .OrderBy(marker => marker.Frame)
            .ToList();

        if (chapterMarkers.Count == 0) return;

        List<RepartOutputSegmentM> outputs = [];
        long first = 0;
        for (int i = 0; i < chapterMarkers.Count; i++)
        {
            long nextFirst = i + 1 < chapterMarkers.Count ? chapterMarkers[i + 1].Frame : plan.TotalFrames;
            long last = nextFirst - 1;
            if (last < first)
            {
                first = nextFirst;
                continue;
            }

            outputs.Add(new RepartOutputSegmentM(
                Guid.NewGuid(),
                RepartConfVM.BuildEpisodeName(i + 1, chapterMarkers[i].Name),
                first,
                last));
            first = nextFirst;
        }

        plan.Outputs.Clear();
        plan.Outputs.AddRange(outputs);
        plan.Dividers.Clear();
        plan.Dividers.AddRange(outputs.Take(outputs.Count - 1).Select(output => new RepartDividerM(Guid.NewGuid(), output.LastFrame, false)));
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
