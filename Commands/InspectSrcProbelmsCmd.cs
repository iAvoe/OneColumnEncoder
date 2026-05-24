using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Linq;

namespace OneColumnEncoder.Commands
{
    public class InspectSrcProbelmsCmd(VideoAnalysisM analysis, SourceCheckCardVM srcValidationCard, ModalNavS modalNavS) : BaseCmd
    {
        private readonly VideoAnalysisM _analysis = analysis;
        private readonly SourceCheckCardVM _srcValidationCard = srcValidationCard;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_analysis.RawJson);

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            string[] severeIssues = _srcValidationCard.Checklist1
                .Concat(_srcValidationCard.Checklist2)
                .Where(e => e.IsEnabled && e.Status == StatusType.Error)
                .Select(e => e.Text)
                .ToArray();

            string[] moderateIssues = _srcValidationCard.Checklist1
                .Concat(_srcValidationCard.Checklist2)
                .Where(e => e.IsEnabled && e.Status == StatusType.Warning)
                .Select(e => e.Text)
                .ToArray();

            if (severeIssues.Length == 0 && moderateIssues.Length == 0)
            {
                new OpenInfoOrDbgModalCmd(
                    _modalNavS,
                    "Source Check",
                    "No obvious source problems were found.").Execute(null);
                return;
            }

            if (severeIssues.Length > 0)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    "Source Severe Issues",
                    string.Join(Environment.NewLine, severeIssues.Select(text => $"- {text}"))).Execute(null);
            }

            if (moderateIssues.Length > 0)
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    "Source Moderate Issues",
                    string.Join(Environment.NewLine, moderateIssues.Select(text => $"- {text}"))).Execute(null);
            }
        }
    }
}
