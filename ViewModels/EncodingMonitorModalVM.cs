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
using System.Runtime.InteropServices;
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
        private const int HeatMapMaxLevel = 8;
        private const long BytesPerMb = 1024L * 1024L;
        private const long BytesPerGb = 1024L * 1024L * 1024L;
        private const string PlaceholderGb = "XX.X GB";
        private const string PlaceholderGbPerSecond = "XX.X GBps";
        private const string PlaceholderCount = "XX,XXX";
        private const string PlaceholderPercent = "XXX%";
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
        private readonly StringBuilder _upstreamStderrBuilder = new();
        private readonly StringBuilder _downstreamStderrBuilder = new();
        private readonly LogFoldState _upstreamStderrFoldState = new();
        private readonly LogFoldState _downstreamStderrFoldState = new();
        private readonly ConcurrentQueue<ProcessLogEntry> _logQueue = new();
        private readonly Lock _logLock = new();
        private DateTime _lastStatsUpdate = DateTime.MinValue;
        private DateTime _lastMemoryStatsUpdate = DateTime.MinValue;
        private CancellationTokenSource? _cts;
        private Process? _upstreamProcess;
        private Process? _encoderProcess;
        private bool _hasStarted;
        private bool _finishEnabledAfterClose;
        private int? _exitCode;
        private bool _success;
        private MemoryStatusSnapshot _lastMemoryStatus;
        private long _lastUpstreamWorkingSetBytes;
        private long _lastEncoderWorkingSetBytes;
        private long _lastOutputSizeBytes;
        private DateTime _lastOutputSizeTime = DateTime.MinValue;
        private double _peakOutputBandwidthBytesPerSecond;

        public string WindowTitle => _isSample ? Lang.WindowTitleSampleMode : Lang.WindowTitle;
        public string ProgressTitle => Lang.ProgressTitle;
        public string MemoryTitle => Lang.MemoryTitle;
        public string DistributionTitle => Lang.DistributionTitle;

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
        public string DistributionAvailableLabel => Lang.DistributionAvailableLabel;
        public string HeatLegendUpstreamLabel => Lang.HeatLegendUpstreamLabel;
        public string HeatLegendDownstreamLabel => Lang.HeatLegendDownstreamLabel;
        public string HeatLegendOtherLabel => Lang.HeatLegendOtherLabel;
        public string HeatLegendCacheLabel => Lang.HeatLegendCacheLabel;
        public string HeatLegendColdText => Lang.HeatLegendColdText;
        public string HeatLegendHotText => Lang.HeatLegendHotText;


        public string StderrTitle => Lang.StderrTitle;

        public ObservableCollection<ColumnTextItemM> MetricColumns { get; } = [];
        public ObservableCollection<ColumnTextItemM> FooterColumns { get; } = [];
        public ObservableCollection<HeatMapCellM> HeatMapA { get; } = [];
        public ObservableCollection<string> SampleIntervalTickLabels { get; } = [];
        public ButtonGroupVM MonitorButtons { get; }
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
            set
            {
                if (!SetProperty(ref _sampleIntervalSeconds, Math.Max(0, value))) return;
                _lastMemoryStatsUpdate = DateTime.MinValue;
            }
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

        private string _distributionAvailable = "XXX,XXX MB";
        public string DistributionAvailable
        {
            get => _distributionAvailable;
            set => SetProperty(ref _distributionAvailable, value);
        }

        private string _blockNo = "#X,X,X,XXXMB";
        public string BlockNo
        {
            get => _blockNo;
            set => SetProperty(ref _blockNo, value);
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
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.PhysicalMemoryTopText, MainText = PlaceholderGb, BottomText = Lang.PhysicalMemoryBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.CommittedMemoryTopText, MainText = PlaceholderGb, BottomText = Lang.CommittedMemoryBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.WorkingSetPeakTopText, MainText = PlaceholderGb, BottomText = Lang.WorkingSetPeakBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.PageFileTopText, MainText = PlaceholderGb, BottomText = Lang.PageFileBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.PageFaultTopText, MainText = PlaceholderCount, BottomText = Lang.PageFaultBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.BandwidthPeakTopText, MainText = PlaceholderGbPerSecond, BottomText = Lang.BandwidthPeakBottomText });
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.MemoryPressureTopText, MainText = Lang.MemoryPressureMediumText, BottomText = PlaceholderPercent });
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
            AppendFoldedLine(_upstreamStderrBuilder, _upstreamStderrFoldState, $"{Lang.UpstreamLabel}: {_request.UpstreamExeName}");
            AppendFoldedLine(_upstreamStderrBuilder, _upstreamStderrFoldState, $"{Lang.ExecutableLabel}: {_request.UpstreamPath}");
            AppendFoldedLine(_upstreamStderrBuilder, _upstreamStderrFoldState, $"{Lang.InputLabel}: {_request.UpstreamInputPath}");
            AppendFoldedLine(_upstreamStderrBuilder, _upstreamStderrFoldState, $"{Lang.ArgumentsLabel}: {_command.UpstreamArgs}");
            AppendFoldedLine(_downstreamStderrBuilder, _downstreamStderrFoldState, $"{Lang.EncoderLabel}: {_request.EncoderExeName}");
            AppendFoldedLine(_downstreamStderrBuilder, _downstreamStderrFoldState, $"{Lang.ExecutableLabel}: {_request.EncoderPath}");
            AppendFoldedLine(_downstreamStderrBuilder, _downstreamStderrFoldState, $"{Lang.OutputLabel}: {_request.OutputPath}");
            AppendFoldedLine(_downstreamStderrBuilder, _downstreamStderrFoldState, $"{Lang.ArgumentsLabel}: {_command.EncoderArgs}");
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
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                _upstreamProcess = upstream;
                _encoderProcess = encoder;

                upstream.Start();
                encoder.Start();

                // Pipe upstream stderr → encoder stdin while capturing binary as text
                Task pipeTask = Task.Run(async () =>
                {
                    try
                    {
                        byte[] buffer = new byte[81920];
                        Stream upstreamStdout = upstream.StandardOutput.BaseStream;
                        Stream encoderStdin = encoder.StandardInput.BaseStream;
                        int bytesRead;
                        while (!cancellationToken.IsCancellationRequested && (bytesRead = await upstreamStdout.ReadAsync(buffer, cancellationToken)) > 0)
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
                // Encoder stderr reader
                Task encoderStderrTask = ReadStreamAsync(encoder.StandardError, ProcessLogKind.DownstreamStderr, cancellationToken);

                await Task.WhenAll(pipeTask, upstreamStderrTask, encoderStderrTask);

                await upstream.WaitForExitAsync(cancellationToken);
                await encoder.WaitForExitAsync(cancellationToken);

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
                while (!ct.IsCancellationRequested && (line = await reader.ReadLineAsync(ct)) != null)
                    EnqueueProcessLine(kind, line);
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
        }

        private void EnqueueProcessLine(ProcessLogKind kind, string? line)
        {
            if (line == null) return;
            _logQueue.Enqueue(new ProcessLogEntry(kind, line));
        }

        private void FlushLogsToProperties()
        {
            ProcessQueuedLogs();
            if (IsFrozen) return;
                lock (_logLock)
                {
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
                DateTime now = DateTime.Now;
                if ((now - _lastStatsUpdate).TotalSeconds >= 1d)
                {
                    _lastStatsUpdate = now;
                    UpdateProgressFromLogs();
                    UpdateFooterTimes(final: false);
                }

                if (IsMemorySampleDue(now))
                {
                    _lastMemoryStatsUpdate = now;
                    UpdateMetrics();
                    UpdateHeatMaps();
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
                    case ProcessLogKind.UpstreamStderr:
                        AppendLogWithOverwrite(_upstreamStderrBuilder, _upstreamStderrFoldState, entry.Line, true);
                        break;
                    case ProcessLogKind.DownstreamStderr:
                        AppendLogWithOverwrite(_downstreamStderrBuilder, _downstreamStderrFoldState, entry.Line, true);
                        break;
                }
            }

            if (changed && !IsFrozen)
            {
                lock (_logLock)
                {
                    UpstreamReportText = _upstreamStderrBuilder.ToString();
                    DownstreamReportText = _downstreamStderrBuilder.ToString();
                }
            }
        }

        private void AppendLogWithOverwrite(StringBuilder target, LogFoldState foldState, string text, bool isStderr)
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

                    TrimLastLine(target, foldState);
                    if (!string.IsNullOrWhiteSpace(latest))
                        AppendFoldedLine(target, foldState, latest);
                    continue;
                }

                string line = newlinePart.TrimEnd();
                if (isStderr && TryHandleProgressLine(line)) continue;
                if (!string.IsNullOrWhiteSpace(line))
                    AppendFoldedLine(target, foldState, line);
            }
        }

        private static void AppendFoldedLine(StringBuilder target, LogFoldState foldState, string line)
        {
            if (foldState.LineIndexByText.TryGetValue(line, out int index))
            {
                LogFoldEntry entry = foldState.Entries[index];
                foldState.Entries[index] = entry with { RepeatCount = entry.RepeatCount + 1 };
                RebuildFoldedLog(target, foldState);
                return;
            }

            foldState.LineIndexByText[line] = foldState.Entries.Count;
            foldState.Entries.Add(new LogFoldEntry(line, 1));
            target.AppendLine(line);
        }

        private static void RebuildFoldedLog(StringBuilder target, LogFoldState foldState)
        {
            target.Clear();
            foreach (LogFoldEntry entry in foldState.Entries)
                target.AppendLine(FormatFoldedLine(entry.Line, entry.RepeatCount));
        }

        private static string FormatFoldedLine(string line, int repeatCount)
        {
            return repeatCount > 1 ? $"{line} (x{repeatCount:N0})" : line;
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

        private static void TrimLastLine(StringBuilder builder, LogFoldState foldState)
        {
            foldState.RemoveLastEntry();
            if (builder.Length == 0) return;
            string text = builder.ToString();
            int searchStart = text.Length - 1;
            while (searchStart >= 0 && (text[searchStart] == '\r' || text[searchStart] == '\n'))
                searchStart--;

            int index = text.LastIndexOf('\n', Math.Max(0, searchStart));
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
            long outputBytes = TryGetOutputSizeBytes();
            MemoryStatusSnapshot memoryStatus = GetMemoryStatusSnapshot();
            _lastMemoryStatus = memoryStatus;
            _lastUpstreamWorkingSetBytes = GetWorkingSetBytes(_upstreamProcess);
            _lastEncoderWorkingSetBytes = GetWorkingSetBytes(_encoderProcess);
            long combinedWorkingSetBytes = _lastUpstreamWorkingSetBytes + _lastEncoderWorkingSetBytes;
            UpdateOutputBandwidth(outputBytes);

            MetricColumns[0].MainText = FormatGb(memoryStatus.UsedPhysicalBytes);
            MetricColumns[0].BottomText = ReplaceMetricValue(Lang.PhysicalMemoryBottomText, FormatGb(memoryStatus.TotalPhysicalBytes));
            MetricColumns[1].MainText = FormatGb(memoryStatus.CommittedBytes);
            MetricColumns[1].BottomText = ReplaceMetricValue(Lang.CommittedMemoryBottomText, FormatGb(memoryStatus.CommitLimitBytes));
            MetricColumns[2].MainText = FormatGb(combinedWorkingSetBytes);
            MetricColumns[2].BottomText = ReplaceMetricValue(Lang.WorkingSetPeakBottomText, FormatGb(combinedWorkingSetBytes));
            MetricColumns[3].MainText = FormatGb(memoryStatus.CommittedBytes);
            MetricColumns[3].BottomText = ReplaceMetricValue(Lang.PageFileBottomText, FormatGb(memoryStatus.CommitLimitBytes));
            MetricColumns[4].MainText = GetTotalPageFaults().ToString("N0", CultureInfo.InvariantCulture);
            MetricColumns[4].BottomText = Lang.PageFaultBottomText;
            MetricColumns[5].MainText = FormatGbPerSecond(_peakOutputBandwidthBytesPerSecond);
            MetricColumns[5].BottomText = ReplaceMetricValue(Lang.BandwidthPeakBottomText, FormatGbPerSecond(seconds > 0 ? outputBytes / seconds : 0d));
            MetricColumns[6].MainText = memoryStatus.MemoryLoadPercent < 75 ? Lang.MemoryPressureMediumText : Lang.MemoryPressureHighText;
            MetricColumns[6].BottomText = $"{memoryStatus.MemoryLoadPercent}%";

            DistributionUpstream = FormatMb(_lastUpstreamWorkingSetBytes);
            DistributionDownstream = FormatMb(_lastEncoderWorkingSetBytes);
            DistributionOther = FormatMb(Math.Max(0, memoryStatus.UsedPhysicalBytes - combinedWorkingSetBytes - memoryStatus.SystemCacheBytes));
            DistributionCache = FormatMb(memoryStatus.SystemCacheBytes);
            DistributionAvailable = FormatMb(memoryStatus.AvailablePhysicalBytes);
        }

        private bool IsMemorySampleDue(DateTime now)
        {
            int intervalSeconds = SampleIntervalSeconds;
            if (intervalSeconds <= 0) return true;
            return (now - _lastMemoryStatsUpdate).TotalSeconds >= intervalSeconds;
        }

        private void UpdateProgressFromLogs()
        {
            ProgressValue = InferProgress(ProgressValue, _downstreamStderrBuilder.ToString() + _upstreamStderrBuilder.ToString());
        }

        private void UpdateHeatMaps()
        {
            UpdateHeatMap(HeatMapA, _lastMemoryStatus);
        }

        private void UpdateHeatMap(ObservableCollection<HeatMapCellM> cells, MemoryStatusSnapshot memoryStatus)
        {
            if (cells.Count == 0) return;

            long totalBytes = memoryStatus.TotalPhysicalBytes;
            long usedBytes = memoryStatus.UsedPhysicalBytes;
            if (totalBytes <= 0)
            {
                foreach (HeatMapCellM cell in cells)
                {
                    cell.Level = 0;
                    cell.Category = MemoryCategory.Empty;
                }
                BlockNo = "#-,-,-,-";
                BlockHeat = "0%";
                return;
            }

            long upstreamBytes = _lastUpstreamWorkingSetBytes;
            long downstreamBytes = _lastEncoderWorkingSetBytes;
            long cacheBytes = memoryStatus.SystemCacheBytes;
            long otherBytes = Math.Max(0, usedBytes - upstreamBytes - downstreamBytes - cacheBytes);

            const int BandCount = 4;
            const int BandRowCount = HeatMapRows / BandCount;
            const int CellsPerBand = BandRowCount * HeatMapColumns;

            string[] categoryNames =
            {
                Lang.HeatLegendUpstreamLabel,
                Lang.HeatLegendDownstreamLabel,
                Lang.HeatLegendOtherLabel,
                Lang.HeatLegendCacheLabel
            };
            MemoryCategory[] categoryOrder =
            {
                MemoryCategory.Upstream,
                MemoryCategory.Downstream,
                MemoryCategory.Other,
                MemoryCategory.Cache
            };
            long[] categoryBytes = { upstreamBytes, downstreamBytes, otherBytes, cacheBytes };

            int hottestCellIndex = 0;
            int hottestLevel = 0;
            double hottestFillFraction = 0;
            MemoryCategory hottestCategory = MemoryCategory.Upstream;
            bool anyHottest = false;

            for (int bandIndex = 0; bandIndex < BandCount; bandIndex++)
            {
                long bytes = categoryBytes[bandIndex];
                MemoryCategory category = categoryOrder[bandIndex];
                string categoryName = categoryNames[bandIndex];

                double fillFraction = Math.Clamp(bytes / (double)totalBytes, 0d, 1d);
                int cellsToFill = Math.Min(CellsPerBand, (int)Math.Ceiling(fillFraction * CellsPerBand));

                int bandStartCell = bandIndex * CellsPerBand;
                for (int i = 0; i < CellsPerBand; i++)
                {
                    int cellIndex = bandStartCell + i;
                    HeatMapCellM cell = cells[cellIndex];
                    cell.Category = category;

                    if (i < cellsToFill)
                    {
                        int level;
                        if (cellsToFill == 1)
                            level = HeatMapMaxLevel;
                        else
                            level = 1 + (i * HeatMapMaxLevel / cellsToFill);
                        if (level < 1) level = 1;
                        if (level > HeatMapMaxLevel) level = HeatMapMaxLevel;
                        cell.Level = level;

                        if (level >= hottestLevel)
                        {
                            hottestLevel = level;
                            hottestCellIndex = cellIndex;
                            hottestFillFraction = fillFraction;
                            hottestCategory = category;
                            anyHottest = true;
                        }
                    }
                    else
                    {
                        cell.Level = 0;
                    }

                    int localRow = i / HeatMapColumns;
                    int localCol = i % HeatMapColumns;
                    int globalRow = bandIndex * BandRowCount + localRow;
                    cell.Tooltip = string.Format(
                        Lang.BlockTooltipFormat,
                        cellIndex)
                        + $" | {categoryName} | R{globalRow}C{localCol} | {FormatMb(bytes)} | {fillFraction * 100:F1}%";
                }
            }

            if (anyHottest)
            {
                int row = hottestCellIndex / HeatMapColumns;
                int column = hottestCellIndex % HeatMapColumns;
                BlockNo = $"#{hottestCellIndex},{row},{column},{hottestCategory}";
                BlockHeat = $"{hottestFillFraction * 100:F1}%";
            }
            else
            {
                BlockNo = "#-,-,-,-";
                BlockHeat = "0%";
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

        private long TryGetOutputSizeBytes()
        {
            try
            {
                if (!File.Exists(_request.OutputPath)) return 0L;
                return new FileInfo(_request.OutputPath).Length;
            }
            catch
            {
                return 0L;
            }
        }

        private void UpdateOutputBandwidth(long outputBytes)
        {
            DateTime now = DateTime.Now;
            if (_lastOutputSizeTime != DateTime.MinValue)
            {
                double elapsedSeconds = Math.Max(0.001d, (now - _lastOutputSizeTime).TotalSeconds);
                long deltaBytes = Math.Max(0, outputBytes - _lastOutputSizeBytes);
                _peakOutputBandwidthBytesPerSecond = Math.Max(_peakOutputBandwidthBytesPerSecond, deltaBytes / elapsedSeconds);
            }

            _lastOutputSizeBytes = outputBytes;
            _lastOutputSizeTime = now;
        }

        private static long GetWorkingSetBytes(Process? process)
        {
            try
            {
                if (process == null || process.HasExited) return 0L;
                process.Refresh();
                return Math.Max(0L, process.WorkingSet64);
            }
            catch
            {
                return 0L;
            }
        }

        private long GetTotalPageFaults()
        {
            return GetPageFaults(_upstreamProcess) + GetPageFaults(_encoderProcess);
        }

        private static long GetPageFaults(Process? process)
        {
            try
            {
                if (process == null || process.HasExited) return 0L;
                process.Refresh();
                PROCESS_MEMORY_COUNTERS counters = new()
                {
                    cb = (uint)Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>()
                };

                return GetProcessMemoryInfo(process.Handle, ref counters, counters.cb)
                    ? counters.PageFaultCount
                    : 0L;
            }
            catch
            {
                return 0L;
            }
        }

        private static string FormatGb(long bytes)
        {
            return $"{Math.Max(0d, bytes / (double)BytesPerGb):0.0} GB";
        }

        private static string FormatMb(long bytes)
        {
            return $"{Math.Max(0d, bytes / (double)BytesPerMb):N0} MB";
        }

        private static string FormatGbPerSecond(double bytesPerSecond)
        {
            return $"{Math.Max(0d, bytesPerSecond / BytesPerGb):0.0} GBps";
        }

        private static string ReplaceMetricValue(string template, string value)
        {
            return FileSizeMetricRegex().Replace(template, value);
        }

        private static MemoryStatusSnapshot GetMemoryStatusSnapshot()
        {
            if (!OperatingSystem.IsWindows()) return default;

            MEMORYSTATUSEX memoryStatus = new()
            {
                dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
            };

            if (!GlobalMemoryStatusEx(ref memoryStatus)) return default;

            long totalPhysicalBytes = ToNonNegativeLong(memoryStatus.ullTotalPhys);
            long availablePhysicalBytes = Math.Min(totalPhysicalBytes, ToNonNegativeLong(memoryStatus.ullAvailPhys));
            long commitLimitBytes = ToNonNegativeLong(memoryStatus.ullTotalPageFile);
            long commitAvailableBytes = Math.Min(commitLimitBytes, ToNonNegativeLong(memoryStatus.ullAvailPageFile));
            long committedBytes = Math.Max(0, commitLimitBytes - commitAvailableBytes);
            long systemCacheBytes = 0;

            PERFORMANCE_INFORMATION performanceInfo = new()
            {
                cb = (uint)Marshal.SizeOf<PERFORMANCE_INFORMATION>()
            };

            if (GetPerformanceInfo(ref performanceInfo, performanceInfo.cb) && performanceInfo.PageSize != 0)
            {
                ulong pageSize = performanceInfo.PageSize;
                totalPhysicalBytes = ToNonNegativeLong(performanceInfo.PhysicalTotal * pageSize);
                availablePhysicalBytes = Math.Min(totalPhysicalBytes, ToNonNegativeLong(performanceInfo.PhysicalAvailable * pageSize));
                commitLimitBytes = ToNonNegativeLong(performanceInfo.CommitLimit * pageSize);
                committedBytes = Math.Min(commitLimitBytes, ToNonNegativeLong(performanceInfo.CommitTotal * pageSize));
                systemCacheBytes = Math.Min(totalPhysicalBytes, ToNonNegativeLong(performanceInfo.SystemCache * pageSize));
            }

            return new MemoryStatusSnapshot(
                totalPhysicalBytes,
                availablePhysicalBytes,
                commitLimitBytes,
                committedBytes,
                systemCacheBytes,
                Math.Clamp((int)memoryStatus.dwMemoryLoad, 0, 100));
        }

        private static long ToNonNegativeLong(ulong value)
        {
            return value > long.MaxValue ? long.MaxValue : (long)value;
        }

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [LibraryImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetProcessMemoryInfo(IntPtr Process, ref PROCESS_MEMORY_COUNTERS ppsmemCounters, uint cb);

        [LibraryImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb;
            public uint PageFaultCount;
            public nuint PeakWorkingSetSize;
            public nuint WorkingSetSize;
            public nuint QuotaPeakPagedPoolUsage;
            public nuint QuotaPagedPoolUsage;
            public nuint QuotaPeakNonPagedPoolUsage;
            public nuint QuotaNonPagedPoolUsage;
            public nuint PagefileUsage;
            public nuint PeakPagefileUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PERFORMANCE_INFORMATION
        {
            public uint cb;
            public nuint CommitTotal;
            public nuint CommitLimit;
            public nuint CommitPeak;
            public nuint PhysicalTotal;
            public nuint PhysicalAvailable;
            public nuint SystemCache;
            public nuint KernelTotal;
            public nuint KernelPaged;
            public nuint KernelNonpaged;
            public nuint PageSize;
            public uint HandleCount;
            public uint ProcessCount;
            public uint ThreadCount;
        }

        private readonly record struct MemoryStatusSnapshot(
            long TotalPhysicalBytes,
            long AvailablePhysicalBytes,
            long CommitLimitBytes,
            long CommittedBytes,
            long SystemCacheBytes,
            int MemoryLoadPercent)
        {
            public long UsedPhysicalBytes => Math.Max(0, TotalPhysicalBytes - AvailablePhysicalBytes);
        }

        private void ResetStats()
        {
            foreach (ColumnTextItemM item in MetricColumns)
                item.BottomText = item.BottomText;
            _lastMemoryStatsUpdate = DateTime.MinValue;
            _lastOutputSizeBytes = 0;
            _lastOutputSizeTime = DateTime.MinValue;
            _peakOutputBandwidthBytesPerSecond = 0d;
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
                return string.Join(Environment.NewLine, _upstreamStderrBuilder, _downstreamStderrBuilder);
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
            OnPropertyChanged(nameof(DistributionAvailableLabel));
            OnPropertyChanged(nameof(HeatLegendUpstreamLabel));
            OnPropertyChanged(nameof(HeatLegendDownstreamLabel));
            OnPropertyChanged(nameof(HeatLegendOtherLabel));
            OnPropertyChanged(nameof(HeatLegendCacheLabel));
            OnPropertyChanged(nameof(HeatLegendColdText));
            OnPropertyChanged(nameof(HeatLegendHotText));


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
            UpstreamStderr,
            DownstreamStderr
        }

        private readonly record struct ProcessLogEntry(ProcessLogKind Kind, string Line);

        private readonly record struct LogFoldEntry(string Line, int RepeatCount);

        private sealed class LogFoldState
        {
            public List<LogFoldEntry> Entries { get; } = [];
            public Dictionary<string, int> LineIndexByText { get; } = new(StringComparer.Ordinal);

            public void RemoveLastEntry()
            {
                if (Entries.Count == 0) return;
                LogFoldEntry lastEntry = Entries[^1];
                LineIndexByText.Remove(lastEntry.Line);
                Entries.RemoveAt(Entries.Count - 1);
            }
        }

        [GeneratedRegex(@"X+(?:\.X+)?\s*(?:GBps|GB|MB|%)?", RegexOptions.CultureInvariant)]
        private static partial Regex FileSizeMetricRegex();
    }
}
