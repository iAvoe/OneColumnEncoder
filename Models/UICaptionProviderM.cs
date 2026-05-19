namespace OneColumnEncoder.Models;

public static class UICaptionProviderM
{
    public static class Cards
    {
        public const string ToolsImport = "Import tools:";
        public const string SourceValidation = "Source Video Validation";
        public const string SourceSevere = "Severe (incompatible / corrupted)";
        public const string SourceModerate = "Moderate (affecting quality)";
        public const string EncPrerequisites = "Encoding Prerequisites";
        public const string EncHardware = "Hardware";
        public const string EncSoftware = "Software";
        public const string BestPractices = "Best Practices";
        public const string BestHardware = "Hardware (self check)";
        public const string BestSoftware = "Software (self check)";
    }

    public static class Buttons
    {
        public const string UsageAndCompliance = "Usage & Compliance";
        public const string Settings = "\u2699\uFE0F Settings";
        public const string ReEvaluate = "Re-Evaluate";
        public const string RunSample = "Run a Sample";
        public const string StartEncode = "Start Encode";
    }

    public static class AppConf
    {
        public static class Groups
        {
            public const string General = "General: disable Start Encode when...";
            public const string Overwrite = "Overwrite Handling";
            public const string Smtp = "SMTP";
            public const string Language = "Language/语言";
        }

        public static class Buttons
        {
            public const string TestSmtp = "Test SMTP";
            public const string Cancel = "Cancel";
            public const string Save = "Save";
        }

        public static class LanguageOptions
        {
            public static readonly string[] Codes = ["en", "zh-cn", "zh-tw"];
        }
    }
}
