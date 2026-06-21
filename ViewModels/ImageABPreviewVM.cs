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
    public class ImageABPreviewVM : BaseVM
    {
        private readonly EncoderConfVM _encoderConfVM;
        private readonly string? _ffmpegPath;
        private readonly string? _sourceVideoPath;
        private readonly string _workDirectory;
        private CancellationTokenSource? _previewCts;
        private Process? _currentProcess;
        private bool _isFitMode = true;

        public DropdownMenuVM EncoderDropdown { get; } = new();
        public ButtonGroupVM ZoomPresetButtons { get; } = ButtonGroupVM.CreateThreeButton("Fit", "100%", "200%");
        public ActionCmd PreviewCommand { get; }
        public ObservableCollection<string> PositionTickLabels { get; } = [];

        public static string WindowTitle => "1cenc A-B Preview";
        public static string EncoderLabel => "Encoder";
        public static string ZoomLabel => "Zoom";
        public static string PositionLabel => "Image Position";
        public static string Hint1Text => "Drag the split line to compare source and encoded frame.";
        public static string Hint2Text => "Preview uses ffmpeg only; available encoder options may differ from imported encoders.";
        public static string Hint3Text => "Compression runs only after Preview is clicked.";

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

        private string _statusText = "Ready.";
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
                PreviewButtonText = value ? "Cancel" : "Preview";
                _encoderConfVM.SetPreviewBusy(value);
            }
        }

        public bool IsIdle => !IsBusy;
        public bool IsFitMode => _isFitMode;

        public ImageABPreviewVM(EncoderConfVM encoderConfVM, string? ffmpegPath, string? sourceVideoPath, string? sourceFfprobeJson)
        {
            _encoderConfVM = encoderConfVM;
            _ffmpegPath = ffmpegPath;
            _sourceVideoPath = sourceVideoPath;
            _workDirectory = Path.Combine(Path.GetTempPath(), "1cenc-image-preview-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_workDirectory);

            EncoderDropdown.Items.Add(new DropdownItemM("libx264") { Tag = PreviewEncoder.X264 });
            EncoderDropdown.Items.Add(new DropdownItemM("libx265") { Tag = PreviewEncoder.X265 });
            EncoderDropdown.Items.Add(new DropdownItemM("libsvtav1") { Tag = PreviewEncoder.SvtAv1 });
            EncoderDropdown.SelectedItem = EncoderDropdown.Items[0];
            EncoderDropdown.SelectionChangedCommand = new ActionCmd(_ => RefreshSelectedEncodedImage());

            FfprobeSourceStats sourceStats = FfprobeSourceStatsH.Read(sourceFfprobeJson ?? string.Empty);
            MaxPositionSeconds = Math.Max(1, (int)Math.Floor(Math.Min(int.MaxValue, sourceStats.DurationSeconds)) - 1);
            PreviewPositionSeconds = Math.Min(MaxPositionSeconds, Math.Max(0, MaxPositionSeconds / 2));
            BuildPositionTickLabels(sourceStats.DurationSeconds);

            PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
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
                StatusText = "ffmpeg.exe is not imported.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_sourceVideoPath) || !File.Exists(_sourceVideoPath))
            {
                StatusText = "No valid video source selected.";
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
                string sourcePath = GetWorkPath("source.png");
                string encodedPath = GetEncodedPath(encoder);
                string decodedPath = GetDecodedPath(encoder);

                StatusText = "Extracting source frame...";
                await RunFfmpegAsync(BuildSourceArgs(sourcePath), token);
                SourceImage = LoadBitmap(sourcePath);

                StatusText = $"Encoding with {GetEncoderTitle(encoder)}...";
                await RunFfmpegAsync(BuildEncodeArgs(encoder, model, sourcePath, encodedPath), token);

                StatusText = "Decoding preview frame...";
                await RunFfmpegAsync(BuildDecodeArgs(encodedPath, decodedPath), token);
                EncodedImage = LoadBitmap(decodedPath);

                StatusText = $"Preview ready: {GetEncoderTitle(encoder)}, CRF {GetCrfValue(encoder, model)}.";
            }
            catch (OperationCanceledException)
            {
                StatusText = "Preview cancelled.";
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

        private string[] BuildSourceArgs(string outputPath) =>
        [
            "-hide_banner",
            "-y",
            "-ss",
            EncodingPipelineH.FormatTimestamp(TimeSpan.FromSeconds(PreviewPositionSeconds)),
            "-i",
            _sourceVideoPath!,
            "-vframes",
            "1",
            "-c:v",
            "png",
            outputPath
        ];

        private static string[] BuildEncodeArgs(PreviewEncoder encoder, EncoderConfM model, string sourcePath, string outputPath)
        {
            List<string> args =
            [
                "-hide_banner",
                "-y",
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

        private static string[] BuildDecodeArgs(string inputPath, string outputPath) =>
        [
            "-hide_banner",
            "-y",
            "-i",
            inputPath,
            "-frames:v",
            "1",
            "-c:v",
            "png",
            outputPath
        ];

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
            PreviewEncoder.X264 => GetWorkPath("x264.h264"),
            PreviewEncoder.X265 => GetWorkPath("x265.hevc"),
            _ => GetWorkPath("svtav1.obu")
        };

        private string GetDecodedPath(PreviewEncoder encoder) => encoder switch
        {
            PreviewEncoder.X264 => GetWorkPath("x264.png"),
            PreviewEncoder.X265 => GetWorkPath("x265.png"),
            _ => GetWorkPath("svtav1.png")
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

        public override void Dispose()
        {
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
    }
}
