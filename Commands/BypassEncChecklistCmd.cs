using OneColumnEncoder.ViewModels.Cards;
using System;

namespace OneColumnEncoder.Commands
{
    public class BypassEncChecklistCmd(EncTermsCardVM encTermsCard, Action updateEncStartButtonsState) : BaseCmd
    {
        private readonly EncTermsCardVM _encTermsCard = encTermsCard;
        private readonly Action _updateEncStartButtonsState = updateEncStartButtonsState;

        public override void Execute(object? parameter)
        {
            _encTermsCard.SetBypassed(!_encTermsCard.IsBypassed);
            _updateEncStartButtonsState();
        }
    }
}
