using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class ImportToolCmd(DropdownMenuVM dropdownVM, ObservableCollection<ChecklistEntryVM> checklist, Action<string>? onSuccess = null) : AsyncBaseCmd
    {
        private readonly DropdownMenuVM _dropdownVm = dropdownVM;
        private readonly ObservableCollection<ChecklistEntryVM> _checklist = checklist;
        private readonly Action<string>? _onSuccess = onSuccess;

        public override bool CanExecute(object? parameter)
        {
            return !IsExecuting &&
                _dropdownVm.SelectedItem != null &&
                !_dropdownVm.SelectedItem.IsSeparator;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            string selectedTool = _dropdownVm.SelectedItem?.Title ?? "";
            if (string.IsNullOrEmpty(selectedTool)) return;

            bool success = await ImportToolAsync(selectedTool);

            if (success)
            {
                _onSuccess?.Invoke(selectedTool);

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
        private static async Task<bool> ImportToolAsync(string toolName)
        {
            await Task.Delay(1000);
            return true;
        }
    }
}
