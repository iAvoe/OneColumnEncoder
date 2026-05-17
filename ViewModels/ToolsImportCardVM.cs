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
    public class ToolsImportCardVM : BaseVM
    {
        private string _name = string.Empty;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public DropdownMenuVM ImportDropdown { get; } = new();
        public ICommand ImportCommand { get; }
        public event Action<string>? ToolImported;

        public ToolsImportCardVM()
        {
            ImportCommand = new ImportToolCmd(ImportDropdown, ToolsChecklist, toolName => ToolImported?.Invoke(toolName));
            ImportDropdown.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ImportDropdown.SelectedItem))
                    (ImportCommand as BaseCmd)?.OnCanExecuteChanged();
            };

            FillCollection(ToolsChecklist, ChecklistProviderS.GetToolsChecklist());
        }
    }
}