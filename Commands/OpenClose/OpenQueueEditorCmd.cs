namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the shared source queue editor. Confirm applies the remaining sources in
/// their displayed order; Cancel leaves the caller's current queue unchanged.
/// </summary>
public sealed class OpenQueueEditorCmd(
    ModalNavS modalNavS,
    Action<string[]> applyEditedPaths,
    int minimumItemCount = 0,
    bool disableSortButtons = false) : OpenCloseBase(modalNavS)
{
    private readonly Action<string[]> _applyEditedPaths = applyEditedPaths;
    private readonly int _minimumItemCount = minimumItemCount;
    private readonly bool _disableSortButtons = disableSortButtons;

    public static string[] EditFilePaths(
        ModalNavS modalNavS,
        IEnumerable<string> filePaths,
        int minimumItemCount = 0,
        bool disableSortButtons = false)
    {
        string[] editedFilePaths = [.. filePaths];
        new OpenQueueEditorCmd(modalNavS, paths => editedFilePaths = paths, minimumItemCount, disableSortButtons)
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
        QueueEditorVM vm = new(window.Close, filePaths, _applyEditedPaths, _minimumItemCount, _disableSortButtons);
        ShowModal(window, vm, showDialog: true);
    }
}
