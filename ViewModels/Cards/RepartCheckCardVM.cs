namespace OneColumnEncoder.ViewModels.Cards;

public sealed class RepartCheckCardVM : SrcCheckCardVM
{
    public int SourceCount { get; private set; }
    public int OutputCount { get; private set; }

    public void ApplyRepartPlan(RepartPlanM plan)
    {
        SourceCount = plan.Sources.Count;
        OutputCount = plan.Outputs.Count;
        ApplyFfprobeAnalysisJson(plan.ReferenceRawJson);
    }

    public new void RefreshLanguage()
    {
        base.RefreshLanguage();
        Name = RepartLangProvider.Current["ValidationTitle"];
        Subtitle = RepartLangProvider.Current["ValidationSubtitle"];
        P1Name = UICaptionProvider.Cards.SourceIncompatOrCorrupted;
        P3Name = UICaptionProvider.Cards.SrcQualityIssues;
    }
}
