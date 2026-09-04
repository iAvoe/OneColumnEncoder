using System.IO;
using static OneColumnEncoder.Models.ConfirmationProviderM;

namespace OneColumnEncoder.Commands;

public class ImportToolCmd(DropdownMenuVM dropdownVM,
                            ObservableCollection<ChecklistEntryVM> knownTools,
                            ModalNavS modalNavS,
                            Func<string, string, string?, Task>? onSuccess = null,
                            Func<string, string?>? getBrowseInitialDirectory = null,
                            Action<bool>? setBusy = null) : AsyncBaseCmd
{
    private readonly DropdownMenuVM _dropdownVm = dropdownVM;
    private readonly ObservableCollection<ChecklistEntryVM> _knownTools = knownTools;
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly Func<string, string, string?, Task>? _onSuccess = onSuccess;
    private readonly Func<string, string?>? _getBrowseInitialDirectory = getBrowseInitialDirectory;
    private readonly Action<bool>? _setBusy = setBusy;

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

        _setBusy?.Invoke(true);
        try
        {
            string? filePath = SelectAndValidateToolPath(
                toolToImport, "Dialog.SelectTitle", _modalNavS, _getBrowseInitialDirectory?.Invoke(toolToImport));
            if (string.IsNullOrEmpty(filePath)) return;

            string? version;
            try
            {
                version = await ToolManagementProviderM.TryDetectAsync(toolToImport, filePath);
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
        finally
        {
            _setBusy?.Invoke(false);
        }
    }

    #region Validations
    internal void ShowVersionDetectTimeoutError(string toolName)
    {
        new OpenErrModalCmd(
            _modalNavS,
            UILangProvider.Current["Import.VersionDetectTimeoutTitle"],
            string.Format(UILangProvider.Current["Import.VersionDetectTimeoutMessage"], toolName)).Execute(null);
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
        Window? owner = OpenCloseBase.GetSafeOwnerWindow();
        if (owner != null)
            window.Owner = owner;
        modalNavS.CurrentModalVM = vm;
        bool result = window.ShowDialog() == true;
        modalNavS.Close();
        return result;
    }

    internal static string? SelectAndValidateToolPath(
        string toolName, string dialogTitleFormat, ModalNavS modalNavS, string? initialDirectory = null)
    {
        string filter = toolName.Equals("AviSynth.dll", StringComparison.OrdinalIgnoreCase)
            ? UILangProvider.Current["Dialog.Filter.Dll"]
            : UILangProvider.Current["Dialog.Filter.Exe"];

        OpenFileDialog dialog = new()
        {
            Filter = filter,
            Title = string.Format(UILangProvider.Current[dialogTitleFormat], toolName),
            CheckFileExists = true,
            CheckPathExists = true
        };

        string? detectedDir = ToolCatalogProviderM.TryFindToolDirectory(toolName);
        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            dialog.InitialDirectory = initialDirectory;
        else if (detectedDir != null)
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
