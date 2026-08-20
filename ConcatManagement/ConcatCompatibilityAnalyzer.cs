using System.IO;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.ConcatManagement;

public static class ConcatCompatibilityAnalyzer
{
    private static AnalyzeSrcVideoCmdLangProvider Lang => new(UILangProvider.Current.LanguageCode);

    public static async Task<ConcatCompatibilityAnalysisResult> AnalyzeAsync(
        string ffprobePath,
        string[] filePaths,
        Func<bool>? isSvtav1SelectedFunc = null)
    {
        string? referenceRawJson = null;
        string? referencePath = null;
        ConcatSourceSignature? referenceSignature = null;
        List<ConcatSourceRawAnalysis> rawAnalyses = [];
        long concatTotalFrames = 0;
        bool hasCompleteFrameCounts = true;
        List<string> warnings = [];
        List<string> variableFrameRateWarnings = [];
        bool hasResolutionMismatch = false;
        string? resolutionMismatchMessage = null;

        for (int i = 0; i < filePaths.Length; i++)
        {
            string filePath = filePaths[i];
            SrcCheckCardVM probeCard = new()
            {
                IsSvtav1SelectedFunc = isSvtav1SelectedFunc
            };

            try
            {
                string rawJson = await FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, filePath);
                probeCard.ApplyFfprobeAnalysisJson(rawJson);

                using JsonDocument rawDocument = JsonDocument.Parse(rawJson);
                JsonElement rawElement = rawDocument.RootElement.Clone();

                bool? isVariableFrameRate = IsVariableFrameRate(rawElement);
                if (isVariableFrameRate == true)
                    variableFrameRateWarnings.Add(FormatVariableFrameRateWarning(filePath));

                ConcatSourceSignature signature = ConcatSourceSignature.From(probeCard.GetSignature(), rawElement)
                    ?? throw new InvalidOperationException(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.NoVideoStream"]);

                if (referenceSignature == null)
                {
                    referenceSignature = signature;
                    referenceRawJson = rawJson;
                    referencePath = filePath;
                }
                else if (referenceSignature.Width != signature.Width
                         || referenceSignature.Height != signature.Height)
                {
                    hasResolutionMismatch = true;
                    resolutionMismatchMessage = string.Format(
                        UILangProvider.Current["SourceConcat.ResolutionMismatch"],
                        i + 1,
                        referenceSignature.Display,
                        signature.Display);
                }
                else if (!referenceSignature.MatchesEssential(signature))
                {
                    warnings.Add(string.Format(
                        UILangProvider.Current["SourceConcat.IncompatibleVideo"],
                        i + 1,
                        referenceSignature.Display,
                        signature.Display));
                }
                else if (!referenceSignature.MatchesFrameRate(signature))
                {
                    warnings.Add(string.Format(
                        UILangProvider.Current["SourceConcat.IncompatibleFrameRate"],
                        i + 1,
                        referenceSignature.Display,
                        signature.Display));
                }

                rawAnalyses.Add(new(filePath, Path.GetFileName(filePath), rawElement));

                if (FrameRate.TryGetFirstVideoStream(rawElement, out JsonElement videoStream))
                {
                    long? fragmentFrames = TryGetFrameCount(videoStream);
                    if (fragmentFrames > 0) concatTotalFrames += fragmentFrames.Value;
                    else hasCompleteFrameCounts = false;
                }
                else
                    hasCompleteFrameCounts = false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    FormatAnalysisFailureMessage(filePath, ex.Message, i + 1, filePaths.Length),
                    ex);
            }
        }

        if (referenceRawJson == null || referencePath == null)
            throw new InvalidOperationException(FormatAllItemsFailedMessage(filePaths.Length));

        return new(
            referenceRawJson,
            referencePath,
            rawAnalyses,
            hasCompleteFrameCounts ? concatTotalFrames : -1,
            warnings,
            variableFrameRateWarnings,
            hasResolutionMismatch,
            resolutionMismatchMessage);
    }

    private static string FormatAnalysisFailureMessage(
        string srcPath,
        string detail,
        int queueIndex,
        int queueTotal)
    {
        return string.Join(
            Environment.NewLine,
            string.Format(Lang.QueueItemProgress, queueIndex, queueTotal),
            string.Format(Lang.SourceFilePath, srcPath),
            detail);
    }

    private static string FormatAllItemsFailedMessage(int count) =>
        string.Format(Lang.AllQueueItemsFailed, count);

    private static bool? IsVariableFrameRate(JsonElement rawElement)
    {
        if (!FrameRate.TryGetFirstVideoStream(rawElement, out JsonElement stream)) return null;
        return FrameRate.IsVariableFrameRate(stream);
    }

    private static string FormatVariableFrameRateWarning(string filePath) =>
        string.Format(
            "{0}: {1}",
            Path.GetFileName(filePath),
            UILangProvider.Current["SourceConcat.VariableFrameRate"]);

    private sealed record ConcatSourceSignature(
        SourceCheckSignature CheckSignature,
        int Width,
        int Height,
        string PixelFormat,
        string Codec,
        string AvgFrameRate,
        string RFrameRate,
        string ColorRange,
        string ColorSpace,
        string ColorTransfer,
        string ColorPrimaries,
        string HdrSummary)
    {
        public string Display => JoinDisplayParts(
                $"{Width}x{Height}",
                FormatField("pix_fmt", PixelFormat),
                FormatField("codec", Codec),
                FormatField("avg_fps", AvgFrameRate),
                FormatField("r_fps", RFrameRate),
                FormatField("color_range", ColorRange),
                FormatField("color_space", ColorSpace),
                FormatField("color_trc", ColorTransfer),
                FormatField("color_primaries", ColorPrimaries),
                string.IsNullOrWhiteSpace(HdrSummary) ? null : $"hdr={HdrSummary}",
                $"checks=[{FormatChecklist(CheckSignature)}]");

        public bool Matches(ConcatSourceSignature other) =>
            CheckSignature.Matches(other.CheckSignature) &&
            Width == other.Width &&
            Height == other.Height &&
            string.Equals(PixelFormat, other.PixelFormat, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(Codec, other.Codec, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(AvgFrameRate, other.AvgFrameRate, StringComparison.Ordinal) &&
            string.Equals(RFrameRate, other.RFrameRate, StringComparison.Ordinal);

        public bool MatchesEssential(ConcatSourceSignature other) =>
            CheckSignature.Matches(other.CheckSignature) &&
            string.Equals(PixelFormat, other.PixelFormat, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(Codec, other.Codec, StringComparison.OrdinalIgnoreCase);

        public bool MatchesFrameRate(ConcatSourceSignature other) =>
            string.Equals(AvgFrameRate, other.AvgFrameRate, StringComparison.Ordinal) &&
            string.Equals(RFrameRate, other.RFrameRate, StringComparison.Ordinal);

        public static ConcatSourceSignature? From(SourceCheckSignature checkSignature, JsonElement rawElement)
        {
            if (!FrameRate.TryGetFirstVideoStream(rawElement, out JsonElement stream)) return null;
            if (!TryGetInt(stream, "width", out int width)) return null;
            if (!TryGetInt(stream, "height", out int height)) return null;

            FFProbeHdrInfo hdrInfo = FFProbeHdrInfoReader.Read(rawElement);

            return new(
                checkSignature,
                width,
                height,
                TryGetString(stream, "pix_fmt") ?? string.Empty,
                TryGetString(stream, "codec_name") ?? string.Empty,
                FrameRate.NormalizeFrameRate(TryGetString(stream, "avg_frame_rate")),
                FrameRate.NormalizeFrameRate(TryGetString(stream, "r_frame_rate")),
                TryGetString(stream, "color_range") ?? string.Empty,
                TryGetString(stream, "color_space") ?? string.Empty,
                TryGetString(stream, "color_transfer") ?? string.Empty,
                TryGetString(stream, "color_primaries") ?? string.Empty,
                hdrInfo.Summary ?? string.Empty);
        }

        private static string JoinDisplayParts(params string?[] parts) =>
            string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!));

        private static string FormatField(string name, string value) =>
            string.IsNullOrWhiteSpace(value) ? $"{name}=?" : $"{name}={value}";

        private static string FormatChecklist(SourceCheckSignature signature)
        {
            List<string> parts = [];
            ChecklistItemDefinitionM[] checklist1 = [.. ChecklistProviderM.GetSrcChecklist1()];
            ChecklistItemDefinitionM[] checklist2 = [.. ChecklistProviderM.GetSrcChecklist2()];

            for (int i = 0; i < Math.Min(signature.Checklist1.Length, checklist1.Length); i++)
                parts.Add($"{checklist1[i].Text}={FormatStatus(signature.Checklist1[i])}");

            for (int i = 0; i < Math.Min(signature.Checklist2.Length, checklist2.Length); i++)
                parts.Add($"{checklist2[i].Text}={FormatStatus(signature.Checklist2[i])}");

            return string.Join(", ", parts);
        }

        private static string FormatStatus(StatusType status) => status.ToString().ToLowerInvariant();
    }
}

public sealed record ConcatCompatibilityAnalysisResult(
    string ReferenceRawJson,
    string ReferencePath,
    IReadOnlyList<ConcatSourceRawAnalysis> RawAnalyses,
    long ConcatTotalFrames,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> VariableFrameRateWarnings,
    bool HasResolutionMismatch,
    string? ResolutionMismatchMessage);

public sealed record ConcatSourceRawAnalysis(
    string FilePath,
    string DisplayName,
    JsonElement FfprobeJson);
