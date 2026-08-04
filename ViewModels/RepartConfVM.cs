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

public sealed class RepartConfVM : BaseVM
{
    private readonly ModalNavS _modalNavS;
    private readonly Action _closeAction;
    private readonly Func<string> _getFfprobePath;
    private readonly Action<RepartPlanM> _applyPlan;
    private RepartPlanM? _analysis;
    private RepartOutputItemVM? _selectedOutput;
    private List<RepartOutputItemVM> _selectedOutputs = [];
    private string _outputNameText = string.Empty;
    private string _startTimeText = "00:00:00.000";
    private string _endTimeText = "00:00:00.000";
    private string _firstFrameText = "0";
    private string _lastFrameText = "0";
    private string _statusText = string.Empty;
    private bool _isBusy;
    private bool _syncingRange;
    private bool _lastRangeInputWasTime;
    private CancellationTokenSource? _analysisCancellation;

    public RepartConfVM(
        ModalNavS modalNavS,
        Action closeAction,
        Func<string> getFfprobePath,
        Action<RepartPlanM> applyPlan)
    {
        _modalNavS = modalNavS;
        _closeAction = closeAction;
        _getFfprobePath = getFfprobePath;
        _applyPlan = applyPlan;

        ImportFolderCommand = new ActionCmd(async _ => await ImportFolderAsync());
        AppendFilesCommand = new ActionCmd(async _ => await AppendFilesAsync());
        RemoveSourceCommand = new ActionCmd(async item => await RemoveSourceAsync(item as RepartSourceItemVM));
        MoveSourceUpCommand = new ActionCmd(async item => await MoveSourceAsync(item as RepartSourceItemVM, -1));
        MoveSourceDownCommand = new ActionCmd(async item => await MoveSourceAsync(item as RepartSourceItemVM, 1));
        AddEpisodeCommand = new ActionCmd(_ => AddEpisode());
        ApplyEditCommand = new ActionCmd(_ => ApplyEdit());
        DeleteEpisodeCommand = new ActionCmd(_ => DeleteSelectedOutputs());
        MergeEpisodesCommand = new ActionCmd(_ => MergeSelectedOutputs());
        SelectTimelineSliceCommand = new ActionCmd(SelectTimelineSlice);
        ApplyCommand = new ActionCmd(_ => ApplyAndClose());
        CancelCommand = new CloseModalCmd(closeAction);
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
    public string EndTimeLabel => RepartLangProvider.Current["EndTime"];
    public string FirstFrameLabel => RepartLangProvider.Current["FirstFrame"];
    public string LastFrameLabel => RepartLangProvider.Current["LastFrame"];
    public string AddEpisodeText => RepartLangProvider.Current["AddEpisode"];
    public string ApplyEditText => RepartLangProvider.Current["ApplyEdit"];
    public string DeleteEpisodeText => RepartLangProvider.Current["DeleteEpisode"];
    public string MergeEpisodesText => RepartLangProvider.Current["MergeEpisodes"];
    public string FrameChangingFiltersWarning => RepartLangProvider.Current["FrameChangingFiltersWarning"];
    public string ApplyText => RepartLangProvider.Current["Apply"];
    public string CancelText => RepartLangProvider.Current["Cancel"];

    public ObservableCollection<RepartSourceItemVM> Sources { get; } = [];
    public ObservableCollection<RepartOutputItemVM> Outputs { get; } = [];
    public ObservableCollection<RepartTimelineSliceVM> TimelineSlices { get; } = [];
    public ICommand ImportFolderCommand { get; }
    public ICommand AppendFilesCommand { get; }
    public ICommand RemoveSourceCommand { get; }
    public ICommand MoveSourceUpCommand { get; }
    public ICommand MoveSourceDownCommand { get; }
    public ICommand AddEpisodeCommand { get; }
    public ICommand ApplyEditCommand { get; }
    public ICommand DeleteEpisodeCommand { get; }
    public ICommand MergeEpisodesCommand { get; }
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
            if (!CanEdit || _analysis == null) return false;
            return TryBuildDraft(out RepartOutputSegmentM? segment, excludeSelectedName: false, showErrors: false)
                && segment != null
                && !Outputs.Any(output => output.Model.Overlaps(segment));
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
            if (!SetProperty(ref _selectedOutput, value) || value == null) return;
            LoadDraft(value.Model);
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

    private async Task ImportFolderAsync()
    {
        OpenFolderDialog dialog = new() { Title = RepartLangProvider.Current["SelectFolder"], Multiselect = false };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;
        string[] folderPaths = SourceFilePicker.GetVideoFilesInFolder(dialog.FolderName);
        if (folderPaths.Length == 0)
        {
            ShowError(RepartLangProvider.Current.SourceRequired);
            return;
        }
        if (!ConfirmSourceMutation()) return;

        string[] paths;
        try
        {
            paths = await AnalyzeSrcVideoCmd.AnalyzeAndFilterQueueFilePathsForImportAsync(
                _getFfprobePath(),
                folderPaths,
                _modalNavS);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return;
        }

        await AnalyzeAndReplaceSourcesAsync(paths, Outputs.Select(output => output.Model).ToList());
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
                paths,
                ConfirmDiscardInterlacedSource,
                _analysisCancellation.Token);
            List<RepartOutputSegmentM> reconciledOutputs = ReconcileOutputs(_analysis, analyzed, outputs);
            _analysis = analyzed;
            LoadSources();
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
        if (_analysis == null) return;
        for (int i = 0; i < _analysis.Sources.Count; i++)
        {
            RepartSourceM source = _analysis.Sources[i];
            Sources.Add(new RepartSourceItemVM(
                source.FilePath,
                source.FirstFrame,
                source.LastFrame,
                RemoveSourceCommand,
                MoveSourceUpCommand,
                MoveSourceDownCommand)
            {
                R2IsEnabled = i > 0,
                R3IsEnabled = i < _analysis.Sources.Count - 1
            });
        }
    }

    private void AddEpisode()
    {
        if (!TryBuildDraft(out RepartOutputSegmentM? segment, excludeSelectedName: false, showErrors: true) || segment == null) return;
        if (Outputs.Any(output => output.Model.Overlaps(segment)))
        {
            ShowError(RepartLangProvider.Current["Overlap"]);
            return;
        }
        List<RepartOutputSegmentM> models = Outputs.Select(output => output.Model).Append(segment).OrderBy(output => output.FirstFrame).ToList();
        ReplaceOutputs(models);
        PrepareNextDraft();
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

    private void DeleteSelectedOutputs()
    {
        HashSet<Guid> selectedIds = _selectedOutputs.Select(output => output.Model.Id).ToHashSet();
        if (selectedIds.Count == 0 && SelectedOutput != null) selectedIds.Add(SelectedOutput.Model.Id);
        ReplaceOutputs(Outputs.Where(output => !selectedIds.Contains(output.Model.Id)).Select(output => output.Model));
        PrepareNextDraft();
    }

    private void MergeSelectedOutputs()
    {
        List<RepartOutputSegmentM> selected = _selectedOutputs.Select(output => output.Model).OrderBy(output => output.FirstFrame).ToList();
        if (selected.Count < 2)
        {
            ShowError(RepartLangProvider.Current["SelectMerge"]);
            return;
        }
        for (int i = 1; i < selected.Count; i++)
        {
            if (!selected[i - 1].IsAdjacentTo(selected[i]))
            {
                ShowError(RepartLangProvider.Current["AdjacentRequired"]);
                return;
            }
        }
        HashSet<Guid> selectedIds = selected.Select(output => output.Id).ToHashSet();
        RepartOutputSegmentM merged = new(
            selected[0].Id,
            selected[0].BaseName,
            selected[0].FirstFrame,
            selected[^1].LastFrame);
        ReplaceOutputs(Outputs.Where(output => !selectedIds.Contains(output.Model.Id)).Select(output => output.Model).Append(merged));
        SelectedOutput = Outputs.FirstOrDefault(output => output.Model.Id == merged.Id);
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

    private void ReplaceOutputs(IEnumerable<RepartOutputSegmentM> models)
    {
        Outputs.Clear();
        if (_analysis != null)
        {
            foreach (RepartOutputSegmentM model in models.OrderBy(model => model.FirstFrame))
                Outputs.Add(new RepartOutputItemVM(model, _analysis.FrameRateNumerator, _analysis.FrameRateDenominator));
        }
        _selectedOutputs = [];
        SelectedOutput = null;
        RefreshTimeline();
        OnPropertyChanged(nameof(CanApply));
        RefreshDraftAvailability();
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
        do { name = $"EP{index:00}"; index++; }
        while (Outputs.Any(output => output.Model.BaseName.Equals(name, StringComparison.OrdinalIgnoreCase)));
        return name;
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
        }
        catch { }
        finally { _syncingRange = false; }
        RefreshDraftAvailability();
    }

    private void SyncTimesFromFrames()
    {
        if (_analysis == null
            || !long.TryParse(FirstFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long first)
            || !long.TryParse(LastFrameText, NumberStyles.Integer, CultureInfo.InvariantCulture, out long last)) return;
        _syncingRange = true;
        SetTimeTexts(first, last);
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
        OnPropertyChanged(nameof(CanAddEpisode));
    }

    private static string FormatReadyStatus(int requestedSourceCount, int acceptedSourceCount) =>
        acceptedSourceCount < requestedSourceCount
            ? string.Format(RepartLangProvider.Current["ReadyWithExcluded"], requestedSourceCount - acceptedSourceCount)
            : RepartLangProvider.Current["Ready"];

    private bool ConfirmDiscardInterlacedSource(RepartInterlacedSourceInfo source)
    {
        OpenWarnModalCmd cmd = new(
            _modalNavS,
            WindowTitleText,
            string.Format(
                RepartLangProvider.Current["InterlacedSourcePrompt"],
                source.DisplayName,
                source.FieldOrder));
        cmd.Execute(null);
        return cmd.DialogResult == true;
    }

    private void RefreshDraftAvailability() => OnPropertyChanged(nameof(CanAddEpisode));

    private void ShowError(string message) =>
        new OpenErrModalCmd(_modalNavS, WindowTitleText, message).Execute(null);

    private void OnLanguageChanged()
    {
        foreach (string property in new[]
        {
            nameof(InputSourcesTitle), nameof(OutputEpisodesTitle), nameof(TimelineTitle),
            nameof(ImportFolderText), nameof(AppendFilesText), nameof(ImportChaptersText), nameof(ImportMplsText),
            nameof(UnavailableText), nameof(OutputNameLabel), nameof(StartTimeLabel), nameof(EndTimeLabel),
            nameof(FirstFrameLabel), nameof(LastFrameLabel), nameof(AddEpisodeText), nameof(ApplyEditText),
            nameof(DeleteEpisodeText), nameof(MergeEpisodesText), nameof(FrameChangingFiltersWarning),
            nameof(ApplyText), nameof(CancelText)
        }) OnPropertyChanged(property);
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
