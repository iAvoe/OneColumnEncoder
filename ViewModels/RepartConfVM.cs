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
    private List<long> _dividerFrames = [];
    private bool _isBusy;
    private bool _syncingRange;
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
        SelectTimelineSliceCommand = new ActionCmd(SelectTimelineSlice);
        ApplyCommand = new ActionCmd(_ => ApplyAndClose());
        CancelCommand = new CloseModalCmd(closeAction);

        InputSourceButtons = ButtonGroupVM.CreateThreeButton(
            ImportMplsText,
            AppendFilesText,
            ImportFolderText,
            null,
            AppendFilesCommand,
            ImportFolderCommand);
        InputSourceButtons.B3_1IsEnabled = false;
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

    public string WindowTitle => WindowTitleText;
    public string InputSourcesTitle => RepartLangProvider.Current["InputSources"];
    public string OutputEpisodesTitle => RepartLangProvider.Current["OutputEpisodes"];
    public string TimelineTitle => RepartLangProvider.Current["Timeline"];
    public string ImportFolderText => RepartLangProvider.Current["ImportFolder"];
    public string AppendFilesText => RepartLangProvider.Current["AppendFiles"];
    public string ImportChaptersText => RepartLangProvider.Current["ImportChapters"];
    public string ImportMplsText => RepartLangProvider.Current["ImportMpls"];
    public string UnavailableText => RepartLangProvider.Current["Unavailable"];
    public string OutputNameLabel => RepartLangProvider.Current["OutputName"];
    public string StartTimeLabel => RepartLangProvider.Current["StartTime"];
    public string SegmentDurationLabel => RepartLangProvider.Current["SegmentDuration"];
    public string EndTimeLabel => RepartLangProvider.Current["EndTime"];
    public string FirstFrameLabel => RepartLangProvider.Current["FirstFrame"];
    public string FrameCountLabel => RepartLangProvider.Current["FrameCount"];
    public string LastFrameLabel => RepartLangProvider.Current["LastFrame"];
    public string TimeFormatText => RepartLangProvider.Current["TimeFormat"];
    public string FrameFormatText => RepartLangProvider.Current["FrameFormat"];
    public string AddEpisodeText => RepartLangProvider.Current["AddDivider"];
    public string ApplyEditText => RepartLangProvider.Current["ApplyEdit"];
    public string DeleteEpisodeText => RepartLangProvider.Current["DeleteDivider"];
    public string MergeLeftText => RepartLangProvider.Current["MergeLeft"];
    public string MergeRightText => RepartLangProvider.Current["MergeRight"];
    public string ResetEditText => RepartLangProvider.Current["ResetEdit"];
    public string FrameChangingFiltersWarning => RepartLangProvider.Current["FrameChangingFiltersWarning"];
    public string ApplyText => RepartLangProvider.Current["Apply"];
    public string CancelText => RepartLangProvider.Current["Cancel"];
    public string SourceStatsText => string.Format(RepartLangProvider.Current["SourceStats"], Sources.Count);

    public ObservableCollection<RepartSourceItemVM> Sources { get; } = [];
    public ObservableCollection<RepartOutputItemVM> Outputs { get; } = [];
    public ObservableCollection<RepartTimelineSliceVM> TimelineSlices { get; } = [];
    public ObservableCollection<string> AxisLabels { get; } = [];
    public ButtonGroupVM InputSourceButtons { get; }
    public ButtonGroupVM EpisodeEditButtons { get; }
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
    public ICommand SelectTimelineSliceCommand { get; }
    public ICommand ApplyCommand { get; }
    public ICommand CancelCommand { get; }

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
            if (!CanEdit || _analysis == null || _analysis.TotalFrames < 2) return false;
            long totalLastFrame = _analysis.TotalFrames - 1;
            return _dividerFrames.Count == 0 || _dividerFrames[^1] < totalLastFrame - 1;
        }
    }
    public bool CanMergeLeft => CanMergeAdjacent(-1);
    public bool CanMergeRight => CanMergeAdjacent(1);
    public bool CanDeleteEpisode => CanEdit && _dividerFrames.Count > 0;
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
            if (value != null) LoadDraft(value.Model);
            RefreshDraftAvailability();
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
                    currentPlan.Sources.Select(source => source.FilePath).ToArray(),
                    currentPlan.Outputs);
                if (refreshed != AnalyzeSourcesResult.Succeeded && _analysis == null) _closeAction();
                return;
            }
            _analysis = currentPlan.Clone();
            LoadSources();
            BuildAxisLabels();
            ReplaceOutputs(currentPlan.Outputs);
            StatusText = RepartLangProvider.Current["Ready"];
            PrepareNextDraft();
            RefreshAnalysisProperties();
            return;
        }
        AnalyzeSourcesResult initialized = await AnalyzeAndReplaceSourcesAsync(filePaths, []);
        if (initialized != AnalyzeSourcesResult.Succeeded && _analysis == null) _closeAction();
    }

    public void SetSelectedOutputs(IEnumerable<RepartOutputItemVM> items)
    {
        _selectedOutputs = items.OrderBy(item => item.Model.FirstFrame).ToList();
        foreach (RepartOutputItemVM output in Outputs)
            output.IsSelected = _selectedOutputs.Contains(output);
        SelectedOutput = _selectedOutputs.LastOrDefault();
    }

    public void SetDraggingSelection(bool isDraggingSelection)
    {
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

        await AnalyzeAndReplaceSourcesAsync(folderPaths, Outputs.Select(output => output.Model).ToList());
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
        string[] existing = Sources.Select(source => source.FilePath).ToArray();
        string[] paths = [.. existing.Concat(dialog.FileNames).Distinct(StringComparer.OrdinalIgnoreCase)];
        if (!ConfirmSourceMutation()) return;
        await AnalyzeAndReplaceSourcesAsync(paths, Outputs.Select(output => output.Model).ToList());
    }

    private async Task RemoveSourceAsync(RepartSourceItemVM? item)
    {
        if (item == null || Sources.Count <= 1 || !ConfirmSourceMutation()) return;
        string[] paths = Sources.Where(source => !ReferenceEquals(source, item)).Select(source => source.FilePath).ToArray();
        await AnalyzeAndReplaceSourcesAsync(paths, Outputs.Select(output => output.Model).ToList());
    }

    private async Task MoveSourceAsync(RepartSourceItemVM? item, int direction)
    {
        if (item == null) return;
        int index = Sources.IndexOf(item);
        int target = index + direction;
        if (index < 0 || target < 0 || target >= Sources.Count || !ConfirmSourceMutation()) return;
        List<string> paths = Sources.Select(source => source.FilePath).ToList();
        (paths[index], paths[target]) = (paths[target], paths[index]);
        await AnalyzeAndReplaceSourcesAsync([.. paths], Outputs.Select(output => output.Model).ToList());
    }

    private async Task<AnalyzeSourcesResult> AnalyzeAndReplaceSourcesAsync(
        string[] paths,
        IReadOnlyList<RepartOutputSegmentM> outputs)
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
            _analysis = analyzed;
            LoadSources();
            BuildAxisLabels();
            ReplaceOutputs(reconciledOutputs);
            StatusText = FormatReadyStatus(paths.Length, analyzed.Sources.Count);
            PrepareNextDraft();
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

        return reconciled.OrderBy(output => output.FirstFrame).ToList();
    }

    private static int FindSourceIndex(IReadOnlyList<RepartSourceM> sources, long frame)
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

        long totalLastFrame = _analysis.TotalFrames - 1;
        long baseFrame = _dividerFrames.Count > 0 ? _dividerFrames[^1] : 0;
        long nextDivider = baseFrame + (totalLastFrame - baseFrame) / 2;
        if (_dividerFrames.Count > 0 && nextDivider <= baseFrame) return;
        if (nextDivider >= totalLastFrame) return;

        _dividerFrames.Add(nextDivider);
        _dividerFrames = [.. _dividerFrames.Distinct().OrderBy(frame => frame)];
        ReplaceOutputs(BuildDividerOutputs(), syncDividers: false);
        SelectedOutput = Outputs.FirstOrDefault(output => output.Model.LastFrame == nextDivider);
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
        if (_analysis == null || _dividerFrames.Count == 0) return;

        long? selectedDivider = GetSelectedDividerFrame();
        long divider = selectedDivider ?? _dividerFrames[^1];
        _dividerFrames.Remove(divider);
        ReplaceOutputs(BuildDividerOutputs(), syncDividers: false);
        SelectedOutput = Outputs.FirstOrDefault(output => output.Model.FirstFrame > divider)
            ?? Outputs.LastOrDefault();
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
        List<RepartOutputItemVM> ordered = Outputs.OrderBy(output => output.Model.FirstFrame).ToList();
        int index = ordered.IndexOf(SelectedOutput);
        int adjacentIndex = index + Math.Sign(direction);
        if (index < 0 || adjacentIndex < 0 || adjacentIndex >= ordered.Count) return false;
        adjacent = ordered[adjacentIndex];
        return true;
    }

    private void ReplaceOutputs(IEnumerable<RepartOutputSegmentM> models, bool syncDividers = true)
    {
        Outputs.Clear();
        List<RepartOutputSegmentM> orderedModels = [.. models.OrderBy(model => model.FirstFrame)];
        if (_analysis != null)
        {
            if (syncDividers)
                SyncDividersFromOutputs(orderedModels);

            foreach (RepartOutputSegmentM model in orderedModels)
                Outputs.Add(new RepartOutputItemVM(model, _analysis.FrameRateNumerator, _analysis.FrameRateDenominator));
        }
        _selectedOutputs = [];
        SelectedOutput = null;
        RefreshTimeline();
        OnPropertyChanged(nameof(CanApply));
        RefreshDraftAvailability();
    }

    private List<RepartOutputSegmentM> BuildDividerOutputs()
    {
        List<RepartOutputSegmentM> outputs = [];
        if (_analysis == null || _analysis.TotalFrames <= 0) return outputs;

        long first = 0;
        int index = 1;
        foreach (long divider in _dividerFrames.Where(frame => frame >= 0 && frame < _analysis.TotalFrames - 1).Distinct().OrderBy(frame => frame))
        {
            if (divider >= first)
            {
                outputs.Add(new RepartOutputSegmentM(Guid.NewGuid(), FormatEpisodeName(index++), first, divider));
                first = divider + 1;
            }
        }

        if (first < _analysis.TotalFrames)
            outputs.Add(new RepartOutputSegmentM(Guid.NewGuid(), FormatEpisodeName(index), first, _analysis.TotalFrames - 1));

        return outputs;
    }

    private void SyncDividersFromOutputs(IReadOnlyList<RepartOutputSegmentM> models)
    {
        if (_analysis == null || _analysis.TotalFrames <= 0)
        {
            _dividerFrames = [];
            return;
        }

        long totalLastFrame = _analysis.TotalFrames - 1;
        _dividerFrames = [.. models
            .Select(model => model.LastFrame)
            .Where(frame => frame >= 0 && frame < totalLastFrame)
            .Distinct()
            .OrderBy(frame => frame)];
    }

    private long? GetSelectedDividerFrame()
    {
        RepartOutputItemVM? selected = _selectedOutputs.LastOrDefault() ?? SelectedOutput;
        if (selected == null) return null;
        long rightDivider = selected.Model.LastFrame;
        if (_dividerFrames.Contains(rightDivider)) return rightDivider;
        long leftDivider = selected.Model.FirstFrame - 1;
        return _dividerFrames.Contains(leftDivider) ? leftDivider : null;
    }

    private void RefreshTimeline()
    {
        TimelineSlices.Clear();
        if (_analysis == null || _analysis.TotalFrames <= 0) return;
        int palette = 0;
        foreach (RepartTimelineRangeM range in _analysis.BuildTimelineRanges(
            Outputs.Select(output => output.Model),
            _analysis.TotalFrames))
        {
            if (range.IsUnallocated)
            {
                TimelineSlices.Add(new RepartTimelineSliceVM(
                    null,
                    RepartLangProvider.Current["Unallocated"],
                    $"{range.FirstFrame:N0} - {range.LastFrame:N0}",
                    range.FirstFrame,
                    range.LastFrame,
                    true,
                    0));
                continue;
            }
            RepartOutputItemVM output = Outputs.First(item => item.Model.Id == range.OutputId);
            TimelineSlices.Add(new RepartTimelineSliceVM(
                output.Model.Id,
                output.Model.BaseName,
                output.P1Text,
                output.Model.FirstFrame,
                output.Model.LastFrame,
                false,
                palette++ % 4));
        }
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

    private void SelectTimelineSlice(object? parameter)
    {
        if (parameter is not RepartTimelineSliceVM slice) return;
        if (slice.OutputId is Guid outputId)
        {
            RepartOutputItemVM? output = Outputs.FirstOrDefault(item => item.Model.Id == outputId);
            _selectedOutputs = output == null ? [] : [output];
            foreach (RepartOutputItemVM item in Outputs) item.IsSelected = ReferenceEquals(item, output);
            SelectedOutput = output;
            return;
        }
        _selectedOutputs = [];
        foreach (RepartOutputItemVM item in Outputs) item.IsSelected = false;
        SelectedOutput = null;
        SetDraft(NewEpisodeName(), slice.FirstFrame, slice.LastFrame);
    }

    private void PrepareNextDraft()
    {
        if (_analysis == null) return;
        RepartTimelineSliceVM? gap = TimelineSlices.FirstOrDefault(slice => slice.IsUnallocated);
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

    private static string FormatEpisodeName(int index) => $"EP{index:00}";

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
        _applyPlan(committed);
        _closeAction();
    }

    private bool ConfirmSourceMutation()
    {
        if (Outputs.Count == 0) return true;
        MessageBoxResult result = MessageBox.Show(
            RepartLangProvider.Current["SourceChangeWarning"],
            RepartLangProvider.Current["SourceChangeTitle"],
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        return result == MessageBoxResult.OK;
    }

    private void RefreshAnalysisProperties()
    {
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanApply));
        RefreshDraftAvailability();
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

    private void ShowError(string message) =>
        new OpenErrModalCmd(_modalNavS, WindowTitleText, message).Execute(null);

    private void OnLanguageChanged()
    {
        foreach (string property in new[]
        {
            nameof(InputSourcesTitle), nameof(OutputEpisodesTitle), nameof(TimelineTitle),
            nameof(ImportFolderText), nameof(AppendFilesText), nameof(ImportChaptersText), nameof(ImportMplsText),
            nameof(UnavailableText), nameof(OutputNameLabel), nameof(StartTimeLabel), nameof(SegmentDurationLabel),
            nameof(EndTimeLabel), nameof(TimeFormatText), nameof(FirstFrameLabel), nameof(FrameCountLabel),
            nameof(LastFrameLabel), nameof(FrameFormatText), nameof(AddEpisodeText), nameof(ApplyEditText),
            nameof(DeleteEpisodeText), nameof(MergeLeftText), nameof(MergeRightText), nameof(ResetEditText),
            nameof(SourceStatsText), nameof(FrameChangingFiltersWarning),
            nameof(ApplyText), nameof(CancelText)
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
