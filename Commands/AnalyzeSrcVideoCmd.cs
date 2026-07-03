using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.ConcatManagement;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Persistence;
using OneColumnEncoder.Json;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

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

        private static AnalyzeSrcVideoCmdLangProviderM Lang => new(UILangProviderM.Current.LanguageCode);
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
                    ? (_getConcatFilePaths?.Invoke().Length ?? 0) > 0
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

                // Single-file analysis: run ffprobe, supplement frame count, apply results to the validation card.
                string ffprobePath = _getFfprobePath();
                string sourcePath = _getSourcePath();
                try
                {
                    string rawJson =
                        await FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, sourcePath);
                    FFProbeFrameCountSupplementResult supplementResult =
                        FFProbeFrameCountSupplement.Supplement(rawJson);
                    rawJson = supplementResult.RawJson;

                    _analysis.FfprobePath = ffprobePath;
                    _analysis.SourcePath = sourcePath;
                    _analysis.RawJson = rawJson;
                    _getActiveSrcValidationCard().ApplyFfprobeAnalysisJson(rawJson);

                    if (supplementResult.IsNbFramesCalculated)
                        ShowFrameCountSupplementedModal(FormatFrameCountSupplementMessage(rawJson, supplementResult.SupplementedCount));
                    else
                        ShowSourceAnalysisCompletedModal(UILangProviderM.Current["SrcAnalysis.Completed"]);
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
                    UILangProviderM.SrcAnalysisWindowTitle,
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
            if (concatFilePaths.Length == 0) return;

            ConcatCheckCardVM concatCard = _getActiveSrcValidationCard() as ConcatCheckCardVM
                ?? throw new InvalidOperationException("Concat source check card is not active.");

            ConcatCompatibilityAnalysisResult result = await ConcatCompatibilityAnalyzer.AnalyzeAsync(
                ffprobePath,
                concatFilePaths,
                concatCard.IsSvtav1SelectedFunc);

            if (result.Warnings.Count > 0)
            {
                string warningMessage = string.Join(
                    Environment.NewLine + Environment.NewLine,
                    result.Warnings);
                new OpenWarnModalCmd(
                    _modalNavS,
                    UICaptionProviderM.SourceInspect.WarnTitle,
                    warningMessage).Execute(null);
            }

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

            string message = string.Format(UILangProviderM.Current["SourceConcat.Analyzed"], concatFilePaths.Length);
            if (result.SupplementedCount > 0)
                message = FormatQueueFrameCountSupplementMessage(message, result.SupplementedCount);
            ShowSourceAnalysisCompletedModal(message);

            if (result.ConcatTotalFrames > 0)
            {
                string totalFramesLabel = new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode).SummaryTotalFramesLabel;
                new OpenDebugModalCmd(
                    _modalNavS,
                    $"Concat {totalFramesLabel}",
                    string.Format(
                        Lang.TotalFramesFormat,
                        Lang.ConcatTotalFramesLabel,
                        result.ConcatTotalFrames)).Execute(null);
            }
        }

        private QueueFilterMode PromptQueueFilterMode()
        {
            OpenInfoModalCmd cmd = new(
                _modalNavS,
                UILangProviderM.Current["SourceQueue.FilterModeTitle"],
                UILangProviderM.Current["SourceQueue.FilterModeMessage"]);
            cmd.Execute(null);
            return cmd.DialogResult == true
                ? QueueFilterMode.FirstStream
                : QueueFilterMode.WeightedVoteThenFirstStream;
        }

        // Step 1: Initialize ffprobe path and queue file list. Abort if empty
        // Step 2: For each file, run ffprobe analysis, supplement frame count, build a candidate signature + vote weight.
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

            List<QueueSourceCandidate> candidates = [];
            List<QueueSourceFailure> skipped = [];
            List<QueueSourceRawAnalysis> rawAnalyses = [];
            int supplementedCount = 0;

            bool shouldFilterQueue = !queueCard.IsBypassed;
            QueueFilterMode filterMode = shouldFilterQueue && queueFilePaths.Length > 1
                ? PromptQueueFilterMode()
                : QueueFilterMode.FirstStream;

            // Step 2
            for (int i = 0; i < queueFilePaths.Length; i++)
            {
                string filePath = queueFilePaths[i];
                SourceCheckCardVM probeCard = new()
                {
                    IsSvtav1SelectedFunc = queueCard.IsSvtav1SelectedFunc
                };

                try
                {
                    string rawJson = await FFProbeVideoAnalysis.AnalyzeAsync(ffprobePath, filePath);
                    FFProbeFrameCountSupplementResult supplementResult = FFProbeFrameCountSupplement.Supplement(rawJson);
                    rawJson = supplementResult.RawJson;
                    supplementedCount += supplementResult.SupplementedCount;
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
                        queueFilePaths.Length,
                        willSkipAndContinue: true);
                    new OpenErrModalCmd(
                        _modalNavS,
                        UILangProviderM.SrcAnalysisWindowTitle,
                        failureMessage).Execute(null);
                    skipped.Add(new(filePath, Path.GetFileName(filePath), ex.Message));
                }
            }

            if (candidates.Count == 0)
                throw new InvalidOperationException(FormatAllQueueItemsFailedMessage(queueFilePaths.Length));

            // Step 3
            QueueSourceCandidate referenceCandidate = shouldFilterQueue
                ? SelectReferenceCandidate(candidates, filterMode)
                : candidates[0];

            // Step 4
            List<QueueSourceEntry> accepted = [];
            List<QueueSourceEntry> excluded = [];
            foreach (QueueSourceCandidate candidate in candidates)
            {
                if (!shouldFilterQueue || candidate.GroupSignature.Matches(referenceCandidate.GroupSignature))
                    accepted.Add(candidate.Entry);
                else
                    excluded.Add(candidate.Entry);
            }

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
            _analysis.QueueRawJson = JsonSerializer.Serialize(new QueueRawAnalysisData(rawAnalyses), CachedJsonOptions);
            queueCard.ApplyQueueResult(accepted.Count, excluded.Count, queueJsonPath, excludedJsonPath ?? string.Empty);
            _onQueueAccepted?.Invoke([.. accepted.Select(entry => entry.FilePath)], queueJsonPath);

            // Step 7
            string message = string.IsNullOrWhiteSpace(excludedJsonPath)
                ? string.Format(UILangProviderM.Current["SourceQueue.AnalyzedNoEx"], queueJsonPath)
                : string.Format(
                    UILangProviderM.Current["SourceQueue.Analyzed"],
                    excluded.Count,
                    queueJsonPath,
                    excludedJsonPath);
            if (supplementedCount > 0)
                message = FormatQueueFrameCountSupplementMessage(message, supplementedCount);
            if (skipped.Count > 0)
                message = FormatQueueSkippedMessage(message, skipped);
            ShowQueueAnalysisCompletedModal(message, queueJsonPath, excludedJsonPath);
        }

        // Builds a message indicating analysis completed and (if available) total frame count + number of supplemented streams
        private static string FormatFrameCountSupplementMessage(string rawJson, int supplementedCount)
        {
            List<string> lines = [UILangProviderM.Current["SrcAnalysis.Completed"]];
            if (TryGetTotalFrameCount(rawJson, out long totalFrameCount))
                lines.Add(string.Format(Lang.TotalFramesFormat, new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode).SummaryTotalFramesLabel, totalFrameCount));

            lines.Add(string.Format(UILangProviderM.Current["SrcAnalysis.FrameCountSupplemented"], supplementedCount));
            return string.Join(Environment.NewLine + Environment.NewLine, lines);
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
            if (!TryGetFirstVideoStream(rawElement, out JsonElement stream)) return 1d;

            double? duration = TryGetDuration(rawElement, stream);
            double? fps = TryGetFramesPerSecond(stream);
            long? frameCount = JsonElementHelper.TryGetFrameCount(stream);

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
            double? streamDuration = JsonElementHelper.TryGetDouble(stream, "duration");
            if (streamDuration is > 0) return streamDuration;

            return root.TryGetProperty("format", out JsonElement format)
                ? JsonElementHelper.TryGetDouble(format, "duration")
                : null;
        }

        // Tries avg_frame_rate first, then r_frame_rate as a fallback.
        private static double? TryGetFramesPerSecond(JsonElement stream)
        {
            if (TryParseFrameRate(JsonElementHelper.TryGetString(stream, "avg_frame_rate"), out double avg) && avg > 0d)
                return avg;
            return TryParseFrameRate(JsonElementHelper.TryGetString(stream, "r_frame_rate"), out double r) && r > 0d
                ? r
                : null;
        }

        // Parses ffprobe frame rate strings like "30000/1001" or "29.97" into a double.
        private static bool TryParseFrameRate(string? value, out double fps)
        {
            fps = 0d;
            if (string.IsNullOrWhiteSpace(value) || value.Equals("0/0", StringComparison.OrdinalIgnoreCase)) return false;

            string[] parts = value.Split('/');
            if (parts.Length == 2
                && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double numerator)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double denominator)
                && denominator != 0d)
            {
                fps = numerator / denominator;
                return true;
            }

            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out fps);
        }

        // Iterates ffprobe stream array and returns the first video stream (codec_type == null or "video").
        private static bool TryGetFirstVideoStream(JsonElement root, out JsonElement stream)
        {
            stream = default;
            if (!root.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
                return false;

            foreach (JsonElement item in streams.EnumerateArray())
            {
                string? codecType = JsonElementHelper.TryGetString(item, "codec_type");
                if (codecType is null || codecType.Equals("video", StringComparison.OrdinalIgnoreCase))
                {
                    stream = item;
                    return true;
                }
            }

            return false;
        }

        // Parses total frame count from the first stream of the raw ffprobe JSON.
        private static bool TryGetTotalFrameCount(string rawJson, out long totalFrameCount)
        {
            totalFrameCount = 0;

            try
            {
                using JsonDocument document = JsonDocument.Parse(rawJson);
                JsonElement stream = document.RootElement.GetProperty("streams")[0];
                long? parsedTotalFrames = JsonElementHelper.TryGetFrameCount(stream);
                if (parsedTotalFrames is null)
                    return false;

                totalFrameCount = parsedTotalFrames.Value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string FormatQueueFrameCountSupplementMessage(string message, int supplementedCount) =>
            message
            + Environment.NewLine
            + Environment.NewLine
            + string.Format(UILangProviderM.Current["SrcAnalysis.FrameCountSupplemented"], supplementedCount);

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

        // Shows an info-level modal when frame count was supplemented for a single-file analysis.
        private void ShowFrameCountSupplementedModal(string message)
        {
            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateInfo(
                UILangProviderM.SrcAnalysisWindowTitle,
                message,
                closeCmd,
                closeCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        // Shows a success-level modal after single-file analysis completes without frame-count supplementation.
        private void ShowSourceAnalysisCompletedModal(string message)
        {
            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateSuccess(
                UILangProviderM.SrcAnalysisWindowTitle,
                message,
                closeCmd,
                closeCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        // Shows a success modal for queue analysis with context-menu items to open/copy the generated JSON files.
        private void ShowQueueAnalysisCompletedModal(string message, string queueJsonPath, string? excludedJsonPath)
        {
            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateSuccess(
                UILangProviderM.SrcAnalysisWindowTitle,
                message,
                closeCmd,
                closeCmd);
            vm.ContextMenuItems.Add(new(
                UILangProviderM.Current["SourceQueue.OpenQueueJson"],
                new ActionCmd(_ => OpenJsonPath(queueJsonPath))));
            vm.ContextMenuItems.Add(new(
                UILangProviderM.Current["SourceQueue.CopyQueueJsonPath"],
                new ActionCmd(_ => Clipboard.SetText(queueJsonPath))));
            if (!string.IsNullOrWhiteSpace(excludedJsonPath))
            {
                vm.ContextMenuItems.Add(new(
                    UILangProviderM.Current["SourceQueue.OpenExcludedJson"],
                    new ActionCmd(_ => OpenJsonPath(excludedJsonPath))));
                vm.ContextMenuItems.Add(new(
                    UILangProviderM.Current["SourceQueue.CopyExcludedJsonPath"],
                    new ActionCmd(_ => Clipboard.SetText(excludedJsonPath))));
            }

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        // Opens a JSON file in the default system editor via shell execute.
        private static void OpenJsonPath(string jsonPath) =>
            Process.Start(new ProcessStartInfo(jsonPath) { UseShellExecute = true });

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

                if (TryGetFirstVideoStream(rawElement, out JsonElement stream))
                {
                    if (JsonElementHelper.TryGetInt(stream, "width", out int parsedWidth)) width = parsedWidth;
                    avgFrameRate = NormalizeFrameRate(JsonElementHelper.TryGetString(stream, "avg_frame_rate"));
                    rFrameRate = NormalizeFrameRate(JsonElementHelper.TryGetString(stream, "r_frame_rate"));
                }

                return new(checkSignature, width, avgFrameRate, rFrameRate);
            }

            public static string NormalizeFrameRate(string? value)
            {
                if (string.IsNullOrWhiteSpace(value) || value.Equals("0/0", StringComparison.OrdinalIgnoreCase))
                    return string.Empty;

                string[] parts = value.Split('/');
                if (parts.Length == 2
                    && long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long numerator)
                    && long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long denominator)
                    && denominator > 0)
                {
                    long gcd = GreatestCommonDivisor(Math.Abs(numerator), Math.Abs(denominator));
                    return string.Create(
                        CultureInfo.InvariantCulture,
                        $"{numerator / gcd}/{denominator / gcd}");
                }

                return value.Trim();
            }

            private static long GreatestCommonDivisor(long a, long b)
            {
                while (b != 0)
                {
                    long t = a % b;
                    a = b;
                    b = t;
                }

                return a == 0 ? 1 : a;
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
