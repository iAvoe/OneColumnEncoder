using Microsoft.Win32;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.ToolManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using static OneColumnEncoder.Models.ConfirmationProviderM;

namespace OneColumnEncoder.Commands
{
    public class ImportToolCmd(DropdownMenuVM dropdownVM,
                               ObservableCollection<ChecklistEntryVM> knownTools,
                               ModalNavS modalNavS,
                               Func<string, string, string?, Task>? onSuccess = null) : AsyncBaseCmd
    {
        private readonly DropdownMenuVM _dropdownVm = dropdownVM;
        private readonly ObservableCollection<ChecklistEntryVM> _knownTools = knownTools;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Func<string, string, string?, Task>? _onSuccess = onSuccess;

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

            string? filePath = SelectAndValidateToolPath(
                toolToImport, "Dialog.SelectTitle", _modalNavS);
            if (string.IsNullOrEmpty(filePath)) return;

            string? version;
            try
            {
                version = await ToolVersionDetect.TryDetectAsync(toolToImport, filePath);
            }
            catch (ToolVersionDetectTimeoutException)
            {
                ShowVersionDetectTimeoutError(toolToImport);
                return;
            }

            if (_onSuccess != null) await _onSuccess(toolToImport, filePath, version);
            foreach (ChecklistEntryVM t in _knownTools)
            {
                if (t.Text.Contains(toolToImport, StringComparison.OrdinalIgnoreCase))
                {
                    t.Status = StatusType.Success;
                }
            }
            OnCanExecuteChanged();
        }

        #region Validations
        internal void ShowVersionDetectTimeoutError(string toolName)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UILangProviderM.Current["Import.VersionDetectTimeoutTitle"],
                string.Format(UILangProviderM.Current["Import.VersionDetectTimeoutMessage"], toolName)).Execute(null);
        }

        internal static bool ShouldSkipFileNameValidation(string toolName) =>
            toolName.Equals("one_line_shot_args.exe", StringComparison.OrdinalIgnoreCase);

        internal static bool IsFileNameMatch(string toolName, string filePath)
        {
            try
            {
                if (ShouldSkipFileNameValidation(toolName)) return true;

                string? actualFileName = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(actualFileName)) return false;

                string expectedExtension = Path.GetExtension(toolName);
                string actualExtension = Path.GetExtension(actualFileName);
                if (!string.IsNullOrEmpty(expectedExtension) &&
                    !expectedExtension.Equals(actualExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                string expectedNameWithoutExt = Path.GetFileNameWithoutExtension(toolName);
                string actualNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(expectedNameWithoutExt) || string.IsNullOrEmpty(actualNameWithoutExt))
                    return false;

                return actualFileName.Equals(toolName, StringComparison.OrdinalIgnoreCase) ||
                       actualNameWithoutExt.Contains(expectedNameWithoutExt, StringComparison.OrdinalIgnoreCase) ||
                       expectedNameWithoutExt.Contains(actualNameWithoutExt, StringComparison.OrdinalIgnoreCase);

            }
            catch { return false; }
        }

        internal static bool ShowDoubleCheckConfirmation(ModalNavS modalNavS, string titleStr, string p1Str) //, string toolName, string supposedName
        {
            ConfirmationModal window = new();
            ConfirmationVM vm = ConfirmationVM.CreateWarning(
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

        internal static string? SelectAndValidateToolPath(
            string toolName, string dialogTitleFormat, ModalNavS modalNavS)
        {
            string filter = toolName.Equals("AviSynth.dll", StringComparison.OrdinalIgnoreCase)
                ? UILangProviderM.Current["Dialog.Filter.Dll"]
                : UILangProviderM.Current["Dialog.Filter.Exe"];

            OpenFileDialog dialog = new()
            {
                Filter = filter,
                Title = string.Format(UILangProviderM.Current[dialogTitleFormat], toolName),
                CheckFileExists = true,
                CheckPathExists = true
            };

            string? detectedDir = ToolCatalogProviderM.TryFindToolDirectory(toolName);
            if (detectedDir != null)
                dialog.InitialDirectory = detectedDir;

            if (dialog.ShowDialog() != true) return null;

            string filePath = dialog.FileName;
            if (IsFileNameMatch(toolName, filePath))
                return filePath;

            return ShowDoubleCheckConfirmation(
                modalNavS,
                ConfirmForceImport.GetSuspiciousImportTitle(toolName),
                ConfirmForceImport.GetWrongToolMessage(filePath, toolName)) ? filePath : null; //, toolName, filePath
        }
        #endregion
    }
}
