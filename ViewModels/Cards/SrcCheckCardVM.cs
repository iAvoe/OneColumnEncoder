namespace OneColumnEncoder.ViewModels.Cards;

public class SrcCheckCardVM : ValidationCardBaseVM
{
    public Func<bool>? IsSvtav1SelectedFunc { get; set; }

    private string? _lastAnalysisJson;

    protected const int MetadataChecklistIdx = 0;
    protected const int ProgressiveChecklistIdx = 1;
    protected const int Svtav1BitDepthChecklistIdx = 2;
    protected const int MaxBitDepthChecklistIdx = 3;

    public SrcCheckCardVM()
    {
        FillCollection(Checklist1, ChecklistProviderM.GetSrcChecklist1());
        FillCollection(Checklist2, ChecklistProviderM.GetSrcChecklist2());
    }

    public void ResetAnalysisStatus()
    {
        _lastAnalysisJson = null;
        Subtitle = string.Empty;

        for (int i = 0; i < Checklist1.Count; i++)
            SetChecklist1(i, StatusType.Waiting);
        for (int i = 0; i < Checklist2.Count; i++)
            SetChecklist2(i, StatusType.Waiting);
    }

    #region FFprobe Analysis
    public void ApplyFfprobeAnalysisJson(string rawJson)
    {
        _lastAnalysisJson = rawJson;
        try
        {
            Subtitle = FFProbeHdrInfoReader.Read(rawJson).Summary ?? string.Empty;
            FFProbeSrcValResult result = FFProbeSrcVal.Analyze(rawJson);

            SetChecklist1(MetadataChecklistIdx, StatusType.Success);
            SetChecklist1(ProgressiveChecklistIdx, result.IsProgressive
                ? StatusType.Success : StatusType.Warning); // Now 1cenc indirectly supports interlaced source conversion, getting error generally is a bad experience
            SetChecklist1(Svtav1BitDepthChecklistIdx, result.IsSvtAv1BitDepthSupported
                ? StatusType.Success
                : IsSelectingSvtav1()
                    ? StatusType.Error
                    : StatusType.Warning);
            SetChecklist1(MaxBitDepthChecklistIdx, result.IsMaxBitDepthSupported
                ? StatusType.Success : StatusType.Error);
            SetChecklist2(0, result.HasConstantFrameRate
                ? StatusType.Success : StatusType.Warning);
            SetChecklist2(1, result.HasSquarePixels
                ? StatusType.Success : StatusType.Warning);
            bool hasCompleteColorMetadata = result.HasColorSpace && result.HasColorTransfer && result.HasColorPrimaries;
            SetChecklist2(2, hasCompleteColorMetadata
                ? StatusType.Success : StatusType.Warning);
            SetChecklist2(3, result.HasSupportedChroma
                ? StatusType.Success : StatusType.Warning);
        }
        catch
        {
            SetAnalysisFailedStatus();
            throw;
        }
    }

    private bool IsSelectingSvtav1()
    {
        return IsSvtav1SelectedFunc?.Invoke() ?? false;
    }

    public void RefreshSvtav1BitDepthStatus()
    {
        if (_lastAnalysisJson == null) return;

        try
        {
            SetChecklist1(Svtav1BitDepthChecklistIdx, FFProbeSrcVal.IsSvtAv1BitDepthSupported(_lastAnalysisJson)
                ? StatusType.Success
                : IsSelectingSvtav1()
                    ? StatusType.Error
                    : StatusType.Warning);
        }
        catch
        {
            // Leave current status as-is
        }
    }

    public void SetAnalysisFailedStatus()
    {
        _lastAnalysisJson = null;
        Subtitle = string.Empty;
        SetChecklist1(MetadataChecklistIdx, StatusType.Error);
        SetChecklist1(ProgressiveChecklistIdx, StatusType.Waiting);
        SetChecklist1(Svtav1BitDepthChecklistIdx, StatusType.Waiting);

        for (int i = 0; i < Checklist2.Count; i++) SetChecklist2(i, StatusType.Waiting);
    }

    public SourceCheckSignature GetSignature() => new(
            [.. Checklist1.Select(entry => entry.Status)],
            [.. Checklist2.Select(entry => entry.Status)]);

    #endregion

    public void RefreshLanguage()
    {
        RefreshChecklist(Checklist1, ChecklistProviderM.GetSrcChecklist1());
        RefreshChecklist(Checklist2, ChecklistProviderM.GetSrcChecklist2());
    }

    #region Issue Formatting (for ConfirmationModal and column clicks)

    public string SevereIssuesFormatted => FormatIssues(StatusType.Error);
    public string ModerateIssuesFormatted => FormatIssues(StatusType.Warning);

    public string Checklist1IssuesFormatted => FormatColumnIssues(Checklist1, GetChecklist1IssueDescription);
    public string Checklist2IssuesFormatted => FormatColumnIssues(Checklist2, GetChecklist2IssueDescription);

    private static string FormatColumnIssues(ObservableCollection<ChecklistEntryVM> column, Func<int, string?> getDescription)
    {
        var items = column
            .Select((entry, index) => (entry, description: getDescription(index)))
            .Where(e => e.entry.IsEnabled && e.entry.Status != StatusType.Success && e.entry.Status != StatusType.Waiting)
            .Select(e => FormatIssue(e.entry.Text, e.description));
        return string.Join(Environment.NewLine + Environment.NewLine, items);
    }

    private string FormatIssues(StatusType status)
    {
        var checklist1Issues = Checklist1
            .Select((entry, index) => (entry, description: GetChecklist1IssueDescription(index)))
            .Where(e => e.entry.IsEnabled && e.entry.Status == status)
            .Select(e => FormatIssue(e.entry.Text, e.description));

        var checklist2Issues = Checklist2
            .Select((entry, index) => (entry, description: GetChecklist2IssueDescription(index)))
            .Where(e => e.entry.IsEnabled && e.entry.Status == status)
            .Select(e => FormatIssue(e.entry.Text, e.description));

        return string.Join(
            Environment.NewLine + Environment.NewLine, checklist1Issues.Concat(checklist2Issues).Distinct());
    }

    private static string FormatIssue(string fallbackText, string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return fallbackText;
        return description;
    }

    private static string? GetChecklist1IssueDescription(int index) => index switch
    {
        MetadataChecklistIdx => UICaptionProvider.SourceInspect.MetadataP1Text,
        ProgressiveChecklistIdx => UICaptionProvider.SourceInspect.ProgressiveP1Text,
        Svtav1BitDepthChecklistIdx => UICaptionProvider.SourceInspect.BitDepthP1Text,
        _ => null,
    };

    private static string? GetChecklist2IssueDescription(int index) => index switch
    {
        0 => UICaptionProvider.SourceInspect.FramerateP1Text,
        1 => UICaptionProvider.SourceInspect.AspectRatioP1Text,
        2 => UICaptionProvider.SourceInspect.ColorMetadataP1Text,
        3 => UICaptionProvider.SourceInspect.ChromaSubsamplingP1Text,
        _ => null,
    };

    #endregion

    #region Private Checklist Setters

    protected void SetChecklist1(int index, StatusType status)
    {
        if (index >= 0 && index < Checklist1.Count)
            Checklist1[index].Status = status;
    }

    protected void SetChecklist2(int index, StatusType status)
    {
        if (index >= 0 && index < Checklist2.Count)
            Checklist2[index].Status = status;
    }

    #endregion

}

public sealed record SourceCheckSignature(StatusType[] Checklist1, StatusType[] Checklist2)
{
    public string MatchKey => string.Join(
        "|",
        Checklist1
            .Select(status => ((int)status).ToString())
            .Concat(Checklist2.Select(status => ((int)status).ToString())));

    public bool Matches(SourceCheckSignature other) =>
        Checklist1.SequenceEqual(other.Checklist1) && Checklist2.SequenceEqual(other.Checklist2);
}
