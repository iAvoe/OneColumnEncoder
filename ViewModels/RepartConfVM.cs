using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.Pipeline;
using OneColumnEncoder.RepartManagement;
using OneColumnEncoder.Stores;
using OneColumnEncoder.Validation;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels;

public sealed class RepartConfVM : BaseVM, IClipRangeSelectorDragAware
{
    private readonly ModalNavS _modalNavS;
    private readonly Action _closeAction;
    private readonly Func<string> _getFfprobePath;
    private readonly Func<string?>? _getFfmpegPath;
    private readonly Action<RepartPlanM> _applyPlan;
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
    private bool _syncingNewDivider;
    private bool _isBusy;
    private bool _syncingRange;
    private bool _syncingDivider;
    private bool _lastRangeInputWasTime;
    private CancellationTokenSource? _analysisCancellation;

    public RepartConfVM(
        ModalNavS modalNavS,
        Action closeAction,
        Func<string> getFfprobePath,
        Func<string?>? getFfmpegPath,
        Action<RepartPlanM> applyPlan)
    {
        _modalNavS = modalNavS;
        _closeAction = closeAction;
        _getFfprobePath = getFfprobePath;
        _getFfmpegPath = getFfmpegPath;
        _applyPlan = applyPlan;

        ImportFolderCommand = new ActionCmd(async _ => await ImportFolderAsync());
        AppendFilesCommand = new ActionCmd(async _ => await AppendFilesAsync());
        RemoveSourceCommand = new ActionCmd(async item => await RemoveSourceAsync(item as RepartSourceItemVM));
        MoveSourceUpCommand = new ActionCmd(async item => await MoveSourceAsync(item as RepartSourceItemVM, -1));
        MoveSourceDownCommand = new ActionCmd(async item => await MoveSourceAsync(item as RepartSourceItemVM, 1));
        AddEpisodeCommand = new ActionCmd(_ => AddDivider());
        ApplyEditCommand = new ActionCmd(_ => ApplyEdit());
        DeleteEpisodeCommand = new ActionCmd(_ => DeleteDivider());
        MergeLeftCommand = new ActionCmd(_ => MergeAdjacentSelected(-1));
        MergeRightCommand = new ActionCmd(_ => MergeAdjacentSelected(1));
        ResetDraftCommand = new ActionCmd(_ => ResetDraft());
        ApplyCommand = new ActionCmd(_ => ApplyAndClose());
        CancelCommand = new CloseModalCmd(closeAction);
        SelectDividerCommand = new ActionCmd(SelectDivider);
        NudgeDividerLeftCommand = new ActionCmd(_ => NudgeSelectedDivider(-1));
        NudgeDividerRightCommand = new ActionCmd(_ => NudgeSelectedDivider(1));
        ToggleDividerLockCommand = new ActionCmd(_ => ToggleSelectedDividerLock());
        DeleteSelectedDividerCommand = new ActionCmd(_ => DeleteSelectedDividers());
        DeleteLeftDividerCommand = new ActionCmd(_ => DeleteAdjacentDivider(-1));
        DeleteRightDividerCommand = new ActionCmd(_ => DeleteAdjacentDivider(1));
        ClearOutputsCommand = new ActionCmd(_ => ClearOutputs());

        InputSourceButtons = ButtonGroupVM.CreateThreeButton(
            ImportMplsText,
            AppendFilesText,
            ImportFolderText,
            null,
            AppendFilesCommand,
            ImportFolderCommand);
        InputSourceButtons.B3_1IsEnabled = false;
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
    public static string ImportFolderText => RepartLangProvider.Current["ImportFolder"];
    public static string AppendFilesText => RepartLangProvider.Current["AppendFiles"];
    public static string ImportChaptersText => RepartLangProvider.Current["ImportChapters"];
    public static string ImportMplsText => RepartLangProvider.Current["ImportMpls"];
    public static string UnavailableText => RepartLangProvider.Current["Unavailable"];
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
    public string SourceStatsText => string.Format(RepartLangProvider.Current["SourceStats"], Sources.Count);
    public string OutputCountText => string.Format(RepartLangProvider.Current["OutputCount"], Outputs.Count);
    public static string TimelineStartText => "00:00:00.000";
    public string TimelineEndText => _analysis == null
        ? "00:00:00.000"
        : EncodingPipeline.FormatTimestamp(TimeSpan.FromSeconds(_analysis.TotalSeconds));

    public ObservableCollection<RepartSourceItemVM> Sources { get; } = [];
    public ObservableCollection<RepartOutputItemVM> Outputs { get; } = [];
    public ObservableCollection<RepartDividerItemVM> DividerItems { get; } = [];
    public ObservableCollection<string> AxisLabels { get; } = [];
    public ButtonGroupVM InputSourceButtons { get; }
    public ButtonGroupVM EpisodeEditButtons { get; }
    public ButtonGroupVM DividerControlButtons { get; }
    public ButtonGroupVM DividerDeleteButtons { get; }
    public ButtonGroupVM FinishButtons { get; }
    public ICommand ImportFolderCommand { get; }
    public ICommand AppendFilesCommand { get; }
    public ICommand RemoveSourceCommand { get; }
    public ICommand MoveSourceUpCommand { get; }
    public ICommand MoveSourceDownCommand { get; }
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

    public async Task InitializeAsync(string[] filePaths, RepartPlanM? currentPlan)
    {
        if (currentPlan != null && filePaths.Length == 0)
        {
            if (currentPlan.Sources.Any(source => !source.MatchesCurrentFile()))
            {
                AnalyzeSourcesResult refreshed = await AnalyzeAndReplaceSourcesAsync(
                    [.. currentPlan.Sources.Select(source => source.FilePath)],
                    currentPlan.Outputs,
                    GetPlanDividers(currentPlan));
                if (refreshed != AnalyzeSourcesResult.Succeeded && _analysis == null) _closeAction();
                return;
            }
            _analysis = currentPlan.Clone();
            LoadSources();
            BuildAxisLabels();
            _dividers = GetPlanDividers(currentPlan);
            ReplaceOutputs(BuildDividerOutputs());
            StatusText = RepartLangProvider.Current["Ready"];
            PrepareNextDraft();
            SetSuggestedNewDividerTexts();
            RefreshAnalysisProperties();
            return;
        }
        AnalyzeSourcesResult initialized = await AnalyzeAndReplaceSourcesAsync(filePaths, [], []);
        if (initialized != AnalyzeSourcesResult.Succeeded && _analysis == null) _closeAction();
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
    }

    public void SelectDividerForInteraction(RepartDividerItemVM? item)
    {
        if (item == null) return;
        SelectOnlyDivider(item.Model.Id);
        SelectedOutput = null;
    }

    public void MoveDividerToPosition(RepartDividerItemVM? item, double position)
    {
        if (item == null || _analysis == null || _analysis.TotalFrames < 2) return;
        double clampedPosition = Clamp(position, 0d, 1d);
        long frame = (long)Math.Ceiling(clampedPosition * _analysis.TotalFrames) - 1;
        MoveDivider(item.Model.Id, frame);
    }

    private async Task ImportFolderAsync()
    {
        OpenFolderDialog dialog = new() { Title = RepartLangProvider.Current["SelectFolder"], Multiselect = false };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;
        string[] folderPaths = SourceFilePicker.GetVideoFilesInFolder(dialog.FolderName);
        if (folderPaths.Length < 2)
        {
            ShowError(RepartLangProvider.Current.MinFolderSources);
            return;
        }
        if (!ConfirmSourceMutation()) return;

        await AnalyzeAndReplaceSourcesAsync(folderPaths, [.. Outputs.Select(output => output.Model)], _dividers);
    }

    private async Task AppendFilesAsync()
    {
        OpenFileDialog dialog = new()
        {
            Title = RepartLangProvider.Current["AppendFiles"],
            Filter = new SourceFilePickerLangProvider(UILangProvider.Current.LanguageCode).VideoFilter,
            Multiselect = true
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;
        string[] existing = [.. Sources.Select(source => source.FilePath)];
        string[] paths = [.. existing.Concat(dialog.FileNames).Distinct(StringComparer.OrdinalIgnoreCase)];
        if (!ConfirmSourceMutation()) return;
        await AnalyzeAndReplaceSourcesAsync(paths, [.. Outputs.Select(output => output.Model)], _dividers);
    }

    private async Task RemoveSourceAsync(RepartSourceItemVM? item)
    {
        if (item == null || Sources.Count <= 1 || !ConfirmSourceMutation()) return;
        string[] paths = [.. Sources.Where(source => !ReferenceEquals(source, item)).Select(source => source.FilePath)];
        await AnalyzeAndReplaceSourcesAsync(paths, [.. Outputs.Select(output => output.Model)], _dividers);
    }

    private async Task MoveSourceAsync(RepartSourceItemVM? item, int direction)
    {
        if (item == null) return;
        int index = Sources.IndexOf(item);
        int target = index + direction;
        if (index < 0 || target < 0 || target >= Sources.Count || !ConfirmSourceMutation()) return;
        List<string> paths = [.. Sources.Select(source => source.FilePath)];
        (paths[index], paths[target]) = (paths[target], paths[index]);
        await AnalyzeAndReplaceSourcesAsync([.. paths], [.. Outputs.Select(output => output.Model)], _dividers);
    }

    private async Task<AnalyzeSourcesResult> AnalyzeAndReplaceSourcesAsync(
        string[] paths,
        IReadOnlyList<RepartOutputSegmentM> outputs,
        IReadOnlyList<RepartDividerM> dividers)
    {
        if (IsBusy) return AnalyzeSourcesResult.Canceled;
        _analysisCancellation?.Cancel();
        _analysisCancellation?.Dispose();
        _analysisCancellation = new CancellationTokenSource();
        IsBusy = true;
        StatusText = RepartLangProvider.Current["Analyzing"];
        try
        {
            RepartPlanM analyzed = await RepartCompatibilityAnalyzer.AnalyzeAsync(
                _getFfprobePath(),
                _getFfmpegPath?.Invoke(),
                paths,
                ConfirmDiscardInterlacedSource,
                _analysisCancellation.Token);
            List<RepartOutputSegmentM> reconciledOutputs = ReconcileOutputs(_analysis, analyzed, outputs);
            List<RepartDividerM> reconciledDividers = ReconcileDividers(_analysis, analyzed, dividers);
            _analysis = analyzed;
            _dividers = reconciledDividers;
            LoadSources();
            BuildAxisLabels();
            ReplaceOutputs(BuildDividerOutputs());
            StatusText = FormatReadyStatus(paths.Length, analyzed.Sources.Count);
            PrepareNextDraft();
            SetSuggestedNewDividerTexts();
            return AnalyzeSourcesResult.Succeeded;
        }
        catch (OperationCanceledException)
        {
            StatusText = _analysis == null ? string.Empty : RepartLangProvider.Current["Ready"];
            return AnalyzeSourcesResult.Canceled;
        }
        catch (Exception ex)
        {
            StatusText = ex.Message;
            ShowError(ex.Message);
            return AnalyzeSourcesResult.Failed;
        }
        finally
        {
            IsBusy = false;
            RefreshAnalysisProperties();
        }
    }

    private static List<RepartOutputSegmentM> ReconcileOutputs(
        RepartPlanM? oldAnalysis,
        RepartPlanM newAnalysis,
        IReadOnlyList<RepartOutputSegmentM> outputs)
    {
        if (oldAnalysis == null || outputs.Count == 0) return [.. outputs];

        Dictionary<string, int> newSourceIndexByPath = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < newAnalysis.Sources.Count; i++)
            newSourceIndexByPath[newAnalysis.Sources[i].FilePath] = i;

        List<RepartOutputSegmentM> reconciled = [];
        foreach (RepartOutputSegmentM output in outputs)
        {
            int oldStartIndex = FindSourceIndex(oldAnalysis.Sources, output.FirstFrame);
            int oldEndIndex = FindSourceIndex(oldAnalysis.Sources, output.LastFrame);
            if (oldStartIndex < 0 || oldEndIndex < oldStartIndex) continue;
            if (TryMapOutputToNewSources(
                oldAnalysis,
                newAnalysis,
                newSourceIndexByPath,
                oldStartIndex,
                oldEndIndex,
                output,
                out RepartOutputSegmentM mapped))
            {
                reconciled.Add(mapped);
            }
        }

        return [.. reconciled.OrderBy(output => output.FirstFrame)];
    }

    private static int FindSourceIndex(List<RepartSourceM> sources, long frame)
    {
        for (int i = 0; i < sources.Count; i++)
        {
            RepartSourceM source = sources[i];
            if (frame >= source.FirstFrame && frame <= source.LastFrame) return i;
        }
        return -1;
    }

    private static bool TryMapOutputToNewSources(
        RepartPlanM oldAnalysis,
        RepartPlanM newAnalysis,
        Dictionary<string, int> newSourceIndexByPath,
        int oldStartIndex,
        int oldEndIndex,
        RepartOutputSegmentM output,
        out RepartOutputSegmentM mapped)
    {
        mapped = output;
        int previousNewIndex = -1;
        int newStartIndex = -1;
        int newEndIndex = -1;

        for (int i = oldStartIndex; i <= oldEndIndex; i++)
        {
            if (!newSourceIndexByPath.TryGetValue(oldAnalysis.Sources[i].FilePath, out int newIndex)) return false;
            if (newIndex <= previousNewIndex) return false;
            if (i == oldStartIndex) newStartIndex = newIndex;
            if (i == oldEndIndex) newEndIndex = newIndex;
            previousNewIndex = newIndex;
        }

        if (newStartIndex < 0 || newEndIndex < 0) return false;
        RepartSourceM oldStartSource = oldAnalysis.Sources[oldStartIndex];
        RepartSourceM newStartSource = newAnalysis.Sources[newStartIndex];
        RepartSourceM oldEndSource = oldAnalysis.Sources[oldEndIndex];
        RepartSourceM newEndSource = newAnalysis.Sources[newEndIndex];

        long newFirst = newStartSource.FirstFrame + output.FirstFrame - oldStartSource.FirstFrame;
        long newLast = newEndSource.FirstFrame + output.LastFrame - oldEndSource.FirstFrame;
        if (newLast < newFirst || newLast - newFirst != output.LastFrame - output.FirstFrame) return false;

        mapped = output with { FirstFrame = newFirst, LastFrame = newLast };
        return true;
    }

    private static List<RepartDividerM> ReconcileDividers(
        RepartPlanM? oldAnalysis,
        RepartPlanM newAnalysis,
        IReadOnlyList<RepartDividerM> dividers)
    {
        if (dividers.Count == 0) return [];
        if (oldAnalysis == null) return NormalizeDividers(dividers, newAnalysis.TotalFrames);

        Dictionary<string, int> newSourceIndexByPath = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < newAnalysis.Sources.Count; i++)
            newSourceIndexByPath[newAnalysis.Sources[i].FilePath] = i;

        List<RepartDividerM> reconciled = [];
        foreach (RepartDividerM divider in dividers)
        {
            int oldIndex = FindSourceIndex(oldAnalysis.Sources, divider.Frame);
            if (oldIndex < 0 || !newSourceIndexByPath.TryGetValue(oldAnalysis.Sources[oldIndex].FilePath, out int newIndex))
                continue;

            RepartSourceM oldSource = oldAnalysis.Sources[oldIndex];
            RepartSourceM newSource = newAnalysis.Sources[newIndex];
            long mappedFrame = newSource.FirstFrame + divider.Frame - oldSource.FirstFrame;
            if (mappedFrame < newSource.FirstFrame || mappedFrame > newSource.LastFrame)
                continue;
            reconciled.Add(divider with { Frame = mappedFrame });
        }

        return NormalizeDividers(reconciled, newAnalysis.TotalFrames);
    }

    private static List<RepartDividerM> NormalizeDividers(IEnumerable<RepartDividerM> dividers, long totalFrames)
    {
        return [.. dividers
            .Where(divider => divider.Frame >= 0 && divider.Frame < totalFrames - 1)
            .GroupBy(divider => divider.Frame)
            .Select(group => group.First())
            .OrderBy(divider => divider.Frame)];
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

    private enum AnalyzeSourcesResult
    {
        Succeeded,
        Failed,
        Canceled
    }

    private void LoadSources()
    {
        Sources.Clear();
        if (_analysis == null)
        {
            OnPropertyChanged(nameof(SourceStatsText));
            return;
        }
        for (int i = 0; i < _analysis.Sources.Count; i++)
        {
            RepartSourceM source = _analysis.Sources[i];
            Sources.Add(new RepartSourceItemVM(
                source.FilePath,
                source.FirstFrame,
                source.LastFrame,
                null,
                MoveSourceUpCommand,
                MoveSourceDownCommand)
            {
                R2IsEnabled = i > 0,
                R3IsEnabled = i < _analysis.Sources.Count - 1
            });
        }
        OnPropertyChanged(nameof(SourceStatsText));
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

        _dividers = [.. _dividers.Append(new RepartDividerM(Guid.NewGuid(), nextDivider, false)).OrderBy(divider => divider.Frame)];
        ReplaceOutputs(BuildDividerOutputs());
        SetSuggestedNewDividerTexts();
        SelectOnlyDivider(_dividers.First(divider => divider.Frame == nextDivider).Id);
        RefreshDividerAvailability();
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
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        foreach (RepartDividerM divider in _dividers.Where(divider => divider.Frame >= 0 && divider.Frame < _analysis.TotalFrames - 1).OrderBy(divider => divider.Frame))
        {
            if (divider.Frame >= first)
            {
                outputs.Add(new RepartOutputSegmentM(Guid.NewGuid(), FormatEpisodeName(index++, timestamp), first, divider.Frame));
                first = divider.Frame + 1;
            }
        }

        if (first < _analysis.TotalFrames)
            outputs.Add(new RepartOutputSegmentM(Guid.NewGuid(), FormatEpisodeName(index, timestamp), first, _analysis.TotalFrames - 1));

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
        do { name = FormatEpisodeName(index++); }
        while (Outputs.Any(output => output.Model.BaseName.Equals(name, StringComparison.OrdinalIgnoreCase)));
        return name;
    }

    private static string FormatEpisodeName(int index, string? timestamp = null) =>
        $"1cenc_rp_E{index:00}_{timestamp ?? DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}";

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
        _applyPlan(committed);
        _closeAction();
    }

    private bool ConfirmSourceMutation()
    {
        if (Outputs.Count == 0) return true;
        OpenWarnModalCmd cmd = new(
            _modalNavS,
            RepartLangProvider.Current["SourceChangeTitle"],
            RepartLangProvider.Current["SourceChangeWarning"]);
        cmd.Execute(null);
        return cmd.DialogResult == true;
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

    private static string FormatReadyStatus(int requestedSourceCount, int acceptedSourceCount) =>
        acceptedSourceCount < requestedSourceCount
            ? string.Format(RepartLangProvider.Current["ReadyWithExcluded"], requestedSourceCount - acceptedSourceCount)
            : RepartLangProvider.Current["Ready"];

    private bool ConfirmDiscardInterlacedSource(RepartInterlacedSourceInfo source) =>
        RepartInterlacedPrompt.Confirm(_modalNavS, WindowTitleText, source);

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
            nameof(ImportFolderText), nameof(AppendFilesText), nameof(ImportChaptersText), nameof(ImportMplsText),
            nameof(UnavailableText), nameof(OutputNameLabel), nameof(StartTimeLabel), nameof(SegmentDurationLabel),
            nameof(EndTimeLabel), nameof(TimeFormatText), nameof(FirstFrameLabel), nameof(FrameCountLabel),
            nameof(LastFrameLabel), nameof(FrameFormatText), nameof(AddEpisodeText), nameof(ApplyEditText),
            nameof(DeleteEpisodeText), nameof(MergeLeftText), nameof(MergeRightText), nameof(ResetEditText),
            nameof(SourceStatsText), nameof(FrameChangingFiltersWarning),
            nameof(ApplyText), nameof(CancelText), nameof(AddDividerText),
            nameof(DividerPreviousFrameText), nameof(DividerNextFrameText), nameof(DividerTimestampLabel),
            nameof(DividerFrameLabel), nameof(LockDividerText),
            nameof(DeleteSelectedDividerText),
            nameof(DeleteLeftDividerText), nameof(DeleteRightDividerText),
            nameof(ClearDividersText), nameof(OutputCountText)
        }) OnPropertyChanged(property);
        InputSourceButtons.B3_1Text = ImportMplsText;
        InputSourceButtons.B3_2Text = AppendFilesText;
        InputSourceButtons.B3_3Text = ImportFolderText;
        foreach (RepartSourceItemVM source in Sources)
            source.RefreshLanguage();
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
        _analysisCancellation?.Cancel();
        _analysisCancellation?.Dispose();
        _analysisCancellation = null;
        base.Dispose();
    }
}
