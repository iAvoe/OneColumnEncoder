namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Typed accessors for common UI captions.
/// </summary>
public static class UICaptionProvider
{
    public static class Cards
    {
        public static string ToolsImport => UILangProvider.Current["Cards.ToolsImport"];
        public static string SourceValidation => UILangProvider.Current["Cards.SourceValidation"];
        public static string SourceIncompatOrCorrupted => UILangProvider.Current["Cards.SrcIncompatOrCorrupted"];
        public static string SrcQualityIssues => UILangProvider.Current["Cards.SrcQualityIssues"];
        public static string EncPrerequisites => UILangProvider.Current["Cards.EncPrerequisites"];
        public static string EncHardware => UILangProvider.Current["Cards.EncHardware"];
        public static string EncSoftware => UILangProvider.Current["Cards.EncSoftware"];
        public static string BestPractices => UILangProvider.Current["Cards.BestPractices"];
        public static string BestHardware => UILangProvider.Current["Cards.BestHardware"];
        public static string BestSoftware => UILangProvider.Current["Cards.BestSoftware"];
        public static string BestPracticesSubtitle => UILangProvider.Current["Cards.BestPracticesSubtitle"];
    }

    public static class Buttons
    {
        public static string UsageAndCompliance => UILangProvider.Current["Buttons.UsageAndCompliance"];
        public static string Settings => UILangProvider.Current["Buttons.Settings"];
        public static string OneClickScriptGen => UILangProvider.Current["Buttons.OneClickScriptGen"];
        public static string OpenScribeSrcScribe => UILangProvider.Current["Buttons.OpenScribeSrcScribe"];
        public static string ShowRawJSON => UILangProvider.Current["Buttons.ShowRawJSON"];
        public static string AnalyzeSrcVideo => UILangProvider.Current["Buttons.AnalyzeSrcVideo"];
        public static string ReEvaluate => UILangProvider.Current["Buttons.ReEvaluate"];
        public static string RunSample => UILangProvider.Current["Buttons.RunSample"];
        public static string StartEncode => UILangProvider.Current["Buttons.StartEncode"];

    }

    public static class SourceInspect
    {
        public static string InfoTitle => UILangProvider.Current["SrcInspect.InfoTitle"];
        public static string InfoMsg => UILangProvider.Current["SrcInspect.InfoMsg"];
        public static string ErrorTitle => UILangProvider.Current["SrcInspect.ErrorTitle"];
        public static string WarnTitle => UILangProvider.Current["SrcInspect.WarnTitle"];
        public static string MetadataP1Text => UILangProvider.Current["SrcInspect.MetadataP1Text"];
        public static string ProgressiveP1Text => UILangProvider.Current["SrcInspect.ProgressiveP1Text"];
        public static string BitDepthP1Text => UILangProvider.Current["SrcInspect.BitDepthP1Text"];
        public static string FramerateP1Text => UILangProvider.Current["SrcInspect.FramerateP1Text"];
        public static string AspectRatioP1Text => UILangProvider.Current["SrcInspect.AspectRatioP1Text"];
        public static string ColorMetadataP1Text => UILangProvider.Current["SrcInspect.ColorMetadataP1Text"];
        public static string ChromaSubsamplingP1Text => UILangProvider.Current["SrcInspect.ChromaSubsamplingP1Text"];
    }

    public static class EncInspect
    {
        public static string InfoTitle => UILangProvider.Current["EncInspect.InfoTitle"];
        public static string InfoMsg => UILangProvider.Current["EncInspect.InfoMsg"];
        public static string P1Text => UILangProvider.Current["EncInspect.P1Text"];
        public static string P1Title => UILangProvider.Current["EncInspect.P1Title"];
        public static string P2Text => UILangProvider.Current["EncInspect.P2Text"];
        public static string P2Title => UILangProvider.Current["EncInspect.P2Title"];
        public static string P3Text => UILangProvider.Current["EncInspect.P3Text"];
        public static string P3Title => UILangProvider.Current["EncInspect.P3Title"];
        public static string P4Text => UILangProvider.Current["EncInspect.P4Text"];
        public static string P4Title => UILangProvider.Current["EncInspect.P4Title"];
        public static string P5Text => UILangProvider.Current["EncInspect.P5Text"];
        public static string P5Title => UILangProvider.Current["EncInspect.P5Title"];
        public static string P6Text => UILangProvider.Current["EncInspect.P6Text"];
        public static string P6Title => UILangProvider.Current["EncInspect.P6Title"];
    }

    public static class Sections
    {
        public static string SelectUpstream => UILangProvider.Current["Section.SelectUpstream"];
        public static string SelectEncoder => UILangProvider.Current["Section.SelectEncoder"];
        public static string SelectAnalytics => UILangProvider.Current["Section.SelectAnalytics"];
        public static string SelectDependencies => UILangProvider.Current["Section.SelectDependencies"];
        public static string ImportSource => UILangProvider.Current["Section.ImportSrc"];
        public static string AnalysisResults => UILangProvider.Current["Section.AnalysisResults"];
        public static string EncodingConfigs => UILangProvider.Current["Section.EncodingConfigs"];
        public static string StartEncoding => UILangProvider.Current["Section.StartEncoding"];
    }

    public static class Hints
    {
        public static string SVFIClipDisabled => UILangProvider.Current["Hint.SVFIClipDisabled"];
        public static string AnalyzeNeedsSource => UILangProvider.Current["Hint.AnalyzeRunConditionDuration"];
        public static string NumaCpuCheckTrigger => UILangProvider.Current["Hint.NumaCpuCheckTrigger"];
        public static string FfmpegOptional => UILangProvider.Current["Hint.FFmpegOptionalBut"];
        public static string QueueRouteSampleClipDisabled => UILangProvider.Current["Hint.QueueRouteSampleClipDisabled"];
        public static string FilterScribeDisabled => UILangProvider.Current["Hint.FilterScribeDisabled"];
        public static string MinDurationFilter => UILangProvider.Current["Hint.MinDurationFilter"];
        public static string DurationFilterAllFiltered => UILangProvider.Current["Hint.DurationFilterAllFiltered"];
        public static string DurationFilterCount => UILangProvider.Current["Hint.DurationFilterCount"];
    }

    public static class AppConf
    {
        private static AppConfLangProvider Lang => AppConfLangProvider.Current;

        public static class Groups
        {
            public static string Overwrite => Lang["AppConf.Overwrite"];
            public static string Language => Lang["AppConf.Language"];
            public static string InitMode => Lang["AppConf.InitMode"];
            public static string Fonts => Lang["AppConf.Fonts"];
            public static string Logs => Lang["AppConf.Logs"];
            public static string AudioMux => Lang["AppConf.AudioMux"];
            public static string AutoMux => Lang["AppConf.AutoMux"];
            public static string TextEditor => Lang["AppConf.TextEditor"];
        }

        public static class Buttons
        {
            public static string Cancel => Lang["Cancel"];
            public static string Save => Lang["Save"];
            public static string ClearOldQueueJson => Lang["AppConf.ClearOldQueueJson"];
        }

        public static class AudioMuxOptions
        {
            public static readonly string[] Codes =
            [
                nameof(EncodingAudioMuxMode.Disable),
                nameof(EncodingAudioMuxMode.Copy),
                nameof(EncodingAudioMuxMode.ReEncodeAAC320),
                nameof(EncodingAudioMuxMode.ReEncodeAAC256),
                nameof(EncodingAudioMuxMode.ReEncodeAAC128),
                nameof(EncodingAudioMuxMode.ReEncodeOpus320),
                nameof(EncodingAudioMuxMode.ReEncodeOpus256),
                nameof(EncodingAudioMuxMode.ReEncodeOpus128)
            ];

            public static string GetDisplayName(string code) => code switch
            {
                nameof(EncodingAudioMuxMode.Disable) => Lang["AudioMux.Option.Disable"],
                nameof(EncodingAudioMuxMode.Copy) => Lang["AudioMux.Option.Copy"],
                nameof(EncodingAudioMuxMode.ReEncodeAAC320) => Lang["AudioMux.Option.AAC320"],
                nameof(EncodingAudioMuxMode.ReEncodeAAC256) => Lang["AudioMux.Option.AAC256"],
                nameof(EncodingAudioMuxMode.ReEncodeAAC128) => Lang["AudioMux.Option.AAC128"],
                nameof(EncodingAudioMuxMode.ReEncodeOpus320) => Lang["AudioMux.Option.OGG320"],
                nameof(EncodingAudioMuxMode.ReEncodeOpus256) => Lang["AudioMux.Option.OGG256"],
                nameof(EncodingAudioMuxMode.ReEncodeOpus128) => Lang["AudioMux.Option.OGG128"],
                _ => code
            };
        }

        public static class LanguageOptions
        {
            public static readonly string[] Codes = ["en", "zh-cn", "zh-tw", "fr", "es", "ja", "ru", "de", "ko", "pt-br"];

            public static string GetDisplayName(string code) => code switch
            {
                "fr" => "fr (!Localized)",
                "es" => "es (!Localized)",
                "ja" => "ja (!Localized)",
                "ru" => "ru (!Localized)",
                "de" => "de (!Localized)",
                "ko" => "ko (!Localized)",
                "pt-br" => "pt-br (!Local)",
                _ => code
            };
        }
    }
}
