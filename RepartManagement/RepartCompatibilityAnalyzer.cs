using System.IO;

namespace OneColumnEncoder.RepartManagement;

public enum RepartAnalysisStage
{
    CheckFiles,
    ProbeFiles,
    AnalyzeStreams,
    ValidateStreams,
    ScanFrames
}

public static class RepartCompatibilityAnalyzer
{
    // Compatibility wrapper for callers that only need the accepted plan
    // (in-window re-imports); analysis itself is shared with AnalyzeAndFilterAsync.
    public static async Task<RepartPlanM> AnalyzeAsync(
        string ffprobePath,
        string? ffmpegPath,
        IReadOnlyList<string> filePaths,
        Func<RepartInterlacedSrcInfo, bool>? confirmDiscardInterlacedSource = null,
        Func<RepartFrameCountFallbackInfo, bool>? confirmExpandFrameCountSearch = null,
        CancellationToken cancellationToken = default)
    {
        RepartAnalysisResult result = await AnalyzeAndFilterAsync(
            ffprobePath,
            ffmpegPath,
            filePaths,
            confirmDiscardInterlacedSource,
            confirmExpandFrameCountSearch,
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
        string? ffmpegPath,
        IReadOnlyList<string> filePaths,
        Func<RepartInterlacedSrcInfo, bool>? confirmDiscardInterlacedSource = null,
        Func<RepartFrameCountFallbackInfo, bool>? confirmExpandFrameCountSearch = null,
        Action<RepartAnalysisStage, int, int, string>? onFileProgress = null,
        Action<RepartExcludedSrcInfo>? onExcluded = null,
        CancellationToken cancellationToken = default,
        bool requireMultipleSources = true)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            throw new FileNotFoundException(RepartLangProvider.Current.FfprobeRequired, ffprobePath);
        if (filePaths.Count == 0)
            throw new InvalidOperationException(RepartLangProvider.Current.SourceRequired);

        List<RepartSrcFile> sourceFiles = [];
        List<RepartSrcFile> probeableSources = [];
        List<(RepartSrcFile SourceFile, RepartRawProbe Probe)> rawProbes = [];
        List<(RepartSrcFile SourceFile, RepartProbeOutcome Analysis)> analyzedProbes = [];
        List<(string Path, string DisplayName, RepartSrcProbe Probe)> candidates = [];
        List<RepartSourceM> sources = [];
        List<RepartExcludedSrcInfo> excluded = [];
        RepartVideoFormatSignature? referenceSignature = null;
        string referenceJson = string.Empty;
        int frameRateNumerator = 0;
        int frameRateDenominator = 0;
        long cumulativeFrames = 0;
        int minimumSourceCount = requireMultipleSources ? 2 : 1;

        void Exclude(RepartExcludedSrcInfo info)
        {
            excluded.Add(info);
            onExcluded?.Invoke(info);
        }

        RepartAnalysisResult? CreateInsufficientSourcesResult(int remainingSourceCount)
        {
            if (remainingSourceCount >= minimumSourceCount) return null;

            string fatalMessage = remainingSourceCount == 0 && excluded.Count > 0
                ? RepartExclusionMessages.FormatReason(excluded[0])
                : minimumSourceCount == 1
                    ? RepartLangProvider.Current.SourceRequired
                    : RepartLangProvider.Current["MinSourcesRequired"];
            return new(null, excluded, fatalMessage);
        }

        // 1. No-ffprobe filters. These checks must run before any ffprobe process
        // starts, so missing or otherwise invalid file paths are excluded cheaply.
        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = Path.GetFullPath(filePaths[i]);
            string displayName = Path.GetFileName(path);
            onFileProgress?.Invoke(RepartAnalysisStage.CheckFiles, i + 1, filePaths.Count, displayName);

            RepartSrcFileOutcome fileCheck = RepartSrcValidator.CheckWithoutFfprobe(path);
            if (fileCheck.RejectionReason != null)
            {
                Exclude(new RepartExcludedSrcInfo(
                    path,
                    displayName,
                    fileCheck.RejectionReason.Value,
                    fileCheck.Detail));
                continue;
            }

            sourceFiles.Add(fileCheck.SrcFile!);
        }

        if (CreateInsufficientSourcesResult(sourceFiles.Count) is RepartAnalysisResult insufficientAfterFileFilter)
            return insufficientAfterFileFilter;

        // 2. Simple ffprobe filters. Run only the lightweight metadata probe here;
        // files that ffprobe cannot analyze are excluded before any heavier analysis.
        for (int i = 0; i < sourceFiles.Count; i++)
        {
            RepartSrcFile sourceFile = sourceFiles[i];
            onFileProgress?.Invoke(RepartAnalysisStage.ProbeFiles, i + 1, sourceFiles.Count, sourceFile.DisplayName);

            RepartSrcFileOutcome probeable = await RepartSrcValidator.ProbeCanAnalyzeAsync(
                ffprobePath,
                sourceFile,
                cancellationToken);
            if (probeable.RejectionReason != null)
            {
                Exclude(new RepartExcludedSrcInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    probeable.RejectionReason.Value,
                    probeable.Detail));
                continue;
            }

            probeableSources.Add(probeable.SrcFile!);
        }

        if (CreateInsufficientSourcesResult(probeableSources.Count) is RepartAnalysisResult insufficientAfterSimpleProbe)
            return insufficientAfterSimpleProbe;

        // 3. ffprobe analysis. Only sources that survived the simple probe reach
        // this Repart-specific probe; failures here are excluded immediately.
        for (int i = 0; i < probeableSources.Count; i++)
        {
            RepartSrcFile sourceFile = probeableSources[i];
            onFileProgress?.Invoke(RepartAnalysisStage.AnalyzeStreams, i + 1, probeableSources.Count, sourceFile.DisplayName);

            RepartRawProbeOutcome rawProbe = await RepartSrcValidator.AnalyzeWithFfprobeAsync(
                ffprobePath,
                sourceFile,
                cancellationToken);
            if (rawProbe.RejectionReason != null)
            {
                Exclude(new RepartExcludedSrcInfo(
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
        foreach ((RepartSrcFile sourceFile, RepartRawProbe rawProbe) in rawProbes)
        {
            analyzedProbes.Add((sourceFile, RepartSrcValidator.AnalyzeProbe(rawProbe)));
        }

        List<(RepartSrcFile SourceFile, RepartSrcProbe Probe)> acceptedProbes = [];
        for (int i = 0; i < analyzedProbes.Count; i++)
        {
            (RepartSrcFile sourceFile, RepartProbeOutcome analysis) = analyzedProbes[i];
            onFileProgress?.Invoke(RepartAnalysisStage.ValidateStreams, i + 1, analyzedProbes.Count, sourceFile.DisplayName);

            if (analysis.RejectionReason != null)
            {
                RepartExcludedSrcInfo excludedInfo = new(
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
                                RepartLangProvider.Current["InterlacedSrcRejected"],
                                sourceFile.DisplayName,
                                analysis.Detail ?? string.Empty),
                            cancellationToken);
                    excluded.Add(excludedInfo);
                    continue;
                }

                Exclude(excludedInfo);
                continue;
            }

            acceptedProbes.Add((sourceFile, analysis.Probe!));
        }

        if (CreateInsufficientSourcesResult(acceptedProbes.Count) is RepartAnalysisResult insufficientAfterAnalysisFilters)
            return insufficientAfterAnalysisFilters;

        // The reference format is the signature shared by the largest total
        // amount of footage (aggregate source size), falling back to the largest
        // matching group. A plain "first file wins" reference is unreliable when
        // the folder mixes the main feature with menus/trailers of other formats,
        // which would otherwise cause the episodic set to be filtered out.
        var referenceGroup = acceptedProbes
            .GroupBy(probe => probe.Probe.Signature)
            .OrderByDescending(group => group.Sum(probe => probe.Probe.InitialLength))
            .ThenByDescending(group => group.Count())
            .First();
        RepartSrcProbe reference = referenceGroup.First().Probe;
        referenceSignature = reference.Signature;
        referenceJson = reference.RawJson;
        frameRateNumerator = reference.FrameRateNumerator;
        frameRateDenominator = reference.FrameRateDenominator;

        foreach ((RepartSrcFile sourceFile, RepartSrcProbe sourceProbe) in acceptedProbes)
        {
            if (sourceProbe.Signature != referenceSignature)
            {
                Exclude(new RepartExcludedSrcInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    RepartExclusionReason.SignatureMismatch,
                    null));
                continue;
            }

            candidates.Add((sourceFile.FilePath, sourceFile.DisplayName, sourceProbe));
        }

        if (CreateInsufficientSourcesResult(candidates.Count) is RepartAnalysisResult insufficientAfterReferenceMatch)
            return insufficientAfterReferenceMatch;

        // 5. Build the plan that will be loaded into RepartConfModal. Only sources
        // that survived every earlier filter reach the expensive frame-count scan.
        // The modal opens only after exclusions are reported.
        int completedScans = 0;
        async Task<(string Path, string DisplayName, string RawJson, RepartScanOutcome Scan)> ScanCandidateAsync(
            string path,
            string displayName,
            RepartSrcProbe sourceProbe)
        {
            RepartScanOutcome scan = await RepartSrcValidator.ScanFramesAsync(
                ffprobePath,
                ffmpegPath,
                path,
                sourceProbe,
                displayName,
                confirmExpandFrameCountSearch,
                cancellationToken);
            int completed = Interlocked.Increment(ref completedScans);
            onFileProgress?.Invoke(RepartAnalysisStage.ScanFrames, completed, candidates.Count, displayName);
            return (path, displayName, sourceProbe.RawJson, scan);
        }

        Task<(string Path, string DisplayName, string RawJson, RepartScanOutcome Scan)>[] scanTasks = candidates
            .Select(candidate => ScanCandidateAsync(
                candidate.Path,
                candidate.DisplayName,
                candidate.Probe))
            .ToArray();

        (string Path, string DisplayName, string RawJson, RepartScanOutcome Scan)[] scanResults = await Task.WhenAll(scanTasks);
        foreach ((string path, string displayName, string rawJson, RepartScanOutcome scan) in scanResults)
        {
            if (scan.RejectionReason != null)
            {
                Exclude(new RepartExcludedSrcInfo(
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
    IReadOnlyList<RepartExcludedSrcInfo> Excluded,
    string? FatalMessage);
