using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Pipeline;
using OneColumnEncoder.CPU;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.Views;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace OneColumnEncoder.ViewModels
{
    public partial class EncodingMonitorVM : BaseVM
    {
        private const int MemoryRangeBlockCount = 128;
        private const int MemoryRangeMaxFillLevel = 8;
        private const int UpstreamShutdownAfterEncoderExitDelayMs = 5000;
        private const int UpstreamKillAfterShutdownTimeoutMs = 1000;
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
        private EncodingPipelineRequest _request;
        private EncodingPipelineCommand _command;
        private readonly bool _isSample;
        private readonly Stopwatch _stopwatch = new();
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
        private readonly StringBuilder _upstreamStderrBuilder = new();
        private readonly StringBuilder _downstreamStderrBuilder = new();
        private LogFoldState _upstreamStderrFoldState = new();
        private LogFoldState _downstreamStderrFoldState = new();
        private readonly ConcurrentQueue<ProcessLogEntry> _logQueue = new();
        private readonly Dictionary<string, EncodingLogSnapshot> _logSnapshotsByJobId = [];
        private long? _totalFrames;
        private readonly IReadOnlyList<(EncodingPipelineRequest Request, EncodingPipelineCommand Command)>? _queueItems;
        private string? _activeLogJobId;
        private QueueJobItemVM? _activeJobVM;
        private readonly Lock _logLock = new();
        private DateTime _lastStatsUpdate = DateTime.MinValue;
        private DateTime _lastMemoryStatsUpdate = DateTime.MinValue;
        private CancellationTokenSource? _cts;
        private Process? _upstreamProcess;
        private Process? _encoderProcess;
        private Process? _muxProcess;
        private bool _hasStarted;
        private bool _finishEnabledAfterClose;
        private bool _isWindowCloseEnabled;
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
        private bool _cancelAllRequested;
        private bool _upstreamInterruptButtonClicked;
        private bool _encoderInterruptButtonClicked;
        private Stream? _upstreamStdoutStream;
        private Stream? _encoderStdinStream;

        public string WindowTitle => _isSample ? EncodingMonitorModalLangProviderM.WindowTitleSampleMode : EncodingMonitorModalLangProviderM.WindowTitle;
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
        public string EnableMuxText => Lang.EnableMuxText;
        public string RichTextModeText => Lang.RichTextModeText;
        public string MuxTimebaseHint => Lang.MuxTimebaseHint;
        public string OpusAudioCommandHint => BuildOpusAudioCommandHint();
        public string OpusAudioBitrateHint => Lang.OpusAudioBitrateHint;
        public bool CanMux => !_isSample && _command.MuxCommand != null;
        public bool IsWindowCloseEnabled
        {
            get => _isWindowCloseEnabled;
            private set => SetProperty(ref _isWindowCloseEnabled, value);
        }
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
        public ActionCmd CancelAllQueueCommand { get; }
        public ActionCmd FreezeOrContinueCmd { get; }
        public ActionCmd ResetStatsCmd { get; }
        public CloseModalCmd CloseCmd { get; }
        public QueueSidebarVM QueueSidebar { get; }
        public bool IsCancelAllEnabled => _hasStarted && !_finishEnabledAfterClose && !_cancelAllRequested;

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                double next = Math.Clamp(value, 0d, 100d);
                if (Math.Abs(_progressValue - next) < 0.005d) return;
                _progressValue = next;
                if (_activeJobVM != null)
                    _activeJobVM.ProgressPercent = (int)Math.Round(next);
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(EstimatedSizeLabel));
            }
        }

        public string ProgressText => ProgressValue is > 0d and < 1d
            ? $"{ProgressValue:F2}%"
            : $"{ProgressValue:F0}%";

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

        private bool _enableMux;
        public bool EnableMux
        {
            get => _enableMux;
            set => SetProperty(ref _enableMux, value && CanMux);
        }

        private bool _isMonitoringEnabled = true;
        public bool IsMonitoringEnabled
        {
            get => _isMonitoringEnabled;
            set
            {
                if (!SetProperty(ref _isMonitoringEnabled, value)) return;
                MonitorButtons.B2_1IsEnabled = value;
                MonitorButtons.B2_2IsEnabled = value;
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

        /// <summary>
        /// Constructs the encoding monitor view model.
        /// Sets up commands, button groups, UI collections, and subscribes to language change events.
        /// Does NOT start encoding — call <see cref="Start"/> to begin.
        /// </summary>
        public EncodingMonitorVM(
            ModalNavS modalNavS,
            Action closeAction,
            EncodingPipelineRequest request,
            EncodingPipelineCommand command,
            bool isSample,
            bool enableQueueSidebar = false)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            _request = request;
            _command = command;
            _isSample = isSample;
            _totalFrames = EncodingPipeline.GetSourceTotalFrames(_request.SourceFfprobeJson, _request.ConcatTotalFrames);
            _enableMux = CanMux && !string.Equals(_request.EncoderExeName, "x264.exe", StringComparison.OrdinalIgnoreCase);

            RefreshLanguageState();

            FreezeOrContinueCmd = new ActionCmd(_ => IsFrozen = !IsFrozen);
            ResetStatsCmd = new ActionCmd(_ => ResetStats());
            CancelAllQueueCommand = new ActionCmd(_ => CancelAllQueue());
            CloseCmd = new CloseModalCmd(() =>
            {
                if (!_finishEnabledAfterClose) return;
                _closeAction();
            });

            MonitorButtons = ButtonGroupVM.CreateTwoButton(
                FreezeOrContinueText,
                Lang.UpdateUsageText,
                FreezeOrContinueCmd,
                ResetStatsCmd);

            ReportButtons = ButtonGroupVM.CreateThreeButton(
                Lang.SaveUpstreamStderrText, Lang.SaveDownstreamStderrText, Lang.RotateLogFontSizeText,
                new ActionCmd(_ => SaveTextAndShowPath(UpstreamReportText, "upstream-stderr.txt")),
                new ActionCmd(_ => SaveTextAndShowPath(DownstreamReportText, "downstream-stderr.txt")),
                new ActionCmd(_ => RotateLogFontSize()));

            FinishButtons = ButtonGroupVM.CreateFiveButton(
                Lang.OpenOutputDirectoryText, Lang.ViewEncodingCommandText, Lang.InterruptUpstreamText, Lang.InterruptEncoderText, Lang.CloseAfterDoneText,
                new ActionCmd(_ => OpenOutputDirectory()),
                new ActionCmd(_ => ShowEncodingCommand()),
                new ActionCmd(_ => TryInterruptUpstream()),
                new ActionCmd(_ => TryInterruptEncoder()),
                CloseCmd);
            FinishButtons.B5_5IsEnabled = false;

            QueueSidebar = new QueueSidebarVM(enableQueueSidebar);
            QueueSidebar.PropertyChanged += OnQueueSidebarPropertyChanged;
            if (!enableQueueSidebar)
            {
                QueueSidebar.AddJob(CreateSidebarJob(_request, _command, "Pending"));
            }
            BuildMetrics();
            BuildFooter();
            BuildMemoryRangeBlocks();
            _timer.Tick += OnTimerTick;
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        /// <summary>
        /// Constructs the encoding monitor view model for queue (batch) mode.
        /// Accepts a list of pipeline request/command pairs and processes them sequentially.
        /// </summary>
        public EncodingMonitorVM(
            ModalNavS modalNavS,
            Action closeAction,
            IReadOnlyList<(EncodingPipelineRequest Request, EncodingPipelineCommand Command)> queueItems)
            : this(modalNavS, closeAction, queueItems[0].Request, queueItems[0].Command, false, true)
        {
            _queueItems = queueItems;
        }

        private double _logFontSize = 11;
        public double LogFontSize
        {
            get => _logFontSize;
            set => SetProperty(ref _logFontSize, value);
        }

        private bool _logRichTextMode = true;
        public bool LogRichTextMode
        {
            get => _logRichTextMode;
            set => SetProperty(ref _logRichTextMode, value);
        }

        /// <summary>
        /// Starts the encoding pipeline. Can only be called once.
        /// Launches the async encoding task (fire-and-forget) and starts the UI timer.
        /// </summary>
        public void Start()
        {
            if (_hasStarted) return;
            _hasStarted = true;
            _cts = new CancellationTokenSource();
            _stopwatch.Start();
            _timer.Start();
            RefreshCancelAllBindings();
            if (_queueItems != null)
                _ = RunQueueEncodingAsync(_cts.Token);
            else
                _ = RunSingleEncodingAsync(_cts.Token);
        }

        private async Task RunSingleEncodingAsync(CancellationToken cancellationToken)
        {
            QueueJobItemVM? jobVM = QueueSidebar.SelectedJob ?? (QueueSidebar.WaitingJobs.Count > 0 ? QueueSidebar.WaitingJobs[0] : null);
            if (jobVM != null)
            {
                _activeLogJobId = jobVM.JobId;
                _activeJobVM = jobVM;
                ResetActiveEncodingState();
                ResetActiveLogState(jobVM.JobId);
                QueueSidebar.MarkJobEncoding(jobVM);
            }

            await RunEncodingAsync(cancellationToken);

            if (jobVM == null) return;
            if (_userInterruptRequested)
                QueueSidebar.MarkJobInterrupted(jobVM);
            else if (_success)
                QueueSidebar.MarkJobCompleted(jobVM);
            else
                QueueSidebar.MarkJobFailed(jobVM, StatusText);
        }

        private QueueJobItemM CreateSidebarJob(EncodingPipelineRequest request, EncodingPipelineCommand command, string status)
        {
            return new QueueJobItemM
            {
                JobId = Guid.NewGuid().ToString(),
                SourcePath = request.UpstreamInputPath,
                OutputPath = request.OutputPath,
                Status = status,
                EncoderExeName = request.EncoderExeName,
                SerializedRequest = JsonSerializer.Serialize(request),
                SerializedCommand = JsonSerializer.Serialize(command),
                QueuedAt = DateTime.Now
            };
        }

        private void ResetActiveLogState(string jobId)
        {
            _upstreamStderrBuilder.Clear();
            _downstreamStderrBuilder.Clear();
            _upstreamStderrFoldState = new LogFoldState();
            _downstreamStderrFoldState = new LogFoldState();
            _logQueue.Clear();
            _logSnapshotsByJobId[jobId] = new EncodingLogSnapshot(string.Empty, string.Empty);
            SetDisplayedLogs(string.Empty, string.Empty);
        }

        private void UpdateActiveLogSnapshot()
        {
            if (_activeLogJobId == null) return;
            _logSnapshotsByJobId[_activeLogJobId] = new EncodingLogSnapshot(
                _upstreamStderrBuilder.ToString(),
                _downstreamStderrBuilder.ToString());
        }

        private bool IsActiveLogSelected()
        {
            QueueJobItemVM? selectedJob = QueueSidebar.SelectedJob;
            return selectedJob == null || selectedJob.JobId == _activeLogJobId;
        }

        private void SetDisplayedLogs(string upstreamText, string downstreamText)
        {
            UpstreamReportText = upstreamText;
            DownstreamReportText = downstreamText;
        }

        private void OnQueueSidebarPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(QueueSidebarVM.SelectedJob)) return;

            lock (_logLock)
            {
                QueueJobItemVM? selectedJob = QueueSidebar.SelectedJob;
                if (selectedJob == null) return;
                if (selectedJob.JobId == _activeLogJobId)
                {
                    SetDisplayedLogs(_upstreamStderrBuilder.ToString(), _downstreamStderrBuilder.ToString());
                }
                else if (_logSnapshotsByJobId.TryGetValue(selectedJob.JobId, out EncodingLogSnapshot snapshot))
                    SetDisplayedLogs(snapshot.UpstreamText, snapshot.DownstreamText);
                else
                    SetDisplayedLogs(string.Empty, string.Empty);
            }

            OnPropertyChanged(nameof(OpusAudioCommandHint));
        }

        /// <summary>
        /// Initializes the 6-column metrics display with placeholder values.
        /// Columns: Physical Memory, Committed Memory, Working Set Peak, Page File, Page Faults, RAM Stress.
        /// </summary>
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

        /// <summary>
        /// Initializes the 6-column footer with start time, elapsed, remaining, completion, rate control, and preset.
        /// </summary>
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

        /// <summary>
        /// Creates 128 memory range blocks, each representing a slice of physical memory.
        /// Initially empty; filled by UpdateMemoryRangeBlocks during timer ticks.
        /// </summary>
        private void BuildMemoryRangeBlocks()
        {
            MemoryRangeBlocks.Clear();
            for (int i = 0; i < MemoryRangeBlockCount; i++)
            {
                MemoryRangeBlocks.Add(new MemoryRangeBlockM { FillLevel = 0, Tooltip = string.Format(Lang.BlockTooltipFormat, i) });
            }
        }

        /// <summary>
        /// Orchestrates the full encoding pipeline: upstream decode -> encode -> optional mux.
        /// Spawns processes, pipes data between them, collects logs, and handles cancellation/errors.
        /// </summary>
        private async Task RunEncodingAsync(CancellationToken cancellationToken)
        {
            _success = false;

            try
            {
                StatusText = Lang.EncodingText;
                // Create the upstream decoder process (e.g. ffmpeg source)
                using Process upstream = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _request.UpstreamPath,
                        Arguments = _command.UpstreamArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
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
                        RedirectStandardError = true,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    },
                    EnableRaisingEvents = true
                };

                _upstreamProcess = upstream;
                _encoderProcess = encoder;

                upstream.Start();
                ApplyParallelismSettings(upstream, isEncoder: false);
                encoder.Start();
                ApplyParallelismSettings(encoder, isEncoder: true);

                // Record PIDs for wait-chain monitoring
                if (_activeJobVM != null)
                {
                    _activeJobVM.UpstreamPid = upstream.Id;
                    _activeJobVM.EncoderPid = encoder.Id;
                }

                // Use TaskCompletionSource backed by Process.Exited for robust exit detection
                TaskCompletionSource upstreamExited = new();
                TaskCompletionSource encoderExited = new();
                upstream.Exited += (_, _) => upstreamExited.TrySetResult();
                encoder.Exited += (_, _) => encoderExited.TrySetResult();
                if (upstream.HasExited) upstreamExited.TrySetResult();
                if (encoder.HasExited) encoderExited.TrySetResult();

                // Pipe upstream stdout -> encoder stdin (raw byte transfer, 80 KB buffer).
                // Closing encoder stdin signals EOF so the encoder can flush and finish.
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
                    finally { TryCloseStream(encoderStdin); }
                }, cancellationToken);

                // Concurrently read stderr from both processes for log/progress parsing
                Task upstreamStderrTask = ReadStreamAsync(
                    upstream.StandardError, ProcessLogKind.UpstreamStderr, cancellationToken);
                Task encoderStderrTask = ReadStreamAsync(
                    encoder.StandardError, ProcessLogKind.DownstreamStderr, cancellationToken);
                Task upstreamShutdownTask = StopUpstreamAfterEncoderExitAsync(encoderExited.Task, upstream, cancellationToken);

                // Wait for data transfer to finish, then ensure processes have exited
                await Task.WhenAll(pipeTask, upstreamStderrTask, encoderStderrTask, upstreamShutdownTask);

                // Close encoder stdin if still open to signal EOF (safety net)
                TryCloseStream(_encoderStdinStream);

                // Wait for process exit with a timeout (after streams closed, should exit quickly)
                const int processExitTimeoutMs = 15000;
                if (await Task.WhenAny(upstreamExited.Task, Task.Delay(processExitTimeoutMs, cancellationToken)) != upstreamExited.Task)
                {
                    TryCloseMainWindow(upstream);
                    if (!upstream.HasExited) TryKillProcess(upstream);
                    await upstreamExited.Task; // ensure we wait until truly dead
                }
                if (await Task.WhenAny(encoderExited.Task, Task.Delay(processExitTimeoutMs, cancellationToken)) != encoderExited.Task)
                {
                    TryCloseMainWindow(encoder);
                    if (!encoder.HasExited) TryKillProcess(encoder);
                    await encoderExited.Task;
                }

                _exitCode = encoder.ExitCode;
                _success = _exitCode == 0;
                if (_success)
                    _success = await RunMuxAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                StatusText = Lang.InterruptedText;
            }
            catch (Exception ex)
            {
                EnqueueProcessLine(ProcessLogKind.DownstreamStderr, ex.ToString());
                StatusText = Lang.FailedText;
            }
            finally
            {
                if (_queueItems == null)
                {
                    _stopwatch.Stop();
                    _timer.Stop();
                    EnableCloseButton();
                }

                ProgressValue = _success ? 100 : ProgressValue;
                UpdateProgressDetails();
                if (_queueItems == null)
                    StatusText = _success
                        ? Lang.CompletedText
                        : _userInterruptRequested
                            ? Lang.InterruptedText
                            : StatusText == Lang.EncodingText || StatusText == Lang.MuxingText ? Lang.FailedText : StatusText;
                FlushLogsToProperties();
                UpdateFooterTimes(final: _queueItems == null);
                IsMonitoringEnabled = false;
                _upstreamStdoutStream = null;
                _encoderStdinStream = null;
            }
        }

        /// <summary>
        /// Processes all queue items sequentially. Populates the sidebar, marks each job
        /// as Encoding while it runs, then Completed or Failed. Stops on first failure.
        /// Timer and stopwatch accumulate across the entire batch.
        /// </summary>
        private async Task RunQueueEncodingAsync(CancellationToken cancellationToken)
        {
            int total = _queueItems!.Count;
            int completed = 0;

            QueueSidebar.ClearAllJobs();
            QueueSidebar.IsVisible = true;

            for (int i = 0; i < total; i++)
            {
                var (request, command) = _queueItems[i];
                QueueSidebar.AddJob(CreateSidebarJob(request, command, "Pending"));
            }
            QueueSidebar.SaveToDisk();

            foreach (QueueJobItemVM job in QueueSidebar.WaitingJobs)
            {
                job.R1Command = new ActionCmd(_ => QueueSidebar.RemoveJob(job));
                job.R2Command = new ActionCmd(_ => QueueSidebar.MoveJobUp(job));
                job.R3Command = new ActionCmd(_ => QueueSidebar.MoveJobDown(job));
            }

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var jobVM = QueueSidebar.GetNextPending();
                if (jobVM == null) break;
                EncodingPipelineRequest? request = jobVM.Request;
                EncodingPipelineCommand? command = jobVM.Command;
                if (request == null || command == null)
                {
                    QueueSidebar.MarkJobFailed(jobVM, "Failed to load queued encoding request.");
                    break;
                }

                _request = request;
                _command = command;
                _totalFrames = EncodingPipeline.GetSourceTotalFrames(request.SourceFfprobeJson, request.ConcatTotalFrames);
                OnPropertyChanged(nameof(OpusAudioCommandHint));
                EnableMux = command.MuxCommand != null
                    && !string.Equals(request.EncoderExeName, "x264.exe", StringComparison.OrdinalIgnoreCase);
                _writtenFrames = 0;
                _currentOutputSizeBytes = 0;
                _upstreamWorkingSetPeakBytes = 0;
                _encoderWorkingSetPeakBytes = 0;
                _success = false;
                _userInterruptRequested = false;
                _upstreamProcess = null;
                _encoderProcess = null;
                _muxProcess = null;

                _activeLogJobId = jobVM.JobId;
                _activeJobVM = jobVM;
                ResetActiveEncodingState();
                ResetActiveLogState(jobVM.JobId);
                QueueSidebar.MarkJobEncoding(jobVM);
                IsMonitoringEnabled = true;

                await RunEncodingAsync(cancellationToken);

                if (_userInterruptRequested)
                {
                    QueueSidebar.MarkJobInterrupted(jobVM);
                    if (_cancelAllRequested || AskStopQueueConfirmation())
                        break;
                }
                else if (_success)
                {
                    QueueSidebar.MarkJobCompleted(jobVM);
                    completed++;
                }
                else
                {
                    QueueSidebar.MarkJobFailed(jobVM, StatusText);
                    break;
                }
            }

            _stopwatch.Stop();
            _timer.Stop();
            EnableCloseButton();
            UpdateFooterTimes(final: true);
            int currentTotal = QueueSidebar.TotalCount;
            StatusText = _cancelAllRequested
                ? $"{Lang.InterruptedText}: {completed}/{currentTotal}"
                : completed == currentTotal
                ? $"{Lang.CompletedText}: {completed}/{currentTotal}"
                : $"{Lang.FailedText}: {completed}/{currentTotal}";
        }

        private void ResetActiveEncodingState()
        {
            ProgressValue = 0;
            SetCurrentOutputSizeBytes(0);
            _writtenFrames = 0;
            _lastUpstreamWorkingSetBytes = 0;
            _lastEncoderWorkingSetBytes = 0;
            _upstreamWorkingSetPeakBytes = 0;
            _encoderWorkingSetPeakBytes = 0;
            OnPropertyChanged(nameof(WrittenFramesLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));

            if (FooterColumns.Count == 6)
            {
                FooterColumns[4].MainText = GetRateControlText();
                FooterColumns[5].MainText = GetPresetText();
            }
        }

        /// <summary>
        /// Runs the optional mux step (e.g. ffmpeg muxing encoded video + audio).
        /// Returns true on success or if muxing is skipped; false on mux failure.
        /// </summary>
        private async Task<bool> RunMuxAsync(CancellationToken cancellationToken)
        {
            EncodingMuxCommand? muxCommand = _command.MuxCommand;
            if (!EnableMux || muxCommand == null) return true;

            if (!File.Exists(muxCommand.EncodedVideoPath))
            {
                EnqueueProcessLine(
                    ProcessLogKind.UpstreamStderr,
                    "\nMux failed: encoded video stream does not exist: " + muxCommand.EncodedVideoPath);
                return false;
            }

            StatusText = Lang.MuxingText;
            EnqueueProcessLine(
                ProcessLogKind.UpstreamStderr,
                "\nMux command: " + muxCommand.CommandLine);

            using Process mux = new()
            {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _request.FfmpegPath,
                        Arguments = muxCommand.Arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    },
                EnableRaisingEvents = true
            };

            _muxProcess = mux;
            mux.Start();
            // Show mux stderr in upstream log since its more intuitive
            Task muxStderrTask = ReadStreamAsync(mux.StandardError, ProcessLogKind.UpstreamStderr, cancellationToken);
            Task muxStdoutTask = ReadStreamAsync(mux.StandardOutput, ProcessLogKind.UpstreamStderr, cancellationToken);
            await mux.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(muxStderrTask, muxStdoutTask);
            _exitCode = mux.ExitCode;
            _muxProcess = null;

            if (mux.ExitCode == 0) return true;
            EnqueueProcessLine(ProcessLogKind.UpstreamStderr, $"Mux failed with exit code {mux.ExitCode}. See ffmpeg output above for details.");
            return false;
        }

        /// <summary>
        /// Applies CPU affinity and thread count settings to a process based on parallelism config.
        /// Uses NUMA node, physical-core preference, and optional thread count limit.
        /// Logs the result (success/skip) to the appropriate stderr stream.
        /// </summary>
        private void ApplyParallelismSettings(Process process, bool isEncoder)
        {
            ParallelismConfM? parallelismConf = _request.ParallelismConf;
            if (parallelismConf == null) return;

            int nodeId = isEncoder ? parallelismConf.DownstreamNodeId : parallelismConf.UpstreamNodeId;
            bool physicalOnly = isEncoder ? parallelismConf.PreferPhysicalCores : parallelismConf.PreferUpstreamPhysicalCores;
            int? maxCpuSets = isEncoder ? parallelismConf.EncoderThreadCount : null;
            ProcessLogKind logKind = isEncoder ? ProcessLogKind.DownstreamStderr : ProcessLogKind.UpstreamStderr;

            bool success = CpuSets.TryApplyProcessDefaultCpuSets(
                process,
                nodeId,
                physicalOnly,
                maxCpuSets,
                _cpuSetsLang,
                out string message);
            EnqueueProcessLine(logKind, $"Parallelism: {(success ? message : _cpuSetsLang.SkippedPrefix + message)}");
        }

        /// <summary>
        /// Reads a process stream character-by-character, splitting on \r and \n.
        /// Handles three cases:
        ///   1. "\r\n" → newline ends the previous line; pending CR is discarded.
        ///   2. "\r" alone → the line is an overwrite (e.g. ffmpeg progress); marks overwritesPreviousLine.
        ///   3. "\n" alone → standard newline, enqueued as a normal line.
        /// This lets us display both regular log output and in-place status updates correctly.
        /// </summary>
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
                            // \r\n sequence: the CR ended the previous line, \n is the real newline
                            if (ch == '\n')
                            {
                                EnqueueProcessLine(kind, pendingCarriageReturnLine, overwritesPreviousLine: false);
                                pendingCarriageReturnLine = null;
                                previousWasCarriageReturnUpdate = false;
                                continue;
                            }

                            // Standalone \r: the pending line was an in-place update, emit it as overwrite
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
                UpdateActiveLogSnapshot();
                if (IsActiveLogSelected())
                    SetDisplayedLogs(_upstreamStderrBuilder.ToString(), _downstreamStderrBuilder.ToString());
            }
        }

        /// <summary>
        /// Timer callback (fires every 500ms). Drains log queue, updates progress/footer
        /// every 1 second, and samples memory at the configured interval.
        /// Skips all UI updates when frozen to reduce CPU usage.
        /// </summary>
        private void OnTimerTick(object? sender, EventArgs e)
        {
            ProcessQueuedLogs();
            if (!IsFrozen)
            {
                FlushLogsToProperties();
                DateTime now = DateTime.Now;
                // Update progress and footer times once per second
                if ((now - _lastStatsUpdate).TotalSeconds >= 1d)
                {
                    _lastStatsUpdate = now;
                    UpdateProgressDetails();
                    UpdateFooterTimes(final: false);
                }

                // Sample memory usage at user-configured intervals
                if (IsMemorySampleDue(now))
                {
                    _lastMemoryStatsUpdate = now;
                    UpdateMetrics();
                    UpdateMemoryRangeBlocks();
                }
            }
        }

        /// <summary>
        /// Drains the concurrent log queue into the upstream/downstream StringBuilder buffers.
        /// Duplicate lines are folded with a (xN) repeat count to keep logs compact.
        /// Only flushes to bound properties if not frozen (UI update optimization).
        /// </summary>
        private void ProcessQueuedLogs()
        {
            bool changed = false;
            while (_logQueue.TryDequeue(out ProcessLogEntry entry))
            {
                changed = true;
                switch (entry.Kind)
                {
                    case ProcessLogKind.UpstreamStderr:
                        // Upstream log: progress parsing disabled (downstream drives main progress)
                        AppendLogWithOverwrite(
                            _upstreamStderrBuilder,
                            _upstreamStderrFoldState,
                            entry.Line,
                            entry.OverwritesPreviousLine,
                            updateMainProgress: false);
                        break;
                    case ProcessLogKind.DownstreamStderr:
                        // Downstream log: progress parsing enabled (encoder drives main progress)
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
                    UpdateActiveLogSnapshot();
                    if (IsActiveLogSelected())
                        SetDisplayedLogs(_upstreamStderrBuilder.ToString(), _downstreamStderrBuilder.ToString());
                }
            }
            else if (changed)
            {
                lock (_logLock)
                    UpdateActiveLogSnapshot();
            }
        }

        /// <summary>
        /// Appends a single log line to the target StringBuilder.
        /// Strips nulls and trailing whitespace. If overwritesPreviousLine is true,
        /// the last line is removed first (for \r-based progress updates).
        /// Duplicate lines are folded into a (xN) repeat count.
        /// </summary>
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

        /// <summary>
        /// Adds a line to the folded log. If the line already exists, increments its repeat count
        /// and rebuilds the full log text. Otherwise appends it as a new unique line.
        /// </summary>
        private static void AppendFoldedLine(StringBuilder target, LogFoldState foldState, string line)
        {
            if (foldState.LineIndexByText.TryGetValue(line, out int index))
            {
                // Existing line: increment count and rebuild the display text
                LogFoldEntry entry = foldState.Entries[index];
                foldState.Entries[index] = entry with { RepeatCount = entry.RepeatCount + 1 };
                RebuildFoldedLog(target, foldState);
                return;
            }

            // New unique line: add to index and append directly
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

        /// <summary>
        /// Extracts progress info from a single log line.
        /// For downstream lines: parses percentage progress and frame counts to update ProgressValue.
        /// Also extracts written frame count for estimated size calculations.
        /// </summary>
        private void UpdateProgressFromLogLine(string line, bool updateMainProgress)
        {
            string trimmed = NormalizeLogLineForProgress(line);
            if (string.IsNullOrWhiteSpace(trimmed) || IsIndexProgressLine(trimmed)) return;

            bool isProgressLine = IsProgressLine(trimmed);
            // Parse percentage-based progress (e.g. "56.3%") from log text
            if (isProgressLine && updateMainProgress)
                ProgressValue = InferProgress(ProgressValue, trimmed);
            // Try to extract frame count for frame-based progress and output size estimation
            if (TryParseEncoderFrame(trimmed) is int frame)
            {
                UpdateWrittenFrames(frame);
                if (updateMainProgress && isProgressLine && _totalFrames is > 0)
                {
                    double frameProgress = Math.Min(100d, frame * 100d / _totalFrames.Value);
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

        [GeneratedRegex(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.CultureInvariant)]
        private static partial Regex AnsiEscapeRegex();

        private static string NormalizeLogLineForProgress(string line)
        {
            return AnsiEscapeRegex().Replace(line, string.Empty).Trim();
        }

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

        [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s+@", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex FramesAtRegex();

        [GeneratedRegex(@"\bencoding\s+frame\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex EncodingFrameRegex();

        [GeneratedRegex(@"(?:^|\D)encoded\s+(\d+)\s+frames?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex EncodedFrameRegex();

        [GeneratedRegex(@"(?<!\d)(\d+)\s+frames?\s+encoded", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex FramesEncodedRegex();

        /// <summary>
        /// Attempts to extract a frame number from a log line using multiple regex patterns.
        /// Supports ffmpeg "frame= 1234", x264 "1234 frames:", "1234/5000 frames",
        /// "encoding frame 1234", "encoded 1234 frames", "1234 frames encoded".
        /// </summary>
        private static int? TryParseEncoderFrame(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            line = NormalizeLogLineForProgress(line);

            if (TryParseFirstRegexGroup(FfmpegFrameRegex().Match(line), out int value)) return value;
            if (TryParseFirstRegexGroup(X264FrameRegex().Match(line), out value)) return value;
            if (TryParseFirstRegexGroup(SlashFrameRegex().Match(line), out value)) return value;
            if (TryParseFirstRegexGroup(FramesAtRegex().Match(line), out value)) return value;
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

        /// <summary>
        /// Removes the last line from the StringBuilder and its associated fold state entry.
        /// Used when a \r-overwrite line needs to replace the previous in-place status line.
        /// </summary>
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

        /// <summary>
        /// Collects system and process memory statistics and updates the UI metrics.
        /// Gathers: physical/committed memory, working set peaks, page faults, RAM stress,
        /// and the memory distribution (upstream vs downstream vs cache vs available).
        /// Uses a child process map to sum working sets across process trees.
        /// </summary>
        private void UpdateMetrics()
        {
            MemoryStatusSnapshot memoryStatus = GetMemoryStatusSnapshot();
            _lastMemoryStatus = memoryStatus;

            // Build parent→child map once, then use it for all process tree queries
            Dictionary<int, List<int>>? childMap = GetChildProcessMap();

            _lastUpstreamWorkingSetBytes = GetWorkingSetBytes(_upstreamProcess, childMap);
            _lastEncoderWorkingSetBytes = GetWorkingSetBytes(_encoderProcess, childMap);
            long combinedWorkingSetBytes = _lastUpstreamWorkingSetBytes + _lastEncoderWorkingSetBytes;
            _upstreamWorkingSetPeakBytes = Math.Max(_upstreamWorkingSetPeakBytes, _lastUpstreamWorkingSetBytes);
            _encoderWorkingSetPeakBytes = Math.Max(_encoderWorkingSetPeakBytes, _lastEncoderWorkingSetBytes);
            long combinedWorkingSetPeakBytes = _upstreamWorkingSetPeakBytes + _encoderWorkingSetPeakBytes;
            long effectiveSystemCacheBytes = GetEffectiveSystemCacheBytes(memoryStatus, combinedWorkingSetBytes);

            // Row 0: Physical memory usage
            MetricColumns[0].MainText = FormatGb(memoryStatus.UsedPhysicalBytes);
            MetricColumns[0].BottomText = ReplaceMetricValue(Lang.PhysicalMemoryBottomText, FormatGb(memoryStatus.TotalPhysicalBytes));
            // Row 1: Committed memory
            MetricColumns[1].MainText = FormatGb(memoryStatus.CommittedBytes);
            MetricColumns[1].BottomText = ReplaceMetricValue(Lang.CommittedMemoryBottomText, FormatGb(memoryStatus.CommitLimitBytes));
            // Row 2: Working set peak (combined upstream + encoder)
            MetricColumns[2].MainText = FormatGb(combinedWorkingSetPeakBytes);
            MetricColumns[2].BottomText = ReplaceMetricValue(Lang.WorkingSetPeakBottomText, FormatGb(combinedWorkingSetBytes));
            // Row 3: Page file usage
            MetricColumns[3].MainText = FormatGb(memoryStatus.CommittedBytes);
            MetricColumns[3].BottomText = ReplaceMetricValue(Lang.PageFileBottomText, FormatGb(memoryStatus.CommitLimitBytes));
            // Row 4: Page faults (sum across upstream + encoder process trees)
            MetricColumns[4].MainText = GetTotalPageFaults(childMap).ToString("N0", CultureInfo.InvariantCulture);
            MetricColumns[4].BottomText = Lang.PageFaultBottomText;
            // Row 5: RAM stress indicator
            MetricColumns[5].MainText = memoryStatus.MemoryLoadPercent < 75 ? Lang.RAMStressMediumText : Lang.RAMStressHighText;
            MetricColumns[5].BottomText = $"{memoryStatus.MemoryLoadPercent}%";

            // Memory distribution values for the range visualization
            DistributionUpstream = FormatMb(_lastUpstreamWorkingSetBytes);
            DistributionDownstream = FormatMb(_lastEncoderWorkingSetBytes);
            DistributionCache = FormatMb(effectiveSystemCacheBytes);
            DistributionAvailable = FormatMb(memoryStatus.AvailablePhysicalBytes);
        }

        /// <summary>
        /// Calculates the portion of system cache that is not part of the encoding processes'
        /// working set. This avoids double-counting cache that belongs to our processes.
        /// </summary>
        private static long GetEffectiveSystemCacheBytes(MemoryStatusSnapshot memoryStatus, long processWorkingSetBytes)
        {
            long nonProcessUsedBytes = Math.Max(0, memoryStatus.UsedPhysicalBytes - processWorkingSetBytes);
            return Math.Min(memoryStatus.SystemCacheBytes, nonProcessUsedBytes);
        }

        /// <summary>
        /// Returns true if enough time has elapsed since the last memory sample
        /// based on the user-configured sample interval.
        /// </summary>
        private bool IsMemorySampleDue(DateTime now)
        {
            int intervalSeconds = SampleIntervalSeconds;
            if (intervalSeconds <= 0) return true;
            return (now - _lastMemoryStatsUpdate).TotalSeconds >= intervalSeconds;
        }

        private void UpdateProgressDetails() =>
            SetCurrentOutputSizeBytes(TryGetOutputSizeBytes());

        private void UpdateMemoryRangeBlocks() =>
            UpdateMemoryRangeBlocks(MemoryRangeBlocks, _lastMemoryStatus);

        /// <summary>
        /// Maps physical memory into a grid of visual blocks (MemoryRangeBlockCount = 128).
        /// Each block represents a byte range of total physical memory. The block's fill level
        /// and color category are determined by which process (upstream/downstream/cache) owns
        /// the memory at that byte offset. The byte range is divided into contiguous categories:
        /// [upstream bytes | downstream bytes | other-used bytes | free].
        /// </summary>
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

            // Calculate how much memory each process owns (may overlap, so we clamp)
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

            // Compute cumulative byte offsets for each category boundary
            int totalBlocks = blocks.Count;
            double bytesPerBlock = totalBytes / (double)totalBlocks;
            long[] categoryEnds = new long[categoryBytes.Length];
            long cumulativeBytes = 0;
            for (int i = 0; i < categoryBytes.Length; i++)
            {
                cumulativeBytes = Math.Min(usedBytes, cumulativeBytes + Math.Max(0, categoryBytes[i]));
                categoryEnds[i] = cumulativeBytes;
            }

            // For each block, compute its byte range and determine which category covers its midpoint
            for (int blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
            {
                double blockStart = blockIndex * bytesPerBlock;
                double blockEnd = blockStart + bytesPerBlock;
                double usedOverlap = Math.Max(0d, Math.Min(blockEnd, usedBytes) - blockStart);
                double fillFraction = bytesPerBlock > 0 ? usedOverlap / bytesPerBlock : 0d;
                MemoryRangeBlockM block = blocks[blockIndex];

                if (fillFraction <= 0d)
                {
                    // Block is in free memory region
                    block.FillLevel = 0;
                    block.Category = MemoryCategory.Empty;
                    block.Tooltip = string.Format(Lang.BlockTooltipFormat, blockIndex)
                        + $" | {Lang.DistributionAvailableLabel} | {FormatRange(blockStart, blockEnd)} | {FormatMb((long)Math.Round(bytesPerBlock))} | 0.0%";
                    continue;
                }

                // Find which category covers the block's midpoint
                int categoryIndex = 0;
                double blockMiddle = blockStart + usedOverlap / 2d;
                while (categoryIndex < categoryEnds.Length - 1 && blockMiddle >= categoryEnds[categoryIndex])
                    categoryIndex++;

                MemoryCategory category = categoryOrder[categoryIndex];
                string categoryName = categoryNames[categoryIndex];
                // Map fill fraction to discrete fill level (1–8)
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

        /// <summary>
        /// Updates the footer elapsed/remaining/completion time display.
        /// Estimates remaining time by linear extrapolation from progress percentage.
        /// </summary>
        private void UpdateFooterTimes(bool final)
        {
            TimeSpan elapsed = _stopwatch.Elapsed;
            FooterColumns[1].MainText = elapsed.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture);
            if (ProgressValue > 0 && !final)
            {
                // Linear extrapolation: total = elapsed / (progress/100)
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

        /// <summary>
        /// Scans log text for percentage values (e.g. "56.3%") and returns the highest found.
        /// Only returns values >= current (monotonic progress).
        /// </summary>
        private static double InferProgress(double current, string log)
        {
            double found = current;
            foreach (string line in log.Split('\n'))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || IsIndexProgressLine(trimmed)) continue;

                MatchCollection matches = ProgressPercentRegex().Matches(trimmed);
                foreach (Match match in matches)
                {
                    if (double.TryParse(match.Value.TrimEnd('%', ' '), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        found = Math.Max(found, Math.Clamp(value, 0d, 100d));
                }
            }
            return found;
        }

        private long TryGetOutputSizeBytes()
        {
            try
            {
                string resolvedPath = _success && _command.MuxCommand != null
                    ? _command.MuxCommand.OutputPath
                    : EncodingPipeline.ResolveOutputPathWithExtension(_request.EncoderExeName, _request.OutputPath);
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

        /// <summary>
        /// Estimates final output size by linearly extrapolating current output size
        /// based on progress ratio (frame-based if available, otherwise percentage-based).
        /// </summary>
        private string GetEstimatedOutputSizeText()
        {
            double progressRatio = GetProgressRatio();
            if (_currentOutputSizeBytes <= 0 || progressRatio <= 0d) return Lang.NotAvailableText;

            double estimatedBytes = _currentOutputSizeBytes / progressRatio;
            if (double.IsNaN(estimatedBytes) || double.IsInfinity(estimatedBytes)) return Lang.NotAvailableText;
            return FormatGbValue((long)Math.Round(Math.Max(0d, estimatedBytes)));
        }

        /// <summary>
        /// Returns the progress ratio (0.0–1.0). Prefers frame-based ratio when total frames
        /// are known; falls back to percentage-based progress value.
        /// </summary>
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

        /// <summary>
        /// Sums a value across an entire process tree (root + all descendants).
        /// Uses the selector function to extract the metric (e.g. working set, page faults)
        /// from each process. Silently returns 0 if any process has exited or is inaccessible.
        /// </summary>
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

        /// <summary>
        /// BFS traversal to collect all descendant process IDs from a root process.
        /// Uses the pre-built child map to avoid repeated Win32 calls.
        /// Returns the root ID plus all descendants.
        /// </summary>
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

        /// <summary>
        /// Builds a parent→children process map by enumerating all running processes
        /// via Win32 CreateToolhelp32Snapshot. Used to calculate working set and page faults
        /// across the entire process tree (e.g. when an encoder spawns helper processes).
        /// </summary>
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

        /// <summary>
        /// Reads system-wide memory statistics from Win32 APIs.
        /// First tries GlobalMemoryStatusEx, then refines with GetPerformanceInfo
        /// (which provides system cache and more accurate page-aligned values).
        /// </summary>
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
            long commitAvailableBytes = ToNonNegativeLong(memoryStatus.ullAvailPageFile);
            long committedBytes = Math.Max(0, commitLimitBytes - commitAvailableBytes);
            long systemCacheBytes = 0;

            // Try to get more detailed info from psapi (includes system cache)
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

        /// <summary>
        /// Represents a snapshot of the system's memory status at a point in time.
        /// Contains information about physical RAM and virtual memory usage.
        /// </summary>
        /// <param name="TotalPhysicalBytes">Total amount of physical RAM in bytes.</param>
        /// <param name="AvailablePhysicalBytes">Amount of available (free) physical RAM in bytes.</param>
        /// <param name="CommitLimitBytes">Maximum amount of virtual memory that can be committed in bytes.</param>
        /// <param name="CommittedBytes">Amount of currently committed virtual memory in bytes.</param>
        /// <param name="SystemCacheBytes">Amount of physical RAM used by the system cache in bytes.</param>
        /// <param name="MemoryLoadPercent">Percentage of physical memory currently in use (0-100).</param>
        private readonly record struct MemoryStatusSnapshot(
            long TotalPhysicalBytes,
            long AvailablePhysicalBytes,
            long CommitLimitBytes,
            long CommittedBytes,
            long SystemCacheBytes,
            int MemoryLoadPercent)
        {
            /// <summary>
            /// Gets the amount of used physical RAM in bytes, calculated as the difference
            /// between total and available physical memory.
            /// </summary>
            public long UsedPhysicalBytes =>
                Math.Max(0, TotalPhysicalBytes - AvailablePhysicalBytes);
        }

        /// <summary>
        /// Resets accumulated memory usage peaks and written-frame counters.
        /// Does NOT reset encoding progress.
        /// </summary>
        private void ResetStats()
        {
            _lastMemoryStatsUpdate = DateTime.Now;
            _upstreamWorkingSetPeakBytes = 0;
            _encoderWorkingSetPeakBytes = 0;
            SetCurrentOutputSizeBytes(TryGetOutputSizeBytes());
            _writtenFrames = 0;
            OnPropertyChanged(nameof(WrittenFramesLabel));
            OnPropertyChanged(nameof(EstimatedSizeLabel));
            StatusText = Lang.ResetUsageStatusText;
            UpdateMetrics();
            UpdateMemoryRangeBlocks();
        }

        /// <summary>
        /// Gracefully interrupts the upstream decoder by closing its main window and stdout stream.
        /// Runs on a background thread to avoid blocking the UI.
        /// </summary>
        private void TryInterruptUpstream()
        {
            _userInterruptRequested = true;
            _upstreamInterruptButtonClicked = true;
            FinishButtons.B5_3IsEnabled = false;
            FinishButtons.B5_4IsEnabled = false;
            FinishButtons.B5_3Text = Lang.InterruptingUpstreamText;
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

        /// <summary>
        /// Interrupts the entire pipeline: encoder stdin (signals EOF to encoder),
        /// then closes mux, encoder, upstream stdout, and upstream processes in order.
        /// Runs on a background thread to avoid blocking the UI.
        /// </summary>
        private void TryInterruptEncoder()
        {
            _userInterruptRequested = true;
            _encoderInterruptButtonClicked = true;
            FinishButtons.B5_3IsEnabled = false;
            FinishButtons.B5_4IsEnabled = false;
            FinishButtons.B5_4Text = Lang.InterruptingEncoderText;
            StatusText = Lang.InterruptingEncoderText;

            Task.Run(() =>
            {
                try
                {
                    TryCloseStream(_encoderStdinStream);
                    TryCloseMainWindow(_muxProcess);
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

        private async Task StopUpstreamAfterEncoderExitAsync(Task encoderExitedTask, Process upstream, CancellationToken cancellationToken)
        {
            try
            {
                await encoderExitedTask.WaitAsync(cancellationToken);
                if (upstream.HasExited) return;

                await Task.Delay(UpstreamShutdownAfterEncoderExitDelayMs, cancellationToken);
                if (upstream.HasExited) return;

                TryCloseStream(_upstreamStdoutStream);
                TryCloseMainWindow(upstream);

                await Task.Delay(UpstreamKillAfterShutdownTimeoutMs, cancellationToken);
                if (!upstream.HasExited)
                    TryKillProcess(upstream);
            }
            catch (OperationCanceledException)
            {
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

        private static void TryKillProcess(Process? process)
        {
            try
            {
                if (process is { HasExited: false })
                    process.Kill();
            }
            catch
            {
            }
        }

        private void EnableCloseButton()
        {
            _finishEnabledAfterClose = true;
            IsWindowCloseEnabled = true;
            FinishButtons.B5_5IsEnabled = true;
            RefreshCancelAllBindings();
        }

        private void CancelAllQueue()
        {
            if (!IsCancelAllEnabled) return;
            _cancelAllRequested = true;
            QueueSidebar.CancelPendingJobs();
            RefreshCancelAllBindings();
            TryInterruptEncoder();
        }

        private void RefreshCancelAllBindings()
        {
            OnPropertyChanged(nameof(IsCancelAllEnabled));
        }

        /// <summary>
        /// Opens the output directory in Windows Explorer.
        /// </summary>
        private void OpenOutputDirectory()
        {
            string? directory = Path.GetDirectoryName(_request.OutputPath);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }

        private void ShowEncodingCommand()
        {
            EncodingPipelineCommand command = QueueSidebar.SelectedJob?.Command ?? _command;
            new OpenDebugModalCmd(_modalNavS, Lang.EncodingCommandTitle, command.DisplayCommandLine).Execute(null);
        }

        private string BuildOpusAudioCommandHint()
        {
            EncodingPipelineRequest request = QueueSidebar.SelectedJob?.Request ?? _request;
            string command = BuildOpusAudioCommand(request);
            return string.IsNullOrEmpty(command)
                ? string.Empty
                : string.Format(CultureInfo.InvariantCulture, Lang.OpusAudioCommandHintFormat, command);
        }

        private static string BuildOpusAudioCommand(EncodingPipelineRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FfmpegPath) || string.IsNullOrWhiteSpace(request.SourceVideoPath))
                return string.Empty;

            string outputDirectory = Path.GetDirectoryName(request.OutputPath) ?? string.Empty;
            string outputFileName = Path.ChangeExtension(Path.GetFileName(request.SourceVideoPath), ".ogg");
            string outputPath = Path.Combine(outputDirectory, outputFileName);
            return $"{QuoteArgument(request.FfmpegPath)} -i {QuoteArgument(request.SourceVideoPath)} -vn -c:a libopus -b:a 320000 -vbr on -compression_level 10 -frame_duration 100 {QuoteArgument(outputPath)}";
        }

        private static string QuoteArgument(string value) =>
            $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

        /// <summary>
        /// Shows a confirmation dialog asking whether to stop the entire queue.
        /// Returns true if the user confirms (stop queue), false to continue.
        /// </summary>
        private bool AskStopQueueConfirmation()
        {
            bool? result = null;

            ConfirmationModal window = new();
            CloseModalCmd cancelCmd = new(() => { result = false; window.Close(); });
            CloseModalCmd confirmCmd = new(() => { result = true; window.Close(); });

            ConfirmationVM vm = ConfirmationVM.CreateWarning(
                Lang.StopQueueConfirmTitle,
                Lang.StopQueueConfirmMessage,
                cancelCmd, confirmCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();

            return result ?? false;
        }

        /// <summary>
        /// Saves log text to a file in the output directory.
        /// </summary>
        private void SaveTextAndShowPath(string text, string fileName)
        {
            if (string.IsNullOrEmpty(text)) return;
            string directory = Path.GetDirectoryName(_request.OutputPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, fileName);
            File.WriteAllText(path, text, Encoding.UTF8);
            new OpenSuccModalCmd(_modalNavS, string.Empty, Path.GetFullPath(path)).Execute(null);
        }

        /// <summary>
        /// Cycles log font size between 10, 12, and 14pt.
        /// </summary>
        private void RotateLogFontSize()
        {
            LogFontSize = LogFontSize switch
            {
                < 12 => 12,
                < 14 => 14,
                _ => 10
            };
        }

        /// <summary>
        /// Formats the rate control display text (e.g. "CRF 18" or "ABR 8 Mbps")
        /// based on the encoder type and configuration.
        /// </summary>
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

        /// <summary>
        /// Formats the encoder preset/mode display text
        /// </summary>
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

        /// <summary>
        /// Refreshes all localised strings when the UI language changes.
        /// Updates button labels, footer columns, and fires PropertyChanged
        /// for all bindable string properties so the UI re-renders.
        /// </summary>
        private void RefreshLanguageBindings()
        {
            Lang = new EncodingMonitorModalLangProviderM(UILangProviderM.Current.LanguageCode);
            _cpuSetsLang = new CpuSetsLangProviderM(UILangProviderM.Current.LanguageCode);
            FreezeOrContinueText = _isFrozen
                ? Lang.ContinueMonitoringText
                : Lang.FreezeContinueText;
            MonitorButtons.B2_1Text = FreezeOrContinueText;
            MonitorButtons.B2_2Text = Lang.UpdateUsageText;
            ReportButtons.B3_1Text = Lang.SaveUpstreamStderrText;
            ReportButtons.B3_2Text = Lang.SaveDownstreamStderrText;
            ReportButtons.B3_3Text = Lang.RotateLogFontSizeText;
            FinishButtons.B5_1Text = Lang.OpenOutputDirectoryText;
            FinishButtons.B5_2Text = Lang.ViewEncodingCommandText;
            FinishButtons.B5_3Text = _upstreamInterruptButtonClicked ? Lang.InterruptingUpstreamText : Lang.InterruptUpstreamText;
            FinishButtons.B5_4Text = _encoderInterruptButtonClicked ? Lang.InterruptingEncoderText : Lang.InterruptEncoderText;
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
            OnPropertyChanged(nameof(EnableMuxText));
            OnPropertyChanged(nameof(RichTextModeText));
            OnPropertyChanged(nameof(MuxTimebaseHint));
            OnPropertyChanged(nameof(OpusAudioCommandHint));
            OnPropertyChanged(nameof(OpusAudioBitrateHint));
            OnPropertyChanged(nameof(DistributionUpstreamLabel));
            OnPropertyChanged(nameof(DistributionDownstreamLabel));
            OnPropertyChanged(nameof(DistributionCacheLabel));
            OnPropertyChanged(nameof(DistributionAvailableLabel));
            OnPropertyChanged(nameof(MemoryRangeLegendTitle));

            OnPropertyChanged(nameof(StderrTitle));
            OnPropertyChanged(nameof(SampleIntervalTickLabels));
        }

        /// <summary>
        /// Cleans up timer, event subscriptions, and cancellation token on disposal.
        /// </summary>
        public override void Dispose()
        {
            _cts?.Cancel();
            TryCloseStream(_encoderStdinStream);
            TryCloseStream(_upstreamStdoutStream);
            TryCloseMainWindow(_muxProcess);
            TryCloseMainWindow(_encoderProcess);
            TryCloseMainWindow(_upstreamProcess);
            TryKillProcess(_muxProcess);
            TryKillProcess(_encoderProcess);
            TryKillProcess(_upstreamProcess);
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            QueueSidebar.PropertyChanged -= OnQueueSidebarPropertyChanged;
            _cts?.Dispose();
            _cts = null;
            _muxProcess = null;
            _encoderProcess = null;
            _upstreamProcess = null;
            _encoderStdinStream = null;
            _upstreamStdoutStream = null;
            QueueSidebar.Dispose();
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

        private readonly record struct EncodingLogSnapshot(string UpstreamText, string DownstreamText);

        private readonly record struct LogFoldEntry(string Line, int RepeatCount);

        /// <summary>
        /// Tracks unique log lines and their repeat counts for log folding.
        /// When a line repeats, its count is incremented instead of adding a new line,
        /// keeping the log compact (e.g. "frame= 100 (x5,432)").
        /// </summary>
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
