using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class ToolsImportCardVM : BaseVM
    {
        public const int UpstreamChecklistIdx = 0;
        public const int EncoderChecklistIdx = 1;
        public const int AnalyticsChecklistIdx = 2;
        public const int UpstreamPickedChecklistIdx = 3;
        public const int DownstreamPickedChecklistIdx = 4;
        public const int AnalysisPickedChecklistIdx = 5;
        public const int CompleteSourceAnalysisChecklistIdx = 6;

        private string _name = string.Empty;
        public string Name
        {
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
            ImportCommand =
                new ImportToolCmd(ImportDropdown,
                                  ToolsChecklist,
                                  modalNavS,
                                  async (toolName, filePath, version) =>
                                  {
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
            RefreshChecklistText();
            RefreshImportDropdownItems();
        }

        public void RefreshToolsChecklist(bool hasUpstreamTool, bool hasEncoderTool, bool hasFfprobe)
        {
            UpdateChecklistStatus(UpstreamChecklistIdx, hasUpstreamTool);
            UpdateChecklistStatus(EncoderChecklistIdx, hasEncoderTool);
            UpdateChecklistStatus(AnalyticsChecklistIdx, hasFfprobe);
            // Complete Source Analysis will be updated separately when AnalyzeSrcVideoCmd succeeds
        }

        public void SetToolPickedStatus(ToolZone zone, bool isPicked)
        {
            int index = zone switch
            {
                ToolZone.Upstream => UpstreamPickedChecklistIdx,
                ToolZone.Encoder => DownstreamPickedChecklistIdx,
                ToolZone.Analytics => AnalysisPickedChecklistIdx,
                _ => -1
            };
            if (index < 0 || index >= ToolsChecklist.Count) return;
            ToolsChecklist[index].Status = isPicked ? StatusType.Success : StatusType.Error;
        }

        public void SetCompleteSourceAnalysisStatus(bool isSuccess)
        {
            UpdateChecklistStatus(CompleteSourceAnalysisChecklistIdx, isSuccess);
        }

        public void ResetCompleteSourceAnalysisStatus()
        {
            if (CompleteSourceAnalysisChecklistIdx >= 0 && CompleteSourceAnalysisChecklistIdx < ToolsChecklist.Count)
                ToolsChecklist[CompleteSourceAnalysisChecklistIdx].Status = StatusType.Waiting;
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
                ToolsChecklist[i].Text = definitions[i].Text;
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
