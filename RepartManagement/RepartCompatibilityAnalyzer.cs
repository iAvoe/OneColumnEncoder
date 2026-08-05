using OneColumnEncoder.Models;
using System.IO;

namespace OneColumnEncoder.RepartManagement;

public static class RepartCompatibilityAnalyzer
{
    // Compatibility wrapper for callers that only need the accepted plan
    // (in-window re-imports); analysis itself is shared with AnalyzeAndFilterAsync.
    public static async Task<RepartPlanM> AnalyzeAsync(
        string ffprobePath,
        IReadOnlyList<string> filePaths,
        Func<RepartInterlacedSourceInfo, bool>? confirmDiscardInterlacedSource = null,
        CancellationToken cancellationToken = default)
    {
        RepartAnalysisResult result = await AnalyzeAndFilterAsync(
            ffprobePath,
            filePaths,
            confirmDiscardInterlacedSource,
            onFileProgress: null,
            onExcluded: null,
            cancellationToken);
        if (result.Plan == null)
            throw new InvalidOperationException(result.FatalMessage ?? RepartLangProvider.Current.SourceRequired);
        return result.Plan;
    }

    // Runs the full Repart Mode check & filter pass for a list of files and reports
    // every excluded source with its reason. The plan is only produced when at least
    // one source was accepted (callers decide how to surface FatalMessage otherwise).
    // The expensive full-file frame-count scan only runs for files that passed the
    // probe checks AND matched the reference signature.
    public static async Task<RepartAnalysisResult> AnalyzeAndFilterAsync(
        string ffprobePath,
        IReadOnlyList<string> filePaths,
        Func<RepartInterlacedSourceInfo, bool>? confirmDiscardInterlacedSource = null,
        Action<int, int, string>? onFileProgress = null,
        Action<RepartExcludedSourceInfo>? onExcluded = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            throw new FileNotFoundException(RepartLangProvider.Current.FfprobeRequired, ffprobePath);
        if (filePaths.Count == 0)
            throw new InvalidOperationException(RepartLangProvider.Current.SourceRequired);

        List<RepartSourceFile> sourceFiles = [];
        List<RepartSourceFile> probeableSources = [];
        List<(RepartSourceFile SourceFile, RepartRawProbe Probe)> rawProbes = [];
        List<(RepartSourceFile SourceFile, RepartProbeOutcome Analysis)> analyzedProbes = [];
        List<(string Path, string DisplayName, RepartSourceProbe Probe)> candidates = [];
        List<RepartSourceM> sources = [];
        List<RepartExcludedSourceInfo> excluded = [];
        RepartVideoFormatSignature? referenceSignature = null;
        string referenceJson = string.Empty;
        int frameRateNumerator = 0;
        int frameRateDenominator = 0;
        long cumulativeFrames = 0;

        void Exclude(RepartExcludedSourceInfo info)
        {
            excluded.Add(info);
            onExcluded?.Invoke(info);
        }

        RepartAnalysisResult? CreateInsufficientSourcesResult(int remainingSourceCount)
        {
            if (remainingSourceCount >= 2) return null;

            string fatalMessage = remainingSourceCount == 0 && excluded.Count > 0
                ? RepartExclusionMessages.FormatReason(excluded[0])
                : RepartLangProvider.Current["MinSourcesRequired"];
            return new(null, excluded, fatalMessage);
        }

        // 1. No-ffprobe filters. These checks must run before any ffprobe process
        // starts, so missing or otherwise invalid file paths are excluded cheaply.
        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = Path.GetFullPath(filePaths[i]);
            string displayName = Path.GetFileName(path);
            onFileProgress?.Invoke(i + 1, filePaths.Count, displayName);

            RepartSourceFileOutcome fileCheck = RepartSourceValidator.CheckWithoutFfprobe(path);
            if (fileCheck.RejectionReason != null)
            {
                Exclude(new RepartExcludedSourceInfo(
                    path,
                    displayName,
                    fileCheck.RejectionReason.Value,
                    fileCheck.Detail));
                continue;
            }

            sourceFiles.Add(fileCheck.SourceFile!);
        }

        if (CreateInsufficientSourcesResult(sourceFiles.Count) is RepartAnalysisResult insufficientAfterFileFilter)
            return insufficientAfterFileFilter;

        // 2. Simple ffprobe filters. Run only the lightweight metadata probe here;
        // files that ffprobe cannot analyze are excluded before any heavier analysis.
        for (int i = 0; i < sourceFiles.Count; i++)
        {
            RepartSourceFile sourceFile = sourceFiles[i];
            onFileProgress?.Invoke(i + 1, sourceFiles.Count, sourceFile.DisplayName);

            RepartSourceFileOutcome probeable = await RepartSourceValidator.ProbeCanAnalyzeAsync(
                ffprobePath,
                sourceFile,
                cancellationToken);
            if (probeable.RejectionReason != null)
            {
                Exclude(new RepartExcludedSourceInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    probeable.RejectionReason.Value,
                    probeable.Detail));
                continue;
            }

            probeableSources.Add(probeable.SourceFile!);
        }

        if (CreateInsufficientSourcesResult(probeableSources.Count) is RepartAnalysisResult insufficientAfterSimpleProbe)
            return insufficientAfterSimpleProbe;

        // 3. ffprobe analysis. Only sources that survived the simple probe reach
        // this Repart-specific probe; failures here are excluded immediately.
        for (int i = 0; i < probeableSources.Count; i++)
        {
            RepartSourceFile sourceFile = probeableSources[i];
            onFileProgress?.Invoke(i + 1, probeableSources.Count, sourceFile.DisplayName);

            RepartRawProbeOutcome rawProbe = await RepartSourceValidator.AnalyzeWithFfprobeAsync(
                ffprobePath,
                sourceFile,
                cancellationToken);
            if (rawProbe.RejectionReason != null)
            {
                Exclude(new RepartExcludedSourceInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    rawProbe.RejectionReason.Value,
                    rawProbe.Detail));
                continue;
            }

            rawProbes.Add((sourceFile, rawProbe.Probe!));
        }

        if (CreateInsufficientSourcesResult(rawProbes.Count) is RepartAnalysisResult insufficientAfterFfprobeAnalysis)
            return insufficientAfterFfprobeAnalysis;

        // 4. Filters based on ffprobe analysis. Parse the Repart-specific raw
        // JSON and reject interlaced/non-CFR/etc. before frame counting.
        foreach ((RepartSourceFile sourceFile, RepartRawProbe rawProbe) in rawProbes)
        {
            analyzedProbes.Add((sourceFile, RepartSourceValidator.AnalyzeProbe(rawProbe)));
        }

        foreach ((RepartSourceFile sourceFile, RepartProbeOutcome analysis) in analyzedProbes)
        {
            if (analysis.RejectionReason != null)
            {
                RepartExcludedSourceInfo excludedInfo = new(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    analysis.RejectionReason.Value,
                    analysis.Detail);

                if (analysis.RejectionReason == RepartExclusionReason.Interlaced)
                {
                    bool shouldDiscard = confirmDiscardInterlacedSource?.Invoke(new(
                        sourceFile.FilePath,
                        sourceFile.DisplayName,
                        analysis.Detail ?? string.Empty)) == true;
                    if (!shouldDiscard)
                        throw new OperationCanceledException(
                            string.Format(
                                RepartLangProvider.Current["InterlacedSourceRejected"],
                                sourceFile.DisplayName,
                                analysis.Detail ?? string.Empty),
                            cancellationToken);
                    excluded.Add(excludedInfo);
                    continue;
                }

                Exclude(excludedInfo);
                continue;
            }

            RepartSourceProbe sourceProbe = analysis.Probe!;
            if (referenceSignature == null)
            {
                referenceSignature = sourceProbe.Signature;
                referenceJson = sourceProbe.RawJson;
                frameRateNumerator = sourceProbe.FrameRateNumerator;
                frameRateDenominator = sourceProbe.FrameRateDenominator;
            }
            else if (referenceSignature != sourceProbe.Signature)
            {
                Exclude(new RepartExcludedSourceInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    RepartExclusionReason.SignatureMismatch,
                    null));
                continue;
            }

            candidates.Add((sourceFile.FilePath, sourceFile.DisplayName, sourceProbe));
        }

        if (CreateInsufficientSourcesResult(candidates.Count) is RepartAnalysisResult insufficientAfterAnalysisFilters)
            return insufficientAfterAnalysisFilters;

        // 5. Build the plan that will be loaded into RepartConfModal. Only sources
        // that survived every earlier filter reach the expensive frame-count scan.
        // The modal opens only after exclusions are reported.
        async Task<(string Path, string DisplayName, string RawJson, RepartScanOutcome Scan)> ScanCandidateAsync(
            string path,
            string displayName,
            RepartSourceProbe sourceProbe,
            int index)
        {
            onFileProgress?.Invoke(index + 1, candidates.Count, displayName);
            RepartScanOutcome scan = await RepartSourceValidator.ScanFramesAsync(
                ffprobePath,
                path,
                sourceProbe,
                cancellationToken);
            return (path, displayName, sourceProbe.RawJson, scan);
        }

        Task<(string Path, string DisplayName, string RawJson, RepartScanOutcome Scan)>[] scanTasks = candidates
            .Select((candidate, index) => ScanCandidateAsync(
                candidate.Path,
                candidate.DisplayName,
                candidate.Probe,
                index))
            .ToArray();

        (string Path, string DisplayName, string RawJson, RepartScanOutcome Scan)[] scanResults = await Task.WhenAll(scanTasks);
        foreach ((string path, string displayName, string rawJson, RepartScanOutcome scan) in scanResults)
        {
            if (scan.RejectionReason != null)
            {
                Exclude(new RepartExcludedSourceInfo(
                    path,
                    displayName,
                    scan.RejectionReason.Value,
                    scan.Detail));
                continue;
            }

            long firstFrame = cumulativeFrames;
            checked { cumulativeFrames += scan.FrameCount; }
            sources.Add(new RepartSourceM(
                path,
                rawJson,
                scan.FrameCount,
                firstFrame,
                cumulativeFrames - 1,
                scan.FileLength,
                scan.LastWriteUtcTicks));
        }

        if (CreateInsufficientSourcesResult(sources.Count) is RepartAnalysisResult insufficientAfterFrameScan)
            return insufficientAfterFrameScan;

        RepartPlanM plan = new()
        {
            FfprobePath = ffprobePath,
            ReferenceRawJson = referenceJson,
            FormatSignature = referenceSignature,
            FrameRateNumerator = frameRateNumerator,
            FrameRateDenominator = frameRateDenominator,
            TotalFrames = cumulativeFrames,
            Sources = sources
        };
        return new(plan, excluded, null);
    }
}

public sealed record RepartAnalysisResult(
    RepartPlanM? Plan,
    IReadOnlyList<RepartExcludedSourceInfo> Excluded,
    string? FatalMessage);
