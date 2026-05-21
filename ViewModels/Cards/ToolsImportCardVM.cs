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

        public string ImportButtonText => UILangProviderM.Current["ImportButton"];

        private string _name = string.Empty;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public DropdownMenuVM ImportDropdown { get; } = new();
        public ICommand ImportCommand { get; }
        public event Action<string, string>? ToolImported;

        private readonly PropertyChangedEventHandler _onDropdownPropertyChanged;

        public ToolsImportCardVM(ModalNavS modalNavS)
        {
            ImportCommand = new ImportToolCmd(ImportDropdown, ToolsChecklist, modalNavS, (toolName, filePath) => ToolImported?.Invoke(toolName, filePath));
            _onDropdownPropertyChanged = (s, e) =>
            {
                if (e.PropertyName == nameof(ImportDropdown.SelectedItem))
                    (ImportCommand as BaseCmd)?.OnCanExecuteChanged();
            };
            ImportDropdown.PropertyChanged += _onDropdownPropertyChanged;

            FillCollection(ToolsChecklist, ChecklistProviderM.GetToolsChecklist());
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

        public override void Dispose()
        {
            ImportDropdown.PropertyChanged -= _onDropdownPropertyChanged;
            base.Dispose();
        }
    }
}
