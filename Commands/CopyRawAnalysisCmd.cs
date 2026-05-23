using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class CopyRawAnalysisCmd(
        VideoAnalysisM analysis,
        ModalNavS modalNavS) : BaseCmd
    {
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_analysis.RawJson);

        public override void Execute(object? parameter)
        {
            if (!CanExecute(null)) return;

            try
            {
                Clipboard.SetText(_analysis.RawJson);
                new OpenInfoOrDbgModalCmd(
                    _modalNavS,
                    UILangProviderM.Current["SrcAnalysis.WindowTitle"],
                    UILangProviderM.Current["SrcAnalysis.Copied"]).Execute(null);
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProviderM.Current["SrcAnalysis.WindowTitle"],
                    ex.Message).Execute(null);
            }
        }
    }
}
