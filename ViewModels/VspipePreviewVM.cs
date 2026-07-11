using OneColumnEncoder.Commands;
using OneColumnEncoder.Pipeline;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace OneColumnEncoder.ViewModels
{
    public class VspipePreviewVM : BaseVM
    {
        private readonly string _vspipePath;
        private readonly string _ffmpegPath;
        private readonly string _vspipeY4mArg;
        private readonly string _workDirectory;
        private readonly int _totalFrames;
        private readonly string _scriptPath;

        public string VideoFilename { get; }

        private CancellationTokenSource? _previewCts;
        private Process? _currentVspipeProcess;
        private Process? _currentFfmpegProcess;

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

        private int _currentFrame;
        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                int clamped = Math.Clamp(value, 0, TotalFrames - 1);
                if (SetProperty(ref _currentFrame, clamped))
                    OnPropertyChanged(nameof(PreviewPositionSeconds));
            }
        }

        public int TotalFrames
        {
            get => _totalFrames;
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
            }
        }

        public bool IsIdle => !IsBusy;

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

        public int PreviewPositionSeconds
        {
            get => CurrentFrame;
            set => CurrentFrame = value;
        }

        private int _maxPositionSeconds = 1;
        public int MaxPositionSeconds
        {
            get => _maxPositionSeconds;
            private set => SetProperty(ref _maxPositionSeconds, value);
        }

        public ObservableCollection<string> PositionTickLabels { get; } = [];

        public ActionCmd PreviewCommand { get; }

        public VspipePreviewVM(
            string vspipePath,
            string ffmpegPath,
            string vspipeY4mArg,
            string scriptContent,
            string videoFilename,
            int totalFrames)
        {
            _vspipePath = vspipePath;
            _ffmpegPath = ffmpegPath;
            _vspipeY4mArg = vspipeY4mArg;
            VideoFilename = videoFilename;
            _totalFrames = totalFrames > 0 ? totalFrames : 1;
            _currentFrame = 0;
            MaxPositionSeconds = _totalFrames - 1;
            BuildPositionTickLabels(MaxPositionSeconds);

            _workDirectory = Path.Combine(
                Path.GetTempPath(),
                "1cenc-vpy-preview-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_workDirectory);

            _scriptPath = Path.Combine(_workDirectory, "preview.vpy");
            File.WriteAllText(_scriptPath, scriptContent);

            StatusText = "Ready";
            PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
        }

        private void BuildPositionTickLabels(int maxFrame)
        {
            PositionTickLabels.Clear();
            double safeMax = Math.Max(1d, maxFrame);
            for (int i = 0; i <= 4; i++)
                PositionTickLabels.Add(Math.Round(safeMax * i / 4d).ToString(CultureInfo.InvariantCulture));
        }

        public void SetZoomPercent(int percent) => ZoomPercent = Math.Max(1, percent);

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
            _previewCts?.Dispose();
            _previewCts = new CancellationTokenSource();
            CancellationToken token = _previewCts.Token;

            try
            {
                IsBusy = true;
                string sourcePath = Path.Combine(_workDirectory, "output-0.png");
                string filteredPath = Path.Combine(_workDirectory, "output-1.png");

                StatusText = "Extracting frame from output 0 (original)...";
                await RunVspipePipeAsync(0, sourcePath, token);
                EnsureFileExists(sourcePath);

                StatusText = "Extracting frame from output 1 (filtered)...";
                await RunVspipePipeAsync(1, filteredPath, token);
                EnsureFileExists(filteredPath);

                SourceImage = PreviewPipeline.LoadBitmap(sourcePath);
                EncodedImage = PreviewPipeline.LoadBitmap(filteredPath);

                StatusText = $"Frame {CurrentFrame} ready — A: original / B: filtered";
            }
            catch (OperationCanceledException)
            {
                StatusText = "Cancelled";
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
            }
            finally
            {
                _currentVspipeProcess = null;
                _currentFfmpegProcess = null;
                IsBusy = false;
            }
        }

        private async Task RunVspipePipeAsync(int outputIndex, string outputPngPath, CancellationToken token)
        {
            ProcessStartInfo vspipePsi = new()
            {
                FileName = _vspipePath,
                WorkingDirectory = _workDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };
            vspipePsi.ArgumentList.Add(_scriptPath);
            vspipePsi.ArgumentList.Add("-o");
            vspipePsi.ArgumentList.Add(outputIndex.ToString());
            vspipePsi.ArgumentList.Add("-s");
            vspipePsi.ArgumentList.Add(CurrentFrame.ToString());
            vspipePsi.ArgumentList.Add("-e");
            vspipePsi.ArgumentList.Add(CurrentFrame.ToString());

            foreach (string arg in PreviewPipeline.SplitArgs(_vspipeY4mArg))
                vspipePsi.ArgumentList.Add(arg);

            vspipePsi.ArgumentList.Add("-");

            using Process vspipeProcess = new() { StartInfo = vspipePsi, EnableRaisingEvents = true };

            ProcessStartInfo ffmpegPsi = new()
            {
                FileName = _ffmpegPath,
                WorkingDirectory = _workDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };
            ffmpegPsi.ArgumentList.Add("-hide_banner");
            ffmpegPsi.ArgumentList.Add("-y");
            ffmpegPsi.ArgumentList.Add("-i");
            ffmpegPsi.ArgumentList.Add("-");
            ffmpegPsi.ArgumentList.Add("-vframes");
            ffmpegPsi.ArgumentList.Add("1");
            ffmpegPsi.ArgumentList.Add("-c:v");
            ffmpegPsi.ArgumentList.Add("png");
            ffmpegPsi.ArgumentList.Add(outputPngPath);

            using Process ffmpegProcess = new() { StartInfo = ffmpegPsi, EnableRaisingEvents = true };

            _currentVspipeProcess = vspipeProcess;
            _currentFfmpegProcess = ffmpegProcess;

            try
            {
                vspipeProcess.Start();
                ffmpegProcess.Start();

                Task pipeTask = vspipeProcess.StandardOutput.BaseStream.CopyToAsync(
                    ffmpegProcess.StandardInput.BaseStream, 81920, token);

                string vspipeStderr = await vspipeProcess.StandardError.ReadToEndAsync();
                string ffmpegStderr = await ffmpegProcess.StandardError.ReadToEndAsync();

                await Task.WhenAll(vspipeProcess.WaitForExitAsync(token), pipeTask).ConfigureAwait(false);
                ffmpegProcess.StandardInput.Close();

                await ffmpegProcess.WaitForExitAsync(token).ConfigureAwait(false);

                if (vspipeProcess.ExitCode != 0)
                {
                    string msg = string.IsNullOrWhiteSpace(vspipeStderr)
                        ? $"vspipe exit code {vspipeProcess.ExitCode}"
                        : PreviewPipeline.TrimProcessMessage(vspipeStderr);
                    throw new InvalidOperationException(msg);
                }

                if (ffmpegProcess.ExitCode != 0)
                {
                    string msg = string.IsNullOrWhiteSpace(ffmpegStderr)
                        ? $"ffmpeg exit code {ffmpegProcess.ExitCode}"
                        : PreviewPipeline.TrimProcessMessage(ffmpegStderr);
                    throw new InvalidOperationException(msg);
                }
            }
            catch (OperationCanceledException)
            {
                PreviewPipeline.TryKillProcess(vspipeProcess);
                PreviewPipeline.TryKillProcess(ffmpegProcess);
                throw;
            }
        }

        private static void EnsureFileExists(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Preview frame file missing", path);
        }

        private void TryKillCurrentProcess()
        {
            if (_currentVspipeProcess != null)
                PreviewPipeline.TryKillProcess(_currentVspipeProcess);
            if (_currentFfmpegProcess != null)
                PreviewPipeline.TryKillProcess(_currentFfmpegProcess);
        }

        public override void Dispose()
        {
            _previewCts?.Cancel();
            TryKillCurrentProcess();
            _previewCts?.Dispose();

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
