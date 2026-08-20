namespace OneColumnEncoder.Models;

/// <summary>
/// Builds checklist items for validation and best-practice checks.
/// </summary>
public class ChecklistProviderM
{
    public static List<ChecklistItemDefinitionM> GetToolsChecklist() =>
    [
        .. GetToolsChecklist1(),
        .. GetToolsChecklist2(),
    ];

    public static List<ChecklistItemDefinitionM> GetToolsChecklist1() =>
    [
        new(UILangProvider.Current["Checklist.Tools.Upstream"], StatusType.Error),
        new(UILangProvider.Current["Checklist.Tools.Downstream"], StatusType.Error),
        new(UILangProvider.Current["Checklist.Tools.Analysis"], StatusType.Error),
        new(UILangProvider.Current["Checklist.Tools.UpstreamPicked"], StatusType.Error),
        new(UILangProvider.Current["Checklist.Tools.DownstreamPicked"], StatusType.Error),
        new(UILangProvider.Current["Checklist.Tools.AnalysisPicked"], StatusType.Error),
        // Only Avs2PipeMod needs it, so don't add this check
        // new(UILangProvider.Current["Checklist.Tools.DependenciesPicked"], StatusType.Error),
    ];

    public static List<ChecklistItemDefinitionM> GetToolsChecklist2() =>
    [
        new(UILangProvider.Current["Checklist.Tools.VideoSourcePicked"], StatusType.Error),
        new(UILangProvider.Current["Checklist.Tools.ScriptSourcePicked"]),
        new(UILangProvider.Current["Checklist.Tools.CompleteSourceAnalysis"], StatusType.Error),
    ];

    public static List<ChecklistItemDefinitionM> GetSrcChecklist1() =>
    [
        new(UILangProvider.Current["Checklist.Source1.Metadata"]),
        new(UILangProvider.Current["Checklist.Source1.Progressive"]),
        new(UILangProvider.Current["Checklist.Source1.BitDepth"]),
        new(UILangProvider.Current["Checklist.Source1.BitDepth2"]),
    ];

    public static List<ChecklistItemDefinitionM> GetSrcChecklist2() =>
    [
        new(UILangProvider.Current["Checklist.Source2.Framerate"]),
        new(UILangProvider.Current["Checklist.Source2.AspectRatio"]),
        new(UILangProvider.Current["Checklist.Source2.ColorMetadata"]),
        new(UILangProvider.Current["Checklist.Source2.ChromaSubsampling"]),
        // Temporary SVT-AV1 4.2 gate. Remove once over-YUV420 support lands.
        new("Temp: Colorspace is YUV420 (SVT-AV1 4.2 req.)"),
    ];

    public static List<ChecklistItemDefinitionM> GetEncodeChecklist1() =>
    [
        new(UILangProvider.Current["Checklist.Enc1.OffGrid"]),
        new(UILangProvider.Current["Checklist.Enc1.DiskSpace"]),
        new(UILangProvider.Current["Checklist.Enc1.NumaCpuLoad"]),
    ];

    public static List<ChecklistItemDefinitionM> GetEncodeChecklist2() =>
    [
        new(UILangProvider.Current["Checklist.Enc2.WritePermission"]),
        new(UILangProvider.Current["Checklist.Enc2.Overwrite"]),
        new(UILangProvider.Current["Checklist.Enc2.LsmashForAvs2Yuv"]),
    ];

    public static List<ChecklistItemDefinitionM> GetBestPracticeChecklist1() =>
    [
        new(UILangProvider.Current["Checklist.Best1.SlowDisk"]),
        new(UILangProvider.Current["Checklist.Best1.DiskThrashing"]),
        new(UILangProvider.Current["Checklist.Best1.BiosDriver"]),
        new(UILangProvider.Current["Checklist.Best1.Temperature"]),
        new(UILangProvider.Current["Checklist.Best1.SMR"]),
    ];

    public static List<ChecklistItemDefinitionM> GetBestPracticeChecklist2() =>
    [
        new(UILangProvider.Current["Checklist.Best2.EncoderVersion"]),
        new(UILangProvider.Current["Checklist.Best2.FAT32"]),
        new(UILangProvider.Current["Checklist.Best2.DiskCompression"]),
    ];
}
