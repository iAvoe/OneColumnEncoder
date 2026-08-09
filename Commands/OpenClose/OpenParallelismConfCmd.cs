namespace OneColumnEncoder.Commands.OpenClose;

public class OpenParallelismConfCmd(ModalNavS modalNavS, ToolItemCardVM targetItem) : OpenCloseBase(modalNavS)
{
    private readonly ToolItemCardVM _targetItem = targetItem;

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<ParallelismConfModal>())
            return;

        ParallelismConfModal window = new();
        ParallelismConfVM vm = new(window.Close, _targetItem);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
