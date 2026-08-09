namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the filename scribe modal for configuring the output filename pattern.
/// </summary>
public class OpenFilenameScribeCmd(
    ModalNavS modalNavS,
    ToolItemCardVM outputSettingItem) : OpenCloseBase(modalNavS)
{
    private readonly ToolItemCardVM _outputSettingItem = outputSettingItem;

    /// <summary>
    /// Always allows execution.
    /// </summary>
    public override bool CanExecute(object? parameter) => true;

    /// <summary>
    /// Brings an already-open window to the front; otherwise shows the filename scribe modal.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<FilenameScribeModal>())
            return;

        FilenameScribeModal window = new();
        FilenameScribeVM vm = new(window.Close, _outputSettingItem);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
