namespace OneColumnEncoder.Commands.OpenClose;

public class OpenUsagesCmd(ModalNavS modelNavS, AppConfM appConfM) : OpenCloseBase(modelNavS)
{
    private readonly AppConfM _appConfM = appConfM;
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<AppUsageModal>())
            return;

        var window = new AppUsageModal();
        var vm = new AppUsageVM(_appConfM, window.Close);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
