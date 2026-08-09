namespace OneColumnEncoder.Commands.OpenClose;

public class OpenQueueAnalysisCompletedModalCmd(
    ModalNavS modalNavS,
    string message,
    string queueJsonPath,
    string? excludedJsonPath) : OpenCloseBase(modalNavS)
{
    private readonly string _message = message;
    private readonly string _queueJsonPath = queueJsonPath;
    private readonly string? _excludedJsonPath = excludedJsonPath;

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

    private static void AddJsonActions(ConfirmationVM vm, string jsonPath, string openTextKey, string copyTextKey)
    {
        vm.ContextMenuItems.Add(new(
            UILangProvider.Current[openTextKey],
            new ActionCmd(_ => OpenJsonPath(jsonPath))));
        vm.ContextMenuItems.Add(new(
            UILangProvider.Current[copyTextKey],
            new ActionCmd(_ => Clipboard.SetText(jsonPath))));
    }

    private static void OpenJsonPath(string jsonPath) =>
        Process.Start(new ProcessStartInfo(jsonPath) { UseShellExecute = true });
}
