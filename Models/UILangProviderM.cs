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
            ["Cards.SourceSevere"] = "Severe (incompatible / corrupted)",
            ["Cards.SourceModerate"] = "Moderate (affecting quality)",
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
            ["ToolField.FileNameWithoutExtension"] = "File name w/out extension",
            ["ToolField.CpuRamNodes"] = "NUMA Nodes",
            ["ToolField.Threads"] = "Threads",
            ["ToolField.Value"] = "Value",
            ["ToolField.Stratagem"] = "Stratagy",
            ["ToolField.MaximumKeyframeGap"] = "Maximum keyframe gap",
            ["ToolField.OtherCustomParams"] = "Other custom params",

            ["Tool.Source.VideoSource"] = "Video Source",
            ["Tool.Source.AviSynth"] = "AviSynth .avs Source",
            ["Tool.Source.VapourSynth"] = "VapourSynth .vpy Source",
            ["Tool.Source.Svfi"] = "SVFI .ini Source",

            ["Tool.Enc.OutputSetting"] = "Output Setting",
            ["Tool.Enc.Parallelism"] = "Parallelism",
            ["Tool.Enc.RateControl"] = "Rate Control Mechanism",
            ["Tool.Enc.BaseParameters"] = "Base Parameters",
            ["Tool.Enc.CustomParameters"] = "Custom Parameters",

            // Dialogs
            ["Dialog.SelectTitle"] = "Select {0}",
            ["Dialog.ReplaceTitle"] = "Replace {0}",
            ["Dialog.Filter.All"] = "All files (*.*)|*.*",
            ["Dialog.Filter.Exe"] = "Executable files (*.exe)|*.exe",
            ["Dialog.Filter.Dll"] = "DLL files (*.dll)|*.dll",

            // Confirmation dialog texts
            ["ConfirmDialog.Cancel"] = "Cancel",
            ["ConfirmDialog.Confirm"] = "Confirm",
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
            ["Checklist.Tools.DependenciesPicked"] = "Click-select a dependency program",
            ["Checklist.Tools.SourcePicked"] = "Click-select a type of source to encode",

            // Checklist - Source Validation 1 (Severe)
            ["Checklist.Source1.Metadata"] = "Metadata and SEI data are readable",
            ["Checklist.Source1.Progressive"] = "Progressive video frame / not interlaced (SVT-AV1 req.)",
            ["Checklist.Source1.BitDepth"] = "Bit-depth is less than 12 (8 or 10, SVT-AV1 req.)",

            // Checklist - Source Validation 2 (Moderate)
            ["Checklist.Source2.Framerate"] = "Framerate is constant / not variable",
            ["Checklist.Source2.AspectRatio"] = "Square pixel aspect ratio / 1:1 SAR",
            ["Checklist.Source2.ColorMatrix"] = "Color matrix metadata is normal",
            ["Checklist.Source2.TransferChars"] = "Transfer characteristics metadata is normal",
            ["Checklist.Source2.ColorPrimaries"] = "Color primaries metadata is normal",
            ["Checklist.Source2.ChromaSubsampling"] = "No chroma subsampling or being \u2190/\u2196 (SVT-AV1 req.)",

            // Checklist - Encoding Prerequisites 1 (Hardware)
            ["Checklist.Enc1.OffGrid"] = "Not off-grid / powering via battery",
            ["Checklist.Enc1.RAM"] = "Sufficient RAM availability",
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
            ["Setting.General.SufficientRAM"] = "Sufficient RAM availability",
            ["Setting.General.SufficientDisk"] = "Sufficient disk space availability",
            ["Setting.General.OSFilename"] = "Output filename is valid for OS",
            ["Setting.General.FTPFilename"] = "Output filename maybe valid for FTP (Pseudo-UTF-8)",
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
            ["SrcScribe.WindowTitle"] = "Generate Upstream Script",
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

            ["SrcAnalysis.WindowTitle"] = "Source Analysis",
            ["SrcAnalysis.Completed"] = "Source analysis completed.",
            ["SrcAnalysis.Copied"] = "Raw ffprobe JSON copied to clipboard."
        },
        ["zh-cn"] = new()
        {
            ["Cards.ToolsImport"] = "选择工具：",
            ["Cards.SourceValidation"] = "视频源检查",
            ["Cards.SourceSevere"] = "严重问题（不兼容/数据损坏）",
            ["Cards.SourceModerate"] = "中等问题（影响质量）",
            ["Cards.EncPrerequisites"] = "开始压制前提",
            ["Cards.EncHardware"] = "硬件条件",
            ["Cards.EncSoftware"] = "软件条件",
            ["Cards.BestPractices"] = "最好看看",
            ["Cards.BestHardware"] = "硬件工况（自查）",
            ["Cards.BestSoftware"] = "软件工况（自查）",

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
            ["ToolField.FileNameWithoutExtension"] = "不含扩展名的文件名",
            ["ToolField.CpuRamNodes"] = "NUMA 节点",
            ["ToolField.Threads"] = "线程",
            ["ToolField.Value"] = "数值",
            ["ToolField.Stratagem"] = "策略",
            ["ToolField.MaximumKeyframeGap"] = "最大关键帧间隔",
            ["ToolField.OtherCustomParams"] = "其他自定义参数",

            ["Tool.Source.VideoSource"] = "视频源",
            ["Tool.Source.AviSynth"] = "AviSynth .avs 源",
            ["Tool.Source.VapourSynth"] = "VapourSynth .vpy 源",
            ["Tool.Source.Svfi"] = "SVFI .ini 源",

            ["Tool.Enc.OutputSetting"] = "输出设置",
            ["Tool.Enc.Parallelism"] = "并行计算机制",
            ["Tool.Enc.RateControl"] = "码率控制机制",
            ["Tool.Enc.BaseParameters"] = "基础参数",
            ["Tool.Enc.CustomParameters"] = "自定义参数",

            ["Dialog.SelectTitle"] = "选择 {0}",
            ["Dialog.ReplaceTitle"] = "替换 {0}",
            ["Dialog.Filter.All"] = "所有文件 (*.*)|*.*",
            ["Dialog.Filter.Exe"] = "可执行文件 (*.exe)|*.exe",
            ["Dialog.Filter.Dll"] = "DLL 文件 (*.dll)|*.dll",

            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "确认",
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
            ["Checklist.Tools.DependenciesPicked"] = "点选依赖程序",
            ["Checklist.Tools.SourcePicked"] = "点选待压制源文件的种类",

            ["Checklist.Source1.Metadata"] = "元数据与 SEI 数据可读",
            ["Checklist.Source1.Progressive"] = "逐行扫描视频帧 / 非隔行（SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth"] = "位深小于 12bit（8 或 10，SVT-AV1 要求）",

            ["Checklist.Source2.Framerate"] = "帧率恒定/非可变帧率（VFR）",
            ["Checklist.Source2.AspectRatio"] = "方形像素变宽比 / 1:1 SAR",
            ["Checklist.Source2.ColorMatrix"] = "色彩矩阵信息正常",
            ["Checklist.Source2.TransferChars"] = "传输特性信息正常",
            ["Checklist.Source2.ColorPrimaries"] = "原色色系信息正常",
            ["Checklist.Source2.ChromaSubsampling"] = "无色度采样压缩或为 \u2190/\u2196（SVT-AV1 要求）",

            ["Checklist.Enc1.OffGrid"] = "使用电池供电 / 离网",
            ["Checklist.Enc1.RAM"] = "内存充足",
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
            ["Setting.General.SufficientRAM"] = "内存不足",
            ["Setting.General.SufficientDisk"] = "磁盘空间不足",
            ["Setting.General.OSFilename"] = "输出文件名不兼容操作系统",
            ["Setting.General.FTPFilename"] = "输出文件名不兼容 FTP（伪 UTF-8）",
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

            ["SrcScribe.WindowTitle"] = "生成上游脚本",
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

            ["SrcAnalysis.WindowTitle"] = "视频源分析",
            ["SrcAnalysis.Completed"] = "视频源分析已完成。",
            ["SrcAnalysis.Copied"] = "ffprobe 原生 JSON 已复制到剪贴板。"
        },
        ["zh-tw"] = new()
        {
            ["Cards.ToolsImport"] = "選擇工具：",
            ["Cards.SourceValidation"] = "影片源檢查",
            ["Cards.SourceSevere"] = "嚴重問題（不相容/數據損壞）",
            ["Cards.SourceModerate"] = "中等問題（影響質量）",
            ["Cards.EncPrerequisites"] = "開始壓制前提",
            ["Cards.EncHardware"] = "硬體條件",
            ["Cards.EncSoftware"] = "軟體條件",
            ["Cards.BestPractices"] = "最好看看",
            ["Cards.BestHardware"] = "硬體工況（自查）",
            ["Cards.BestSoftware"] = "軟體工況（自查）",

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
            ["ToolField.FileNameWithoutExtension"] = "不含副檔名的檔名",
            ["ToolField.CpuRamNodes"] = "NUMA 節點",
            ["ToolField.Threads"] = "執行緒",
            ["ToolField.Value"] = "數值",
            ["ToolField.Stratagem"] = "策略",
            ["ToolField.MaximumKeyframeGap"] = "最大關鍵幀間隔",
            ["ToolField.OtherCustomParams"] = "其他自訂參數",

            ["Tool.Source.VideoSource"] = "視訊來源",
            ["Tool.Source.AviSynth"] = "AviSynth .avs 來源",
            ["Tool.Source.VapourSynth"] = "VapourSynth .vpy 來源",
            ["Tool.Source.Svfi"] = "SVFI .ini 來源",

            ["Tool.Enc.OutputSetting"] = "輸出設定",
            ["Tool.Enc.Parallelism"] = "平行計算機制",
            ["Tool.Enc.RateControl"] = "位元率控制機制",
            ["Tool.Enc.BaseParameters"] = "基礎參數",
            ["Tool.Enc.CustomParameters"] = "自訂參數",

            ["Dialog.SelectTitle"] = "選擇 {0}",
            ["Dialog.ReplaceTitle"] = "替換 {0}",
            ["Dialog.Filter.All"] = "所有檔案 (*.*)|*.*",
            ["Dialog.Filter.Exe"] = "可執行檔 (*.exe)|*.exe",
            ["Dialog.Filter.Dll"] = "DLL 檔案 (*.dll)|*.dll",

            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "確認",
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
            ["Checklist.Tools.DependenciesPicked"] = "點選依賴程式",
            ["Checklist.Tools.SourcePicked"] = "點選待壓制原始檔的種類",

            ["Checklist.Source1.Metadata"] = "元數據與 SEI 數據可讀",
            ["Checklist.Source1.Progressive"] = "逐行掃描影片幀 / 非隔行（SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth"] = "位深小於 12bit（8 或 10，SVT-AV1 要求）",

            ["Checklist.Source2.Framerate"] = "幀率恆定/非可變幀率（VFR）",
            ["Checklist.Source2.AspectRatio"] = "方形像素變寬比 / 1:1 SAR",
            ["Checklist.Source2.ColorMatrix"] = "色彩矩陣資訊正常",
            ["Checklist.Source2.TransferChars"] = "傳輸特性資訊正常",
            ["Checklist.Source2.ColorPrimaries"] = "原色色系資訊正常",
            ["Checklist.Source2.ChromaSubsampling"] = "無色度採樣壓縮或為 \u2190/\u2196（SVT-AV1 要求）",

            ["Checklist.Enc1.OffGrid"] = "使用電池供電 / 離網",
            ["Checklist.Enc1.RAM"] = "記憶體充足",
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
            ["Setting.General.SufficientRAM"] = "記憶體不足",
            ["Setting.General.SufficientDisk"] = "磁碟空間不足",
            ["Setting.General.OSFilename"] = "輸出檔案名不相容操作系統",
            ["Setting.General.FTPFilename"] = "輸出檔案名不相容 FTP（偽 UTF-8）",
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

            ["SrcScribe.WindowTitle"] = "生成上游腳本",
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

            ["SrcAnalysis.WindowTitle"] = "影片源分析",
            ["SrcAnalysis.Completed"] = "影片源分析已完成。",
            ["SrcAnalysis.Copied"] = "ffprobe 原生 JSON 已複製到剪貼簿。"
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
        if (hasChanged)
        {
            CurrentChanged?.Invoke();
        }
    }

    public static void SetLanguage(string languageCode) => _ = new UILangProviderM(languageCode);
}
