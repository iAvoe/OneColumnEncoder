using System.IO;

namespace OneColumnEncoder.ViewModels;

public sealed class QueueEditorVM : BaseVM
{
    private readonly Action _closeAction;
    private readonly Action<string[]> _applyEditedPaths;

    public QueueEditorVM(Action closeAction, IEnumerable<string> filePaths, Action<string[]> applyEditedPaths)
    {
        _closeAction = closeAction;
        _applyEditedPaths = applyEditedPaths;

        RemoveItemCommand = new ActionCmd(item => RemoveItem(item as QueueEditorItemVM));
        MoveItemUpCommand = new ActionCmd(item => MoveItem(item as QueueEditorItemVM, -1));
        MoveItemDownCommand = new ActionCmd(item => MoveItem(item as QueueEditorItemVM, 1));

        foreach (string filePath in filePaths)
            Items.Add(new QueueEditorItemVM(filePath, RemoveItemCommand, MoveItemUpCommand, MoveItemDownCommand));

        FinishButtons = ButtonGroupVM.CreateTwoButton(
            ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"],
            ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"],
            new ActionCmd(_ => _closeAction()),
            new ActionCmd(_ => Confirm()));

        RefreshItemStates();
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    public ObservableCollection<QueueEditorItemVM> Items { get; } = [];
    public static string WindowTitle => QueueSidebarLangProvider.Current.QueueEditorTitleText;
    public ActionCmd RemoveItemCommand { get; }
    public ActionCmd MoveItemUpCommand { get; }
    public ActionCmd MoveItemDownCommand { get; }
    public ButtonGroupVM FinishButtons { get; }

    private void RemoveItem(QueueEditorItemVM? item)
    {
        if (item == null || !Items.Remove(item)) return;
        RefreshItemStates();
    }

    private void MoveItem(QueueEditorItemVM? item, int offset)
    {
        if (item == null) return;

        int oldIndex = Items.IndexOf(item);
        int newIndex = oldIndex + offset;
        if (oldIndex < 0 || newIndex < 0 || newIndex >= Items.Count) return;

        Items.Move(oldIndex, newIndex);
        RefreshItemStates();
    }

    private void RefreshItemStates()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].CanMoveUp = i > 0;
            Items[i].CanMoveDown = i < Items.Count - 1;
        }

        if (FinishButtons != null)
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
        foreach (QueueEditorItemVM item in Items)
            item.RefreshLanguage();

        FinishButtons.B2_1Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Cancel"];
        FinishButtons.B2_2Text = ConfirmDialogLangProvider.Current["ConfirmDialog.Confirm"];
    }

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class QueueEditorItemVM(string filePath, ICommand removeCommand, ICommand moveUpCommand, ICommand moveDownCommand) : BaseVM
{
    private bool _canMoveUp;
    private bool _canMoveDown;

    public string FilePath { get; } = filePath;
    public string Name => Path.GetFileName(FilePath);
    public string P1Text => FilePath;
    public string DisplayR1Text => QueueSidebarLangProvider.Current.QueueItemRemoveText;
    public string R2Text => QueueSidebarLangProvider.Current.QueueItemMoveUpText;
    public string R3Text => QueueSidebarLangProvider.Current.QueueItemMoveDownText;
    public ICommand R1Command { get; } = removeCommand;
    public ICommand R2Command { get; } = moveUpCommand;
    public ICommand R3Command { get; } = moveDownCommand;
    public static bool R1IsEnabled => true;
    public bool R2IsEnabled => _canMoveUp;
    public bool R3IsEnabled => _canMoveDown;
    public static bool IsSelected => false;
    public static bool IsCancel => false;
    public static bool IsRecentlyMoved => false;

    public bool CanMoveUp
    {
        set
        {
            if (_canMoveUp == value) return;
            _canMoveUp = value;
            OnPropertyChanged(nameof(R2IsEnabled));
        }
    }

    public bool CanMoveDown
    {
        set
        {
            if (_canMoveDown == value) return;
            _canMoveDown = value;
            OnPropertyChanged(nameof(R3IsEnabled));
        }
    }

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(DisplayR1Text));
        OnPropertyChanged(nameof(R2Text));
        OnPropertyChanged(nameof(R3Text));
    }
}
