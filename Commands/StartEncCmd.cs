using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace OneColumnEncoder.Commands
{
    public class StartEncCmd(
        Func<EncodingPipelineRequest?> buildRequest,
        ModalNavS modalNavS,
        AppConfM appConfM,
        Func<bool>? isQueueRoute = null,
        Func<string>? getQueueJsonPath = null,
        Func<string[], EncodingPipelineRequest[]?>? buildQueueRequests = null,
        Func<bool>? isQueueRouteSupported = null) : BaseCmd
    {
        private const int MaxListedOverwriteTargets = 50;
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly AppConfM _appConfM = appConfM;
        private readonly Func<bool>? _isQueueRoute = isQueueRoute;
        private readonly Func<string>? _getQueueJsonPath = getQueueJsonPath;
        private readonly Func<string[], EncodingPipelineRequest[]?>? _buildQueueRequests = buildQueueRequests;
        private readonly Func<bool>? _isQueueRouteSupported = isQueueRouteSupported;
        private readonly StartEncCmdLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);

        public override void Execute(object? parameter)
        {
            if (IsQueueRoute())
            {
                ExecuteQueueRoute();
                return;
            }

            EncodingPipelineRequest? request = _buildRequest();
            if (request == null)
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    _lang.WarnTitle,
                    _lang.MissingUpstreamMsg).Execute(null);
                return;
            }

            EncodingPipelineCommand command = EncodingPipelineH.BuildY4mCommand(request);

            ConfirmationModal? existing = Application.Current.Windows
                .OfType<ConfirmationModal>()
                .FirstOrDefault(w => w.DataContext is ConfirmationVM &&
                                w.Owner == Application.Current.MainWindow);
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateDebug(
                _lang.ConfirmTitle, command.DisplayCommandLine,
                closeCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    OpenOverwriteConfirmationOrStart(request, command);
                }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        private bool IsQueueRoute() => _isQueueRoute?.Invoke() == true;

        private void ExecuteQueueRoute()
        {
            if (_isQueueRouteSupported?.Invoke() == false)
            {
                new OpenErrModalCmd(_modalNavS, _lang.WarnTitle, _lang.QueueUnsupportedRouteMsg).Execute(null);
                return;
            }

            string[]? sourcePaths = LoadQueueSourcePaths();
            if (sourcePaths == null) return;

            QueueEncodingItem[]? queueItems;
            try
            {
                queueItems = BuildQueueEncodingItems(sourcePaths);
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(_modalNavS, _lang.WarnTitle, ex.Message).Execute(null);
                return;
            }

            if (queueItems == null || queueItems.Length == 0)
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    _lang.WarnTitle,
                    _lang.MissingUpstreamMsg).Execute(null);
                return;
            }

            string? queueError = GetQueueValidationError(queueItems);
            if (!string.IsNullOrWhiteSpace(queueError))
            {
                new OpenErrModalCmd(_modalNavS, _lang.WarnTitle, queueError).Execute(null);
                return;
            }

            OpenQueueOverwriteConfirmationOrStart(queueItems);
        }

        private string[]? LoadQueueSourcePaths()
        {
            string queueJsonPath = _getQueueJsonPath?.Invoke() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(queueJsonPath) || !File.Exists(queueJsonPath))
            {
                new OpenErrModalCmd(_modalNavS, _lang.WarnTitle, _lang.QueueJsonMissingMsg).Execute(null);
                return null;
            }

            try
            {
                string json = File.ReadAllText(queueJsonPath);
                QueueSourceData? queueSourceData = JsonSerializer.Deserialize<QueueSourceData>(json);
                string[] sourcePaths = queueSourceData?.Entries?
                    .Select(entry => entry.FilePath)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => path!)
                    .ToArray() ?? [];

                if (sourcePaths.Length == 0)
                {
                    new OpenErrModalCmd(_modalNavS, _lang.WarnTitle, _lang.QueueJsonNoEntriesMsg).Execute(null);
                    return null;
                }

                return sourcePaths;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    _lang.WarnTitle,
                    string.Format(_lang.QueueJsonInvalidMsg, ex.Message)).Execute(null);
                return null;
            }
        }

        private QueueEncodingItem[]? BuildQueueEncodingItems(string[] sourcePaths)
        {
            EncodingPipelineRequest[]? requests = _buildQueueRequests?.Invoke(sourcePaths);
            if (requests == null || requests.Length == 0) return null;
            return [.. requests.Select(request => new QueueEncodingItem(request, EncodingPipelineH.BuildY4mCommand(request)))];
        }

        private void OpenOverwriteConfirmationOrStart(EncodingPipelineRequest request, EncodingPipelineCommand command)
        {
            OverwriteTarget[] overwriteTargets = GetExistingOverwriteTargets(request, command);
            OpenOverwriteConfirmationOrStart(overwriteTargets, () => StartEncoding(request, command));
        }

        private void OpenQueueOverwriteConfirmationOrStart(QueueEncodingItem[] queueItems)
        {
            OverwriteTarget[] overwriteTargets = GetExistingOverwriteTargets(queueItems);
            OpenOverwriteConfirmationOrStart(overwriteTargets, () => StartQueueEncoding(queueItems));
        }

        private void OpenOverwriteConfirmationOrStart(OverwriteTarget[] overwriteTargets, Action startAction)
        {
            if (overwriteTargets.Length == 0)
            {
                startAction();
                return;
            }

            long maxOutputLengthBytes = overwriteTargets.Max(t => t.LengthBytes);
            int confirmDelayMs = CalculateOverwriteConfirmDelayMs(maxOutputLengthBytes);

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateWarning(
                _lang.OverwriteTitle,
                BuildOverwriteWarningMessage(overwriteTargets, maxOutputLengthBytes, confirmDelayMs),
                closeCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    startAction();
                }));

            DispatcherTimer? timer = null;
            if (confirmDelayMs > 0)
            {
                vm.FinishWarnErrButtons.B2_2IsEnabled = false;
                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(confirmDelayMs) };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    vm.FinishWarnErrButtons.B2_2IsEnabled = true;
                };
            }

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) =>
            {
                timer?.Stop();
                _modalNavS.Close();
            };
            _modalNavS.CurrentModalVM = vm;
            timer?.Start();
            window.ShowDialog();
        }

        private OverwriteTarget[] GetExistingOverwriteTargets(EncodingPipelineRequest request, EncodingPipelineCommand command)
        {
            return GetOverwriteCandidates(request, command)
                .Where(t => File.Exists(t.Path))
                .Select(t => t with { LengthBytes = GetFileLengthBytes(t.Path) })
                .ToArray();
        }

        private OverwriteTarget[] GetExistingOverwriteTargets(QueueEncodingItem[] queueItems)
        {
            return [.. queueItems
                .SelectMany(item => GetOverwriteCandidates(item.Request, item.Command, GetQueueSourceLabel(item.Request)))
                .Where(t => File.Exists(t.Path))
                .Select(t => t with { LengthBytes = GetFileLengthBytes(t.Path) })];
        }

        private OverwriteTarget[] GetOverwriteCandidates(EncodingPipelineRequest request, EncodingPipelineCommand command, string? sourceLabel = null)
        {
            string Label(string label) => string.IsNullOrWhiteSpace(sourceLabel)
                ? label
                : $"{sourceLabel} - {label}";

            return command.MuxCommand == null
                ? new[]
                {
                    new OverwriteTarget(
                        Label(_lang.EncodedOutputLabel),
                        EncodingPipelineH.ResolveOutputPathWithExtension(request.EncoderExeName, request.OutputPath),
                        0L)
                }
                : new[]
                {
                    new OverwriteTarget(Label(_lang.EncodedOutputLabel), command.MuxCommand.EncodedVideoPath, 0L),
                    new OverwriteTarget(Label(_lang.MuxOutputLabel), command.MuxCommand.OutputPath, 0L)
                };
        }

        private string? GetQueueValidationError(QueueEncodingItem[] queueItems)
        {
            string[] missingSourcePaths = [.. queueItems
                .Select(item => item.Request.UpstreamInputPath)
                .Where(path => string.IsNullOrWhiteSpace(path) || !File.Exists(path))];
            if (missingSourcePaths.Length > 0)
                return BuildPathListMessage(_lang.QueueSourceMissingMsg, missingSourcePaths);

            string[] duplicateOutputPaths = [.. queueItems
                .SelectMany(item => GetOverwriteCandidates(item.Request, item.Command))
                .GroupBy(target => target.Path, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)];
            return duplicateOutputPaths.Length > 0
                ? BuildPathListMessage(_lang.QueueDuplicateOutputMsg, duplicateOutputPaths)
                : null;
        }

        private string BuildPathListMessage(string header, string[] paths)
        {
            IEnumerable<string> listedPaths = paths
                .Take(MaxListedOverwriteTargets)
                .Select(path => $"- {path}");
            string omittedLine = paths.Length > MaxListedOverwriteTargets
                ? Environment.NewLine + string.Format(_lang.AdditionalOverwriteTargetsLabel, paths.Length - MaxListedOverwriteTargets)
                : string.Empty;

            return string.Join(Environment.NewLine, new[] { header }.Concat(listedPaths)) + omittedLine;
        }

        private void StartEncoding(EncodingPipelineRequest request, EncodingPipelineCommand command)
        {
            new OpenEncodingMonitorCmd(_modalNavS, request, command).Execute(null);
        }

        private void StartQueueEncoding(QueueEncodingItem[] queueItems)
        {
            new OpenErrModalCmd(_modalNavS, _lang.WarnTitle, _lang.QueueEncodingPendingMsg).Execute(null);
        }

        private int CalculateOverwriteConfirmDelayMs(long fileLengthBytes)
        {
            int divisor = Math.Max(1, _appConfM.Overwrite.LongPressMegabyteDivisor);
            int minMs = Math.Max(0, _appConfM.Overwrite.MinLongPressMs);
            int maxMs = Math.Max(minMs, _appConfM.Overwrite.MaxLongPressMs);
            double megabytes = fileLengthBytes / (1024d * 1024d);
            double delayMs = megabytes / divisor * 1000d;

            return (int)Math.Round(Math.Clamp(delayMs, minMs, maxMs));
        }

        private static long GetFileLengthBytes(string path)
        {
            try { return Math.Max(0L, new FileInfo(path).Length); }
            catch { return 0L; }
        }

        private string BuildOverwriteWarningMessage(OverwriteTarget[] targets, long maxFileLengthBytes, int confirmDelayMs)
        {
            double seconds = confirmDelayMs / 1000d;
            string[] targetLines = targets
                .Take(MaxListedOverwriteTargets)
                .Select(t => string.Format(_lang.OverwriteTargetLabel, t.Label, t.Path, FormatFileSize(t.LengthBytes)))
                .ToArray();
            string omittedLine = targets.Length > MaxListedOverwriteTargets
                ? string.Format(_lang.AdditionalOverwriteTargetsLabel, targets.Length - MaxListedOverwriteTargets)
                : string.Empty;

            string[] messageParts =
            [
                _lang.OverwriteMsg,
                string.Join(Environment.NewLine, targetLines),
                omittedLine,
                string.Format(_lang.LargestExistingSizeLabel, FormatFileSize(maxFileLengthBytes)),
                string.Format(_lang.ConfirmDelayLabel, seconds.ToString("0.0", CultureInfo.InvariantCulture))
            ];

            return string.Join(Environment.NewLine, messageParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string GetQueueSourceLabel(EncodingPipelineRequest request)
        {
            string sourcePath = !string.IsNullOrWhiteSpace(request.SourceVideoPath)
                ? request.SourceVideoPath
                : request.UpstreamInputPath;
            return Path.GetFileNameWithoutExtension(sourcePath) ?? sourcePath;
        }

        private string FormatFileSize(long bytes)
        {
            const long bytesPerMb = 1024L * 1024L;
            const long bytesPerGb = 1024L * 1024L * 1024L;

            if (bytes >= bytesPerGb)
                return $"{bytes / (double)bytesPerGb:0.0}{_lang.GbSuffix}";
            return $"{bytes / (double)bytesPerMb:0.0}{_lang.MbSuffix}";
        }

        private readonly record struct OverwriteTarget(string Label, string Path, long LengthBytes);

        private readonly record struct QueueEncodingItem(EncodingPipelineRequest Request, EncodingPipelineCommand Command);

        private sealed class QueueSourceData
        {
            public List<QueueSourceEntry> Entries { get; set; } = [];
        }

        private sealed class QueueSourceEntry
        {
            public string FilePath { get; set; } = string.Empty;
        }
    }
}
