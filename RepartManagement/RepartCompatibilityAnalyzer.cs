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
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            throw new FileNotFoundException(RepartLangProvider.Current.FfprobeRequired, ffprobePath);
        if (filePaths.Count == 0)
            throw new InvalidOperationException(RepartLangProvider.Current.SourceRequired);

        List<RepartSourceFile> sourceFiles = [];
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
                excluded.Add(new RepartExcludedSourceInfo(
                    path,
                    displayName,
                    fileCheck.RejectionReason.Value,
                    fileCheck.Detail));
                continue;
            }

            sourceFiles.Add(fileCheck.SourceFile!);
        }

        // 2. Simple ffprobe filters. Run only the lightweight metadata probe here;
        // files that ffprobe cannot analyze are excluded before any heavier analysis.
        for (int i = 0; i < sourceFiles.Count; i++)
        {
            RepartSourceFile sourceFile = sourceFiles[i];
            onFileProgress?.Invoke(i + 1, sourceFiles.Count, sourceFile.DisplayName);

            RepartRawProbeOutcome rawProbe = await RepartSourceValidator.ProbeCanAnalyzeAsync(
                ffprobePath,
                sourceFile,
                cancellationToken);
            if (rawProbe.RejectionReason != null)
            {
                excluded.Add(new RepartExcludedSourceInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    rawProbe.RejectionReason.Value,
                    rawProbe.Detail));
                continue;
            }

            rawProbes.Add((sourceFile, rawProbe.Probe!));
        }

        if (rawProbes.Count == 0)
        {
            RepartExcludedSourceInfo first = excluded[0];
            return new(null, excluded, RepartExclusionMessages.FormatReason(first));
        }

        // 3. ffprobe analysis. Parse the raw ffprobe JSON into Repart-specific
        // facts, but keep the filtering decision in the next stage.
        for (int i = 0; i < rawProbes.Count; i++)
        {
            (RepartSourceFile sourceFile, RepartRawProbe rawProbe) = rawProbes[i];
            onFileProgress?.Invoke(i + 1, rawProbes.Count, sourceFile.DisplayName);

            analyzedProbes.Add((sourceFile, RepartSourceValidator.AnalyzeProbe(rawProbe)));
        }

        // 4. Filters based on ffprobe analysis. Reject interlaced/non-CFR/etc.
        // before frame counting; the expensive frame-count scan only runs after a
        // source has passed these analysis-based checks and signature matching.
        foreach ((RepartSourceFile sourceFile, RepartProbeOutcome analysis) in analyzedProbes)
        {
            if (analysis.RejectionReason != null)
            {
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
                }
                excluded.Add(new RepartExcludedSourceInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    analysis.RejectionReason.Value,
                    analysis.Detail));
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
                excluded.Add(new RepartExcludedSourceInfo(
                    sourceFile.FilePath,
                    sourceFile.DisplayName,
                    RepartExclusionReason.SignatureMismatch,
                    null));
                continue;
            }

            candidates.Add((sourceFile.FilePath, sourceFile.DisplayName, sourceProbe));
        }

        if (candidates.Count == 0)
        {
            RepartExcludedSourceInfo first = excluded[0];
            return new(null, excluded, RepartExclusionMessages.FormatReason(first));
        }

        // 5. Build the plan that will be loaded into RepartConfModal. Only sources
        // that survived every earlier filter reach the expensive frame-count scan.
        // The modal opens only after exclusions are reported.
        for (int i = 0; i < candidates.Count; i++)
        {
            (string path, string displayName, RepartSourceProbe sourceProbe) = candidates[i];
            onFileProgress?.Invoke(i + 1, candidates.Count, displayName);

            RepartScanOutcome scan = await RepartSourceValidator.ScanFramesAsync(
                ffprobePath,
                path,
                sourceProbe,
                cancellationToken);
            if (scan.RejectionReason != null)
            {
                excluded.Add(new RepartExcludedSourceInfo(
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
                sourceProbe.RawJson,
                scan.FrameCount,
                firstFrame,
                cumulativeFrames - 1,
                scan.FileLength,
                scan.LastWriteUtcTicks));
        }

        if (sources.Count == 0)
        {
            RepartExcludedSourceInfo first = excluded[0];
            return new(null, excluded, RepartExclusionMessages.FormatReason(first));
        }

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
