namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the L-SMASH index prefetch progress modal and starts indexing.
/// </summary>
public class OpenLsmashIndexProgressCmd(ModalNavS modalNavS, IReadOnlyList<string> videoPaths) : OpenCloseBase(modalNavS)
{
    private readonly IReadOnlyList<string> _videoPaths = videoPaths;

    public override void Execute(object? parameter)
    {
        if (_videoPaths.Count == 0) return;

        if (TryActivateExistingWindow<LsmashIndexProgressModal>())
            return;

        LsmashIndexProgressModal window = new();
        LsmashIndexProgressVM vm = new(ModalNavS, window.Close);
        AttachModal(window, vm);
        window.Show();

        // Start indexing - fire and forget since VM handles completion
        _ = vm.RunIndexingAsync(_videoPaths);
    }
}
