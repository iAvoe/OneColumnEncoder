using OneColumnEncoder.Commands.OpenClose;
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
        Action? onCompleted = null) : AsyncBaseCmd
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

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_getFfprobePath()) &&
            (IsQueueRoute()
                ? (_getQueueFilePaths?.Invoke().Length ?? 0) > 0
                : !string.IsNullOrWhiteSpace(_getSourcePath()));

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
                        QueueSourceSignature.From(signature, rawElement),
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

            QueueSourceCandidate referenceCandidate = shouldFilterQueue
                ? SelectReferenceCandidate(candidates, filterMode)
                : candidates[0];
            List<QueueSourceEntry> accepted = [];
            List<QueueSourceEntry> excluded = [];
            foreach (QueueSourceCandidate candidate in candidates)
            {
                if (!shouldFilterQueue || candidate.Signature.Matches(referenceCandidate.Signature))
                    accepted.Add(candidate.Entry);
                else
                    excluded.Add(candidate.Entry);
            }

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

            JsonSerializerOptions jsonOptions = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            UTF8Encoding utf8NoBom = new(false);
            File.WriteAllText(queueJsonPath, JsonSerializer.Serialize(new QueueSourceData(referencePath, accepted), jsonOptions), utf8NoBom);
            if (!string.IsNullOrWhiteSpace(excludedJsonPath))
                File.WriteAllText(excludedJsonPath, JsonSerializer.Serialize(new QueueSourceData(referencePath, excluded), jsonOptions), utf8NoBom);

            _analysis.FfprobePath = ffprobePath;
            _analysis.SourcePath = referencePath;
            _analysis.RawJson = referenceRawJson;
            _analysis.QueueRawJson = JsonSerializer.Serialize(new QueueRawAnalysisData(rawAnalyses), jsonOptions);
            queueCard.ApplyQueueResult(accepted.Count, excluded.Count, queueJsonPath, excludedJsonPath ?? string.Empty);
            _onQueueAccepted?.Invoke([.. accepted.Select(entry => entry.FilePath)], queueJsonPath);

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

        private static string FormatFrameCountSupplementMessage(string rawJson, int supplementedCount)
        {
            List<string> lines = [UILangProviderM.Current["SrcAnalysis.Completed"]];
            if (TryGetTotalFrameCount(rawJson, out long totalFrameCount))
                lines.Add($"{new ClipRangeSelectorLangProviderM(UILangProviderM.Current.LanguageCode).SummaryTotalFramesLabel}: {totalFrameCount}");

            lines.Add(string.Format(UILangProviderM.Current["SrcAnalysis.FrameCountSupplemented"], supplementedCount));
            return string.Join(Environment.NewLine + Environment.NewLine, lines);
        }

        private static QueueSourceCandidate SelectReferenceCandidate(
            IReadOnlyList<QueueSourceCandidate> candidates,
            QueueFilterMode filterMode) =>
            filterMode == QueueFilterMode.FirstStream
                ? SelectFirstStreamReferenceCandidate(candidates)
                : SelectWeightedVoteThenFirstStreamReferenceCandidate(candidates);

        private static QueueSourceCandidate SelectWeightedVoteThenFirstStreamReferenceCandidate(IReadOnlyList<QueueSourceCandidate> candidates)
        {
            IEnumerable<QueueSourceCandidate> votedGroup = candidates
                .GroupBy(candidate => candidate.Signature.MatchKey, StringComparer.Ordinal)
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

        private static QueueSourceCandidate SelectFirstStreamReferenceCandidate(IEnumerable<QueueSourceCandidate> candidates) =>
            candidates.OrderBy(candidate => candidate.Sequence).First();

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

        private static double? TryGetDuration(JsonElement root, JsonElement stream)
        {
            double? streamDuration = JsonElementHelper.TryGetDouble(stream, "duration");
            if (streamDuration is > 0) return streamDuration;

            return root.TryGetProperty("format", out JsonElement format)
                ? JsonElementHelper.TryGetDouble(format, "duration")
                : null;
        }

        private static double? TryGetFramesPerSecond(JsonElement stream)
        {
            if (TryParseFrameRate(JsonElementHelper.TryGetString(stream, "avg_frame_rate"), out double avg) && avg > 0d)
                return avg;
            return TryParseFrameRate(JsonElementHelper.TryGetString(stream, "r_frame_rate"), out double r) && r > 0d
                ? r
                : null;
        }

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

        private static string FormatAnalysisFailureMessage(
            string sourcePath,
            string detail,
            int? queueIndex = null,
            int? queueTotal = null,
            bool willSkipAndContinue = false)
        {
            List<string> lines = [];
            if (queueIndex.HasValue && queueTotal.HasValue)
                lines.Add($"Queue item {queueIndex.Value}/{queueTotal.Value}");

            lines.Add($"Source: {sourcePath}");
            lines.Add(detail);
            if (willSkipAndContinue)
            {
                lines.Add(string.Empty);
                lines.Add("This queue item will be skipped. Close this dialog to continue analyzing the remaining queue items.");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string FormatAllQueueItemsFailedMessage(int queueCount) =>
            $"Source queue analysis failed: all {queueCount} queue item(s) were skipped because they could not be analyzed.";

        private static string FormatQueueSkippedMessage(string message, IReadOnlyList<QueueSourceFailure> skipped)
        {
            const int maxDetails = 5;
            List<string> skippedLines = [$"Skipped failed queue item(s): {skipped.Count}"];
            skippedLines.AddRange(skipped
                .Take(maxDetails)
                .Select(item => string.IsNullOrWhiteSpace(item.DisplayName) ? item.FilePath : item.DisplayName)
                .Select(detail => $"- {detail}"));
            int omitted = skipped.Count - Math.Min(skipped.Count, maxDetails);

            if (omitted > 0) skippedLines.Add($"...and {omitted} more.");
            return message
                + Environment.NewLine
                + Environment.NewLine
                + string.Join(Environment.NewLine, skippedLines);
        }

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

        private static void OpenJsonPath(string jsonPath) =>
            Process.Start(new ProcessStartInfo(jsonPath) { UseShellExecute = true });

        private sealed class SaveLoadPlaceholder : SaveLoadBase<SaveLoadPlaceholder>
        {
            protected override string FilePath => string.Empty;
        }

        private enum QueueFilterMode
        {
            WeightedVoteThenFirstStream,
            FirstStream
        }

        private sealed record QueueSourceCandidate(
            int Sequence,
            QueueSourceEntry Entry,
            QueueSourceSignature Signature,
            string RawJson,
            double VoteWeight);

        private sealed record QueueSourceSignature(
            SourceCheckSignature CheckSignature,
            int? Width,
            int? Height,
            string AvgFrameRate,
            string RFrameRate)
        {
            public string MatchKey => string.Join(
                "|",
                CheckSignature.MatchKey,
                Width?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                Height?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                AvgFrameRate,
                RFrameRate);

            public bool Matches(QueueSourceSignature other) =>
                string.Equals(MatchKey, other.MatchKey, StringComparison.Ordinal);

            public static QueueSourceSignature From(SourceCheckSignature checkSignature, JsonElement rawElement)
            {
                int? width = null;
                int? height = null;
                string avgFrameRate = string.Empty;
                string rFrameRate = string.Empty;

                if (TryGetFirstVideoStream(rawElement, out JsonElement stream))
                {
                    if (JsonElementHelper.TryGetInt(stream, "width", out int parsedWidth)) width = parsedWidth;
                    if (JsonElementHelper.TryGetInt(stream, "height", out int parsedHeight)) height = parsedHeight;
                    avgFrameRate = NormalizeFrameRate(JsonElementHelper.TryGetString(stream, "avg_frame_rate"));
                    rFrameRate = NormalizeFrameRate(JsonElementHelper.TryGetString(stream, "r_frame_rate"));
                }

                return new(checkSignature, width, height, avgFrameRate, rFrameRate);
            }

            private static string NormalizeFrameRate(string? value)
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

        private sealed record QueueSourceEntry(
            string FilePath,
            string DisplayName,
            QueueSourceCheckResult CheckResult,
            JsonElement FfprobeJson);

        private sealed record QueueSourceRawAnalysis(
            string FilePath,
            string DisplayName,
            JsonElement FfprobeJson);

        private sealed record QueueSourceFailure(string FilePath, string DisplayName, string ErrorMessage);

        private sealed record QueueSourceCheckResult(
            IReadOnlyList<QueueSourceCheckItem> Severe,
            IReadOnlyList<QueueSourceCheckItem> Moderate)
        {
            public static QueueSourceCheckResult FromCard(SourceCheckCardVM card) =>
                new(
                    [.. card.Checklist1.Select(QueueSourceCheckItem.FromEntry)],
                    [.. card.Checklist2.Select(QueueSourceCheckItem.FromEntry)]);
        }

        private sealed record QueueSourceCheckItem(string Text, StatusType Status)
        {
            public static QueueSourceCheckItem FromEntry(ChecklistEntryVM entry) =>
                new(entry.Text, entry.Status);
        }

        private sealed record QueueSourceData(string ReferenceFilePath, IReadOnlyList<QueueSourceEntry> Entries);

        private sealed record QueueRawAnalysisData(IReadOnlyList<QueueSourceRawAnalysis> Entries);
    }
}
