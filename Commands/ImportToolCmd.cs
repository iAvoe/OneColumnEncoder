using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class ImportToolCmd(DropdownMenuVM dropdownVM, ObservableCollection<ChecklistEntryVM> checklist, Action<string, string>? onSuccess = null) : AsyncBaseCmd
    {
        private readonly DropdownMenuVM _dropdownVm = dropdownVM;
        private readonly ObservableCollection<ChecklistEntryVM> _checklist = checklist;
        private readonly Action<string, string>? _onSuccess = onSuccess;

        public override bool CanExecute(object? parameter)
        {
            return !IsExecuting &&
                _dropdownVm.SelectedItem != null &&
                !_dropdownVm.SelectedItem.IsSeparator &&
                !_dropdownVm.SelectedItem.Title.Equals("No Selection", StringComparison.OrdinalIgnoreCase);
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            string selectedTool = _dropdownVm.SelectedItem?.Title ?? "";
            if (string.IsNullOrEmpty(selectedTool)) return;

            string? filePath = await ImportToolAsync(selectedTool);

            if (!string.IsNullOrEmpty(filePath))
            {
                _onSuccess?.Invoke(selectedTool, filePath);

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

        private static async Task<string?> ImportToolAsync(string toolName)
        {
            // TODO: Implement version detection later
            // For now, just get the path via file dialog

            // Determine filter based on tool type
            string filter = "Executable files (*.exe)|*.exe";
            if (toolName.Equals("AviSynth.dll", StringComparison.OrdinalIgnoreCase))
            {
                filter = "DLL files (*.dll)|*.dll";
            }

            // Use WPF OpenFileDialog
            OpenFileDialog dialog = new()
            {
                Filter = filter,
                Title = $"Select {toolName}",
                CheckFileExists = true,
                CheckPathExists = true
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }

            return null;
        }
    }
}