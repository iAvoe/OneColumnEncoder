using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System;

namespace OneColumnEncoder.Commands
{
    public class InspectEncProblemsCmd(EncTermsCardVM encTermsCard, ModalNavS modalNavS) : BaseCmd
    {
        private readonly EncTermsCardVM _encTermsCard = encTermsCard;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override void Execute(object? parameter)
        {
            string inspectText = _encTermsCard.InspectAllFormatted;

            if (string.IsNullOrWhiteSpace(inspectText))
            {
                new OpenInfoModalCmd(
                    _modalNavS,
                    UICaptionProviderM.EncInspect.InfoTitle,
                    UICaptionProviderM.EncInspect.InfoMsg).Execute(null);
                return;
            }

            new OpenInfoModalCmd(
                _modalNavS,
                UICaptionProviderM.EncInspect.InfoTitle,
                inspectText).Execute(null);
        }
    }
}
