namespace OneColumnEncoder.Models;

public class UILangProviderM
{
    public static UILangProviderM Current { get; private set; } = null!;

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            // Cards section headers
            ["Cards.ToolsImport"] = "Import tools:",
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
            ["Buttons.Settings"] = "\u2699\uFE0F Settings",
            ["Buttons.ReEvaluate"] = "Re-Evaluate",
            ["Buttons.RunSample"] = "Run a Sample",
            ["Buttons.StartEncode"] = "Start Encode",

            // AppConf group headers
            ["AppConf.General"] = "General: disable Start Encode when...",
            ["AppConf.Overwrite"] = "Overwrite Handling",
            ["AppConf.Smtp"] = "SMTP",
            ["AppConf.Language"] = "Language/\u8BED\u8A00",

            // AppConf buttons
            ["AppConf.TestSmtp"] = "Test SMTP",
            ["AppConf.Cancel"] = "Cancel",
            ["AppConf.Save"] = "Save",

            // Section headers in MainUI
            ["Section.ImportTools"] = "Import Tools",
            ["Section.SelectUpstream"] = "Select Upstream Tool",
            ["Section.SelectEncoder"] = "Select Encoder",
            ["Section.SelectAnalytics"] = "Select Analytics & Dependencies",
            ["Section.ImportSource"] = "Import or Create Source File",
            ["Section.AnalysisResults"] = "Analysis Results",
            ["Section.EncodingConfigs"] = "Encoding Configurations",
            ["Section.StartEncoding"] = "Start Encoding",

            // AppConfModal window title and header
            ["AppConfModal.Title"] = "1cenc Settings",
            ["AppConfModal.Header"] = "Settings",

            // Import button on ToolsImportCard
            ["ImportButton"] = "Import",

            // ItemCard separator
            ["ItemCard.Separator"] = ": ",

            // Confirmation dialog texts
            ["ConfirmDialog.Cancel"] = "Cancel",
            ["ConfirmDialog.Confirm"] = "Confirm",
            ["ConfirmDialog.WarningPrefix"] = "Warning: ",
            ["ConfirmDialog.ErrorPrefix"] = "Error: ",
            ["ConfirmDialog.DebugPrefix"] = "Debug: ",

            // Confirmation provider messages (with {0} / {1} format placeholders)
            ["ConfirmProvider.SuspiciousImportTitle"] = "Suspicious import for {0}",
            ["ConfirmProvider.ProceedToRun"] = "Proceed to run {0} to get its version?",
            ["ConfirmProvider.WrongTool"] = "Importing {0} for {1}?",

            // Checklist - Tools
            ["Checklist.Tools.Upstream"] = "One upstream program available",
            ["Checklist.Tools.Downstream"] = "One downstream program available",
            ["Checklist.Tools.Analysis"] = "One analysis program available",

            // Checklist - Source Validation 1 (Severe)
            ["Checklist.Source1.Metadata"] = "Metadata and SEI data are readable",
            ["Checklist.Source1.Progressive"] = "Progressive video frame / not interlaced (SVT-AV1 req.)",
            ["Checklist.Source1.BitDepth"] = "Bit-depth is less than 12 (8 or 10, SVT-AV1 req.)",

            // Checklist - Source Validation 2 (Moderate)
            ["Checklist.Source2.Framerate"] = "Framerate is constant / not variable",
            ["Checklist.Source2.AspectRatio"] = "Square pixel aspect ratio / 1:1 sar",
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
        },
        ["zh-cn"] = new()
        {
            ["Cards.ToolsImport"] = "导入工具：",
            ["Cards.SourceValidation"] = "源视频验证",
            ["Cards.SourceSevere"] = "严重（不兼容/损坏）",
            ["Cards.SourceModerate"] = "中等（影响质量）",
            ["Cards.EncPrerequisites"] = "编码前提条件",
            ["Cards.EncHardware"] = "硬件",
            ["Cards.EncSoftware"] = "软件",
            ["Cards.BestPractices"] = "最佳实践",
            ["Cards.BestHardware"] = "硬件（自查）",
            ["Cards.BestSoftware"] = "软件（自查）",

            ["Buttons.UsageAndCompliance"] = "使用与合规",
            ["Buttons.Settings"] = "\u2699\uFE0F 设置",
            ["Buttons.ReEvaluate"] = "重新评估",
            ["Buttons.RunSample"] = "运行样本",
            ["Buttons.StartEncode"] = "开始编码",

            ["AppConf.General"] = "常规：在以下情况禁用「开始编码」...",
            ["AppConf.Overwrite"] = "覆盖处理",
            ["AppConf.Smtp"] = "SMTP",
            ["AppConf.Language"] = "语言/Language",

            ["AppConf.TestSmtp"] = "测试 SMTP",
            ["AppConf.Cancel"] = "取消",
            ["AppConf.Save"] = "保存",

            ["Section.ImportTools"] = "导入工具",
            ["Section.SelectUpstream"] = "选择上游工具",
            ["Section.SelectEncoder"] = "选择编码器",
            ["Section.SelectAnalytics"] = "选择分析工具与依赖",
            ["Section.ImportSource"] = "导入或创建源文件",
            ["Section.AnalysisResults"] = "分析结果",
            ["Section.EncodingConfigs"] = "编码配置",
            ["Section.StartEncoding"] = "开始编码",

            ["AppConfModal.Title"] = "1cenc 设置",
            ["AppConfModal.Header"] = "设置",

            ["ImportButton"] = "导入",

            ["ItemCard.Separator"] = "：",

            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "确认",
            ["ConfirmDialog.WarningPrefix"] = "警告：",
            ["ConfirmDialog.ErrorPrefix"] = "错误：",
            ["ConfirmDialog.DebugPrefix"] = "调试：",

            ["ConfirmProvider.SuspiciousImportTitle"] = "导入 {0} 存在可疑",
            ["ConfirmProvider.ProceedToRun"] = "继续运行 {0} 以获取其版本？",
            ["ConfirmProvider.WrongTool"] = "将 {0} 导入为 {1}？",

            ["Checklist.Tools.Upstream"] = "至少一个上游程序可用",
            ["Checklist.Tools.Downstream"] = "至少一个下游程序可用",
            ["Checklist.Tools.Analysis"] = "至少一个分析程序可用",

            ["Checklist.Source1.Metadata"] = "元数据与 SEI 数据可读",
            ["Checklist.Source1.Progressive"] = "逐行扫描视频帧/非隔行（SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth"] = "位深小于 12（8 或 10，SVT-AV1 要求）",

            ["Checklist.Source2.Framerate"] = "帧率恒定/非可变帧率",
            ["Checklist.Source2.AspectRatio"] = "方形像素宽高比 / 1:1 sar",
            ["Checklist.Source2.ColorMatrix"] = "色彩矩阵元数据正常",
            ["Checklist.Source2.TransferChars"] = "传输特性元数据正常",
            ["Checklist.Source2.ColorPrimaries"] = "原色元数据正常",
            ["Checklist.Source2.ChromaSubsampling"] = "无色度采样或为 \u2190/\u2196（SVT-AV1 要求）",

            ["Checklist.Enc1.OffGrid"] = "未使用电池供电/离网",
            ["Checklist.Enc1.RAM"] = "内存充足",
            ["Checklist.Enc1.DiskSpace"] = "磁盘空间充足",

            ["Checklist.Enc2.OSFilename"] = "输出文件名对操作系统有效",
            ["Checklist.Enc2.FTPFilename"] = "输出文件名可能对 FTP 有效（伪 UTF-8）",
            ["Checklist.Enc2.WritePermission"] = "输出文件夹有写入权限",
            ["Checklist.Enc2.Overwrite"] = "输出不会覆盖现有文件",

            ["Checklist.Best1.SlowDisk"] = "避免慢速磁盘连接（USB2、蓝牙等）",
            ["Checklist.Best1.DiskThrashing"] = "避免磁盘抖动（同硬盘读写）",
            ["Checklist.Best1.BiosDriver"] = "使用最新的 BIOS、芯片组驱动与硬盘固件",
            ["Checklist.Best1.Temperature"] = "温度：SSD、RAM 低于 75\u00B0C，HDD 低于 55\u00B0C",
            ["Checklist.Best1.SMR"] = "不写入 SMR 硬盘",

            ["Checklist.Best2.EncoderVersion"] = "使用最新的编码器版本",
            ["Checklist.Best2.FAT32"] = "不写入 FAT32 分区",
            ["Checklist.Best2.DiskCompression"] = "输出文件夹禁用文件系统磁盘压缩",

            ["Setting.General.NotOffGrid"] = "未使用电池供电/离网",
            ["Setting.General.SufficientRAM"] = "内存充足",
            ["Setting.General.SufficientDisk"] = "磁盘空间充足",
            ["Setting.General.OSFilename"] = "输出文件名对操作系统有效",
            ["Setting.General.FTPFilename"] = "输出文件名可能对 FTP 有效（伪 UTF-8）",
            ["Setting.General.WritePermission"] = "输出文件夹有写入权限",
            ["Setting.General.NotOverwrite"] = "输出不会覆盖现有文件",

            ["Setting.Overwrite.LongPressDivisor"] = "长按兆字节除数",
            ["Setting.Overwrite.MinLongPress"] = "最小长按持续时间（毫秒）",
            ["Setting.Overwrite.MaxLongPress"] = "最大长按持续时间（毫秒）",

            ["Setting.Smtp.ServerUrl"] = "服务器地址",
            ["Setting.Smtp.Port"] = "端口",
            ["Setting.Smtp.UseSSL"] = "使用 SSL",
            ["Setting.Smtp.Username"] = "用户名",
            ["Setting.Smtp.Password"] = "密码",
            ["Setting.Smtp.FromEmail"] = "发件邮箱地址",
            ["Setting.Smtp.ToEmail"] = "收件邮箱地址",
            ["Setting.Smtp.NotifySuccess"] = "成功时通知",
            ["Setting.Smtp.NotifyFailure"] = "失败时通知",
            ["Setting.Smtp.NotifyAFK"] = "离开时通知",
            ["Setting.Smtp.SuccessThreshold"] = "成功通知阈值（分钟）",
            ["Setting.Smtp.FailureThreshold"] = "失败通知阈值（分钟）",
            ["Setting.Smtp.AFKThreshold"] = "离开通知阈值（分钟）",

            ["Setting.Language.Select"] = "选择语言",
        },
        ["zh-tw"] = new()
        {
            ["Cards.ToolsImport"] = "匯入工具：",
            ["Cards.SourceValidation"] = "來源影片驗證",
            ["Cards.SourceSevere"] = "嚴重（不相容/損壞）",
            ["Cards.SourceModerate"] = "中等（影響品質）",
            ["Cards.EncPrerequisites"] = "編碼前提條件",
            ["Cards.EncHardware"] = "硬體",
            ["Cards.EncSoftware"] = "軟體",
            ["Cards.BestPractices"] = "最佳實踐",
            ["Cards.BestHardware"] = "硬體（自查）",
            ["Cards.BestSoftware"] = "軟體（自查）",

            ["Buttons.UsageAndCompliance"] = "使用與合規",
            ["Buttons.Settings"] = "\u2699\uFE0F 設定",
            ["Buttons.ReEvaluate"] = "重新評估",
            ["Buttons.RunSample"] = "運行範例",
            ["Buttons.StartEncode"] = "開始編碼",

            ["AppConf.General"] = "一般：在以下情況禁用「開始編碼」...",
            ["AppConf.Overwrite"] = "覆寫處理",
            ["AppConf.Smtp"] = "SMTP",
            ["AppConf.Language"] = "語言/Language",

            ["AppConf.TestSmtp"] = "測試 SMTP",
            ["AppConf.Cancel"] = "取消",
            ["AppConf.Save"] = "儲存",

            ["Section.ImportTools"] = "匯入工具",
            ["Section.SelectUpstream"] = "選擇上游工具",
            ["Section.SelectEncoder"] = "選擇編碼器",
            ["Section.SelectAnalytics"] = "選擇分析工具與依賴",
            ["Section.ImportSource"] = "匯入或建立來源檔案",
            ["Section.AnalysisResults"] = "分析結果",
            ["Section.EncodingConfigs"] = "編碼設定",
            ["Section.StartEncoding"] = "開始編碼",

            ["AppConfModal.Title"] = "1cenc 設定",
            ["AppConfModal.Header"] = "設定",

            ["ImportButton"] = "匯入",

            ["ItemCard.Separator"] = "：",

            ["ConfirmDialog.Cancel"] = "取消",
            ["ConfirmDialog.Confirm"] = "確認",
            ["ConfirmDialog.WarningPrefix"] = "警告：",
            ["ConfirmDialog.ErrorPrefix"] = "錯誤：",
            ["ConfirmDialog.DebugPrefix"] = "偵錯：",

            ["ConfirmProvider.SuspiciousImportTitle"] = "匯入 {0} 存在可疑",
            ["ConfirmProvider.ProceedToRun"] = "繼續執行 {0} 以取得其版本？",
            ["ConfirmProvider.WrongTool"] = "將 {0} 匯入為 {1}？",

            ["Checklist.Tools.Upstream"] = "至少一個上游程式可用",
            ["Checklist.Tools.Downstream"] = "至少一個下游程式可用",
            ["Checklist.Tools.Analysis"] = "至少一個分析程式可用",

            ["Checklist.Source1.Metadata"] = "元資料與 SEI 資料可讀",
            ["Checklist.Source1.Progressive"] = "逐行掃描視訊幀/非交錯（SVT-AV1 要求）",
            ["Checklist.Source1.BitDepth"] = "位元深度小於 12（8 或 10，SVT-AV1 要求）",

            ["Checklist.Source2.Framerate"] = "幀率恆定/非可變幀率",
            ["Checklist.Source2.AspectRatio"] = "方形像素寬高比 / 1:1 sar",
            ["Checklist.Source2.ColorMatrix"] = "色彩矩陣元資料正常",
            ["Checklist.Source2.TransferChars"] = "傳輸特性元資料正常",
            ["Checklist.Source2.ColorPrimaries"] = "原色元資料正常",
            ["Checklist.Source2.ChromaSubsampling"] = "無色度抽樣或為 \u2190/\u2196（SVT-AV1 要求）",

            ["Checklist.Enc1.OffGrid"] = "未使用電池供電/離網",
            ["Checklist.Enc1.RAM"] = "記憶體充足",
            ["Checklist.Enc1.DiskSpace"] = "磁碟空間充足",

            ["Checklist.Enc2.OSFilename"] = "輸出檔名對作業系統有效",
            ["Checklist.Enc2.FTPFilename"] = "輸出檔名可能對 FTP 有效（偽 UTF-8）",
            ["Checklist.Enc2.WritePermission"] = "輸出資料夾有寫入權限",
            ["Checklist.Enc2.Overwrite"] = "輸出不會覆寫現有檔案",

            ["Checklist.Best1.SlowDisk"] = "避免慢速磁碟連接（USB2、藍牙等）",
            ["Checklist.Best1.DiskThrashing"] = "避免磁碟顫動（同硬碟讀寫）",
            ["Checklist.Best1.BiosDriver"] = "使用最新的 BIOS、晶片組驅動與硬碟韌體",
            ["Checklist.Best1.Temperature"] = "溫度：SSD、RAM 低於 75\u00B0C，HDD 低於 55\u00B0C",
            ["Checklist.Best1.SMR"] = "不寫入 SMR 硬碟",

            ["Checklist.Best2.EncoderVersion"] = "使用最新的編碼器版本",
            ["Checklist.Best2.FAT32"] = "不寫入 FAT32 分割區",
            ["Checklist.Best2.DiskCompression"] = "輸出資料夾停用檔案系統磁碟壓縮",

            ["Setting.General.NotOffGrid"] = "未使用電池供電/離網",
            ["Setting.General.SufficientRAM"] = "記憶體充足",
            ["Setting.General.SufficientDisk"] = "磁碟空間充足",
            ["Setting.General.OSFilename"] = "輸出檔名對作業系統有效",
            ["Setting.General.FTPFilename"] = "輸出檔名可能對 FTP 有效（偽 UTF-8）",
            ["Setting.General.WritePermission"] = "輸出資料夾有寫入權限",
            ["Setting.General.NotOverwrite"] = "輸出不會覆寫現有檔案",

            ["Setting.Overwrite.LongPressDivisor"] = "長按百萬位元組除數",
            ["Setting.Overwrite.MinLongPress"] = "最小長按持續時間（毫秒）",
            ["Setting.Overwrite.MaxLongPress"] = "最大長按持續時間（毫秒）",

            ["Setting.Smtp.ServerUrl"] = "伺服器位址",
            ["Setting.Smtp.Port"] = "連接埠",
            ["Setting.Smtp.UseSSL"] = "使用 SSL",
            ["Setting.Smtp.Username"] = "使用者名稱",
            ["Setting.Smtp.Password"] = "密碼",
            ["Setting.Smtp.FromEmail"] = "寄件信箱位址",
            ["Setting.Smtp.ToEmail"] = "收件信箱位址",
            ["Setting.Smtp.NotifySuccess"] = "成功時通知",
            ["Setting.Smtp.NotifyFailure"] = "失敗時通知",
            ["Setting.Smtp.NotifyAFK"] = "離開時通知",
            ["Setting.Smtp.SuccessThreshold"] = "成功通知閾值（分鐘）",
            ["Setting.Smtp.FailureThreshold"] = "失敗通知閾值（分鐘）",
            ["Setting.Smtp.AFKThreshold"] = "離開通知閾值（分鐘）",

            ["Setting.Language.Select"] = "選擇語言",
        }
    };

    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public UILangProviderM(string languageCode)
    {
        _d = Data.TryGetValue(languageCode, out var lang) ? lang : Data["en"];
        Current = this;
    }
}
