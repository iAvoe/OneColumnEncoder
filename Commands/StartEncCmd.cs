using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace OneColumnEncoder.Commands
{
    public class StartEncCmd(Func<EncodingPipelineRequest?> buildRequest, ModalNavS modalNavS, AppConfM appConfM) : BaseCmd
    {
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly AppConfM _appConfM = appConfM;
        private readonly StartEncCmdLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);

        public override void Execute(object? parameter)
        {
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

        private void OpenOverwriteConfirmationOrStart(EncodingPipelineRequest request, EncodingPipelineCommand command)
        {
            OverwriteTarget[] overwriteTargets = GetExistingOverwriteTargets(request, command);

            if (overwriteTargets.Length == 0)
            {
                StartEncoding(request, command);
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
                    StartEncoding(request, command);
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
            var candidates = command.MuxCommand == null
                ? new[]
                {
                    new OverwriteTarget(
                        _lang.EncodedOutputLabel,
                        EncodingPipelineH.ResolveOutputPathWithExtension(request.EncoderExeName, request.OutputPath),
                        0L)
                }
                : new[]
                {
                    new OverwriteTarget(_lang.EncodedOutputLabel, command.MuxCommand.EncodedVideoPath, 0L),
                    new OverwriteTarget(_lang.MuxOutputLabel, command.MuxCommand.OutputPath, 0L)
                };

            return candidates
                .Where(t => File.Exists(t.Path))
                .Select(t => t with { LengthBytes = GetFileLengthBytes(t.Path) })
                .ToArray();
        }

        private void StartEncoding(EncodingPipelineRequest request, EncodingPipelineCommand command)
        {
            new OpenEncodingMonitorCmd(_modalNavS, request, command).Execute(null);
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
                .Select(t => string.Format(_lang.OverwriteTargetLabel, t.Label, t.Path, FormatFileSize(t.LengthBytes)))
                .ToArray();

            return string.Join(Environment.NewLine,
                _lang.OverwriteMsg,
                string.Join(Environment.NewLine, targetLines),
                string.Format(_lang.LargestExistingSizeLabel, FormatFileSize(maxFileLengthBytes)),
                string.Format(_lang.ConfirmDelayLabel, seconds.ToString("0.0", CultureInfo.InvariantCulture)));
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
    }
}
