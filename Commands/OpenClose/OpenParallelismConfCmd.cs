namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the parallelism configuration modal for a target tool item.
/// </summary>
public class OpenParallelismConfCmd(ModalNavS modalNavS, ToolItemCardVM targetItem) : OpenCloseBase(modalNavS)
{
    private readonly ToolItemCardVM _targetItem = targetItem;

    /// <summary>
    /// Brings an already-open window to the front; otherwise shows the parallelism config modal.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<ParallelismConfModal>())
            return;

        ParallelismConfModal window = new();
        ParallelismConfVM vm = new(window.Close, _targetItem);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
