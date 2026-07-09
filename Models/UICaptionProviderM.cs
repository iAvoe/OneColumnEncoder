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
        public static string BestPracticesSubtitle => UILangProviderM.Current["Cards.BestPracticesSubtitle"];
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
        public static string InspectEncPreProblems => UILangProviderM.Current["Buttons.InspectEncPreProblems"];
        public static string BypassEncChecklist => UILangProviderM.Current["Buttons.BypassEncChecklist"];
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

    public static class EncInspect
    {
        public static string InfoTitle => UILangProviderM.Current["EncInspect.InfoTitle"];
        public static string InfoMsg => UILangProviderM.Current["EncInspect.InfoMsg"];
        public static string P1Text => UILangProviderM.Current["EncInspect.P1Text"];
        public static string P1Title => UILangProviderM.Current["EncInspect.P1Title"];
        public static string P2Text => UILangProviderM.Current["EncInspect.P2Text"];
        public static string P2Title => UILangProviderM.Current["EncInspect.P2Title"];
        public static string P3Text => UILangProviderM.Current["EncInspect.P3Text"];
        public static string P3Title => UILangProviderM.Current["EncInspect.P3Title"];
        public static string P4Text => UILangProviderM.Current["EncInspect.P4Text"];
        public static string P4Title => UILangProviderM.Current["EncInspect.P4Title"];
        public static string P5Text => UILangProviderM.Current["EncInspect.P5Text"];
        public static string P5Title => UILangProviderM.Current["EncInspect.P5Title"];
        public static string P6Text => UILangProviderM.Current["EncInspect.P6Text"];
        public static string P6Title => UILangProviderM.Current["EncInspect.P6Title"];
    }

    public static class Sections
    {
        public static string SelectUpstream => UILangProviderM.Current["Section.SelectUpstream"];
        public static string SelectEncoder => UILangProviderM.Current["Section.SelectEncoder"];
        public static string SelectAnalytics => UILangProviderM.Current["Section.SelectAnalytics"];
        public static string SelectDependencies => UILangProviderM.Current["Section.SelectDependencies"];
        public static string ImportSource => UILangProviderM.Current["Section.ImportSource"];
        public static string AnalysisResults => UILangProviderM.Current["Section.AnalysisResults"];
        public static string EncodingConfigs => UILangProviderM.Current["Section.EncodingConfigs"];
        public static string StartEncoding => UILangProviderM.Current["Section.StartEncoding"];
    }

    public static class Hints
    {
        public static string SVFIClipDisabled => UILangProviderM.Current["Hint.SVFIClipDisabled"];
        public static string AnalyzeNeedsSource => UILangProviderM.Current["Hint.AnalyzeRunConditionDuration"];
        public static string NumaCpuCheckTrigger => UILangProviderM.Current["Hint.NumaCpuCheckTrigger"];
        public static string QueueRouteSampleClipDisabled => UILangProviderM.Current["Hint.QueueRouteSampleClipDisabled"];
        public static string FilterScribeDisabled => UILangProviderM.Current["Hint.FilterScribeDisabled"];
        public static string MinDurationFilter => UILangProviderM.Current["Hint.MinDurationFilter"];
        public static string DurationFilterAllFiltered => UILangProviderM.Current["Hint.DurationFilterAllFiltered"];
        public static string DurationFilterCount => UILangProviderM.Current["Hint.DurationFilterCount"];
    }

    public static class AppConf
    {
        private static AppConfLangProviderM Lang => AppConfLangProviderM.Current;

        public static class Groups
        {
            public static string Overwrite => Lang["AppConf.Overwrite"];
            public static string Language => Lang["AppConf.Language"];
            public static string InitMode => Lang["AppConf.InitMode"];
            public static string Bypass => Lang["AppConf.Bypass"];
        }

        public static class BypassLabels
        {
            public static string SrcValidationGroup => Lang["Setting.Bypass.SrcValidationGroup"];
            public static string EncTermsValidationGroup => Lang["Setting.Bypass.EncTermsValidationGroup"];
        }

        public static class Buttons
        {
            public static string Cancel => Lang["AppConf.Cancel"];
            public static string Save => Lang["AppConf.Save"];
            public static string ClearOldQueueJson => Lang["AppConf.ClearOldQueueJson"];
        }

        public static class LanguageOptions
        {
            public static readonly string[] Codes = ["en", "zh-cn", "zh-tw", "fr", "es", "ja", "ru"];

            public static string GetDisplayName(string code) => code switch
            {
                "fr" => "fr (!Localized)",
                "es" => "es (!Localized)",
                "ja" => "ja (!Localized)",
                "ru" => "ru (!Localized)",
                _ => code
            };
        }
    }
}
