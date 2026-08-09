namespace OneColumnEncoder.Commands.OpenClose;

public class OpenFilenameScribeCmd(
    ModalNavS modalNavS,
    ToolItemCardVM outputSettingItem) : OpenCloseBase(modalNavS)
{
    private readonly ToolItemCardVM _outputSettingItem = outputSettingItem;

    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<FilenameScribeModal>())
            return;

        FilenameScribeModal window = new();
        FilenameScribeVM vm = new(window.Close, _outputSettingItem);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
