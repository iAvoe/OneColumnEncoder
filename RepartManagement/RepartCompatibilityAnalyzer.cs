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
            cancellationToken);
        if (result.Plan == null)
            throw new InvalidOperationException(result.FatalMessage ?? RepartLangProvider.Current.SourceRequired);
        return result.Plan;
    }

    // Runs the full Repart Mode check & filter pass for a list of files and reports
    // every excluded source with its reason. The plan is only produced when at least
    // one source was accepted (callers decide how to surface FatalMessage otherwise).
    public static async Task<RepartAnalysisResult> AnalyzeAndFilterAsync(
        string ffprobePath,
        IReadOnlyList<string> filePaths,
        Func<RepartInterlacedSourceInfo, bool>? confirmDiscardInterlacedSource = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            throw new FileNotFoundException(RepartLangProvider.Current.FfprobeRequired, ffprobePath);
        if (filePaths.Count == 0)
            throw new InvalidOperationException(RepartLangProvider.Current.SourceRequired);

        List<RepartSourceM> sources = [];
        List<RepartExcludedSourceInfo> excluded = [];
        RepartVideoFormatSignature? referenceSignature = null;
        string referenceJson = string.Empty;
        int frameRateNumerator = 0;
        int frameRateDenominator = 0;
        long cumulativeFrames = 0;

        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = Path.GetFullPath(filePaths[i]);
            RepartSourceValidation validation = await RepartSourceValidator.ValidateAsync(
                ffprobePath,
                path,
                cancellationToken);

            if (!validation.IsAccepted)
            {
                if (validation.ExclusionReason == RepartExclusionReason.Interlaced)
                {
                    bool shouldDiscard = confirmDiscardInterlacedSource?.Invoke(new(
                        path,
                        Path.GetFileName(path),
                        validation.Detail ?? string.Empty)) == true;
                    if (!shouldDiscard)
                        throw new OperationCanceledException(
                            string.Format(
                                RepartLangProvider.Current["InterlacedSourceRejected"],
                                Path.GetFileName(path),
                                validation.Detail ?? string.Empty),
                            cancellationToken);
                }
                excluded.Add(new RepartExcludedSourceInfo(
                    path,
                    Path.GetFileName(path),
                    validation.ExclusionReason ?? RepartExclusionReason.ProbeFailed,
                    validation.Detail));
                continue;
            }

            if (referenceSignature == null)
            {
                referenceSignature = validation.Signature;
                referenceJson = validation.RawJson;
                frameRateNumerator = validation.FrameRateNumerator;
                frameRateDenominator = validation.FrameRateDenominator;
            }
            else if (referenceSignature != validation.Signature)
            {
                excluded.Add(new RepartExcludedSourceInfo(
                    path,
                    Path.GetFileName(path),
                    RepartExclusionReason.SignatureMismatch,
                    null));
                continue;
            }

            long firstFrame = cumulativeFrames;
            checked { cumulativeFrames += validation.FrameCount; }
            sources.Add(new RepartSourceM(
                path,
                validation.RawJson,
                validation.FrameCount,
                firstFrame,
                cumulativeFrames - 1,
                validation.FileLength,
                validation.LastWriteUtcTicks));
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
