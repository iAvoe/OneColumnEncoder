using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.Commands
{
    public class InspectSrcProblemsCmd(VideoAnalysisM analysis, Func<SourceCheckCardVM> getSrcValidationCard, ModalNavS modalNavS) : BaseCmd
    {
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly Func<SourceCheckCardVM> _getSrcValidationCard = getSrcValidationCard;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_analysis.RawJson);

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            SourceCheckCardVM srcValidationCard = _getSrcValidationCard();
            string severeText = srcValidationCard.SevereIssuesFormatted;
            string moderateText = srcValidationCard.ModerateIssuesFormatted;

            if (severeText.Length == 0 && moderateText.Length == 0)
            {
                new OpenInfoModalCmd(
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
