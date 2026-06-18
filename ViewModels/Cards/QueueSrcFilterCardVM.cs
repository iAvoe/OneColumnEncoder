using OneColumnEncoder.Models;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class QueueSrcFilterCardVM : SourceCheckCardVM
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
            SetSourcePickedStatus(includedCount > 0);
        }

        public new void RefreshLanguage()
        {
            base.RefreshLanguage();
            Name = UILangProviderM.Current["Cards.QueueSourceFilter"];
            Subtitle = UILangProviderM.Current["Cards.QueueSourceFilterSubtitle"];
            P1Name = UICaptionProviderM.Cards.SourceIncompatOrCorrupted;
            P3Name = UICaptionProviderM.Cards.SrcQualityIssues;
        }
    }
}
