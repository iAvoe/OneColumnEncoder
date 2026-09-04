using System.IO;
using System.Runtime.InteropServices;

namespace OneColumnEncoder.ViewModels;

public sealed partial class QueueEditorVM : BaseVM
{
    private readonly Action _closeAction;
    private readonly Action<string[]> _applyEditedPaths;
    private readonly int _minimumItemCount;
    private readonly bool _disableSortButtons;

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

        RemoveItemCommand = new ActionCmd(item => RemoveItem(item as SrcQueueItemVM));
        MoveItemUpCommand = new ActionCmd(item => MoveItem(item as SrcQueueItemVM, -1));
        MoveItemDownCommand = new ActionCmd(item => MoveItem(item as SrcQueueItemVM, 1));
        SortButtons = ButtonGroupVM.CreateTwoButton(
            QueueEditorLangProvider.Current["QueueEditor.SortBySize"],
            QueueEditorLangProvider.Current["QueueEditor.SortByFilename"],
            new ActionCmd(_ => SortBySize()),
            new ActionCmd(_ => SortByFilename()));

        foreach (string filePath in filePaths)
            Items.Add(new SrcQueueItemVM(filePath, RemoveItemCommand, MoveItemUpCommand, MoveItemDownCommand));

        FinishButtons = ButtonGroupVM.CreateTwoButton(
            ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"],
            ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"],
            new ActionCmd(_ => _closeAction()),
            new ActionCmd(_ => Confirm()));

        RefreshItemStates();
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public ObservableCollection<SrcQueueItemVM> Items { get; } = [];
    public static string WindowTitle => QueueEditorLangProvider.Current["QueueEditor.Title"];
    public static string HintText => QueueEditorLangProvider.Current["Hint.DoubleClickSortReverse"];
    public ActionCmd RemoveItemCommand { get; }
    public ActionCmd MoveItemUpCommand { get; }
    public ActionCmd MoveItemDownCommand { get; }
    public ButtonGroupVM SortButtons { get; }
    public ButtonGroupVM FinishButtons { get; }

    private void RemoveItem(SrcQueueItemVM? item)
    {
        if (item == null || !Items.Remove(item)) return;
        item.Dispose();
        RefreshItemStates();
    }

    private void MoveItem(SrcQueueItemVM? item, int offset)
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

        bool sortAscending = !IsAscending(Items, item => item.SizeBytes, Comparer<long>.Default);
        ApplySortedOrder(sortAscending
            ? [.. Items.OrderBy(item => item.SizeBytes)]
            : [.. Items.OrderByDescending(item => item.SizeBytes)]);
    }

    private void SortByFilename()
    {
        if (Items.Count < 2) return;

        bool sortAscending = !IsAscending(Items, item => item.FilePath, NaturalFileNameComparer.Instance);
        ApplySortedOrder(sortAscending
            ? [.. Items.OrderBy(item => item.FilePath, NaturalFileNameComparer.Instance)]
            : [.. Items.OrderByDescending(item => item.FilePath, NaturalFileNameComparer.Instance)]);
    }

    private void ApplySortedOrder(IReadOnlyList<SrcQueueItemVM> orderedItems)
    {
        Dictionary<SrcQueueItemVM, int> originalIndices = Items
            .Select((item, index) => (item, index))
            .ToDictionary(x => x.item, x => x.index);

        for (int i = 0; i < orderedItems.Count; i++)
        {
            SrcQueueItemVM desiredItem = orderedItems[i];
            int currentIndex = Items.IndexOf(desiredItem);
            if (currentIndex == i) continue;

            Items.Move(currentIndex, i);
        }

        RefreshItemStates();

        for (int i = 0; i < Items.Count; i++)
        {
            SrcQueueItemVM item = Items[i];
            if (originalIndices[item] != i)
                item.FlashMovedHighlight();
        }
    }

    private static bool IsAscending<T>(ObservableCollection<SrcQueueItemVM> items, Func<SrcQueueItemVM, T> keySelector, IComparer<T> comparer)
    {
        for (int i = 1; i < items.Count; i++)
        {
            if (comparer.Compare(keySelector(items[i-1]), keySelector(items[i])) > 0)
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
            Items[i].CanRemove = Items.Count > _minimumItemCount;
        }

        bool sortButtonsEnabled = !_disableSortButtons && Items.Count > 1;
        SortButtons.B2_1IsEnabled = sortButtonsEnabled;
        SortButtons.B2_2IsEnabled = sortButtonsEnabled;
        FinishButtons.B2_2IsEnabled = Items.Count >= _minimumItemCount && Items.Count > 0;
    }

    private void Confirm()
    {
        if (Items.Count == 0) return;

        _applyEditedPaths([.. Items.Select(item => item.FilePath)]);
        _closeAction();
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(HintText));
        foreach (SrcQueueItemVM item in Items)
            item.RefreshLanguage();

        SortButtons.B2_1Text = QueueEditorLangProvider.Current["QueueEditor.SortBySize"];
        SortButtons.B2_2Text = QueueEditorLangProvider.Current["QueueEditor.SortByFilename"];
        FinishButtons.B2_1Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"];
        FinishButtons.B2_2Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"];
    }

    private sealed partial class NaturalFileNameComparer : IComparer<string>
    {
        public static NaturalFileNameComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            string xName = Path.GetFileName(x ?? string.Empty);
            string yName = Path.GetFileName(y ?? string.Empty);
            int result = StrCmpLogicalW(xName, yName);
            return result != 0
                ? result
                : StringComparer.OrdinalIgnoreCase.Compare(x, y);
        }

        [LibraryImport("shlwapi.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int StrCmpLogicalW(string x, string y);
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        foreach (SrcQueueItemVM item in Items) item.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
