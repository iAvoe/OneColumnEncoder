namespace OneColumnEncoder.Commands.OpenClose;

public class OpenEncoderConfCmd(ModalNavS modalNavS,
    ToolItemCardVM? compressionParamsItem = null,
    Func<string?>? getFfmpegPath = null,
    Func<string?>? getSourceVideoPath = null,
    Func<string?>? getSourceFfprobeJson = null) : OpenCloseBase(modalNavS)
{
    private readonly ToolItemCardVM? _compressionParamsItem = compressionParamsItem;
    private readonly Func<string?>? _getFfmpegPath = getFfmpegPath;
    private readonly Func<string?>? _getSourceVideoPath = getSourceVideoPath;
    private readonly Func<string?>? _getSourceFfprobeJson = getSourceFfprobeJson;

    public override void Execute(object? parameter)
    {
        if (TryActivateExistingWindow<EncoderConfModal>())
            return;

        EncoderConfModal window = new();
        EncoderConfVM vm = new(
            window.Close,
            _compressionParamsItem,
            ModalNavS,
            _getFfmpegPath?.Invoke(),
            _getSourceVideoPath?.Invoke(),
            _getSourceFfprobeJson?.Invoke());
        ShowModal(window, vm, closeOpenStack: true);
    }
}
