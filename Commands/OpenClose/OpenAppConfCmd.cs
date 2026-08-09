namespace OneColumnEncoder.Commands.OpenClose;

public class OpenAppConfCmd(ModalNavS modalNavS, AppConfM appConfS) : OpenCloseBase(modalNavS)
{
    private readonly AppConfM _appConfS = appConfS;

    public Action? OnAfterClose { get; set; }

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<AppConfModal>())
            return;

        AppConfModal window = new();
        var vm = new AppConfVM(_appConfS, window.Close);
        ShowModal(window, vm, closeOpenStack: true, onClosed: () => OnAfterClose?.Invoke());
    }
}
