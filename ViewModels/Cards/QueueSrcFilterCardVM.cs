namespace OneColumnEncoder.ViewModels.Cards;

public class QueueSrcFilterCardVM : SrcCheckCardVM
{
    private const string TempYuv420Text = "Temp: Colorspace is YUV420 (SVT-AV1 4.2 req.)";
    private const int TempYuv420ChecklistIdx = 4;

    public int IncludedCount { get; private set; }
    public int ExcludedCount { get; private set; }
    public string QueueJsonPath { get; private set; } = string.Empty;
    public string ExcludedJsonPath { get; private set; } = string.Empty;

    public QueueSrcFilterCardVM()
    {
        Checklist2.Add(new ChecklistEntryVM { Text = TempYuv420Text });
    }

    public void ApplyQueueResult(int includedCount, int excludedCount, string queueJsonPath, string excludedJsonPath)
    {
        IncludedCount = includedCount;
        ExcludedCount = excludedCount;
        QueueJsonPath = queueJsonPath;
        ExcludedJsonPath = excludedJsonPath;
    }

    public void RefreshTempColorspaceStatus(string rawJson)
    {
        if (!FrameRate.TryGetFirstVideoStream(JsonDocument.Parse(rawJson).RootElement, out JsonElement stream))
            return;

        string? pixelFormat = stream.TryGetProperty("pix_fmt", out JsonElement pixFmtElement)
            ? pixFmtElement.GetString()
            : null;

        Checklist2[TempYuv420ChecklistIdx].Status =
            !string.IsNullOrWhiteSpace(pixelFormat) && pixelFormat.Contains("420", StringComparison.OrdinalIgnoreCase)
                ? StatusType.Success
                : StatusType.Warning;
    }

    public new void RefreshLanguage()
    {
        base.RefreshLanguage();
        Name = UILangProvider.Current["Cards.QueueSourceFilter"];
        Subtitle = UILangProvider.Current["Cards.QueueSourceFilterSubtitle"];
        P1Name = UICaptionProvider.Cards.SourceIncompatOrCorrupted;
        P3Name = UICaptionProvider.Cards.SrcQualityIssues;
        Checklist2[TempYuv420ChecklistIdx].Text = TempYuv420Text;
    }
}
