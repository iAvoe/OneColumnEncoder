namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Shows a confirmation modal reporting a completed queue source analysis, with context-menu
/// actions to open or copy the produced queue and excluded-sources JSON paths.
/// </summary>
public class OpenQueueAnalysisCompletedModalCmd(
    ModalNavS modalNavS,
    string message,
    string queueJsonPath,
    string? excludedJsonPath) : OpenCloseBase(modalNavS)
{
    private readonly string _message = message;
    private readonly string _queueJsonPath = queueJsonPath;
    private readonly string? _excludedJsonPath = excludedJsonPath;

    /// <summary>
    /// Shows the success confirmation dialog with JSON open/copy actions.
    /// </summary>
    public override void Execute(object? parameter)
    {
        ConfirmationModal window = new();
        CloseModalCmd closeCmd = new(window.Close);
        ConfirmationVM vm = ConfirmationVM.CreateSuccess(
            UILangProvider.SrcAnalysisWindowTitle,
            _message,
            closeCmd,
            closeCmd);

        AddJsonActions(vm, _queueJsonPath, "SourceQueue.OpenQueueJson", "SourceQueue.CopyQueueJsonPath");
        if (!string.IsNullOrWhiteSpace(_excludedJsonPath))
            AddJsonActions(vm, _excludedJsonPath, "SourceQueue.OpenExcludedJson", "SourceQueue.CopyExcludedJsonPath");

        ShowModal(window, vm, showDialog: true);
    }

    /// <summary>
    /// Adds context-menu items to open the JSON file and to copy its path.
    /// </summary>
    private static void AddJsonActions(ConfirmationVM vm, string jsonPath, string openTextKey, string copyTextKey)
    {
        vm.ContextMenuItems.Add(new(
            UILangProvider.Current[openTextKey],
            new ActionCmd(_ => OpenJsonPath(jsonPath))));
        vm.ContextMenuItems.Add(new(
            UILangProvider.Current[copyTextKey],
            new ActionCmd(_ => Clipboard.SetText(jsonPath))));
    }

    /// <summary>
    /// Opens the JSON file in its default application.
    /// </summary>
    private static void OpenJsonPath(string jsonPath) =>
        Process.Start(new ProcessStartInfo(jsonPath) { UseShellExecute = true });
}
