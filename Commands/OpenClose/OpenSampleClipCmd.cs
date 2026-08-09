namespace OneColumnEncoder.Commands.OpenClose;

public class OpenSampleClipCmd(ModalNavS modalNavS, AppConfM appConfM, Func<EncodingPipelineRequest?> buildRequest, VideoAnalysisM srcVideoAnalysis, Func<bool> isMultiSourceRouteActive) : OpenCloseBase(modalNavS)
{
    private readonly AppConfM _appConfM = appConfM;
    private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
    private readonly VideoAnalysisM _srcVideoAnalysis = srcVideoAnalysis;
    private readonly Func<bool> _isMultiSourceRouteActive = isMultiSourceRouteActive;

    public override void Execute(object? parameter)
    {
        if (_isMultiSourceRouteActive())
        {
            new OpenWarnModalCmd(
                ModalNavS,
                UICaptionProvider.SourceInspect.WarnTitle,
                UICaptionProvider.Hints.QueueRouteSampleClipDisabled).Execute(null);
            return;
        }

        if (TryActivateExistingWindow<SampleClipModal>())
            return;

        SampleClipModal window = new();
        SampleClipVM vm = new(ModalNavS, window.Close, _appConfM, _buildRequest, _srcVideoAnalysis);
        ShowModal(window, vm, closeOpenStack: true);
    }
}
