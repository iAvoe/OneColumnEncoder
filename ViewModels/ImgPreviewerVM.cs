using OneColumnEncoder.Analytics;
using OneColumnEncoder.Models.Analysis;
using OneColumnEncoder.Models.Encoding;
using System.IO;

namespace OneColumnEncoder.ViewModels;

public class ImgPreviewerVM : BaseVM, IPreviewViewModel
{
    private enum PreviewTextState
    {
        Ready,
        NoFfmpeg,
        NoSource,
        Extracting,
        Converting,
        Encoding,
        Decoding,
        ComputingScores,
        PreviewReady,
        Cancelled,
        DisplayModeBlocked,
        DisplayModeSet,
        Custom,
    }

    private readonly EncoderConfVM _encoderConfVM;
    private readonly ModalNavS _modalNavS;
    private readonly string? _ffmpegPath;
    private readonly string? _sourceVideoPath;
    private readonly PreviewSourceInfo[] _previewSources;
    private readonly string _workDirectory;
    private readonly ColorSpaceAnalysisM _colorSpaceAnalysis;
    private readonly double _frameRate;
    private bool _isDisposed;
    // CTS for ffmpeg operations only (extract, encode, decode).
    // Created fresh each preview run. Score tools (ssimulacra2, butteraugli)
    // do NOT observe this token — they run independently once decoding finishes.
    private CancellationTokenSource? _previewCts;

    // Tracks the running ffmpeg process so it can be force-killed on cancel.
    // Not used for external score-tool processes.
    private Process? _currentProcess;
    private string? _lastFFmpegStderr;
    private PreviewTextState _statusState = PreviewTextState.Ready;
    private string? _statusDetail;
    private string? _previewReadyEncoder;
    private string? _previewReadyCrf;
    private bool _isFitMode;
    private PreviewDisplayMode _displayMode = PreviewDisplayMode.Raw;
    private ImgPreviewerLangProvider _lang = new(UILangProvider.Current.LanguageCode);
    public ImgPreviewerLangProvider Lang
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
    public string ZoomLabel => Lang.ZoomLabel;
    public string PositionLabel => SampleClip.FormatAxisTimestamp(PreviewPositionSeconds);
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
        set
        {
            if (SetProperty(ref _previewPositionSeconds, Math.Max(0, Math.Min(MaxPositionSeconds, value))))
                OnPropertyChanged(nameof(PositionLabel));
        }
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

    public ImgPreviewerVM(
        EncoderConfVM encoderConfVM,
        Stores.ModalNavS modalNavS,
        string? ffmpegPath,
        string? sourceVideoPath,
        string? sourceFfprobeJson,
        Func<JsonElement, long>? getTotalFrames = null,
        IReadOnlyList<PreviewSourceInfo>? previewSources = null,
        PreviewEncoder? initialEncoder = null)
    {
        _encoderConfVM = encoderConfVM;
        _modalNavS = modalNavS;
        _ffmpegPath = ffmpegPath;
        _sourceVideoPath = sourceVideoPath;
        _previewSources = previewSources?.Where(source => !string.IsNullOrWhiteSpace(source.FilePath) && source.FrameCount > 0).ToArray() ?? [];
        _workDirectory = PreviewPipeline.CreateWorkDirectory("1cenc-image-preview-");

        ZoomPresetButtons = ButtonGroupVM.CreateThreeButton(Lang["Fit"], "100%", "200%");

        EncoderDropdown.Items.Add(new DropdownItemM("libx264") { Tag = PreviewEncoder.X264 });
        EncoderDropdown.Items.Add(new DropdownItemM("libx265") { Tag = PreviewEncoder.X265 });
        EncoderDropdown.Items.Add(new DropdownItemM("libsvtav1") { Tag = PreviewEncoder.SvtAv1 });
        EncoderDropdown.Items.Add(new DropdownItemM("libvvenc (Preview)") { Tag = PreviewEncoder.Vvenc });
        EncoderDropdown.SelectedItem = initialEncoder.HasValue
            ? EncoderDropdown.Items.FirstOrDefault(i => i.Tag is PreviewEncoder encoder && encoder == initialEncoder.Value)
            : null;
        // Null (no match / no MainVM selection) falls back to libx264;
        // GetSelectedEncoder() also treats null SelectedItem as X264.
        EncoderDropdown.SelectedItem ??= EncoderDropdown.Items[0];
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
        using JsonDocument? sourceDocument = hasSourceStats ? JsonDocument.Parse(sourceFfprobeJson!) : null;
        JsonElement sourceRoot = sourceDocument?.RootElement ?? default;
        _colorSpaceAnalysis = hasSourceStats
            ? ColorSpaceConverter.AnalyzeRoot(sourceRoot)
            : ColorSpaceConverter.Analyze(null);
        FFProbeSrcStats sourceStats = hasSourceStats
            ? FFProbeSourceStatsReader.Read(sourceRoot)
            : FFProbeSourceStatsReader.Read(string.Empty);
        _frameRate = sourceStats.FrameRate > 0d ? sourceStats.FrameRate : 30d;
        long totalFrames = _previewSources.Length > 0
            ? _previewSources.Sum(source => Math.Max(0, source.FrameCount))
            : getTotalFrames != null
                ? getTotalFrames.Invoke(sourceRoot)
                : hasSourceStats
                    ? EncodingPipeline.GetSourceTotalFrames(sourceRoot) ?? 0
                    : 0;
        double previewDurationSeconds = _previewSources.Length > 0
            ? _previewSources.Sum(source => source.DurationSeconds > 0d
                ? source.DurationSeconds
                : source.FrameCount > 0 && _frameRate > 0d
                    ? source.FrameCount / _frameRate
                    : 0d)
            : totalFrames > 0 && sourceStats.FrameRate > 0d
                ? totalFrames / sourceStats.FrameRate
                : sourceStats.DurationSeconds;
        MaxPositionSeconds = Math.Max(1, (int)Math.Floor(Math.Min(int.MaxValue, previewDurationSeconds)) - 1);
        bool canCenterPreviewPosition = previewDurationSeconds > 0d;
        PreviewPositionSeconds = canCenterPreviewPosition
            ? MaxPositionSeconds / 2
            : 0;
        BuildPositionTickLabels(previewDurationSeconds);

        SetStatus(PreviewTextState.Ready);
        PreviewButtonText = Lang.PreviewButtonText;
        PreviewCommand = new ActionCmd(_ => PreviewOrCancel());
        RefreshSsimulacra2Status();
        RefreshButteraugliStatus();
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public void SetZoomPercent(int percent) => ZoomPercent = Math.Max(1, percent);

    public void SetFitMode(bool isFitMode) => _isFitMode = isFitMode;

    // Toggle: cancel in-flight preview or start a new one.
    // Cancellation signals the CTS, then immediately kills ffmpeg.
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
            SetStatus(PreviewTextState.NoFfmpeg);
            return;
        }

        if (string.IsNullOrWhiteSpace(_sourceVideoPath) || !File.Exists(_sourceVideoPath))
        {
            SetStatus(PreviewTextState.NoSource);
            return;
        }

        CancellationTokenSource? previousCts = _previewCts;
        CancellationTokenSource cts = new();
        _previewCts = cts;
        previousCts?.Dispose();
        CancellationToken token = cts.Token;
        _lastFFmpegStderr = null;
        IsBusy = true;

        try
        {
            EncoderConfM model = _encoderConfVM.CreatePreviewModel();
            PreviewEncoder encoder = GetSelectedEncoder();

            if (encoder == PreviewEncoder.SvtAv1 && PreviewPipeline.IsSource12Bit(_colorSpaceAnalysis))
            {
                _modalNavS.Close();
                new OpenErrModalCmd(_modalNavS, Lang.EncoderLabel, Lang.WarnSvtAv1No12Bit).Execute(null);
                return;
            }

            string displayFilter = PreviewPipeline.BuildDisplayFilter(_displayMode, _colorSpaceAnalysis) ?? string.Empty;
            string rawsrcPath = GetWorkPath("source-raw.png");
            string srcPath = string.IsNullOrWhiteSpace(displayFilter)
                ? rawsrcPath
                : GetWorkPath($"source-{PreviewPipeline.GetDisplayModeFileSuffix(_displayMode)}.png");
            string encodedPath = GetEncodedPath(encoder);
            string decodedPath = GetDecodedPath(encoder);
            (string sourcePath, TimeSpan sourcePosition) = ResolvePreviewSource(TimeSpan.FromSeconds(PreviewPositionSeconds));

            SetStatus(PreviewTextState.Extracting);
            await RunFFmpegAsync(PreviewPipeline.BuildSourceArgs(sourcePath, sourcePosition, rawsrcPath), token);
            PreviewPipeline.EnsureFileExists(rawsrcPath, "!SOURCE");

            if (!string.IsNullOrWhiteSpace(displayFilter))
            {
                SetStatus(PreviewTextState.Converting, GetDisplayModeTitle(_displayMode));
                await RunFFmpegAsync(PreviewPipeline.BuildSourceArgs(sourcePath, sourcePosition, srcPath, displayFilter), token);
            }
            PreviewPipeline.EnsureFileExists(srcPath, "!SOURCE");
            SourceImage = PreviewPipeline.LoadBitmap(srcPath);

            SetStatus(PreviewTextState.Encoding, PreviewPipeline.GetEncoderTitle(encoder));
            await RunFFmpegAsync(PreviewPipeline.BuildEncodeArgs(encoder, model, srcPath, encodedPath), token);

            SetStatus(PreviewTextState.Decoding);
            await RunFFmpegAsync(PreviewPipeline.BuildDecodeArgs(encodedPath, decodedPath), token);
            PreviewPipeline.EnsureFileExists(decodedPath, "!ENCODE");
            EncodedImage = PreviewPipeline.LoadBitmap(decodedPath);

            SetStatus(PreviewTextState.ComputingScores);

            // NOTE: Score tools do NOT accept the cancellation token.
            // If the user cancels during this phase, the tools will still
            // run to completion, then IsBusy resets normally.
            if (Ssimulacra2.IsSsimU2Present)
            {
                Ssimulacra2StatusText = Lang.Ssimulacra2ToolPresent;
                var (score, error) = await Ssimulacra2.RunScoreAsync(srcPath, decodedPath);
                Ssimulacra2StatusText = score.HasValue
                    ? $"SSIMULACRA2.1: {score.Value:F2}"
                    : $"SSIMULACRA2.1: {error}";
            }

            if (Butteraugli.IsPresent)
            {
                ButteraugliStatusText = Lang.ButteraugliToolPresent;
                var (score, error) = await Butteraugli.RunScoreAsync(srcPath, decodedPath);
                ButteraugliStatusText = score.HasValue
                    ? $"Butteraugli: {score.Value:F4}"
                    : $"Butteraugli: {error}";
            }

            SetPreviewReadyStatus(
                PreviewPipeline.GetEncoderTitle(encoder),
                PreviewPipeline.GetCrfValue(encoder, model).ToString(CultureInfo.CurrentCulture));
        }
        catch (OperationCanceledException)
        {
            if (!_isDisposed)
                SetStatus(PreviewTextState.Cancelled);
        }
        catch (ObjectDisposedException) when (_isDisposed) { }
        catch (Exception ex)
        {
            if (!_isDisposed)
            {
                SetStatus(PreviewTextState.Custom, ex.Message);
                if (!string.IsNullOrWhiteSpace(_lastFFmpegStderr))
                {
                    _modalNavS.Close();
                    new OpenErrModalCmd(
                        _modalNavS,
                        Lang.EncoderLabel,
                        _lastFFmpegStderr).Execute(null);
                }
            }
        }
        finally
        {
            _currentProcess = null;
            if (ReferenceEquals(_previewCts, cts))
                _previewCts = null;
            cts.Dispose();

            if (_isDisposed) DeleteWorkDirectory();
            else IsBusy = false;
        }
    }

    // Runs ffmpeg with the given args. If token is cancelled during
    // execution the process is killed and OperationCanceledException
    // propagates to the caller.
    private async Task RunFFmpegAsync(IReadOnlyList<string> args, CancellationToken token)
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
        using CancellationTokenRegistration killRegistration = token.Register(() => PreviewPipeline.TryKillProcess(process));
        try
        {
            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(token);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(token);

            await process.WaitForExitAsync(token);

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                _lastFFmpegStderr = stderr;
                string diagnostic = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                throw new InvalidOperationException(
                    $"ffmpeg exited with code {process.ExitCode}. " +
                    PreviewPipeline.TrimProcessMessage(diagnostic));
            }
        }
        catch
        {
            PreviewPipeline.TryKillProcess(process);
            throw;
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
            PositionTickLabels.Add(SampleClip.FormatAxisTimestamp(Math.Round(safeDuration * i / 4d)));
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

    private (string SourcePath, TimeSpan SourcePosition) ResolvePreviewSource(TimeSpan previewPosition)
    {
        if (_previewSources.Length == 0 || 0d > _frameRate)
            return (string.IsNullOrWhiteSpace(_sourceVideoPath) ? string.Empty : _sourceVideoPath, previewPosition);

        long totalFrames = 0;
        foreach (PreviewSourceInfo source in _previewSources)
            totalFrames = checked(totalFrames + Math.Max(0, source.FrameCount));

        if (totalFrames <= 0)
            return (string.IsNullOrWhiteSpace(_sourceVideoPath) ? string.Empty : _sourceVideoPath, previewPosition);

        long globalFrame = Math.Clamp((long)Math.Floor(previewPosition.TotalSeconds * _frameRate), 0, totalFrames - 1);
        long cursor = 0;
        foreach (PreviewSourceInfo source in _previewSources)
        {
            long frameCount = Math.Max(0, source.FrameCount);
            if (frameCount <= 0)
                continue;

            long nextCursor = cursor + frameCount;
            if (globalFrame < nextCursor)
            {
                long localFrame = globalFrame - cursor;
                return (source.FilePath, TimeSpan.FromSeconds(localFrame / _frameRate));
            }

            cursor = nextCursor;
        }

        PreviewSourceInfo fallback = _previewSources[^1];
        return (fallback.FilePath, TimeSpan.Zero);
    }

    private void SetDisplayMode(PreviewDisplayMode displayMode)
    {
        if (_displayMode == displayMode) return;
        if (IsBusy)
        {
            SetStatus(PreviewTextState.DisplayModeBlocked);
            return;
        }

        _displayMode = displayMode;
        SetStatus(PreviewTextState.DisplayModeSet);
        RefreshSelectedEncodedImage();
        if (!IsBusy && SourceImage != null)
            _ = GeneratePreviewAsync();
    }

    private void TryKillCurrentProcess()
    {
        if (_currentProcess != null)
            PreviewPipeline.TryKillProcess(_currentProcess);
    }

    private void CancelPreview()
    {
        try { _previewCts?.Cancel(); }
        catch (ObjectDisposedException) { }
        TryKillCurrentProcess();
    }

    private void SetStatus(PreviewTextState state, string? detail = null)
    {
        _statusState = state;
        _statusDetail = detail;
        _previewReadyEncoder = null;
        _previewReadyCrf = null;
        StatusText = BuildStatusText();
    }

    private void SetPreviewReadyStatus(string encoder, string crf)
    {
        _statusState = PreviewTextState.PreviewReady;
        _statusDetail = null;
        _previewReadyEncoder = encoder;
        _previewReadyCrf = crf;
        StatusText = BuildStatusText();
    }

    private string BuildStatusText()
    {
        return _statusState switch
        {
            PreviewTextState.Ready => Lang.StatusReady,
            PreviewTextState.NoFfmpeg => Lang.StatusNoFfmpeg,
            PreviewTextState.NoSource => Lang.StatusNoSource,
            PreviewTextState.Extracting => Lang.StatusExtracting,
            PreviewTextState.Converting => string.Format(Lang.StatusConverting, _statusDetail ?? string.Empty),
            PreviewTextState.Encoding => string.Format(Lang.StatusEncoding, _statusDetail ?? string.Empty),
            PreviewTextState.Decoding => Lang.StatusDecoding,
            PreviewTextState.ComputingScores => Lang.StatusComputingScores,
            PreviewTextState.PreviewReady => BuildPreviewReadyStatus(),
            PreviewTextState.Cancelled => Lang.StatusCancelled,
            PreviewTextState.DisplayModeBlocked => Lang.StatusDisplayModeBlocked,
            PreviewTextState.DisplayModeSet => string.Format(Lang.StatusDisplayModeSet, GetDisplayModeTitle(_displayMode)),
            PreviewTextState.Custom => _statusDetail ?? string.Empty,
            _ => Lang.StatusReady,
        };
    }

    private string BuildPreviewReadyStatus()
    {
        return string.Format(
            Lang.StatusPreviewReady,
            _previewReadyEncoder ?? string.Empty,
            _previewReadyCrf ?? string.Empty);
    }

    private void DeleteWorkDirectory()
    {
        if (string.IsNullOrEmpty(_workDirectory) || !Directory.Exists(_workDirectory))
            return;

        TryKillCurrentProcess();

        try { PreviewPipeline.DeleteDirectoryQuietly(_workDirectory); }
        catch (IOException)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                try { PreviewPipeline.DeleteDirectoryQuietly(_workDirectory); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            });
        }
        catch (UnauthorizedAccessException) { }
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
        Lang = new ImgPreviewerLangProvider(UILangProvider.Current.LanguageCode);
        ZoomPresetButtons.B3_1Text = Lang["Fit"]; // Use LangProviderBase extension
        DisplayModeButtons.B5_1Text = Lang.RawButtonText;
        if (!IsBusy)
            PreviewButtonText = Lang.PreviewButtonText;
        StatusText = BuildStatusText();
        OnPropertyChanged(nameof(EncoderLabel));
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
        if (_isDisposed) return;

        _isDisposed = true;
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
        // Order: cancel first so in-flight ffmpeg knows to stop,
        // then kill the process, then release CTS resources.
        CancelPreview();
        if (!IsBusy)
            DeleteWorkDirectory();
        _encoderConfVM.SetPreviewBusy(false);

        base.Dispose();
    }
}

public sealed record PreviewSourceInfo(string FilePath, long FrameCount, double DurationSeconds = 0d);
