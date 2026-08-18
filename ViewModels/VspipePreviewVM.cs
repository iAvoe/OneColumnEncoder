using System.IO;

namespace OneColumnEncoder.ViewModels;

public class PreviewSourceItem
{
    public string FullPath { get; }
    public string VideoFilename { get; }
    public string Title => VideoFilename;
    public PreviewSourceItem(string fullPath)
    {
        FullPath = fullPath;
        VideoFilename = Path.GetFileName(fullPath);
    }
}

public class VspipePreviewVM : BaseVM
{
    private readonly ModalNavS _modalNavS;
    private readonly string _vspipePath;
    private readonly string _vspipeY4mArg;
    private readonly string _workDirectory;
    private readonly int _totalFrames;
    private readonly string _scriptPath;
    private readonly Func<string, string>? _buildPreviewScript;
    private bool _suppressSwitch;
    private readonly string _scriptContent;

    private string _videoFilename;
    public string VideoFilename
    {
        get => _videoFilename;
        private set => SetProperty(ref _videoFilename, value);
    }

    public ObservableCollection<PreviewSourceItem> PreviewSources { get; } = [];

    private PreviewSourceItem? _selectedPreviewSource;
    public PreviewSourceItem? SelectedPreviewSource
    {
        get => _selectedPreviewSource;
        set
        {
            if (SetProperty(ref _selectedPreviewSource, value) && value != null && !_suppressSwitch)
                SwitchSource(value.FullPath);
        }
    }

    private CancellationTokenSource? _previewCts;
    private Process? _currentVspipeProcess;
    private bool _isDisposed;
    private readonly object _vspipeLogLock = new();
    private readonly List<string> _vspipeLogLines = [];

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

    private string _vspipeLogText = "";
    public string VspipeLogText
    {
        get => _vspipeLogText;
        private set => SetProperty(ref _vspipeLogText, value);
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
    public ActionCmd InspectFrameDataCommand { get; }

    public VspipePreviewVM(
        ModalNavS modalNavS,
        string vspipePath,
        string vspipeY4mArg,
        string scriptContent,
        string srcPath,
        int totalFrames,
        Func<string, string>? buildPreviewScript = null,
        IEnumerable<string>? queueFilePaths = null)
    {
        _modalNavS = modalNavS;
        _vspipePath = vspipePath;
        _vspipeY4mArg = vspipeY4mArg;
        _buildPreviewScript = buildPreviewScript;
        _scriptContent = scriptContent;
        _videoFilename = Path.GetFileName(srcPath);
_totalFrames = totalFrames > 0 ? totalFrames : 1;
        MaxPositionSeconds = _totalFrames - 1;
        _currentFrame = Math.Min(MaxPositionSeconds, Math.Max(0, MaxPositionSeconds / 2));
        BuildPositionTickLabels(MaxPositionSeconds);

        _workDirectory = PreviewPipeline.CreateWorkDirectory("1cenc-vpy-preview-");

        _scriptPath = Path.Combine(_workDirectory, "preview.vpy");
        File.WriteAllText(_scriptPath, scriptContent);

        _suppressSwitch = true;
        if (queueFilePaths != null)
        {
            foreach (string path in queueFilePaths)
                PreviewSources.Add(new PreviewSourceItem(path));
            SelectedPreviewSource = PreviewSources.FirstOrDefault();
        }
        else
        {
            PreviewSources.Add(new PreviewSourceItem(srcPath));
            SelectedPreviewSource = PreviewSources[0];
        }
        _suppressSwitch = false;

        StatusText = "Ready";
        VspipeLogText = "Ready";
        PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
        InspectFrameDataCommand = new ActionCmd(_ => ShowFrameDataDebug());
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
            RefreshPreviewScript();
            IsBusy = true;
            ResetVspipeLog();
            string srcPath = Path.Combine(_workDirectory, "output-0.y4m");
            string filteredPath = Path.Combine(_workDirectory, "output-1.y4m");

            StatusText = "Extracting frame from output 0 (original)...";
            await RunVspipeY4mAsync(0, srcPath, token);
            PreviewPipeline.EnsureFileExists(srcPath, "Preview frame file missing");

            StatusText = "Extracting frame from output 1 (filtered)...";
            await RunVspipeY4mAsync(1, filteredPath, token);
            PreviewPipeline.EnsureFileExists(filteredPath, "Preview frame file missing");

            SourceImage = Y4mFrameReader.LoadFirstFrame(srcPath);
            EncodedImage = Y4mFrameReader.LoadFirstFrame(filteredPath);

            StatusText = $"Frame {CurrentFrame} rendered";
        }
        catch (OperationCanceledException)
        {
            if (!_isDisposed)
            {
                StatusText = "Cancelled";
                AppendVspipeLogLine("Cancelled");
            }
        }
        catch (ObjectDisposedException) when (_isDisposed) { }
        catch (Exception ex)
        {
            if (!_isDisposed)
            {
                StatusText = ex.Message;
                AppendVspipeLogLine($"Error: {ex.Message}");
            }
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
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
            CreateNoWindow = true
        };
        foreach (string arg in PreviewPipeline.BuildVspipeY4mArgs(
            _scriptPath,
            outputIndex,
            CurrentFrame,
            _vspipeY4mArg,
            outputY4mPath))
            vspipePsi.ArgumentList.Add(arg);

        using Process vspipeProcess = new() { StartInfo = vspipePsi, EnableRaisingEvents = true };
        _currentVspipeProcess = vspipeProcess;

        using CancellationTokenRegistration killRegistration = token.Register(() => PreviewPipeline.TryKillProcess(vspipeProcess));
        try
        {
            AppendVspipeLogLine($"vspipe output {outputIndex}");
            vspipeProcess.Start();
            Task stdoutTask = ReadVspipeStreamAsync(vspipeProcess.StandardOutput, token);
            Task stderrTask = ReadVspipeStreamAsync(vspipeProcess.StandardError, token);

            await vspipeProcess.WaitForExitAsync(token);
            await Task.WhenAll(stdoutTask, stderrTask);

            if (vspipeProcess.ExitCode != 0)
            {
                string msg = $"vspipe exit code {vspipeProcess.ExitCode}";
                AppendVspipeLogLine(msg);
                throw new InvalidOperationException(msg);
            }
        }
        catch (OperationCanceledException)
        {
            PreviewPipeline.TryKillProcess(vspipeProcess);
            throw;
        }
    }

    private void ResetVspipeLog()
    {
        lock (_vspipeLogLock)
        {
            _vspipeLogLines.Clear();
        }

        RunOnUi(() => VspipeLogText = string.Empty);
    }

    private void AppendVspipeLogLine(string? line, bool overwritePreviousLine = false)
    {
        if (string.IsNullOrWhiteSpace(line)) return;

        string normalized = line.Replace("\0", string.Empty, StringComparison.Ordinal).TrimEnd();
        if (string.IsNullOrWhiteSpace(normalized)) return;

        string snapshot;
        lock (_vspipeLogLock)
        {
            if (overwritePreviousLine && _vspipeLogLines.Count > 0)
                _vspipeLogLines.RemoveAt(_vspipeLogLines.Count - 1);

            _vspipeLogLines.Add(normalized);
            snapshot = string.Join(Environment.NewLine, _vspipeLogLines);
        }

        RunOnUi(() => VspipeLogText = snapshot);
    }

    private async Task ReadVspipeStreamAsync(StreamReader reader, CancellationToken token)
    {
        try
        {
            char[] buffer = new char[4096];
            StringBuilder lineBuilder = new();
            string? pendingCarriageReturnLine = null;
            bool previousWasCarriageReturnUpdate = false;

            while (!token.IsCancellationRequested)
            {
                int charsRead = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
                if (charsRead == 0) break;

                for (int i = 0; i < charsRead; i++)
                {
                    char ch = buffer[i];
                    if (pendingCarriageReturnLine != null)
                    {
                        if (ch == '\n')
                        {
                            AppendVspipeLogLine(pendingCarriageReturnLine, overwritePreviousLine: false);
                            pendingCarriageReturnLine = null;
                            previousWasCarriageReturnUpdate = false;
                            continue;
                        }

                        AppendVspipeLogLine(pendingCarriageReturnLine, overwritePreviousLine: previousWasCarriageReturnUpdate);
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
                        AppendVspipeLogLine(lineBuilder.ToString(), overwritePreviousLine: false);
                        lineBuilder.Clear();
                        previousWasCarriageReturnUpdate = false;
                        continue;
                    }

                    lineBuilder.Append(ch);
                }
            }

            if (pendingCarriageReturnLine != null)
                AppendVspipeLogLine(pendingCarriageReturnLine, overwritePreviousLine: previousWasCarriageReturnUpdate);
            if (lineBuilder.Length > 0)
                AppendVspipeLogLine(lineBuilder.ToString(), overwritePreviousLine: false);
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
    }

    private static void RunOnUi(Action action)
    {
        System.Windows.Threading.Dispatcher? dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _ = dispatcher.InvokeAsync(action);
    }

    private void SwitchSource(string newPath)
    {
        CancelPreview();
        IsBusy = false;
        SourceImage = null;
        EncodedImage = null;
        CurrentFrame = 0;
        VideoFilename = Path.GetFileName(newPath);

        if (_buildPreviewScript != null)
        {
            try
            {
                string script = _buildPreviewScript(newPath);
                File.WriteAllText(_scriptPath, script);
            }
            catch (Exception ex)
            {
                StatusText = $"Script error: {ex.Message}";
                AppendVspipeLogLine($"Error: Script error: {ex.Message}");
                return;
            }
        }

        StatusText = "Ready";
        VspipeLogText = "Ready";
    }

    private void RefreshPreviewScript()
    {
        if (_buildPreviewScript == null)
            return;

        string? srcPath = SelectedPreviewSource?.FullPath;
        if (string.IsNullOrWhiteSpace(srcPath))
            return;

        string script = _buildPreviewScript(srcPath);
        File.WriteAllText(_scriptPath, script);
    }

    private void CancelPreview()
    {
        try { _previewCts?.Cancel(); }
        catch (ObjectDisposedException) { }
        TryKillCurrentProcess();
    }

    private void ShowFrameDataDebug()
    {
        StringBuilder sb = new();
        sb.AppendLine($"Source video: {SelectedPreviewSource?.FullPath ?? "<none>"}");
        sb.AppendLine($"vspipe path: {_vspipePath}");
        sb.AppendLine($"vspipe y4m arg: {_vspipeY4mArg}");
        sb.AppendLine($"TotalFrames: {TotalFrames}");
        sb.AppendLine($"MaxPositionSeconds: {MaxPositionSeconds}");
        sb.AppendLine();
        sb.AppendLine("Preview script:");
        sb.AppendLine(_scriptContent);

        new OpenDebugModalCmd(_modalNavS, "VapourSynth Preview frame data", sb.ToString()).Execute(null);
    }

    private void TryKillCurrentProcess()
    {
        if (_currentVspipeProcess != null)
            PreviewPipeline.TryKillProcess(_currentVspipeProcess);
    }

    private void DeleteWorkDirectory()
    {
        PreviewPipeline.DeleteDirectoryQuietly(_workDirectory);
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
