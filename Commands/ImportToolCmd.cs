using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static OneColumnEncoder.Models.ConfirmationProviderM;

namespace OneColumnEncoder.Commands
{
    public class ImportToolCmd(DropdownMenuVM dropdownVM, ObservableCollection<ChecklistEntryVM> knownTools, Action<string, string>? onSuccess = null) : AsyncBaseCmd
    {
        private readonly DropdownMenuVM _dropdownVm = dropdownVM;
        private readonly ObservableCollection<ChecklistEntryVM> _knownTools = knownTools;
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

            // This line is for tools that are executable, but in suspecious sizes, currently its here for debugging
            if (!DoubleCheckSuspiciousImport(selectedTool)) return;

            string? filePath = await ImportToolAsync(selectedTool);
            if (string.IsNullOrEmpty(filePath)) return;

            _onSuccess?.Invoke(selectedTool, filePath);
            foreach (var l in _knownTools)
            {
                if (l.Text.Contains(selectedTool, StringComparison.OrdinalIgnoreCase))
                {
                    l.Status = StatusType.Success;
                }
            }

            OnCanExecuteChanged();
        }

        private static bool DoubleCheckSuspiciousImport(string toolName)
        {
            var window = new ConfirmationModal();
            window.DataContext = ConfirmationModalVM.CreateWarning(
                title: ConfirmSuspiciousImport.GetTitle(toolName),
                p1Text: ConfirmSuspiciousImport.GetMessage(toolName),
                cancelCmd: new ActionCmd(_ => { window.DialogResult = false; window.Close(); }),
                confirmCmd: new ActionCmd(_ => { window.DialogResult = true; window.Close(); }));
            window.Owner = Application.Current.MainWindow;
            return window.ShowDialog() == true;
        }

        // There is no awaitable file dialog in WPF, so this warning can be ignored for now
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