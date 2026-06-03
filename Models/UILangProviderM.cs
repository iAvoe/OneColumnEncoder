namespace OneColumnEncoder.Models;

public class UILangProviderM
{
    public static UILangProviderM Current { get; private set; } = null!;
    public static event Action? CurrentChanged;
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            // Cards section headers
            ["Cards.ToolsImport"] = "Select tool:",
            ["Cards.SourceValidation"] = "Source Video Validation",
            ["Cards.SrcIncompatOrCorrupted"] = "Incompatibility / Corrupted (red)",
            ["Cards.SrcQualityIssues"] = "Quality Issues (yellow-orange)",
            ["Cards.EncPrerequisites"] = "Encoding Prerequisites",
            ["Cards.EncHardware"] = "Hardware",
            ["Cards.EncSoftware"] = "Software",
            ["Cards.BestPractices"] = "Best Practices",
            ["Cards.BestHardware"] = "Hardware (self check)",
            ["Cards.BestSoftware"] = "Software (self check)",

            // Main buttons
            ["Buttons.UsageAndCompliance"] = "Usage & Compliance",
            ["Buttons.Settings"] = "Settings",
            ["Buttons.OneClickScriptGen"] = "One-Click Script Gen.",
            ["Buttons.OpenScribeSrcScribe"] = "Open Script Scribe",
            ["Buttons.CopyRawAnalysis"] = "Copy Raw JSON",
            ["Buttons.AnalyzeSrcVideo"] = "Run Source Analysis",
            ["Buttons.ReEvaluate"] = "Re-Evaluate",
            ["Buttons.RunSample"] = "Run a Sample",
            ["Buttons.StartEncode"] = "Start Encode",
            ["Buttons.InspectSrcProbelms"] = "Inspect Source Problems",
            ["Buttons.BypassSrcChecklist"] = "Bypass All Checks",
            ["Buttons.Add"] = "Add",
            ["Buttons.Replace"] = "Replace",
            ["Buttons.Delete"] = "Delete",
            ["Buttons.Clear"] = "Clear",
            ["Buttons.Edit"] = "Edit",

            // AppConf group headers
            ["AppConf.General"] = "General: disable Start Encode upon...",
            ["AppConf.Overwrite"] = "Overwrite Handling",
            ["AppConf.Smtp"] = "SMTP Setting",
            ["AppConf.Language"] = "Language/\u8BED\u8A00",

            // AppConf buttons
            ["AppConf.TestSmtp"] = "Test SMTP",
            ["AppConf.Cancel"] = "Cancel",
            ["AppConf.Save"] = "Save",

            // Section headers in MainUI
            ["Section.ImportTools"] = "Import or Replace Tools",
            ["Section.SelectUpstream"] = "Select Upstream Tool",
            ["Section.SelectEncoder"] = "Select Encoder",
            ["Section.SelectAnalytics"] = "Select Video Analyzer",
            ["Section.SelectDependencies"] = "(When upstream is red) Select Dependencies",
            ["Section.ImportSource"] = "Import or Create Source File",
            ["Section.AnalysisResults"] = "Analysis Results",
            ["Section.EncodingConfigs"] = "Encoding Configurations",
            ["Section.StartEncoding"] = "Start Encoding",

            // AppConfModal window title and header
            ["AppConfModal.Title"] = "1cenc Settings",
            ["AppConfModal.Header"] = "Settings",

            ["Import.NoSelection"] = "No Selection",

            // ItemCard separator
            ["ItemCard.Separator"] = ": ",

            // Tool card captions
            ["ToolField.Version"] = "Version",
            ["ToolField.Path"] = "Path",
            ["ToolField.Name"] = "Name",
            ["ToolField.Mode"] = "Mode",
            ["ToolField.FileName"] = "Filename",
            ["ToolField.NumaNodes"] = "NUMA Affinity", // i.e., upstream from node 0 to encoder at node 1 gives “0 → 1”
            ["ToolField.Threads"] = "Threads", // Value: int thread count, with ToolField.EncThreadClampOn/Off
            ["ToolField.Value"] = "Value",
            ["ToolField.Strategy"] = "Strategy",
            ["ToolField.MaxKeyframeGap"] = "Max keyframe gap",
            ["ToolField.OtherCustomParams"] = "Other custom params",

            ["Tool.Source.VideoSource"] = "Video Source",
            ["Tool.Source.AviSynth"] = "AviSynth .avs Source",
            ["Tool.Source.VapourSynth"] = "VapourSynth .vpy Source",
            ["Tool.Source.Svfi"] = "SVFI .ini Source",

            ["Tool.Enc.OutputSetting"] = "Output Setting",
            ["Tool.Enc.Parallelism"] = "Parallelism Control",
            ["Tool.Enc.EncParams"] = "Encode Settings",

            // Dialogs
            ["Dialog.SelectTitle"] = "Select {0}",
            ["Dialog.ReplaceTitle"] = "Replace {0}",
            ["Dialog.Filter.All"] = "All files (*.*)|*.*",
            ["Dialog.Filter.Exe"] = "Executable files (*.exe)|*.exe",
            ["Dialog.Filter.Dll"] = "DLL files (*.dll)|*.dll",

            // Confirmation dialog texts
            ["ConfirmDialog.Cancel"] = "Cancel",
            ["ConfirmDialog.Confirm"] = "Confirm",
            ["ConfirmDialog.CopyText"] = "Copy Message",
            ["ConfirmDialog.CopyHint"] = "Right-click on text to copy",
            ["ConfirmDialog.WarningPrefix"] = "Warning: ",
            ["ConfirmDialog.ErrorPrefix"] = "Error: ",
            ["ConfirmDialog.DebugPrefix"] = "Debug: ",

            // Confirmation provider messages (with {0} / {1} format placeholders)
            ["ConfirmProvider.SuspiciousImportTitle"] = "Import does not match {0}",
            ["ConfirmProvider.ProceedToRun"] = "Proceed to run {0} to get its version?",
            ["ConfirmProvider.WrongTool"] = "Importing {0} for {1}?",

            // Checklist - Tools
            ["Checklist.Tools.Upstream"] = "One upstream program imported",
            ["Checklist.Tools.Downstream"] = "One downstream program imported",
            ["Checklist.Tools.Analysis"] = "One analysis program imported",
            ["Checklist.Tools.UpstreamPicked"] = "Click-select an upstream program",
            ["Checklist.Tools.DownstreamPicked"] = "Click-select a downstream program",
            ["Checklist.Tools.AnalysisPicked"] = "Click-select an analysis program",
            ["Checklist.Tools.CompleteSourceAnalysis"] = "Complete source analysis",
            ["Checklist.Tools.DependenciesPicked"] = "Click-select a dependency program",
            ["Checklist.Tools.SourcePicked"] = "Source video is imported and selected",

            // Checklist - Source Validation 1 (Severe)
            ["Checklist.Source1.Metadata"] = "Metadata and SEI data are readable",
            ["Checklist.Source1.Progressive"] = "Progressive video frame / not interlaced (SVT-AV1 req.)",
            ["Checklist.Source1.BitDepth"] = "Bit-depth is less than 12 (8 or 10, SVT-AV1 req.)",
            ["Checklist.Source1.BitDepth2"] = "Bit-depth is less than 16",

            // Checklist - Source Validation 2 (Moderate)
            ["Checklist.Source2.Framerate"] = "Framerate is constant / not variable",
            ["Checklist.Source2.AspectRatio"] = "Square pixel aspect ratio / 1:1 SAR",
            ["Checklist.Source2.ColorMatrix"] = "Color matrix metadata is normal",
            ["Checklist.Source2.TransferChars"] = "Transfer characteristics metadata is normal",
            ["Checklist.Source2.ColorPrimaries"] = "Color primaries metadata is normal",
            ["Checklist.Source2.ChromaSubsampling"] = "No chroma subsampling or being \u2190/\u2196 (SVT-AV1 req.)",

            // Checklist - Encoding Prerequisites 1 (Hardware)
            ["Checklist.Enc1.OffGrid"] = "Not off-grid / powering via battery",
            ["Checklist.Enc1.DiskSpace"] = "Sufficient disk space availability",

            // Checklist - Encoding Prerequisites 2 (Software)
            ["Checklist.Enc2.OSFilename"] = "Output filename is valid for OS",
            ["Checklist.Enc2.FTPFilename"] = "Output filename maybe valid for FTP (Pseudo-UTF-8)",
            ["Checklist.Enc2.WritePermission"] = "Write permission in output folder",
            ["Checklist.Enc2.Overwrite"] = "Output does not overwrite existing file",
            ["Checklist.Enc2.LsmashForAvs2Yuv"] = "libvslsmashsource.dll under AviSynth+ path (Avs2Yuv)",

            // Checklist - Best Practices 1 (Hardware)
            ["Checklist.Best1.SlowDisk"] = "Avoiding slow disk connection (USB2, Bluetooth, etc.)",
            ["Checklist.Best1.DiskThrashing"] = "Avoiding disk thrashing (R&W on the same HDD)",
            ["Checklist.Best1.BiosDriver"] = "Using latest BIOS, Chipset driver & hard drive firmware",
            ["Checklist.Best1.Temperature"] = "\u00B0C (\u00B0F): SSD, RAM below 75 (167), HDD below 55 (131)",
            ["Checklist.Best1.SMR"] = "Not writing to a SMR HDD",

            // Checklist - Best Practices 2 (Software)
            ["Checklist.Best2.EncoderVersion"] = "Using latest encoder version",
            ["Checklist.Best2.FAT32"] = "Not writing to a FAT32 volume",
            ["Checklist.Best2.DiskCompression"] = "Output folder disables file system disk compression",

            // Settings - General labels
            ["Setting.General.NotOffGrid"] = "Not off-grid / powering via battery",
            ["Setting.General.SufficientDisk"] = "Sufficient disk space availability",
            ["Setting.General.WritePermission"] = "Write permission in output folder",
            ["Setting.General.NotOverwrite"] = "Output does not overwrite existing file",

            // Settings - Overwrite labels
            ["Setting.Overwrite.LongPressDivisor"] = "Long Press Megabyte Divisor",
            ["Setting.Overwrite.MinLongPress"] = "Minimum Long Press Duration (ms)",
            ["Setting.Overwrite.MaxLongPress"] = "Maximum Long Press Duration (ms)",

            // Settings - SMTP labels
            ["Setting.Smtp.ServerUrl"] = "Server URL",
            ["Setting.Smtp.Port"] = "Port",
            ["Setting.Smtp.UseSSL"] = "Use SSL",
            ["Setting.Smtp.Username"] = "Username",
            ["Setting.Smtp.Password"] = "Password",
            ["Setting.Smtp.FromEmail"] = "From Email Address",
            ["Setting.Smtp.ToEmail"] = "To Email Address",
            ["Setting.Smtp.NotifySuccess"] = "Notify on Success",
            ["Setting.Smtp.NotifyFailure"] = "Notify on Failure",
            ["Setting.Smtp.NotifyAFK"] = "Notify when AFK",
            ["Setting.Smtp.SuccessThreshold"] = "Notify on Success Threshold (min)",
            ["Setting.Smtp.FailureThreshold"] = "Notify on Failure Threshold (min)",
            ["Setting.Smtp.AFKThreshold"] = "Notify on AFK for (min)",

            // Settings - Language label
            ["Setting.Language.Select"] = "Select Language",

            // ScriptSrcScribeModal
            ["SrcScribe.WindowTitle"] = "1cenc Script Generator",
            ["SrcScribe.Description1"] = "Automatically builds a decoder-to-Y4M pipe script based on imported video path. You may paste additional filters here, or copy the In/Out section to your desired script.",
            ["SrcScribe.Description2"] = "If buttons are locked, return to the main UI and import a video file first.",
            ["SrcScribe.NoVidSrcWarning"] = "Please return to the main UI and import a video file first",
            ["SrcScribe.NoteText"] = "Note: Resize window to resize textbox",
            ["SrcScribe.TabAvs"] = "AviSynth (.avs)",
            ["SrcScribe.TabVpy"] = "VapourSynth (.vpy)",
            ["SrcScribe.CopyFull"] = "Copy Full Script",
            ["SrcScribe.CopyInOut"] = "Copy In/Out Section",
            ["SrcScribe.SaveAsFile"] = "Save as File",
            ["SrcScribe.Cancel"] = "Cancel (Close Window Only)",
            ["SrcScribe.Confirm"] = "Confirm (Save & Import All)",
            ["SrcScribe.CopiedFull"] = "Full script copied to clipboard!",
            ["SrcScribe.CopiedSection"] = "Base in/out section copied to clipboard!",
            ["SrcScribe.FilterAvs"] = "AviSynth Script (*.avs)|*.avs",
            ["SrcScribe.FilterVpy"] = "VapourSynth Script (*.vpy)|*.vpy",
            ["SrcScribe.AvsPrefix"] = "LWLibavVideoSource(\"video file path\")",
            ["SrcScribe.AvsPrefix2"] = "# Add more filters below or leave empty...",
            ["SrcScribe.AvsSuffix"] = "# ... end of edit section",
            ["SrcScribe.VpyPrefix"] = "import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"video file path\")",
            ["SrcScribe.VpyPrefix2"] = "# Add filters here or leave empty ...",
            ["SrcScribe.VpySuffix"] = "# ... end of edit section (keep src variable or assign back to src in the end)\r\nsrc.set_output()",
            ["SrcScribe.SavingWindowTitle"] = "Saving all scripts (AVS & VPY)...",

            // FilenameScribeModal
            ["FilenameScribe.WindowTitle"] = "1cenc Filename",
            ["FilenameScribe.MiniHeader"] = "File Name",
            ["FilenameScribe.Placeholder"] = "Type or paste output file name here",
            ["FilenameScribe.PreviewHeader"] = "Preview",
            ["FilenameScribe.Preview30Label"] = "PC & Tablet File List (30 full-width chars)",
            ["FilenameScribe.Preview25Label"] = "Player Title Bar (25 full-width chars)",
            ["FilenameScribe.Preview20Label"] = "Player Sidebar / Playlist (20 full-width chars)",
            ["FilenameScribe.Preview15Label"] = "Mini Phone Display (15 full-width chars)",
            ["FilenameScribe.FormatCheckHeader"] = "Format Check",
            ["FilenameScribe.SevereIssueHeader"] = "Severe issues",
            ["FilenameScribe.GeneralIssueHeader"] = "General issues",
            ["FilenameScribe.CheckEmpty"] = "Not empty",
            ["FilenameScribe.CheckLength"] = "50 characters or less",
            ["FilenameScribe.CheckReserved"] = "Not an OS reserved name",
            ["FilenameScribe.CheckInvalidChars"] = "No quotes or control marks (\" ' ` < > | * ? \\ / : &)",
            ["FilenameScribe.CheckExtendedChars"] = "No character over BMP range (Emoticons, Emoji, etc.)",
            ["FilenameScribe.CheckSpaces"] = "No spaces (use _ or - instead)",
            ["FilenameScribe.CheckCombiningMarks"] = "No Unicode combining marks",
            ["FilenameScribe.CheckSpecialSpaceVariants"] = "No special variant of space character",
            ["FilenameScribe.SelfCheckHeader"] = "Self check: media scraper compatibility",
            ["FilenameScribe.SelfCheck1"] = "Use yyyy-mm-dd date format consistently",
            ["FilenameScribe.SelfCheck2"] = "Ordering: Pad 0s to double & higher digit consecutive #s",
            ["FilenameScribe.SelfCheck3"] = "If the filename is a translated show name, ensure this alias is on TMDB",
            ["FilenameScribe.PasteFromClipboard"] = "Paste from Clipboard",
            ["FilenameScribe.RotateFontSize"] = "Rotate font size",
            ["FilenameScribe.Cancel"] = "Cancel",
            ["FilenameScribe.Confirm"] = "Done → Set Export Path",
            ["FilenameScribe.FooterHint"] = "File extension is set by selected encoder and cannot be edited here",

            // Hints
            ["Hint.SVFIClipDisabled"] = "OneLineShotArgs does not support sample clipping, disabling Run Sample.",
            ["Hint.AnalyzeNeedsSource"] = "Import a source video to run analysis",

            // Heatmap
            ["Heatmap.Cold"] = "Cold",
            ["Heatmap.Hot"] = "Hot",

            ["SrcAnalysis.WindowTitle"] = "1cenc Source Analysis",
            ["SrcAnalysis.Completed"] = "Source analysis completed.",
            ["SrcAnalysis.Copied"] = "Raw ffprobe JSON copied to clipboard.",

            // InspectSrcProblems modal texts
            ["SrcInspect.InfoTitle"] = "Source Check",
            ["SrcInspect.InfoMsg"] = "No obvious source problems were found.",
            ["SrcInspect.ErrorTitle"] = "Source Severe Issues",
            ["SrcInspect.WarnTitle"] = "Source Moderate Issues",
            ["SrcInspect.MetadataP1Text"] = "The source metadata cannot be read. The file may be corrupted or not a video file, and encoding cannot continue because this tool relies on metadata to choose safe encoding parameters.",
            ["SrcInspect.ProgressiveP1Text"] = "This tool cannot inspect inter-frame patterns to configure IVTC filters. See https://iavoe.github.io/deint-ivtc-web-tutorial/HTML for guidance.",
            ["SrcInspect.BitDepthP1Text"] = "SVT-AV1 does not support 12-bit video. If SVT-AV1 is not selected, this issue is treated as a warning instead, and not disabling Start Encode button.",
            ["SrcInspect.FramerateP1Text"] = "This tool cannot align variable-frame-rate sources. Encoding VFR directly may cause audio/video desync over time. To fix it, transcode to FFV1 with ffmpeg, specify the real frame rate via -r <real-frame-rate>.",
            ["SrcInspect.AspectRatioP1Text"] = "This tool cannot compensate for non-square pixels. Continuing may produce unexpected video dimensions. To fix the source, transcode to FFV1 with ffmpeg via -aspect <current-SAR>.",
            ["SrcInspect.ColorMatrixP1Text"] = "Players often fall back to BT.709 when color matrix metadata is missing, but many other matrices exist and only one is correct for the source.",
            ["SrcInspect.TransferCharsP1Text"] = "Players often fall back to BT.709 when transfer characteristics metadata is missing, but many transfer curves exist and only one is correct for the source.",
            ["SrcInspect.ColorPrimariesP1Text"] = "Players often fall back to BT.709 when color primaries metadata is missing, but many primary sets exist and only one is correct for the source.",
            ["SrcInspect.ChromaSubsamplingP1Text"] = "Incorrect chroma sample location can blur colored edges or shift them away from object borders. Unlike AVC and HEVC, AV1 supports only a limited set of chroma sample locations."
        },
        ["zh-cn"] = new()
        {
            ["Cards.ToolsImport"] = "选择工具：",
            ["Cards.SourceValidation"] = "视频源检查",
            ["Cards.SrcIncompatOrCorrupted"] = "兼容问题 / 数据损坏（红色）",
            ["Cards.SrcQualityIssues"] = "质量问题（橙黄色）",
            ["Cards.EncPrerequisites"] = "开始压制前提",
            ["Cards.EncHardware"] = "硬件条件",
            ["Cards.EncSoftware"] = "软件条件",
            ["Cards.BestPractices"] = "最好看看",
            ["Cards.BestHardware"] = "自查：硬件工况",
            ["Cards.BestSoftware"] = "自查：软件工况",

            ["Buttons.UsageAndCompliance"] = "用法与合规指南",
            ["Buttons.Settings"] = "设置",
            ["Buttons.OneClickScriptGen"] = "一键生成脚本",
            ["Buttons.OpenScribeSrcScribe"] = "脚本编辑窗口",
            ["Buttons.CopyRawAnalysis"] = "复制原生 JSON",
            ["Buttons.AnalyzeSrcVideo"] = "运行视频源分析",
            ["Buttons.ReEvaluate"] = "重新检查",
            ["Buttons.RunSample"] = "取段打样",
            ["Buttons.StartEncode"] = "开始压制",
            ["Buttons.InspectSrcProbelms"] = "检阅视频源问题",
            ["Buttons.BypassSrcChecklist"] = "全部绕过",
            ["Buttons.Add"] = "添加",
            ["Buttons.Replace"] = "替换",
            ["Buttons.Delete"] = "删除",
            ["Buttons.Clear"] = "清空",
            ["Buttons.Edit"] = "编辑",

            ["AppConf.General"] = "通用：禁用「开始压制」按钮的时机",
            ["AppConf.Overwrite"] = "文件覆盖确认行为",
            ["AppConf.Smtp"] = "SMTP 消息设置",
            ["AppConf.Language"] = "语言/Language",

            ["AppConf.TestSmtp"] = "发送测试 SMTP",
            ["AppConf.Cancel"] = "取消",
            ["AppConf.Save"] = "保存",

            ["Section.ImportTools"] = "导入或更换程序",
            ["Section.SelectUpstream"] = "选择上游工具",
            ["Section.SelectEncoder"] = "选择下游程序 / 编码器",
            ["Section.SelectAnalytics"] = "选择视频分析工具",
            ["Section.SelectDependencies"] = "（选中的上游程序泛红时）选择依赖文件",
            ["Section.ImportSource"] = "导入或创建源文件",
            ["Section.AnalysisResults"] = "视频源分析报告",
            ["Section.EncodingConfigs"] = "配置编码选项",
            ["Section.StartEncoding"] = "开始压制选项",

            ["AppConfModal.Title"] = "1cenc 设置",
            ["AppConfModal.Header"] = "设置",

            ["Import.NoSelection"] = "未选择",

            ["ItemCard.Separator"] = "：",
            ["ToolField.Version"] = "版本",
            ["ToolField.Path"] = "路径",
            ["ToolField.Name"] = "名称",
            ["ToolField.Mode"] = "模式",
            ["ToolField.FileName"] = "文件名",
            ["ToolField.NumaNodes"] = "NUMA 软绑定",
            ["ToolField.Threads"] = "线程",
            ["ToolField.Value"] = "数值",
            ["ToolField.Strategy"] = "策略",
            ["ToolField.MaxKeyframeGap"] = "最大关键帧间隔",
            ["ToolField.OtherCustomParams"] = "其他自定义参数",

            ["Tool.Source.VideoSource"] = "视频源",
            ["Tool.Source.AviSynth"] = "AviSynth .avs 源",
            ["Tool.Source.VapourSynth"] = "VapourSynth .vpy 源",
            ["Tool.Source.Svfi"] = "SVFI .ini 源",

            ["Tool.Enc.OutputSetting"] = "输出设置",
            ["Tool.Enc.Parallelism"] = "并行计算调度",
            ["Tool.Enc.EncParams"] = "压缩参数配置",

            ["Dialog.SelectTitle"] = "选择 {0}",
            ["Dialog.ReplaceTitle"] = "替换 {0}",
            ["Dialog.Filter.All"] = "所有文件 (*.*)|*.*",
            ["Dialog.Filter.Exe"] = "可执行文件 (*.exe)|*.exe",
            ["Dialog.Filter.Dll"] = "DLL 文件 (*.dll)|*.dll",

            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "确认",
            ["ConfirmDialog.CopyText"] = "复制文本",
            ["ConfirmDialog.CopyHint"] = "右键单击文本以复制",
            ["ConfirmDialog.WarningPrefix"] = "警告：",
            ["ConfirmDialog.ErrorPrefix"] = "错误：",
            ["ConfirmDialog.DebugPrefix"] = "调试：",

            ["ConfirmProvider.SuspiciousImportTitle"] = "导入内容对不上 {0}",
            ["ConfirmProvider.ProceedToRun"] = "继续运行 {0} 以获取其版本？",
            ["ConfirmProvider.WrongTool"] = "将 {0} 导入为 {1}？",

            ["Checklist.Tools.Upstream"] = "至少导入一个上游程序",
            ["Checklist.Tools.Downstream"] = "至少导入一个下游程序",
            ["Checklist.Tools.Analysis"] = "至少导入一个分析程序",
            ["Checklist.Tools.UpstreamPicked"] = "点选上游程序",
            ["Checklist.Tools.DownstreamPicked"] = "点选下游程序",
            ["Checklist.Tools.AnalysisPicked"] = "点选分析工具",
            ["Checklist.Tools.CompleteSourceAnalysis"] = "完成视频源分析",
            ["Checklist.Tools.DependenciesPicked"] = "点选依赖程序",
            ["Checklist.Tools.SourcePicked"] = "待压制的源文件存在且已被选择",

            ["Checklist.Source1.Metadata"] = "元数据与 SEI 数据可读",
            ["Checklist.Source1.Progressive"] = "逐行扫描视频帧 / 非隔行（SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth"] = "位深小于 12bit（8 或 10，SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth2"] = "位深小于 16bit",

            ["Checklist.Source2.Framerate"] = "帧率是否恒定/非可变帧率（VFR）",
            ["Checklist.Source2.AspectRatio"] = "是否为方形像素变宽比 / 1:1 SAR",
            ["Checklist.Source2.ColorMatrix"] = "色彩矩阵信息是否正常",
            ["Checklist.Source2.TransferChars"] = "传输特性信息是否正常",
            ["Checklist.Source2.ColorPrimaries"] = "原色色系信息是否正常",
            ["Checklist.Source2.ChromaSubsampling"] = "是否关闭色度采样压缩或朝向 \u2190/\u2196（SVT-AV1 要求）",

            ["Checklist.Enc1.OffGrid"] = "使用电池供电 / 离网",
            ["Checklist.Enc1.DiskSpace"] = "磁盘空间充足",

            ["Checklist.Enc2.OSFilename"] = "输出文件名兼容操作系统",
            ["Checklist.Enc2.FTPFilename"] = "输出文件名可能兼容 FTP（伪 UTF-8）",
            ["Checklist.Enc2.WritePermission"] = "输出文件夹有写入权限",
            ["Checklist.Enc2.Overwrite"] = "输出不覆盖现有文件",
            ["Checklist.Enc2.LsmashForAvs2Yuv"] = "AviSynth+ 路径含 libvslsmashsource.dll（Avs2Yuv）",

            ["Checklist.Best1.SlowDisk"] = "避免低速磁盘连接协议（USB2、蓝牙等）",
            ["Checklist.Best1.DiskThrashing"] = "避免 HDD 磁头寻道冲突（同盘读写或非机械盘）",
            ["Checklist.Best1.BiosDriver"] = "使用最新的 BIOS、芯片组驱动与磁盘固件",
            ["Checklist.Best1.Temperature"] = "温度：SSD、RAM 低于 75\u00B0C，HDD 低于 55\u00B0C",
            ["Checklist.Best1.SMR"] = "不写入 SMR 硬盘",

            ["Checklist.Best2.EncoderVersion"] = "使用最新的编码器版本",
            ["Checklist.Best2.FAT32"] = "不写入 FAT32 分区",
            ["Checklist.Best2.DiskCompression"] = "输出文件夹禁用文件系统磁盘压缩",

            ["Setting.General.NotOffGrid"] = "未使用电池供电/离网",
            ["Setting.General.SufficientDisk"] = "磁盘空间不足",
            ["Setting.General.WritePermission"] = "无输出文件夹写入权限",
            ["Setting.General.NotOverwrite"] = "输出会覆盖现有文件",

            ["Setting.Overwrite.LongPressDivisor"] = "长按兆字节除数",
            ["Setting.Overwrite.MinLongPress"] = "最小长按持续时间（毫秒）",
            ["Setting.Overwrite.MaxLongPress"] = "最大长按持续时间（毫秒）",

            ["Setting.Smtp.ServerUrl"] = "SMTP 服务器网址",
            ["Setting.Smtp.Port"] = "端口号",
            ["Setting.Smtp.UseSSL"] = "使用 SSL",
            ["Setting.Smtp.Username"] = "用户名",
            ["Setting.Smtp.Password"] = "密码（将记住密码）",
            ["Setting.Smtp.FromEmail"] = "发件人邮箱地址",
            ["Setting.Smtp.ToEmail"] = "收件人邮箱地址",
            ["Setting.Smtp.NotifySuccess"] = "成功时通知",
            ["Setting.Smtp.NotifyFailure"] = "失败时通知",
            ["Setting.Smtp.NotifyAFK"] = "仅离开时通知",
            ["Setting.Smtp.SuccessThreshold"] = "成功通知阈值（分钟，0=不管）",
            ["Setting.Smtp.FailureThreshold"] = "失败通知阈值（分钟，0=不管）",
            ["Setting.Smtp.AFKThreshold"] = "判断离开阈值（无操作分钟，0=不管）",

            ["Setting.Language.Select"] = "选择语言",

            ["SrcScribe.WindowTitle"] = "1cenc Script Generator",
            ["SrcScribe.Description1"] = "自动根据已导入的视频构建「调用解码器生成 Y4M 流并导出」的脚本，可以将需要的滤镜粘贴进来，也可以将解码输出段落复制给其它的待命脚本。",
            ["SrcScribe.Description2"] = "若按钮锁定，则先回到主界面完成视频文件导入操作。",
            ["SrcScribe.NoVidSrcWarning"] = "请先回到主界面，完成视频文件导入操作",
            ["SrcScribe.NoteText"] = "注：仅使用「确认」按钮生成的脚本；拖拽窗口边缘以缩放文本框",
            ["SrcScribe.TabAvs"] = "AviSynth (.avs)",
            ["SrcScribe.TabVpy"] = "VapourSynth (.vpy)",
            ["SrcScribe.CopyFull"] = "复制完整脚本",
            ["SrcScribe.CopyInOut"] = "复制输入输出段",
            ["SrcScribe.SaveAsFile"] = "另存为文件",
            ["SrcScribe.Cancel"] = "取消（仅关闭窗口）",
            ["SrcScribe.Confirm"] = "确认（保存并导入所有脚本）",
            ["SrcScribe.CopiedFull"] = "完整脚本已复制到剪贴板",
            ["SrcScribe.CopiedSection"] = "脚本片段已复制到剪贴板",
            ["SrcScribe.FilterAvs"] = "AviSynth 脚本 (*.avs)|*.avs",
            ["SrcScribe.FilterVpy"] = "VapourSynth 脚本 (*.vpy)|*.vpy",
            ["SrcScribe.AvsPrefix"] = "LWLibavVideoSource(\"视频文件路径\")",
            ["SrcScribe.AvsPrefix2"] = "# 在下方添加更多滤镜或留空...",
            ["SrcScribe.AvsSuffix"] = "# ... 编辑结束位置",
            ["SrcScribe.VpyPrefix"] = "import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"视频文件路径\")",
            ["SrcScribe.VpyPrefix2"] = "# 按需在此加入滤镜或留空...",
            ["SrcScribe.VpySuffix"] = "# ... 编辑结束位置（沿用 src 或在最后赋值回 src）\r\nsrc.set_output()",
            ["SrcScribe.SavingWindowTitle"] = "保存所有脚本到文件 (AVS & VPY)...",

            // FilenameScribeModal
            ["FilenameScribe.WindowTitle"] = "1cenc Filename",
            ["FilenameScribe.MiniHeader"] = "文件名",
            ["FilenameScribe.Placeholder"] = "在此写入或粘贴导出文件名",
            ["FilenameScribe.PreviewHeader"] = "预览效果",
            ["FilenameScribe.Preview30Label"] = "PC 与平板电脑文件列表（30 全宽字长度）",
            ["FilenameScribe.Preview25Label"] = "播放器标题栏（25 全宽字长度）",
            ["FilenameScribe.Preview20Label"] = "播放器侧边栏 / 播放列表（20 全宽字长度）",
            ["FilenameScribe.Preview15Label"] = "小屏幕手机端（15 全宽字长度）",
            ["FilenameScribe.FormatCheckHeader"] = "格式检查",
            ["FilenameScribe.SevereIssueHeader"] = "严重问题",
            ["FilenameScribe.GeneralIssueHeader"] = "一般问题",
            ["FilenameScribe.CheckEmpty"] = "不为空",
            ["FilenameScribe.CheckLength"] = "50 字以内",
            ["FilenameScribe.CheckReserved"] = "非系统保留文件名",
            ["FilenameScribe.CheckInvalidChars"] = "不含引号与控制符（\" ' ` < > | * ? \\ / : &）",
            ["FilenameScribe.CheckExtendedChars"] = "不含超出 BMP 范围的扩展字（颜文字、Emoji 等）",
            ["FilenameScribe.CheckSpaces"] = "无空格（用 _ 或 - 替代）",
            ["FilenameScribe.CheckCombiningMarks"] = "不含 Unicode 组合附加符",
            ["FilenameScribe.CheckSpecialSpaceVariants"] = "不含特殊空格变体",
            ["FilenameScribe.SelfCheckHeader"] = "自查：媒体刮削器兼容性",
            ["FilenameScribe.SelfCheck1"] = "统一用 yyyy-mm-dd 日期格式",
            ["FilenameScribe.SelfCheck2"] = "排序：十位或更高的连续值补零",
            ["FilenameScribe.SelfCheck3"] = "若使用影视作品的译名命名，则确保该译名存在于 TMDB 中",
            ["FilenameScribe.PasteFromClipboard"] = "从剪贴板粘贴",
            ["FilenameScribe.RotateFontSize"] = "轮换字体大小",
            ["FilenameScribe.Cancel"] = "取消",
            ["FilenameScribe.Confirm"] = "确认并定位输出路径",
            ["FilenameScribe.FooterHint"] = "后缀名由选择的编码器程序决定，无法编辑",

            // Hints
            ["Hint.SVFIClipDisabled"] = "OneLineShotArgs 上游不支持取段打样，已禁用取段打样按钮。",
            ["Hint.AnalyzeNeedsSource"] = "分析需要导入视频源文件",

            // Heatmap
            ["Heatmap.Cold"] = "冷",
            ["Heatmap.Hot"] = "热",

            ["SrcAnalysis.WindowTitle"] = "1cenc Source Analysis",
            ["SrcAnalysis.Completed"] = "视频源分析已完成。",
            ["SrcAnalysis.Copied"] = "ffprobe 原生 JSON 已复制到剪贴板。",

            // InspectSrcProblems modal texts
            ["SrcInspect.InfoTitle"] = "视频源检查",
            ["SrcInspect.InfoMsg"] = "未发现明显的源文件问题。",
            ["SrcInspect.ErrorTitle"] = "视频源严重问题",
            ["SrcInspect.WarnTitle"] = "视频源中度问题",
            ["SrcInspect.MetadataP1Text"] = "无法读取视频源元数据。文件可能已经损坏，或本身不是视频文件；本工具依赖元数据选择安全的压制参数，因此无法继续处理。",
            ["SrcInspect.ProgressiveP1Text"] = "本工具无法识别原生隔行或 pulldown，也不能自动选择 IVTC 滤镜。可参考 https://iavoe.github.io/deint-ivtc-web-tutorial/HTML。",
            ["SrcInspect.BitDepthP1Text"] = "SVT-AV1 不支持 12-bit 视频。若当前没有选择 SVT-AV1，此项会作为警告而不是错误处理，也不会禁用开始压制按钮。",
            ["SrcInspect.FramerateP1Text"] = "本工具无法处理可变帧率（VFR）的时间轴对齐。直接压制 VFR 可能随着播放时间推进导致音画不同步。若要修复，可用 ffmpeg 转码为 FFV1，并用 -r <真实帧率> 指定真实帧率。",
            ["SrcInspect.AspectRatioP1Text"] = "本工具无法补偿非方形像素。继续处理可能得到不符合预期的输出宽高。若要修复，可用 ffmpeg 转码为 FFV1，并用 -aspect <当前 SAR> 保留当前 SAR。",
            ["SrcInspect.ColorMatrixP1Text"] = "播放器常在缺少色彩矩阵信息时回退到 BT.709，但实际存在许多色彩矩阵，且只有一种与源文件匹配。",
            ["SrcInspect.TransferCharsP1Text"] = "播放器常在缺少传输特性信息时回退到 BT.709，但实际存在许多传输曲线，且只有一种与源文件匹配。",
            ["SrcInspect.ColorPrimariesP1Text"] = "播放器常在缺少原色色系信息时回退到 BT.709，但实际存在许多原色定义，且只有一种与源文件匹配。",
            ["SrcInspect.ChromaSubsamplingP1Text"] = "色度采样位置错误可能让彩色边缘变糊，或偏离物体边界。不同于 AVC 与 HEVC，AV1 只支持有限的色度采样位置。"
        },
        ["zh-tw"] = new()
        {
            ["Cards.ToolsImport"] = "選擇工具：",
            ["Cards.SourceValidation"] = "影片源檢查",
            ["Cards.SrcIncompatOrCorrupted"] = "相容問題 / 數據損壞（紅色）",
            ["Cards.SrcQualityIssues"] = "品質問題（橙黃色）",
            ["Cards.EncPrerequisites"] = "開始壓制前提",
            ["Cards.EncHardware"] = "硬體條件",
            ["Cards.EncSoftware"] = "軟體條件",
            ["Cards.BestPractices"] = "最好看看",
            ["Cards.BestHardware"] = "自查：硬體工況",
            ["Cards.BestSoftware"] = "自查：軟體工況",

            ["Buttons.UsageAndCompliance"] = "用法與合規指南",
            ["Buttons.Settings"] = "設定",
            ["Buttons.OneClickScriptGen"] = "一鍵生成腳本",
            ["Buttons.OpenScribeSrcScribe"] = "腳本編輯視窗",
            ["Buttons.CopyRawAnalysis"] = "複製原生 JSON",
            ["Buttons.AnalyzeSrcVideo"] = "運行影片源分析",
            ["Buttons.ReEvaluate"] = "重新檢查",
            ["Buttons.RunSample"] = "取段打樣",
            ["Buttons.StartEncode"] = "開始壓制",
            ["Buttons.InspectSrcProbelms"] = "檢閱影片源問題",
            ["Buttons.BypassSrcChecklist"] = "全部繞過",
            ["Buttons.Add"] = "添加",
            ["Buttons.Replace"] = "替換",
            ["Buttons.Delete"] = "刪除",
            ["Buttons.Clear"] = "清空",
            ["Buttons.Edit"] = "編輯",

            ["AppConf.General"] = "通用：禁用「開始壓制」按鈕的時機",
            ["AppConf.Overwrite"] = "文件覆蓋確認行為",
            ["AppConf.Smtp"] = "SMTP 消息設定",
            ["AppConf.Language"] = "語言/Language",

            ["AppConf.TestSmtp"] = "發送測試 SMTP",
            ["AppConf.Cancel"] = "取消",
            ["AppConf.Save"] = "保存",

            ["Section.ImportTools"] = "導入或更換程序",
            ["Section.SelectUpstream"] = "選擇上游工具",
            ["Section.SelectEncoder"] = "選擇下遊程序 / 編碼器",
            ["Section.SelectAnalytics"] = "選擇影片分析工具",
            ["Section.SelectDependencies"] = "（選取的上游程式泛紅時）選擇依賴文件",
            ["Section.ImportSource"] = "導入或創建源文件",
            ["Section.AnalysisResults"] = "影片源分析報告",
            ["Section.EncodingConfigs"] = "配置編碼選項",
            ["Section.StartEncoding"] = "開始壓制選項",

            ["AppConfModal.Title"] = "1cenc 設置",
            ["AppConfModal.Header"] = "設置",

            ["Import.NoSelection"] = "未選擇",

            ["ItemCard.Separator"] = "：",
            ["ToolField.Version"] = "版本",
            ["ToolField.Path"] = "路徑",
            ["ToolField.Name"] = "名稱",
            ["ToolField.Mode"] = "模式",
            ["ToolField.FileName"] = "檔名",
            ["ToolField.NumaNodes"] = "NUMA 軟綁定",
            ["ToolField.Threads"] = "執行緒",
            ["ToolField.Value"] = "數值",
            ["ToolField.Strategy"] = "策略",
            ["ToolField.MaxKeyframeGap"] = "最大關鍵幀間隔",
            ["ToolField.OtherCustomParams"] = "其他自訂參數",

            ["Tool.Source.VideoSource"] = "視訊來源",
            ["Tool.Source.AviSynth"] = "AviSynth .avs 來源",
            ["Tool.Source.VapourSynth"] = "VapourSynth .vpy 來源",
            ["Tool.Source.Svfi"] = "SVFI .ini 來源",

            ["Tool.Enc.OutputSetting"] = "輸出設定",
            ["Tool.Enc.Parallelism"] = "平行計算調度",
            ["Tool.Enc.EncParams"] = "壓縮參數配置",

            ["Dialog.SelectTitle"] = "選擇 {0}",
            ["Dialog.ReplaceTitle"] = "替換 {0}",
            ["Dialog.Filter.All"] = "所有檔案 (*.*)|*.*",
            ["Dialog.Filter.Exe"] = "可執行檔 (*.exe)|*.exe",
            ["Dialog.Filter.Dll"] = "DLL 檔案 (*.dll)|*.dll",

            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "確認",
            ["ConfirmDialog.CopyText"] = "複製文字",
            ["ConfirmDialog.CopyHint"] = "右鍵點擊文字以複製",
            ["ConfirmDialog.WarningPrefix"] = "警告：",
            ["ConfirmDialog.ErrorPrefix"] = "錯誤：",
            ["ConfirmDialog.DebugPrefix"] = "除錯：",

            ["ConfirmProvider.SuspiciousImportTitle"] = "導入內容對不上 {0}",
            ["ConfirmProvider.ProceedToRun"] = "繼續運行 {0} 以獲取其版本？",
            ["ConfirmProvider.WrongTool"] = "將 {0} 導入為 {1}？",

            ["Checklist.Tools.Upstream"] = "至少導入一個上遊程序",
            ["Checklist.Tools.Downstream"] = "至少導入一個下遊程序",
            ["Checklist.Tools.Analysis"] = "至少導入一個分析程序",
            ["Checklist.Tools.UpstreamPicked"] = "點選上遊程序",
            ["Checklist.Tools.DownstreamPicked"] = "點選下遊程序",
            ["Checklist.Tools.AnalysisPicked"] = "點選分析工具",
            ["Checklist.Tools.CompleteSourceAnalysis"] = "完成影片來源分析",
            ["Checklist.Tools.DependenciesPicked"] = "點選依賴程式",
            ["Checklist.Tools.SourcePicked"] = "待壓制的源文件存在且已被選擇",

            ["Checklist.Source1.Metadata"] = "元數據與 SEI 數據可讀",
            ["Checklist.Source1.Progressive"] = "逐行掃描影片幀 / 非隔行（SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth"] = "位深小於 12bit（8 或 10，SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth2"] = "位深小於 16bit",

            ["Checklist.Source2.Framerate"] = "幀率是否恆定/非可變幀率（VFR）",
            ["Checklist.Source2.AspectRatio"] = "是否為方形象素變寬比 / 1:1 SAR",
            ["Checklist.Source2.ColorMatrix"] = "色彩矩陣資訊是否正常",
            ["Checklist.Source2.TransferChars"] = "傳輸特性資訊是否正常",
            ["Checklist.Source2.ColorPrimaries"] = "原色色系資訊是否正常",
            ["Checklist.Source2.ChromaSubsampling"] = "是否關閉色度採樣壓縮或朝向 \u2190/\u2196（SVT-AV1 要求）",

            ["Checklist.Enc1.OffGrid"] = "使用電池供電 / 離網",
            ["Checklist.Enc1.DiskSpace"] = "磁碟空間充足",

            ["Checklist.Enc2.OSFilename"] = "輸出檔案名相容操作系統",
            ["Checklist.Enc2.FTPFilename"] = "輸出檔案名可能相容 FTP（偽 UTF-8）",
            ["Checklist.Enc2.WritePermission"] = "輸出文件夾有寫入權限",
            ["Checklist.Enc2.Overwrite"] = "輸出不覆蓋現有文件",
            ["Checklist.Enc2.LsmashForAvs2Yuv"] = "AviSynth+ 路徑含 libvslsmashsource.dll（Avs2Yuv）",

            ["Checklist.Best1.SlowDisk"] = "避免低速磁碟連接協議（USB2、藍牙等）",
            ["Checklist.Best1.DiskThrashing"] = "避免 HDD 磁頭尋道衝突（同盤讀寫或非機械盤）",
            ["Checklist.Best1.BiosDriver"] = "使用最新的 BIOS、晶片組驅動與磁碟韌體",
            ["Checklist.Best1.Temperature"] = "溫度：SSD、RAM 低於 75\u00B0C，HDD 低於 55\u00B0C",
            ["Checklist.Best1.SMR"] = "不寫入 SMR 硬碟",

            ["Checklist.Best2.EncoderVersion"] = "使用最新的編碼器版本",
            ["Checklist.Best2.FAT32"] = "不寫入 FAT32 分區",
            ["Checklist.Best2.DiskCompression"] = "輸出文件夾禁用文件系統磁碟壓縮",

            ["Setting.General.NotOffGrid"] = "未使用電池供電/離網",
            ["Setting.General.SufficientDisk"] = "磁碟空間不足",
            ["Setting.General.WritePermission"] = "無輸出文件夾寫入權限",
            ["Setting.General.NotOverwrite"] = "輸出會覆蓋現有文件",

            ["Setting.Overwrite.LongPressDivisor"] = "長按百萬位元組除數",
            ["Setting.Overwrite.MinLongPress"] = "最小長按持續時間（毫秒）",
            ["Setting.Overwrite.MaxLongPress"] = "最大長按持續時間（毫秒）",

            ["Setting.Smtp.ServerUrl"] = "SMTP 伺服器網址",
            ["Setting.Smtp.Port"] = "埠號",
            ["Setting.Smtp.UseSSL"] = "使用 SSL",
            ["Setting.Smtp.Username"] = "使用者名稱",
            ["Setting.Smtp.Password"] = "密碼（將記住密碼）",
            ["Setting.Smtp.FromEmail"] = "發件人信箱地址",
            ["Setting.Smtp.ToEmail"] = "收件人信箱地址",
            ["Setting.Smtp.NotifySuccess"] = "成功時通知",
            ["Setting.Smtp.NotifyFailure"] = "失敗時通知",
            ["Setting.Smtp.NotifyAFK"] = "僅離開時通知",
            ["Setting.Smtp.SuccessThreshold"] = "成功通知閾值（分鐘，0=不管）",
            ["Setting.Smtp.FailureThreshold"] = "失敗通知閾值（分鐘，0=不管）",
            ["Setting.Smtp.AFKThreshold"] = "判斷離開閾值（無操作分鐘，0=不管）",

            ["Setting.Language.Select"] = "選擇語言",

            ["SrcScribe.WindowTitle"] = "1cenc Script Generator",
            ["SrcScribe.Description1"] = "自動根據已導入的影片構建「調用解碼器生成 Y4M 流並導出」的腳本，可以將需要的濾鏡粘貼進來，也可以將解碼輸出段落複製給其它的待命腳本。",
            ["SrcScribe.Description2"] = "若按鈕鎖定，則先回到主界面完成影片文件導入操作。",
            ["SrcScribe.NoVidSrcWarning"] = "請先回到主界面，完成影片文件導入操作",
            ["SrcScribe.NoteText"] = "註：僅使用「確認」按鈕生成的腳本；拖拽窗口邊緣以縮放文本框",
            ["SrcScribe.TabAvs"] = "AviSynth (.avs)",
            ["SrcScribe.TabVpy"] = "VapourSynth (.vpy)",
            ["SrcScribe.CopyFull"] = "複製完整腳本",
            ["SrcScribe.CopyInOut"] = "複製輸入/輸出段",
            ["SrcScribe.SaveAsFile"] = "另存為文件",
            ["SrcScribe.Cancel"] = "取消（僅關閉窗口）",
            ["SrcScribe.Confirm"] = "確認（保存並導入所有腳本）",
            ["SrcScribe.CopiedFull"] = "完整腳本已複製到剪貼簿",
            ["SrcScribe.CopiedSection"] = "腳本片段已複製到剪貼簿",
            ["SrcScribe.FilterAvs"] = "AviSynth 腳本 (*.avs)|*.avs",
            ["SrcScribe.FilterVpy"] = "VapourSynth 腳本 (*.vpy)|*.vpy",
            ["SrcScribe.AvsPrefix"] = "LWLibavVideoSource(\"影片檔案路徑\")",
            ["SrcScribe.AvsPrefix2"] = "# 在下方新增更多濾鏡或留空...",
            ["SrcScribe.AvsSuffix"] = "# ... 編輯結束位置",
            ["SrcScribe.VpyPrefix"] = "import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"影片檔案路徑\")",
            ["SrcScribe.VpyPrefix2"] = "# 按需在此加入濾鏡或留空...",
            ["SrcScribe.VpySuffix"] = "# ... 編輯結束位置（沿用 src 或在最後賦值回 src）\r\nsrc.set_output()",
            ["SrcScribe.SavingWindowTitle"] = "儲存所有腳本到檔案 (AVS & VPY)...",

            // FilenameScribeModal
            ["FilenameScribe.WindowTitle"] = "1cenc Filename",
            ["FilenameScribe.MiniHeader"] = "檔案名",
            ["FilenameScribe.Placeholder"] = "在此寫入或貼上匯出檔案名稱",
            ["FilenameScribe.PreviewHeader"] = "預覽效果",
            ["FilenameScribe.Preview30Label"] = "PC 與平板電腦文件列表（30 全寬字長度）",
            ["FilenameScribe.Preview25Label"] = "播放器標題欄（25 全寬字長度）",
            ["FilenameScribe.Preview20Label"] = "播放器側邊欄 / 播放列表（20 全寬字長度）",
            ["FilenameScribe.Preview15Label"] = "小螢幕手機端（15 全寬字長度）",
            ["FilenameScribe.FormatCheckHeader"] = "格式檢查",
            ["FilenameScribe.SevereIssueHeader"] = "嚴重問題",
            ["FilenameScribe.GeneralIssueHeader"] = "一般問題",
            ["FilenameScribe.CheckEmpty"] = "不為空",
            ["FilenameScribe.CheckLength"] = "50 字以內",
            ["FilenameScribe.CheckReserved"] = "非系統保留檔案名",
            ["FilenameScribe.CheckInvalidChars"] = "不含引號與控制符（\" ' ` < > | * ? \\ / : &）",
            ["FilenameScribe.CheckExtendedChars"] = "不含超出 BMP 範圍的擴展字（顏文字、Emoji 等）",
            ["FilenameScribe.CheckSpaces"] = "無空格（用 _ 或 - 替代）",
            ["FilenameScribe.CheckCombiningMarks"] = "不含 Unicode 組合附加符",
            ["FilenameScribe.CheckSpecialSpaceVariants"] = "不含特殊空格變體",
            ["FilenameScribe.SelfCheckHeader"] = "自查：媒體刮削器相容性",
            ["FilenameScribe.SelfCheck1"] = "統一用 yyyy-mm-dd 日期格式",
            ["FilenameScribe.SelfCheck2"] = "排序：為十位或更高的連續值補零",
            ["FilenameScribe.SelfCheck3"] = "若使用影視作品的譯名命名，則確保該譯名存在於 TMDB 中",
            ["FilenameScribe.PasteFromClipboard"] = "從剪貼簿貼上",
            ["FilenameScribe.RotateFontSize"] = "輪換字體大小",
            ["FilenameScribe.Cancel"] = "取消",
            ["FilenameScribe.Confirm"] = "確認並定位輸出路徑",
            ["FilenameScribe.FooterHint"] = "副檔名由選擇的編碼器程式決定，無法編輯",

            // Hints
            ["Hint.SVFIClipDisabled"] = "OneLineShotArgs 上游不支援取段打樣，已禁用取段打樣按鈕。",
            ["Hint.AnalyzeNeedsSource"] = "分析需要導入影片源文件",

            // Heatmap
            ["Heatmap.Cold"] = "冷",
            ["Heatmap.Hot"] = "熱",

            ["SrcAnalysis.WindowTitle"] = "1cenc Source Analysis",
            ["SrcAnalysis.Completed"] = "影片源分析已完成。",
            ["SrcAnalysis.Copied"] = "ffprobe 原生 JSON 已複製到剪貼簿。",

            // InspectSrcProblems modal texts
            ["SrcInspect.InfoTitle"] = "影片源檢查",
            ["SrcInspect.InfoMsg"] = "未發現明顯的源文件問題。",
            ["SrcInspect.ErrorTitle"] = "影片源嚴重問題",
            ["SrcInspect.WarnTitle"] = "影片源中等問題",
            ["SrcInspect.MetadataP1Text"] = "無法讀取影片源元數據。文件可能已經損壞，或本身不是影片文件；本工具依賴元數據選擇安全的壓制參數，因此無法繼續處理。",
            ["SrcInspect.ProgressiveP1Text"] = "本工具無法檢查幀間模式、識別原生隔行或 pulldown，也不能自動選擇 IVTC 濾鏡。可參考 https://iavoe.github.io/deint-ivtc-web-tutorial/HTML。",
            ["SrcInspect.BitDepthP1Text"] = "SVT-AV1 不支援 12-bit 影片。若目前沒有選擇 SVT-AV1，此項會作為警告而不是錯誤處理，也不會禁用開始壓制按鈕。",
            ["SrcInspect.FramerateP1Text"] = "無法處理可變幀率（VFR）的時間軸對齊。直接壓制 VFR 可能隨著播放時間推進導致音畫不同步。若要修復，可用 ffmpeg 轉碼為 FFV1，並用 -r <真實幀率> 指定真實幀率。",
            ["SrcInspect.AspectRatioP1Text"] = "無法補償非方形象素。繼續處理可能得到不符合預期的輸出寬高。若要修復，可用 ffmpeg 轉碼為 FFV1，並用 -aspect <目前 SAR> 保留目前 SAR。",
            ["SrcInspect.ColorMatrixP1Text"] = "播放器常在缺少色彩矩陣資訊時回退到 BT.709，但實際存在許多色彩矩陣，且只有一種與源文件匹配。",
            ["SrcInspect.TransferCharsP1Text"] = "播放器常在缺少傳輸特性資訊時回退到 BT.709，但實際存在許多傳輸曲線，且只有一種與源文件匹配。",
            ["SrcInspect.ColorPrimariesP1Text"] = "播放器常在缺少原色色系資訊時回退到 BT.709，但實際存在許多原色定義，且只有一種與源文件匹配。",
            ["SrcInspect.ChromaSubsamplingP1Text"] = "色度採樣位置錯誤可能讓彩色邊緣變糊，或偏離物體邊界。不同於 AVC 與 HEVC，AV1 只支援有限的色度採樣位置。"
        }
    };

    private readonly Dictionary<string, string> _d;
    public string LanguageCode { get; }

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public UILangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        bool hasChanged = Current is null ||
            !string.Equals(Current.LanguageCode, LanguageCode, StringComparison.OrdinalIgnoreCase);
        Current = this;
        if (hasChanged) CurrentChanged?.Invoke();
    }

    public static void SetLanguage(string languageCode) => _ = new UILangProviderM(languageCode);
}
