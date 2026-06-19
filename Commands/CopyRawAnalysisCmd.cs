using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class CopyRawAnalysisCmd(
        VideoAnalysisM analysis,
        ModalNavS modalNavS,
        Func<bool>? isQueueRoute = null) : BaseCmd
    {
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Func<bool>? _isQueueRoute = isQueueRoute;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(GetRawJson());

        public override void Execute(object? parameter)
        {
            if (!CanExecute(null)) return;

            try
            {
                Clipboard.SetText(GetRawJson());
                new OpenSuccModalCmd(
                    _modalNavS,
                    UILangProviderM.SrcAnalysisWindowTitle,
                    UILangProviderM.Current["SrcAnalysis.Copied"]).Execute(null);
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProviderM.SrcAnalysisWindowTitle,
                    ex.Message).Execute(null);
            }
        }

        private string GetRawJson() =>
            _isQueueRoute?.Invoke() == true && !string.IsNullOrWhiteSpace(_analysis.QueueRawJson)
                ? _analysis.QueueRawJson
                : _analysis.RawJson;
    }
}
