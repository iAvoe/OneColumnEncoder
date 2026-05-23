using OneColumnEncoder.Models;

namespace OneColumnEncoder.Models
{
    public class ChecklistProviderM
    {
        public static List<ChecklistItemDefinitionM> GetToolsChecklist() =>
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

        public static List<ChecklistItemDefinitionM> GetSourceChecklist1() =>
        [
            new(UILangProviderM.Current["Clecklist.Tools.SourcePicked"]),
            new(UILangProviderM.Current["Checklist.Source1.Metadata"]),
            new(UILangProviderM.Current["Checklist.Source1.Progressive"]),
            new(UILangProviderM.Current["Checklist.Source1.BitDepth"]),
        ];

        public static List<ChecklistItemDefinitionM> GetSourceChecklist2() =>
        [
            new(UILangProviderM.Current["Checklist.Source2.Framerate"]),
            new(UILangProviderM.Current["Checklist.Source2.AspectRatio"]),
            new(UILangProviderM.Current["Checklist.Source2.ColorMatrix"]),
            new(UILangProviderM.Current["Checklist.Source2.TransferChars"]),
            new(UILangProviderM.Current["Checklist.Source2.ColorPrimaries"]),
            new(UILangProviderM.Current["Checklist.Source2.ChromaSubsampling"]),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist1() =>
        [
            new(UILangProviderM.Current["Checklist.Enc1.OffGrid"]),
            new(UILangProviderM.Current["Checklist.Enc1.RAM"]),
            new(UILangProviderM.Current["Checklist.Enc1.DiskSpace"]),
        ];

        public static List<ChecklistItemDefinitionM> GetEncodeChecklist2() =>
        [
            new(UILangProviderM.Current["Checklist.Enc2.OSFilename"]),
            new(UILangProviderM.Current["Checklist.Enc2.FTPFilename"]),
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