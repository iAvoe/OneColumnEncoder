using System.IO;

namespace OneColumnEncoder.ViewModels;

public sealed class QueueEditorVM : BaseVM
{
    private readonly Action _closeAction;
    private readonly Action<string[]>? _applyEditedPaths;
    private readonly Action<Guid[]>? _applyEditedOutputIds;
    private readonly int _minimumItemCount;
    private readonly bool _disableSortButtons;
    private readonly bool _isOutputMode;

    public QueueEditorVM(
        Action closeAction,
        IEnumerable<string> filePaths,
        Action<string[]> applyEditedPaths,
        int minimumItemCount = 0,
        bool disableSortButtons = false)
    {
        _closeAction = closeAction;
        _applyEditedPaths = applyEditedPaths;
        _minimumItemCount = minimumItemCount;
        _disableSortButtons = disableSortButtons;
        _isOutputMode = false;

        Initialize(filePaths.Select(filePath =>
            (IQueueEditorItem)new SrcQueueItemVM(filePath, null, null, null)));
    }

    public QueueEditorVM(
        Action closeAction,
        IEnumerable<RepartOutputSegmentM> outputSegments,
        int frameRateNumerator,
        int frameRateDenominator,
        Action<Guid[]> applyEditedOutputIds)
    {
        RepartOutputSegmentM[] segments = [.. outputSegments];
        _closeAction = closeAction;
        _applyEditedOutputIds = applyEditedOutputIds;
        _minimumItemCount = segments.Length;
        _disableSortButtons = false;
        _isOutputMode = true;

        Initialize(segments.Select(segment =>
            (IQueueEditorItem)new RepartQueueItemVM(segment, frameRateNumerator, frameRateDenominator)));
    }

    public ObservableCollection<IQueueEditorItem> Items { get; } = [];
    public string WindowTitle => _isOutputMode
        ? RepartLangProvider.Current["FilterScribeOutputOrdering"]
        : QueueEditorLangProvider.Current["QueueEditor.Title"];
    public string HintText => QueueEditorLangProvider.Current["Hint.DoubleClickSortReverse"];
    public ActionCmd RemoveItemCommand { get; private set; } = null!;
    public ActionCmd MoveItemUpCommand { get; private set; } = null!;
    public ActionCmd MoveItemDownCommand { get; private set; } = null!;
    public ButtonGroupVM SortButtons { get; private set; } = null!;
    public ButtonGroupVM FinishButtons { get; private set; } = null!;

    private void Initialize(IEnumerable<IQueueEditorItem> queueItems)
    {
        RemoveItemCommand = new ActionCmd(item => RemoveItem(item as IQueueEditorItem));
        MoveItemUpCommand = new ActionCmd(item => MoveItem(item as IQueueEditorItem, -1));
        MoveItemDownCommand = new ActionCmd(item => MoveItem(item as IQueueEditorItem, 1));
        SortButtons = ButtonGroupVM.CreateTwoButton(
            GetSortByFirstButtonText(),
            QueueEditorLangProvider.Current["QueueEditor.SortByFilename"],
            new ActionCmd(_ => SortBySize()),
            new ActionCmd(_ => SortByFilename()));

        foreach (IQueueEditorItem item in queueItems)
        {
            item.R1Command = RemoveItemCommand;
            item.R2Command = MoveItemUpCommand;
            item.R3Command = MoveItemDownCommand;
            Items.Add(item);
        }

        FinishButtons = ButtonGroupVM.CreateTwoButton(
            ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"],
            ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"],
            new ActionCmd(_ => _closeAction()),
            new ActionCmd(_ => Confirm()));

        RefreshItemStates();
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private void RemoveItem(IQueueEditorItem? item)
    {
        if (_isOutputMode || item == null || !Items.Remove(item)) return;
        item.Dispose();
        RefreshItemStates();
    }

    private void MoveItem(IQueueEditorItem? item, int offset)
    {
        if (item == null) return;

        int oldIndex = Items.IndexOf(item);
        int newIndex = oldIndex + offset;
        if (oldIndex < 0 || newIndex < 0 || newIndex >= Items.Count) return;

        Items.Move(oldIndex, newIndex);
        item.FlashMovedHighlight();
        RefreshItemStates();
    }

    private void SortBySize()
    {
        if (Items.Count < 2) return;

        bool sortAscending = !IsAscending(Items, item => item.SortSize, Comparer<long>.Default);
        ApplySortedOrder(sortAscending
            ? [.. Items.OrderBy(item => item.SortSize)]
            : [.. Items.OrderByDescending(item => item.SortSize)]);
    }

    private void SortByFilename()
    {
        if (Items.Count < 2) return;

        bool sortAscending = !IsAscending(Items, item => item.SortName, NaturalFileNameComparer.Instance);
        ApplySortedOrder(sortAscending
            ? [.. Items.OrderBy(item => item.SortName, NaturalFileNameComparer.Instance)]
            : [.. Items.OrderByDescending(item => item.SortName, NaturalFileNameComparer.Instance)]);
    }

    private void ApplySortedOrder(IReadOnlyList<IQueueEditorItem> orderedItems)
    {
        Dictionary<IQueueEditorItem, int> originalIndices = Items
            .Select((item, index) => (item, index))
            .ToDictionary(x => x.item, x => x.index);

        for (int i = 0; i < orderedItems.Count; i++)
        {
            IQueueEditorItem desiredItem = orderedItems[i];
            int currentIndex = Items.IndexOf(desiredItem);
            if (currentIndex != i) Items.Move(currentIndex, i);
        }

        RefreshItemStates();
        for (int i = 0; i < Items.Count; i++)
        {
            IQueueEditorItem item = Items[i];
            if (originalIndices[item] != i) item.FlashMovedHighlight();
        }
    }

    private static bool IsAscending<T>(
        ObservableCollection<IQueueEditorItem> items,
        Func<IQueueEditorItem, T> keySelector,
        IComparer<T> comparer)
    {
        for (int i = 1; i < items.Count; i++)
        {
            if (comparer.Compare(keySelector(items[i - 1]), keySelector(items[i])) > 0)
                return false;
        }
        return true;
    }

    private void RefreshItemStates()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].CanMoveUp = i > 0;
            Items[i].CanMoveDown = i < Items.Count - 1;
            Items[i].CanRemove = !_isOutputMode && Items.Count > _minimumItemCount;
        }

        bool sortButtonsEnabled = !_disableSortButtons && Items.Count > 1;
        SortButtons.B2_1IsEnabled = sortButtonsEnabled;
        SortButtons.B2_2IsEnabled = sortButtonsEnabled;
        FinishButtons.B2_2IsEnabled = Items.Count >= _minimumItemCount && Items.Count > 0;
    }

    private void Confirm()
    {
        if (Items.Count == 0) return;

        if (_isOutputMode)
            _applyEditedOutputIds?.Invoke([.. Items.Select(item => item.OutputId!.Value)]);
        else
            _applyEditedPaths?.Invoke([.. Items.Cast<SrcQueueItemVM>().Select(item => item.FilePath)]);
        _closeAction();
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(HintText));
        foreach (IQueueEditorItem item in Items)
            item.RefreshLanguage();

        SortButtons.B2_1Text = GetSortByFirstButtonText();
        SortButtons.B2_2Text = QueueEditorLangProvider.Current["QueueEditor.SortByFilename"];
        FinishButtons.B2_1Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"];
        FinishButtons.B2_2Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"];
    }

    private string GetSortByFirstButtonText() => _isOutputMode
        ? QueueEditorLangProvider.Current["QueueEditor.SortByTotalFrames"]
        : QueueEditorLangProvider.Current["QueueEditor.SortBySize"];

    private sealed class NaturalFileNameComparer : IComparer<string>
    {
        public static NaturalFileNameComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            string xName = Path.GetFileName(x ?? string.Empty);
            string yName = Path.GetFileName(y ?? string.Empty);
            int result = LibImportProviderM.CompareLogical(xName, yName);
            return result != 0
                ? result
                : StringComparer.OrdinalIgnoreCase.Compare(x, y);
        }
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        foreach (IQueueEditorItem item in Items) item.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
