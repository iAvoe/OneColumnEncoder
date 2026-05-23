using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.Commands
{
    public class AnalyzeSrcVideoCmd(
        Func<string> getFfprobePath,
        Func<string> getSourcePath,
        VideoAnalysisM analysis,
        SourceCheckCardVM srcValidationCard,
        ModalNavS modalNavS) : AsyncBaseCmd
    {
        private readonly Func<string> _getFfprobePath = getFfprobePath;
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly SourceCheckCardVM _srcValidationCard = srcValidationCard;
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
                string rawJson = await FfprobeVideoAnalysisH.AnalyzeAsync(ffprobePath, sourcePath);

                _analysis.FfprobePath = ffprobePath;
                _analysis.SourcePath = sourcePath;
                _analysis.RawJson = rawJson;
                _srcValidationCard.ApplyFfprobeAnalysisJson(rawJson);

                new OpenInfoOrDbgModalCmd(
                    _modalNavS,
                    UILangProviderM.Current["SrcAnalysis.WindowTitle"],
                    UILangProviderM.Current["SrcAnalysis.Completed"]).Execute(null);
            }
            catch (Exception ex)
            {
                _srcValidationCard.SetAnalysisFailedStatus();
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProviderM.Current["SrcAnalysis.WindowTitle"],
                    ex.Message).Execute(null);
            }
        }
    }
}
