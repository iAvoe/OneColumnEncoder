namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the accepted-source editor after queue filtering. Confirm applies the
/// remaining sources in their displayed order; Cancel leaves the filtered queue unchanged.
/// </summary>
public sealed class OpenQueueEditorCmd(ModalNavS modalNavS, Action<string[]> applyEditedPaths) : OpenCloseBase(modalNavS)
{
    private readonly Action<string[]> _applyEditedPaths = applyEditedPaths;

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<QueueEditorModal>())
            return;

        if (parameter is not string[] filePaths || filePaths.Length == 0)
            return;

        QueueEditorModal window = new();
        QueueEditorVM vm = new(window.Close, filePaths, _applyEditedPaths);
        ShowModal(window, vm, showDialog: true);
    }
}
