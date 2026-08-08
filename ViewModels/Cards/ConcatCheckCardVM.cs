namespace OneColumnEncoder.ViewModels.Cards;

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
        P1Name = UICaptionProvider.Cards.SourceIncompatOrCorrupted;
        P3Name = UICaptionProvider.Cards.SrcQualityIssues;
    }
}
