using OneColumnEncoder.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;

namespace OneColumnEncoder.ViewModels
{
    public class ContainerCardVM : BaseVM
    {
        private readonly string _name = string.Empty;
        public string Name { get => _name; }

        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public ObservableCollection<ChecklistEntryVM> EncodeChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> EncodeChecklist2 { get; } = [];
        public DropdownMenuVM ImportDropdown { get; } = new();
        public ICommand ImportCommand { get; }

        public ContainerCardVM()
        {
            ImportCommand = new ImportToolCmd(ImportDropdown, ToolsChecklist);
            ImportDropdown.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ImportDropdown.SelectedItem))
                    (ImportCommand as BaseCmd)?.OnCanExecuteChanged();
            };

            FillCollection(ToolsChecklist, ChecklistProviderS.GetToolsChecklist());
            FillCollection(EncodeChecklist1, ChecklistProviderS.GetEncodeChecklist1());
            FillCollection(EncodeChecklist2, ChecklistProviderS.GetEncodeChecklist2());
        }
    }
}