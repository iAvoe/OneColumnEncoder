using OneColumnEncoder.Models;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class ConcatCheckCardVM : SourceCheckCardVM
    {
        public string[] ConcatFilePaths { get; private set; } = [];

        public void ApplyConcatAnalysis(string[] filePaths, bool allValid)
        {
            ConcatFilePaths = filePaths;
        }

        public new void RefreshLanguage()
        {
            base.RefreshLanguage();
            Name = UILangProvider.Current["Cards.ConcatSourceFilter"];
            Subtitle = UILangProvider.Current["Cards.ConcatSourceFilterSubtitle"];
            P1Name = UICaptionProviderM.Cards.SourceIncompatOrCorrupted;
            P3Name = UICaptionProviderM.Cards.SrcQualityIssues;
        }
    }
}
