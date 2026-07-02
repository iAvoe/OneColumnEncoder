using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class ConcatSourceListVM : BaseVM
    {
        public ObservableCollection<ConcatSourceItemVM> Items { get; } = [];

        public ICommand? RemoveItemCommand { get; set; }
        public ICommand? MoveItemUpCommand { get; set; }
        public ICommand? MoveItemDownCommand { get; set; }

        public void LoadItems(string[] filePaths)
        {
            DisposeItems();
            Items.Clear();
            for (int i = 0; i < filePaths.Length; i++)
            {
                var item = new ConcatSourceItemVM(filePaths[i], i,
                    RemoveItemCommand, MoveItemUpCommand, MoveItemDownCommand);
                Items.Add(item);
            }
            RefreshMoveStates();
        }

        public void RemoveItem(ConcatSourceItemVM item)
        {
            int index = Items.IndexOf(item);
            if (index < 0) return;
            Items.RemoveAt(index);
            item.Dispose();
            RefreshMoveStates();
        }

        public bool MoveItemUp(ConcatSourceItemVM item)
        {
            int index = Items.IndexOf(item);
            if (index <= 0) return false;
            Items.Move(index, index - 1);
            item.FlashMovedHighlight();
            RefreshMoveStates();
            return true;
        }

        public bool MoveItemDown(ConcatSourceItemVM item)
        {
            int index = Items.IndexOf(item);
            if (index < 0 || index >= Items.Count - 1) return false;
            Items.Move(index, index + 1);
            item.FlashMovedHighlight();
            RefreshMoveStates();
            return true;
        }

        public string[] GetCurrentFilePaths() =>
            Items.Select(i => i.FilePath).ToArray();

        public void RefreshLanguage(string removeText, string moveUpText, string moveDownText)
        {
            foreach (var item in Items)
            {
                item.DisplayR1Text = removeText;
                item.R2Text = moveUpText;
                item.R3Text = moveDownText;
            }
        }

        private void RefreshMoveStates()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].CanMoveUp = i > 0;
                Items[i].CanMoveDown = i < Items.Count - 1;
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
