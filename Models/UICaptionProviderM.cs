namespace OneColumnEncoder.Models;

public static class UICaptionProviderM
{
    public static class Cards
    {
        public static string ToolsImport => UILangProviderM.Current["Cards.ToolsImport"];
        public static string SourceValidation => UILangProviderM.Current["Cards.SourceValidation"];
        public static string SourceIncompatOrCorrupted => UILangProviderM.Current["Cards.SrcIncompatOrCorrupted"];
        public static string SrcQualityIssues => UILangProviderM.Current["Cards.SrcQualityIssues"];
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
        public static string OneClickScriptGen => UILangProviderM.Current["Buttons.OneClickScriptGen"];
        public static string OpenScribeSrcScribe => UILangProviderM.Current["Buttons.OpenScribeSrcScribe"];
        public static string CopyRawAnalysis => UILangProviderM.Current["Buttons.CopyRawAnalysis"];
        public static string AnalyzeSrcVideo => UILangProviderM.Current["Buttons.AnalyzeSrcVideo"];
        public static string InspectSrcProbelms => UILangProviderM.Current["Buttons.InspectSrcProbelms"];
        public static string BypassSrcChecklist => UILangProviderM.Current["Buttons.BypassSrcChecklist"];
        public static string ReEvaluate => UILangProviderM.Current["Buttons.ReEvaluate"];
        public static string RunSample => UILangProviderM.Current["Buttons.RunSample"];
        public static string StartEncode => UILangProviderM.Current["Buttons.StartEncode"];

    }

    public static class SourceInspect
    {
        public static string InfoTitle => UILangProviderM.Current["SrcInspect.InfoTitle"];
        public static string InfoMsg => UILangProviderM.Current["SrcInspect.InfoMsg"];
        public static string ErrorTitle => UILangProviderM.Current["SrcInspect.ErrorTitle"];
        public static string WarnTitle => UILangProviderM.Current["SrcInspect.WarnTitle"];
        public static string MetadataP1Text => UILangProviderM.Current["SrcInspect.MetadataP1Text"];
        public static string ProgressiveP1Text => UILangProviderM.Current["SrcInspect.ProgressiveP1Text"];
        public static string BitDepthP1Text => UILangProviderM.Current["SrcInspect.BitDepthP1Text"];
        public static string FramerateP1Text => UILangProviderM.Current["SrcInspect.FramerateP1Text"];
        public static string AspectRatioP1Text => UILangProviderM.Current["SrcInspect.AspectRatioP1Text"];
        public static string ColorMatrixP1Text => UILangProviderM.Current["SrcInspect.ColorMatrixP1Text"];
        public static string TransferCharsP1Text => UILangProviderM.Current["SrcInspect.TransferCharsP1Text"];
        public static string ColorPrimariesP1Text => UILangProviderM.Current["SrcInspect.ColorPrimariesP1Text"];
        public static string ChromaSubsamplingP1Text => UILangProviderM.Current["SrcInspect.ChromaSubsamplingP1Text"];
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
