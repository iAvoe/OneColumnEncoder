using OneColumnEncoder.Validation;

namespace OneColumnEncoder.ViewModels.Cards;

public class EncTermsCardVM : ValidationCardBaseVM
{
    private const int OffGridChecklistIdx = 0;
    private const int DiskSpaceChecklistIdx = 1;
    private const int NumaCpuLoadChecklistIdx = 2;

    private const int WritePermissionChecklistIdx = 0;
    private const int OverwriteChecklistIdx = 1;
    private const int LsmashChecklistIdx = 2;

    private const double DiskSpaceSafetyMultiplier = 1.5;
    private const long FallbackMinDiskBytes = 1L * 1024 * 1024 * 1024;

    public Func<string>? GetOutputDirectoryFunc { get; set; }
    public Func<string>? GetOutputFilePathFunc { get; set; }
    public Func<bool>? IsAvs2yuvSelectedFunc { get; set; }
    public Func<string>? GetAviSynthDllPathFunc { get; set; }
    public Func<string>? GetSourceVideoFilePathFunc { get; set; }
    public Func<int>? GetEncoderNodeIdFunc { get; set; }

    public EncTermsCardVM()
    {
        FillCollection(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
        FillCollection(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
    }

    public void RunAllChecks()
    {
        RunChecklist1Checks();
        RunChecklist2Checks();
    }

    public void SetLsmashCheckEnabled(bool isEnabled)
    {
        if (Checklist2.Count <= LsmashChecklistIdx) return;

        Checklist2[LsmashChecklistIdx].IsEnabled = isEnabled;
        if (!isEnabled)
            Checklist2[LsmashChecklistIdx].Status = StatusType.Waiting;
    }

    #region Checklist1: Hardware checks

    private void RunChecklist1Checks()
    {
        SetChecklist1(OffGridChecklistIdx,
            EncTermsCheck.IsOnAcPower()
                ? StatusType.Success
                : StatusType.Warning);

        string? outputDir = GetOutputDirectoryFunc?.Invoke();
        string? sourcePath = GetSourceVideoFilePathFunc?.Invoke();
        SetChecklist1(DiskSpaceChecklistIdx, EvaluateDiskSpace(outputDir, sourcePath));

        SetChecklist1(NumaCpuLoadChecklistIdx,
            EncTermsCheck.EvaluateNumaNodeCpuUsage(GetEncoderNodeIdFunc?.Invoke() ?? -1));
    }

    private static StatusType EvaluateDiskSpace(string? outputDir, string? sourcePath)
    {
        long availBytes = EncTermsCheck.GetAvailableDiskSpaceBytes(outputDir);
        if (availBytes < 0) return StatusType.Waiting;

        long requiredBytes;
        long sourceSize = EncTermsCheck.GetSourceVideoFileSize(sourcePath);

        if (sourceSize > 0)
            requiredBytes = (long)(sourceSize * DiskSpaceSafetyMultiplier);
        else
            requiredBytes = FallbackMinDiskBytes;

        return availBytes >= requiredBytes
            ? StatusType.Success
            : StatusType.Error;
    }

    #endregion

    #region Checklist2: Software checks

    private void RunChecklist2Checks()
    {
        string? outputDir = GetOutputDirectoryFunc?.Invoke();
        SetChecklist2(WritePermissionChecklistIdx,
            EncTermsCheck.HasWritePermission(outputDir)
                ? StatusType.Success
                : StatusType.Error);

        string? outputPath = GetOutputFilePathFunc?.Invoke();
        SetChecklist2(OverwriteChecklistIdx,
            EncTermsCheck.OutputFileExists(outputPath)
                ? StatusType.Warning
                : StatusType.Success);

        bool isAvs2yuv = IsAvs2yuvSelectedFunc?.Invoke() ?? false;
        string? avisynthPath = GetAviSynthDllPathFunc?.Invoke();
        SetChecklist2(LsmashChecklistIdx,
            !isAvs2yuv
                ? StatusType.Success
                : EncTermsCheck.HasLsmashPlugin(avisynthPath)
                    ? StatusType.Success
                    : StatusType.Error);
    }

    #endregion

    #region Issue Formatting (for inspect modal)

    private static string GetChecklist1Description(int index) => index switch
    {
        OffGridChecklistIdx => UICaptionProvider.EncInspect.P1Text,
        DiskSpaceChecklistIdx => UICaptionProvider.EncInspect.P2Text,
        NumaCpuLoadChecklistIdx => UICaptionProvider.EncInspect.P6Text,
        _ => UICaptionProvider.EncInspect.InfoMsg,
    };

    private static string GetChecklist1Title(int index) => index switch
    {
        OffGridChecklistIdx => UICaptionProvider.EncInspect.P1Title,
        DiskSpaceChecklistIdx => UICaptionProvider.EncInspect.P2Title,
        NumaCpuLoadChecklistIdx => UICaptionProvider.EncInspect.P6Title,
        _ => "",
    };

    private static string GetChecklist2Description(int index) => index switch
    {
        WritePermissionChecklistIdx => UICaptionProvider.EncInspect.P3Text,
        OverwriteChecklistIdx => UICaptionProvider.EncInspect.P4Text,
        LsmashChecklistIdx => UICaptionProvider.EncInspect.P5Text,
        _ => UICaptionProvider.EncInspect.InfoMsg,
    };

    private static string GetChecklist2Title(int index) => index switch
    {
        WritePermissionChecklistIdx => UICaptionProvider.EncInspect.P3Title,
        OverwriteChecklistIdx => UICaptionProvider.EncInspect.P4Title,
        LsmashChecklistIdx => UICaptionProvider.EncInspect.P5Title,
        _ => "",
    };

    public string InspectAllFormatted
    {
        get
        {
            var lines = new System.Collections.Generic.List<string>();
            for (int i = 0; i < Checklist1.Count && i < 3; i++)
            {
                string title = GetChecklist1Title(i);
                string text = GetChecklist1Description(i);
                if (!string.IsNullOrEmpty(title))
                    lines.Add($"{title}\n{text}");
            }
            for (int i = 0; i < Checklist2.Count && i < 3; i++)
            {
                string title = GetChecklist2Title(i);
                string text = GetChecklist2Description(i);
                if (!string.IsNullOrEmpty(title))
                    lines.Add($"{title}\n{text}");
            }
            return string.Join(Environment.NewLine + Environment.NewLine, lines);
        }
    }

    public string Checklist1InspectFormatted
    {
        get
        {
            var lines = new System.Collections.Generic.List<string>();
            for (int i = 0; i < Checklist1.Count && i < 3; i++)
            {
                string title = GetChecklist1Title(i);
                string text = GetChecklist1Description(i);
                if (!string.IsNullOrEmpty(title))
                    lines.Add($"{title}\n{text}");
            }
            return string.Join(Environment.NewLine + Environment.NewLine, lines);
        }
    }

    public string Checklist2InspectFormatted
    {
        get
        {
            var lines = new System.Collections.Generic.List<string>();
            for (int i = 0; i < Checklist2.Count && i < 3; i++)
            {
                string title = GetChecklist2Title(i);
                string text = GetChecklist2Description(i);
                if (!string.IsNullOrEmpty(title))
                    lines.Add($"{title}\n{text}");
            }
            return string.Join(Environment.NewLine + Environment.NewLine, lines);
        }
    }

    #endregion

    public void RefreshLanguage()
    {
        RefreshChecklist(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
        RefreshChecklist(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
    }

    #region Private Checklist Setters

    private void SetChecklist1(int index, StatusType status)
    {
        if (index >= 0 && index < Checklist1.Count)
            Checklist1[index].Status = status;
    }

    private void SetChecklist2(int index, StatusType status)
    {
        if (index >= 0 && index < Checklist2.Count)
            Checklist2[index].Status = status;
    }

    #endregion
}
