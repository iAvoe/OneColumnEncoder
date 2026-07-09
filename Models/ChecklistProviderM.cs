namespace OneColumnEncoder.Models
{
    public class ChecklistProviderM
    {
        public static List<ChecklistItemDefinitionM> GetToolsChecklist() =>
        [
            .. GetToolsChecklist1(),
            .. GetToolsChecklist2(),
        ];

        public static List<ChecklistItemDefinitionM> GetToolsChecklist1() =>
        [
            new(UILangProviderM.Current["Checklist.Tools.Upstream"], StatusType.Error),
            new(UILangProviderM.Current["Checklist.Tools.Downstream"], StatusType.Error),
            new(UILangProviderM.Current["Checklist.Tools.Analysis"], StatusType.Error),
            new(UILangProviderM.Current["Checklist.Tools.UpstreamPicked"], StatusType.Error),
            new(UILangProviderM.Current["Checklist.Tools.DownstreamPicked"], StatusType.Error),
            new(UILangProviderM.Current["Checklist.Tools.AnalysisPicked"], StatusType.Error),
            // Only Avs2PipeMod needs it, so don't add this check
            // new(UILangProviderM.Current["Checklist.Tools.DependenciesPicked"], StatusType.Error),
        ];

        public static List<ChecklistItemDefinitionM> GetToolsChecklist2() =>
        [
            new(UILangProviderM.Current["Checklist.Tools.VideoSourcePicked"], StatusType.Error),
            new(UILangProviderM.Current["Checklist.Tools.ScriptSourcePicked"]),
            new(UILangProviderM.Current["Checklist.Tools.CompleteSourceAnalysis"], StatusType.Error),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist1() =>
        [
            new(UILangProviderM.Current["Checklist.Source1.Metadata"]),
            new(UILangProviderM.Current["Checklist.Source1.Progressive"]),
            new(UILangProviderM.Current["Checklist.Source1.BitDepth"]),
            new(UILangProviderM.Current["Checklist.Source1.BitDepth2"]),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist2() =>
        [
            new(UILangProviderM.Current["Checklist.Source2.Framerate"]),
            new(UILangProviderM.Current["Checklist.Source2.AspectRatio"]),
            new(UILangProviderM.Current["Checklist.Source2.ColorMetadata"]),
            new(UILangProviderM.Current["Checklist.Source2.ChromaSubsampling"]),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist1() =>
        [
            new(UILangProviderM.Current["Checklist.Enc1.OffGrid"]),
            new(UILangProviderM.Current["Checklist.Enc1.DiskSpace"]),
            new(UILangProviderM.Current["Checklist.Enc1.NumaCpuLoad"]),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist2() =>
        [
            new(UILangProviderM.Current["Checklist.Enc2.WritePermission"]),
            new(UILangProviderM.Current["Checklist.Enc2.Overwrite"]),
            new(UILangProviderM.Current["Checklist.Enc2.LsmashForAvs2Yuv"]),
        ];

        public static List<ChecklistItemDefinitionM> GetBestPracticeChecklist1() =>
        [
            new(UILangProviderM.Current["Checklist.Best1.SlowDisk"]),
            new(UILangProviderM.Current["Checklist.Best1.DiskThrashing"]),
            new(UILangProviderM.Current["Checklist.Best1.BiosDriver"]),
            new(UILangProviderM.Current["Checklist.Best1.Temperature"]),
            new(UILangProviderM.Current["Checklist.Best1.SMR"]),
        ];

        public static List<ChecklistItemDefinitionM> GetBestPracticeChecklist2() =>
        [
            new(UILangProviderM.Current["Checklist.Best2.EncoderVersion"]),
            new(UILangProviderM.Current["Checklist.Best2.FAT32"]),
            new(UILangProviderM.Current["Checklist.Best2.DiskCompression"]),
        ];
    }
}
