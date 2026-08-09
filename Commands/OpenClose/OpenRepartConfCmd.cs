using OneColumnEncoder.ChapterTool;
using OneColumnEncoder.RepartManagement;

namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the Repart configuration modal. If a plan already exists, it is shown directly;
/// otherwise the sources are imported, analyzed, and passed to the modal for editing.
/// </summary>
public sealed class OpenRepartConfCmd(
    ModalNavS modalNavS,
    Func<string> getFfprobePath,
    Func<string?>? getFfmpegPath,
    Func<RepartPlanM?> getCurrentPlan,
    Action<RepartPlanM> applyPlan) : OpenCloseBase(modalNavS)
{
    /// <summary>
    /// Brings an already-open window to the front; otherwise imports and analyzes sources,
    /// then shows the configuration modal with the plan.
    /// </summary>
    public override async void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<RepartConfModal>())
            return;

        RepartPlanM? currentPlan = getCurrentPlan();
        RepartPlanM? initialPlan = currentPlan;
        if (currentPlan == null)
        {
            bool importAsChapterFile = RepartChapterImportPrompt.Confirm(ModalNavS);
            initialPlan = importAsChapterFile
                ? await ImportChapterFolderAsync()
                : await ImportFolderAsync();
            if (initialPlan == null) return;
        }

        RepartConfModal window = new();
        RepartConfVM vm = new(ModalNavS, window.Close, applyPlan, getFfmpegPath?.Invoke(), getFfprobePath());
        ShowModal(window, vm, closeOpenStack: true);
        _ = vm.InitializeAsync(initialPlan);
    }

    /// <summary>
    /// Lets the user pick a source folder, runs compatibility analysis, and shows an import summary.
    /// </summary>
    /// <returns>The analyzed plan, or null if cancelled or no plan could be produced.</returns>
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
            new OpenErrModalCmd(ModalNavS, RepartConfVM.WindowTitleText, RepartLangProvider.Current.MinFolderSources).Execute(null);
            return null;
        }

        RepartAnalysisResult? result = await RunAnalysisAsync(folderPaths);
        if (result?.Plan == null) return null;

        new OpenSuccModalCmd(
            ModalNavS,
            RepartConfVM.WindowTitleText,
            string.Format(
                RepartLangProvider.Current["ImportSummary"],
                result.Plan.Sources.Count,
                result.Excluded.Count)).Execute(null);
        return result.Plan;
    }

    /// <summary>
    /// Imports a disc chapter/playlist folder and builds output segments and dividers from chapter markers.
    /// </summary>
    /// <returns>The analyzed plan with chapter-based segments, or null if cancelled or no plan was produced.</returns>
    private async Task<RepartPlanM?> ImportChapterFolderAsync()
    {
        PlaylistImportResult? import = await PlaylistImportService.ImportAsync(
            ModalNavS,
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
            ModalNavS,
            RepartConfVM.WindowTitleText,
            string.Format(
                RepartLangProvider.Current["ImportSummary"],
                result.Plan.Sources.Count,
                result.Excluded.Count)).Execute(null);
        return result.Plan;
    }

    /// <summary>
    /// Builds an error message for a scan with no usable MPLS playlists, listing the folder and diagnostics.
    /// </summary>
    private static string BuildPlaylistScanFailureMessage(string folderPath, IReadOnlyList<string> diagnostics)
    {
        List<string> lines = [$"No usable MPLS playlists were found in: {folderPath}"];
        lines.AddRange(diagnostics.Take(8));
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Runs compatibility checks and filtering on the given files before the window opens,
    /// showing a cancellable progress modal and reporting excluded sources in a single summary.
    /// </summary>
    /// <param name="filePaths">The video files to analyze.</param>
    /// <param name="requireMultipleSources">If true, at least two sources must pass.</param>
    /// <returns>The analysis result, or null if cancelled, errored, or no plan could be formed.</returns>
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
        AttachModal(progressWindow, progressVM, onClosed: () => cancellation.Cancel());

        int excludedCount = 0;
        List<RepartExcludedSourceInfo> excludedItems = [];
        Task<RepartAnalysisResult> analysisTask = RepartCompatibilityAnalyzer.AnalyzeAndFilterAsync(
            ffprobePath: getFfprobePath(),
            ffmpegPath: getFfmpegPath?.Invoke(),
            filePaths: filePaths,
            confirmDiscardInterlacedSource: source => RepartInterlacedPrompt.Confirm(ModalNavS, RepartConfVM.WindowTitleText, source),
            confirmExpandFrameCountSearch: source => RepartFrameCountPrompt.Confirm(ModalNavS, RepartConfVM.WindowTitleText, source),
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
            new OpenErrModalCmd(ModalNavS, RepartConfVM.WindowTitleText, ex.Message).Execute(null);
            return null;
        }

        if (result.Plan == null)
        {
            if (excludedItems.Count > 0)
            {
                new OpenErrModalCmd(
                    ModalNavS,
                    RepartConfVM.WindowTitleText,
                    BuildExcludedSummary(excludedItems, result.FatalMessage)).Execute(null);
            }
            else
            {
                new OpenErrModalCmd(
                    ModalNavS,
                    RepartConfVM.WindowTitleText,
                    result.FatalMessage ?? RepartLangProvider.Current.SourceRequired).Execute(null);
            }
            return null;
        }

        return result;
    }

    /// <summary>
    /// Replaces the plan's segments and dividers with chapter-derived segments,
    /// converting chapter start times to frames using the plan's frame rate.
    /// </summary>
    private static void ApplyChapterDividers(RepartPlanM plan, DiscChapterReadResult chapters)
    {
        if (plan.FrameRate <= 0d || plan.TotalFrames <= 0) return;

        List<(long Frame, string Name)> chapterMarkers = [.. chapters.Chapters
            .Where(chapter => !chapter.IsSeparator)
            .Select(chapter => (
                Frame: (long)Math.Round(chapter.StartTime.TotalSeconds * plan.FrameRate),
                chapter.Name))
            .Where(marker => marker.Frame >= 0 && marker.Frame < plan.TotalFrames)
            .DistinctBy(marker => marker.Frame)
            .OrderBy(marker => marker.Frame)];

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

    /// <summary>
    /// Closes the progress modal once the task completes, regardless of its outcome.
    /// </summary>
    private static async Task CloseWhenCompletedAsync(Task task, ProgressModal modal)
    {
        try { await task; } catch {}
        if (modal.IsVisible) modal.Close();
    }

    /// <summary>
    /// Maps an analysis stage to its localized progress label key.
    /// </summary>
    private static string StageKey(RepartAnalysisStage stage) => stage switch
    {
        RepartAnalysisStage.CheckFiles => "StageCheckFiles",
        RepartAnalysisStage.ProbeFiles => "StageProbeFiles",
        RepartAnalysisStage.AnalyzeStreams => "StageAnalyzeStreams",
        RepartAnalysisStage.ValidateStreams => "StageValidateStreams",
        RepartAnalysisStage.ScanFrames => "StageScanFrames",
        _ => "StageCheckFiles"
    };

    /// <summary>
    /// Builds the no-plan error message, combining the excluded sources summary with any fatal message.
    /// </summary>
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

    /// <summary>
    /// Formats an excluded source as its display name plus the localized exclusion reason.
    /// </summary>
    private static string FormatExcludedSummaryLine(RepartExcludedSourceInfo info) =>
        string.Join(
            Environment.NewLine,
            info.DisplayName,
            RepartExclusionMessages.FormatReason(info));
}
