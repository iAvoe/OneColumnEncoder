using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.ConcatManagement;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Persistence;
using OneColumnEncoder.Json;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.Commands
{
    // Command that analyzes source video files via ffprobe, supports both single-file and queue (batch) analysis.
    public class AnalyzeSrcVideoCmd(
        Func<string> getFfprobePath,
        Func<string> getSourcePath,
        VideoAnalysisM analysis,
        Func<SourceCheckCardVM> getActiveSrcValidationCard,
        ModalNavS modalNavS,
        Func<bool>? isQueueRoute = null,
        Func<string[]>? getQueueFilePaths = null,
        Action<string[], string>? onQueueAccepted = null,
        Action<bool>? onAnalysisCompleted = null,
        Action? onCompleted = null,
        Func<bool>? isConcatRoute = null,
        Func<string[]>? getConcatFilePaths = null) : AsyncBaseCmd
    {
        private readonly Func<string> _getFfprobePath = getFfprobePath;
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly Func<SourceCheckCardVM> _getActiveSrcValidationCard = getActiveSrcValidationCard;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Func<bool>? _isQueueRoute = isQueueRoute;
        private readonly Func<string[]>? _getQueueFilePaths = getQueueFilePaths;
        private readonly Action<string[], string>? _onQueueAccepted = onQueueAccepted;
        private readonly Action<bool>? _onAnalysisCompleted = onAnalysisCompleted;
        private readonly Action? _onCompleted = onCompleted;
        private readonly Func<bool>? _isConcatRoute = isConcatRoute;
        private readonly Func<string[]>? _getConcatFilePaths = getConcatFilePaths;

        private static AnalyzeSrcVideoCmdLangProvider Lang => new(UILangProvider.Current.LanguageCode);
        private static readonly JsonSerializerOptions CachedJsonOptions = CreateJsonSerializerOptions();

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_getFfprobePath()) &&
            (IsQueueRoute()
                ? (_getQueueFilePaths?.Invoke().Length ?? 0) > 0
                : IsConcatRoute()
                    ? (_getConcatFilePaths?.Invoke().Length ?? 0) > 1
                : !string.IsNullOrWhiteSpace(_getSourcePath()));

        // Main entry: route to queue analysis or single-file analysis, then show result or error modal.
        protected override async Task ExecuteAsync(object? parameter)
        {
            _analysis.Clear();
            _onCompleted?.Invoke();

            try
            {
                if (IsQueueRoute())
                {
                    await ExecuteQueueAnalysisAsync();
                    return;
                }

                if (IsConcatRoute())
                {
                    await ExecuteConcatAnalysisAsync();
                    return;
                }

                // Single-file analysis: run ffprobe and apply results to the validation card.
                string ffprobePath = _getFfprobePath();
                string sourcePath = _getSourcePath();
                try
                {
                    string rawJson =
                        await FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, sourcePath);

                    _analysis.FfprobePath = ffprobePath;
                    _analysis.SourcePath = sourcePath;
                    _analysis.RawJson = rawJson;
                    _getActiveSrcValidationCard().ApplyFfprobeAnalysisJson(rawJson);

                    ShowSourceAnalysisCompletedModal(UILangProvider.Current["SrcAnalysis.Completed"]);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        FormatAnalysisFailureMessage(sourcePath, ex.Message),
                        ex);
                }
            }
            catch (Exception ex)
            {
                _getActiveSrcValidationCard().SetAnalysisFailedStatus();
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProvider.SrcAnalysisWindowTitle,
                    ex.Message).Execute(null);
            }
            finally
            {
                _onAnalysisCompleted?.Invoke(!string.IsNullOrWhiteSpace(_analysis.RawJson));
                _onCompleted?.Invoke();
            }
        }

        private bool IsQueueRoute() => _isQueueRoute?.Invoke() == true;
        private bool IsConcatRoute() => _isConcatRoute?.Invoke() == true;

        private async Task ExecuteConcatAnalysisAsync()
        {
            string ffprobePath = _getFfprobePath();
            string[] concatFilePaths = _getConcatFilePaths?.Invoke() ?? [];
            if (concatFilePaths.Length < 2)
                throw new InvalidOperationException(FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSources"]);

            ConcatCheckCardVM concatCard = _getActiveSrcValidationCard() as ConcatCheckCardVM
                ?? throw new InvalidOperationException("Concat source check card is not active.");

            ConcatCompatibilityAnalysisResult result = await ConcatCompatibilityAnalyzer.AnalyzeAsync(
                ffprobePath,
                concatFilePaths,
                concatCard.IsSvtav1SelectedFunc);

            if (result.HasResolutionMismatch)
                throw new InvalidOperationException(result.ResolutionMismatchMessage ?? string.Empty);

            _analysis.FfprobePath = ffprobePath;
            _analysis.SourcePath = result.ReferencePath;
            _analysis.RawJson = result.ReferenceRawJson;
            _analysis.ConcatTotalFrames = result.ConcatTotalFrames;
            _analysis.QueueRawJson = JsonSerializer.Serialize(
                new QueueRawAnalysisData([.. result.RawAnalyses.Select(entry =>
                    new QueueSourceRawAnalysis(entry.FilePath, entry.DisplayName, entry.FfprobeJson))]),
                CachedJsonOptions);
            concatCard.ApplyFfprobeAnalysisJson(result.ReferenceRawJson);
            concatCard.ApplyConcatAnalysis(concatFilePaths, allValid: true);

            string message = string.Format(UILangProvider.Current["SourceConcat.Analyzed"], concatFilePaths.Length);
            ShowSourceAnalysisCompletedModal(message);

            if (result.Warnings.Count > 0)
            {
                string warningMessage = string.Join(
                    Environment.NewLine + Environment.NewLine,
                    result.Warnings);
                new OpenWarnModalCmd(
                    _modalNavS,
                    UICaptionProvider.SourceInspect.WarnTitle,
                    warningMessage).Execute(null);
            }

            if (result.ConcatTotalFrames > 0)
            {
                string totalFramesLabel = new ClipRangeSelectorLangProvider(UILangProvider.Current.LanguageCode).SummaryTotalFramesLabel;
                new OpenDebugModalCmd(
                    _modalNavS,
                    $"Concat {totalFramesLabel}",
                    string.Format(
                        Lang.TotalFramesFormat,
                        Lang.ConcatTotalFramesLabel,
                        result.ConcatTotalFrames)).Execute(null);
            }
        }

        internal static async Task<string[]> AnalyzeAndFilterQueueFilePathsForImportAsync(
            string ffprobePath,
            IReadOnlyList<string> queueFilePaths,
            ModalNavS modalNavS)
        {
            if (queueFilePaths.Count == 0) return [];
            QueueFilterMode filterMode = queueFilePaths.Count > 1
                ? PromptQueueFilterMode(modalNavS)
                : QueueFilterMode.FirstStream;
            QueueSourceFilterResult result = await AnalyzeAndFilterQueueSourcesAsync(
                ffprobePath,
                queueFilePaths,
                modalNavS,
                () => new SourceCheckCardVM(),
                filterMode,
                shouldFilterQueue: true);
            return [.. result.Accepted.Select(entry => entry.FilePath)];
        }

        private static QueueFilterMode PromptQueueFilterMode(ModalNavS modalNavS)
        {
            OpenInfoModalCmd cmd = new(
                modalNavS,
                UILangProvider.Current["SourceQueue.FilterModeTitle"],
                UILangProvider.Current["SourceQueue.FilterModeMessage"]);
            cmd.Execute(null);
            return cmd.DialogResult == true
                ? QueueFilterMode.FirstStream
                : QueueFilterMode.WeightedVoteThenFirstStream;
        }

        // Step 1: Initialize ffprobe path and queue file list. Abort if empty
        // Step 2: For each file, run ffprobe analysis and build a candidate signature + vote weight.
        // Step 3: Select a reference candidate (first-stream or weighted-vote-then-first-stream)
        // Step 4: Filter candidates: accepted if signature matches reference, otherwise excluded
        // Step 5: Serialize accepted/excluded lists to JSON files in config directory
        // Step 6: Update analysis model with reference file results and raw JSON for all files
        // Step 7: Notify caller via _onQueueAccepted and show completion modal with summary
        private async Task ExecuteQueueAnalysisAsync()
        {
            string ffprobePath = _getFfprobePath();
            string[] queueFilePaths = _getQueueFilePaths?.Invoke() ?? [];
            if (queueFilePaths.Length == 0) return;

            QueueSrcFilterCardVM queueCard = _getActiveSrcValidationCard() as QueueSrcFilterCardVM
                ?? throw new InvalidOperationException("Queue source filter card is not active.");

            bool shouldFilterQueue = true;
            QueueFilterMode filterMode = shouldFilterQueue && queueFilePaths.Length > 1
                ? PromptQueueFilterMode(_modalNavS)
                : QueueFilterMode.FirstStream;

            QueueSourceFilterResult result = await AnalyzeAndFilterQueueSourcesAsync(
                ffprobePath,
                queueFilePaths,
                _modalNavS,
                () => new SourceCheckCardVM { IsSvtav1SelectedFunc = queueCard.IsSvtav1SelectedFunc },
                filterMode,
                shouldFilterQueue);
            QueueSourceCandidate referenceCandidate = result.ReferenceCandidate;
            List<QueueSourceEntry> accepted = result.Accepted;
            List<QueueSourceEntry> excluded = result.Excluded;

            // Step 5
            string referenceRawJson = referenceCandidate.RawJson;
            string referencePath = referenceCandidate.Entry.FilePath;
            queueCard.ApplyFfprobeAnalysisJson(referenceRawJson);

            string directory = SaveLoadBase<SaveLoadPlaceholder>.GetConfigDirectory();
            Directory.CreateDirectory(directory);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string queueJsonPath = Path.Combine(directory, $"source_queue_{timestamp}.json");
            string? excludedJsonPath = excluded.Count > 0
                ? Path.Combine(directory, $"source_queue_excluded_{timestamp}.json")
                : null;

            UTF8Encoding utf8NoBom = new(false);
            File.WriteAllText(queueJsonPath, JsonSerializer.Serialize(new QueueSourceData(referencePath, accepted), CachedJsonOptions), utf8NoBom);
            if (!string.IsNullOrWhiteSpace(excludedJsonPath))
                File.WriteAllText(excludedJsonPath, JsonSerializer.Serialize(new QueueSourceData(referencePath, excluded), CachedJsonOptions), utf8NoBom);

            // Step 6
            _analysis.FfprobePath = ffprobePath;
            _analysis.SourcePath = referencePath;
            _analysis.RawJson = referenceRawJson;
            _analysis.QueueRawJson = JsonSerializer.Serialize(new QueueRawAnalysisData(result.RawAnalyses), CachedJsonOptions);
            queueCard.ApplyQueueResult(accepted.Count, excluded.Count, queueJsonPath, excludedJsonPath ?? string.Empty);
            _onQueueAccepted?.Invoke([.. accepted.Select(entry => entry.FilePath)], queueJsonPath);

            // Step 7
            string message = string.IsNullOrWhiteSpace(excludedJsonPath)
                ? string.Format(UILangProvider.Current["SourceQueue.AnalyzedNoEx"], queueJsonPath)
                : string.Format(
                    UILangProvider.Current["SourceQueue.Analyzed"],
                    excluded.Count,
                    queueJsonPath,
                    excludedJsonPath);
            if (result.Skipped.Count > 0)
                message = FormatQueueSkippedMessage(message, result.Skipped);
            new OpenQueueAnalysisCompletedModalCmd(
                _modalNavS,
                message,
                queueJsonPath,
                excludedJsonPath).Execute(null);
        }

        private static async Task<QueueSourceFilterResult> AnalyzeAndFilterQueueSourcesAsync(
            string ffprobePath,
            IReadOnlyList<string> queueFilePaths,
            ModalNavS modalNavS,
            Func<SourceCheckCardVM> createProbeCard,
            QueueFilterMode filterMode,
            bool shouldFilterQueue)
        {
            List<QueueSourceCandidate> candidates = [];
            List<QueueSourceFailure> skipped = [];
            List<QueueSourceRawAnalysis> rawAnalyses = [];

            for (int i = 0; i < queueFilePaths.Count; i++)
            {
                string filePath = queueFilePaths[i];
                SourceCheckCardVM probeCard = createProbeCard();

                try
                {
                    string rawJson = await FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, filePath);
                    probeCard.ApplyFfprobeAnalysisJson(rawJson);
                    SourceCheckSignature signature = probeCard.GetSignature();
                    using JsonDocument rawDocument = JsonDocument.Parse(rawJson);
                    JsonElement rawElement = rawDocument.RootElement.Clone();
                    QueueSourceEntry entry = new(
                        filePath,
                        Path.GetFileName(filePath),
                        QueueSourceCheckResult.FromCard(probeCard),
                        rawElement);
                    rawAnalyses.Add(new(filePath, Path.GetFileName(filePath), rawElement));
                    candidates.Add(new(
                        candidates.Count,
                        entry,
                        QueueSourceGroupSignature.From(signature, rawElement),
                        rawJson,
                        CalculateQueueVoteWeight(rawElement)));
                }
                catch (Exception ex)
                {
                    string failureMessage = FormatAnalysisFailureMessage(
                        filePath,
                        ex.Message,
                        i + 1,
                        queueFilePaths.Count,
                        willSkipAndContinue: true);
                    new OpenErrModalCmd(
                        modalNavS,
                        UILangProvider.SrcAnalysisWindowTitle,
                        failureMessage).Execute(null);
                    skipped.Add(new(filePath, Path.GetFileName(filePath), ex.Message));
                }
            }

            if (candidates.Count == 0)
                throw new InvalidOperationException(FormatAllQueueItemsFailedMessage(queueFilePaths.Count));

            QueueSourceCandidate referenceCandidate = shouldFilterQueue
                ? SelectReferenceCandidate(candidates, filterMode)
                : candidates[0];

            List<QueueSourceEntry> accepted = [];
            List<QueueSourceEntry> excluded = [];
            foreach (QueueSourceCandidate candidate in candidates)
            {
                if (!shouldFilterQueue || candidate.GroupSignature.Matches(referenceCandidate.GroupSignature))
                    accepted.Add(candidate.Entry);
                else
                    excluded.Add(candidate.Entry);
            }

            return new(referenceCandidate, accepted, excluded, skipped, rawAnalyses);
        }

        // Delegates reference selection based on filter mode: first-stream or weighted-vote-then-first-stream
        private static QueueSourceCandidate SelectReferenceCandidate(
            IReadOnlyList<QueueSourceCandidate> candidates,
            QueueFilterMode filterMode) =>
            filterMode == QueueFilterMode.FirstStream
                ? SelectFirstStreamReferenceCandidate(candidates)
                : SelectWeightedReferenceCandidate(candidates);

        // Groups candidates by group key, then picks the group with the highest total vote weight
        // (ties broken by count, then earliest sequence). Within that group, returns the first stream.
        private static QueueSourceCandidate SelectWeightedReferenceCandidate(IReadOnlyList<QueueSourceCandidate> candidates)
        {
            IEnumerable<QueueSourceCandidate> votedGroup = candidates
                .GroupBy(candidate => candidate.GroupSignature.MatchKey, StringComparer.Ordinal)
                .Select(group => new
                {
                    Candidates = group,
                    Weight = group.Sum(candidate => candidate.VoteWeight),
                    Count = group.Count(),
                    FirstSequence = group.Min(candidate => candidate.Sequence)
                })
                .OrderByDescending(group => group.Weight)
                .ThenByDescending(group => group.Count)
                .ThenBy(group => group.FirstSequence)
                .First()
                .Candidates;

            return SelectFirstStreamReferenceCandidate(votedGroup);
        }

        // Returns the candidate with the smallest sequence number (first file in insertion order)
        private static QueueSourceCandidate SelectFirstStreamReferenceCandidate(IEnumerable<QueueSourceCandidate> candidates) =>
            candidates.OrderBy(candidate => candidate.Sequence).First();

        // Computes a vote weight for a candidate based on its duration (squared) or frame count (squared)
        // Longer / higher-frame-count files get more influence in the weighted-vote reference selection
        private static double CalculateQueueVoteWeight(JsonElement rawElement)
        {
            if (!FrameRate.TryGetFirstVideoStream(rawElement, out JsonElement stream)) return 1d;

            double? duration = TryGetDuration(rawElement, stream);
            double? fps = TryGetFramesPerSecond(stream);
            long? frameCount = TryGetFrameCount(stream);

            if (duration is not > 0 && frameCount is > 0 && fps is > 0)
                duration = frameCount.Value / fps.Value;

            if (duration is > 0)
                return Math.Pow(duration.Value, 2d);

            if (frameCount is > 0)
                return Math.Pow(frameCount.Value, 2d);

            return 1d;
        }

        // Attempts to extract duration from a stream property; falls back to the format-level duration if the stream value is missing/invalid.
        private static double? TryGetDuration(JsonElement root, JsonElement stream)
        {
            double? streamDuration = TryGetDouble(stream, "duration");
            if (streamDuration is > 0) return streamDuration;

            return root.TryGetProperty("format", out JsonElement format)
                ? TryGetDouble(format, "duration")
                : null;
        }

        // Tries avg_frame_rate first, then r_frame_rate as a fallback.
        private static double? TryGetFramesPerSecond(JsonElement stream)
        {
            if (FrameRate.TryParseFrameRate(TryGetString(stream, "avg_frame_rate"), out double avg) && avg > 0d)
                return avg;
            return FrameRate.TryParseFrameRate(TryGetString(stream, "r_frame_rate"), out double r) && r > 0d
                ? r
                : null;
        }

        // Formats an error message with optional queue progress (e.g., "3/10") and a skip-notice suffix for queue mode.
        private static string FormatAnalysisFailureMessage(
            string sourcePath,
            string detail,
            int? queueIndex = null,
            int? queueTotal = null,
            bool willSkipAndContinue = false)
        {
            List<string> lines = [];
            if (queueIndex.HasValue && queueTotal.HasValue)
                lines.Add(string.Format(Lang.QueueItemProgress, queueIndex.Value, queueTotal.Value));

            lines.Add(string.Format(Lang.SourceFilePath, sourcePath));
            lines.Add(detail);
            if (willSkipAndContinue)
            {
                lines.Add(string.Empty);
                lines.Add(Lang.QueueItemSkipMsg);
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string FormatAllQueueItemsFailedMessage(int queueCount) =>
            string.Format(Lang.AllQueueItemsFailed, queueCount);

        // Appends a list of skipped files (up to 5 details) to the existing message, with an "and N more" suffix if truncated.
        private static string FormatQueueSkippedMessage(string message, IReadOnlyList<QueueSourceFailure> skipped)
        {
            const int maxDetails = 5;
            List<string> skippedLines = [string.Format(Lang.SkippedItemsLabel, skipped.Count)];
            skippedLines.AddRange(skipped
                .Take(maxDetails)
                .Select(item => string.IsNullOrWhiteSpace(item.DisplayName) ? item.FilePath : item.DisplayName)
                .Select(detail => string.Format(Lang.ListItemPrefix, detail)));
            int omitted = skipped.Count - Math.Min(skipped.Count, maxDetails);

            if (omitted > 0) skippedLines.Add(string.Format(Lang.AndMoreLabel, omitted));
            return message
                + Environment.NewLine
                + Environment.NewLine
                + string.Join(Environment.NewLine, skippedLines);
        }

        private void ShowSourceAnalysisCompletedModal(string message)
        {
            new OpenSuccModalCmd(
                _modalNavS,
                UILangProvider.SrcAnalysisWindowTitle,
                message).Execute(null);
        }

        // Placeholder type needed to resolve the config directory from SaveLoadBase.
        private sealed class SaveLoadPlaceholder : SaveLoadBase<SaveLoadPlaceholder>
        {
            protected override string FilePath => string.Empty;
        }

        // Determines how the reference candidate is chosen from the queue.
        private enum QueueFilterMode
        {
            WeightedVoteThenFirstStream,
            FirstStream
        }

        // A candidate in queue analysis: carries its position (Sequence), analysis result, signature, raw JSON, and vote weight.
        private sealed record QueueSourceCandidate(
            int Sequence,
            QueueSourceEntry Entry,
            QueueSourceGroupSignature GroupSignature,
            string RawJson,
            double VoteWeight);

        // Composite group signature used to compare queue items: checklist results + width + frame rates.
        private sealed record QueueSourceGroupSignature(
            SourceCheckSignature CheckSignature,
            int? Width,
            string AvgFrameRate,
            string RFrameRate)
        {
            public string MatchKey => string.Join(
                "|",
                CheckSignature.MatchKey,
                Width?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                AvgFrameRate,
                RFrameRate);

            public bool Matches(QueueSourceGroupSignature other) =>
                string.Equals(MatchKey, other.MatchKey, StringComparison.Ordinal);

            public static QueueSourceGroupSignature From(SourceCheckSignature checkSignature, JsonElement rawElement)
            {
                int? width = null;
                string avgFrameRate = string.Empty;
                string rFrameRate = string.Empty;

                if (FrameRate.TryGetFirstVideoStream(rawElement, out JsonElement stream))
                {
                    if (TryGetInt(stream, "width", out int parsedWidth)) width = parsedWidth;
                    avgFrameRate = FrameRate.NormalizeFrameRate(TryGetString(stream, "avg_frame_rate"));
                    rFrameRate = FrameRate.NormalizeFrameRate(TryGetString(stream, "r_frame_rate"));
                }

                return new(checkSignature, width, avgFrameRate, rFrameRate);
            }
        }

        // A single queue entry: file path, display name, check-list results, and the raw ffprobe JSON tree.
        private sealed record QueueSourceEntry(
            string FilePath,
            string DisplayName,
            QueueSourceCheckResult CheckResult,
            JsonElement FfprobeJson);

        // Lightweight record for raw analysis data (used in the queue-raw-json snapshot).
        private sealed record QueueSourceRawAnalysis(
            string FilePath,
            string DisplayName,
            JsonElement FfprobeJson);

        // Records a file that was skipped during queue analysis due to an error.
        private sealed record QueueSourceFailure(string FilePath, string DisplayName, string ErrorMessage);

        private sealed record QueueSourceFilterResult(
            QueueSourceCandidate ReferenceCandidate,
            List<QueueSourceEntry> Accepted,
            List<QueueSourceEntry> Excluded,
            List<QueueSourceFailure> Skipped,
            List<QueueSourceRawAnalysis> RawAnalyses);

        // Holds the severe and moderate check-list items from the validation card.
        private sealed record QueueSourceCheckResult(
            IReadOnlyList<QueueSourceCheckItem> Severe,
            IReadOnlyList<QueueSourceCheckItem> Moderate)
        {
            public static QueueSourceCheckResult FromCard(SourceCheckCardVM card) =>
                new(
                    [.. card.Checklist1.Select(QueueSourceCheckItem.FromEntry)],
                    [.. card.Checklist2.Select(QueueSourceCheckItem.FromEntry)]);
        }

        // A single check-list item used in the serialized queue source result.
        private sealed record QueueSourceCheckItem(string Text, StatusType Status)
        {
            public static QueueSourceCheckItem FromEntry(ChecklistEntryVM entry) =>
                new(entry.Text, entry.Status);
        }

        // Top-level container for serialized queue data: reference file path + list of entries.
        private sealed record QueueSourceData(string ReferenceFilePath, IReadOnlyList<QueueSourceEntry> Entries);

        // Container for all raw ffprobe analyses in the queue (used for later re-inspection).
        private sealed record QueueRawAnalysisData(IReadOnlyList<QueueSourceRawAnalysis> Entries);
    }
}
