using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System;

namespace OneColumnEncoder.Commands
{
    public class InspectSrcProblemsCmd(VideoAnalysisM analysis, SourceCheckCardVM srcValidationCard, ModalNavS modalNavS) : BaseCmd
    {
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly SourceCheckCardVM _srcValidationCard = srcValidationCard;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_analysis.RawJson);

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            string severeText = _srcValidationCard.SevereIssuesFormatted;
            string moderateText = _srcValidationCard.ModerateIssuesFormatted;

            if (severeText.Length == 0 && moderateText.Length == 0)
            {
                new OpenInfoOrDbgModalCmd(
                    _modalNavS,
                    UICaptionProviderM.SourceInspect.InfoTitle,
                    UICaptionProviderM.SourceInspect.InfoMsg).Execute(null);
                return;
            }

            if (severeText.Length > 0)
                new OpenErrModalCmd(_modalNavS, UICaptionProviderM.SourceInspect.ErrorTitle, severeText).Execute(null);

            if (moderateText.Length > 0)
                new OpenWarnModalCmd(_modalNavS, UICaptionProviderM.SourceInspect.WarnTitle, moderateText).Execute(null);
        }
    }
}
