using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

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
                string rawJson =
                    await FfprobeVideoAnalysisH.AnalyzeAsync(ffprobePath, sourcePath);

                _analysis.FfprobePath = ffprobePath;
                _analysis.SourcePath = sourcePath;
                _analysis.RawJson = rawJson;
                _getActiveSrcValidationCard().ApplyFfprobeAnalysisJson(rawJson);

                new OpenInfoModalCmd(
                    _modalNavS,
                    UILangProviderM.SrcAnalysisWindowTitle,
                    UILangProviderM.Current["SrcAnalysis.Completed"]).Execute(null);
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

        private async Task ExecuteQueueAnalysisAsync()
        {
            string ffprobePath = _getFfprobePath();
            string[] queueFilePaths = _getQueueFilePaths?.Invoke() ?? [];
            if (queueFilePaths.Length == 0) return;

            QueueSrcFilterCardVM queueCard = _getActiveSrcValidationCard() as QueueSrcFilterCardVM
                ?? throw new InvalidOperationException("Queue source filter card is not active.");

            List<QueueSourceEntry> accepted = [];
            List<QueueSourceEntry> excluded = [];
            SourceCheckSignature? referenceSignature = null;
            string referenceRawJson = string.Empty;
            string referencePath = string.Empty;

            foreach (string filePath in queueFilePaths)
            {
                SourceCheckCardVM probeCard = new()
                {
                    IsSvtav1SelectedFunc = queueCard.IsSvtav1SelectedFunc
                };
                probeCard.SetSourcePickedStatus(true);

                string rawJson = await FfprobeVideoAnalysisH.AnalyzeAsync(ffprobePath, filePath);
                probeCard.ApplyFfprobeAnalysisJson(rawJson);
                SourceCheckSignature signature = probeCard.GetSignature();
                using JsonDocument rawDocument = JsonDocument.Parse(rawJson);
                QueueSourceEntry entry = new(
                    filePath,
                    Path.GetFileName(filePath),
                    QueueSourceCheckResult.FromCard(probeCard),
                    rawDocument.RootElement.Clone());

                if (referenceSignature == null)
                {
                    referenceSignature = signature;
                    referenceRawJson = rawJson;
                    referencePath = filePath;
                    accepted.Add(entry);
                    queueCard.SetSourcePickedStatus(true);
                    queueCard.ApplyFfprobeAnalysisJson(rawJson);
                    continue;
                }

                if (signature.Matches(referenceSignature))
                    accepted.Add(entry);
                else
                    excluded.Add(entry);
            }

            string directory = SaveLoadBaseH<SaveLoadPlaceholder>.GetConfigDirectory();
            Directory.CreateDirectory(directory);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string queueJsonPath = Path.Combine(directory, $"source_queue_{timestamp}.json");
            string excludedJsonPath = Path.Combine(directory, $"source_queue_excluded_{timestamp}.json");

            JsonSerializerOptions jsonOptions = new()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            UTF8Encoding utf8NoBom = new(false);
            File.WriteAllText(queueJsonPath, JsonSerializer.Serialize(new QueueSourceData(referencePath, accepted), jsonOptions), utf8NoBom);
            File.WriteAllText(excludedJsonPath, JsonSerializer.Serialize(new QueueSourceData(referencePath, excluded), jsonOptions), utf8NoBom);

            _analysis.FfprobePath = ffprobePath;
            _analysis.SourcePath = referencePath;
            _analysis.RawJson = referenceRawJson;
            queueCard.ApplyQueueResult(accepted.Count, excluded.Count, queueJsonPath, excludedJsonPath);
            _onQueueAccepted?.Invoke([.. accepted.Select(entry => entry.FilePath)], queueJsonPath);

            string message = string.Format(
                UILangProviderM.Current["SourceQueue.AnalysisCompleted"],
                excluded.Count,
                queueJsonPath,
                excludedJsonPath);
            new OpenInfoModalCmd(_modalNavS, UILangProviderM.SrcAnalysisWindowTitle, message).Execute(null);
        }

        private sealed class SaveLoadPlaceholder : SaveLoadBaseH<SaveLoadPlaceholder>
        {
            protected override string FilePath => string.Empty;
        }

        private sealed record QueueSourceEntry(
            string FilePath,
            string DisplayName,
            QueueSourceCheckResult CheckResult,
            JsonElement FfprobeJson);

        private sealed record QueueSourceCheckResult(
            IReadOnlyList<QueueSourceCheckItem> Severe,
            IReadOnlyList<QueueSourceCheckItem> Moderate)
        {
            public static QueueSourceCheckResult FromCard(SourceCheckCardVM card) =>
                new(
                    [.. card.Checklist1.Select(QueueSourceCheckItem.FromEntry)],
                    [.. card.Checklist2.Select(QueueSourceCheckItem.FromEntry)]);
        }

        private sealed record QueueSourceCheckItem(string Text, StatusType Status, bool IsEnabled)
        {
            public static QueueSourceCheckItem FromEntry(ChecklistEntryVM entry) =>
                new(entry.Text, entry.Status, entry.IsEnabled);
        }

        private sealed record QueueSourceData(string ReferenceFilePath, IReadOnlyList<QueueSourceEntry> Entries);
    }
}
