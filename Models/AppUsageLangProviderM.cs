namespace OneColumnEncoder.Models;

public class AppUsageLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WindowTitle"] = "1cenc Usage & Compliance",
            ["Title"] = "Usage & Compliance",
            ["HowToUse"] = "How to use this program",
            ["Description"] = "This program stictly follows a top\u2192down, left\u2192right operation sequence——all 'next' buttons are on the right side.",
            ["CopyHint"] = "Tip: This window supports text selection and Ctrl+C to copy texts",
            ["GettingStarted"] = "The simplest way to get started is to:",
            ["Step1"] = "1. Import & select an upstream tool (ffmpeg, vspipe, avs2yuv, etc.)",
            ["Step2"] = "2. Import & select an encoder / downstream tool (x264, x265, etc.)",
            ["Step3"] = "3. Import source video file",
            ["Step4"] = "4. Select encoding settings (that is validated as compatible & healthy)",
            ["Step5"] = "5. Clear the checklist and start",
            ["WhyDisabledTitle"] = "Why is my Start Encoding button disabled",
            ["WhyDisabled1"] = "1. Turn hardware-based disablements off in \u2699\uFE0F Settings",
            ["WhyDisabled2"] = "2. Garbage in, garbage out (most of them are not disabling though)",
            ["WhyDisabled3"] = "3. Encoding of corrupted video can crash your PC (BSOD) in rare cases",
            ["ToolDownloadTitle"] = "Download Video Encoding Related Tools",
            ["ComplianceTitle"] = "Commercial Usage Compliance",
            ["ComplianceDesc"] = "This program is licensed under the Apache License 2.0. For commercial usage, please refer to compliance requirement of the programs imported to this tool.",
            ["LicenseFfmpeg"] = "\u00B7 FFmpeg / FFprobe Legal & License: https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "\u00B7 VapourSynth License: https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "\u00B7 Avs2YUV License: https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "\u00B7 Avs2Pipemod License: https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "\u00B7 SVFI License: https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "\u00B7 x264 License & AVC Patent Info: https://x264.org/licensing/",
            ["LicenseX265"] = "\u00B7 x265 License & HEVC Patent Info: https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "\u00B7 SVT-AV1 / AV1 License Info: https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "...Including the video container formats, audio codecs, and most importantly, the font types involved for commercial usage.",
            ["ComplianceDisclaimer"] = "Users are responsible for ensuring compliance with software licenses, codec patents, media formats, and font licenses in their region.",
            ["ParamConfigTitle"] = "Parameter Configuration Feature Description",
            ["ParamConfigIntro"] = "This program uses an align-with-source strategy for parameter configuration, while attempts to leave enough room for the encoder to perform.",
            ["AutoParamTitle"] = "Auto-specified or Y4M-provided Parameters",
            ["AutoParamBase"] = "· Basic parameters: frame rate, resolution, total frames, color matrix, transfer characteristics, color primaries",
            ["AutoParamEncode"] = "· Encoding parameters: lookahead distance, motion estimation radius, subpixel motion estimation strength",
            ["AutoParamFooter"] = "...excluding encoders that can auto-configure the above parameters",
            ["ManualBaseTitle"] = "Manually Specified - Basic Parameters",
            ["ManualBaseList"] = "· x264: general-purpose, stock footage\n· x265: general-purpose, film, stock footage, anime, exhausive\n· SVT-AV1: ultra HQ, high compression, fast",
            ["ManualBaseFooter"] = "...determined by current CPU perf. & settings exposed to commandline (dev's intent), affecting the precision of bit allocation (% of bits to be losslessly compressed)",
            ["ManualCrfTitle"] = "Manually Specified - Rate Control Modes",
            ["ManualCrfList"] = "· Only the Constant Rate Factor (CRF) mode is supported. Range=UHQ|HQ|Streaming|Lossless|Default\n· x264: 13~16 | 18~20 | 21~24 | 0 | 23\n· x265: 17~20 | 21~25 | 26~30 | 0 | 28\n· SVT-AV1: 28~32 | 33~36 | 37~40 | 1 | 35",
            ["ManualExtTitle"] = "Manually Specified - 3rd-party Extended Parameters",
            ["ManualExtIntro"] = "Modded video encoders may provide extended features, and the settings supported by this program include:",
            ["ManualExtList"] = "· x264: Film Grain Based Rate Distorsion Optimization (FGO)\n· x265: Adaptive Quantization with Hysteresis\n· SVT-AV1: Percise Deblocking Filter (DLF 2), Auto Tile size",
            ["CloseButtonText"] = "Close"
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "1cenc 使用与合规",
            ["Title"] = "使用与合规",
            ["HowToUse"] = "如何使用本程序",
            ["Description"] = "本程序严格遵循自上而下、从左到右的操作顺序——确认按钮皆位于右侧。",
            ["CopyHint"] = "提示：本窗口支持拖选与 Ctrl+C 复制",
            ["GettingStarted"] = "开始使用的最简单方法是：",
            ["Step1"] = "1. 导入并点选上游工具（ffmpeg、vspipe、avs2yuv 等）",
            ["Step2"] = "2. 导入并点选编码器/下游工具（x264、x265 等）",
            ["Step3"] = "3. 导入源视频文件",
            ["Step4"] = "4. 选择编码设置（将验证兼容性与健康状态）",
            ["Step5"] = "5. 清除检查清单并开始",
            ["WhyDisabledTitle"] = "为什么「开始编码」按钮不可用",
            ["WhyDisabled1"] = "1. 在 \u2699\uFE0F 设置中关闭基于硬件的禁用选项",
            ["WhyDisabled2"] = "2. 视频源—朽木不可雕也（但大多数情况并不会禁用）",
            ["WhyDisabled3"] = "3. 编码损坏的视频在极少数情况下可能导致电脑崩溃（蓝屏）",
            ["ToolDownloadTitle"] = "视频压制相关工具下载",
            ["ComplianceTitle"] = "商业使用合规要求",
            ["ComplianceDesc"] = "本程序使用 Apache 2.0 许可证。对于商业用途，请参考导入本工具的程序的相关合规要求。",
            ["LicenseFfmpeg"] = "\u00B7 FFmpeg / FFprobe 法律与许可证：https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "\u00B7 VapourSynth 许可证：https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "\u00B7 Avs2YUV 许可证：https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "\u00B7 Avs2Pipemod 许可证：https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "\u00B7 SVFI 许可证：https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "\u00B7 x264 许可证与 AVC 专利信息：https://x264.org/licensing/",
            ["LicenseX265"] = "\u00B7 x265 许可证与 HEVC 专利信息：https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "\u00B7 SVT-AV1 / AV1 许可证信息：https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "……包括视频容器格式、音频编码器，以及商业用途中涉及的字体的合规要求。",
            ["ComplianceDisclaimer"] = "用户有责任确保其所在地区的软件许可证、编解码器专利、媒体格式和字体许可证的合规性。",
            ["ParamConfigTitle"] = "参数配置功能说明",
            ["ParamConfigIntro"] = "本程序配置参数的策略为保持与源一致，并给予视频编码器充足的发挥空间。",
            ["AutoParamTitle"] = "自动指定或通过 Y4M 管道配置的编码参数",
            ["AutoParamBase"] = "· 基础参数：帧率、分辨率、总帧数、矩阵格式、传输特质、原色色系",
            ["AutoParamEncode"] = "· 编码参数：前瞻帧数、动态搜索直径、子像素对齐搜索强度",
            ["AutoParamFooter"] = "...除能够自主配置上述参数的编码器外",
            ["ManualBaseTitle"] = "手动指定—基础参数",
            ["ManualBaseList"] = "· x264：通用、剪辑素材\n· x265：通用、录像、剪辑素材、动漫、穷举法\n· SVT-AV1：极致画质、压缩优先、速度优先",
            ["ManualBaseFooter"] = "「手动指定」参数由当下的处理器性能与下放到命令行的参数（即开发者意图）而定，这些参数会影响码率分配的精度，即无损压缩机制的压缩率。",
            ["ManualCrfTitle"] = "手动指定—率控制模式",
            ["ManualCrfList"] = "· 仅支持码率调谐常量（CRF）模式，范围-超高清|高清|流媒体|无损|默认\n· x264：13~16 | 18~20 | 21~24 | 0 | 23\n· x265：17~20 | 21~25 | 26~30 | 0 | 28\n· SVT-AV1：28~32 | 33~36 | 37~40 | 1 | 35",
            ["ManualExtTitle"] = "手动指定—第三方扩展参数",
            ["ManualExtIntro"] = "非官方版的视频编码器提供了扩展功能，本程序支持的设置包括：",
            ["ManualExtList"] = "· x264：基于高频信息量的率失真优化（Film Grain Optimization）\n· x265：延迟自适应量化（AQ Hysteresis）\n· SVT-AV1：高精度去块滤镜（DLF 2）、自动计算瓦片大小（Auto Tile）",
            ["CloseButtonText"] = "关闭"
        },
        ["zh-tw"] = new()
        {
            ["WindowTitle"] = "1cenc 使用與合規",
            ["Title"] = "使用與合規",
            ["HowToUse"] = "如何使用本程式",
            ["Description"] = "本程式嚴格遵循自上而下、由左至右的操作順序—確認按鈕皆位於右側。",
            ["CopyHint"] = "提示：本視窗支援拖選與 Ctrl+C 複製",
            ["GettingStarted"] = "開始使用的最簡單方法是：",
            ["Step1"] = "1. 匯入並點選上游工具（ffmpeg、vspipe、avs2yuv 等）",
            ["Step2"] = "2. 匯入並點選編碼器/下游工具（x264、x265 等）",
            ["Step3"] = "3. 匯入來源影片檔案",
            ["Step4"] = "4. 選擇編碼設定（將驗證相容性與健康狀態）",
            ["Step5"] = "5. 清除檢查清單並開始",
            ["WhyDisabledTitle"] = "為什麼「開始編碼」按鈕不可用",
            ["WhyDisabled1"] = "1. 在 \u2699\uFE0F 設定中關閉基於硬體的禁用選項",
            ["WhyDisabled2"] = "2. 影片源—朽木不可雕也（但大多數情況並不會禁用）",
            ["WhyDisabled3"] = "3. 編碼損壞的影片在極少數情況下可能導致電腦當機（藍屏）",
            ["ToolDownloadTitle"] = "視訊壓制相關工具下載",
            ["ComplianceTitle"] = "商業使用合規要求",
            ["ComplianceDesc"] = "本程式使用 Apache 2.0 授權。對於商業用途，請參考導入本程式的程式的相關合規要求。",
            ["LicenseFfmpeg"] = "\u00B7 FFmpeg / FFprobe 法律與授權：https://ffmpeg.org/legal.html",
            ["LicenseVapourSynth"] = "\u00B7 VapourSynth 授權：https://github.com/vapoursynth/vapoursynth/blob/master/COPYING",
            ["LicenseAvs2yuv"] = "\u00B7 Avs2YUV 授權：https://github.com/FFMS/ffms2/blob/master/COPYING.GPLv3",
            ["LicenseAvs2pipemod"] = "\u00B7 Avs2Pipemod 授權：https://github.com/pinterf/AvsPmod",
            ["LicenseSvfi"] = "\u00B7 SVFI 授權：https://github.com/Justin62628/Squirrel-RIFE/blob/master/LICENSE",
            ["LicenseX264"] = "\u00B7 x264 授權與 AVC 專利資訊：https://x264.org/licensing/",
            ["LicenseX265"] = "\u00B7 x265 授權與 HEVC 專利資訊：https://www.videolan.org/developers/x265.html",
            ["LicenseSvtAv1"] = "\u00B7 SVT-AV1 / AV1 授權資訊：https://gitlab.com/AOMediaCodec/SVT-AV1/-/blob/master/LICENSE.md",
            ["ComplianceFooter"] = "……包括影片容器格式、音訊編碼器，以及商業用途中涉及的字型的合規要求。",
            ["ComplianceDisclaimer"] = "使用者有責任確保其所在地區的軟體授權、編解碼器專利、媒體格式和字型授權的合規性。",
            ["ParamConfigTitle"] = "參數配置功能說明",
            ["ParamConfigIntro"] = "本程式配置參數的策略為保持與源一致，並給予視訊編碼器充足的發揮空間。",
            ["AutoParamTitle"] = "自動指定或通過 Y4M 管道配置的編碼參數",
            ["AutoParamBase"] = "· 基礎參數：幀率、解析度、總幀數、矩陣格式、傳輸特質、原色色系",
            ["AutoParamEncode"] = "· 編碼參數：前瞻幀數、動態搜尋直徑、子像素對齊搜尋強度",
            ["AutoParamFooter"] = "...除能夠自主配置上述參數的編碼器外",
            ["ManualBaseTitle"] = "手動指定—基礎參數",
            ["ManualBaseList"] = "· x264：通用、剪輯素材\n· x265：通用、錄像、剪輯素材、動漫、窮舉法\n· SVT-AV1：極致畫質、壓縮優先、速度優先",
            ["ManualBaseFooter"] = "「手動指定」參數當下的處理器效能與下放到命令列的參數（即開發者意圖）而定，這些參數會影響碼率分配的精度，即無損壓縮機制的壓縮率。",
            ["ManualCrfTitle"] = "手動指定—率控制模式",
            ["ManualCrfList"] = "· 僅支援碼率調諧常量（CRF）模式，範圍-超高清|高清|串流媒體|無損|預設\n· x264：13~16 | 18~20 | 21~24 | 0 | 23\n· x265：17~20 | 21~25 | 26~30 | 0 | 28\n· SVT-AV1：28~32 | 33~36 | 37~40 | 1 | 35",
            ["ManualExtTitle"] = "手動指定—第三方擴展參數",
            ["ManualExtIntro"] = "非官方版的視訊編碼器提供了擴展功能，本程式支援的設定包括：",
            ["ManualExtList"] = "· x264：基於高頻資訊量的率失真最佳化（Film Grain Optimization）\n· x265：延遲自適應量化（AQ Hysteresis）\n· SVT-AV1：高精度去塊濾鏡（DLF 2）、自動計算瓦片大小（Auto Tile）",
            ["CloseButtonText"] = "關閉"
        }
    };

    public string WindowTitle { get; }
    public string Title { get; }
    public string HowToUse { get; }
    public string Description { get; }
    public string CopyHint { get; }
    public string GettingStarted { get; }
    public string Step1 { get; }
    public string Step2 { get; }
    public string Step3 { get; }
    public string Step4 { get; }
    public string Step5 { get; }
    public string WhyDisabledTitle { get; }
    public string WhyDisabled1 { get; }
    public string WhyDisabled2 { get; }
    public string WhyDisabled3 { get; }
    public string ToolDownloadTitle { get; }
    public string ToolDownloadLink { get; } = "\u00B7 https://github.com/iAvoe/encoding-tools-download-tutorial";
    public string ComplianceTitle { get; }
    public string ComplianceDesc { get; }
    public string LicenseFfmpeg { get; }
    public string LicenseVapourSynth { get; }
    public string LicenseAvs2yuv { get; }
    public string LicenseAvs2pipemod { get; }
    public string LicenseSvfi { get; }
    public string LicenseX264 { get; }
    public string LicenseX265 { get; }
    public string LicenseSvtAv1 { get; }
    public string ComplianceFooter { get; }
    public string ComplianceDisclaimer { get; }
    public string ParamConfigTitle { get; }
    public string ParamConfigIntro { get; }
    public string AutoParamTitle { get; }
    public string AutoParamBase { get; }
    public string AutoParamEncode { get; }
    public string AutoParamFooter { get; }
    public string ManualBaseTitle { get; }
    public string ManualBaseList { get; }
    public string ManualBaseFooter { get; }
    public string ManualCrfTitle { get; }
    public string ManualCrfList { get; }
    public string ManualExtTitle { get; }
    public string ManualExtIntro { get; }
    public string ManualExtList { get; }
    public string CloseButtonText { get; }

    public AppUsageLangProviderM(string languageCode)
    {
        var d = Data.TryGetValue(languageCode, out var lang) ? lang : Data["en"];
        WindowTitle = d["WindowTitle"];
        Title = d["Title"];
        HowToUse = d["HowToUse"];
        Description = d["Description"];
        CopyHint = d["CopyHint"];
        GettingStarted = d["GettingStarted"];
        Step1 = d["Step1"];
        Step2 = d["Step2"];
        Step3 = d["Step3"];
        Step4 = d["Step4"];
        Step5 = d["Step5"];
        WhyDisabledTitle = d["WhyDisabledTitle"];
        WhyDisabled1 = d["WhyDisabled1"];
        WhyDisabled2 = d["WhyDisabled2"];
        WhyDisabled3 = d["WhyDisabled3"];
        ToolDownloadTitle = d["ToolDownloadTitle"];
        ComplianceTitle = d["ComplianceTitle"];
        ComplianceDesc = d["ComplianceDesc"];
        LicenseFfmpeg = d["LicenseFfmpeg"];
        LicenseVapourSynth = d["LicenseVapourSynth"];
        LicenseAvs2yuv = d["LicenseAvs2yuv"];
        LicenseAvs2pipemod = d["LicenseAvs2pipemod"];
        LicenseSvfi = d["LicenseSvfi"];
        LicenseX264 = d["LicenseX264"];
        LicenseX265 = d["LicenseX265"];
        LicenseSvtAv1 = d["LicenseSvtAv1"];
        ComplianceFooter = d["ComplianceFooter"];
        ComplianceDisclaimer = d["ComplianceDisclaimer"];
        ParamConfigTitle = d["ParamConfigTitle"];
        ParamConfigIntro = d["ParamConfigIntro"];
        AutoParamTitle = d["AutoParamTitle"];
        AutoParamBase = d["AutoParamBase"];
        AutoParamEncode = d["AutoParamEncode"];
        AutoParamFooter = d["AutoParamFooter"];
        ManualBaseTitle = d["ManualBaseTitle"];
        ManualBaseList = d["ManualBaseList"];
        ManualBaseFooter = d["ManualBaseFooter"];
        ManualCrfTitle = d["ManualCrfTitle"];
        ManualCrfList = d["ManualCrfList"];
        ManualExtTitle = d["ManualExtTitle"];
        ManualExtIntro = d["ManualExtIntro"];
        ManualExtList = d["ManualExtList"];
        CloseButtonText = d["CloseButtonText"];
    }
}
