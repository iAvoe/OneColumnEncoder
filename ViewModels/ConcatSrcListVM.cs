namespace OneColumnEncoder.ViewModels;

public class ConcatSrcListVM : BaseVM
{
    private string[] _originalFilePaths = [];
    private bool _hasOriginalQueueChanges;
    private bool _isRepartMode;

    public ObservableCollection<SourceQueueItemVM> Items { get; } = [];

    public string OrderingTitle => _isRepartMode
        ? RepartLangProvider.Current["SourceOrdering"]
        : UILangProvider.Current["SourceConcat.OrderingTitle"];
    public static string RestoreOriginalQueueText => UILangProvider.Current["SourceConcat.RestoreOriginalQueue"];

    public bool HasOriginalQueueChanges
    {
        get => _hasOriginalQueueChanges;
        private set => SetProperty(ref _hasOriginalQueueChanges, value);
    }

    public ICommand? RemoveItemCommand { get; set; }
    public ICommand? MoveItemUpCommand { get; set; }
    public ICommand? MoveItemDownCommand { get; set; }
    public ICommand? RestoreOriginalQueueCommand { get; set; }

    public void LoadItems(string[] filePaths)
    {
        _originalFilePaths = [.. filePaths];
        ReplaceItems(filePaths);
        RefreshChangeState();
    }

    public void RemoveItem(SourceQueueItemVM item)
    {
        int index = Items.IndexOf(item);
        if (index < 0) return;
        Items.RemoveAt(index);
        item.Dispose();
        RefreshItemStates();
        RefreshChangeState();
    }

    public bool MoveItemUp(SourceQueueItemVM item)
    {
        int index = Items.IndexOf(item);
        if (index <= 0) return false;
        Items.Move(index, index - 1);
        item.FlashMovedHighlight();
        RefreshItemStates();
        RefreshChangeState();
        return true;
    }

    public bool MoveItemDown(SourceQueueItemVM item)
    {
        int index = Items.IndexOf(item);
        if (index < 0 || index >= Items.Count - 1) return false;
        Items.Move(index, index + 1);
        item.FlashMovedHighlight();
        RefreshItemStates();
        RefreshChangeState();
        return true;
    }

    public bool RestoreOriginalQueue()
    {
        if (!HasOriginalQueueChanges) return false;

        ReplaceItems(_originalFilePaths);
        RefreshChangeState();
        return true;
    }

    public string[] GetCurrentFilePaths() =>
        [.. Items.Select(i => i.FilePath)];

    public void RefreshLanguage()
    {
        OnPropertyChanged(nameof(OrderingTitle));
        OnPropertyChanged(nameof(RestoreOriginalQueueText));
        foreach (SourceQueueItemVM item in Items)
            item.RefreshLanguage();
    }

    public bool IsRepartMode
    {
        get => _isRepartMode;
        set
        {
            if (SetProperty(ref _isRepartMode, value))
            {
                OnPropertyChanged(nameof(OrderingTitle));
                RefreshItemStates();
            }
        }
    }

    private void ReplaceItems(string[] filePaths)
    {
        DisposeItems();
        Items.Clear();
        for (int i = 0; i < filePaths.Length; i++)
        {
            SourceQueueItemVM item = new(filePaths[i], RemoveItemCommand, MoveItemUpCommand, MoveItemDownCommand);
            Items.Add(item);
        }
        RefreshItemStates();
    }

    private void RefreshChangeState()
    {
        HasOriginalQueueChanges = !GetCurrentFilePaths().SequenceEqual(_originalFilePaths);
    }

    private void RefreshItemStates()
    {
        bool canRemove = !_isRepartMode && Items.Count > 2;
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].CanMoveUp = i > 0;
            Items[i].CanMoveDown = i < Items.Count - 1;
            Items[i].CanRemove = canRemove;
        }
    }

    private void DisposeItems()
    {
        foreach (SourceQueueItemVM item in Items)
            item.Dispose();
    }

    public override void Dispose()
    {
        DisposeItems();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
