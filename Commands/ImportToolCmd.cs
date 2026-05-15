using OneColumnEncoder.CommonMethods;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class ImportToolCmd : AsyncBaseCmd
    {
        private readonly DropdownMenuVM _dropdownVm;
        private readonly ObservableCollection<ChecklistEntryVM> _checklist;

        // private readonly IToolImportService _importService; // Maybe create this

        public ImportToolCmd(DropdownMenuVM dropdownVM, ObservableCollection<ChecklistEntryVM> checklist)
        {
            _dropdownVm = dropdownVM;
            _checklist = checklist;
        }

        public override bool CanExecute(object? parameter)
        {
            return !IsExecuting &&
                _dropdownVm.SelectedItem != null &&
                !_dropdownVm.SelectedItem.IsSeparator;
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            string selectedTool = _dropdownVm.SelectedItem?.Title ?? "";
            if (string.IsNullOrEmpty(selectedTool)) return;

            bool success = await ImportToolAsync(selectedTool);

            if (success)
            {
                // Re-generate checklist
                foreach (var l in _checklist)
                {
                    if (l.Text.Contains(selectedTool, System.StringComparison.OrdinalIgnoreCase))
                    {
                        l.Status = StatusType.Success;
                    }
                }
            }
            else
            {
                foreach (var l in _checklist)
                {
                    if (l.Text.Contains(selectedTool))
                        l.Status = StatusType.Error;
                }
            }

            OnCanExecuteChanged();
        }

        // TODO
        private async Task<bool> ImportToolAsync(string toolName)
        {
            await Task.Delay(1000);
            return true;
        }
    }
}
