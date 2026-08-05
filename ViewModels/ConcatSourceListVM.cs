using OneColumnEncoder.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class ConcatSourceListVM : BaseVM
    {
        private string[] _originalFilePaths = [];
        private bool _hasOriginalQueueChanges;

        public ObservableCollection<ConcatSourceItemVM> Items { get; } = [];

        public string OrderingTitle => UILangProvider.Current["SourceConcat.OrderingTitle"];
        public string RestoreOriginalQueueText => UILangProvider.Current["SourceConcat.RestoreOriginalQueue"];

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

        public void RemoveItem(ConcatSourceItemVM item)
        {
            int index = Items.IndexOf(item);
            if (index < 0) return;
            Items.RemoveAt(index);
            item.Dispose();
            RefreshItemStates();
            RefreshChangeState();
        }

        public bool MoveItemUp(ConcatSourceItemVM item)
        {
            int index = Items.IndexOf(item);
            if (index <= 0) return false;
            Items.Move(index, index - 1);
            item.FlashMovedHighlight();
            RefreshItemStates();
            RefreshChangeState();
            return true;
        }

        public bool MoveItemDown(ConcatSourceItemVM item)
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
            Items.Select(i => i.FilePath).ToArray();

        public void RefreshLanguage()
        {
            QueueSidebarLangProvider lang = QueueSidebarLangProvider.Current;
            OnPropertyChanged(nameof(OrderingTitle));
            OnPropertyChanged(nameof(RestoreOriginalQueueText));
            foreach (var item in Items)
            {
                item.DisplayR1Text = lang.QueueItemRemoveText;
                item.R2Text = lang.QueueItemMoveUpText;
                item.R3Text = lang.QueueItemMoveDownText;
            }
        }

        private void ReplaceItems(string[] filePaths)
        {
            DisposeItems();
            Items.Clear();
            for (int i = 0; i < filePaths.Length; i++)
            {
                var item = new ConcatSourceItemVM(filePaths[i], i,
                    RemoveItemCommand, MoveItemUpCommand, MoveItemDownCommand);
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
            bool canRemove = Items.Count > 2;
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].CanMoveUp = i > 0;
                Items[i].CanMoveDown = i < Items.Count - 1;
                Items[i].CanRemove = canRemove;
            }
        }

        private void DisposeItems()
        {
            foreach (ConcatSourceItemVM item in Items)
                item.Dispose();
        }

        public override void Dispose()
        {
            DisposeItems();
            base.Dispose();
        }
    }
}
