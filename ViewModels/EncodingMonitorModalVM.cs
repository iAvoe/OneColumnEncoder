using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// XAML:
    /// Progress Bar → RAM Heatmap → RAM Distribution → Heatmap block indicators
    /// → RAM Sampling Interval → RAM Sampling Freeze/Reset → Standard Out Log (Two Columns)
    /// → Standard Error (Two Columns) → Bottom Status → FiveButtonGroups
    /// </summary>
    public partial class EncodingMonitorModalVM : BaseVM
    {
        private const int HeatMapRows = 16;
        private const int HeatMapColumns = 32;
        private EncodingMonitorModalLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);
        public EncodingMonitorModalLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }
        private readonly ModalNavS _modalNavS;
        private readonly Action _closeAction;
        private readonly EncodingPipelineRequest _request;
        private readonly EncodingPipelineCommand _command;
        private readonly AppConfM _appConfM;
        private readonly bool _isSample;
        private readonly Stopwatch _stopwatch = new();
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
        private readonly StringBuilder _upstreamStdoutBuilder = new();
        private readonly StringBuilder _downstreamStdoutBuilder = new();
        private readonly StringBuilder _upstreamStderrBuilder = new();
        private readonly StringBuilder _downstreamStderrBuilder = new();
        private readonly ConcurrentQueue<ProcessLogEntry> _logQueue = new();
        private readonly Lock _logLock = new();
        private readonly Random _random = new();
        private DateTime _lastStatsUpdate = DateTime.MinValue;
        private CancellationTokenSource? _cts;
        private Process? _upstreamProcess;
        private Process? _encoderProcess;
        private bool _hasStarted;
        private bool _finishEnabledAfterClose;
        private int? _exitCode;
        private bool _success;

        public string WindowTitle => _isSample ? Lang.WindowTitleSampleMode : Lang.WindowTitle;
        public string ProgressTitle => Lang.ProgressTitle;
        public string MemoryTitle => Lang.MemoryTitle;
        public string DistributionTitle => Lang.DistributionTitle;
        public string BlockDetailsTitle => Lang.BlockDetailsTitle;
        public string LogTitle => Lang.LogTitle;
        public string DragLogReportHint => Lang.DragLogReportHint;
        public string CurrentSizeLabel => Lang.CurrentSizeLabel;
        public string EstimatedSizeLabel => Lang.EstimatedSizeLabel;
        public string WrittenFramesLabel => Lang.WrittenFramesLabel;
        public string SampleIntervalLabel => Lang.SampleIntervalLabel;
        public string StartedAtLabel => Lang.StartedAtLabel;
        public string ElapsedLabel => Lang.ElapsedLabel;
        public string RemainingLabel => Lang.RemainingLabel;
        public string CompleteAtLabel => Lang.CompleteAtLabel;
        public string EncoderFileLabel => Lang.EncoderFileLabel;
        public string RateControlLabel => Lang.RateControlLabel;
        public string ArgsLabel => Lang.ArgsLabel;
        public string SmallNoteText => Lang.SmallNoteText;
        public string DistributionUpstreamLabel => Lang.DistributionUpstreamLabel;
        public string DistributionDownstreamLabel => Lang.DistributionDownstreamLabel;
        public string DistributionOtherLabel => Lang.DistributionOtherLabel;
        public string DistributionCacheLabel => Lang.DistributionCacheLabel;
        public string BlockDetailPosLabel => Lang.BlockDetailPosLabel;
        public string BlockDetailSegmentLabel => Lang.BlockDetailSegmentLabel;
        public string BlockDetailHeatLabel => Lang.BlockDetailHeatLabel;
        public string StderrTitle => Lang.StderrTitle;

        public ObservableCollection<ColumnTextItemM> MetricColumns { get; } = [];
        public ObservableCollection<ColumnTextItemM> FooterColumns { get; } = [];
        public ObservableCollection<HeatMapCellM> HeatMapA { get; } = [];
        public ObservableCollection<string> SampleIntervalTickLabels { get; } = [];
        public ButtonGroupVM MonitorButtons { get; }
        public ButtonGroupVM LogButtons { get; }
        public ButtonGroupVM ReportButtons { get; }
        public ButtonGroupVM FinishButtons { get; }
        public ActionCmd FreezeOrContinueCmd { get; }
        public ActionCmd ResetStatsCmd { get; }
        public CloseModalCmd CloseCmd { get; }

        private int _progressValue;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (!SetProperty(ref _progressValue, Math.Clamp(value, 0, 100))) return;
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{ProgressValue}%";

        private int _sampleIntervalSeconds = 30;
        public int SampleIntervalSeconds
        {
            get => _sampleIntervalSeconds;
            set => SetProperty(ref _sampleIntervalSeconds, value);
        }

        private bool _isFrozen;
        public bool IsFrozen
        {
            get => _isFrozen;
            set
            {
                if (!SetProperty(ref _isFrozen, value)) return;
                FreezeOrContinueText = _isFrozen ? Lang.ContinueMonitoringText : Lang.FreezeContinueText;
                MonitorButtons.B2_1Text = FreezeOrContinueText;
            }
        }

        private string _freezeOrContinueText = string.Empty;
        public string FreezeOrContinueText
        {
            get => _freezeOrContinueText;
            private set => SetProperty(ref _freezeOrContinueText, value);
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _upstreamLogText = string.Empty;
        public string UpstreamLogText
        {
            get => _upstreamLogText;
            set => SetProperty(ref _upstreamLogText, value);
        }

        private string _downstreamLogText = string.Empty;
        public string DownstreamLogText
        {
            get => _downstreamLogText;
            set => SetProperty(ref _downstreamLogText, value);
        }

        private string _upstreamReportText = string.Empty;
        public string UpstreamReportText
        {
            get => _upstreamReportText;
            set => SetProperty(ref _upstreamReportText, value);
        }

        private string _downstreamReportText = string.Empty;
        public string DownstreamReportText
        {
            get => _downstreamReportText;
            set => SetProperty(ref _downstreamReportText, value);
        }

        private string _distributionUpstream = "XXX,XXX MB";
        public string DistributionUpstream
        {
            get => _distributionUpstream;
            set => SetProperty(ref _distributionUpstream, value);
        }

        private string _distributionDownstream = "XXX,XXX MB";
        public string DistributionDownstream
        {
            get => _distributionDownstream;
            set => SetProperty(ref _distributionDownstream, value);
        }

        private string _distributionOther = "XXX,XXX MB";
        public string DistributionOther
        {
            get => _distributionOther;
            set => SetProperty(ref _distributionOther, value);
        }

        private string _distributionCache = "XXX,XXX MB";
        public string DistributionCache
        {
            get => _distributionCache;
            set => SetProperty(ref _distributionCache, value);
        }

        private string _blockNo = "#X,X,X,XXXMB";
        public string BlockNo
        {
            get => _blockNo;
            set => SetProperty(ref _blockNo, value);
        }

        private string _blockSegment = "帧缓冲";
        public string BlockSegment
        {
            get => _blockSegment;
            set => SetProperty(ref _blockSegment, value);
        }

        private string _blockHeat = "XXX%";
        public string BlockHeat
        {
            get => _blockHeat;
            set => SetProperty(ref _blockHeat, value);
        }

        public EncodingMonitorModalVM(
            ModalNavS modalNavS,
            Action closeAction,
            EncodingPipelineRequest request,
            EncodingPipelineCommand command,
            AppConfM appConfM,
            bool isSample)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            _request = request;
            _command = command;
            _appConfM = appConfM;
            _isSample = isSample;

            RefreshLanguageState();

            FreezeOrContinueCmd = new ActionCmd(_ => IsFrozen = !IsFrozen);
            ResetStatsCmd = new ActionCmd(_ => ResetStats());
            CloseCmd = new CloseModalCmd(() =>
            {
                if (!_finishEnabledAfterClose) return;
                _closeAction();
            });

            MonitorButtons = ButtonGroupVM.CreateTwoButton(FreezeOrContinueText, Lang.ResetUsageText, FreezeOrContinueCmd, ResetStatsCmd);

            LogButtons = ButtonGroupVM.CreateFiveButton(
                Lang.SaveUpstreamStdoutText, Lang.SaveDownstreamStdoutText, Lang.CopyUpstreamStdoutText, Lang.CopyDownstreamStdoutText, Lang.RotateLogFontSizeText,
                new ActionCmd(_ => SaveText(UpstreamLogText, "upstream-stdout.txt")),
                new ActionCmd(_ => SaveText(DownstreamLogText, "downstream-stdout.txt")),
                new ActionCmd(_ => CopyText(UpstreamLogText)),
                new ActionCmd(_ => CopyText(DownstreamLogText)),
                new ActionCmd(_ => RotateLogFontSize()));

            ReportButtons = ButtonGroupVM.CreateFiveButton(
                Lang.SaveUpstreamStderrText, Lang.SaveDownstreamStderrText, Lang.CopyUpstreamStderrText, Lang.CopyDownstreamStderrText, Lang.RotateLogFontSizeText,
                new ActionCmd(_ => SaveText(UpstreamReportText, "upstream-stderr.txt")),
                new ActionCmd(_ => SaveText(DownstreamReportText, "downstream-stderr.txt")),
                new ActionCmd(_ => CopyText(UpstreamReportText)),
                new ActionCmd(_ => CopyText(DownstreamReportText)),
                new ActionCmd(_ => RotateLogFontSize()));

            FinishButtons = ButtonGroupVM.CreateFiveButton(
                Lang.OpenOutputDirectoryText, Lang.ViewEncodingCommandText, Lang.InterruptKeepResultText, Lang.ForceQuitText, Lang.CloseAfterDoneText,
                new ActionCmd(_ => OpenOutputDirectory()),
                new ActionCmd(_ => new OpenInfoOrDbgModalCmd(_modalNavS, Lang.EncodingCommandTitle, _command.CommandLine).Execute(null)),
                new ActionCmd(_ => TryInterrupt()),
                new ActionCmd(_ => TryKill()),
                CloseCmd);
            FinishButtons.B5_5IsEnabled = false;

            BuildMetrics();
            BuildFooter();
            BuildHeatMaps();
            AppendInitialLogs();
            _timer.Tick += OnTimerTick;
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private double _logFontSize = 11;
        public double LogFontSize
        {
            get => _logFontSize;
            set => SetProperty(ref _logFontSize, value);
        }

        public void Start()
        {
            if (_hasStarted) return;
            _hasStarted = true;
            _cts = new CancellationTokenSource();
            _stopwatch.Start();
            _timer.Start();
            _ = RunEncodingAsync(_cts.Token);
        }

        private void BuildMetrics()
        {
            MetricColumns.Clear();
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.PhysicalMemoryTopText, MainText = Lang.PhysicalMemoryMainText, BottomText = Lang.PhysicalMemoryBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.CommittedMemoryTopText, MainText = Lang.CommittedMemoryMainText, BottomText = Lang.CommittedMemoryBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.WorkingSetPeakTopText, MainText = Lang.WorkingSetPeakMainText, BottomText = Lang.WorkingSetPeakBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.PageFileTopText, MainText = Lang.PageFileMainText, BottomText = Lang.PageFileBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.PageFaultTopText, MainText = Lang.PageFaultMainText, BottomText = Lang.PageFaultBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.BandwidthPeakTopText, MainText = Lang.BandwidthPeakMainText, BottomText = Lang.BandwidthPeakBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.MemoryPressureTopText, MainText = Lang.MemoryPressureMediumText, BottomText = Lang.MemoryPressureBottomText });
        }

        private void BuildFooter()
        {
            FooterColumns.Clear();
            FooterColumns.Add(new ColumnTextItemM { MainText = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) });
            FooterColumns.Add(new ColumnTextItemM { MainText = "00:00:00" });
            FooterColumns.Add(new ColumnTextItemM { MainText = "--:--:--" });
            FooterColumns.Add(new ColumnTextItemM { MainText = "--:--:--" });
            FooterColumns.Add(new ColumnTextItemM { MainText = _request.EncoderExeName });
            FooterColumns.Add(new ColumnTextItemM { MainText = GetRateControlText() });
            FooterColumns.Add(new ColumnTextItemM { MainText = GetPresetText() });
        }

        private void BuildHeatMaps()
        {
            HeatMapA.Clear();
            for (int i = 0; i < HeatMapRows * HeatMapColumns; i++)
            {
                HeatMapA.Add(new HeatMapCellM { Level = 0, Tooltip = string.Format(Lang.BlockTooltipFormat, i) });
            }
        }

        private void AppendInitialLogs()
        {
            AppendLine(_upstreamStdoutBuilder, string.Format(Lang.UpstreamStdoutHeaderFormat, _request.UpstreamExeName, _request.EncoderExeName));
            AppendLine(_downstreamStdoutBuilder, string.Format(Lang.DownstreamStdoutHeaderFormat, _request.EncoderExeName));
            AppendLine(_upstreamStderrBuilder, $"{Lang.UpstreamLabel}: {_request.UpstreamExeName}");
            AppendLine(_upstreamStderrBuilder, $"{Lang.ExecutableLabel}: {_request.UpstreamPath}");
            AppendLine(_upstreamStderrBuilder, $"{Lang.InputLabel}: {_request.UpstreamInputPath}");
            AppendLine(_upstreamStderrBuilder, $"{Lang.ArgumentsLabel}: {_command.UpstreamArgs}");
            AppendLine(_downstreamStderrBuilder, $"{Lang.EncoderLabel}: {_request.EncoderExeName}");
            AppendLine(_downstreamStderrBuilder, $"{Lang.ExecutableLabel}: {_request.EncoderPath}");
            AppendLine(_downstreamStderrBuilder, $"{Lang.OutputLabel}: {_request.OutputPath}");
            AppendLine(_downstreamStderrBuilder, $"{Lang.ArgumentsLabel}: {_command.EncoderArgs}");
            FlushLogsToProperties();
        }

        private async Task RunEncodingAsync(CancellationToken cancellationToken)
        {
            string processOutput = string.Empty;
            _success = false;

            try
            {
                StatusText = Lang.EncodingText;
                using Process upstream = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _request.UpstreamPath,
                        Arguments = _command.UpstreamArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                using Process encoder = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _request.EncoderPath,
                        Arguments = _command.EncoderArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                _upstreamProcess = upstream;
                _encoderProcess = encoder;

                upstream.Start();
                encoder.Start();

                // Pipe upstream stdout → encoder stdin while capturing binary as text
                Task pipeTask = Task.Run(async () =>
                {
                    try
                    {
                        byte[] buffer = new byte[81920];
                        Stream upstreamStdout = upstream.StandardOutput.BaseStream;
                        Stream encoderStdin = encoder.StandardInput.BaseStream;
                        int bytesRead;
                        while ((bytesRead = await upstreamStdout.ReadAsync(buffer, cancellationToken)) > 0)
                        {
                            await encoderStdin.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                            await encoderStdin.FlushAsync(cancellationToken);
                        }
                        encoder.StandardInput.Close();
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        EnqueueProcessLine(ProcessLogKind.UpstreamStderr, Lang.PipeErrorPrefix + ex.Message);
                    }
                }, cancellationToken);

                // Upstream stderr reader
                Task upstreamStderrTask = ReadStreamAsync(upstream.StandardError, ProcessLogKind.UpstreamStderr, cancellationToken);
                // Encoder stdout reader
                Task encoderStdoutTask = ReadStreamAsync(encoder.StandardOutput, ProcessLogKind.DownstreamStdout, cancellationToken);
                // Encoder stderr reader
                Task encoderStderrTask = ReadStreamAsync(encoder.StandardError, ProcessLogKind.DownstreamStderr, cancellationToken);

                await Task.WhenAll(pipeTask, upstreamStderrTask, encoderStdoutTask, encoderStderrTask);

                upstream.WaitForExit();
                encoder.WaitForExit();

                _exitCode = encoder.ExitCode;
                _success = _exitCode == 0;
                processOutput = GetCombinedOutput();
            }
            catch (OperationCanceledException)
            {
                StatusText = Lang.InterruptedText;
                processOutput = GetCombinedOutput();
            }
            catch (Exception ex)
            {
                EnqueueProcessLine(ProcessLogKind.DownstreamStderr, ex.ToString());
                StatusText = Lang.FailedText;
                processOutput = GetCombinedOutput();
            }
            finally
            {
                _stopwatch.Stop();
                _timer.Stop();
                ProgressValue = _success ? 100 : ProgressValue;
                StatusText = _success ? Lang.CompletedText : StatusText == Lang.EncodingText ? Lang.FailedText : StatusText;
                FlushLogsToProperties();
                UpdateFooterTimes(final: true);
                EnableCloseButton();
            }

            if (!_isSample)
                await TrySendNotificationAsync(_success, processOutput);
        }

        private async Task ReadStreamAsync(StreamReader reader, ProcessLogKind kind, CancellationToken ct)
        {
            try
            {
                string? line;
                while ((line = await reader.ReadLineAsync(ct)) != null)
                    EnqueueProcessLine(kind, line);
            }
            catch (OperationCanceledException) { }
        }

        private void EnqueueProcessLine(ProcessLogKind kind, string? line)
        {
            if (line == null) return;
            _logQueue.Enqueue(new ProcessLogEntry(kind, line));
        }

        private static void AppendLine(StringBuilder builder, string value)
        {
            builder.AppendLine(value);
        }

        private void FlushLogsToProperties()
        {
            ProcessQueuedLogs();
            if (IsFrozen) return;
            lock (_logLock)
            {
                UpstreamLogText = _upstreamStdoutBuilder.ToString();
                DownstreamLogText = _downstreamStdoutBuilder.ToString();
                UpstreamReportText = _upstreamStderrBuilder.ToString();
                DownstreamReportText = _downstreamStderrBuilder.ToString();
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            ProcessQueuedLogs();
            if (!IsFrozen)
            {
                FlushLogsToProperties();
                if ((DateTime.Now - _lastStatsUpdate).TotalSeconds >= 1d)
                {
                    _lastStatsUpdate = DateTime.Now;
                    UpdateMetrics();
                    UpdateHeatMaps();
                    UpdateFooterTimes(final: false);
                }
            }
        }

        private void ProcessQueuedLogs()
        {
            bool changed = false;
            while (_logQueue.TryDequeue(out ProcessLogEntry entry))
            {
                changed = true;
                switch (entry.Kind)
                {
                    case ProcessLogKind.UpstreamStdout:
                        AppendLogWithOverwrite(_upstreamStdoutBuilder, entry.Line, false);
                        break;
                    case ProcessLogKind.DownstreamStdout:
                        AppendLogWithOverwrite(_downstreamStdoutBuilder, entry.Line, false);
                        break;
                    case ProcessLogKind.UpstreamStderr:
                        AppendLogWithOverwrite(_upstreamStderrBuilder, entry.Line, true);
                        break;
                    case ProcessLogKind.DownstreamStderr:
                        AppendLogWithOverwrite(_downstreamStderrBuilder, entry.Line, true);
                        break;
                }
            }

            if (changed && !IsFrozen)
            {
                lock (_logLock)
                {
                    UpstreamLogText = _upstreamStdoutBuilder.ToString();
                    DownstreamLogText = _downstreamStdoutBuilder.ToString();
                    UpstreamReportText = _upstreamStderrBuilder.ToString();
                    DownstreamReportText = _downstreamStderrBuilder.ToString();
                }
            }
        }

        private void AppendLogWithOverwrite(StringBuilder target, string text, bool isStderr)
        {
            string normalized = text.Replace("\0", string.Empty, StringComparison.Ordinal);
            string[] newlineParts = normalized.Split('\n');
            foreach (string newlinePart in newlineParts)
            {
                string[] carriageParts = newlinePart.Split('\r');
                if (carriageParts.Length > 1)
                {
                    string latest = carriageParts[^1].TrimEnd();
                    if (isStderr && TryHandleProgressLine(latest)) continue;

                    TrimLastLine(target);
                    if (!string.IsNullOrWhiteSpace(latest))
                        target.AppendLine(latest);
                    continue;
                }

                string line = newlinePart.TrimEnd();
                if (isStderr && TryHandleProgressLine(line)) continue;
                if (!string.IsNullOrWhiteSpace(line))
                    target.AppendLine(line);
            }
        }

        private bool TryHandleProgressLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || !IsProgressLine(line)) return false;
            ProgressValue = InferProgress(ProgressValue, line);
            StatusText = line.Trim();
            return true;
        }

        [GeneratedRegex(@"(?<!\d)\d{1,3}\s*%")]
        private static partial Regex ProgressLineRegex();

        [GeneratedRegex(@"(?<!\d)(\d{1,3})\s*%")]
        private static partial Regex ProgressPercentRegex();

        private static bool IsProgressLine(string line)
        {
            string lower = line.ToLowerInvariant();
            return lower.Contains("fps", StringComparison.Ordinal)
                || lower.Contains("frame=", StringComparison.Ordinal)
                || lower.Contains("frames", StringComparison.Ordinal) && lower.Contains("kb/s", StringComparison.Ordinal)
                || lower.Contains("eta", StringComparison.Ordinal) && lower.Contains('%', StringComparison.Ordinal)
                || ProgressLineRegex().IsMatch(line);
        }

        private static void TrimLastLine(StringBuilder builder)
        {
            if (builder.Length == 0) return;
            int index = builder.ToString().LastIndexOf('\n');
            if (index < 0)
            {
                builder.Clear();
                return;
            }
            builder.Length = index + 1;
        }

        private void UpdateMetrics()
        {
            double seconds = Math.Max(1d, _stopwatch.Elapsed.TotalSeconds);
            double outputGb = TryGetOutputSizeGb();
            ProgressValue = InferProgress(ProgressValue, _downstreamStderrBuilder.ToString() + _upstreamStderrBuilder.ToString());
            MetricColumns[0].MainText = $"{Math.Max(0.1, outputGb / 2d + 1d):0.0} GB";
            MetricColumns[1].MainText = $"{Math.Max(0.1, outputGb + 0.5):0.0} GB";
            MetricColumns[2].MainText = $"{Math.Max(0.1, outputGb / 3d + 0.2):0.0} GB";
            MetricColumns[3].MainText = $"{Math.Max(0.1, outputGb / 4d + 0.1):0.0} GB";
            MetricColumns[4].MainText = $"{Math.Min(99999, (int)(seconds * 3)):N0}";
            MetricColumns[5].MainText = $"{Math.Max(0.1, outputGb / seconds):0.0} GBps";
            MetricColumns[6].MainText = ProgressValue < 75 ? Lang.MemoryPressureMediumText : Lang.MemoryPressureHighText;
            MetricColumns[6].BottomText = $"{Math.Min(100, 35 + ProgressValue / 2)}%";
            DistributionDownstream = $"{outputGb * 1024:0} MB";
        }

        private void UpdateHeatMaps()
        {
            UpdateHeatMap(HeatMapA);
            int index = _random.Next(HeatMapA.Count);
            BlockNo = $"#{index},0,0,{_random.Next(64, 2048)}MB";
            BlockHeat = $"{_random.Next(5, 100)}%";
        }

        private void UpdateHeatMap(ObservableCollection<HeatMapCellM> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                int drift = _random.Next(-1, 3);
                cells[i].Level = Math.Clamp(cells[i].Level + drift, 0, 8);
            }
        }

        private void UpdateFooterTimes(bool final)
        {
            TimeSpan elapsed = _stopwatch.Elapsed;
            FooterColumns[1].MainText = elapsed.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture);
            if (ProgressValue > 0 && !final)
            {
                double totalSeconds = elapsed.TotalSeconds / ProgressValue * 100d;
                TimeSpan remaining = TimeSpan.FromSeconds(Math.Max(0d, totalSeconds - elapsed.TotalSeconds));
                FooterColumns[2].MainText = remaining.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture);
                FooterColumns[3].MainText = DateTime.Now.Add(remaining).ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (final)
            {
                FooterColumns[2].MainText = "00:00:00";
                FooterColumns[3].MainText = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
        }

        private static int InferProgress(int current, string log)
        {
            MatchCollection matches = ProgressPercentRegex().Matches(log);
            int found = current;
            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out int value))
                    found = Math.Max(found, Math.Clamp(value, 0, 100));
            }
            return found;
        }

        private double TryGetOutputSizeGb()
        {
            try
            {
                if (!File.Exists(_request.OutputPath)) return 0d;
                return new FileInfo(_request.OutputPath).Length / 1024d / 1024d / 1024d;
            }
            catch
            {
                return 0d;
            }
        }

        private void ResetStats()
        {
            foreach (ColumnTextItemM item in MetricColumns)
                item.BottomText = item.BottomText;
            ProgressValue = 0;
            StatusText = Lang.ResetUsageStatusText;
        }

        private void TryInterrupt()
        {
            try
            {
                _cts?.Cancel();
                _encoderProcess?.CloseMainWindow();
                _upstreamProcess?.CloseMainWindow();
                StatusText = Lang.InterruptingText;
            }
            catch (Exception ex)
            {
                EnqueueProcessLine(ProcessLogKind.DownstreamStderr, ex.Message);
            }
        }

        private void TryKill()
        {
            try
            {
                _cts?.Cancel();
                _encoderProcess?.Kill(entireProcessTree: true);
                _upstreamProcess?.Kill(entireProcessTree: true);
                StatusText = Lang.ForcedExitStatusText;
            }
            catch (Exception ex)
            {
                EnqueueProcessLine(ProcessLogKind.DownstreamStderr, ex.Message);
            }
        }

        private void EnableCloseButton()
        {
            _finishEnabledAfterClose = true;
            FinishButtons.B5_5IsEnabled = true;
        }

        private void OpenOutputDirectory()
        {
            string? directory = Path.GetDirectoryName(_request.OutputPath);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }

        private static void CopyText(string text)
        {
            if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
        }

        private void SaveText(string text, string fileName)
        {
            if (string.IsNullOrEmpty(text)) return;
            string directory = Path.GetDirectoryName(_request.OutputPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, fileName);
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        private void RotateLogFontSize()
        {
            LogFontSize = LogFontSize switch
            {
                < 12 => 12,
                < 14 => 14,
                _ => 10
            };
        }

        private string GetCombinedOutput()
        {
            lock (_logLock)
            {
                return string.Join(Environment.NewLine, _upstreamStdoutBuilder, _downstreamStdoutBuilder, _upstreamStderrBuilder, _downstreamStderrBuilder);
            }
        }

        private async Task TrySendNotificationAsync(bool success, string processOutput)
        {
            try
            {
                await SmtpNotificationH.SendEncodingResultAsync(
                    _appConfM.Smtp,
                    _request,
                    success,
                    _stopwatch.Elapsed,
                    _exitCode,
                    processOutput);
            }
            catch (Exception ex)
            {
                EnqueueProcessLine(ProcessLogKind.DownstreamStderr, "SMTP Notification Failed: " + ex.Message);
                FlushLogsToProperties();
            }
        }

        private string GetRateControlText()
        {
            EncoderConfM conf = _request.EncoderConf;
            bool abr = conf.RateControlMode.Equals(Lang.ABRText, StringComparison.OrdinalIgnoreCase);
            return _request.EncoderExeName.ToLowerInvariant() switch
            {
                "x264.exe" => abr ? $"{Lang.ABRText} {conf.X264Abr} Mbps" : $"{Lang.CRFText} {conf.X264Crf}",
                "x265.exe" => abr ? $"{Lang.ABRText} {conf.X265Abr} Mbps" : $"{Lang.CRFText} {conf.X265Crf}",
                "svtav1encapp.exe" => abr ? $"{Lang.ABRText} {conf.SvtAv1Abr} Mbps" : $"{Lang.CRFText} {conf.SvtAv1Crf}",
                _ => conf.RateControlMode
            };
        }

        private string GetPresetText()
        {
            EncoderConfM conf = _request.EncoderConf;
            return _request.EncoderExeName.ToLowerInvariant() switch
            {
                "x264.exe" => $"x264 {Lang.ModeText} {conf.X264Mode}",
                "x265.exe" => $"x265 {Lang.ModeText} {conf.X265Mode}",
                "svtav1encapp.exe" => $"SVT-AV1 {Lang.ModeText} {conf.SvtAv1Mode}",
                _ => Lang.NotAvailableText
            };
        }

        private void RefreshLanguageState()
        {
            Lang = new EncodingMonitorModalLangProviderM(UILangProviderM.Current.LanguageCode);
            SampleIntervalTickLabels.Clear();
            foreach (string label in Lang.SampleIntervalTickLabels)
                SampleIntervalTickLabels.Add(label);
            FreezeOrContinueText = _isFrozen ? Lang.ContinueMonitoringText : Lang.FreezeContinueText;
            StatusText = Lang.ReadyToStartText;
        }

        private void RefreshLanguageBindings()
        {
            Lang = new EncodingMonitorModalLangProviderM(UILangProviderM.Current.LanguageCode);
            FreezeOrContinueText = _isFrozen ? Lang.ContinueMonitoringText : Lang.FreezeContinueText;
            MonitorButtons.B2_1Text = FreezeOrContinueText;
            MonitorButtons.B2_2Text = Lang.ResetUsageText;
            LogButtons.B5_1Text = Lang.SaveUpstreamStdoutText;
            LogButtons.B5_2Text = Lang.SaveDownstreamStdoutText;
            LogButtons.B5_3Text = Lang.CopyUpstreamStdoutText;
            LogButtons.B5_4Text = Lang.CopyDownstreamStdoutText;
            LogButtons.B5_5Text = Lang.RotateLogFontSizeText;
            ReportButtons.B5_1Text = Lang.SaveUpstreamStderrText;
            ReportButtons.B5_2Text = Lang.SaveDownstreamStderrText;
            ReportButtons.B5_3Text = Lang.CopyUpstreamStderrText;
            ReportButtons.B5_4Text = Lang.CopyDownstreamStderrText;
            ReportButtons.B5_5Text = Lang.RotateLogFontSizeText;
            FinishButtons.B5_1Text = Lang.OpenOutputDirectoryText;
            FinishButtons.B5_2Text = Lang.ViewEncodingCommandText;
            FinishButtons.B5_3Text = Lang.InterruptKeepResultText;
            FinishButtons.B5_4Text = Lang.ForceQuitText;
            FinishButtons.B5_5Text = Lang.CloseAfterDoneText;

            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(ProgressTitle));
            OnPropertyChanged(nameof(MemoryTitle));
            OnPropertyChanged(nameof(DistributionTitle));
            OnPropertyChanged(nameof(BlockDetailsTitle));
            OnPropertyChanged(nameof(LogTitle));
            OnPropertyChanged(nameof(DragLogReportHint));
            OnPropertyChanged(nameof(CurrentSizeLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));
            OnPropertyChanged(nameof(WrittenFramesLabel));
            OnPropertyChanged(nameof(SampleIntervalLabel));
            OnPropertyChanged(nameof(StartedAtLabel));
            OnPropertyChanged(nameof(ElapsedLabel));
            OnPropertyChanged(nameof(RemainingLabel));
            OnPropertyChanged(nameof(CompleteAtLabel));
            OnPropertyChanged(nameof(EncoderFileLabel));
            OnPropertyChanged(nameof(RateControlLabel));
            OnPropertyChanged(nameof(ArgsLabel));
            OnPropertyChanged(nameof(SmallNoteText));
            OnPropertyChanged(nameof(DistributionUpstreamLabel));
            OnPropertyChanged(nameof(DistributionDownstreamLabel));
            OnPropertyChanged(nameof(DistributionOtherLabel));
            OnPropertyChanged(nameof(DistributionCacheLabel));
            OnPropertyChanged(nameof(BlockDetailPosLabel));
            OnPropertyChanged(nameof(BlockDetailSegmentLabel));
            OnPropertyChanged(nameof(BlockDetailHeatLabel));
            OnPropertyChanged(nameof(StderrTitle));
            OnPropertyChanged(nameof(SampleIntervalTickLabels));
        }

        public override void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            _cts?.Dispose();
            base.Dispose();
        }

        private void OnLanguageChanged()
        {
            RefreshLanguageBindings();
        }

        private enum ProcessLogKind
        {
            UpstreamStdout,
            DownstreamStdout,
            UpstreamStderr,
            DownstreamStderr
        }

        private readonly record struct ProcessLogEntry(ProcessLogKind Kind, string Line);
    }
}
