using OneColumnEncoder.Commands;
using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class ToolsImportCardVM : BaseVM
    {
        private const int UpstreamChecklistIndex = 0;
        private const int EncoderChecklistIndex = 1;
        private const int AnalyticsChecklistIndex = 2;

        public string ImportButtonText =>
            UILangProviderM.Current["ImportButton"];

        private string _name = string.Empty;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public DropdownMenuVM ImportDropdown { get; } = new();
        public ICommand ImportCommand { get; }
        public event Func<string, string, string?, Task>? ToolImported;

        private readonly PropertyChangedEventHandler _onDropdownPropertyChanged;

        public ToolsImportCardVM(ModalNavS modalNavS)
        {
            ImportCommand = new ImportToolCmd(ImportDropdown,
                                               ToolsChecklist,
                                               modalNavS,
                                               async (toolName, filePath, version) => {
                                                   if (ToolImported != null)
                                                       await ToolImported(toolName, filePath, version);
                                               });
            _onDropdownPropertyChanged = (s, e) =>
            {
                if (e.PropertyName == nameof(ImportDropdown.SelectedItem))
                    (ImportCommand as BaseCmd)?.OnCanExecuteChanged();
            };
            ImportDropdown.PropertyChanged += _onDropdownPropertyChanged;

            FillCollection(ToolsChecklist, ChecklistProviderM.GetToolsChecklist());
        }

        public void RefreshLanguage()
        {
            OnPropertyChanged(nameof(ImportButtonText));
            RefreshChecklistText();
            RefreshImportDropdownItems();
        }

        public void RefreshToolsChecklist(bool hasUpstreamTool, bool hasEncoderTool, bool hasFfprobe)
        {
            UpdateChecklistStatus(UpstreamChecklistIndex, hasUpstreamTool);
            UpdateChecklistStatus(EncoderChecklistIndex, hasEncoderTool);
            UpdateChecklistStatus(AnalyticsChecklistIndex, hasFfprobe);
        }

        private void UpdateChecklistStatus(int index, bool isReady)
        {
            if (index < 0 || index >= ToolsChecklist.Count) return;
            ToolsChecklist[index].Status = isReady ? StatusType.Success : StatusType.Error;
        }

        private void RefreshChecklistText()
        {
            List<ChecklistItemDefinitionM> definitions = ChecklistProviderM.GetToolsChecklist();
            for (int i = 0; i < definitions.Count && i < ToolsChecklist.Count; i++)
            {
                ToolsChecklist[i].Text = definitions[i].Text;
            }
        }

        private void RefreshImportDropdownItems()
        {
            string? selectedTitle = ImportDropdown.SelectedItem?.Title;
            bool selectedPlaceholder = ImportDropdown.SelectedItem?.IsPlaceholder == true;

            ImportDropdown.Items.Clear();
            foreach (DropdownItemM item in ToolCatalogProviderM.GetImportDropdownItems())
            {
                ImportDropdown.Items.Add(item);
            }

            ImportDropdown.SelectedItem =
                ImportDropdown.Items.FirstOrDefault(i =>
                    (selectedPlaceholder && i.IsPlaceholder) ||
                    (!selectedPlaceholder && i.Title == selectedTitle)) ??
                ImportDropdown.Items.FirstOrDefault(i => i.IsPlaceholder) ??
                ImportDropdown.Items.FirstOrDefault();
        }

        public override void Dispose()
        {
            ImportDropdown.PropertyChanged -= _onDropdownPropertyChanged;
            base.Dispose();
        }
    }
}
