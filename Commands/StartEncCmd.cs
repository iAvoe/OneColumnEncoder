using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class StartEncCmd(Func<EncodingPipelineRequest?> buildRequest, ModalNavS modalNavS, AppConfM appConfM) : BaseCmd
    {
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly AppConfM _appConfM = appConfM;

        public override void Execute(object? parameter)
        {
            EncodingPipelineRequest? request = _buildRequest();
            if (request == null) return;

            EncodingPipelineCommand command = EncodingPipelineH.BuildY4mCommand(request);

            ConfirmationModal? existing = Application.Current.Windows
                .OfType<ConfirmationModal>()
                .FirstOrDefault(w => w.DataContext is ConfirmationModalVM &&
                                w.Owner == Application.Current.MainWindow);
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationModalVM vm = ConfirmationModalVM.CreateDebug(
                "Encoding Command", command.CommandLine,
                closeCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    _ = RunEncodingAsync(request, command);
                }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        private async Task RunEncodingAsync(EncodingPipelineRequest request, EncodingPipelineCommand command)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int? exitCode = null;
            string processOutput = string.Empty;
            bool success = false;

            try
            {
                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c " + command.CommandLine,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    },
                    EnableRaisingEvents = true
                };

                StringBuilder output = new();
                process.OutputDataReceived += (_, e) => AppendLine(output, e.Data);
                process.ErrorDataReceived += (_, e) => AppendLine(output, e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                exitCode = process.ExitCode;
                processOutput = output.ToString();
                success = exitCode == 0;
            }
            catch (Exception ex)
            {
                processOutput = ex.ToString();
            }
            finally
            {
                stopwatch.Stop();
            }

            try
            {
                await SmtpNotificationH.SendEncodingResultAsync(
                    _appConfM.Smtp,
                    request,
                    success,
                    stopwatch.Elapsed,
                    exitCode,
                    processOutput);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show(ex.Message, "SMTP Notification Failed", MessageBoxButton.OK, MessageBoxImage.Warning));
            }

            string title = success ? "Encoding Completed" : "Encoding Failed";
            string message = success
                ? $"Encoding completed in {stopwatch.Elapsed:hh\\:mm\\:ss}."
                : $"Encoding failed after {stopwatch.Elapsed:hh\\:mm\\:ss}.\nExit code: {(exitCode?.ToString() ?? "N/A")}";

            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(message, title, MessageBoxButton.OK, success ? MessageBoxImage.Information : MessageBoxImage.Error));
        }

        private static void AppendLine(StringBuilder builder, string? line)
        {
            if (line == null) return;

            lock (builder)
            {
                builder.AppendLine(line);
            }
        }
    }
}
