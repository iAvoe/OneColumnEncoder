namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the app configuration modal.
/// </summary>
public class OpenAppConfCmd(ModalNavS modalNavS, AppConfM appConfS) : OpenCloseBase(modalNavS)
{
    private readonly AppConfM _appConfS = appConfS;

    /// <summary>
    /// Invoked after the configuration modal is closed.
    /// </summary>
    public Action? OnAfterClose { get; set; }

    /// <summary>
    /// Brings an already-open window to the front; otherwise shows the app configuration modal.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<AppConfModal>())
            return;

        AppConfModal window = new();
        var vm = new AppConfVM(_appConfS, ModalNavS, window.Close);
        ShowModal(window, vm, closeOpenStack: true, onClosed: () => OnAfterClose?.Invoke());
    }
}
