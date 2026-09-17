using System.IO;

namespace OneColumnEncoder.ViewModels;

public class FFmpegPreviewerVM : BaseVM, IPreviewViewModel
{
    private enum PreviewTextState
    {
        Ready,
        ExtractingSource,
        ExtractingFiltered,
        FrameRendered,
        Cancelled,
        Custom,
    }

    private readonly ModalNavS _modalNavS;
    private readonly string _ffmpegPath;
    private readonly Func<string?> _getFilterArgs;
    private readonly double _frameRate;
    private readonly int _totalFrames;
    private readonly SynchronizationContext? _uiContext;
    private readonly bool _suppressSwitch;
    private bool _isDisposed;
    private CancellationTokenSource? _previewCts;
    private PreviewTextState _statusState = PreviewTextState.Ready;
    private string? _statusDetail;
    private VpyPreviewerLangProvider _lang = new(UILangProvider.Current.LanguageCode);

    public VpyPreviewerLangProvider Lang { get => _lang; private set => SetProperty(ref _lang, value); }
    public static bool IsAvsPreview => false;
    public ObservableCollection<AviSynthPreviewToolItem> AviSynthPreviewTools { get; } = [];
    public AviSynthPreviewToolItem? SelectedPreviewTool { get; set; }
    public ObservableCollection<PreviewSourceItem> PreviewSources { get; } = [];
    public ObservableCollection<string> PositionTickLabels { get; } = [];
    public ActionCmd PreviewCommand { get; }
    public ActionCmd InspectFrameDataCommand { get; }

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

    private string _videoFilename;
    public string VideoFilename { get => _videoFilename; private set => SetProperty(ref _videoFilename, value); }

    private ImageSource? _sourceImage;
    public ImageSource? SourceImage { get => _sourceImage; private set => SetProperty(ref _sourceImage, value); }

    private ImageSource? _encodedImage;
    public ImageSource? EncodedImage { get => _encodedImage; private set => SetProperty(ref _encodedImage, value); }

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
            UpdatePreviewButtonText();
        }
    }

    public bool IsIdle => !IsBusy;

    private string _statusText = string.Empty;
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }

    private string _avsVsLogText = string.Empty;
    public string FrameServerLogText { get => _avsVsLogText; private set => SetProperty(ref _avsVsLogText, value); }

    private string _previewButtonText = string.Empty;
    public string PreviewButtonText { get => _previewButtonText; private set => SetProperty(ref _previewButtonText, value); }

    private int _zoomPercent = 100;
    public int ZoomPercent { get => _zoomPercent; private set => SetProperty(ref _zoomPercent, value); }

    public int PreviewPositionSeconds { get => CurrentFrame; set => CurrentFrame = value; }

    private int _maxPositionSeconds = 1;
    public int MaxPositionSeconds { get => _maxPositionSeconds; private set => SetProperty(ref _maxPositionSeconds, value); }

    private bool _isFitMode;
    public bool IsFitMode => _isFitMode;

    public FFmpegPreviewerVM(
        ModalNavS modalNavS,
        string ffmpegPath,
        Func<string?> getFilterArgs,
        string srcPath,
        int totalFrames,
        string? sourceFfprobeJson = null,
        IEnumerable<string>? queueFilePaths = null)
    {
        _modalNavS = modalNavS;
        _ffmpegPath = ffmpegPath;
        _getFilterArgs = getFilterArgs;
        _uiContext = SynchronizationContext.Current;
        _videoFilename = Path.GetFileName(srcPath);
        _totalFrames = totalFrames > 0 ? totalFrames : 1;
        _frameRate = Math.Max(1d, FFProbeSourceStatsReader.Read(sourceFfprobeJson ?? string.Empty).FrameRate);
        MaxPositionSeconds = _totalFrames - 1;
        _currentFrame = Math.Min(MaxPositionSeconds, Math.Max(0, MaxPositionSeconds / 2));
        BuildPositionTickLabels(MaxPositionSeconds);

        _suppressSwitch = true;
        IEnumerable<string> paths = queueFilePaths ?? [srcPath];
        foreach (string path in paths.Where(path => !string.IsNullOrWhiteSpace(path)))
            PreviewSources.Add(new PreviewSourceItem(path));
        SelectedPreviewSource = PreviewSources.FirstOrDefault();
        _suppressSwitch = false;

        SetReadyTexts();
        PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
        InspectFrameDataCommand = new ActionCmd(_ => ShowFrameDataDebug());
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private void BuildPositionTickLabels(int maxFrame)
    {
        PositionTickLabels.Clear();
        double safeMax = Math.Max(1d, maxFrame);
        for (int i = 0; i <= 4; i++)
            PositionTickLabels.Add(Math.Round(safeMax * i / 4d).ToString(CultureInfo.InvariantCulture));
    }

    public void SetZoomPercent(int percent) => ZoomPercent = Math.Max(1, percent);
    public void SetFitMode(bool isFitMode) => _isFitMode = isFitMode;

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

        if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
        {
            SetStatus(PreviewTextState.Custom, "!ffmpeg.exe");
            return;
        }

        string? sourcePath = SelectedPreviewSource?.FullPath;
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            SetStatus(PreviewTextState.Custom, "!source");
            return;
        }

        CancellationTokenSource? previousCts = _previewCts;
        CancellationTokenSource cts = new();
        _previewCts = cts;
        previousCts?.Dispose();
        CancellationToken token = cts.Token;

        try
        {
            IsBusy = true;
            FrameServerLogText = string.Empty;
            TimeSpan position = TimeSpan.FromSeconds(CurrentFrame / _frameRate);
            string? filter = PreviewPipeline.TryExtractVideoFilter(_getFilterArgs(), out string chain)
                ? chain
                : null;

            SetStatus(PreviewTextState.ExtractingSource);
            SourceImage = await RunFFmpegBitmapPipeAsync(sourcePath, position, null, token);

            SetStatus(PreviewTextState.ExtractingFiltered);
            EncodedImage = await RunFFmpegBitmapPipeAsync(sourcePath, position, filter, token);

            SetStatus(PreviewTextState.FrameRendered);
        }
        catch (OperationCanceledException)
        {
            if (!_isDisposed) SetStatus(PreviewTextState.Cancelled);
        }
        catch (ObjectDisposedException) when (_isDisposed) { }
        catch (Exception ex)
        {
            if (!_isDisposed)
            {
                SetStatus(PreviewTextState.Custom, ex.Message);
                RunOnUi(() => FrameServerLogText = PreviewPipeline.TrimProcessMessage(ex.Message));
            }
        }
        finally
        {
            if (ReferenceEquals(_previewCts, cts)) _previewCts = null;
            cts.Dispose();
            if (!_isDisposed) IsBusy = false;
        }
    }

    private async Task<ImageSource> RunFFmpegBitmapPipeAsync(
        string sourcePath,
        TimeSpan position,
        string? filter,
        CancellationToken token)
    {
        string[] args = PreviewPipeline.BuildFFmpegBitmapPipeArgs(sourcePath, position, filter);
        FFmpegProcessResultWithOutput result = await FFmpegProcessRunner
            .RunAsyncWithOutput(_ffmpegPath, args, TimeSpan.FromMinutes(1), token)
            .ConfigureAwait(false);
        using MemoryStream output = result.Output;

        if (result.ExitCode != 0)
        {
            string diagnostic = result.Stderr;
            throw new InvalidOperationException($"ffmpeg exit code {result.ExitCode}: {PreviewPipeline.TrimProcessMessage(diagnostic)}");
        }

        if (output.Length == 0)
            throw new InvalidOperationException(Lang.PreviewFrameFileMissing);

        return PreviewPipeline.LoadBitmap(output);
    }

    private void SwitchSource(string newPath)
    {
        CancelPreview();
        IsBusy = false;
        SourceImage = null;
        EncodedImage = null;
        CurrentFrame = 0;
        VideoFilename = Path.GetFileName(newPath);
        SetReadyTexts();
    }

    private void CancelPreview()
    {
        try { _previewCts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    private void ShowFrameDataDebug()
    {
        StringBuilder sb = new();
        sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugSourceVideo, SelectedPreviewSource?.FullPath ?? Lang.NoneText));
        sb.AppendLine($"ffmpeg path: {_ffmpegPath}");
        sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugTotalFrames, TotalFrames));
        sb.AppendLine(string.Format(CultureInfo.CurrentCulture, Lang.DebugMaxPositionSeconds, MaxPositionSeconds));
        sb.AppendLine();
        sb.AppendLine("FFmpeg filter args:");
        sb.AppendLine(_getFilterArgs() ?? string.Empty);
        new OpenDebugModalCmd(_modalNavS, "FFmpeg Filter Preview frame data", sb.ToString()).Execute(null);
    }

    private void SetReadyTexts()
    {
        SetStatus(PreviewTextState.Ready);
        RunOnUi(() => FrameServerLogText = Lang.StatusReady);
        UpdatePreviewButtonText();
    }

    private void SetStatus(PreviewTextState state, string? detail = null)
    {
        _statusState = state;
        _statusDetail = detail;
        RunOnUi(() => StatusText = BuildStatusText());
    }

    private string BuildStatusText() => _statusState switch
    {
        PreviewTextState.Ready => Lang.StatusReady,
        PreviewTextState.ExtractingSource => Lang.StatusExtractingSource,
        PreviewTextState.ExtractingFiltered => Lang.StatusExtractingFiltered,
        PreviewTextState.FrameRendered => string.Format(CultureInfo.CurrentCulture, Lang.StatusFrameRendered, CurrentFrame),
        PreviewTextState.Cancelled => Lang.StatusCancelled,
        PreviewTextState.Custom => _statusDetail ?? string.Empty,
        _ => Lang.StatusReady,
    };

    private void UpdatePreviewButtonText() => PreviewButtonText = IsBusy ? Lang["Cancel"] : Lang["Preview"];

    private void RunOnUi(Action action)
    {
        if (_uiContext == null || SynchronizationContext.Current == _uiContext)
        {
            action();
            return;
        }

        _uiContext.Post(static state => ((Action)state!).Invoke(), action);
    }

    private void OnLanguageChanged()
    {
        Lang = new VpyPreviewerLangProvider(UILangProvider.Current.LanguageCode);
        UpdatePreviewButtonText();
        RunOnUi(() => StatusText = BuildStatusText());
    }

    public override void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        CancelPreview();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
