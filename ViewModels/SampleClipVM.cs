namespace OneColumnEncoder.ViewModels;

public class SampleClipVM : BaseVM, IClipRangeSelectorDragAware
{
    private const int MinClipLengthSeconds = 10;
    private const int MaxClipLengthSeconds = 600;

    private readonly ModalNavS _modalNavS;
    private readonly AppConfM _appConfM;
    private readonly Action _closeAction;
    private readonly Func<EncodingPipelineRequest?> _buildRequest;
    private readonly double _totalSeconds;
    private readonly double _frameRate;
    private readonly long _totalFrames;
    private readonly string _fieldOrderKind = "unknown";
    private readonly string _frameRateKind = "unknown";
    private ClipRangeSelectorLangProvider _lang = null!;
    private bool _isDraggingSelection;

    private bool _isSyncing;

    public ClipRangeSelectorLangProvider Lang
    {
        get => _lang;
        private set => SetProperty(ref _lang, value);
    }
    public static string WindowTitle => ClipRangeSelectorLangProvider.WindowTitle;
    public string TimelineSectionTitle => Lang.TimelineSectionTitle;
    public string SelectionHintText => Lang.SelectionHintText;
    public string DurationSectionTitle => Lang.DurationSectionTitle;
    public string ClipLengthLabel => Lang.ClipLengthLabel;
    public string StartTimeLabel => Lang.StartTimeLabel;
    public string ClipDurationLabel => Lang.ClipDurationLabel;
    public string EndTimeLabel => Lang.EndTimeLabel;
    public string TimeFormatText => Lang.TimeFormatText;
    public string StartFrameLabel => Lang.StartFrameLabel;
    public string ClipFrameCountLabel => Lang.ClipFrameCountLabel;
    public string EndFrameLabel => Lang.EndFrameLabel;
    public string FrameFormatText => Lang.FrameFormatText;
    public string Note2Text => Lang.Note2Text;
    public ObservableCollection<ColumnTextItemM> SummaryColumns { get; } = [];
    public ObservableCollection<string> AxisLabels { get; } = [];
    public ButtonGroupVM FinishButtons { get; }
    public CloseModalCmd CloseCmd { get; }
    public ActionCmd RunSampleCmd { get; }

    public static string[] ClipLengthTickLabels => ["10", "30", "90", "150", "300", "600"];

    private double _selectionStart = 0.48d;
    public double SelectionStart
    {
        get => _selectionStart;
        set
        {
            if (!SetProperty(ref _selectionStart, value)) return;
            SyncFromSelection(updateClipLength: true);
        }
    }

    private double _selectionEnd = 0.52d;
    public double SelectionEnd
    {
        get => _selectionEnd;
        set
        {
            if (!SetProperty(ref _selectionEnd, value)) return;
            SyncFromSelection(updateClipLength: true);
        }
    }

    private int _clipLengthSeconds = 5;
    public int ClipLengthSeconds
    {
        get => _clipLengthSeconds;
        set
        {
            int next = Math.Max(MinClipLengthSeconds, Math.Min(MaxClipLengthSeconds, value));
            if (!SetProperty(ref _clipLengthSeconds, next)) return;
            ApplyClipLengthToSelection();
        }
    }

    private string _startTimeText = "00:00:00.000";
    public string StartTimeText
    {
        get => _startTimeText;
        set
        {
            if (!SetProperty(ref _startTimeText, value)) return;
            if (!_isSyncing) CommitStartTimeText();
        }
    }

    private string _clipDurationText = "00:00:30.000";
    public string ClipDurationText
    {
        get => _clipDurationText;
        set => SetProperty(ref _clipDurationText, value);
    }

    private string _endTimeText = "00:00:30.000";
    public string EndTimeText
    {
        get => _endTimeText;
        set
        {
            if (!SetProperty(ref _endTimeText, value)) return;
            if (!_isSyncing) CommitEndTimeText();
        }
    }

    private string _startFrameText = "0";
    public string StartFrameText
    {
        get => _startFrameText;
        set
        {
            if (!SetProperty(ref _startFrameText, value)) return;
            if (!_isSyncing) CommitStartFrameText();
        }
    }

    private string _clipFrameCountText = "0";
    public string ClipFrameCountText
    {
        get => _clipFrameCountText;
        set => SetProperty(ref _clipFrameCountText, value);
    }

    private string _endFrameText = "0";
    public string EndFrameText
    {
        get => _endFrameText;
        set
        {
            if (!SetProperty(ref _endFrameText, value)) return;
            if (!_isSyncing) CommitEndFrameText();
        }
    }

    public SampleClipVM(ModalNavS modalNavS, Action closeAction, AppConfM appConfM, Func<EncodingPipelineRequest?> buildRequest, VideoAnalysisM srcVideoAnalysis)
    {
        _modalNavS = modalNavS;
        _appConfM = appConfM;
        _closeAction = closeAction;
        _buildRequest = buildRequest;
        FFProbeSrcStats sourceStats = FFProbeSourceStatsReader.Read(srcVideoAnalysis.RawJson);
        _totalSeconds = sourceStats.DurationSeconds;
        _frameRate = sourceStats.FrameRate;
        _totalFrames = Math.Max(1L, sourceStats.TotalFrames);
        _fieldOrderKind = sourceStats.FieldOrderKind;
        _frameRateKind = sourceStats.FrameRateKind;

        Lang = new ClipRangeSelectorLangProvider(UILangProvider.Current.LanguageCode);

        BuildSummary();
        BuildAxisLabels();

        CloseCmd = new CloseModalCmd(closeAction);
        RunSampleCmd = new ActionCmd(_ => RunSample());
        Lang = new ClipRangeSelectorLangProvider(UILangProvider.Current.LanguageCode);
        FinishButtons = ButtonGroupVM.CreateTwoButton(Lang.CancelButtonText, Lang.ConfirmButtonText, CloseCmd, RunSampleCmd);

        int initialLength = (int)Math.Round(Math.Min(30d, Math.Max(10d, _totalSeconds * 0.04d)));
        _clipLengthSeconds = initialLength;
        ApplyClipLengthToSelection();
        SyncFromSelection(updateClipLength: false);
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private void BuildSummary()
    {
        string frameBottomText = _fieldOrderKind switch
        {
            "progressive" => Lang.SummaryProgressive,
            "interlaced" => Lang.SummaryInterlaced,
            _ => Lang.SummaryUnknown,
        };

        SummaryColumns.Clear();
        // Total duration seconds
        SummaryColumns.Add(new ColumnTextItemM
        {
            TopText = Lang.SummaryDurationLabel,
            MainText = $"{Math.Round(_totalSeconds, 1).ToString("0.#", CultureInfo.InvariantCulture)}",
            BottomText = Lang.SummarySecondsUnit
        });
        // Total frames
        SummaryColumns.Add(new ColumnTextItemM
        {
            TopText = Lang.SummaryTotalFramesLabel,
            MainText = $"{_totalFrames} f",
            BottomText = frameBottomText
        });
        // Frame rate
        SummaryColumns.Add(new ColumnTextItemM
        {
            TopText = Lang.SummaryFrameRateLabel,
            MainText = $"{_frameRate.ToString("0.###", CultureInfo.InvariantCulture)} fps",
            BottomText = _frameRateKind switch
            {
                "constant" => Lang.SummaryConstantFrameRate,
                "variable" => Lang.SummaryVariableFrameRate,
                _ => Lang.SummaryFrameRateUnknown,
            }
        });
    }

    private void BuildAxisLabels()
    {
        AxisLabels.Clear();
        for (int i = 0; i <= 4; i++)
        {
            double seconds = _totalSeconds * i / 4d;
            AxisLabels.Add(SampleClip.FormatAxisTimestamp(seconds));
        }
    }

    private void ApplyClipLengthToSelection()
    {
        if (_isSyncing || _totalSeconds <= 0) return;

        _isSyncing = true;
        double span = Math.Min(1d, ClipLengthSeconds / _totalSeconds);
        double start = Math.Max(0d, Math.Min(SelectionStart, 1d - span));
        SelectionStart = start;
        SelectionEnd = Math.Min(1d, start + span);
        _isSyncing = false;
        SyncFromSelection(updateClipLength: false);
    }

    public void SetDraggingSelection(bool isDraggingSelection)
    {
        _isDraggingSelection = isDraggingSelection;
    }

    private void SyncFromSelection(bool updateClipLength)
    {
        if (_isSyncing || _totalSeconds <= 0) return;

        if (updateClipLength && _isDraggingSelection)
            updateClipLength = false;

        _isSyncing = true;
        double start = Math.Max(0d, Math.Min(1d, SelectionStart));
        double end = Math.Max(0d, Math.Min(1d, SelectionEnd));
        if (end < start) (start, end) = (end, start); // Fix overlap

        double startSeconds = start * _totalSeconds;
        double endSeconds = end * _totalSeconds;
        double durationSeconds = Math.Max(0d, endSeconds - startSeconds);

        StartTimeText =
            EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(startSeconds));
        ClipDurationText =
            EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(durationSeconds));
        EndTimeText =
            EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(endSeconds));

        long startFrame =
            Math.Min(_totalFrames - 1L, SampleClip.SecondsToFirstFrame(startSeconds, _frameRate));
        long endFrame =
            Math.Min(_totalFrames - 1L, Math.Max(startFrame, SampleClip.SecondsToLastFrame(endSeconds, _frameRate)));
        StartFrameText =
            startFrame.ToString(CultureInfo.InvariantCulture);
        ClipFrameCountText =
            Math.Max(1, endFrame - startFrame + 1).ToString(CultureInfo.InvariantCulture);
        EndFrameText =
            endFrame.ToString(CultureInfo.InvariantCulture);

        if (updateClipLength)
        {
            int seconds = Math.Max(
                MinClipLengthSeconds,
                Math.Min(MaxClipLengthSeconds,
                (int)Math.Round(durationSeconds)));
            SetProperty(ref _clipLengthSeconds, seconds, nameof(ClipLengthSeconds));
        }

        _isSyncing = false;
    }

    private void CommitStartTimeText()
    {
        if (!TryParseSourceSeconds(StartTimeText, allowSourceEnd: false, out double startSeconds))
        {
            SyncFromSelection(updateClipLength: false);
            return;
        }

        double durationSeconds = GetCurrentClipDurationSeconds();
        ApplySelectionSeconds(startSeconds, startSeconds + durationSeconds, anchorEnd: false);
    }

    private void CommitEndTimeText()
    {
        if (!TryParseSourceSeconds(EndTimeText, allowSourceEnd: true, out double endSeconds) || endSeconds <= 0d)
        {
            SyncFromSelection(updateClipLength: false);
            return;
        }

        double durationSeconds = GetCurrentClipDurationSeconds();
        ApplySelectionSeconds(endSeconds - durationSeconds, endSeconds, anchorEnd: true);
    }

    private void CommitStartFrameText()
    {
        if (!TryParseSourceFrame(StartFrameText, out long startFrame))
        {
            SyncFromSelection(updateClipLength: false);
            return;
        }

        double startSeconds = startFrame / _frameRate;
        double durationSeconds = GetCurrentClipDurationSeconds();
        ApplySelectionSeconds(startSeconds, startSeconds + durationSeconds, anchorEnd: false);
    }

    private void CommitEndFrameText()
    {
        if (!TryParseSourceFrame(EndFrameText, out long endFrame))
        {
            SyncFromSelection(updateClipLength: false);
            return;
        }

        double endSeconds = Math.Min(_totalSeconds, (endFrame + 1d) / _frameRate);
        double durationSeconds = GetCurrentClipDurationSeconds();
        ApplySelectionSeconds(endSeconds - durationSeconds, endSeconds, anchorEnd: true);
    }

    #region Clip Input Queries
    private double GetCurrentClipDurationSeconds()
    {
        double durationSeconds = Math.Abs(SelectionEnd - SelectionStart) * _totalSeconds;
        if (durationSeconds <= 0d)
            durationSeconds = ClipLengthSeconds;

        return SampleClip.ClampDuration(durationSeconds, _totalSeconds, MinClipLengthSeconds, MaxClipLengthSeconds);
    }

    private bool TryParseSourceSeconds(string text, bool allowSourceEnd, out double seconds)
    {
        try
        {
            return SampleClip.TryParseSourceSeconds(text, _totalSeconds, allowSourceEnd, out seconds);
        }
        catch
        {
            seconds = 0d;
            return false;
        }
    }

    private bool TryParseSourceFrame(string text, out long frame)
    {
        return SampleClip.TryParseSourceFrame(text, _totalFrames, out frame);
    }
    #endregion

    private void ApplySelectionSeconds(double startSeconds, double endSeconds, bool anchorEnd)
    {
        var selection = SampleClip.NormalizeSelectionSeconds(
            startSeconds,
            endSeconds,
            anchorEnd,
            _totalSeconds,
            MinClipLengthSeconds,
            MaxClipLengthSeconds);
        if (!selection.HasValue)
        {
            SyncFromSelection(updateClipLength: false);
            return;
        }

        _isSyncing = true;
        SelectionStart = selection.Value.selectionStart;
        SelectionEnd = selection.Value.selectionEnd;
        _isSyncing = false;
        SyncFromSelection(updateClipLength: true);
    }

    private void RunSample()
    {
        try
        {
            EncodingPipelineRequest? request = _buildRequest();
            if (request == null)
            {
                new OpenDebugModalCmd(_modalNavS, "Sample Clip Error", "Missing upstream input path. Make sure a video source or script source is selected for the chosen upstream tool.").Execute(null);
                return;
            }

            EncodingClipRequest clip = BuildClipRequest();
            EncodingPipelineCommand command = EncodingPipeline.BuildY4mCommand(request with { Clip = clip });

            ConfirmationModal? existing = Application.Current.Windows
                .OfType<ConfirmationModal>()
                .FirstOrDefault(w => w.DataContext is ConfirmationVM &&
                                w.Owner == Application.Current.MainWindow);
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateDebug(
                "Sample Encoding Command", command.DisplayCommandLine,
                closeCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    _closeAction();
                    new OpenEncodingMonitorCmd(_modalNavS, _appConfM, request with { Clip = clip }, command, isSample: true).Execute(null);
                }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            new OpenDebugModalCmd(_modalNavS, "Sample Clip Error", ex.Message).Execute(null);
        }
    }

    private EncodingClipRequest BuildClipRequest()
    {
        string startTime = EncodingPipeline.FormatTimestamp(EncodingPipeline.ParseTimestamp(StartTimeText));
        string endTime = EncodingPipeline.FormatTimestamp(EncodingPipeline.ParseTimestamp(EndTimeText));
        long? firstFrame = SampleClip.TryParseNonNegativeLong(StartFrameText);
        long? lastFrame = SampleClip.TryParseNonNegativeLong(EndFrameText);

        return new EncodingClipRequest(startTime, endTime, firstFrame, lastFrame, _frameRate);
    }

    private void OnLanguageChanged()
    {
        Lang = new ClipRangeSelectorLangProvider(UILangProvider.Current.LanguageCode);
        FinishButtons.B2_1Text = Lang.CancelButtonText;
        FinishButtons.B2_2Text = Lang.ConfirmButtonText;
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(TimelineSectionTitle));
        OnPropertyChanged(nameof(SelectionHintText));
        OnPropertyChanged(nameof(DurationSectionTitle));
        OnPropertyChanged(nameof(ClipLengthLabel));
        OnPropertyChanged(nameof(StartTimeLabel));
        OnPropertyChanged(nameof(ClipDurationLabel));
        OnPropertyChanged(nameof(EndTimeLabel));
        OnPropertyChanged(nameof(TimeFormatText));
        OnPropertyChanged(nameof(StartFrameLabel));
        OnPropertyChanged(nameof(ClipFrameCountLabel));
        OnPropertyChanged(nameof(EndFrameLabel));
        OnPropertyChanged(nameof(FrameFormatText));
        OnPropertyChanged(nameof(Note2Text));
        OnPropertyChanged(nameof(ClipLengthTickLabels));
        BuildSummary();
    }

    public override void Dispose()
    {
        // Unsubscribe from the global language change event to avoid keeping this modal alive.
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

}
