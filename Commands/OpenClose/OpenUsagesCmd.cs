namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the app usages (help/reference) modal.
/// </summary>
public class OpenUsagesCmd(ModalNavS modelNavS, AppConfM appConfM) : OpenCloseBase(modelNavS)
{
    private readonly AppConfM _appConfM = appConfM;

    /// <summary>
    /// Brings an already-open window to the front; otherwise shows the usages modal.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<AppUsageModal>())
            return;

        var window = new AppUsageModal();
        var vm = new AppUsageVM(_appConfM);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
