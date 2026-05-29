namespace OneColumnEncoder.Models;

public class EncoderConfLangProviderM
{
    public static EncoderConfLangProviderM Current { get; private set; } = null!;
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WindowTitle"] = "Encoder Configuration",
            ["TabRateControl"] = "Rate Control",
            ["TabAdvanced"] = "Advanced",
            ["RateControlModeText"] = "Rate control mode",
            ["CrfSliderLabel"] = "CRF / CQP value",
            ["TargetBitrateText"] = "Target bitrate (kbps)",
            ["PresetText"] = "Encoder preset",
            ["TuneText"] = "Encoder tuning",
            ["ProfileText"] = "Profile",
            ["KeyframeIntervalText"] = "Maximum keyframe interval",
            ["FastDecodeText"] = "Optimize for fast decode",
            ["ZeroLatencyText"] = "Zero latency encoding",
            ["CustomParamsText"] = "Extra encoder arguments",
            ["CustomParamsHint"] = "Additional params will be appended after preset/tune/profile",
            ["CancelButtonText"] = "Cancel",
            ["ConfirmButtonText"] = "Confirm",
            ["ModeCrf"] = "CRF (Constant Rate Factor)",
            ["ModeCbr"] = "CBR (Constant Bitrate)",
            ["ModeVbr"] = "VBR (Variable Bitrate)",
            ["PresetPlacebo"] = "placebo",
            ["PresetVerySlow"] = "veryslow",
            ["PresetSlower"] = "slower",
            ["PresetSlow"] = "slow",
            ["PresetMedium"] = "medium",
            ["PresetFast"] = "fast",
            ["PresetFaster"] = "faster",
            ["PresetVeryFast"] = "veryfast",
            ["PresetSuperFast"] = "superfast",
            ["PresetUltraFast"] = "ultrafast",
            ["TuneNone"] = "none",
            ["TuneFilm"] = "film",
            ["TuneAnimation"] = "animation",
            ["TuneGrain"] = "grain",
            ["TuneStillImage"] = "stillimage",
            ["TunePsnr"] = "psnr",
            ["TuneSsim"] = "ssim",
            ["TuneFastDecode"] = "fastdecode",
            ["TuneZeroLatency"] = "zerolatency",
            ["ProfileAuto"] = "auto",
            ["ProfileMain"] = "main",
            ["ProfileHigh"] = "high",
            ["ProfileHigh10"] = "high10",
            ["ProfileHigh444"] = "high444",
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "编码器参数配置",
            ["TabRateControl"] = "率控制",
            ["TabAdvanced"] = "高级",
            ["RateControlModeText"] = "率控制模式",
            ["CrfSliderLabel"] = "CRF / CQP 数值",
            ["TargetBitrateText"] = "目标码率（kbps）",
            ["PresetText"] = "编码器预设",
            ["TuneText"] = "编码器调优",
            ["ProfileText"] = "档次（Profile）",
            ["KeyframeIntervalText"] = "最大关键帧间隔",
            ["FastDecodeText"] = "优化快速解码",
            ["ZeroLatencyText"] = "零延迟编码",
            ["CustomParamsText"] = "额外编码参数",
            ["CustomParamsHint"] = "额外参数将附加在预设/调优/档次参数之后",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "确认",
            ["ModeCrf"] = "CRF（固定质量因子）",
            ["ModeCbr"] = "CBR（固定码率）",
            ["ModeVbr"] = "VBR（可变码率）",
            ["PresetPlacebo"] = "placebo",
            ["PresetVerySlow"] = "veryslow",
            ["PresetSlower"] = "slower",
            ["PresetSlow"] = "slow",
            ["PresetMedium"] = "medium",
            ["PresetFast"] = "fast",
            ["PresetFaster"] = "faster",
            ["PresetVeryFast"] = "veryfast",
            ["PresetSuperFast"] = "superfast",
            ["PresetUltraFast"] = "ultrafast",
            ["TuneNone"] = "none",
            ["TuneFilm"] = "film",
            ["TuneAnimation"] = "animation",
            ["TuneGrain"] = "grain",
            ["TuneStillImage"] = "stillimage",
            ["TunePsnr"] = "psnr",
            ["TuneSsim"] = "ssim",
            ["TuneFastDecode"] = "fastdecode",
            ["TuneZeroLatency"] = "zerolatency",
            ["ProfileAuto"] = "auto",
            ["ProfileMain"] = "main",
            ["ProfileHigh"] = "high",
            ["ProfileHigh10"] = "high10",
            ["ProfileHigh444"] = "high444",
        },
        ["zh-tw"] = new()
        {
            ["WindowTitle"] = "編碼器參數配置",
            ["TabRateControl"] = "率控制",
            ["TabAdvanced"] = "進階",
            ["RateControlModeText"] = "率控制模式",
            ["CrfSliderLabel"] = "CRF / CQP 數值",
            ["TargetBitrateText"] = "目標碼率（kbps）",
            ["PresetText"] = "編碼器預設",
            ["TuneText"] = "編碼器調優",
            ["ProfileText"] = "檔次（Profile）",
            ["KeyframeIntervalText"] = "最大關鍵幀間隔",
            ["FastDecodeText"] = "優化快速解碼",
            ["ZeroLatencyText"] = "零延遲編碼",
            ["CustomParamsText"] = "額外編碼參數",
            ["CustomParamsHint"] = "額外參數將附加在預設/調優/檔次參數之後",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認",
            ["ModeCrf"] = "CRF（固定品質因子）",
            ["ModeCbr"] = "CBR（固定碼率）",
            ["ModeVbr"] = "VBR（可變碼率）",
            ["PresetPlacebo"] = "placebo",
            ["PresetVerySlow"] = "veryslow",
            ["PresetSlower"] = "slower",
            ["PresetSlow"] = "slow",
            ["PresetMedium"] = "medium",
            ["PresetFast"] = "fast",
            ["PresetFaster"] = "faster",
            ["PresetVeryFast"] = "veryfast",
            ["PresetSuperFast"] = "superfast",
            ["PresetUltraFast"] = "ultrafast",
            ["TuneNone"] = "none",
            ["TuneFilm"] = "film",
            ["TuneAnimation"] = "animation",
            ["TuneGrain"] = "grain",
            ["TuneStillImage"] = "stillimage",
            ["TunePsnr"] = "psnr",
            ["TuneSsim"] = "ssim",
            ["TuneFastDecode"] = "fastdecode",
            ["TuneZeroLatency"] = "zerolatency",
            ["ProfileAuto"] = "auto",
            ["ProfileMain"] = "main",
            ["ProfileHigh"] = "high",
            ["ProfileHigh10"] = "high10",
            ["ProfileHigh444"] = "high444",
        }
    };

    public string WindowTitle { get; }
    public string TabRateControl { get; }
    public string TabAdvanced { get; }
    public string RateControlModeText { get; }
    public string CrfSliderLabel { get; }
    public string TargetBitrateText { get; }
    public string PresetText { get; }
    public string TuneText { get; }
    public string ProfileText { get; }
    public string KeyframeIntervalText { get; }
    public string FastDecodeText { get; }
    public string ZeroLatencyText { get; }
    public string CustomParamsText { get; }
    public string CustomParamsHint { get; }
    public string CancelButtonText { get; }
    public string ConfirmButtonText { get; }
    public string ModeCrf { get; }
    public string ModeCbr { get; }
    public string ModeVbr { get; }
    public string PresetPlacebo { get; }
    public string PresetVerySlow { get; }
    public string PresetSlower { get; }
    public string PresetSlow { get; }
    public string PresetMedium { get; }
    public string PresetFast { get; }
    public string PresetFaster { get; }
    public string PresetVeryFast { get; }
    public string PresetSuperFast { get; }
    public string PresetUltraFast { get; }
    public string TuneNone { get; }
    public string TuneFilm { get; }
    public string TuneAnimation { get; }
    public string TuneGrain { get; }
    public string TuneStillImage { get; }
    public string TunePsnr { get; }
    public string TuneSsim { get; }
    public string TuneFastDecode { get; }
    public string TuneZeroLatency { get; }
    public string ProfileAuto { get; }
    public string ProfileMain { get; }
    public string ProfileHigh { get; }
    public string ProfileHigh10 { get; }
    public string ProfileHigh444 { get; }

    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public EncoderConfLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        WindowTitle = _d["WindowTitle"];
        TabRateControl = _d["TabRateControl"];
        TabAdvanced = _d["TabAdvanced"];
        RateControlModeText = _d["RateControlModeText"];
        CrfSliderLabel = _d["CrfSliderLabel"];
        TargetBitrateText = _d["TargetBitrateText"];
        PresetText = _d["PresetText"];
        TuneText = _d["TuneText"];
        ProfileText = _d["ProfileText"];
        KeyframeIntervalText = _d["KeyframeIntervalText"];
        FastDecodeText = _d["FastDecodeText"];
        ZeroLatencyText = _d["ZeroLatencyText"];
        CustomParamsText = _d["CustomParamsText"];
        CustomParamsHint = _d["CustomParamsHint"];
        CancelButtonText = _d["CancelButtonText"];
        ConfirmButtonText = _d["ConfirmButtonText"];
        ModeCrf = _d["ModeCrf"];
        ModeCbr = _d["ModeCbr"];
        ModeVbr = _d["ModeVbr"];
        PresetPlacebo = _d["PresetPlacebo"];
        PresetVerySlow = _d["PresetVerySlow"];
        PresetSlower = _d["PresetSlower"];
        PresetSlow = _d["PresetSlow"];
        PresetMedium = _d["PresetMedium"];
        PresetFast = _d["PresetFast"];
        PresetFaster = _d["PresetFaster"];
        PresetVeryFast = _d["PresetVeryFast"];
        PresetSuperFast = _d["PresetSuperFast"];
        PresetUltraFast = _d["PresetUltraFast"];
        TuneNone = _d["TuneNone"];
        TuneFilm = _d["TuneFilm"];
        TuneAnimation = _d["TuneAnimation"];
        TuneGrain = _d["TuneGrain"];
        TuneStillImage = _d["TuneStillImage"];
        TunePsnr = _d["TunePsnr"];
        TuneSsim = _d["TuneSsim"];
        TuneFastDecode = _d["TuneFastDecode"];
        TuneZeroLatency = _d["TuneZeroLatency"];
        ProfileAuto = _d["ProfileAuto"];
        ProfileMain = _d["ProfileMain"];
        ProfileHigh = _d["ProfileHigh"];
        ProfileHigh10 = _d["ProfileHigh10"];
        ProfileHigh444 = _d["ProfileHigh444"];
        Current = this;
    }
}
