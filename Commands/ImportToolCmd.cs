using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
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
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Commands
{
    public class ImportToolCmd(DropdownMenuVM dropdownVM,
                               ObservableCollection<ChecklistEntryVM> knownTools,
                               ModalNavS modalNavS,
                               Action<string, string, string?>? onSuccess = null) : AsyncBaseCmd
    {
        private readonly DropdownMenuVM _dropdownVm = dropdownVM;
        private readonly ObservableCollection<ChecklistEntryVM> _knownTools = knownTools;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Action<string, string, string?>? _onSuccess = onSuccess;

        public override bool CanExecute(object? parameter)
        {
            return !IsExecuting &&
                _dropdownVm.SelectedItem != null &&
                !_dropdownVm.SelectedItem.IsSeparator &&
                !_dropdownVm.SelectedItem.IsPlaceholder;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            string toolToImport = _dropdownVm.SelectedItem?.Title ?? "";
            if (string.IsNullOrEmpty(toolToImport)) return;

            string? filePath = await ImportToolAsync(toolToImport);
            if (string.IsNullOrEmpty(filePath)) return;

            if (!IsFileNameMatch(toolToImport, filePath) &&
                !DoubleCheckFilenameMismatchImport(toolToImport, filePath)) return;

            string? version = await ToolVersionDetector.TryDetectAsync(toolToImport, filePath);

            _onSuccess?.Invoke(toolToImport, filePath, version);
            foreach (ChecklistEntryVM l in _knownTools)
            {
                if (l.Text.Contains(toolToImport, StringComparison.OrdinalIgnoreCase))
                {
                    l.Status = StatusType.Success;
                }
            }
            OnCanExecuteChanged();
        }

        #region Validations
        internal static bool IsFileNameMatch(string toolName, string filePath)
        {
            try
            {
                string? actualFileName = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(actualFileName)) return false;
                if (toolName.Contains('.', StringComparison.Ordinal))
                {
                    return actualFileName.Contains(
                        toolName,
                        StringComparison.OrdinalIgnoreCase);
                }
                // Tool without extension (not really possible, just exe and dll)
                string actualNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                return actualNameWithoutExt.Equals(
                    toolName,
                    StringComparison.OrdinalIgnoreCase);

            }
            catch { return false; }
        }

        internal static bool ShowDoubleCheckConfirmation(ModalNavS modalNavS, string titleStr, string p1Str, string toolName, string supposedName)
        {
            ConfirmationModal window = new();
            ConfirmationModalVM vm = ConfirmationModalVM.CreateWarning(
                title: titleStr,
                p1Text: p1Str,
                cancelCmd: new ActionCmd(_ => { window.DialogResult = false; window.Close(); }),
                confirmCmd: new ActionCmd(_ => { window.DialogResult = true; window.Close(); }));
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            modalNavS.CurrentModalVM = vm;
            bool result = window.ShowDialog() == true;
            modalNavS.Close();
            return result;
        }

        private bool DoubleCheckFilenameMismatchImport(string toolName, string supposedName)
        {
            return ShowDoubleCheckConfirmation(
                _modalNavS,
                ConfirmForceImport.GetSuspiciousImportTitle(toolName),
                ConfirmForceImport.GetWrongToolMessage(supposedName, toolName),
                supposedName,
                toolName);
        }
        #endregion

        // There is no awaitable file dialog in WPF, so this warning can be ignored for now
        private static async Task<string?> ImportToolAsync(string toolName)
        {
            // TODO: Implement version detection later
            // For now, just get the path via file dialog

            // Determine filter based on tool type
            string filter = UILangProviderM.Current["Dialog.Filter.Exe"];
            if (toolName.Equals("AviSynth.dll", StringComparison.OrdinalIgnoreCase))
            {
                filter = UILangProviderM.Current["Dialog.Filter.Dll"];
            }

            // Use WPF OpenFileDialog
            OpenFileDialog dialog = new()
            {
                Filter = filter,
                Title = string.Format(UILangProviderM.Current["Dialog.SelectTitle"], toolName),
                CheckFileExists = true,
                CheckPathExists = true
            };

            string? detectedDir = ToolCatalogProviderM.TryFindToolDirectory(toolName);
            if (detectedDir != null)
            {
                dialog.InitialDirectory = detectedDir;
            }

            bool? result = dialog.ShowDialog();
            if (result == true) return dialog.FileName;
            return null;
        }

    }
}
