namespace OneColumnEncoder.ViewModels.Cards;

public class QueueSrcFilterCardVM : SrcCheckCardVM
{
    public int IncludedCount { get; private set; }
    public int ExcludedCount { get; private set; }
    public string QueueJsonPath { get; private set; } = string.Empty;
    public string ExcludedJsonPath { get; private set; } = string.Empty;

    public void ApplyQueueResult(int includedCount, int excludedCount, string queueJsonPath, string excludedJsonPath)
    {
        IncludedCount = includedCount;
        ExcludedCount = excludedCount;
        QueueJsonPath = queueJsonPath;
        ExcludedJsonPath = excludedJsonPath;
    }

    public new void RefreshLanguage()
    {
        base.RefreshLanguage();
        Name = UILangProvider.Current["Cards.QueueSourceFilter"];
        Subtitle = UILangProvider.Current["Cards.QueueSourceFilterSubtitle"];
        P1Name = UICaptionProvider.Cards.SourceIncompatOrCorrupted;
        P3Name = UICaptionProvider.Cards.SrcQualityIssues;
    }
}
