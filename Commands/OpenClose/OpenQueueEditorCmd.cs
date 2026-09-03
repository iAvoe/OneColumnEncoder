namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the shared source queue editor. Confirm applies the remaining sources in
/// their displayed order; Cancel leaves the caller's current queue unchanged.
/// </summary>
public sealed class OpenQueueEditorCmd(
    ModalNavS modalNavS,
    Action<string[]> applyEditedPaths,
    int minimumItemCount = 0) : OpenCloseBase(modalNavS)
{
    private readonly Action<string[]> _applyEditedPaths = applyEditedPaths;
    private readonly int _minimumItemCount = minimumItemCount;

    public static string[] EditFilePaths(
        ModalNavS modalNavS,
        IEnumerable<string> filePaths,
        int minimumItemCount = 0)
    {
        string[] editedFilePaths = [.. filePaths];
        new OpenQueueEditorCmd(modalNavS, paths => editedFilePaths = paths, minimumItemCount)
            .Execute(editedFilePaths);
        return editedFilePaths;
    }

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<QueueEditorModal>())
            return;

        if (parameter is not string[] filePaths || filePaths.Length == 0)
            return;

        QueueEditorModal window = new();
        QueueEditorVM vm = new(window.Close, filePaths, _applyEditedPaths, _minimumItemCount);
        ShowModal(window, vm, showDialog: true);
    }
}
