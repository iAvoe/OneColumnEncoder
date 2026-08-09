namespace OneColumnEncoder.Commands.OpenClose;

/// <summary>
/// Opens the encoding monitor modal that tracks an encoding pipeline run.
/// </summary>
public class OpenEncodingMonitorCmd(
    ModalNavS modalNavS,
    AppConfM appConfM,
    EncodingPipelineRequest request,
    EncodingPipelineCommand command,
    bool isSample = false) : OpenCloseBase(modalNavS)
{
    private readonly AppConfM _appConfM = appConfM;
    private readonly EncodingPipelineRequest _request = request;
    private readonly EncodingPipelineCommand _command = command;
    private readonly bool _isSample = isSample;

    /// <summary>
    /// Brings an already-open window to the front; otherwise shows the encoding monitor modal.
    /// </summary>
    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<EncodingMonitorModal>())
            return;

        EncodingMonitorModal window = new();
        EncodingMonitorVM vm = new(ModalNavS, window.Close, _appConfM, _request, _command, _isSample);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
