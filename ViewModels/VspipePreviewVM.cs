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
        private readonly string _vspipeY4mArg;
        private readonly string _workDirectory;
        private readonly int _totalFrames;
        private readonly string _scriptPath;

        public string VideoFilename { get; }

        private CancellationTokenSource? _previewCts;
        private Process? _currentVspipeProcess;
        private bool _isDisposed;

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

        public int TotalFrames => _totalFrames;

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
            string vspipeY4mArg,
            string scriptContent,
            string videoFilename,
            int totalFrames)
        {
            _vspipePath = vspipePath;
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
            if (_isDisposed) return;

            if (IsBusy)
            {
                CancelPreview();
                return;
            }

            _ = GeneratePreviewAsync();
        }

        private async Task GeneratePreviewAsync()
        {
            if (_isDisposed) return;

            CancellationTokenSource? previousCts = _previewCts;
            CancellationTokenSource cts = new();
            _previewCts = cts;
            previousCts?.Dispose();
            CancellationToken token = cts.Token;

            try
            {
                IsBusy = true;
                string sourcePath = Path.Combine(_workDirectory, "output-0.y4m");
                string filteredPath = Path.Combine(_workDirectory, "output-1.y4m");

                StatusText = "Extracting frame from output 0 (original)...";
                await RunVspipeY4mAsync(0, sourcePath, token);
                EnsureFileExists(sourcePath);

                StatusText = "Extracting frame from output 1 (filtered)...";
                await RunVspipeY4mAsync(1, filteredPath, token);
                EnsureFileExists(filteredPath);

                SourceImage = Y4mFrameReader.LoadFirstFrame(sourcePath);
                EncodedImage = Y4mFrameReader.LoadFirstFrame(filteredPath);

                StatusText = $"Frame {CurrentFrame} rendered";
            }
            catch (OperationCanceledException)
            {
                if (!_isDisposed) StatusText = "Cancelled";
            }
            catch (ObjectDisposedException) when (_isDisposed) {}
            catch (Exception ex)
            {
                if (!_isDisposed) StatusText = ex.Message;
            }
            finally
            {
                _currentVspipeProcess = null;
                if (ReferenceEquals(_previewCts, cts))
                    _previewCts = null;
                cts.Dispose();

                if (_isDisposed) DeleteWorkDirectory();
                else IsBusy = false;
            }
        }

        private async Task RunVspipeY4mAsync(int outputIndex, string outputY4mPath, CancellationToken token)
        {
            ProcessStartInfo vspipePsi = new()
            {
                FileName = _vspipePath,
                WorkingDirectory = _workDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };
            vspipePsi.ArgumentList.Add(_scriptPath);
            vspipePsi.ArgumentList.Add("-o");
            vspipePsi.ArgumentList.Add(outputIndex.ToString(CultureInfo.InvariantCulture));
            vspipePsi.ArgumentList.Add("-s");
            vspipePsi.ArgumentList.Add(CurrentFrame.ToString(CultureInfo.InvariantCulture));
            vspipePsi.ArgumentList.Add("-e");
            vspipePsi.ArgumentList.Add(CurrentFrame.ToString(CultureInfo.InvariantCulture));

            foreach (string arg in PreviewPipeline.SplitArgs(_vspipeY4mArg))
                vspipePsi.ArgumentList.Add(arg);

            vspipePsi.ArgumentList.Add(outputY4mPath);

            using Process vspipeProcess = new() { StartInfo = vspipePsi, EnableRaisingEvents = true };
            _currentVspipeProcess = vspipeProcess;

            try
            {
                vspipeProcess.Start();
                Task<string> vspipeStderrTask = vspipeProcess.StandardError.ReadToEndAsync(token);

                await vspipeProcess.WaitForExitAsync(token).ConfigureAwait(false);
                string vspipeStderr = await vspipeStderrTask.ConfigureAwait(false);

                if (vspipeProcess.ExitCode != 0)
                {
                    string msg = string.IsNullOrWhiteSpace(vspipeStderr)
                        ? $"vspipe exit code {vspipeProcess.ExitCode}"
                        : PreviewPipeline.TrimProcessMessage(vspipeStderr);
                    throw new InvalidOperationException(msg);
                }
            }
            catch (OperationCanceledException)
            {
                PreviewPipeline.TryKillProcess(vspipeProcess);
                throw;
            }
        }

        private static void EnsureFileExists(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Preview frame file missing", path);
        }

        private void CancelPreview()
        {
            try { _previewCts?.Cancel(); }
            catch (ObjectDisposedException) { }
            TryKillCurrentProcess();
        }

        private void TryKillCurrentProcess()
        {
            if (_currentVspipeProcess != null)
                PreviewPipeline.TryKillProcess(_currentVspipeProcess);
        }

        private void DeleteWorkDirectory()
        {
            try
            {
                if (Directory.Exists(_workDirectory))
                    Directory.Delete(_workDirectory, recursive: true);
            }
            catch {}
        }

        public override void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            CancelPreview();
            if (!IsBusy) DeleteWorkDirectory();

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
