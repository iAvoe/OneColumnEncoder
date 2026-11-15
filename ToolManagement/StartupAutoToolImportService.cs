namespace OneColumnEncoder.ToolManagement;

/// <summary>
/// Handles the one-time startup workflow that scans for importable tools,
/// prompts the user, and persists any imported tool paths.
/// </summary>
public static class StartupAutoToolImportService
{
    /// <summary>
    /// Runs the startup auto-import pass when the user enabled it in app config.
    /// Discovery is offloaded to a background thread so startup UI stays responsive.
    /// </summary>
    /// <param name="appConf">Application configuration that stores the re-import flag.</param>
    /// <param name="tools">Persisted tool catalog state to reconcile against.</param>
    /// <param name="modalNavS">Shared modal navigator for confirmation dialogs.</param>
    /// <param name="onToolImported">Callback that persists a discovered tool into app data.</param>
    public static async Task TryAutoImportToolsOnStartupAsync(
        AppConfM appConf,
        AppDataM.Importables tools,
        ModalNavS modalNavS,
        Func<string, string, string?, Task> onToolImported)
    {
        ArgumentNullException.ThrowIfNull(appConf);
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(modalNavS);
        ArgumentNullException.ThrowIfNull(onToolImported);

        if (!appConf.Reimport) return;

        try
        {
            IReadOnlyList<AutoToolImport.Candidate> candidates = await Task.Run(
                () => AutoToolImport.FindImportableToolsAsync(tools));

            if (candidates.Count == 0)
            {
                new OpenInfoModalCmd(
                    modalNavS,
                    UILangProvider.Current["AutoImport.Title"],
                    UILangProvider.Current["AutoImport.NotFoundMessage"]).Execute(null);
                ShowAvx512Notice(modalNavS);
                return;
            }

            if (!ShowAutoImportConfirmation(candidates, modalNavS))
            {
                ShowAvx512Notice(modalNavS);
                return;
            }

            foreach (AutoToolImport.Candidate candidate in candidates)
                await onToolImported(candidate.ExeName, candidate.FilePath, candidate.Version);

            ShowAvx512Notice(modalNavS);
        }
        finally
        {
            appConf.Reimport = false;
            appConf.Save();
        }
    }

    /// <summary>
    /// Shows the end-of-pass AVX-512 capability notice. If x265 itself crashed during probing,
    /// a dedicated message replaces it: x265 is never auto-imported from a different build because
    /// the compiled instruction set of the binary cannot be determined from its file name.
    /// </summary>
    private static void ShowAvx512Notice(ModalNavS modalNavS)
    {
        string messageKey;
        if (AutoToolImport.X265RequiresManualImport)
            messageKey = "AutoImport.X265ManualImportRequired";
        else
            messageKey = AutoToolImport.SupportsAvx512
                ? "AutoImport.Avx512Supported"
                : "AutoImport.Avx512Unsupported";

        new OpenInfoModalCmd(
            modalNavS,
            UILangProvider.Current["AutoImport.Title"],
            UILangProvider.Current[messageKey]).Execute(null);
    }

    /// <summary>
    /// Prompts before importing any tools that were discovered on disk.
    /// </summary>
    private static bool ShowAutoImportConfirmation(
        IReadOnlyList<AutoToolImport.Candidate> candidates,
        ModalNavS modalNavS)
    {
        string itemText = string.Join(Environment.NewLine, candidates.Select(candidate => string.Format(
            UILangProvider.ToolImportStringFormat,
            candidate.ExeName,
            candidate.FilePath)))[..^1]; // [..^1] removes last line break
        string message = string.Format(UILangProvider.Current["AutoImport.FoundMessage"], itemText);

        ConfirmationModal window = new();
        ConfirmationVM vm = ConfirmationVM.CreateInfo(
            UILangProvider.Current["AutoImport.Title"],
            message,
            new ActionCmd(_ => { window.DialogResult = false; window.Close(); }),
            new ActionCmd(_ => { window.DialogResult = true; window.Close(); }));

        OpenCloseBase.ShowModal(modalNavS, window, vm, showDialog: true);
        return window.DialogResult == true;
    }
}
