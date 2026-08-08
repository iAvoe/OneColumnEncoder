namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenSampleClipCmd(ModalNavS modalNavS, AppConfM appConfM, Func<EncodingPipelineRequest?> buildRequest, VideoAnalysisM srcVideoAnalysis, Func<bool> isMultiSourceRouteActive) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly AppConfM _appConfM = appConfM;
        private readonly Func<EncodingPipelineRequest?> _buildRequest = buildRequest;
        private readonly VideoAnalysisM _srcVideoAnalysis = srcVideoAnalysis;
        private readonly Func<bool> _isMultiSourceRouteActive = isMultiSourceRouteActive;

        public override void Execute(object? parameter)
        {
            if (_isMultiSourceRouteActive())
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    UICaptionProvider.SourceInspect.WarnTitle,
                    UICaptionProvider.Hints.QueueRouteSampleClipDisabled).Execute(null);
                return;
            }

            SampleClipModal? existingWindow = Application.Current.Windows
                .OfType<SampleClipModal>()
                .FirstOrDefault();

            if (existingWindow != null)
            {
                existingWindow.Activate();
                return;
            }

            if (_modalNavS.IsOpen)
                _modalNavS.Close();

            SampleClipModal window = new();
            SampleClipVM vm = new(_modalNavS, window.Close, _appConfM, _buildRequest, _srcVideoAnalysis);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.Show();
        }
    }
}
