using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class CopyRawAnalysisCmd(
        Func<string> getFfprobePath,
        Func<string> getSourcePath,
        VideoAnalysisM analysis,
        ModalNavS modalNavS) : AsyncBaseCmd
    {
        private readonly Func<string> _getFfprobePath = getFfprobePath;
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_getFfprobePath()) &&
            !string.IsNullOrWhiteSpace(_getSourcePath());

        protected override async Task ExecuteAsync(object? parameter)
        {
            try
            {
                string ffprobePath = _getFfprobePath();
                string sourcePath = _getSourcePath();
                if (NeedsFreshAnalysis(ffprobePath, sourcePath))
                {
                    _analysis.RawJson = await FfprobeVideoAnalysisH.AnalyzeAsync(ffprobePath, sourcePath);
                    _analysis.FfprobePath = ffprobePath;
                    _analysis.SourcePath = sourcePath;
                }

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

        private bool NeedsFreshAnalysis(string ffprobePath, string sourcePath) =>
            string.IsNullOrWhiteSpace(_analysis.RawJson) ||
            !string.Equals(_analysis.FfprobePath, ffprobePath, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_analysis.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase);
    }
}
