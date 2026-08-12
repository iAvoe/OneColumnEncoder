using OneColumnEncoder.Validation;
using System.IO;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartConfVM : BaseVM, IClipRangeSelectorDragAware
{
    // Modal navigation, close callback, and plan commit callback
    private readonly ModalNavS _modalNavS;
    private readonly Action _closeAction;
    private readonly Action<RepartPlanM> _applyPlan;

    // External tool paths for ffmpeg/ffprobe and temp directory for preview thumbnails
    private readonly string? _ffmpegPath;
    private readonly string? _ffprobePath;
    private readonly string _previewWorkDirectory;

    // Keyframe index window: 30s margin for long-GOP safety, 0.25s lead for seek overshoot
    private const double KeyframeIndexWindowMarginSeconds = 30d;
    private const double KeyframeIndexWindowLeadSeconds = 0.25d;
    private const double KeyframeIndexCacheReuseToleranceSeconds = 1d;

    // Cache infrastructure: sync root, file->index map, lifetime CTS (distinct from per-preview CTS)
    private readonly Lock _keyframeIndexCacheSync = new();
    private readonly Dictionary<string, KeyframeIndex> _keyframeIndexCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _keyframeIndexLifetimeCts = new();

    private RepartPlanM? _analysis;
    private RepartOutputItemVM? _selectedOutput;
    private List<RepartOutputItemVM> _selectedOutputs = [];
    private string _outputNameText = string.Empty;
    private string _startTimeText = "00:00:00.000";
    private string _endTimeText = "00:00:00.000";
    private string _firstFrameText = "0";
    private string _lastFrameText = "0";
    private string _segmentDurationText = "00:00:00.000";
    private string _frameCountText = "0";
    private string _statusText = string.Empty;
    private double _selectionStart;
    private double _selectionEnd = 1d;
    private List<RepartDividerM> _dividers = [];
    private List<RepartDividerItemVM> _selectedDividers = [];
    private RepartDividerItemVM? _selectedDivider;
    private string _dividerTimestampText = string.Empty;
    private string _dividerFrameText = string.Empty;
    private string _newDividerTimestampText = "00:00:00.000";
    private string _newDividerFrameText = "0";
    private CancellationTokenSource? _dividerPreviewCts;
    private readonly SemaphoreSlim _dividerPreviewRenderGate = new(1, 1);
    private readonly ObservableCollection<DividerPreviewFrameVM> _dividerPreviewFrames = [];
    private string _dividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewSelectDivider"];
    private bool _suppressDividerPreviewRefresh;
    private bool _isDraggingDivider;
    private long? _dividerPreviewRenderedFrame;
    private long? _dividerPreviewTargetFrame;
    private long _dividerPreviewRequestVersion;

    // Undo/Redo: bounded snapshot history of committed divider states (max depth 24).
    // Records are immutable, so snapshots are cheap deep-by-reference list copies.
    // A state is recorded AFTER each committed edit; _editCursor points at the current state.
    private const int MaxUndoDepth = 24;
    private readonly List<DividerEditSnapshot> _editHistory = [];
    private int _editCursor = -1;
    private bool _dragEditPending;

    // Reentrancy guards: prevent cascading property-change loops during time<->frame sync
    private bool _syncingNewDivider;
    private bool _isBusy;
    private bool _syncingRange;
    private bool _syncingDivider;

    public RepartConfVM(
        ModalNavS modalNavS,
        Action closeAction,
        Action<RepartPlanM> applyPlan,
        string? ffmpegPath,
        string? ffprobePath)
    {
        _modalNavS = modalNavS;
        _closeAction = closeAction;
        _applyPlan = applyPlan;
        _ffmpegPath = ffmpegPath;
        _ffprobePath = ffprobePath;
        _previewWorkDirectory = PreviewPipeline.CreateWorkDirectory("1cenc-repart-preview-");

        AddEpisodeCommand = new ActionCmd(_ => AddDivider());
        ApplyCommand = new ActionCmd(_ => ApplyAndClose());
        CancelCommand = new ActionCmd(_ => CancelAndClose());
        DeleteSelectedDividerCommand = new ActionCmd(_ => DeleteSelectedDividers());
        DeleteLeftDividerCommand = new ActionCmd(_ => DeleteAdjacentDivider(-1));
        DeleteRightDividerCommand = new ActionCmd(_ => DeleteAdjacentDivider(1));
        ClearOutputsCommand = new ActionCmd(_ => ClearOutputs());
        UndoEditCommand = new ActionCmd(_ => UndoEdit());
        RedoEditCommand = new ActionCmd(_ => RedoEdit());

        // Button groups: B3 = 3-button group, B2 = 2-button group
        DividerDeleteButtons = ButtonGroupVM.CreateThreeButton(
            DeleteEpisodeText,
            DeleteLeftDividerText,
            DeleteRightDividerText,
            DeleteSelectedDividerCommand,
            DeleteLeftDividerCommand,
            DeleteRightDividerCommand);
        FinishButtons = ButtonGroupVM.CreateTwoButton(CancelText, ApplyText, CancelCommand, ApplyCommand);
        RefreshDraftAvailability();

        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public const string WindowTitleText = RepartLangProvider.WindowTitle;

    public static string WindowTitle => WindowTitleText;
    public static string InputSourcesTitle => RepartLangProvider.Current["InputSources"];
    public static string OutputEpisodesTitle => RepartLangProvider.Current["OutputEpisodes"];
    public static string TimelineTitle => RepartLangProvider.Current["Timeline"];
    public static string TimelineControlTitle => RepartLangProvider.Current["TimelineControl"];
    public static string DividerControlTitle => RepartLangProvider.Current["DividerControl"];
    public static string AddNewDividerTitle => RepartLangProvider.Current["AddNewDivider"];
    public static string DividerOpsTitle => RepartLangProvider.Current["ManageDividers"];
    public static string OutputNameLabel => RepartLangProvider.Current["OutputName"];
    public static string StartTimeLabel => RepartLangProvider.Current["StartTime"];
    public static string SegmentDurationLabel => RepartLangProvider.Current["SegmentDuration"];
    public static string EndTimeLabel => RepartLangProvider.Current["EndTime"];
    public static string FirstFrameLabel => RepartLangProvider.Current["FirstFrame"];
    public static string FrameCountLabel => RepartLangProvider.Current["FrameCount"];
    public static string LastFrameLabel => RepartLangProvider.Current["LastFrame"];
    public static string TimeFormatText => RepartLangProvider.Current["TimeFormat"];
    public static string FrameFormatText => RepartLangProvider.Current["FrameFormat"];
    public static string AddEpisodeText => RepartLangProvider.Current["AddDivider"];
    public static string DeleteEpisodeText => RepartLangProvider.Current["DeleteDivider"];
    public static string FrameChangingFiltersWarning => RepartLangProvider.Current["FrameChangingFiltersWarning"];
    public static string ApplyText => RepartLangProvider.Current["Confirm"];
    public static string CancelText => RepartLangProvider.Current["Cancel"];
    public static string AddDividerText => RepartLangProvider.Current["AddDivider"];
    public static string DividerTimestampLabel => RepartLangProvider.Current["DividerTimestampLabel"];
    public static string DividerFrameLabel => RepartLangProvider.Current["DividerFrameLabel"];
    public static string DeleteSelectedDividerText => RepartLangProvider.Current["DeleteSelectedDivider"];
    public static string DeleteLeftDividerText => RepartLangProvider.Current["DeleteLeftDivider"];
    public static string DeleteRightDividerText => RepartLangProvider.Current["DeleteRightDivider"];
    public static string ClearDividersText => RepartLangProvider.Current["ClearDividers"];
    public static string UndoEditText => RepartLangProvider.Current["Undo"];
    public static string RedoEditText => RepartLangProvider.Current["Redo"];
    public static string TimelineHintText => RepartLangProvider.Current["TimelineHintDetailed"];
    public string OutputCountText => string.Format(RepartLangProvider.Current["OutputCount"], Outputs.Count);
    public static string TimelineStartText => "00:00:00.000";
    public string TimelineEndText => _analysis == null
        ? "00:00:00.000"
        : EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(_analysis.TotalSeconds));

    public ObservableCollection<RepartSrcItemVM> Sources { get; } = [];
    public ObservableCollection<RepartOutputItemVM> Outputs { get; } = [];
    public ObservableCollection<RepartDividerItemVM> DividerItems { get; } = [];
    public ObservableCollection<string> AxisLabels { get; } = [];
    public ObservableCollection<DividerPreviewFrameVM> DividerPreviewFrames
    {
        get => _dividerPreviewFrames;
    }
    public string DividerPreviewStatusText
    {
        get => _dividerPreviewStatusText;
        private set => SetProperty(ref _dividerPreviewStatusText, value);
    }
    public ButtonGroupVM DividerDeleteButtons { get; }
    public ButtonGroupVM FinishButtons { get; }
    public ICommand AddEpisodeCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteSelectedDividerCommand { get; }
    public ICommand DeleteLeftDividerCommand { get; }
    public ICommand DeleteRightDividerCommand { get; }
    public ICommand ClearOutputsCommand { get; }
    public ICommand UndoEditCommand { get; }
    public ICommand RedoEditCommand { get; }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanApply));
            }
        }
    }

    public bool CanEdit => !IsBusy && _analysis != null;
    public bool CanApply => CanEdit && Outputs.Count > 0;
    public bool CanAddEpisode
    {
        get
        {
            if (!CanEdit || _analysis == null) return false;
            return TryGetSuggestedNewDividerFrame(out _);
        }
    }
    public bool CanDeleteSelectedDivider => CanEdit && _selectedDividers.Any(item => !item.IsLocked);
    public bool CanDeleteLeftDivider => CanEdit && SelectedDivider != null
        && _dividers.Any(divider => divider.Frame < SelectedDivider.Frame && !divider.IsLocked);
    public bool CanDeleteRightDivider => CanEdit && SelectedDivider != null
        && _dividers.Any(divider => divider.Frame > SelectedDivider.Frame && !divider.IsLocked);
    public bool CanNudgeDivider => CanEdit && SelectedDivider is { IsLocked: false };
    public bool CanClearOutputs => CanEdit && _dividers.Count > 0;
    public bool CanUndo => _editCursor > 0;
    public bool CanRedo => _editCursor < _editHistory.Count - 1;
    public string SummaryText => _analysis == null
        ? string.Empty
        : string.Format(
            RepartLangProvider.Current["Summary"],
            _analysis.Sources.Count,
            _analysis.TotalFrames,
            _analysis.FrameRateNumerator,
            _analysis.FrameRateDenominator,
            EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(_analysis.TotalSeconds)));

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    // Output/Divider selection are mutually exclusive: selecting an output deselects all dividers,
    // and selecting a divider deselects all outputs (via SelectDividerForInteraction).
    public RepartOutputItemVM? SelectedOutput
    {
        get => _selectedOutput;
        set
        {
            if (!SetProperty(ref _selectedOutput, value)) return;
            if (value != null)
            {
                SelectOnlyDivider(null);
                LoadDraft(value.Model);
            }
            else if (SelectedDivider == null)
            {
                SetDraft(NewEpisodeName(), 0, 0);
            }
            RefreshDraftAvailability();
        }
    }

    public RepartDividerItemVM? SelectedDivider
    {
        get => _selectedDivider;
        private set
        {
            if (!SetProperty(ref _selectedDivider, value)) return;
            SetDividerTexts(value?.Model.Frame);
            RefreshDividerAvailability();
            if (!_suppressDividerPreviewRefresh)
                RefreshDividerPreview();
        }
    }

    public string DividerTimestampText
    {
        get => _dividerTimestampText;
        set
        {
            if (!SetProperty(ref _dividerTimestampText, value) || _syncingDivider) return;
            if (_analysis == null || SelectedDivider == null) return;
            try
            {
                MoveSelectedDivider(EncodingPipeline.TimestampToLastFrame(value, _analysis.FrameRate));
            }
            catch { }
        }
    }

    public string DividerFrameText
    {
        get => _dividerFrameText;
        set
        {
            if (!SetProperty(ref _dividerFrameText, value) || _syncingDivider) return;
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long frame))
                MoveSelectedDivider(frame);
        }
    }

    public string NewDividerTimestampText
    {
        get => _newDividerTimestampText;
        set
        {
            if (!SetProperty(ref _newDividerTimestampText, value) || _syncingNewDivider || _analysis == null) return;
            try
            {
                SetNewDividerFrame(EncodingPipeline.TimestampToLastFrame(value, _analysis.FrameRate));
            }
            catch { }
        }
    }

    public string NewDividerFrameText
    {
        get => _newDividerFrameText;
        set
        {
            if (!SetProperty(ref _newDividerFrameText, value) || _syncingNewDivider || _analysis == null) return;
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long frame)) return;
            SetNewDividerTimestamp(frame);
        }
    }

    public string OutputNameText
    {
        get => _outputNameText;
        set
        {
            if (SetProperty(ref _outputNameText, value))
                RefreshDraftAvailability();
        }
    }

    // Bidirectional sync group: time <-> frame <-> selection.
    // _syncingRange prevents infinite loops.
    public string StartTimeText
    {
        get => _startTimeText;
        set
        {
            if (!SetProperty(ref _startTimeText, value) || _syncingRange) return;
            SyncFramesFromTimes();
            RefreshDraftAvailability();
        }
    }

    public string SegmentDurationText
    {
        get => _segmentDurationText;
        private set => SetProperty(ref _segmentDurationText, value);
    }

    public string EndTimeText
    {
        get => _endTimeText;
        set
        {
            if (!SetProperty(ref _endTimeText, value) || _syncingRange) return;
            SyncFramesFromTimes();
            RefreshDraftAvailability();
        }
    }

    public string FirstFrameText
    {
        get => _firstFrameText;
        set
        {
            if (!SetProperty(ref _firstFrameText, value) || _syncingRange) return;
            SyncTimesFromFrames();
            RefreshDraftAvailability();
        }
    }

    public string FrameCountText
    {
        get => _frameCountText;
        private set => SetProperty(ref _frameCountText, value);
    }

    public string LastFrameText
    {
        get => _lastFrameText;
        set
        {
            if (!SetProperty(ref _lastFrameText, value) || _syncingRange) return;
            SyncTimesFromFrames();
            RefreshDraftAvailability();
        }
    }

    public double SelectionStart
    {
        get => _selectionStart;
        set
        {
            if (!SetProperty(ref _selectionStart, value) || _syncingRange) return;
            SyncDraftFromSelection();
        }
    }

    public double SelectionEnd
    {
        get => _selectionEnd;
        set
        {
            if (!SetProperty(ref _selectionEnd, value) || _syncingRange) return;
            SyncDraftFromSelection();
        }
    }

    // Entry point: bootstrap VM from a RepartPlanM. Stale sources are fatal (plan references changed files).
    public Task InitializeAsync(RepartPlanM? currentPlan)
    {
        if (currentPlan == null)
        {
            ShowError(RepartLangProvider.Current.SourceRequired);
            _closeAction();
            return Task.CompletedTask;
        }

        // Stale source check: if any source file has changed since plan creation, abort
        if (currentPlan.Sources.Any(source => !source.MatchesCurrentFile()))
        {
            OpenWarnModalCmd cmd = new(
                _modalNavS,
                WindowTitleText,
                RepartLangProvider.Current["StalePlanSourceChanged"]);
            cmd.Execute(null);
            _closeAction();
            return Task.CompletedTask;
        }

        _analysis = currentPlan.Clone();
        DisposeKeyframeIndexCache();
        LoadSources();
        BuildAxisLabels();
_dividers = GetPlanDividers(currentPlan);
        ReplaceOutputs(BuildDividerOutputs());
        ResetEditHistory();
        StatusText = RepartLangProvider.Current["Ready"];
        PrepareNextDraft();
        SetSuggestedNewDividerTexts();
        RefreshAnalysisProperties();

        return Task.CompletedTask;
    }

    public void SetSelectedOutputs(IEnumerable<RepartOutputItemVM> items)
    {
        _selectedOutputs = [.. items.OrderBy(item => item.Model.FirstFrame)];
        foreach (RepartOutputItemVM output in Outputs)
            output.IsSelected = _selectedOutputs.Contains(output);
        SelectedOutput = _selectedOutputs.LastOrDefault();
    }

    public void SetDraggingSelection(bool isDraggingSelection)
    {
        _isDraggingDivider = isDraggingSelection;
        _suppressDividerPreviewRefresh = isDraggingSelection;
        if (isDraggingSelection)
        {
            _dragEditPending = false;
        }
        else if (_dragEditPending)
        {
            // Commit the coalesced drag as a single undo step at gesture end.
            PushEditState();
        }
    }

    public void SelectDividerForInteraction(RepartDividerItemVM? item)
    {
        if (item == null) return;
        SelectOnlyDivider(item.Model.Id);
        SelectedOutput = null;
        EnsureDividerPreviewUpToDate();
    }

    public void EnsureDividerPreviewUpToDate()
    {
        if (_isDraggingDivider) return;
        if (_analysis == null || SelectedDivider == null) return;
        if (IsRenderInFlightForCurrentFrame()) return;

        if (DividerPreviewFrames.Count == 0 || _dividerPreviewRenderedFrame != SelectedDivider.Frame)
            RefreshDividerPreview();
    }

    private bool IsRenderInFlightForCurrentFrame()
    {
        return _dividerPreviewCts != null
            && _dividerPreviewTargetFrame == SelectedDivider?.Frame;
    }

    // Convert normalized 0..1 position to frame number. -1 offset because dividers cannot be placed at the very last frame.
    public void MoveDividerToPosition(RepartDividerItemVM? item, double position)
    {
        if (item == null || _analysis == null || _analysis.TotalFrames < 2) return;
        double clampedPosition = Clamp(position, 0d, 1d);
        long frame = (long)Math.Ceiling(clampedPosition * _analysis.TotalFrames) - 1;
        MoveDivider(item.Model.Id, frame);
    }

    // Convert normalized 0..1 position to frame number for new divider.
    // Clamped to [0, TotalFrames-2] because last valid divider is one frame before end.
    public void AddDividerAtPosition(double position)
    {
        if (_analysis == null || !CanAddEpisode) return;

        double clampedPosition = Clamp(position, 0d, 1d);
        long frame = (long)Math.Ceiling(clampedPosition * _analysis.TotalFrames) - 1;
        frame = Math.Min(_analysis.TotalFrames - 2, Math.Max(0, frame));

        SetNewDividerFrame(frame);
        SetNewDividerTimestamp(frame);
        AddDivider();
    }

    // Get dividers from plan, or synthesize from output boundaries (legacy compat path).
    // Filters out dividers at last frame (meaningless for empty trailing segment).
    private static List<RepartDividerM> GetPlanDividers(RepartPlanM plan)
    {
        if (plan.Dividers.Count > 0)
            return [.. plan.Dividers.OrderBy(divider => divider.Frame)];

        return [.. plan.Outputs
            .Select(output => new RepartDividerM(Guid.NewGuid(), output.LastFrame, false))
            .Where(divider => divider.Frame >= 0 && divider.Frame < plan.TotalFrames - 1)
            .OrderBy(divider => divider.Frame)];
    }

    private void LoadSources()
    {
        Sources.Clear();
        if (_analysis == null) return;
        for (int i = 0; i < _analysis.Sources.Count; i++)
        {
            RepartSourceM source = _analysis.Sources[i];
            Sources.Add(new RepartSrcItemVM(
                source.FilePath,
                source.FirstFrame,
                source.LastFrame));
        }
    }

    private void SetSourceIndexState(string filePath, RepartSourceIndexState state)
    {
        void Apply()
        {
            RepartSrcItemVM? item = Sources.FirstOrDefault(source =>
                string.Equals(source.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            item?.SetIndexState(state);
        }

        if (Application.Current?.Dispatcher?.CheckAccess() == true)
            Apply();
        else
            Application.Current?.Dispatcher.InvokeAsync(Apply);
    }

    private bool IsCachedKeyframeIndex(string filePath, KeyframeIndex index)
    {
        lock (_keyframeIndexCacheSync)
        {
            return _keyframeIndexCache.TryGetValue(filePath, out KeyframeIndex? cached)
                && ReferenceEquals(cached, index);
        }
    }

    private bool RemoveCachedKeyframeIndex(string filePath, KeyframeIndex index)
    {
        lock (_keyframeIndexCacheSync)
        {
            if (!_keyframeIndexCache.TryGetValue(filePath, out KeyframeIndex? cached)
                || !ReferenceEquals(cached, index))
                return false;

            _keyframeIndexCache.Remove(filePath);
            return true;
        }
    }

    private KeyframeIndex[] ClearKeyframeIndexCache()
    {
        lock (_keyframeIndexCacheSync)
        {
            KeyframeIndex[] indexes = [.. _keyframeIndexCache.Values];
            _keyframeIndexCache.Clear();
            return indexes;
        }
    }

    private void DisposeKeyframeIndexCache()
    {
        foreach (KeyframeIndex index in ClearKeyframeIndexCache())
            index.Dispose();
    }

    // Add a divider at the suggested position. Deduplication: select existing if frame matches.
    private void AddDivider()
    {
        if (_analysis == null || !CanAddEpisode) return;

        if (!TryGetNewDividerFrame(out long nextDivider))
        {
            ShowError(RepartLangProvider.Current["InvalidRange"]);
            return;
        }
        // Deduplication: if a divider already exists at this frame, select it instead of adding a duplicate
        if (_dividers.Any(divider => divider.Frame == nextDivider))
        {
            SelectOnlyDivider(_dividers.First(divider => divider.Frame == nextDivider).Id);
            return;
        }

        // Suppress preview refresh during batch update (ReplaceOutputs -> RefreshTimeline would fire redundant render)
        _suppressDividerPreviewRefresh = true;
        try
        {
            _dividers = [.. _dividers.Append(new RepartDividerM(Guid.NewGuid(), nextDivider, false)).OrderBy(divider => divider.Frame)];
            ReplaceOutputs(BuildDividerOutputs());
            SetSuggestedNewDividerTexts();
            SelectOnlyDivider(_dividers.First(divider => divider.Frame == nextDivider).Id);
            RefreshDividerAvailability();
        }
        finally
        {
            _suppressDividerPreviewRefresh = false;
        }

        PushEditState();
        RefreshDividerPreview();
    }

    // Select a single divider by ID. _selectedDividers (plural) holds batch operations set,
    // SelectedDivider (singular) holds the "primary" for text display.
    private void SelectOnlyDivider(Guid? id)
    {
        foreach (RepartDividerItemVM item in DividerItems)
            item.IsSelected = id.HasValue && item.Model.Id == id.Value;

        _selectedDividers = [.. DividerItems.Where(item => item.IsSelected)];
        SelectedDivider = _selectedDividers.LastOrDefault();
    }

    private void DeleteSelectedDividers()
    {
        if (!CanEdit) return;
        HashSet<Guid> ids = [.. _selectedDividers
            .Where(item => !item.IsLocked)
            .Select(item => item.Model.Id)];
        if (ids.Count == 0) return;
        _suppressDividerPreviewRefresh = true;
        try
        {
            _dividers = [.. _dividers.Where(divider => !ids.Contains(divider.Id))];
            ReplaceOutputs(BuildDividerOutputs());
            SetSuggestedNewDividerTexts();
            SelectOnlyDivider(null);
            RefreshDividerAvailability();
        }
        finally
        {
            _suppressDividerPreviewRefresh = false;
        }
        PushEditState();
        RefreshDividerPreview();
    }

    // Delete every unlocked divider on the requested side of the selected divider,
    // keeping the selected one as the anchor boundary.
    private void DeleteAdjacentDivider(int direction)
    {
        if (SelectedDivider == null) return;
        long anchorFrame = SelectedDivider.Frame;
        Guid anchorId = SelectedDivider.Model.Id;
        HashSet<Guid> ids = [.. _dividers
            .Where(divider => !divider.IsLocked
                && (direction < 0 ? divider.Frame < anchorFrame : divider.Frame > anchorFrame))
            .Select(divider => divider.Id)];
        if (ids.Count == 0) return;
        _suppressDividerPreviewRefresh = true;
        try
        {
            _dividers = [.. _dividers.Where(divider => !ids.Contains(divider.Id))];
            ReplaceOutputs(BuildDividerOutputs());
            SetSuggestedNewDividerTexts();
            SelectOnlyDivider(anchorId);
            RefreshDividerAvailability();
        }
        finally
        {
            _suppressDividerPreviewRefresh = false;
        }
        PushEditState();
        EnsureDividerPreviewUpToDate();
    }

    private void MoveSelectedDivider(long requestedFrame)
    {
        if (SelectedDivider is not { IsLocked: false } selected) return;
        MoveDivider(selected.Model.Id, requestedFrame);
    }

    // Move a divider to requested frame, clamped to [prev+1, next-1] to prevent overlap.
    // Domain invariant: dividers cannot touch the timeline edges (TotalFrames-2 is max).
    private void MoveDivider(Guid id, long requestedFrame)
    {
        if (_analysis == null) return;
        int index = _dividers.FindIndex(divider => divider.Id == id);
        if (index < 0 || _dividers[index].IsLocked) return;

        RepartDividerM selected = _dividers[index];
        long minimum = index == 0 ? 0 : _dividers[index - 1].Frame + 1;
        long maximum = index == _dividers.Count - 1
            ? _analysis.TotalFrames - 2  // Last valid divider is one frame before end
            : _dividers[index + 1].Frame - 1;
        long frame = Math.Max(minimum, Math.Min(maximum, requestedFrame));
        if (frame == selected.Frame) return;

        RepartDividerM updated = selected with { Frame = frame };
        _dividers[index] = updated;
        _dividers = [.. _dividers.OrderBy(divider => divider.Frame)];
        ReplaceOutputs(BuildDividerOutputs(), refreshTimeline: false);
        RefreshDividerItem(updated);
        SetDividerTexts(updated.Frame);
        SetSuggestedNewDividerTexts();
        SelectOnlyDivider(updated.Id);

        // Record the post-move state. A drag gesture is coalesced into a single
        // snapshot committed when the drag ends; text-edit moves push per commit.
        if (_isDraggingDivider)
            _dragEditPending = true;
        else
            PushEditState();

        RequestDividerPreviewRefresh();
    }

    private void RequestDividerPreviewRefresh()
    {
        if (_isDraggingDivider || SelectedDivider == null) return;
        RefreshDividerPreview();
    }

    private bool TryGetNewDividerFrame(out long frame)
    {
        frame = 0;
        if (_analysis == null
            || !long.TryParse(NewDividerFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
            || parsed < 0
            || parsed >= _analysis.TotalFrames - 1)
        {
            return false;
        }

        frame = parsed;
        return true;
    }

    private bool TryGetSuggestedNewDividerFrame(out long frame)
    {
        frame = 0;
        if (_analysis == null) return false;

        long lastDividerFrame = _dividers.Count == 0 ? -1 : _dividers.Max(divider => divider.Frame);
        long remainingFrames = _analysis.TotalFrames - lastDividerFrame - 1;
        if (remainingFrames < 2) return false;

        frame = lastDividerFrame + (remainingFrames / 2);
        return frame >= 0 && frame < _analysis.TotalFrames - 1;
    }

    private void SetSuggestedNewDividerTexts()
    {
        if (!TryGetSuggestedNewDividerFrame(out long frame))
        {
            SetNewDividerTexts(string.Empty, string.Empty);
            return;
        }
        SetNewDividerFrame(frame);
        SetNewDividerTimestamp(frame);
    }

    private void SetNewDividerTexts(string frameText, string timestampText)
    {
        _syncingNewDivider = true;
        _newDividerFrameText = frameText;
        _newDividerTimestampText = timestampText;
        OnPropertyChanged(nameof(NewDividerFrameText));
        OnPropertyChanged(nameof(NewDividerTimestampText));
        _syncingNewDivider = false;
    }

    private void SetNewDividerFrame(long frame)
    {
        _syncingNewDivider = true;
        _newDividerFrameText = frame.ToString(CultureInfo.InvariantCulture);
        OnPropertyChanged(nameof(NewDividerFrameText));
        _syncingNewDivider = false;
    }

    private void SetNewDividerTimestamp(long frame)
    {
        if (_analysis == null) return;
        _syncingNewDivider = true;
        _newDividerTimestampText = EncodingPipeline.FormatTimestamp(
            TimeSpan.FromSeconds((double)(frame + 1) / _analysis.FrameRate));
        OnPropertyChanged(nameof(NewDividerTimestampText));
        _syncingNewDivider = false;
    }

    private void SetDividerTexts(long? frame)
    {
        _syncingDivider = true;
        if (frame is null || _analysis == null)
        {
            _dividerFrameText = string.Empty;
            _dividerTimestampText = string.Empty;
        }
        else
        {
            _dividerFrameText = frame.Value.ToString(CultureInfo.InvariantCulture);
            _dividerTimestampText = EncodingPipeline.FormatTimestamp(
                TimeSpan.FromSeconds((double)(frame.Value + 1) / _analysis.FrameRate));
        }
        OnPropertyChanged(nameof(DividerFrameText));
        OnPropertyChanged(nameof(DividerTimestampText));
        _syncingDivider = false;
    }

    private void ClearOutputs()
    {
        if (!CanEdit || _dividers.Count == 0) return;
        _suppressDividerPreviewRefresh = true;
        try
        {
            _dividers = [];
            ReplaceOutputs(BuildDividerOutputs());
            SetSuggestedNewDividerTexts();
            SelectOnlyDivider(null);
        }
        finally
        {
            _suppressDividerPreviewRefresh = false;
        }
        PushEditState();
        RefreshDividerPreview();
    }

    // Seed history with the initial plan dividers so undo can never exceed the baseline.
    private void ResetEditHistory()
    {
        _editHistory.Clear();
        _editHistory.Add(new DividerEditSnapshot([.. _dividers]));
        _editCursor = 0;
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    // Record the current divider state after a committed edit. Truncates any redo tail,
    // then bounds history depth to MaxUndoDepth by dropping the oldest snapshot.
    private void PushEditState()
    {
        if (_editCursor < _editHistory.Count - 1)
            _editHistory.RemoveRange(_editCursor + 1, _editHistory.Count - _editCursor - 1);

        _editHistory.Add(new DividerEditSnapshot([.. _dividers]));
        if (_editHistory.Count > MaxUndoDepth)
            _editHistory.RemoveAt(0);

        _editCursor = _editHistory.Count - 1;
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    private void UndoEdit()
    {
        if (!CanUndo) return;
        _editCursor--;
        ApplySnapshot(_editHistory[_editCursor]);
    }

    private void RedoEdit()
    {
        if (!CanRedo) return;
        _editCursor++;
        ApplySnapshot(_editHistory[_editCursor]);
    }

    // Restore a saved divider state: rebuild outputs/timeline without carrying selection.
    // Undo/redo should not change the active divider selection or refresh preview.
    private void ApplySnapshot(DividerEditSnapshot snapshot)
    {
        _suppressDividerPreviewRefresh = true;
        try
        {
            _dividers = [.. snapshot.Dividers];
            ReplaceOutputs(BuildDividerOutputs());
            SetSuggestedNewDividerTexts();
            SelectOnlyDivider(null);
            RefreshDividerAvailability();
        }
        finally
        {
            _suppressDividerPreviewRefresh = false;
        }

        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    private readonly record struct DividerEditSnapshot(List<RepartDividerM> Dividers);

    private void ReplaceOutputs(IEnumerable<RepartOutputSegmentM> models, bool refreshTimeline = true)
    {
        Outputs.Clear();
        List<RepartOutputSegmentM> orderedModels = [.. models.OrderBy(model => model.FirstFrame)];
        if (_analysis != null)
        {
            foreach (RepartOutputSegmentM model in orderedModels)
                Outputs.Add(new RepartOutputItemVM(model, _analysis.FrameRateNumerator, _analysis.FrameRateDenominator));
        }
        _selectedOutputs = [];
        SelectedOutput = null;
        if (refreshTimeline) RefreshTimeline();
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(OutputCountText));
        RefreshDraftAvailability();
        RefreshDividerAvailability();
    }

    // Build output segments from dividers. When count matches original plan,
    // preserve original base names to maintain user naming across re-partition.
    private List<RepartOutputSegmentM> BuildDividerOutputs()
    {
        List<RepartOutputSegmentM> outputs = [];
        if (_analysis == null || _analysis.TotalFrames <= 0) return outputs;

        long first = 0;
        int index = 1;
        foreach (RepartDividerM divider in _dividers.Where(divider => divider.Frame >= 0 && divider.Frame < _analysis.TotalFrames - 1).OrderBy(divider => divider.Frame))
        {
            if (divider.Frame >= first)
            {
                outputs.Add(new RepartOutputSegmentM(Guid.NewGuid(), BuildEpisodeName(index++), first, divider.Frame));
                first = divider.Frame + 1;
            }
        }

        if (first < _analysis.TotalFrames)
            outputs.Add(new RepartOutputSegmentM(Guid.NewGuid(), BuildEpisodeName(index), first, _analysis.TotalFrames - 1));

        if (_analysis.Outputs.Count == outputs.Count)
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                string baseName = _analysis.Outputs[i].BaseName;
                if (!string.IsNullOrWhiteSpace(baseName))
                    outputs[i] = outputs[i] with { BaseName = baseName };
            }
        }

        return outputs;
    }

    // Rebuild timeline UI from divider models, preserving selection state.
    private void RefreshTimeline()
    {
        DividerItems.Clear();
        if (_analysis == null || _analysis.TotalFrames <= 0) return;
        Guid? selectedId = _selectedDivider?.Model.Id;
        foreach (RepartDividerM divider in _dividers.OrderBy(divider => divider.Frame))
        {
            DividerItems.Add(new RepartDividerItemVM(divider, _analysis.TotalFrames)
            {
                IsSelected = selectedId == divider.Id
            });
        }
        _selectedDividers = [.. DividerItems.Where(item => item.IsSelected)];
        _selectedDivider = DividerItems.FirstOrDefault(item => item.IsSelected);
        OnPropertyChanged(nameof(SelectedDivider));
        SetDividerTexts(_selectedDivider?.Frame);
        if (!_suppressDividerPreviewRefresh)
            RefreshDividerPreview();
    }

    private void RefreshDividerItem(RepartDividerM divider)
    {
        if (_analysis == null) return;
        RepartDividerItemVM? item = DividerItems.FirstOrDefault(item => item.Model.Id == divider.Id);
        item?.Update(divider, _analysis.TotalFrames);
    }

    private void BuildAxisLabels()
    {
        AxisLabels.Clear();
        if (_analysis == null || _analysis.TotalSeconds <= 0d) return;
        for (int i = 0; i <= 4; i++)
        {
            double seconds = _analysis.TotalSeconds * i / 4d;
            AxisLabels.Add(SampleClip.FormatAxisTimestamp(seconds));
        }
    }

    // Pre-fill draft editor with first unallocated gap, or select everything if no gap exists.
    private void PrepareNextDraft()
    {
        if (_analysis == null) return;
        RepartTimelineRangeM? gap = _analysis.BuildTimelineRanges(
            Outputs.Select(output => output.Model),
            _analysis.TotalFrames)
            .FirstOrDefault(slice => slice.IsUnallocated);
        if (gap != null) SetDraft(NewEpisodeName(), gap.FirstFrame, gap.LastFrame);
        else SetDraft(NewEpisodeName(), 0, Math.Max(0, _analysis.TotalFrames - 1));
    }

    private string NewEpisodeName()
    {
        int index = 1;
        string name;
        do { name = BuildEpisodeName(index++); }
        while (Outputs.Any(output => output.Model.BaseName.Equals(name, StringComparison.OrdinalIgnoreCase)));
        return name;
    }

    public static string BuildEpisodeName(int index, string? chapterName = null)
    {
        string rawName = $"1cenc_rp_E{index:00}_{DateTime.Now:yyyy-MM-dd}";
        string name = FilenameValidation.ToCompatibleFileName(rawName);
        if (string.IsNullOrWhiteSpace(chapterName)) return name;

        string chapterSuffix = FilenameValidation.ToCompatibleFileName(chapterName);
        return string.IsNullOrWhiteSpace(chapterSuffix) ? name : $"{name}_{chapterSuffix}";
    }

    private void LoadDraft(RepartOutputSegmentM model) => SetDraft(model.BaseName, model.FirstFrame, model.LastFrame);

    private void SetDraft(string name, long first, long last)
    {
        _syncingRange = true;
        OutputNameText = name;
        _firstFrameText = first.ToString(CultureInfo.InvariantCulture);
        _lastFrameText = last.ToString(CultureInfo.InvariantCulture);
        OnPropertyChanged(nameof(FirstFrameText));
        OnPropertyChanged(nameof(LastFrameText));
        SetTimeTexts(first, last);
        UpdateSelectionFromFrames(first, last);
        UpdateRangeDerivedTexts(first, last);
        _syncingRange = false;
        RefreshDraftAvailability();
    }

    private void SyncFramesFromTimes()
    {
        if (_analysis == null) return;
        try
        {
            long first = EncodingPipeline.TimestampToFirstFrame(StartTimeText, _analysis.FrameRate);
            long last = EncodingPipeline.TimestampToLastFrame(EndTimeText, _analysis.FrameRate);
            _syncingRange = true;
            _firstFrameText = first.ToString(CultureInfo.InvariantCulture);
            _lastFrameText = last.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(FirstFrameText));
            OnPropertyChanged(nameof(LastFrameText));
            UpdateSelectionFromFrames(first, last);
            UpdateRangeDerivedTexts(first, last);
        }
        catch { }
        finally { _syncingRange = false; }
        RefreshDraftAvailability();
    }

    private void SyncTimesFromFrames()
    {
        if (_analysis == null
            || !long.TryParse(FirstFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long first)
            || !long.TryParse(LastFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long last)
            || first < 0
            || last < first) return;
        _syncingRange = true;
        SetTimeTexts(first, last);
        UpdateSelectionFromFrames(first, last);
        UpdateRangeDerivedTexts(first, last);
        _syncingRange = false;
        RefreshDraftAvailability();
    }

    private void SetTimeTexts(long first, long last)
    {
        if (_analysis == null) return;
        _startTimeText = EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds((double)first / _analysis.FrameRate));
        _endTimeText = EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds((double)(last + 1) / _analysis.FrameRate));
        OnPropertyChanged(nameof(StartTimeText));
        OnPropertyChanged(nameof(EndTimeText));
    }

    // Sync draft from timeline selection. Handles right-to-left drag (swap on line 1128)
    // and ensures minimum segment size of 1 frame (line 1129).
    private void SyncDraftFromSelection()
    {
        if (_analysis == null || _analysis.TotalFrames <= 0 || _syncingRange) return;

        double start = Clamp(SelectionStart, 0d, 1d);
        double end = Clamp(SelectionEnd, 0d, 1d);
        if (end < start) (start, end) = (end, start);
        if (end <= start) end = Math.Min(1d, start + (1d / _analysis.TotalFrames));

        long first = Math.Min(_analysis.TotalFrames - 1, Math.Max(0, (long)Math.Floor(start * _analysis.TotalFrames)));
        long last = Math.Min(_analysis.TotalFrames - 1, Math.Max(first, (long)Math.Ceiling(end * _analysis.TotalFrames) - 1));

        _syncingRange = true;
        _firstFrameText = first.ToString(CultureInfo.InvariantCulture);
        _lastFrameText = last.ToString(CultureInfo.InvariantCulture);
        OnPropertyChanged(nameof(FirstFrameText));
        OnPropertyChanged(nameof(LastFrameText));
        SetTimeTexts(first, last);
        UpdateRangeDerivedTexts(first, last);
        _syncingRange = false;
        RefreshDraftAvailability();
    }

    private void UpdateSelectionFromFrames(long first, long last)
    {
        if (_analysis == null || _analysis.TotalFrames <= 0) return;
        long clampedFirst = Math.Min(_analysis.TotalFrames - 1, Math.Max(0, first));
        long clampedLast = Math.Min(_analysis.TotalFrames - 1, Math.Max(clampedFirst, last));
        SelectionStart = (double)clampedFirst / _analysis.TotalFrames;
        SelectionEnd = (double)(clampedLast + 1) / _analysis.TotalFrames;
    }

    private void UpdateRangeDerivedTexts(long first, long last)
    {
        if (_analysis == null || first < 0 || last < first)
        {
            SegmentDurationText = "00:00:00.000";
            FrameCountText = "0";
            return;
        }

        long frameCount = last - first + 1;
        SegmentDurationText = EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(frameCount / _analysis.FrameRate));
        FrameCountText = frameCount.ToString(CultureInfo.InvariantCulture);
    }

    private static double Clamp(double value, double min, double max) =>
        Math.Max(min, Math.Min(max, value));

    private void ApplyAndClose()
    {
        if (_analysis == null || Outputs.Count == 0)
        {
            ShowError(RepartLangProvider.Current["OutputsRequired"]);
            return;
        }
        RepartPlanM committed = _analysis.Clone();
        committed.Outputs.Clear();
        committed.Outputs.AddRange(Outputs.Select(output => output.Model).OrderBy(output => output.FirstFrame));
        committed.Dividers.Clear();
        committed.Dividers.AddRange(_dividers.OrderBy(divider => divider.Frame));
        InterruptWindowWork();
        _applyPlan(committed);
        _closeAction();
    }

    private void CancelAndClose()
    {
        InterruptWindowWork();
        _closeAction();
    }

    // Atomically cancel in-flight preview work. Interlocked.Exchange prevents race conditions
    // where two threads cancel the same CTS. Bare catch blocks handle expected ObjectDisposedException during shutdown.
    public void InterruptWindowWork()
    {
        CancellationTokenSource? dividerPreviewCts = Interlocked.Exchange(ref _dividerPreviewCts, null);

        try { dividerPreviewCts?.Cancel(); }
        catch { }

        try { _keyframeIndexLifetimeCts.Cancel(); }
        catch { }
    }

    private void RefreshAnalysisProperties()
    {
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(TimelineEndText));
        RefreshDraftAvailability();
        RefreshDividerAvailability();
    }

    // "Latest wins" cancellation pattern: cancel previous, create new, dispose after taking reference.
    // Prevents use-after-dispose race condition.
    private void RefreshDividerPreview()
    {
        CancellationTokenSource? previous = _dividerPreviewCts;
        CancellationTokenSource cts = new();
        _dividerPreviewCts = cts;
        try { previous?.Cancel(); }
        catch (ObjectDisposedException) { }
        previous?.Dispose();

        if (_analysis == null || SelectedDivider == null)
        {
            _dividerPreviewTargetFrame = null;
            _dividerPreviewRenderedFrame = null;
            DividerPreviewFrames.Clear();
            DividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewSelectDivider"];
            cts.Dispose();
            _dividerPreviewCts = null;
            return;
        }

        _dividerPreviewTargetFrame = SelectedDivider.Frame;
        long requestVersion = Interlocked.Increment(ref _dividerPreviewRequestVersion);
        _ = RefreshDividerPreviewAsync(cts, requestVersion);
    }

    // Render a 7-frame preview strip around the selected divider using ffmpeg.
    // Window is +/-3 frames for UX context. Handles source boundary spanning and seek optimization.
    private async Task RefreshDividerPreviewAsync(CancellationTokenSource cts, long requestVersion)
    {
        bool gateHeld = false;
        try
        {
            _dividerPreviewRenderedFrame = null;
            RepartDividerItemVM? divider = SelectedDivider;

            if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
            {
                RunOnUi(() =>
                {
                    DividerPreviewFrames.Clear();
                    DividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewFfmpegUnavailable"];
                });
                return;
            }

            if (_analysis == null || divider == null)
                return;

            await _dividerPreviewRenderGate.WaitAsync(cts.Token).ConfigureAwait(false);
            gateHeld = true;

            if (requestVersion != Volatile.Read(ref _dividerPreviewRequestVersion))
                return;

            long selectedFrame = divider.Frame;
            long windowFirst = Math.Max(0, selectedFrame - 3);
            long windowLast = Math.Min(_analysis.TotalFrames - 1, selectedFrame + 3);

            string runId = Guid.NewGuid().ToString("N");
            RunOnUi(() =>
            {
                DividerPreviewFrames.Clear();
                DividerPreviewStatusText = string.Format(
                    RepartLangProvider.Current["DividerPreviewReadingWindow"],
                    selectedFrame);
            });

            DividerPreviewBuildResult result = await Task
                .Run(async () => await BuildDividerPreviewAsync(selectedFrame, windowFirst, windowLast, runId, cts.Token), cts.Token)
                .ConfigureAwait(false);

            if (cts.IsCancellationRequested) return;
            if (requestVersion != Volatile.Read(ref _dividerPreviewRequestVersion)) return;

            RunOnUi(() =>
            {
                if (requestVersion != Volatile.Read(ref _dividerPreviewRequestVersion))
                    return;
                DividerPreviewFrames.Clear();
                foreach (DividerPreviewFrameVM frame in result.Frames)
                    DividerPreviewFrames.Add(frame);
                _dividerPreviewRenderedFrame = selectedFrame;
                DividerPreviewStatusText = result.StatusText;
            });
        }
        catch (OperationCanceledException)
        {
            if (!cts.IsCancellationRequested && requestVersion == Volatile.Read(ref _dividerPreviewRequestVersion))
                RunOnUi(() => DividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewCancelled"]);
        }
        catch (Exception ex)
        {
            if (requestVersion == Volatile.Read(ref _dividerPreviewRequestVersion))
            {
                RunOnUi(() =>
                {
                    DividerPreviewFrames.Clear();
                    DividerPreviewStatusText = ex.Message;
                });
            }
        }
        finally
        {
            if (gateHeld)
                _dividerPreviewRenderGate.Release();

            if (ReferenceEquals(_dividerPreviewCts, cts))
                _dividerPreviewCts = null;
            cts.Dispose();
        }
    }

    private async Task<DividerPreviewBuildResult> BuildDividerPreviewAsync(
        long selectedFrame,
        long windowFirst,
        long windowLast,
        string runId,
        CancellationToken token)
    {
        if (_analysis == null)
            throw new InvalidOperationException("Divider preview source data missing.");

        foreach (string file in Directory.GetFiles(_previewWorkDirectory, "divider-preview-*.jpg"))
        {
            try { File.Delete(file); }
            catch { }
        }

        List<DividerPreviewFrameVM> frames = [];
        List<RepartSourceM> overlapping = [.. _analysis.Sources
            .Where(source => source.LastFrame >= windowFirst && source.FirstFrame <= windowLast)
            .OrderBy(source => source.FirstFrame)];

        foreach (RepartSourceM source in overlapping)
        {
            token.ThrowIfCancellationRequested();
            if (!File.Exists(source.FilePath)) continue;

            long relFirst = Math.Max(0, Math.Max(windowFirst, source.FirstFrame) - source.FirstFrame);
            long relLast = Math.Max(relFirst, Math.Min(source.LastFrame, windowLast) - source.FirstFrame);

            double frameDuration = (double)_analysis.FrameRateDenominator / _analysis.FrameRateNumerator;
            double sourceStartTime = TryGetSourceStartTime(source.RawJson) ?? 0d;
            double targetTime = sourceStartTime + relFirst * frameDuration;

            KeyframeIndex? index = await BuildKeyframeIndexAsync(source, targetTime, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            double keyframeTime = 0d;
            bool canSeek = index != null
                && index.TryFindNearestBefore(targetTime, out keyframeTime);
            long keyframeFrame = canSeek
                ? Math.Max(0, (long)Math.Round(
                    (keyframeTime - sourceStartTime) / frameDuration,
                    MidpointRounding.AwayFromZero))
                : 0;

            string patternPrefix = $"divider-preview-{runId}-{source.FirstFrame}";
            string pattern = Path.Combine(_previewWorkDirectory, patternPrefix + "-%02d.png");

            string[] args = !canSeek
                ? PreviewPipeline.BuildSourceFrameArgs(source.FilePath, relFirst, relLast, pattern)
                : PreviewPipeline.BuildSourceFrameSeekArgs(
                    source.FilePath,
                    keyframeTime,
                    relFirst - keyframeFrame,
                    relLast - keyframeFrame,
                    pattern);

            await PreviewPipeline.RunFfmpegAsync(_ffmpegPath!, _previewWorkDirectory, args, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            string[] files = [.. Directory.GetFiles(_previewWorkDirectory, patternPrefix + "-*.png").OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

            for (int i = 0; i < files.Length; i++)
            {
                long frameNumber = source.FirstFrame + relFirst + i;
                if (frameNumber < windowFirst || frameNumber > windowLast) continue;
                bool isDivider = frameNumber == selectedFrame;
                frames.Add(new DividerPreviewFrameVM(
                    frameNumber,
                    PreviewPipeline.LoadBitmap(files[i]),
                    isDivider,
                    !isDivider));
            }
        }

        if (frames.Count == 0)
            throw new InvalidOperationException("Divider preview frame file missing.");

        string sourceName = _analysis.Sources
            .Where(source => selectedFrame >= source.FirstFrame && selectedFrame <= source.LastFrame)
            .Select(source => source.DisplayName)
            .FirstOrDefault() ?? "?";

        return new DividerPreviewBuildResult(
            frames,
            string.Format(
                RepartLangProvider.Current["DividerPreviewSummary"],
                sourceName,
                selectedFrame,
                frames.Count));
    }

    // Build or reuse a keyframe index for seek optimization. Uses cache with 30s window margin
    // for long-GOP safety. WidenWindow retry handles content with keyframes >30s apart.
    // Returns partial index on cancellation for reuse by next interaction.
    private async Task<KeyframeIndex?> BuildKeyframeIndexAsync(
        RepartSourceM source,
        double targetTime,
        CancellationToken token,
        bool widenWindow = false)
    {
        double margin = widenWindow ? KeyframeIndexWindowMarginSeconds * 2d : KeyframeIndexWindowMarginSeconds;
        double windowStart = Math.Max(0d, targetTime - margin);
        double windowEnd = targetTime + KeyframeIndexWindowLeadSeconds;

        KeyframeIndex? cached;
        lock (_keyframeIndexCacheSync)
            _keyframeIndexCache.TryGetValue(source.FilePath, out cached);

        if (cached != null && cached.CoversRange(windowStart, windowEnd, KeyframeIndexCacheReuseToleranceSeconds))
        {
            await cached.Completion.WaitAsync(token);
            return cached.Count > 0 ? cached : null;
        }

        if (string.IsNullOrWhiteSpace(_ffprobePath) || !File.Exists(_ffprobePath))
            return null;

        // The cached window no longer covers the request; replace it.
        if (cached != null)
        {
            if (RemoveCachedKeyframeIndex(source.FilePath, cached))
                cached.Dispose();
        }

        SetSourceIndexState(source.FilePath, RepartSourceIndexState.Loading);
        RunOnUi(() =>
        {
            DividerPreviewStatusText = string.Format(
                RepartLangProvider.Current["DividerPreviewBuildingIndex"],
                source.DisplayName);
        });
        KeyframeIndex index;
        try
        {
            await WarmSourceWindowCacheAsync(source, targetTime, margin, token);
            index = KeyframeIndex.Start(
                _ffprobePath,
                source.FilePath,
                _keyframeIndexLifetimeCts.Token,
                windowStart,
                windowEnd);
        }
        catch
        {
            SetSourceIndexState(source.FilePath, RepartSourceIndexState.Failed);
            throw;
        }

        lock (_keyframeIndexCacheSync)
        {
            if (_keyframeIndexCache.TryGetValue(source.FilePath, out cached))
            {
                index.Dispose();
                index = cached;
            }
            else
            {
                _keyframeIndexCache[source.FilePath] = index;
            }
        }

        if (!ReferenceEquals(index, cached))
        {
            _ = index.Completion.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    RemoveCachedKeyframeIndex(source.FilePath, index);
                }
                else if (task.IsFaulted)
                {
                    if (RemoveCachedKeyframeIndex(source.FilePath, index))
                        SetSourceIndexState(source.FilePath, RepartSourceIndexState.Failed);
                    index.Dispose();
                }
                else if (IsCachedKeyframeIndex(source.FilePath, index))
                {
                    SetSourceIndexState(source.FilePath, RepartSourceIndexState.Ready);
                }
            }, TaskScheduler.Default);
        }

        try
        {
            await index.Completion.WaitAsync(token);

            if (index.Count == 0)
            {
                // No keyframe before the target inside the window. Long-GOP
                // content: widen once; otherwise fall back to a non-seek decode.
                if (RemoveCachedKeyframeIndex(source.FilePath, index))
                    index.Dispose();

                if (!widenWindow && windowStart > 0d)
                    return await BuildKeyframeIndexAsync(source, targetTime, token, widenWindow: true);

                return null;
            }

            return index;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Keep the scan alive so the next click can reuse its partial result.
            return index;
        }
        catch
        {
            if (RemoveCachedKeyframeIndex(source.FilePath, index))
                SetSourceIndexState(source.FilePath, RepartSourceIndexState.Failed);
            index.Dispose();
            throw;
        }
    }

    private void RunOnUi(Action action)
    {
        System.Windows.Threading.Dispatcher? dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
        {
            action();
            return;
        }

        if (dispatcher.CheckAccess())
            action();
        else
            dispatcher.Invoke(action);
    }

    private readonly record struct DividerPreviewBuildResult(
        List<DividerPreviewFrameVM> Frames,
        string StatusText);

    // Best-effort sequential read of the estimated byte range covering the
    // divider window, so the OS page cache serves the subsequent ffprobe/ffmpeg
    // reads from memory instead of the hard disk.
    private async Task WarmSourceWindowCacheAsync(
        RepartSourceM source,
        double targetTime,
        double margin,
        CancellationToken token)
    {
        try
        {
            long fileLength = source.FileLength;
            if (fileLength <= 0 || _analysis == null || _analysis.FrameRate <= 0d) return;

            double duration = source.FrameCount / _analysis.FrameRate;
            if (!(duration > 0d)) return;

            double startSec = Math.Max(0d, targetTime - margin);
            double endSec = Math.Min(duration, targetTime + 1d);
            if (endSec <= startSec) return;

            long startByte = (long)(startSec / duration * fileLength);
            long endByte = Math.Min(fileLength, (long)(endSec / duration * fileLength));
            if (endByte <= startByte) return;

            using FileStream stream = new(
                source.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                1 << 20,
                FileOptions.SequentialScan);
            if (startByte > 0) stream.Seek(startByte, SeekOrigin.Begin);

            long remaining = endByte - startByte;
            byte[] buffer = new byte[1 << 20];
            while (remaining > 0)
            {
                token.ThrowIfCancellationRequested();
                int read = await stream.ReadAsync(
                    buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                    token);
                if (read <= 0) break;
                remaining -= read;
            }
        }
        catch {}
    }

    // Extract container-level start_time from ffprobe JSON. Needed because some containers
    // (e.g., MPEG-TS) have non-zero start times that shift frame-to-time calculations.
    private static double? TryGetSourceStartTime(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (document.RootElement.TryGetProperty("streams", out JsonElement streams)
                && streams.ValueKind == JsonValueKind.Array
                && streams.GetArrayLength() > 0)
            {
                double? streamStart = TryGetJsonDouble(streams[0], "start_time");
                if (streamStart != null) return streamStart;
            }

            if (document.RootElement.TryGetProperty("format", out JsonElement format))
                return TryGetJsonDouble(format, "start_time");
        }
        catch (JsonException) {}

        return null;
    }

    private static double? TryGetJsonDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double number))
            return number;
        if (value.ValueKind == JsonValueKind.String
            && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double text))
            return text;
        return null;
    }

    // Manually refresh button enable states. ButtonGroupVM does not auto-bind to
    // ICommand.CanExecute (by design) to avoid tight coupling.
    private void RefreshDraftAvailability()
    {
        OnPropertyChanged(nameof(CanAddEpisode));
    }

    private void RefreshDividerAvailability()
    {
        foreach (string property in new[]
        {
            nameof(CanDeleteSelectedDivider),
            nameof(CanDeleteLeftDivider),
            nameof(CanDeleteRightDivider),
            nameof(CanNudgeDivider),
            nameof(CanClearOutputs),
            nameof(CanUndo),
            nameof(CanRedo)
        }) OnPropertyChanged(property);

        DividerDeleteButtons.B3_1IsEnabled = CanDeleteSelectedDivider;
        DividerDeleteButtons.B3_2IsEnabled = CanDeleteLeftDivider;
        DividerDeleteButtons.B3_3IsEnabled = CanDeleteRightDivider;
        FinishButtons.B2_2IsEnabled = CanApply;
    }

    private void ShowError(string message) =>
        new OpenErrModalCmd(_modalNavS, WindowTitleText, message).Execute(null);

    private void OnLanguageChanged()
    {
        foreach (string property in new[]
        {
            nameof(InputSourcesTitle), nameof(OutputEpisodesTitle), nameof(TimelineTitle),
            nameof(TimelineControlTitle), nameof(DividerControlTitle), nameof(AddNewDividerTitle), nameof(DividerOpsTitle),
            nameof(OutputNameLabel), nameof(StartTimeLabel), nameof(SegmentDurationLabel),
            nameof(EndTimeLabel), nameof(TimeFormatText), nameof(FirstFrameLabel), nameof(FrameCountLabel),
            nameof(LastFrameLabel), nameof(FrameFormatText), nameof(AddEpisodeText),
            nameof(DeleteEpisodeText),
            nameof(FrameChangingFiltersWarning),
            nameof(ApplyText), nameof(CancelText), nameof(AddDividerText),
            nameof(DividerTimestampLabel),
            nameof(DividerFrameLabel),
            nameof(DeleteSelectedDividerText),
            nameof(DeleteLeftDividerText), nameof(DeleteRightDividerText),
            nameof(ClearDividersText), nameof(UndoEditText), nameof(RedoEditText),
            nameof(OutputCountText), nameof(TimelineHintText)
        }) OnPropertyChanged(property);
        DividerDeleteButtons.B3_1Text = DeleteEpisodeText;
        DividerDeleteButtons.B3_2Text = DeleteLeftDividerText;
        DividerDeleteButtons.B3_3Text = DeleteRightDividerText;
        FinishButtons.B2_1Text = CancelText;
        FinishButtons.B2_2Text = ApplyText;
        OnPropertyChanged(nameof(SummaryText));
        RefreshTimeline();
    }

    // Dispose order matters: cancel lifetime CTS after disposing cache (which may reference it),
    // delete preview work directory last.
    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        InterruptWindowWork();
        _keyframeIndexLifetimeCts.Cancel();
        DisposeKeyframeIndexCache();
        _keyframeIndexLifetimeCts.Dispose();
        PreviewPipeline.DeleteDirectoryQuietly(_previewWorkDirectory);
        base.Dispose();
    }
}

public sealed class DividerPreviewFrameVM(long frame, ImageSource frameImage, bool isSelected, bool isNeighbor)
{
    public long Frame { get; } = frame;
    public ImageSource FrameImage { get; } = frameImage;
    public bool IsSelected { get; } = isSelected;
    public bool IsNeighbor { get; } = isNeighbor;
    public string FrameText => $"{Frame:N0}";
}
