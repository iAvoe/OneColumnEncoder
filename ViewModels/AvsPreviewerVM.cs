using System.IO;

namespace OneColumnEncoder.ViewModels;

public class AvsPreviewerVM : BaseVM, IPreviewViewModel
{
    private enum PreviewTextState { Ready, ExtractingSource, ExtractingFiltered, FrameRendered, Cancelled, ScriptError, Custom }

    private readonly ModalNavS _modalNavS;
    private readonly string _workDirectory;
    private readonly int _totalFrames;
    private readonly string _sourceScript;
    private readonly Func<string, string>? _buildPreviewScript;
    private readonly Func<string, string>? _buildSourceScript;
    private readonly SynchronizationContext? _uiContext;
    private readonly Lock _logLock = new();
    private readonly StringBuilder _logBuilder = new();
    private CancellationTokenSource? _previewCts;
    private Process? _currentProcess;
    private bool _isDisposed;
    private bool _suppressSwitch;
    private bool _logReady;
    private PreviewTextState _statusState = PreviewTextState.Ready;
    private string? _statusDetail;
    private AvsPreviewerLangProvider _lang = new(UILangProvider.Current.LanguageCode);

    public AvsPreviewerLangProvider Lang { get => _lang; private set => SetProperty(ref _lang, value); }
    private string _videoFilename;
    public string VideoFilename { get => _videoFilename; private set => SetProperty(ref _videoFilename, value); }
    public ObservableCollection<PreviewSourceItem> PreviewSources { get; } = [];
    public bool IsAvsPreview => true;
    public ObservableCollection<AviSynthPreviewToolItem> AviSynthPreviewTools { get; } = [];
    private AviSynthPreviewToolItem? _selectedPreviewTool;
    public AviSynthPreviewToolItem? SelectedPreviewTool
    {
        get => _selectedPreviewTool;
        set
        {
            if (!SetProperty(ref _selectedPreviewTool, value) || value == null) return;
            CancelPreview();
            IsBusy = false;
            SourceImage = null;
            EncodedImage = null;
            SetReadyTexts();
        }
    }
    private PreviewSourceItem? _selectedPreviewSource;
    public PreviewSourceItem? SelectedPreviewSource
    {
        get => _selectedPreviewSource;
        set { if (SetProperty(ref _selectedPreviewSource, value) && value != null && !_suppressSwitch) SwitchSource(value.FullPath); }
    }
    private ImageSource? _sourceImage;
    public ImageSource? SourceImage { get => _sourceImage; private set => SetProperty(ref _sourceImage, value); }
    private ImageSource? _encodedImage;
    public ImageSource? EncodedImage { get => _encodedImage; private set => SetProperty(ref _encodedImage, value); }
    private int _currentFrame;
    public int CurrentFrame
    {
        get => _currentFrame;
        set { int clamped = Math.Clamp(value, 0, TotalFrames - 1); if (SetProperty(ref _currentFrame, clamped)) OnPropertyChanged(nameof(PreviewPositionSeconds)); }
    }
    public int TotalFrames => _totalFrames;
    private bool _isBusy;
    public bool IsBusy { get => _isBusy; private set { if (!SetProperty(ref _isBusy, value)) return; OnPropertyChanged(nameof(IsIdle)); UpdatePreviewButtonText(); } }
    public bool IsIdle => !IsBusy;
    private string _statusText = "";
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    private string _logText = "";
    public string AvsVsLogText { get => _logText; private set => SetProperty(ref _logText, value); }
    private string _previewButtonText = "";
    public string PreviewButtonText { get => _previewButtonText; private set => SetProperty(ref _previewButtonText, value); }
    private int _zoomPercent = 100;
    public int ZoomPercent { get => _zoomPercent; private set => SetProperty(ref _zoomPercent, value); }
    public int PreviewPositionSeconds { get => CurrentFrame; set => CurrentFrame = value; }
    private int _maxPositionSeconds = 1;
    public int MaxPositionSeconds { get => _maxPositionSeconds; private set => SetProperty(ref _maxPositionSeconds, value); }
    public ObservableCollection<string> PositionTickLabels { get; } = [];
    public ActionCmd PreviewCommand { get; }
    public ActionCmd InspectFrameDataCommand { get; }
    private bool _isFitMode;
    public bool IsFitMode => _isFitMode;

    public AvsPreviewerVM(ModalNavS modalNavS, string toolPath, string scriptContent, string srcPath, int totalFrames,
        Func<string, string>? buildPreviewScript = null, IEnumerable<string>? queueFilePaths = null,
        Func<string, string>? buildSourceScript = null, IEnumerable<string>? toolPaths = null)
    {
        _modalNavS = modalNavS;
        _sourceScript = scriptContent;
        _buildPreviewScript = buildPreviewScript;
        _buildSourceScript = buildSourceScript;
        _uiContext = SynchronizationContext.Current;
        _videoFilename = Path.GetFileName(srcPath);
        _totalFrames = totalFrames > 0 ? totalFrames : 1;
        MaxPositionSeconds = _totalFrames - 1;
        _currentFrame = MaxPositionSeconds / 2;
        for (int i = 0; i <= 4; i++) PositionTickLabels.Add(Math.Round(Math.Max(1, MaxPositionSeconds) * i / 4d).ToString(CultureInfo.InvariantCulture));
        _workDirectory = PreviewPipeline.CreateWorkDirectory("1cenc-avs-preview-");
        string scriptPath = Path.Combine(_workDirectory, "preview.avs");
        File.WriteAllText(scriptPath, scriptContent);

        IEnumerable<string> previewToolPaths = toolPaths ?? [toolPath];
        foreach (string path in previewToolPaths.Where(path => !string.IsNullOrWhiteSpace(path)))
            AviSynthPreviewTools.Add(new AviSynthPreviewToolItem(path));
        SelectedPreviewTool = AviSynthPreviewTools.FirstOrDefault(tool =>
            tool.FullPath.Equals(toolPath, StringComparison.OrdinalIgnoreCase))
            ?? AviSynthPreviewTools.FirstOrDefault();

        _suppressSwitch = true;
        IEnumerable<string> paths = queueFilePaths ?? [srcPath];
        foreach (string path in paths) PreviewSources.Add(new PreviewSourceItem(path));
        SelectedPreviewSource = PreviewSources.FirstOrDefault();
        _suppressSwitch = false;
        SetReadyTexts();
        PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
        InspectFrameDataCommand = new ActionCmd(_ => ShowFrameDataDebug());
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public void SetZoomPercent(int percent) => ZoomPercent = Math.Max(1, percent);
    public void SetFitMode(bool isFitMode) => _isFitMode = isFitMode;
    private void PreviewOrCancel() { if (_isDisposed) return; if (IsBusy) { CancelPreview(); return; } _ = GeneratePreviewAsync(); }

    private async Task GeneratePreviewAsync()
    {
        CancellationTokenSource cts = new();
        CancellationTokenSource? previous = _previewCts;
        _previewCts = cts;
        previous?.Dispose();
        try
        {
            RefreshPreviewScript();
            IsBusy = true;
            ResetLog();
            string sourcePath = Path.Combine(_workDirectory, "output-0.y4m");
            string filteredPath = Path.Combine(_workDirectory, "output-1.y4m");
            SetStatus(PreviewTextState.ExtractingSource);
            await RunToolAsync(Path.Combine(_workDirectory, "source.avs"), sourcePath, cts.Token);
            PreviewPipeline.EnsureFileExists(sourcePath, Lang.PreviewFrameFileMissing);
            SetStatus(PreviewTextState.ExtractingFiltered);
            await RunToolAsync(Path.Combine(_workDirectory, "preview.avs"), filteredPath, cts.Token);
            PreviewPipeline.EnsureFileExists(filteredPath, Lang.PreviewFrameFileMissing);
            SourceImage = Y4mFrameReader.LoadFirstFrame(sourcePath);
            EncodedImage = Y4mFrameReader.LoadFirstFrame(filteredPath);
            SetStatus(PreviewTextState.FrameRendered);
        }
        catch (OperationCanceledException) { if (!_isDisposed) { SetStatus(PreviewTextState.Cancelled); AppendLog(Lang.StatusCancelled); } }
        catch (Exception ex) { if (!_isDisposed) { SetStatus(PreviewTextState.Custom, ex.Message); AppendLog(Lang.LogErrorPrefix + ex.Message); } }
        finally
        {
            _currentProcess = null;
            if (ReferenceEquals(_previewCts, cts)) _previewCts = null;
            cts.Dispose();
            if (_isDisposed) DeleteWorkDirectory(); else IsBusy = false;
        }
    }

    private async Task RunToolAsync(string scriptPath, string outputPath, CancellationToken token)
    {
        string toolPath = SelectedPreviewTool?.FullPath
            ?? throw new InvalidOperationException("AviSynth preview tool missing.");
        string toolName = Path.GetFileNameWithoutExtension(toolPath);
        ProcessStartInfo psi = new() { FileName = toolPath, WorkingDirectory = _workDirectory, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        string[] args = toolName.Equals("avs2yuv", StringComparison.OrdinalIgnoreCase)
            ? PreviewPipeline.BuildAvs2yuvY4mArgs(scriptPath, CurrentFrame)
            : PreviewPipeline.BuildAvs2pipemodY4mArgs(scriptPath, CurrentFrame);
        foreach (string arg in args) psi.ArgumentList.Add(arg);
        using Process process = new() { StartInfo = psi, EnableRaisingEvents = true };
        _currentProcess = process;
        using CancellationTokenRegistration registration = token.Register(() => PreviewPipeline.TryKillProcess(process));
        process.Start();
        using FileStream output = File.Create(outputPath);
        Task outputTask = process.StandardOutput.BaseStream.CopyToAsync(output, token);
        Task<string> errorTask = process.StandardError.ReadToEndAsync(token);
        await process.WaitForExitAsync(token);
        await Task.WhenAll(outputTask, errorTask);
        string error = await errorTask;
        if (!string.IsNullOrWhiteSpace(error)) AppendLog(error, toolName);
        if (process.ExitCode != 0)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Lang.LogExitCode, toolName, process.ExitCode);
            AppendLog(message, toolName);
            throw new InvalidOperationException(message);
        }
    }

    private void RefreshPreviewScript()
    {
        string? path = SelectedPreviewSource?.FullPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        File.WriteAllText(Path.Combine(_workDirectory, "source.avs"), _buildSourceScript?.Invoke(path) ?? _sourceScript);
        File.WriteAllText(Path.Combine(_workDirectory, "preview.avs"), _buildPreviewScript?.Invoke(path) ?? _sourceScript);
    }
    private void SwitchSource(string path) { CancelPreview(); IsBusy = false; SourceImage = null; EncodedImage = null; CurrentFrame = 0; VideoFilename = Path.GetFileName(path); try { RefreshPreviewScript(); SetReadyTexts(); } catch (Exception ex) { SetStatus(PreviewTextState.ScriptError, ex.Message); } }
    private void CancelPreview() { try { _previewCts?.Cancel(); } catch (ObjectDisposedException) { } if (_currentProcess != null) PreviewPipeline.TryKillProcess(_currentProcess); }
    private void ResetLog() { lock (_logLock) _logBuilder.Clear(); _logReady = false; RunOnUi(() => AvsVsLogText = string.Empty); }
    private void AppendLog(string text, string? toolName = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        string[] lines = text.Replace("\r", "", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string activeToolName = toolName ?? Path.GetFileNameWithoutExtension(SelectedPreviewTool?.FullPath ?? string.Empty);
        string[] filteredLines = [.. lines.Where(line => IsUsefulAvsLogLine(activeToolName, line))];
        if (filteredLines.Length == 0) return;

        lock (_logLock)
        {
            foreach (string line in filteredLines)
            {
                if (_logBuilder.Length > 0) _logBuilder.Append('\n');
                _logBuilder.Append(line);
            }
            string snapshot = _logBuilder.ToString();
            RunOnUi(() => AvsVsLogText = snapshot);
        }
        _logReady = false;
    }

    private static bool IsUsefulAvsLogLine(string toolName, string line)
    {
        if (toolName.Equals("avs2pipemod", StringComparison.OrdinalIgnoreCase))
            return line.Contains("avs2pipemod", StringComparison.OrdinalIgnoreCase) &&
                line.Contains("writing 1 frames", StringComparison.OrdinalIgnoreCase) ||
                IsAvsPreviewStatusLogLine(line);

        if (!toolName.Equals("avs2yuv", StringComparison.OrdinalIgnoreCase))
            return !line.Contains("Creating lwi index file", StringComparison.OrdinalIgnoreCase);

        return
        line.StartsWith("Script file:", StringComparison.OrdinalIgnoreCase) ||
        IsAvsPreviewStatusLogLine(line);
    }

    private static bool IsAvsPreviewStatusLogLine(string line) =>
        line.StartsWith("Error:", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("[error]", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("错误：", StringComparison.Ordinal) ||
        line.Contains("exit code", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("退出码", StringComparison.Ordinal) ||
        line.Contains("Cancelled", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("已取消", StringComparison.Ordinal);
    private void SetReadyTexts() { SetStatus(PreviewTextState.Ready); _logReady = true; RunOnUi(() => AvsVsLogText = Lang.StatusReady); UpdatePreviewButtonText(); }
    private void SetStatus(PreviewTextState state, string? detail = null) { _statusState = state; _statusDetail = detail; RunOnUi(() => StatusText = BuildStatusText()); }
    private string BuildStatusText() => _statusState switch { PreviewTextState.Ready => Lang.StatusReady, PreviewTextState.ExtractingSource => Lang.StatusExtractingSource, PreviewTextState.ExtractingFiltered => Lang.StatusExtractingFiltered, PreviewTextState.FrameRendered => string.Format(CultureInfo.CurrentCulture, Lang.StatusFrameRendered, CurrentFrame), PreviewTextState.Cancelled => Lang.StatusCancelled, PreviewTextState.ScriptError => string.Format(CultureInfo.CurrentCulture, Lang.StatusScriptError, _statusDetail ?? ""), PreviewTextState.Custom => _statusDetail ?? "", _ => Lang.StatusReady };
    private void UpdatePreviewButtonText() => PreviewButtonText = IsBusy ? Lang["Cancel"] : Lang["Preview"];
    private void RunOnUi(Action action) { if (_uiContext == null || SynchronizationContext.Current == _uiContext) action(); else _uiContext.Post(static state => ((Action)state!).Invoke(), action); }
    private void ShowFrameDataDebug() { StringBuilder sb = new(); sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugSourceVideo, SelectedPreviewSource?.FullPath ?? Lang.NoneText)); sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugToolPath, SelectedPreviewTool?.FullPath ?? Lang.NoneText)); sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugTotalFrames, TotalFrames)); sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugMaxPositionSeconds, MaxPositionSeconds)); sb.AppendLine(); sb.AppendLine(Lang.DebugPreviewScript); sb.AppendLine(_sourceScript); new OpenDebugModalCmd(_modalNavS, AvsPreviewerLangProvider.DebugWindowTitle, sb.ToString()).Execute(null); }
    private void DeleteWorkDirectory() { if (Directory.Exists(_workDirectory)) PreviewPipeline.DeleteDirectoryQuietly(_workDirectory); }
    public override void Dispose() { if (_isDisposed) return; _isDisposed = true; UILangProvider.CurrentChanged -= OnLanguageChanged; CancelPreview(); if (!IsBusy) DeleteWorkDirectory(); base.Dispose(); GC.SuppressFinalize(this); }
    private void OnLanguageChanged() { Lang = new AvsPreviewerLangProvider(UILangProvider.Current.LanguageCode); UpdatePreviewButtonText(); RunOnUi(() => { StatusText = BuildStatusText(); if (_logReady) AvsVsLogText = Lang.StatusReady; }); }
}
