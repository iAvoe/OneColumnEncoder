using OneColumnEncoder.Validation;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartConfVM : BaseVM, IClipRangeSelectorDragAware
{
    private readonly ModalNavS _modalNavS;
    private readonly Action _closeAction;
    private readonly Action<RepartPlanM> _applyPlan;
    private readonly string? _ffmpegPath;
    private readonly string? _ffprobePath;
    private readonly string _previewWorkDirectory;
    private const double KeyframeIndexWindowMarginSeconds = 30d;
    private const double KeyframeIndexWindowLeadSeconds = 0.25d;
    private const double KeyframeIndexCacheReuseToleranceSeconds = 1d;
    private readonly object _keyframeIndexCacheSync = new();
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
    private readonly ObservableCollection<DividerPreviewFrameVM> _dividerPreviewFrames = [];
    private string _dividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewSelectDivider"];
    private bool _suppressDividerPreviewRefresh;
    private bool _isDraggingDivider;
    private long? _dividerPreviewRenderedFrame;
    private long? _dividerPreviewTargetFrame;
    private bool _syncingNewDivider;
    private bool _isBusy;
    private bool _syncingRange;
    private bool _syncingDivider;
    private bool _lastRangeInputWasTime;

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
        ApplyEditCommand = new ActionCmd(_ => ApplyEdit());
        DeleteEpisodeCommand = new ActionCmd(_ => DeleteDivider());
        MergeLeftCommand = new ActionCmd(_ => MergeAdjacentSelected(-1));
        MergeRightCommand = new ActionCmd(_ => MergeAdjacentSelected(1));
        ResetDraftCommand = new ActionCmd(_ => ResetDraft());
        ApplyCommand = new ActionCmd(_ => ApplyAndClose());
        CancelCommand = new ActionCmd(_ => CancelAndClose());
        SelectDividerCommand = new ActionCmd(SelectDivider);
        NudgeDividerLeftCommand = new ActionCmd(_ => NudgeSelectedDivider(-1));
        NudgeDividerRightCommand = new ActionCmd(_ => NudgeSelectedDivider(1));
        ToggleDividerLockCommand = new ActionCmd(_ => ToggleSelectedDividerLock());
        DeleteSelectedDividerCommand = new ActionCmd(_ => DeleteSelectedDividers());
        DeleteLeftDividerCommand = new ActionCmd(_ => DeleteAdjacentDivider(-1));
        DeleteRightDividerCommand = new ActionCmd(_ => DeleteAdjacentDivider(1));
        ClearOutputsCommand = new ActionCmd(_ => ClearOutputs());

        DividerControlButtons = ButtonGroupVM.CreateThreeButton(
            DividerPreviousFrameText,
            DividerNextFrameText,
            LockDividerText,
            NudgeDividerLeftCommand,
            NudgeDividerRightCommand,
            ToggleDividerLockCommand);
        DividerDeleteButtons = ButtonGroupVM.CreateThreeButton(
            DeleteEpisodeText,
            DeleteLeftDividerText,
            DeleteRightDividerText,
            DeleteSelectedDividerCommand,
            DeleteLeftDividerCommand,
            DeleteRightDividerCommand);
         FinishButtons = ButtonGroupVM.CreateTwoButton(CancelText, ApplyText, CancelCommand, ApplyCommand);
        EpisodeEditButtons = ButtonGroupVM.CreateFiveButton(
            MergeLeftText,
            MergeRightText,
            DeleteEpisodeText,
            ResetEditText,
            ApplyEditText,
            MergeLeftCommand,
            MergeRightCommand,
            DeleteEpisodeCommand,
            ResetDraftCommand,
            ApplyEditCommand);
        RefreshDraftAvailability();

        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public const string WindowTitleText = "1cenc Episode Repartition";

    public static string WindowTitle => WindowTitleText;
    public static string InputSourcesTitle => RepartLangProvider.Current["InputSources"];
    public static string OutputEpisodesTitle => RepartLangProvider.Current["OutputEpisodes"];
    public static string TimelineTitle => RepartLangProvider.Current["Timeline"];
    public static string TimelineControlTitle => RepartLangProvider.Current["TimelineControl"];
    public static string DividerControlTitle => RepartLangProvider.Current["DividerControl"];
    public static string AddNewDividerTitle => RepartLangProvider.Current["AddNewDivider"];
    public static string DividerOpsTitle => RepartLangProvider.Current["ManageDividers"];
    public static string PreviewTitle => RepartLangProvider.Current["Preview"];
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
    public static string ApplyEditText => RepartLangProvider.Current["ApplyEdit"];
    public static string DeleteEpisodeText => RepartLangProvider.Current["DeleteDivider"];
    public static string MergeLeftText => RepartLangProvider.Current["MergeLeft"];
    public static string MergeRightText => RepartLangProvider.Current["MergeRight"];
    public static string ResetEditText => RepartLangProvider.Current["ResetEdit"];
    public static string FrameChangingFiltersWarning => RepartLangProvider.Current["FrameChangingFiltersWarning"];
    public static string ApplyText => RepartLangProvider.Current["Confirm"];
    public static string CancelText => RepartLangProvider.Current["Cancel"];
    public static string AddDividerText => RepartLangProvider.Current["AddDivider"];
    public static string DividerPreviousFrameText => RepartLangProvider.Current["DividerPreviousFrame"];
    public static string DividerNextFrameText => RepartLangProvider.Current["DividerNextFrame"];
    public static string DividerTimestampLabel => RepartLangProvider.Current["DividerTimestampLabel"];
    public static string DividerFrameLabel => RepartLangProvider.Current["DividerFrameLabel"];
    public string LockDividerText => SelectedDivider?.IsLocked == true
        ? RepartLangProvider.Current["UnlockDivider"]
        : RepartLangProvider.Current["LockDivider"];
    public static string DeleteSelectedDividerText => RepartLangProvider.Current["DeleteSelectedDivider"];
    public static string DeleteLeftDividerText => RepartLangProvider.Current["DeleteLeftDivider"];
    public static string DeleteRightDividerText => RepartLangProvider.Current["DeleteRightDivider"];
    public static string ClearDividersText => RepartLangProvider.Current["ClearDividers"];
    public static string TimelineHintText => RepartLangProvider.Current["TimelineHintDetailed"];
    public string OutputCountText => string.Format(RepartLangProvider.Current["OutputCount"], Outputs.Count);
    public static string TimelineStartText => "00:00:00.000";
    public string TimelineEndText => _analysis == null
        ? "00:00:00.000"
        : EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(_analysis.TotalSeconds));

    public ObservableCollection<RepartSourceItemVM> Sources { get; } = [];
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
    public ButtonGroupVM EpisodeEditButtons { get; }
    public ButtonGroupVM DividerControlButtons { get; }
    public ButtonGroupVM DividerDeleteButtons { get; }
    public ButtonGroupVM FinishButtons { get; }
    public ICommand AddEpisodeCommand { get; }
    public ICommand ApplyEditCommand { get; }
    public ICommand DeleteEpisodeCommand { get; }
    public ICommand MergeLeftCommand { get; }
    public ICommand MergeRightCommand { get; }
    public ICommand ResetDraftCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectDividerCommand { get; }
    public ICommand NudgeDividerLeftCommand { get; }
    public ICommand NudgeDividerRightCommand { get; }
    public ICommand ToggleDividerLockCommand { get; }
    public ICommand DeleteSelectedDividerCommand { get; }
    public ICommand DeleteLeftDividerCommand { get; }
    public ICommand DeleteRightDividerCommand { get; }
    public ICommand ClearOutputsCommand { get; }

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
    public bool CanMergeLeft => CanMergeAdjacent(-1);
    public bool CanMergeRight => CanMergeAdjacent(1);
    public bool CanDeleteEpisode => CanEdit && _dividers.Count > 0;
    public bool CanResetDraft => CanEdit;
    public bool CanApplyEdit
    {
        get
        {
            if (!CanEdit || SelectedOutput == null) return false;
            return TryBuildDraft(out RepartOutputSegmentM? draft, excludeSelectedName: true, showErrors: false)
                && draft != null
                && !Outputs.Where(output => output.Model.Id != SelectedOutput.Model.Id).Any(output => output.Model.Overlaps(draft));
        }
    }
    public bool CanDeleteSelectedDivider => CanEdit && _selectedDividers.Any(item => !item.IsLocked);
    public bool CanDeleteLeftDivider => CanEdit && GetAdjacentDivider(-1) is { IsLocked: false };
    public bool CanDeleteRightDivider => CanEdit && GetAdjacentDivider(1) is { IsLocked: false };
    public bool CanNudgeDivider => CanEdit && SelectedDivider is { IsLocked: false };
    public bool CanToggleDividerLock => CanEdit && SelectedDivider != null;
    public bool CanClearOutputs => CanEdit && _dividers.Count > 0;
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
            OnPropertyChanged(nameof(LockDividerText));
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

    public string StartTimeText
    {
        get => _startTimeText;
        set
        {
            if (!SetProperty(ref _startTimeText, value) || _syncingRange) return;
            _lastRangeInputWasTime = true;
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
            _lastRangeInputWasTime = true;
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
            _lastRangeInputWasTime = false;
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
            _lastRangeInputWasTime = false;
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

    public Task InitializeAsync(RepartPlanM? currentPlan)
    {
        if (currentPlan == null)
        {
            ShowError(RepartLangProvider.Current.SourceRequired);
            _closeAction();
            return Task.CompletedTask;
        }

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

    public void MoveDividerToPosition(RepartDividerItemVM? item, double position)
    {
        if (item == null || _analysis == null || _analysis.TotalFrames < 2) return;
        double clampedPosition = Clamp(position, 0d, 1d);
        long frame = (long)Math.Ceiling(clampedPosition * _analysis.TotalFrames) - 1;
        MoveDivider(item.Model.Id, frame);
    }

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
            Sources.Add(new RepartSourceItemVM(
                source.FilePath,
                source.FirstFrame,
                source.LastFrame));
        }
    }

    private void SetSourceIndexState(string filePath, RepartSourceIndexState state)
    {
        void Apply()
        {
            RepartSourceItemVM? item = Sources.FirstOrDefault(source =>
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

    private void AddDivider()
    {
        if (_analysis == null || !CanAddEpisode) return;

        if (!TryGetNewDividerFrame(out long nextDivider))
        {
            ShowError(RepartLangProvider.Current["InvalidRange"]);
            return;
        }
        if (_dividers.Any(divider => divider.Frame == nextDivider))
        {
            SelectOnlyDivider(_dividers.First(divider => divider.Frame == nextDivider).Id);
            return;
        }

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

        RefreshDividerPreview();
    }

    private void ApplyEdit()
    {
        if (SelectedOutput == null || !TryBuildDraft(out RepartOutputSegmentM? draft, excludeSelectedName: true, showErrors: true) || draft == null) return;
        RepartOutputSegmentM replacement = draft with { Id = SelectedOutput.Model.Id };
        if (Outputs.Where(output => output.Model.Id != replacement.Id).Any(output => output.Model.Overlaps(replacement)))
        {
            ShowError(RepartLangProvider.Current["Overlap"]);
            return;
        }
        ReplaceOutputs(Outputs.Select(output => output.Model.Id == replacement.Id ? replacement : output.Model));
        SelectedOutput = Outputs.FirstOrDefault(output => output.Model.Id == replacement.Id);
    }

    private void DeleteDivider()
    {
        DeleteSelectedDividers();
    }

    private void SelectDivider(object? parameter)
    {
        if (parameter is not RepartDividerItemVM item) return;
        SelectOnlyDivider(item.Model.Id);
        SelectedOutput = null;
    }

    private void SelectOnlyDivider(Guid? id)
    {
        foreach (RepartDividerItemVM item in DividerItems)
            item.IsSelected = id.HasValue && item.Model.Id == id.Value;

        _selectedDividers = [.. DividerItems.Where(item => item.IsSelected)];
        SelectedDivider = _selectedDividers.LastOrDefault();
    }

    private RepartDividerItemVM? GetAdjacentDivider(int direction)
    {
        if (SelectedDivider == null) return null;
        List<RepartDividerItemVM> ordered = [.. DividerItems.OrderBy(item => item.Frame)];
        int index = ordered.IndexOf(SelectedDivider);
        int target = index + Math.Sign(direction);
        return index >= 0 && target >= 0 && target < ordered.Count ? ordered[target] : null;
    }

    private void DeleteSelectedDividers()
    {
        if (!CanEdit) return;
        HashSet<Guid> ids = [.. _selectedDividers
            .Where(item => !item.IsLocked)
            .Select(item => item.Model.Id)];
        if (ids.Count == 0) return;
        _dividers = [.. _dividers.Where(divider => !ids.Contains(divider.Id))];
        ReplaceOutputs(BuildDividerOutputs());
        SetSuggestedNewDividerTexts();
        SelectOnlyDivider(null);
        RefreshDividerAvailability();
    }

    private void DeleteAdjacentDivider(int direction)
    {
        RepartDividerItemVM? divider = GetAdjacentDivider(direction);
        if (divider is not { IsLocked: false }) return;
        _dividers = [.. _dividers.Where(item => item.Id != divider.Model.Id)];
        ReplaceOutputs(BuildDividerOutputs());
        SetSuggestedNewDividerTexts();
        SelectOnlyDivider(null);
        RefreshDividerAvailability();
    }

    private void NudgeSelectedDivider(int direction)
    {
        if (SelectedDivider is not { IsLocked: false }) return;
        MoveSelectedDivider(SelectedDivider.Frame + Math.Sign(direction));
    }

    private void MoveSelectedDivider(long requestedFrame)
    {
        if (SelectedDivider is not { IsLocked: false } selected) return;
        MoveDivider(selected.Model.Id, requestedFrame);
    }

    private void MoveDivider(Guid id, long requestedFrame)
    {
        if (_analysis == null) return;
        int index = _dividers.FindIndex(divider => divider.Id == id);
        if (index < 0 || _dividers[index].IsLocked) return;

        RepartDividerM selected = _dividers[index];
        long minimum = index == 0 ? 0 : _dividers[index - 1].Frame + 1;
        long maximum = index == _dividers.Count - 1
            ? _analysis.TotalFrames - 2
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
        RequestDividerPreviewRefresh();
    }

    private void RequestDividerPreviewRefresh()
    {
        if (_isDraggingDivider || SelectedDivider == null) return;
        RefreshDividerPreview();
    }

    private void ToggleSelectedDividerLock()
    {
        if (!CanToggleDividerLock || SelectedDivider == null) return;
        Guid id = SelectedDivider.Model.Id;
        _dividers = [.. _dividers.Select(divider => divider.Id == id
            ? divider with { IsLocked = !divider.IsLocked }
            : divider)];
        RefreshTimeline();
        SelectOnlyDivider(id);
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
        if (!CanEdit) return;
        _dividers = [];
        ReplaceOutputs(BuildDividerOutputs());
        SetSuggestedNewDividerTexts();
        SelectOnlyDivider(null);
    }

    private void MergeAdjacentSelected(int direction)
    {
        if (SelectedOutput == null || !TryGetAdjacentOutput(direction, out RepartOutputItemVM? adjacent) || adjacent == null) return;
        RepartOutputItemVM left = direction < 0 ? adjacent : SelectedOutput;
        RepartOutputItemVM right = direction < 0 ? SelectedOutput : adjacent;
        if (!left.Model.IsAdjacentTo(right.Model)) return;

        RepartOutputSegmentM merged = new(
            left.Model.Id,
            left.Model.BaseName,
            left.Model.FirstFrame,
            right.Model.LastFrame);
        HashSet<Guid> mergedIds = [left.Model.Id, right.Model.Id];
        ReplaceOutputs(Outputs.Where(output => !mergedIds.Contains(output.Model.Id)).Select(output => output.Model).Append(merged));
        SelectedOutput = Outputs.FirstOrDefault(output => output.Model.Id == merged.Id);
    }

    private void ResetDraft()
    {
        if (SelectedOutput != null) LoadDraft(SelectedOutput.Model);
        else PrepareNextDraft();
    }

    private bool TryBuildDraft(out RepartOutputSegmentM? segment, bool excludeSelectedName, bool showErrors)
    {
        segment = null;
        long first;
        long last;
        try
        {
            if (_lastRangeInputWasTime && _analysis != null)
            {
                first = EncodingPipeline.TimestampToFirstFrame(StartTimeText, _analysis.FrameRate);
                last = EncodingPipeline.TimestampToLastFrame(EndTimeText, _analysis.FrameRate);
            }
            else if (!long.TryParse(FirstFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out first)
                     || !long.TryParse(LastFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out last))
            {
                throw new FormatException();
            }
        }
        catch
        {
            if (showErrors) ShowError(RepartLangProvider.Current["InvalidRange"]);
            return false;
        }

        if (_analysis == null
            || first < 0 || last < first || last >= _analysis.TotalFrames)
        {
            if (showErrors) ShowError(RepartLangProvider.Current["InvalidRange"]);
            return false;
        }

        string baseName = Path.GetFileNameWithoutExtension(OutputNameText.Trim());
        if (string.IsNullOrWhiteSpace(baseName)
            || !FilenameValidation.IsValidLength(baseName)
            || !FilenameValidation.HasNoInvalidChars(baseName)
            || !FilenameValidation.IsNotReservedName(baseName)
            || Outputs.Any(output => (!excludeSelectedName || output.Model.Id != SelectedOutput?.Model.Id)
                && output.Model.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
        {
            if (showErrors) ShowError(RepartLangProvider.Current["UniqueName"]);
            return false;
        }

        segment = new RepartOutputSegmentM(Guid.NewGuid(), baseName, first, last);
        return true;
    }

    private bool CanMergeAdjacent(int direction) =>
        CanEdit
        && SelectedOutput != null
        && TryGetAdjacentOutput(direction, out RepartOutputItemVM? adjacent)
        && adjacent != null
        && SelectedOutput.Model.IsAdjacentTo(adjacent.Model);

    private bool TryGetAdjacentOutput(int direction, out RepartOutputItemVM? adjacent)
    {
        adjacent = null;
        if (SelectedOutput == null) return false;
        List<RepartOutputItemVM> ordered = [.. Outputs.OrderBy(output => output.Model.FirstFrame)];
        int index = ordered.IndexOf(SelectedOutput);
        int adjacentIndex = index + Math.Sign(direction);
        if (index < 0 || adjacentIndex < 0 || adjacentIndex >= ordered.Count) return false;
        adjacent = ordered[adjacentIndex];
        return true;
    }

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
        OnPropertyChanged(nameof(LockDividerText));
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
        _lastRangeInputWasTime = false;
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
        _lastRangeInputWasTime = false;
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

    private void RefreshDividerPreview()
    {
        CancellationTokenSource? previous = _dividerPreviewCts;
        CancellationTokenSource cts = new();
        _dividerPreviewCts = cts;
        previous?.Cancel();
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
        _ = RefreshDividerPreviewAsync(cts);
    }

    private async Task RefreshDividerPreviewAsync(CancellationTokenSource cts)
    {
        try
        {
            _dividerPreviewRenderedFrame = null;
            RepartDividerItemVM? divider = SelectedDivider;

            if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
            {
                DividerPreviewFrames.Clear();
                DividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewFfmpegUnavailable"];
                return;
            }

            if (_analysis == null || divider == null)
                return;

            long selectedFrame = divider.Frame;
            long windowFirst = Math.Max(0, selectedFrame - 3);
            long windowLast = Math.Min(_analysis.TotalFrames - 1, selectedFrame + 3);

            string runId = Guid.NewGuid().ToString("N");
            DividerPreviewFrames.Clear();
            DividerPreviewStatusText = string.Format(
                RepartLangProvider.Current["DividerPreviewReadingWindow"],
                selectedFrame);

            foreach (string file in Directory.GetFiles(_previewWorkDirectory, "divider-preview-*.jpg"))
            {
                try { File.Delete(file); }
                catch { }
            }

            List<RepartSourceM> overlapping = [.. _analysis.Sources
                .Where(source => source.LastFrame >= windowFirst && source.FirstFrame <= windowLast)
                .OrderBy(source => source.FirstFrame)];

            foreach (RepartSourceM source in overlapping)
            {
                if (cts.IsCancellationRequested) return;
                if (!File.Exists(source.FilePath)) continue;

                long relFirst = Math.Max(0, Math.Max(windowFirst, source.FirstFrame) - source.FirstFrame);
                long relLast = Math.Max(relFirst, Math.Min(source.LastFrame, windowLast) - source.FirstFrame);

                double frameDuration = (double)_analysis.FrameRateDenominator / _analysis.FrameRateNumerator;
                double sourceStartTime = TryGetSourceStartTime(source.RawJson) ?? 0d;
                double targetTime = sourceStartTime + relFirst * frameDuration;

                KeyframeIndex? index = await BuildKeyframeIndexAsync(source, targetTime, cts.Token);
                if (cts.IsCancellationRequested) return;

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

                await PreviewPipeline.RunFfmpegAsync(_ffmpegPath, _previewWorkDirectory, args, cts.Token);

                if (cts.IsCancellationRequested) return;

                string[] files = Directory.GetFiles(_previewWorkDirectory, patternPrefix + "-*.png")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                for (int i = 0; i < files.Length; i++)
                {
                    long frameNumber = source.FirstFrame + relFirst + i;
                    if (frameNumber < windowFirst || frameNumber > windowLast) continue;
                    DividerPreviewFrames.Add(new DividerPreviewFrameVM(
                        frameNumber,
                        PreviewPipeline.LoadBitmap(files[i]),
                        frameNumber == selectedFrame));
                }
            }

            if (DividerPreviewFrames.Count == 0)
                throw new InvalidOperationException("Divider preview frame file missing.");

            string sourceName = _analysis.Sources
                .Where(source => selectedFrame >= source.FirstFrame && selectedFrame <= source.LastFrame)
                .Select(source => source.DisplayName)
                .FirstOrDefault() ?? "?";
            _dividerPreviewRenderedFrame = selectedFrame;
            DividerPreviewStatusText = string.Format(
                RepartLangProvider.Current["DividerPreviewSummary"],
                sourceName,
                selectedFrame,
                DividerPreviewFrames.Count);
        }
        catch (OperationCanceledException)
        {
            if (!cts.IsCancellationRequested)
                DividerPreviewStatusText = RepartLangProvider.Current["DividerPreviewCancelled"];
        }
        catch (Exception ex)
        {
            DividerPreviewFrames.Clear();
            DividerPreviewStatusText = ex.Message;
        }
        finally
        {
            if (ReferenceEquals(_dividerPreviewCts, cts))
                _dividerPreviewCts = null;
            cts.Dispose();
        }
    }

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
        DividerPreviewStatusText = string.Format(
            RepartLangProvider.Current["DividerPreviewBuildingIndex"],
            source.DisplayName);
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
        catch
        {
        }
    }

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
        catch (JsonException) { }

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

    private void RefreshDraftAvailability()
    {
        OnPropertyChanged(nameof(CanAddEpisode));
        OnPropertyChanged(nameof(CanMergeLeft));
        OnPropertyChanged(nameof(CanMergeRight));
        OnPropertyChanged(nameof(CanDeleteEpisode));
        OnPropertyChanged(nameof(CanResetDraft));
        OnPropertyChanged(nameof(CanApplyEdit));
        EpisodeEditButtons.B5_1IsEnabled = CanMergeLeft;
        EpisodeEditButtons.B5_2IsEnabled = CanMergeRight;
        EpisodeEditButtons.B5_3IsEnabled = CanDeleteEpisode;
        EpisodeEditButtons.B5_4IsEnabled = CanResetDraft;
        EpisodeEditButtons.B5_5IsEnabled = CanApplyEdit;
    }

    private void RefreshDividerAvailability()
    {
        foreach (string property in new[]
        {
            nameof(CanDeleteSelectedDivider), nameof(CanDeleteLeftDivider), nameof(CanDeleteRightDivider), nameof(CanNudgeDivider),
            nameof(CanToggleDividerLock), nameof(CanClearOutputs)
        }) OnPropertyChanged(property);

        DividerControlButtons.B3_1IsEnabled = CanNudgeDivider;
        DividerControlButtons.B3_2IsEnabled = CanNudgeDivider;
        DividerControlButtons.B3_3IsEnabled = CanToggleDividerLock;
        DividerControlButtons.B3_3Text = LockDividerText;
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
            nameof(LastFrameLabel), nameof(FrameFormatText), nameof(AddEpisodeText), nameof(ApplyEditText),
            nameof(DeleteEpisodeText), nameof(MergeLeftText), nameof(MergeRightText), nameof(ResetEditText),
            nameof(FrameChangingFiltersWarning),
            nameof(ApplyText), nameof(CancelText), nameof(AddDividerText),
            nameof(DividerPreviousFrameText), nameof(DividerNextFrameText), nameof(DividerTimestampLabel),
            nameof(DividerFrameLabel), nameof(LockDividerText),
            nameof(DeleteSelectedDividerText),
            nameof(DeleteLeftDividerText), nameof(DeleteRightDividerText),
            nameof(ClearDividersText), nameof(OutputCountText), nameof(TimelineHintText)
        }) OnPropertyChanged(property);
        EpisodeEditButtons.B5_1Text = MergeLeftText;
        EpisodeEditButtons.B5_2Text = MergeRightText;
        EpisodeEditButtons.B5_3Text = DeleteEpisodeText;
        EpisodeEditButtons.B5_4Text = ResetEditText;
        EpisodeEditButtons.B5_5Text = ApplyEditText;
        DividerControlButtons.B3_1Text = DividerPreviousFrameText;
        DividerControlButtons.B3_2Text = DividerNextFrameText;
        DividerControlButtons.B3_3Text = LockDividerText;
        DividerDeleteButtons.B3_1Text = DeleteEpisodeText;
        DividerDeleteButtons.B3_2Text = DeleteLeftDividerText;
        DividerDeleteButtons.B3_3Text = DeleteRightDividerText;
        FinishButtons.B2_1Text = CancelText;
        FinishButtons.B2_2Text = ApplyText;
        OnPropertyChanged(nameof(SummaryText));
        RefreshTimeline();
    }

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

public sealed class DividerPreviewFrameVM
{
    public DividerPreviewFrameVM(long frame, ImageSource frameImage, bool isSelected)
    {
        Frame = frame;
        FrameImage = frameImage;
        IsSelected = isSelected;
    }

    public long Frame { get; }
    public ImageSource FrameImage { get; }
    public bool IsSelected { get; }
    public string FrameText => $"{Frame:N0}";
}
