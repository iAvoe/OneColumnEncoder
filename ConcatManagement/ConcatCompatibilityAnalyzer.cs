using OneColumnEncoder.FFmpeg;
using static OneColumnEncoder.Json.JsonElementHelper;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace OneColumnEncoder.ConcatManagement
{
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
                SourceCheckCardVM probeCard = new()
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
            string sourcePath,
            string detail,
            int queueIndex,
            int queueTotal)
        {
            return string.Join(
                Environment.NewLine,
                string.Format(Lang.QueueItemProgress, queueIndex, queueTotal),
                string.Format(Lang.SourceFilePath, sourcePath),
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
            string RFrameRate)
        {
            public string Display => string.Join(
                ", ",
                $"{Width}x{Height}",
                string.IsNullOrWhiteSpace(PixelFormat) ? "pix_fmt=?" : $"pix_fmt={PixelFormat}",
                string.IsNullOrWhiteSpace(Codec) ? "codec=?" : $"codec={Codec}",
                string.IsNullOrWhiteSpace(AvgFrameRate) ? "avg_fps=?" : $"avg_fps={AvgFrameRate}",
                string.IsNullOrWhiteSpace(RFrameRate) ? "r_fps=?" : $"r_fps={RFrameRate}");

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

                return new(
                    checkSignature,
                    width,
                    height,
                    TryGetString(stream, "pix_fmt") ?? string.Empty,
                    TryGetString(stream, "codec_name") ?? string.Empty,
                    FrameRate.NormalizeFrameRate(TryGetString(stream, "avg_frame_rate")),
                    FrameRate.NormalizeFrameRate(TryGetString(stream, "r_frame_rate")));
            }
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
}
