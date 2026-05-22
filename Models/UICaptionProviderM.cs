namespace OneColumnEncoder.Models;

public static class UICaptionProviderM
{
    public static class Cards
    {
        public static string ToolsImport => UILangProviderM.Current["Cards.ToolsImport"];
        public static string SourceValidation => UILangProviderM.Current["Cards.SourceValidation"];
        public static string SourceSevere => UILangProviderM.Current["Cards.SourceSevere"];
        public static string SourceModerate => UILangProviderM.Current["Cards.SourceModerate"];
        public static string EncPrerequisites => UILangProviderM.Current["Cards.EncPrerequisites"];
        public static string EncHardware => UILangProviderM.Current["Cards.EncHardware"];
        public static string EncSoftware => UILangProviderM.Current["Cards.EncSoftware"];
        public static string BestPractices => UILangProviderM.Current["Cards.BestPractices"];
        public static string BestHardware => UILangProviderM.Current["Cards.BestHardware"];
        public static string BestSoftware => UILangProviderM.Current["Cards.BestSoftware"];
    }

    public static class Buttons
    {
        public static string UsageAndCompliance => UILangProviderM.Current["Buttons.UsageAndCompliance"];
        public static string Settings => UILangProviderM.Current["Buttons.Settings"];
        public static string ReEvaluate => UILangProviderM.Current["Buttons.ReEvaluate"];
        public static string RunSample => UILangProviderM.Current["Buttons.RunSample"];
        public static string StartEncode => UILangProviderM.Current["Buttons.StartEncode"];
    }

    public static class AppConf
    {
        public static class Groups
        {
            public static string General => UILangProviderM.Current["AppConf.General"];
            public static string Overwrite => UILangProviderM.Current["AppConf.Overwrite"];
            public static string Smtp => UILangProviderM.Current["AppConf.Smtp"];
            public static string Language => UILangProviderM.Current["AppConf.Language"];
        }

        public static class Buttons
        {
            public static string TestSmtp => UILangProviderM.Current["AppConf.TestSmtp"];
            public static string Cancel => UILangProviderM.Current["AppConf.Cancel"];
            public static string Save => UILangProviderM.Current["AppConf.Save"];
        }

        public static class LanguageOptions
        {
            public static readonly string[] Codes = ["en", "zh-cn", "zh-tw"];
        }
    }
}