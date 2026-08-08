using System.Collections.ObjectModel;
using System.ComponentModel;

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
        public const int VideoSourcePickedChecklistIdx = 0;
        public const int ScriptSourcePickedChecklistIdx = 1;
        public const int CompleteSourceAnalysisChecklistIdx = 2;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private double _cardOpacity = 1.0;
        public double CardOpacity
        {
            get => _cardOpacity;
            set => SetProperty(ref _cardOpacity, value);
        }

        public ObservableCollection<ChecklistEntryVM> Checklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> Checklist2 { get; } = [];
        public DropdownMenuVM ImportDropdown { get; } = new();
        public ICommand ImportCommand { get; }
        public event Func<string, string, string?, Task>? ToolImported;

        private readonly PropertyChangedEventHandler _onDropdownPropertyChanged;

        public ToolsImportCardVM(ModalNavS modalNavS, Func<string, string?>? getBrowseInitialDirectory = null)
        {
            ImportCommand =
                new ImportToolCmd(ImportDropdown,
                                  Checklist1,
                                  modalNavS,
                                  async (toolName, filePath, version) =>
                                  {
                                      if (ToolImported != null)
                                          await ToolImported(toolName, filePath, version);
                                  },
                                  getBrowseInitialDirectory);
            _onDropdownPropertyChanged = (s, e) =>
            {
                if (e.PropertyName == nameof(ImportDropdown.SelectedItem))
                    (ImportCommand as BaseCmd)?.OnCanExecuteChanged();
            };
            ImportDropdown.PropertyChanged += _onDropdownPropertyChanged;

            FillCollection(Checklist1, ChecklistProviderM.GetToolsChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetToolsChecklist2());
            SetScriptSourcePickedStatus(isRequired: false, isPicked: false);
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
            if (index < 0 || index >= Checklist1.Count) return;
            Checklist1[index].Status = isPicked ? StatusType.Success : StatusType.Error;
        }

        public void SetVideoSourcePickedStatus(bool isPicked)
        {
            UpdateChecklistStatus(Checklist2, VideoSourcePickedChecklistIdx, isPicked);
        }

        public void SetScriptSourcePickedStatus(bool isRequired, bool isPicked)
        {
            if (ScriptSourcePickedChecklistIdx < 0 || ScriptSourcePickedChecklistIdx >= Checklist2.Count) return;

            ChecklistEntryVM entry = Checklist2[ScriptSourcePickedChecklistIdx];
            entry.IsEnabled = isRequired;
            entry.Status = !isRequired
                ? StatusType.Waiting
                : isPicked
                    ? StatusType.Success
                    : StatusType.Error;
        }

        public void SetCompleteSourceAnalysisStatus(bool isSuccess)
        {
            UpdateChecklistStatus(Checklist2, CompleteSourceAnalysisChecklistIdx, isSuccess);
        }

        public void ResetCompleteSourceAnalysisStatus()
        {
            if (CompleteSourceAnalysisChecklistIdx >= 0 && CompleteSourceAnalysisChecklistIdx < Checklist2.Count)
                Checklist2[CompleteSourceAnalysisChecklistIdx].Status = StatusType.Error;
        }

        private void UpdateChecklistStatus(int index, bool isReady)
        {
            UpdateChecklistStatus(Checklist1, index, isReady);
        }

        private static void UpdateChecklistStatus(ObservableCollection<ChecklistEntryVM> collection, int index, bool isReady)
        {
            if (index < 0 || index >= collection.Count) return;
            collection[index].Status = isReady ? StatusType.Success : StatusType.Error;
        }

        private void RefreshChecklistText()
        {
            RefreshChecklistText(Checklist1, ChecklistProviderM.GetToolsChecklist1());
            RefreshChecklistText(Checklist2, ChecklistProviderM.GetToolsChecklist2());
        }

        private static void RefreshChecklistText(
            ObservableCollection<ChecklistEntryVM> collection,
            List<ChecklistItemDefinitionM> definitions)
        {
            for (int i = 0; i < definitions.Count && i < collection.Count; i++)
                collection[i].Text = definitions[i].Text;
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
            GC.SuppressFinalize(this);
        }
    }
}
