using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                if (SetProperty(ref _selectedItem, value))
                {
                    SelectionChangedCommand?.Execute(value);
                }
            }
        }
        public ICommand? SelectionChangedCommand { get; set; }
    }
}