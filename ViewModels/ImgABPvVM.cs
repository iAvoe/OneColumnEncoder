using OneColumnEncoder.Commands;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Pipeline;
using OneColumnEncoder.Analytics;
using OneColumnEncoder.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace OneColumnEncoder.ViewModels
{
    public class ImgABPvVM : BaseVM
    {
        private readonly EncoderConfVM _encoderConfVM;
        private readonly Stores.ModalNavS _modalNavS;
        private readonly string? _ffmpegPath;
        private readonly string? _sourceVideoPath;
        private readonly string _workDirectory;
        private readonly ColorSpaceAnalysisM _colorSpaceAnalysis;
        // CTS for ffmpeg operations only (extract, encode, decode).
        // Created fresh each preview run. Score tools (ssimulacra2, butteraugli)
        // do NOT observe this token — they run independently once decoding finishes.
        private CancellationTokenSource? _previewCts;

        // Tracks the running ffmpeg process so it can be force-killed on cancel.
        // Not used for external score-tool processes.
        private Process? _currentProcess;
        private string? _lastFfmpegStderr;
        private bool _isFitMode = true;
        private PreviewDisplayMode _displayMode = PreviewDisplayMode.Raw;
        private ImgABPvLangProviderM _lang = new(UILangProviderM.Current.LanguageCode);
        public ImgABPvLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }

        public DropdownMenuVM EncoderDropdown { get; } = new();
        public ButtonGroupVM ZoomPresetButtons { get; }
        public ButtonGroupVM DisplayModeButtons { get; }
        public ActionCmd PreviewCommand { get; }
        public ObservableCollection<string> PositionTickLabels { get; } = [];

        public string EncoderLabel => Lang.EncoderLabel;
        public string DisplayModeLabel => Lang.DisplayModeLabel;
        public string ZoomLabel => Lang.ZoomLabel;
        public string PositionLabel => Lang.PositionLabel;
        public string Hint1Text => Lang.Hint1Text;
        public string Hint2Text => Lang.Hint2Text;
        public string Hint3Text => Lang.Hint3Text;
        public string SsimulacraScoreHint => Lang.SsimulacraScoreHint;
        public string ButteraugliScoreHint => Lang.ButteraugliScoreHint;

        private ImageSource? _sourceImage;
        public ImageSource? SourceImage
        {
            get => _sourceImage;
            private set => SetProperty(ref _sourceImage, value);
        }

        private ImageSource? _encodedImage;
        public ImageSource? EncodedImage
        {
            get => _encodedImage;
            private set => SetProperty(ref _encodedImage, value);
        }

        private int _previewPositionSeconds;
        public int PreviewPositionSeconds
        {
            get => _previewPositionSeconds;
            set => SetProperty(ref _previewPositionSeconds, Math.Max(0, Math.Min(MaxPositionSeconds, value)));
        }

        private int _maxPositionSeconds = 1;
        public int MaxPositionSeconds
        {
            get => _maxPositionSeconds;
            private set => SetProperty(ref _maxPositionSeconds, Math.Max(1, value));
        }

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        private string _previewButtonText = "Preview";
        public string PreviewButtonText
        {
            get => _previewButtonText;
            private set => SetProperty(ref _previewButtonText, value);
        }

        private int _zoomPercent = 100;
        public int ZoomPercent
        {
            get => _zoomPercent;
            private set => SetProperty(ref _zoomPercent, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (!SetProperty(ref _isBusy, value)) return;
                OnPropertyChanged(nameof(IsIdle));
                PreviewButtonText = value ? Lang.CancelButtonText : Lang.PreviewButtonText;
                _encoderConfVM.SetPreviewBusy(value);
            }
        }

        public bool IsIdle => !IsBusy;
        public bool IsFitMode => _isFitMode;

        private string _ssimulacra2StatusText = "";
        public string Ssimulacra2StatusText
        {
            get => _ssimulacra2StatusText;
            private set => SetProperty(ref _ssimulacra2StatusText, value);
        }

        private string _butteraugliStatusText = "";
        public string ButteraugliStatusText
        {
            get => _butteraugliStatusText;
            private set => SetProperty(ref _butteraugliStatusText, value);
        }

        public ImgABPvVM(EncoderConfVM encoderConfVM, Stores.ModalNavS modalNavS, string? ffmpegPath, string? sourceVideoPath, string? sourceFfprobeJson)
        {
            _encoderConfVM = encoderConfVM;
            _modalNavS = modalNavS;
            _ffmpegPath = ffmpegPath;
            _sourceVideoPath = sourceVideoPath;
            _workDirectory = Path.Combine(Path.GetTempPath(), "1cenc-image-preview-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_workDirectory);

            ZoomPresetButtons = ButtonGroupVM.CreateThreeButton(Lang.FitButtonText, "100%", "200%");

            EncoderDropdown.Items.Add(new DropdownItemM("libx264") { Tag = PreviewEncoder.X264 });
            EncoderDropdown.Items.Add(new DropdownItemM("libx265") { Tag = PreviewEncoder.X265 });
            EncoderDropdown.Items.Add(new DropdownItemM("libsvtav1") { Tag = PreviewEncoder.SvtAv1 });
            EncoderDropdown.Items.Add(new DropdownItemM("libvvenc (Preview Only)") { Tag = PreviewEncoder.Vvenc });
            EncoderDropdown.SelectedItem = EncoderDropdown.Items[0];
            EncoderDropdown.SelectionChangedCommand = new ActionCmd(_ => RefreshSelectedEncodedImage());
            DisplayModeButtons = ButtonGroupVM.CreateFiveButton(
                Lang.RawButtonText,
                "Low\u2192Bt709",
                "WCG\u2192Bt709",
                "HDR\u2192SDR",
                "HDRWCG\u2192SDR709",
                new ActionCmd(_ => SetDisplayMode(PreviewDisplayMode.Raw)),
                new ActionCmd(_ => SetDisplayMode(PreviewDisplayMode.LowToBt709)),
                new ActionCmd(_ => SetDisplayMode(PreviewDisplayMode.WcgToBt709)),
                new ActionCmd(_ => SetDisplayMode(PreviewDisplayMode.HdrToSdr)),
                new ActionCmd(_ => SetDisplayMode(PreviewDisplayMode.HighHdrToSdr)));

            bool hasSourceStats = !string.IsNullOrWhiteSpace(sourceFfprobeJson);
            _colorSpaceAnalysis = ColorSpaceConverter.Analyze(sourceFfprobeJson);
            FFProbeSourceStats sourceStats = FFProbeSourceStatsReader.Read(sourceFfprobeJson ?? string.Empty);
            MaxPositionSeconds = Math.Max(1, (int)Math.Floor(Math.Min(int.MaxValue, sourceStats.DurationSeconds)) - 1);
            PreviewPositionSeconds = hasSourceStats
                ? Math.Min(MaxPositionSeconds, Math.Max(0, MaxPositionSeconds / 2))
                : 0;
            BuildPositionTickLabels(sourceStats.DurationSeconds);

            StatusText = Lang.StatusReady;
            PreviewButtonText = Lang.PreviewButtonText;
            PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
            RefreshSsimulacra2Status();
            RefreshButteraugliStatus();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        public void SetZoomPercent(int percent) => ZoomPercent = Math.Max(1, percent);

        public void SetFitMode(bool isFitMode) => _isFitMode = isFitMode;

        // Toggle: cancel in-flight preview or start a new one.
        // Cancellation signals the CTS, then immediately kills ffmpeg.
        private void PreviewOrCancel()
        {
            if (IsBusy)
            {
                _previewCts?.Cancel();
                TryKillCurrentProcess();
                return;
            }

            _ = GeneratePreviewAsync();
        }

        private async Task GeneratePreviewAsync()
        {
            if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
            {
                StatusText = Lang.StatusNoFfmpeg;
                return;
            }

            if (string.IsNullOrWhiteSpace(_sourceVideoPath) || !File.Exists(_sourceVideoPath))
            {
                StatusText = Lang.StatusNoSource;
                return;
            }

            // Discard previous cancellation scope and start a fresh one
            // for this preview run.
            _previewCts?.Dispose();
            _previewCts = new CancellationTokenSource();
            CancellationToken token = _previewCts.Token;

            try
            {
                EncoderConfM model = _encoderConfVM.CreatePreviewModel();
                PreviewEncoder encoder = GetSelectedEncoder();

                if (encoder == PreviewEncoder.SvtAv1 && PreviewPipeline.IsSource12Bit(_colorSpaceAnalysis))
                {
                    _modalNavS.Close();
                    new Commands.OpenClose.OpenErrModalCmd(_modalNavS, Lang.EncoderLabel, Lang.WarnSvtAv1No12Bit).Execute(null);
                    IsBusy = false;
                    return;
                }

                string displayFilter = PreviewPipeline.BuildDisplayFilter(_displayMode, _colorSpaceAnalysis) ?? string.Empty;
                string rawSourcePath = GetWorkPath("source-raw.png");
                string sourcePath = string.IsNullOrWhiteSpace(displayFilter)
                    ? rawSourcePath
                    : GetWorkPath($"source-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.png");
                string encodedPath = GetEncodedPath(encoder);
                string decodedPath = GetDecodedPath(encoder);

                StatusText = Lang.StatusExtracting;
                await RunFfmpegAsync(PreviewPipeline.BuildSourceArgs(_sourceVideoPath!, PreviewPositionSeconds, rawSourcePath), token);
                EnsureFileExists(rawSourcePath, "!SOURCE");

                if (!string.IsNullOrWhiteSpace(displayFilter))
                {
                    StatusText = string.Format(Lang.StatusConverting, GetDisplayModeTitle(_displayMode));
                    await RunFfmpegAsync(PreviewPipeline.BuildSourceArgs(_sourceVideoPath!, PreviewPositionSeconds, sourcePath, displayFilter), token);
                }
                EnsureFileExists(sourcePath, "!SOURCE");
                SourceImage = PreviewPipeline.LoadBitmap(sourcePath);

                StatusText = string.Format(Lang.StatusEncoding, PreviewPipeline.GetEncoderTitle(encoder));
                await RunFfmpegAsync(PreviewPipeline.BuildEncodeArgs(encoder, model, sourcePath, encodedPath), token);

                StatusText = Lang.StatusDecoding;
                await RunFfmpegAsync(PreviewPipeline.BuildDecodeArgs(encodedPath, decodedPath), token);
                EnsureFileExists(decodedPath, "!ENCODE");
                EncodedImage = PreviewPipeline.LoadBitmap(decodedPath);

                StatusText = Lang.StatusComputingScores;

                // NOTE: Score tools do NOT accept the cancellation token.
                // If the user cancels during this phase, the tools will still
                // run to completion, then IsBusy resets normally.
                if (Ssimulacra2.IsSsimU2Present)
                {
                    Ssimulacra2StatusText = Lang.Ssimulacra2ToolPresent;
                    var (score, error) = await Ssimulacra2.RunScoreAsync(sourcePath, decodedPath);
                    Ssimulacra2StatusText = score.HasValue
                        ? $"SSIMULACRA2.1: {score.Value:F2}"
                        : $"SSIMULACRA2.1: {error}";
                }

                if (Butteraugli.IsPresent)
                {
                    ButteraugliStatusText = Lang.ButteraugliToolPresent;
                    var (score, error) = await Butteraugli.RunScoreAsync(sourcePath, decodedPath);
                    ButteraugliStatusText = score.HasValue
                        ? $"Butteraugli: {score.Value:F4}"
                        : $"Butteraugli: {error}";
                }

                StatusText = string.Format(Lang.StatusPreviewReady, PreviewPipeline.GetEncoderTitle(encoder), PreviewPipeline.GetCrfValue(encoder, model));
            }
            catch (OperationCanceledException)
            {
                StatusText = Lang.StatusCancelled;
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
                if (!string.IsNullOrWhiteSpace(_lastFfmpegStderr))
                {
                    _modalNavS.Close();
                    new Commands.OpenClose.OpenErrModalCmd(
                        _modalNavS,
                        Lang.EncoderLabel,
                        _lastFfmpegStderr).Execute(null);
                }
            }
            finally { _currentProcess = null; }

            IsBusy = false;
        }

        // Runs ffmpeg with the given args. If token is cancelled during
        // execution the process is killed and OperationCanceledException
        // propagates to the caller.
        private async Task RunFfmpegAsync(IReadOnlyList<string> args, CancellationToken token)
        {
            ProcessStartInfo psi = new()
            {
                FileName = _ffmpegPath!,
                WorkingDirectory = _workDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };

            foreach (string arg in args)
                psi.ArgumentList.Add(arg);

            using Process process = new() { StartInfo = psi, EnableRaisingEvents = true };
            _currentProcess = process;
            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(token);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(token);

            try
            {
                await process.WaitForExitAsync(token);
            }
            catch (OperationCanceledException)
            {
                PreviewPipeline.TryKillProcess(process);
                throw;
            }

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                _lastFfmpegStderr = stderr;
                string diagnostic = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                throw new InvalidOperationException(
                    $"ffmpeg exited with code {process.ExitCode}. " +
                    PreviewPipeline.TrimProcessMessage(diagnostic));
            }
        }

        private void RefreshSelectedEncodedImage()
        {
            string decodedPath = GetDecodedPath(GetSelectedEncoder());
            EncodedImage = File.Exists(decodedPath) ? PreviewPipeline.LoadBitmap(decodedPath) : null;
        }

        private void BuildPositionTickLabels(double durationSeconds)
        {
            PositionTickLabels.Clear();
            double safeDuration = Math.Max(1d, Math.Min(MaxPositionSeconds, durationSeconds));
            for (int i = 0; i <= 4; i++)
                PositionTickLabels.Add(Math.Round(safeDuration * i / 4d).ToString(CultureInfo.InvariantCulture));
        }

        #region Preview Path Queries
        private PreviewEncoder GetSelectedEncoder() =>
            EncoderDropdown.SelectedItem?.Tag is PreviewEncoder encoder ? encoder : PreviewEncoder.X264;

        private string GetWorkPath(string fileName) => Path.Combine(_workDirectory, fileName);

        private string GetEncodedPath(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => GetWorkPath($"x264-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.h264"),
            PreviewEncoder.X265 => GetWorkPath($"x265-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.hevc"),
            PreviewEncoder.Vvenc => GetWorkPath($"vvenc-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.vvc"),
            _ => GetWorkPath($"svtav1-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.obu")
        };

        private string GetDecodedPath(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => GetWorkPath($"x264-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.png"),
            PreviewEncoder.X265 => GetWorkPath($"x265-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.png"),
            PreviewEncoder.Vvenc => GetWorkPath($"vvenc-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.png"),
            _ => GetWorkPath($"svtav1-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.png")
        };

        private string GetDisplayModeTitle(PreviewDisplayMode displayMode) => displayMode switch
        {
            PreviewDisplayMode.LowToBt709 => Lang.DisplayModeLowToBt709,
            PreviewDisplayMode.WcgToBt709 => Lang.DisplayModeWcgToBt709,
            PreviewDisplayMode.HdrToSdr => Lang.DisplayModeHdrToSdr,
            PreviewDisplayMode.HighHdrToSdr => Lang.DisplayModeHighHdrToSdr,
            _ => Lang.DisplayModeRaw
        };
        #endregion

        private void SetDisplayMode(PreviewDisplayMode displayMode)
        {
            if (_displayMode == displayMode) return;
            if (IsBusy)
            {
                StatusText = Lang.StatusDisplayModeBlocked;
                return;
            }

            _displayMode = displayMode;
            StatusText = string.Format(Lang.StatusDisplayModeSet, GetDisplayModeTitle(displayMode));
            RefreshSelectedEncodedImage();
            if (!IsBusy && SourceImage != null)
                _ = GeneratePreviewAsync();
        }

        private static void EnsureFileExists(string path, string message)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(message, path);
        }

        private void TryKillCurrentProcess()
        {
            if (_currentProcess != null)
                PreviewPipeline.TryKillProcess(_currentProcess);
        }

        private void RefreshSsimulacra2Status()
        {
            if (!Ssimulacra2.Is64Bit)
            {
                Ssimulacra2StatusText = "";
                return;
            }

            Ssimulacra2StatusText = Ssimulacra2.IsSsimU2Present
                ? Lang.Ssimulacra2ToolPresent
                : Lang.Ssimulacra2ToolMissing;
        }

        private void RefreshButteraugliStatus()
        {
            if (!Butteraugli.Is64Bit)
            {
                ButteraugliStatusText = "";
                return;
            }

            ButteraugliStatusText = Butteraugli.IsPresent
                ? Lang.ButteraugliToolPresent
                : Lang.ButteraugliToolMissing;
        }

        private void OnLanguageChanged()
        {
            Lang = new ImgABPvLangProviderM(UILangProviderM.Current.LanguageCode);
            ZoomPresetButtons.B3_1Text = Lang.FitButtonText;
            DisplayModeButtons.B5_1Text = Lang.RawButtonText;
            if (!IsBusy)
                PreviewButtonText = Lang.PreviewButtonText;
            OnPropertyChanged(nameof(EncoderLabel));
            OnPropertyChanged(nameof(DisplayModeLabel));
            OnPropertyChanged(nameof(ZoomLabel));
            OnPropertyChanged(nameof(PositionLabel));
            OnPropertyChanged(nameof(Hint1Text));
            OnPropertyChanged(nameof(Hint2Text));
            OnPropertyChanged(nameof(Hint3Text));
            OnPropertyChanged(nameof(SsimulacraScoreHint));
            OnPropertyChanged(nameof(ButteraugliScoreHint));
            RefreshSsimulacra2Status();
            RefreshButteraugliStatus();
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            GC.SuppressFinalize(this);
            // Order: cancel first so in-flight ffmpeg knows to stop,
            // then kill the process, then release CTS resources.
            _previewCts?.Cancel();
            TryKillCurrentProcess();
            _previewCts?.Dispose();
            _encoderConfVM.SetPreviewBusy(false);

            try
            {
                if (Directory.Exists(_workDirectory))
                    Directory.Delete(_workDirectory, recursive: true);
            }
            catch { }

            base.Dispose();
        }
    }
}
