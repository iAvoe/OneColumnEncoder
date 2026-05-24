using OneColumnEncoder.ViewModels.Cards;
using System;

namespace OneColumnEncoder.Commands
{
    public class BypsSrcChecklistCmd(SourceCheckCardVM srcValidationCard, Func<bool> hasRawJson, Action updateEncStartButtonsState) : BaseCmd
    {
        private readonly SourceCheckCardVM _srcValidationCard = srcValidationCard;
        private readonly Func<bool> _hasRawJson = hasRawJson;
        private readonly Action _updateEncStartButtonsState = updateEncStartButtonsState;

        public override bool CanExecute(object? parameter) => _hasRawJson();

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            _srcValidationCard.SetBypassed(!_srcValidationCard.IsBypassed);
            _updateEncStartButtonsState();
        }
    }
}
