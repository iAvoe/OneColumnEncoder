namespace OneColumnEncoder.ViewModels;

public sealed class QueueEditorVM : BaseVM
{
    private readonly Action _closeAction;
    private readonly Action<string[]> _applyEditedPaths;

    public QueueEditorVM(Action closeAction, IEnumerable<string> filePaths, Action<string[]> applyEditedPaths)
    {
        _closeAction = closeAction;
        _applyEditedPaths = applyEditedPaths;

        RemoveItemCommand = new ActionCmd(item => RemoveItem(item as SrcQueueItemVM));
        MoveItemUpCommand = new ActionCmd(item => MoveItem(item as SrcQueueItemVM, -1));
        MoveItemDownCommand = new ActionCmd(item => MoveItem(item as SrcQueueItemVM, 1));

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
    public ActionCmd RemoveItemCommand { get; }
    public ActionCmd MoveItemUpCommand { get; }
    public ActionCmd MoveItemDownCommand { get; }
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

    private void RefreshItemStates()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].CanMoveUp = i > 0;
            Items[i].CanMoveDown = i < Items.Count - 1;
        }

        FinishButtons.B2_2IsEnabled = Items.Count > 0;
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
        foreach (SrcQueueItemVM item in Items)
            item.RefreshLanguage();

        FinishButtons.B2_1Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"];
        FinishButtons.B2_2Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"];
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        foreach (SrcQueueItemVM item in Items)
            item.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
