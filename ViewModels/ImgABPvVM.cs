using OneColumnEncoder.Commands;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        private CancellationTokenSource? _previewCts;
        private Process? _currentProcess;
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
            _colorSpaceAnalysis = ColorSpaceConverterH.Analyze(sourceFfprobeJson);
            FfprobeSourceStats sourceStats = FfprobeSourceStatsH.Read(sourceFfprobeJson ?? string.Empty);
            MaxPositionSeconds = Math.Max(1, (int)Math.Floor(Math.Min(int.MaxValue, sourceStats.DurationSeconds)) - 1);
            PreviewPositionSeconds = hasSourceStats
                ? Math.Min(MaxPositionSeconds, Math.Max(0, MaxPositionSeconds / 2))
                : 0;
            BuildPositionTickLabels(sourceStats.DurationSeconds);

            StatusText = Lang.StatusReady;
            PreviewButtonText = Lang.PreviewButtonText;
            PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        public void SetZoomPercent(int percent) => ZoomPercent = Math.Max(1, percent);

        public void SetFitMode(bool isFitMode) => _isFitMode = isFitMode;

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

            _previewCts?.Dispose();
            _previewCts = new CancellationTokenSource();
            CancellationToken token = _previewCts.Token;
            IsBusy = true;

            try
            {
                EncoderConfM model = _encoderConfVM.CreatePreviewModel();
                PreviewEncoder encoder = GetSelectedEncoder();

                if (encoder == PreviewEncoder.SvtAv1 && IsSource12Bit())
                {
                    _modalNavS.Close();
                    new Commands.OpenClose.OpenErrModalCmd(_modalNavS, Lang.EncoderLabel, Lang.WarnSvtAv1No12Bit).Execute(null);
                    return;
                }

                string displayFilter = BuildDisplayFilter() ?? string.Empty;
                string rawSourcePath = GetWorkPath("source-raw.png");
                string sourcePath = string.IsNullOrWhiteSpace(displayFilter)
                    ? rawSourcePath
                    : GetWorkPath($"source-{GetDisplayModeFileSuffix()}.png");
                string encodedPath = GetEncodedPath(encoder);
                string decodedPath = GetDecodedPath(encoder);

                StatusText = Lang.StatusExtracting;
                await RunFfmpegAsync(BuildSourceArgs(rawSourcePath), token);
                EnsureFileExists(rawSourcePath, "Source preview frame was not generated.");

                if (!string.IsNullOrWhiteSpace(displayFilter))
                {
                    StatusText = string.Format(Lang.StatusConverting, GetDisplayModeTitle(_displayMode));
                    await RunFfmpegAsync(BuildSourceArgs(sourcePath, displayFilter), token);
                }
                EnsureFileExists(sourcePath, "Source preview frame was not generated.");
                SourceImage = LoadBitmap(sourcePath);

                StatusText = string.Format(Lang.StatusEncoding, GetEncoderTitle(encoder));
                await RunFfmpegAsync(BuildEncodeArgs(encoder, model, sourcePath, encodedPath), token);

                StatusText = Lang.StatusDecoding;
                await RunFfmpegAsync(BuildDecodeArgs(encodedPath, decodedPath), token);
                EnsureFileExists(decodedPath, "Encoded preview frame was not generated.");
                EncodedImage = LoadBitmap(decodedPath);

                StatusText = string.Format(Lang.StatusPreviewReady, GetEncoderTitle(encoder), GetCrfValue(encoder, model));
            }
            catch (OperationCanceledException)
            {
                StatusText = Lang.StatusCancelled;
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
            }
            finally
            {
                _currentProcess = null;
                IsBusy = false;
            }
        }

        private string[] BuildSourceArgs(string outputPath, string? displayFilter = null)
        {
            List<string> args =
            [
                "-hide_banner",
                "-y",
                "-strict",
                "unofficial",
                "-ss",
                EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(PreviewPositionSeconds)),
                "-i",
                _sourceVideoPath!
            ];

            if (!string.IsNullOrWhiteSpace(displayFilter))
                args.AddRange(["-vf", displayFilter]);

            args.AddRange(
            [
                "-vframes",
                "1",
                "-c:v",
                "png",
                outputPath
            ]);
            return [.. args];
        }

        private static string[] BuildEncodeArgs(PreviewEncoder encoder, EncoderConfM model, string sourcePath, string outputPath)
        {
            List<string> args =
            [
                "-hide_banner",
                "-y",
                "-strict",
                "unofficial",
                "-i",
                sourcePath,
                "-c:v",
                GetFfmpegEncoderName(encoder),
                "-crf",
                GetCrfValue(encoder, model).ToString(CultureInfo.InvariantCulture)
            ];

            args.AddRange(SplitArgs(GetCustomParams(encoder, model)));
            args.AddRange(["-frames:v", "1"]);

            if (encoder == PreviewEncoder.X264)
                args.AddRange(["-f", "h264"]);
            else if (encoder == PreviewEncoder.X265)
                args.AddRange(["-f", "hevc"]);

            args.Add(outputPath);
            return [.. args];
        }

        private static string[] BuildDecodeArgs(string inputPath, string outputPath)
        {
            List<string> args =
            [
                "-hide_banner",
                "-y",
                "-strict",
                "unofficial",
                "-i",
                inputPath
            ];

            args.AddRange(
            [
                "-frames:v",
                "1",
                "-c:v",
                "png",
                outputPath
            ]);
            return [.. args];
        }

        private async Task RunFfmpegAsync(IReadOnlyList<string> args, CancellationToken token)
        {
            ProcessStartInfo psi = new()
            {
                FileName = _ffmpegPath!,
                WorkingDirectory = _workDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
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
                TryKillProcess(process);
                throw;
            }

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                string message = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                throw new InvalidOperationException(TrimProcessMessage(message));
            }
        }

        private static BitmapImage LoadBitmap(string path)
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static void EnsureFileExists(string path, string message)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(message, path);
        }

        private void RefreshSelectedEncodedImage()
        {
            string decodedPath = GetDecodedPath(GetSelectedEncoder());
            EncodedImage = File.Exists(decodedPath) ? LoadBitmap(decodedPath) : null;
        }

        private void BuildPositionTickLabels(double durationSeconds)
        {
            PositionTickLabels.Clear();
            double safeDuration = Math.Max(1d, Math.Min(MaxPositionSeconds, durationSeconds));
            for (int i = 0; i <= 4; i++)
                PositionTickLabels.Add(Math.Round(safeDuration * i / 4d).ToString(CultureInfo.InvariantCulture));
        }

        private PreviewEncoder GetSelectedEncoder() =>
            EncoderDropdown.SelectedItem?.Tag is PreviewEncoder encoder ? encoder : PreviewEncoder.X264;

        private string GetWorkPath(string fileName) => Path.Combine(_workDirectory, fileName);

        private string GetEncodedPath(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => GetWorkPath($"x264-{GetDisplayModeFileSuffix()}.h264"),
            PreviewEncoder.X265 => GetWorkPath($"x265-{GetDisplayModeFileSuffix()}.hevc"),
            _ => GetWorkPath($"svtav1-{GetDisplayModeFileSuffix()}.obu")
        };

        private string GetDecodedPath(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => GetWorkPath($"x264-{GetDisplayModeFileSuffix()}.png"),
            PreviewEncoder.X265 => GetWorkPath($"x265-{GetDisplayModeFileSuffix()}.png"),
            _ => GetWorkPath($"svtav1-{GetDisplayModeFileSuffix()}.png")
        };

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

        private string? BuildDisplayFilter()
        {
            ColorSpaceStrategy? strategy = _displayMode switch
            {
                PreviewDisplayMode.LowToBt709 => ColorSpaceStrategy.LowToHigh,
                PreviewDisplayMode.WcgToBt709 => ColorSpaceStrategy.HighToLow,
                PreviewDisplayMode.HdrToSdr => ColorSpaceStrategy.HdrToSdr,
                PreviewDisplayMode.HighHdrToSdr => ColorSpaceStrategy.HighHdrToSdr,
                _ => null
            };
            if (strategy == null) return null;

            string? filter = ColorSpaceConverterH.BuildFfmpegFilter(
                strategy.Value,
                _colorSpaceAnalysis.ColorMatrix,
                _colorSpaceAnalysis.ColorChromaLocation,
                _colorSpaceAnalysis.ColorPrimaries,
                _colorSpaceAnalysis.PixelFormat);
            if (string.IsNullOrWhiteSpace(filter)) return null;

            filter = filter.Replace("<nits>", "1000", StringComparison.Ordinal);
            if (strategy == ColorSpaceStrategy.HdrToSdr)
                filter = string.Join(',', filter, "zscale=matrix=bt709:primaries=bt709:transfer=bt709");
            return string.Join(',', filter, "format=rgb24");
        }

        private string GetDisplayModeFileSuffix() => _displayMode switch
        {
            PreviewDisplayMode.LowToBt709 => "low709",
            PreviewDisplayMode.WcgToBt709 => "wcg709",
            PreviewDisplayMode.HdrToSdr => "hdrsdr",
            PreviewDisplayMode.HighHdrToSdr => "highhdrsdr",
            _ => "raw"
        };

        private string GetDisplayModeTitle(PreviewDisplayMode displayMode) => displayMode switch
        {
            PreviewDisplayMode.LowToBt709 => Lang.DisplayModeLowToBt709,
            PreviewDisplayMode.WcgToBt709 => Lang.DisplayModeWcgToBt709,
            PreviewDisplayMode.HdrToSdr => Lang.DisplayModeHdrToSdr,
            PreviewDisplayMode.HighHdrToSdr => Lang.DisplayModeHighHdrToSdr,
            _ => Lang.DisplayModeRaw
        };

        private static string GetFfmpegEncoderName(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => "libx264",
            PreviewEncoder.X265 => "libx265",
            _ => "libsvtav1"
        };

        private static string GetEncoderTitle(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => "libx264",
            PreviewEncoder.X265 => "libx265",
            _ => "libsvtav1"
        };

        private static int GetCrfValue(PreviewEncoder encoder, EncoderConfM model) => encoder switch
        {
            PreviewEncoder.X264 => model.X264Crf,
            PreviewEncoder.X265 => model.X265Crf,
            _ => model.SvtAv1Crf
        };

        private static string GetCustomParams(PreviewEncoder encoder, EncoderConfM model) => encoder switch
        {
            PreviewEncoder.X264 => model.CustomParamsX264,
            PreviewEncoder.X265 => model.CustomParamsX265,
            _ => model.CustomParamsSvtAv1
        };

        private static IEnumerable<string> SplitArgs(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) yield break;

            StringBuilder current = new();
            bool inQuotes = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        yield return current.ToString();
                        current.Clear();
                    }
                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
                yield return current.ToString();
        }

        private bool IsSource12Bit() =>
            _colorSpaceAnalysis.PixelFormat?.Contains("12le", StringComparison.OrdinalIgnoreCase) == true;

        private static string TrimProcessMessage(string message)
        {
            string text = string.IsNullOrWhiteSpace(message) ? "ffmpeg failed." : message.Trim();
            text = text.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
            while (text.Contains("  ", StringComparison.Ordinal))
                text = text.Replace("  ", " ", StringComparison.Ordinal);
            return text.Length <= 700 ? text : text[^700..];
        }

        private void TryKillCurrentProcess()
        {
            if (_currentProcess != null)
                TryKillProcess(_currentProcess);
        }

        private static void TryKillProcess(Process process)
        {
            try { if (!process.HasExited) process.Kill(true); }
            catch { }
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
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            GC.SuppressFinalize(this);
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

        private enum PreviewEncoder { X264, X265, SvtAv1 }
        private enum PreviewDisplayMode { Raw, LowToBt709, WcgToBt709, HdrToSdr, HighHdrToSdr }
    }
}
