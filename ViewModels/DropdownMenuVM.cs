namespace OneColumnEncoder.ViewModels
{
    public class DropdownMenuVM : BaseVM
    {
        public ObservableCollection<DropdownItemM> Items { get; } = [];
        private DropdownItemM? _selectedItem;
        public DropdownItemM? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!SetProperty(ref _selectedItem, value)) return;
                SelectionChangedCommand?.Execute(value);
            }
        }
        public ICommand? SelectionChangedCommand { get; set; }
    }
}