using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.Commands
{
    public class BypsSrcChecklistCmd(Func<SourceCheckCardVM> getSrcValidationCard, Func<bool> hasRawJson, Action updateEncStartButtonsState) : BaseCmd
    {
        private readonly Func<SourceCheckCardVM> _getSrcValidationCard = getSrcValidationCard;
        private readonly Func<bool> _hasRawJson = hasRawJson;
        private readonly Action _updateEncStartButtonsState = updateEncStartButtonsState;

        public override bool CanExecute(object? parameter) => _hasRawJson();

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            SourceCheckCardVM srcValidationCard = _getSrcValidationCard();
            srcValidationCard.SetBypassed(!srcValidationCard.IsBypassed);
            _updateEncStartButtonsState();
        }
    }
}
