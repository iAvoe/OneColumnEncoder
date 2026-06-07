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
    public partial class EncodingMonitorModalVM : BaseVM
    {
        private const int MemoryRangeBlockCount = 128;
        private const int MemoryRangeMaxFillLevel = 8;
        private const long BytesPerMb = 1024L * 1024L;
        private const long BytesPerGb = 1024L * 1024L * 1024L;
        private const uint TH32CS_SNAPPROCESS = 0x00000002;
        private static readonly IntPtr InvalidHandleValue = new(-1);
        private const string PlaceholderGb = "XX.X GB";
        private const string PlaceholderCount = "XX,XXX";
        private const string PlaceholderPercent = "XXX%";
        private EncodingMonitorModalLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);
        public EncodingMonitorModalLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }
        private CpuSetsLangProviderM _cpuSetsLang = new(UILangProviderM.Current.LanguageCode);
        private readonly ModalNavS _modalNavS;
        private readonly Action _closeAction;
        private readonly EncodingPipelineRequest _request;
        private readonly EncodingPipelineCommand _command;
        private readonly AppConfM _appConfM;
        private readonly bool _isSample;
        private readonly Stopwatch _stopwatch = new();
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
        private readonly StringBuilder _upstreamStderrBuilder = new();
        private readonly StringBuilder _downstreamStderrBuilder = new();
        private readonly LogFoldState _upstreamStderrFoldState = new();
        private readonly LogFoldState _downstreamStderrFoldState = new();
        private readonly ConcurrentQueue<ProcessLogEntry> _logQueue = new();
        private readonly long? _totalFrames;
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
        private long _upstreamWorkingSetPeakBytes;
        private long _encoderWorkingSetPeakBytes;
        private long _currentOutputSizeBytes;
        private int _writtenFrames;
        private bool _userInterruptRequested;
        private Stream? _upstreamStdoutStream;
        private Stream? _encoderStdinStream;

        public string WindowTitle => _isSample ? Lang.WindowTitleSampleMode : Lang.WindowTitle;
        public string ProgressTitle => Lang.ProgressTitle;
        public string MemoryTitle => Lang.MemoryTitle;

        public string DragLogReportHint => Lang.DragLogReportHint;
        public string CurrentSizeLabel => $"{Lang.CurrentSizeLabel}: {FormatGbValue(_currentOutputSizeBytes)}";
        public string EstimatedSizeLabel => $"{Lang.EstimatedSizeLabel}: {GetEstimatedOutputSizeText()}";
        public string WrittenFramesLabel => $"{Lang.WrittenFramesLabel}: {GetWrittenFramesText()}";
        public string SampleIntervalLabel => Lang.SampleIntervalLabel;
        public string StartedAtLabel => Lang.StartedAtLabel;
        public string ElapsedLabel => Lang.ElapsedLabel;
        public string RemainingLabel => Lang.RemainingLabel;
        public string CompleteAtLabel => Lang.CompleteAtLabel;
        public static string RateControlLabel => "ABR / CRF";
        public string ArgsLabel => Lang.ArgsLabel;
        public string SmallNoteText => Lang.SmallNoteText;
        public string DistributionUpstreamLabel => Lang.DistributionUpstreamLabel;
        public string DistributionDownstreamLabel => Lang.DistributionDownstreamLabel;
        public string DistributionCacheLabel => Lang.DistributionCacheLabel;
        public string DistributionAvailableLabel => Lang.DistributionAvailableLabel;
        public string MemoryRangeLegendTitle => Lang.MemoryRangeLegendTitle;

        public string StderrTitle => Lang.StderrTitle;

        public ObservableCollection<ColumnTextItemM> MetricColumns { get; } = [];
        public ObservableCollection<ColumnTextItemM> FooterColumns { get; } = [];
        public ObservableCollection<MemoryRangeBlockM> MemoryRangeBlocks { get; } = [];
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
                OnPropertyChanged(nameof(EstimatedSizeLabel));
            }
        }

        public string ProgressText => $"{ProgressValue}%";

        private int _sampleIntervalSeconds = 10;
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

        private string _rangeSummary = "XX.X%";
        public string RangeSummary
        {
            get => _rangeSummary;
            set => SetProperty(ref _rangeSummary, value);
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
            _totalFrames = EncodingPipelineH.GetSourceTotalFrames(_request.SourceFfprobeJson);

            RefreshLanguageState();

            FreezeOrContinueCmd = new ActionCmd(_ => IsFrozen = !IsFrozen);
            ResetStatsCmd = new ActionCmd(_ => ResetStats());
            CloseCmd = new CloseModalCmd(() =>
            {
                if (!_finishEnabledAfterClose) return;
                _closeAction();
            });

            MonitorButtons = ButtonGroupVM.CreateTwoButton(FreezeOrContinueText, Lang.ResetUsageText, FreezeOrContinueCmd, ResetStatsCmd);

            ReportButtons = ButtonGroupVM.CreateThreeButton(
                Lang.SaveUpstreamStderrText, Lang.SaveDownstreamStderrText, Lang.RotateLogFontSizeText,
                new ActionCmd(_ => SaveText(UpstreamReportText, "upstream-stderr.txt")),
                new ActionCmd(_ => SaveText(DownstreamReportText, "downstream-stderr.txt")),
                new ActionCmd(_ => RotateLogFontSize()));

            FinishButtons = ButtonGroupVM.CreateFiveButton(
                Lang.OpenOutputDirectoryText, Lang.ViewEncodingCommandText, Lang.InterruptUpstreamText, Lang.InterruptEncoderText, Lang.CloseAfterDoneText,
                new ActionCmd(_ => OpenOutputDirectory()),
                new ActionCmd(_ => new OpenDebugModalCmd(_modalNavS, Lang.EncodingCommandTitle, _command.CommandLine).Execute(null)),
                new ActionCmd(_ => TryInterruptUpstream()),
                new ActionCmd(_ => TryInterruptEncoder()),
                CloseCmd);
            FinishButtons.B5_5IsEnabled = false;

            BuildMetrics();
            BuildFooter();
            BuildMemoryRangeBlocks();
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
            MetricColumns.Add(new ColumnTextItemM { TopText = Lang.RAMStressTopText, MainText = Lang.RAMStressMediumText, BottomText = PlaceholderPercent });
        }

        private void BuildFooter()
        {
            FooterColumns.Clear();
            FooterColumns.Add(new ColumnTextItemM { TopText = StartedAtLabel, MainText = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) });
            FooterColumns.Add(new ColumnTextItemM { TopText = ElapsedLabel, MainText = "00:00:00" });
            FooterColumns.Add(new ColumnTextItemM { TopText = RemainingLabel, MainText = "--:--:--" });
            FooterColumns.Add(new ColumnTextItemM { TopText = CompleteAtLabel, MainText = "--:--:--" });
            FooterColumns.Add(new ColumnTextItemM { TopText = RateControlLabel, MainText = GetRateControlText() });
            FooterColumns.Add(new ColumnTextItemM { TopText = ArgsLabel, MainText = GetPresetText() });
        }

        private void BuildMemoryRangeBlocks()
        {
            MemoryRangeBlocks.Clear();
            for (int i = 0; i < MemoryRangeBlockCount; i++)
            {
                MemoryRangeBlocks.Add(new MemoryRangeBlockM { FillLevel = 0, Tooltip = string.Format(Lang.BlockTooltipFormat, i) });
            }
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
                ApplyParallelismSettings(upstream, isEncoder: false);
                encoder.Start();
                ApplyParallelismSettings(encoder, isEncoder: true);

                // Pipe upstream stdout to encoder stdin. Closing encoder stdin lets the encoder flush on EOF.
                Task pipeTask = Task.Run(async () =>
                {
                    Stream? encoderStdin = null;
                    try
                    {
                        byte[] buffer = new byte[81920];
                        Stream upstreamStdout = upstream.StandardOutput.BaseStream;
                        encoderStdin = encoder.StandardInput.BaseStream;
                        _upstreamStdoutStream = upstreamStdout;
                        _encoderStdinStream = encoderStdin;
                        int bytesRead;
                        while (!cancellationToken.IsCancellationRequested && (bytesRead = await upstreamStdout.ReadAsync(buffer, cancellationToken)) > 0)
                        {
                            await encoderStdin.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                            await encoderStdin.FlushAsync(cancellationToken);
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        EnqueueProcessLine(ProcessLogKind.UpstreamStderr, Lang.PipeErrorPrefix + ex.Message);
                    }
                    finally
                    {
                        TryCloseStream(encoderStdin);
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
                UpdateProgressDetails();
                StatusText = _success ? Lang.CompletedText : _userInterruptRequested ? Lang.InterruptedText : StatusText == Lang.EncodingText ? Lang.FailedText : StatusText;
                FlushLogsToProperties();
                UpdateFooterTimes(final: true);
                EnableCloseButton();
                _upstreamStdoutStream = null;
                _encoderStdinStream = null;
            }

            if (!_isSample)
                await TrySendNotificationAsync(_success, processOutput);
        }

        private void ApplyParallelismSettings(Process process, bool isEncoder)
        {
            ParallelismConfM? parallelismConf = _request.ParallelismConf;
            if (parallelismConf == null) return;

            int nodeId = isEncoder ? parallelismConf.DownstreamNodeId : parallelismConf.UpstreamNodeId;
            bool physicalOnly = isEncoder ? parallelismConf.PreferPhysicalCores : parallelismConf.PreferUpstreamPhysicalCores;
            int? maxCpuSets = isEncoder ? parallelismConf.EncoderThreadCount : null;
            ProcessLogKind logKind = isEncoder ? ProcessLogKind.DownstreamStderr : ProcessLogKind.UpstreamStderr;

            bool success = CpuSetsH.TryApplyProcessDefaultCpuSets(
                process,
                nodeId,
                physicalOnly,
                maxCpuSets,
                _cpuSetsLang,
                out string message);
            EnqueueProcessLine(logKind, $"Parallelism: {(success ? message : _cpuSetsLang.SkippedPrefix + message)}");
        }

        private async Task ReadStreamAsync(StreamReader reader, ProcessLogKind kind, CancellationToken ct)
        {
            try
            {
                char[] buffer = new char[4096];
                StringBuilder lineBuilder = new();
                string? pendingCarriageReturnLine = null;
                bool previousWasCarriageReturnUpdate = false;

                while (!ct.IsCancellationRequested)
                {
                    int charsRead = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
                    if (charsRead == 0) break;

                    for (int i = 0; i < charsRead; i++)
                    {
                        char ch = buffer[i];
                        if (pendingCarriageReturnLine != null)
                        {
                            if (ch == '\n')
                            {
                                EnqueueProcessLine(kind, pendingCarriageReturnLine, overwritesPreviousLine: false);
                                pendingCarriageReturnLine = null;
                                previousWasCarriageReturnUpdate = false;
                                continue;
                            }

                            EnqueueProcessLine(kind, pendingCarriageReturnLine, previousWasCarriageReturnUpdate);
                            pendingCarriageReturnLine = null;
                            previousWasCarriageReturnUpdate = true;
                        }

                        if (ch == '\r')
                        {
                            pendingCarriageReturnLine = lineBuilder.ToString();
                            lineBuilder.Clear();
                            continue;
                        }

                        if (ch == '\n')
                        {
                            EnqueueProcessLine(kind, lineBuilder.ToString(), overwritesPreviousLine: false);
                            lineBuilder.Clear();
                            previousWasCarriageReturnUpdate = false;
                            continue;
                        }

                        lineBuilder.Append(ch);
                    }
                }

                if (pendingCarriageReturnLine != null)
                    EnqueueProcessLine(kind, pendingCarriageReturnLine, previousWasCarriageReturnUpdate);
                if (lineBuilder.Length > 0)
                    EnqueueProcessLine(kind, lineBuilder.ToString(), overwritesPreviousLine: false);
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
        }

        private void EnqueueProcessLine(ProcessLogKind kind, string? line, bool overwritesPreviousLine = false)
        {
            if (line == null) return;
            _logQueue.Enqueue(new ProcessLogEntry(kind, line, overwritesPreviousLine));
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
                    UpdateProgressDetails();
                    UpdateFooterTimes(final: false);
                }

                if (IsMemorySampleDue(now))
                {
                    _lastMemoryStatsUpdate = now;
                    UpdateMetrics();
                    UpdateMemoryRangeBlocks();
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
                        AppendLogWithOverwrite(
                            _upstreamStderrBuilder,
                            _upstreamStderrFoldState,
                            entry.Line,
                            entry.OverwritesPreviousLine,
                            updateMainProgress: false);
                        break;
                    case ProcessLogKind.DownstreamStderr:
                        AppendLogWithOverwrite(
                            _downstreamStderrBuilder,
                            _downstreamStderrFoldState,
                            entry.Line,
                            entry.OverwritesPreviousLine,
                            updateMainProgress: true);
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

        private void AppendLogWithOverwrite(
            StringBuilder target,
            LogFoldState foldState,
            string text,
            bool overwritesPreviousLine,
            bool updateMainProgress)
        {
            string line = text.Replace("\0", string.Empty, StringComparison.Ordinal).TrimEnd();
            if (string.IsNullOrWhiteSpace(line)) return;

            UpdateProgressFromLogLine(line, updateMainProgress);
            if (overwritesPreviousLine)
                TrimLastLine(target, foldState);
            AppendFoldedLine(target, foldState, line);
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

        private void UpdateProgressFromLogLine(string line, bool updateMainProgress)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || IsIndexProgressLine(trimmed)) return;

            bool isProgressLine = IsProgressLine(trimmed);
            if (isProgressLine && updateMainProgress)
                ProgressValue = InferProgress(ProgressValue, trimmed);
            if (TryParseEncoderFrame(trimmed) is int frame)
            {
                UpdateWrittenFrames(frame);
                if (updateMainProgress && _totalFrames is > 0)
                {
                    int frameProgress = (int)Math.Min(100d, Math.Round(frame * 100d / _totalFrames.Value));
                    ProgressValue = Math.Max(ProgressValue, frameProgress);
                }
            }
            if (isProgressLine && updateMainProgress)
                StatusText = trimmed;
        }

        [GeneratedRegex(@"(?<![\d.])\d{1,3}(?:\.\d+)?\s*%")]
        private static partial Regex ProgressLineRegex();

        [GeneratedRegex(@"(?<![\d.])(\d{1,3})(?:\.\d+)?\s*%")]
        private static partial Regex ProgressPercentRegex();

        [GeneratedRegex(@"(?:^|\s)(?:frame|fps|size|time|bitrate|speed|dup|drop|progress)\s*=\s*[^\s]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex FfmpegProgressFieldRegex();

        [GeneratedRegex(@"(?:^|\s)(?:frame|fps|size|time|bitrate|speed|dup|drop)\s*[=:]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex FfmpegProgressKeyRegex();

        private static bool IsProgressLine(string line)
        {
            string lower = line.ToLowerInvariant();
            return FfmpegProgressFieldRegex().IsMatch(line)
                || FfmpegProgressKeyRegex().IsMatch(line)
                || lower.Contains("progress=continue", StringComparison.Ordinal)
                || lower.Contains("progress=end", StringComparison.Ordinal)
                || lower.Contains("fps", StringComparison.Ordinal) && lower.Contains("size=", StringComparison.Ordinal)
                || lower.Contains("frames", StringComparison.Ordinal) && lower.Contains("kb/s", StringComparison.Ordinal)
                || lower.Contains("eta", StringComparison.Ordinal) && lower.Contains('%', StringComparison.Ordinal)
                || ProgressLineRegex().IsMatch(line);
        }

        private static bool IsIndexProgressLine(string line)
        {
            return line.Contains("Creating lwi index file", StringComparison.OrdinalIgnoreCase);
        }

        [GeneratedRegex(@"(?:^|\D)frame\s*=\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex FfmpegFrameRegex();

        [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s*:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex X264FrameRegex();

        [GeneratedRegex(@"(?<!\d)(\d+)\s*/\s*\d+\s+frames?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex SlashFrameRegex();

        [GeneratedRegex(@"\bencoding\s+frame\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex EncodingFrameRegex();

        [GeneratedRegex(@"(?:^|\D)encoded\s+(\d+)\s+frames?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex EncodedFrameRegex();

        [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s+encoded", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex FramesEncodedRegex();

        private static int? TryParseEncoderFrame(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            if (TryParseFirstRegexGroup(FfmpegFrameRegex().Match(line), out int value)) return value;
            if (TryParseFirstRegexGroup(X264FrameRegex().Match(line), out value)) return value;
            if (TryParseFirstRegexGroup(SlashFrameRegex().Match(line), out value)) return value;
            if (TryParseFirstRegexGroup(EncodingFrameRegex().Match(line), out value)) return value;
            if (TryParseFirstRegexGroup(EncodedFrameRegex().Match(line), out value)) return value;
            if (TryParseFirstRegexGroup(FramesEncodedRegex().Match(line), out value)) return value;
            return null;
        }

        private static bool TryParseFirstRegexGroup(Match match, out int value)
        {
            value = 0;
            return match.Success
                && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
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
            MemoryStatusSnapshot memoryStatus = GetMemoryStatusSnapshot();
            _lastMemoryStatus = memoryStatus;

            Dictionary<int, List<int>>? childMap = GetChildProcessMap();

            _lastUpstreamWorkingSetBytes = GetWorkingSetBytes(_upstreamProcess, childMap);
            _lastEncoderWorkingSetBytes = GetWorkingSetBytes(_encoderProcess, childMap);
            long combinedWorkingSetBytes = _lastUpstreamWorkingSetBytes + _lastEncoderWorkingSetBytes;
            _upstreamWorkingSetPeakBytes = Math.Max(_upstreamWorkingSetPeakBytes, _lastUpstreamWorkingSetBytes);
            _encoderWorkingSetPeakBytes = Math.Max(_encoderWorkingSetPeakBytes, _lastEncoderWorkingSetBytes);
            long combinedWorkingSetPeakBytes = _upstreamWorkingSetPeakBytes + _encoderWorkingSetPeakBytes;
            long effectiveSystemCacheBytes = GetEffectiveSystemCacheBytes(memoryStatus, combinedWorkingSetBytes);

            MetricColumns[0].MainText = FormatGb(memoryStatus.UsedPhysicalBytes);
            MetricColumns[0].BottomText = ReplaceMetricValue(Lang.PhysicalMemoryBottomText, FormatGb(memoryStatus.TotalPhysicalBytes));
            MetricColumns[1].MainText = FormatGb(memoryStatus.CommittedBytes);
            MetricColumns[1].BottomText = ReplaceMetricValue(Lang.CommittedMemoryBottomText, FormatGb(memoryStatus.CommitLimitBytes));
            MetricColumns[2].MainText = FormatGb(combinedWorkingSetPeakBytes);
            MetricColumns[2].BottomText = ReplaceMetricValue(Lang.WorkingSetPeakBottomText, FormatGb(combinedWorkingSetBytes));
            MetricColumns[3].MainText = FormatGb(memoryStatus.CommittedBytes);
            MetricColumns[3].BottomText = ReplaceMetricValue(Lang.PageFileBottomText, FormatGb(memoryStatus.CommitLimitBytes));
            MetricColumns[4].MainText = GetTotalPageFaults(childMap).ToString("N0", CultureInfo.InvariantCulture);
            MetricColumns[4].BottomText = Lang.PageFaultBottomText;
            MetricColumns[5].MainText = memoryStatus.MemoryLoadPercent < 75 ? Lang.RAMStressMediumText : Lang.RAMStressHighText;
            MetricColumns[5].BottomText = $"{memoryStatus.MemoryLoadPercent}%";

            DistributionUpstream = FormatMb(_lastUpstreamWorkingSetBytes);
            DistributionDownstream = FormatMb(_lastEncoderWorkingSetBytes);
            DistributionCache = FormatMb(effectiveSystemCacheBytes);
            DistributionAvailable = FormatMb(memoryStatus.AvailablePhysicalBytes);
        }

        private static long GetEffectiveSystemCacheBytes(MemoryStatusSnapshot memoryStatus, long processWorkingSetBytes)
        {
            long nonProcessUsedBytes = Math.Max(0, memoryStatus.UsedPhysicalBytes - processWorkingSetBytes);
            return Math.Min(memoryStatus.SystemCacheBytes, nonProcessUsedBytes);
        }

        private bool IsMemorySampleDue(DateTime now)
        {
            int intervalSeconds = SampleIntervalSeconds;
            if (intervalSeconds <= 0) return true;
            return (now - _lastMemoryStatsUpdate).TotalSeconds >= intervalSeconds;
        }

        private void UpdateProgressFromLogs()
        {
            ProgressValue = InferProgress(ProgressValue, _downstreamStderrBuilder.ToString());
        }

        private void UpdateProgressDetails()
        {
            SetCurrentOutputSizeBytes(TryGetOutputSizeBytes());
        }

        private void UpdateMemoryRangeBlocks()
        {
            UpdateMemoryRangeBlocks(MemoryRangeBlocks, _lastMemoryStatus);
        }

        private void UpdateMemoryRangeBlocks(ObservableCollection<MemoryRangeBlockM> blocks, MemoryStatusSnapshot memoryStatus)
        {
            if (blocks.Count == 0) return;

            long totalBytes = memoryStatus.TotalPhysicalBytes;
            long usedBytes = memoryStatus.UsedPhysicalBytes;
            if (totalBytes <= 0)
            {
                foreach (MemoryRangeBlockM block in blocks)
                {
                    block.FillLevel = 0;
                    block.Category = MemoryCategory.Empty;
                }
                RangeSummary = "0%";
                return;
            }

            long upstreamBytes = _lastUpstreamWorkingSetBytes;
            long downstreamBytes = _lastEncoderWorkingSetBytes;
            long otherUsedBytes = Math.Max(0, usedBytes - upstreamBytes - downstreamBytes);

            string[] categoryNames =
            [
                Lang.DistributionUpstreamLabel,
                Lang.DistributionDownstreamLabel,
                Lang.DistributionCacheLabel
            ];
            MemoryCategory[] categoryOrder =
            [
                MemoryCategory.Upstream,
                MemoryCategory.Downstream,
                MemoryCategory.Cache
            ];
            long[] categoryBytes = [upstreamBytes, downstreamBytes, otherUsedBytes];
            double occupancyFraction = Math.Clamp(usedBytes / (double)totalBytes, 0d, 1d);

            int totalBlocks = blocks.Count;
            double bytesPerBlock = totalBytes / (double)totalBlocks;
            long[] categoryEnds = new long[categoryBytes.Length];
            long cumulativeBytes = 0;
            for (int i = 0; i < categoryBytes.Length; i++)
            {
                cumulativeBytes = Math.Min(usedBytes, cumulativeBytes + Math.Max(0, categoryBytes[i]));
                categoryEnds[i] = cumulativeBytes;
            }

            for (int blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
            {
                double blockStart = blockIndex * bytesPerBlock;
                double blockEnd = blockStart + bytesPerBlock;
                double usedOverlap = Math.Max(0d, Math.Min(blockEnd, usedBytes) - blockStart);
                double fillFraction = bytesPerBlock > 0 ? usedOverlap / bytesPerBlock : 0d;
                MemoryRangeBlockM block = blocks[blockIndex];

                if (fillFraction <= 0d)
                {
                    block.FillLevel = 0;
                    block.Category = MemoryCategory.Empty;
                    block.Tooltip = string.Format(Lang.BlockTooltipFormat, blockIndex)
                        + $" | {Lang.DistributionAvailableLabel} | {FormatRange(blockStart, blockEnd)} | {FormatMb((long)Math.Round(bytesPerBlock))} | 0.0%";
                    continue;
                }

                int categoryIndex = 0;
                double blockMiddle = blockStart + usedOverlap / 2d;
                while (categoryIndex < categoryEnds.Length - 1 && blockMiddle >= categoryEnds[categoryIndex])
                    categoryIndex++;

                MemoryCategory category = categoryOrder[categoryIndex];
                string categoryName = categoryNames[categoryIndex];
                int fillLevel = Math.Clamp((int)Math.Ceiling(fillFraction * MemoryRangeMaxFillLevel), 1, MemoryRangeMaxFillLevel);
                block.FillLevel = fillLevel;
                block.Category = category;

                block.Tooltip = string.Format(Lang.BlockTooltipFormat, blockIndex)
                    + $" | {categoryName} | {FormatRange(blockStart, blockEnd)} | {FormatMb((long)Math.Round(usedOverlap))} | {occupancyFraction * 100:F1}%";
            }

            RangeSummary = $"{occupancyFraction * 100:F1}%";
        }

        private static string FormatRange(double startBytes, double endBytes)
        {
            return $"{FormatMb((long)Math.Round(startBytes))}-{FormatMb((long)Math.Round(endBytes))}";
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
            int found = current;
            foreach (string line in log.Split('\n'))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || IsIndexProgressLine(trimmed)) continue;

                MatchCollection matches = ProgressPercentRegex().Matches(trimmed);
                foreach (Match match in matches)
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
                        found = Math.Max(found, Math.Clamp(value, 0, 100));
                }
            }
            return found;
        }

        private long TryGetOutputSizeBytes()
        {
            try
            {
                string resolvedPath = EncodingPipelineH.ResolveOutputPathWithExtension(_request.EncoderExeName, _request.OutputPath);
                if (!File.Exists(resolvedPath)) return 0L;
                return new FileInfo(resolvedPath).Length;
            }
            catch
            {
                return 0L;
            }
        }

        private void SetCurrentOutputSizeBytes(long outputSizeBytes)
        {
            long next = Math.Max(0L, outputSizeBytes);
            if (_currentOutputSizeBytes == next) return;

            _currentOutputSizeBytes = next;
            OnPropertyChanged(nameof(CurrentSizeLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));
        }

        private void UpdateWrittenFrames(int frame)
        {
            int next = Math.Max(_writtenFrames, frame);
            if (_writtenFrames == next) return;

            _writtenFrames = next;
            OnPropertyChanged(nameof(WrittenFramesLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));
        }

        private string GetEstimatedOutputSizeText()
        {
            double progressRatio = GetProgressRatio();
            if (_currentOutputSizeBytes <= 0 || progressRatio <= 0d) return Lang.NotAvailableText;

            double estimatedBytes = _currentOutputSizeBytes / progressRatio;
            if (double.IsNaN(estimatedBytes) || double.IsInfinity(estimatedBytes)) return Lang.NotAvailableText;
            return FormatGbValue((long)Math.Round(Math.Max(0d, estimatedBytes)));
        }

        private double GetProgressRatio()
        {
            if (_totalFrames is > 0 && _writtenFrames > 0)
                return Math.Clamp(_writtenFrames / (double)_totalFrames.Value, 0d, 1d);

            return ProgressValue > 0 ? Math.Clamp(ProgressValue / 100d, 0d, 1d) : 0d;
        }

        private string GetWrittenFramesText()
        {
            if (_totalFrames is > 0)
                return $"{_writtenFrames.ToString("N0", CultureInfo.InvariantCulture)} / {_totalFrames.Value.ToString("N0", CultureInfo.InvariantCulture)}";

            return _writtenFrames > 0 ? _writtenFrames.ToString("N0", CultureInfo.InvariantCulture) : Lang.NotAvailableText;
        }

        private static long GetWorkingSetBytes(Process? process, Dictionary<int, List<int>>? childMap = null)
        {
            return SumProcessTreeValue(process, GetSingleProcessWorkingSetBytes, childMap);
        }

        private static long GetSingleProcessWorkingSetBytes(Process process)
        {
            try
            {
                if (process.HasExited) return 0L;
                process.Refresh();
                return Math.Max(0L, process.WorkingSet64);
            }
            catch
            {
                return 0L;
            }
        }

        private long GetTotalPageFaults(Dictionary<int, List<int>>? childMap = null)
        {
            return SumProcessTreeValue(_upstreamProcess, GetSingleProcessPageFaults, childMap) + SumProcessTreeValue(_encoderProcess, GetSingleProcessPageFaults, childMap);
        }

        private static long GetSingleProcessPageFaults(Process process)
        {
            try
            {
                if (process.HasExited) return 0L;
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

        private static long SumProcessTreeValue(Process? rootProcess, Func<Process, long> selector, Dictionary<int, List<int>>? childMap = null)
        {
            if (rootProcess == null) return 0L;

            try
            {
                if (rootProcess.HasExited) return 0L;
                int rootProcessId = rootProcess.Id;
                HashSet<int> processIds = GetProcessTreeIds(rootProcessId, childMap);
                processIds.Add(rootProcessId);

                long total = 0L;
                foreach (int processId in processIds)
                {
                    try
                    {
                        using Process process = Process.GetProcessById(processId);
                        total += Math.Max(0L, selector(process));
                    }
                    catch
                    {
                    }
                }

                return total;
            }
            catch
            {
                return 0L;
            }
        }

        private static HashSet<int> GetProcessTreeIds(int rootProcessId, Dictionary<int, List<int>>? childMap = null)
        {
            Dictionary<int, List<int>> childIdsByParentId = childMap ?? GetChildProcessMap();
            HashSet<int> processIds = [];
            Queue<int> pending = new();
            pending.Enqueue(rootProcessId);

            while (pending.Count > 0)
            {
                int processId = pending.Dequeue();
                if (!processIds.Add(processId)) continue;
                if (!childIdsByParentId.TryGetValue(processId, out List<int>? childIds)) continue;

                foreach (int childId in childIds)
                    pending.Enqueue(childId);
            }

            return processIds;
        }

        private static Dictionary<int, List<int>> GetChildProcessMap()
        {
            Dictionary<int, List<int>> childIdsByParentId = [];
            if (!OperatingSystem.IsWindows()) return childIdsByParentId;

            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (snapshot == InvalidHandleValue) return childIdsByParentId;

            try
            {
                PROCESSENTRY32 entry = new()
                {
                    dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>()
                };

                if (!Process32First(snapshot, ref entry)) return childIdsByParentId;

                do
                {
                    int parentProcessId = unchecked((int)entry.th32ParentProcessID);
                    int processId = unchecked((int)entry.th32ProcessID);
                    if (!childIdsByParentId.TryGetValue(parentProcessId, out List<int>? childIds))
                    {
                        childIds = [];
                        childIdsByParentId[parentProcessId] = childIds;
                    }

                    childIds.Add(processId);
                    entry.dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>();
                }
                while (Process32Next(snapshot, ref entry));

                return childIdsByParentId;
            }
            finally
            {
                CloseHandle(snapshot);
            }
        }

        private static string FormatGb(long bytes)
        {
            return $"{Math.Max(0d, bytes / (double)BytesPerGb):0.0} GB";
        }

        private static string FormatGbValue(long bytes)
        {
            return Math.Max(0d, bytes / (double)BytesPerGb).ToString("0.0", CultureInfo.InvariantCulture);
        }

        private static string FormatMb(long bytes)
        {
            return $"{Math.Max(0d, bytes / (double)BytesPerMb):N0} MB";
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

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [LibraryImport("kernel32.dll", EntryPoint = "Process32FirstW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [LibraryImport("kernel32.dll", EntryPoint = "Process32NextW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [LibraryImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(IntPtr hObject);

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private unsafe struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            public fixed char szExeFile[260];
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
            _upstreamWorkingSetPeakBytes = 0;
            _encoderWorkingSetPeakBytes = 0;
            SetCurrentOutputSizeBytes(TryGetOutputSizeBytes());
            _writtenFrames = 0;
            OnPropertyChanged(nameof(WrittenFramesLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));
            ProgressValue = 0;
            StatusText = Lang.ResetUsageStatusText;
        }

        private void TryInterruptUpstream()
        {
            _userInterruptRequested = true;
            FinishButtons.B5_3IsEnabled = false;
            StatusText = Lang.InterruptingUpstreamText;

            Task.Run(() =>
            {
                try
                {
                    TryCloseMainWindow(_upstreamProcess);
                    TryCloseStream(_upstreamStdoutStream);
                }
                catch (Exception ex)
                {
                    EnqueueProcessLine(ProcessLogKind.UpstreamStderr, ex.Message);
                }
            });
        }

        private void TryInterruptEncoder()
        {
            _userInterruptRequested = true;
            FinishButtons.B5_3IsEnabled = false;
            FinishButtons.B5_4IsEnabled = false;
            StatusText = Lang.InterruptingEncoderText;

            Task.Run(() =>
            {
                try
                {
                    TryCloseStream(_encoderStdinStream);
                    TryCloseMainWindow(_encoderProcess);
                    TryCloseStream(_upstreamStdoutStream);
                    TryCloseMainWindow(_upstreamProcess);
                }
                catch (Exception ex)
                {
                    EnqueueProcessLine(ProcessLogKind.DownstreamStderr, ex.Message);
                }
            });
        }

        private static void TryCloseMainWindow(Process? process)
        {
            try
            {
                if (process is { HasExited: false })
                    process.CloseMainWindow();
            }
            catch
            {
            }
        }

        private static void TryCloseStream(Stream? stream)
        {
            try
            {
                stream?.Close();
            }
            catch
            {
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
            _cpuSetsLang = new CpuSetsLangProviderM(UILangProviderM.Current.LanguageCode);
            SampleIntervalTickLabels.Clear();
            foreach (string label in Lang.SampleIntervalTickLabels)
                SampleIntervalTickLabels.Add(label);
            FreezeOrContinueText = _isFrozen ? Lang.ContinueMonitoringText : Lang.FreezeContinueText;
            StatusText = Lang.ReadyToStartText;
        }

        private void RefreshLanguageBindings()
        {
            Lang = new EncodingMonitorModalLangProviderM(UILangProviderM.Current.LanguageCode);
            _cpuSetsLang = new CpuSetsLangProviderM(UILangProviderM.Current.LanguageCode);
            FreezeOrContinueText = _isFrozen ? Lang.ContinueMonitoringText : Lang.FreezeContinueText;
            MonitorButtons.B2_1Text = FreezeOrContinueText;
            MonitorButtons.B2_2Text = Lang.ResetUsageText;
            ReportButtons.B3_1Text = Lang.SaveUpstreamStderrText;
            ReportButtons.B3_2Text = Lang.SaveDownstreamStderrText;
            ReportButtons.B3_3Text = Lang.RotateLogFontSizeText;
            FinishButtons.B5_1Text = Lang.OpenOutputDirectoryText;
            FinishButtons.B5_2Text = Lang.ViewEncodingCommandText;
            FinishButtons.B5_3Text = Lang.InterruptUpstreamText;
            FinishButtons.B5_4Text = Lang.InterruptEncoderText;
            FinishButtons.B5_5Text = Lang.CloseAfterDoneText;

            if (FooterColumns.Count == 6)
            {
                FooterColumns[0].TopText = StartedAtLabel;
                FooterColumns[1].TopText = ElapsedLabel;
                FooterColumns[2].TopText = RemainingLabel;
                FooterColumns[3].TopText = CompleteAtLabel;
                FooterColumns[4].TopText = RateControlLabel;
                FooterColumns[5].TopText = ArgsLabel;
            }

            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(ProgressTitle));
            OnPropertyChanged(nameof(MemoryTitle));
            OnPropertyChanged(nameof(DragLogReportHint));
            OnPropertyChanged(nameof(CurrentSizeLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));
            OnPropertyChanged(nameof(WrittenFramesLabel));
            OnPropertyChanged(nameof(SampleIntervalLabel));
            OnPropertyChanged(nameof(StartedAtLabel));
            OnPropertyChanged(nameof(ElapsedLabel));
            OnPropertyChanged(nameof(RemainingLabel));
            OnPropertyChanged(nameof(CompleteAtLabel));
            OnPropertyChanged(nameof(ArgsLabel));
            OnPropertyChanged(nameof(SmallNoteText));
            OnPropertyChanged(nameof(DistributionUpstreamLabel));
            OnPropertyChanged(nameof(DistributionDownstreamLabel));
            OnPropertyChanged(nameof(DistributionCacheLabel));
            OnPropertyChanged(nameof(DistributionAvailableLabel));
            OnPropertyChanged(nameof(MemoryRangeLegendTitle));

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

        private readonly record struct ProcessLogEntry(ProcessLogKind Kind, string Line, bool OverwritesPreviousLine);

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
