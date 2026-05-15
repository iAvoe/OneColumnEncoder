using OneColumnEncoder.CommonMethods;
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
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist2 { get; } = [];
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
            FillCollection(SourceChecklist1, ChecklistProviderS.GetSourceChecklist1());
            FillCollection(SourceChecklist2, ChecklistProviderS.GetSourceChecklist2());
            FillCollection(EncodeChecklist1, ChecklistProviderS.GetEncodeChecklist1());
            FillCollection(EncodeChecklist2, ChecklistProviderS.GetEncodeChecklist2());

        }

        private static void FillCollection(ObservableCollection<ChecklistEntryVM> collection, List<ChecklistItemDefinitionM> definitions)
        {
            collection.Clear();
            foreach (var def in definitions)
            {
                collection.Add(new ChecklistEntryVM
                {
                    Text = def.Text,
                    Status = def.InitialStatus
                });
            }
        }
    }
}