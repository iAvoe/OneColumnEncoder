namespace OneColumnEncoder.Models;

public class EncodingMonitorModalLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WindowTitle"] = "1cenc Encoding Monitor",
            ["WindowTitleSampleMode"] = "1cenc Encoding Monitor (Sample mode)",
            ["ProgressTitle"] = "Progress",
            ["MemoryTitle"] = "Physical page monitor (32x16)",
            ["DistributionTitle"] = "Distribution",
            ["BlockDetailsTitle"] = "Block details (hover to inspect)",
            ["LogTitle"] = "Upstream | downstream stdout",
            ["StderrTitle"] = "Upstream | downstream stderr",
            ["DragLogReportHint"] = "Drag the window to resize the log area; drag the log divider to adjust width",
            ["CurrentSizeLabel"] = "Current size / GB",
            ["EstimatedSizeLabel"] = "Estimated total / GB",
            ["WrittenFramesLabel"] = "Frames written",
            ["SampleIntervalLabel"] = "Sampling interval",
            ["StartedAtLabel"] = "Started at hh:mm:ss",
            ["ElapsedLabel"] = "Elapsed hh:mm:ss",
            ["RemainingLabel"] = "Remaining hh:mm:ss",
            ["CompleteAtLabel"] = "Complete at hh:mm:ss",
            ["EncoderFileLabel"] = "Encoder file name",
            ["RateControlLabel"] = "ABR Mbps or CRF value",
            ["ArgsLabel"] = "Other preset name",
            ["SmallNoteText"] = "This program does not support progress quantization; interrupting will discard task progress.",
            ["DistributionUpstreamLabel"] = "Upstream program",
            ["DistributionDownstreamLabel"] = "Downstream program (encoder)",
            ["DistributionOtherLabel"] = "Other programs",
            ["DistributionCacheLabel"] = "System cache",
            ["BlockDetailPosLabel"] = "Index, row, col, size",
            ["BlockDetailSegmentLabel"] = "Segment",
            ["BlockDetailHeatLabel"] = "Heat",
            ["SampleIntervalTickLabels"] = "0 (RT)|60S|120S|180S|240S",
            ["ContinueMonitoringText"] = "Continue monitoring",
            ["FreezeContinueText"] = "Freeze / Continue",
            ["ResetUsageText"] = "Reset usage values",
            ["SaveUpstreamStdoutText"] = "Save upstream stdout",
            ["SaveDownstreamStdoutText"] = "Save downstream stdout",
            ["CopyUpstreamStdoutText"] = "Copy upstream stdout",
            ["CopyDownstreamStdoutText"] = "Copy downstream stdout",
            ["RotateLogFontSizeText"] = "Rotate log font size",
            ["SaveUpstreamStderrText"] = "Save upstream stderr",
            ["SaveDownstreamStderrText"] = "Save downstream stderr",
            ["CopyUpstreamStderrText"] = "Copy upstream stderr",
            ["CopyDownstreamStderrText"] = "Copy downstream stderr",
            ["OpenOutputDirectoryText"] = "Open output folder",
            ["ViewEncodingCommandText"] = "View encoding command",
            ["InterruptKeepResultText"] = "Interrupt (keep result)",
            ["ForceQuitText"] = "Force quit",
            ["CloseAfterDoneText"] = "Close window (enabled after completion)",
            ["EncodingCommandTitle"] = "Encoding Command",
            ["PhysicalMemoryTopText"] = "Physical memory",
            ["PhysicalMemoryMainText"] = "XX.X GB",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
            ["CommittedMemoryTopText"] = "Committed",
            ["CommittedMemoryMainText"] = "XX.X GB",
            ["CommittedMemoryBottomText"] = "Limit XX GB",
            ["WorkingSetPeakTopText"] = "Working set peak",
            ["WorkingSetPeakMainText"] = "XX.X GB",
            ["WorkingSetPeakBottomText"] = "Current XX GB",
            ["PageFileTopText"] = "Page file",
            ["PageFileMainText"] = "XX.X GB",
            ["PageFileBottomText"] = "Total XX GB",
            ["PageFaultTopText"] = "Page faults",
            ["PageFaultMainText"] = "XX,XXX",
            ["PageFaultBottomText"] = "Hard faults XX",
            ["BandwidthPeakTopText"] = "Bandwidth peak",
            ["BandwidthPeakMainText"] = "XX.X GBps",
            ["BandwidthPeakBottomText"] = "Current XX.X GBps",
            ["MemoryPressureTopText"] = "Memory pressure",
            ["MemoryPressureMediumText"] = "Mid",
            ["MemoryPressureHighText"] = "High",
            ["MemoryPressureBottomText"] = "XXX%",
            ["BlockTooltipFormat"] = "Block {0}",
            ["UpstreamStdoutHeaderFormat"] = "[upstream stdout :: {0} pipe -> {1}]",
            ["DownstreamStdoutHeaderFormat"] = "[downstream stdout :: {0}]",
            ["UpstreamLabel"] = "Upstream",
            ["ExecutableLabel"] = "Executable",
            ["InputLabel"] = "Input",
            ["ArgumentsLabel"] = "Arguments",
            ["EncoderLabel"] = "Encoder",
            ["OutputLabel"] = "Output",
            ["PipeErrorPrefix"] = "Pipe error: ",
            ["ReadyToStartText"] = "Ready to start",
            ["EncodingText"] = "Encoding",
            ["InterruptedText"] = "Interrupted",
            ["FailedText"] = "Encoding failed",
            ["CompletedText"] = "Encoding completed",
            ["ResetUsageStatusText"] = "Usage values reset",
            ["InterruptingText"] = "Interrupting",
            ["ForcedExitText"] = "Force quit",
            ["ModeText"] = "mode",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
            ["LargePagePrivilegeCheckTitle"] = "Large page privilege check",
            ["NoPrivilegeDiagnosticText"] = "PrivilegeCheckH.HasLockMemoryPrivilege returned without a diagnostic message.",
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "1cenc 编码监视器",
            ["WindowTitleSampleMode"] = "1cenc 编码监视器（取样模式）",
            ["ProgressTitle"] = "进度",
            ["MemoryTitle"] = "物理页监视器（32x16）",
            ["DistributionTitle"] = "区段分布",
            ["BlockDetailsTitle"] = "单块详情（光标悬停时显示）",
            ["LogTitle"] = "上游 | 下游程序 stdout",
            ["StderrTitle"] = "上游 | 下游 stderr",
            ["DragLogReportHint"] = "拖拽窗口以调整日志显示面积；拖拽日志分界线以调整宽度",
            ["CurrentSizeLabel"] = "当前大小/GB",
            ["EstimatedSizeLabel"] = "预计总大小/GB",
            ["WrittenFramesLabel"] = "已写入帧数",
            ["SampleIntervalLabel"] = "采样间隔",
            ["StartedAtLabel"] = "开始时间 hh:mm:ss",
            ["ElapsedLabel"] = "已用时 hh:mm:ss",
            ["RemainingLabel"] = "预计剩余 hh:mm:ss",
            ["CompleteAtLabel"] = "预计完成 hh:mm:ss",
            ["EncoderFileLabel"] = "编码器文件名",
            ["RateControlLabel"] = "ABR Mbps 或 CRF 值",
            ["ArgsLabel"] = "其他参数预设名",
            ["SmallNoteText"] = "本程序不支持进度量化；中断将丢弃任务进度。",
            ["DistributionUpstreamLabel"] = "上游程序",
            ["DistributionDownstreamLabel"] = "下游程序（编码器）",
            ["DistributionOtherLabel"] = "其它程序",
            ["DistributionCacheLabel"] = "系统缓存",
            ["BlockDetailPosLabel"] = "号，行，列，大小",
            ["BlockDetailSegmentLabel"] = "隶属区段",
            ["BlockDetailHeatLabel"] = "热度",
            ["SampleIntervalTickLabels"] = "0（实时）|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "继续监测",
            ["FreezeContinueText"] = "冻结 / 继续监测",
            ["ResetUsageText"] = "重置占用值",
            ["SaveUpstreamStdoutText"] = "保存上游 stdout",
            ["SaveDownstreamStdoutText"] = "保存下游 stdout",
            ["CopyUpstreamStdoutText"] = "复制上游 stdout",
            ["CopyDownstreamStdoutText"] = "复制下游 stdout",
            ["RotateLogFontSizeText"] = "轮换日志字号",
            ["SaveUpstreamStderrText"] = "保存上游 stderr",
            ["SaveDownstreamStderrText"] = "保存下游 stderr",
            ["CopyUpstreamStderrText"] = "复制上游 stderr",
            ["CopyDownstreamStderrText"] = "复制下游 stderr",
            ["OpenOutputDirectoryText"] = "打开输出目录",
            ["ViewEncodingCommandText"] = "查看编码参数",
            ["InterruptKeepResultText"] = "中断（保留结果）",
            ["ForceQuitText"] = "强制退出",
            ["CloseAfterDoneText"] = "关闭窗口（完成后启用）",
            ["EncodingCommandTitle"] = "编码命令",
            ["PhysicalMemoryTopText"] = "物理内存",
            ["PhysicalMemoryMainText"] = "XX.X GB",
            ["PhysicalMemoryBottomText"] = "共 XX GB",
            ["CommittedMemoryTopText"] = "已提交",
            ["CommittedMemoryMainText"] = "XX.X GB",
            ["CommittedMemoryBottomText"] = "限额 XX GB",
            ["WorkingSetPeakTopText"] = "工作集峰值",
            ["WorkingSetPeakMainText"] = "XX.X GB",
            ["WorkingSetPeakBottomText"] = "当前 XX GB",
            ["PageFileTopText"] = "页文件",
            ["PageFileMainText"] = "XX.X GB",
            ["PageFileBottomText"] = "总计 XX GB",
            ["PageFaultTopText"] = "页错误",
            ["PageFaultMainText"] = "XX,XXX",
            ["PageFaultBottomText"] = "硬错误 XX",
            ["BandwidthPeakTopText"] = "带宽峰值",
            ["BandwidthPeakMainText"] = "XX.X GBps",
            ["BandwidthPeakBottomText"] = "当前 XX.X GBps",
            ["MemoryPressureTopText"] = "内存压力",
            ["MemoryPressureMediumText"] = "中",
            ["MemoryPressureHighText"] = "高",
            ["MemoryPressureBottomText"] = "XXX%",
            ["BlockTooltipFormat"] = "区块 {0}",
            ["UpstreamStdoutHeaderFormat"] = "[上游 stdout :: {0} 管道 → {1}]",
            ["DownstreamStdoutHeaderFormat"] = "[下游 stdout :: {0}]",
            ["UpstreamLabel"] = "上游",
            ["ExecutableLabel"] = "可执行文件",
            ["InputLabel"] = "输入",
            ["ArgumentsLabel"] = "参数",
            ["EncoderLabel"] = "编码器",
            ["OutputLabel"] = "输出",
            ["PipeErrorPrefix"] = "管道错误：",
            ["ReadyToStartText"] = "准备启动",
            ["EncodingText"] = "正在压制",
            ["InterruptedText"] = "已中断",
            ["FailedText"] = "压制失败",
            ["CompletedText"] = "压制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingText"] = "正在中断",
            ["ForcedExitText"] = "已强制退出",
            ["ModeText"] = "模式",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
            ["LargePagePrivilegeCheckTitle"] = "大页面权限检查",
            ["NoPrivilegeDiagnosticText"] = "PrivilegeCheckH.HasLockMemoryPrivilege 返回时没有给出诊断信息。",
        },
        ["zh-tw"] = new()
        {
            ["WindowTitle"] = "1cenc 編碼監視器",
            ["WindowTitleSampleMode"] = "1cenc 編碼監視器（取樣模式）",
            ["ProgressTitle"] = "進度",
            ["MemoryTitle"] = "實體頁監視器（32x16）",
            ["DistributionTitle"] = "區段分佈",
            ["BlockDetailsTitle"] = "單塊詳情（游標停留時顯示）",
            ["LogTitle"] = "上游 | 下游程序 stdout",
            ["StderrTitle"] = "上游 | 下游 stderr",
            ["DragLogReportHint"] = "拖曳視窗以調整日誌顯示面積；拖曳日誌分界線以調整寬度",
            ["CurrentSizeLabel"] = "目前大小/GB",
            ["EstimatedSizeLabel"] = "預計總大小/GB",
            ["WrittenFramesLabel"] = "已寫入幀數",
            ["SampleIntervalLabel"] = "取樣間隔",
            ["StartedAtLabel"] = "開始時間 hh:mm:ss",
            ["ElapsedLabel"] = "已用時 hh:mm:ss",
            ["RemainingLabel"] = "預計剩餘 hh:mm:ss",
            ["CompleteAtLabel"] = "預計完成 hh:mm:ss",
            ["EncoderFileLabel"] = "編碼器檔名",
            ["RateControlLabel"] = "ABR Mbps 或 CRF 值",
            ["ArgsLabel"] = "其他參數預設名",
            ["SmallNoteText"] = "本程式不支援進度量化；中斷將丟棄任務進度。",
            ["DistributionUpstreamLabel"] = "上游程式",
            ["DistributionDownstreamLabel"] = "下游程式（編碼器）",
            ["DistributionOtherLabel"] = "其他程式",
            ["DistributionCacheLabel"] = "系統快取",
            ["BlockDetailPosLabel"] = "號、行、列、大小",
            ["BlockDetailSegmentLabel"] = "隸屬區段",
            ["BlockDetailHeatLabel"] = "熱度",
            ["SampleIntervalTickLabels"] = "0（即時）|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "繼續監測",
            ["FreezeContinueText"] = "凍結 / 繼續監測",
            ["ResetUsageText"] = "重置占用值",
            ["SaveUpstreamStdoutText"] = "儲存上游 stdout",
            ["SaveDownstreamStdoutText"] = "儲存下游 stdout",
            ["CopyUpstreamStdoutText"] = "複製上游 stdout",
            ["CopyDownstreamStdoutText"] = "複製下游 stdout",
            ["RotateLogFontSizeText"] = "輪換日誌字型大小",
            ["SaveUpstreamStderrText"] = "儲存上游 stderr",
            ["SaveDownstreamStderrText"] = "儲存下游 stderr",
            ["CopyUpstreamStderrText"] = "複製上游 stderr",
            ["CopyDownstreamStderrText"] = "複製下游 stderr",
            ["OpenOutputDirectoryText"] = "開啟輸出資料夾",
            ["ViewEncodingCommandText"] = "檢視編碼參數",
            ["InterruptKeepResultText"] = "中斷（保留結果）",
            ["ForceQuitText"] = "強制退出",
            ["CloseAfterDoneText"] = "關閉視窗（完成後啟用）",
            ["EncodingCommandTitle"] = "編碼命令",
            ["PhysicalMemoryTopText"] = "實體記憶體",
            ["PhysicalMemoryMainText"] = "XX.X GB",
            ["PhysicalMemoryBottomText"] = "共 XX GB",
            ["CommittedMemoryTopText"] = "已提交",
            ["CommittedMemoryMainText"] = "XX.X GB",
            ["CommittedMemoryBottomText"] = "上限 XX GB",
            ["WorkingSetPeakTopText"] = "工作集峰值",
            ["WorkingSetPeakMainText"] = "XX.X GB",
            ["WorkingSetPeakBottomText"] = "目前 XX GB",
            ["PageFileTopText"] = "分頁檔",
            ["PageFileMainText"] = "XX.X GB",
            ["PageFileBottomText"] = "總計 XX GB",
            ["PageFaultTopText"] = "分頁錯誤",
            ["PageFaultMainText"] = "XX,XXX",
            ["PageFaultBottomText"] = "硬錯誤 XX",
            ["BandwidthPeakTopText"] = "頻寬峰值",
            ["BandwidthPeakMainText"] = "XX.X GBps",
            ["BandwidthPeakBottomText"] = "目前 XX.X GBps",
            ["MemoryPressureTopText"] = "記憶體壓力",
            ["MemoryPressureMediumText"] = "中",
            ["MemoryPressureHighText"] = "高",
            ["MemoryPressureBottomText"] = "XXX%",
            ["BlockTooltipFormat"] = "區塊 {0}",
            ["UpstreamStdoutHeaderFormat"] = "[上游 stdout :: {0} 管道 → {1}]",
            ["DownstreamStdoutHeaderFormat"] = "[下游 stdout :: {0}]",
            ["UpstreamLabel"] = "上游",
            ["ExecutableLabel"] = "可執行檔",
            ["InputLabel"] = "輸入",
            ["ArgumentsLabel"] = "參數",
            ["EncoderLabel"] = "編碼器",
            ["OutputLabel"] = "輸出",
            ["PipeErrorPrefix"] = "管道錯誤：",
            ["ReadyToStartText"] = "準備啟動",
            ["EncodingText"] = "正在壓制",
            ["InterruptedText"] = "已中斷",
            ["FailedText"] = "壓制失敗",
            ["CompletedText"] = "壓制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingText"] = "正在中斷",
            ["ForcedExitText"] = "已強制退出",
            ["ModeText"] = "模式",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
            ["LargePagePrivilegeCheckTitle"] = "大頁面權限檢查",
            ["NoPrivilegeDiagnosticText"] = "PrivilegeCheckH.HasLockMemoryPrivilege 傳回時沒有提供診斷資訊。",
        }
    };

    public string WindowTitle { get; }
    public string WindowTitleSampleMode { get; }
    public string ProgressTitle { get; }
    public string MemoryTitle { get; }
    public string DistributionTitle { get; }
    public string BlockDetailsTitle { get; }
    public string LogTitle { get; }
    public string StderrTitle { get; }
    public string DragLogReportHint { get; }
    public string CurrentSizeLabel { get; }
    public string EstimatedSizeLabel { get; }
    public string WrittenFramesLabel { get; }
    public string SampleIntervalLabel { get; }
    public string StartedAtLabel { get; }
    public string ElapsedLabel { get; }
    public string RemainingLabel { get; }
    public string CompleteAtLabel { get; }
    public string EncoderFileLabel { get; }
    public string RateControlLabel { get; }
    public string ArgsLabel { get; }
    public string SmallNoteText { get; }
    public string DistributionUpstreamLabel { get; }
    public string DistributionDownstreamLabel { get; }
    public string DistributionOtherLabel { get; }
    public string DistributionCacheLabel { get; }
    public string BlockDetailPosLabel { get; }
    public string BlockDetailSegmentLabel { get; }
    public string BlockDetailHeatLabel { get; }
    public string[] SampleIntervalTickLabels { get; }
    public string ContinueMonitoringText { get; }
    public string FreezeContinueText { get; }
    public string ResetUsageText { get; }
    public string SaveUpstreamStdoutText { get; }
    public string SaveDownstreamStdoutText { get; }
    public string CopyUpstreamStdoutText { get; }
    public string CopyDownstreamStdoutText { get; }
    public string RotateLogFontSizeText { get; }
    public string SaveUpstreamStderrText { get; }
    public string SaveDownstreamStderrText { get; }
    public string CopyUpstreamStderrText { get; }
    public string CopyDownstreamStderrText { get; }
    public string OpenOutputDirectoryText { get; }
    public string ViewEncodingCommandText { get; }
    public string InterruptKeepResultText { get; }
    public string ForceQuitText { get; }
    public string CloseAfterDoneText { get; }
    public string EncodingCommandTitle { get; }
    public string PhysicalMemoryTopText { get; }
    public string PhysicalMemoryMainText { get; }
    public string PhysicalMemoryBottomText { get; }
    public string CommittedMemoryTopText { get; }
    public string CommittedMemoryMainText { get; }
    public string CommittedMemoryBottomText { get; }
    public string WorkingSetPeakTopText { get; }
    public string WorkingSetPeakMainText { get; }
    public string WorkingSetPeakBottomText { get; }
    public string PageFileTopText { get; }
    public string PageFileMainText { get; }
    public string PageFileBottomText { get; }
    public string PageFaultTopText { get; }
    public string PageFaultMainText { get; }
    public string PageFaultBottomText { get; }
    public string BandwidthPeakTopText { get; }
    public string BandwidthPeakMainText { get; }
    public string BandwidthPeakBottomText { get; }
    public string MemoryPressureTopText { get; }
    public string MemoryPressureMediumText { get; }
    public string MemoryPressureHighText { get; }
    public string MemoryPressureBottomText { get; }
    public string BlockTooltipFormat { get; }
    public string UpstreamStdoutHeaderFormat { get; }
    public string DownstreamStdoutHeaderFormat { get; }
    public string UpstreamLabel { get; }
    public string ExecutableLabel { get; }
    public string InputLabel { get; }
    public string ArgumentsLabel { get; }
    public string EncoderLabel { get; }
    public string OutputLabel { get; }
    public string PipeErrorPrefix { get; }
    public string ReadyToStartText { get; }
    public string EncodingText { get; }
    public string InterruptedText { get; }
    public string FailedText { get; }
    public string CompletedText { get; }
    public string ResetUsageStatusText { get; }
    public string InterruptingText { get; }
    public string ForcedExitStatusText { get; }
    public string ModeText { get; }
    public string NotAvailableText { get; }
    public string ABRText { get; }
    public string CRFText { get; }
    public string LargePagePrivilegeCheckTitle { get; }
    public string NoPrivilegeDiagnosticText { get; }

    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public EncodingMonitorModalLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        WindowTitle = _d["WindowTitle"];
        WindowTitleSampleMode = _d["WindowTitleSampleMode"];
        ProgressTitle = _d["ProgressTitle"];
        MemoryTitle = _d["MemoryTitle"];
        DistributionTitle = _d["DistributionTitle"];
        BlockDetailsTitle = _d["BlockDetailsTitle"];
        LogTitle = _d["LogTitle"];
        StderrTitle = _d["StderrTitle"];
        DragLogReportHint = _d["DragLogReportHint"];
        CurrentSizeLabel = _d["CurrentSizeLabel"];
        EstimatedSizeLabel = _d["EstimatedSizeLabel"];
        WrittenFramesLabel = _d["WrittenFramesLabel"];
        SampleIntervalLabel = _d["SampleIntervalLabel"];
        StartedAtLabel = _d["StartedAtLabel"];
        ElapsedLabel = _d["ElapsedLabel"];
        RemainingLabel = _d["RemainingLabel"];
        CompleteAtLabel = _d["CompleteAtLabel"];
        EncoderFileLabel = _d["EncoderFileLabel"];
        RateControlLabel = _d["RateControlLabel"];
        ArgsLabel = _d["ArgsLabel"];
        SmallNoteText = _d["SmallNoteText"];
        DistributionUpstreamLabel = _d["DistributionUpstreamLabel"];
        DistributionDownstreamLabel = _d["DistributionDownstreamLabel"];
        DistributionOtherLabel = _d["DistributionOtherLabel"];
        DistributionCacheLabel = _d["DistributionCacheLabel"];
        BlockDetailPosLabel = _d["BlockDetailPosLabel"];
        BlockDetailSegmentLabel = _d["BlockDetailSegmentLabel"];
        BlockDetailHeatLabel = _d["BlockDetailHeatLabel"];
        SampleIntervalTickLabels = _d["SampleIntervalTickLabels"].Split('|');
        ContinueMonitoringText = _d["ContinueMonitoringText"];
        FreezeContinueText = _d["FreezeContinueText"];
        ResetUsageText = _d["ResetUsageText"];
        SaveUpstreamStdoutText = _d["SaveUpstreamStdoutText"];
        SaveDownstreamStdoutText = _d["SaveDownstreamStdoutText"];
        CopyUpstreamStdoutText = _d["CopyUpstreamStdoutText"];
        CopyDownstreamStdoutText = _d["CopyDownstreamStdoutText"];
        RotateLogFontSizeText = _d["RotateLogFontSizeText"];
        SaveUpstreamStderrText = _d["SaveUpstreamStderrText"];
        SaveDownstreamStderrText = _d["SaveDownstreamStderrText"];
        CopyUpstreamStderrText = _d["CopyUpstreamStderrText"];
        CopyDownstreamStderrText = _d["CopyDownstreamStderrText"];
        OpenOutputDirectoryText = _d["OpenOutputDirectoryText"];
        ViewEncodingCommandText = _d["ViewEncodingCommandText"];
        InterruptKeepResultText = _d["InterruptKeepResultText"];
        ForceQuitText = _d["ForceQuitText"];
        CloseAfterDoneText = _d["CloseAfterDoneText"];
        EncodingCommandTitle = _d["EncodingCommandTitle"];
        PhysicalMemoryTopText = _d["PhysicalMemoryTopText"];
        PhysicalMemoryMainText = _d["PhysicalMemoryMainText"];
        PhysicalMemoryBottomText = _d["PhysicalMemoryBottomText"];
        CommittedMemoryTopText = _d["CommittedMemoryTopText"];
        CommittedMemoryMainText = _d["CommittedMemoryMainText"];
        CommittedMemoryBottomText = _d["CommittedMemoryBottomText"];
        WorkingSetPeakTopText = _d["WorkingSetPeakTopText"];
        WorkingSetPeakMainText = _d["WorkingSetPeakMainText"];
        WorkingSetPeakBottomText = _d["WorkingSetPeakBottomText"];
        PageFileTopText = _d["PageFileTopText"];
        PageFileMainText = _d["PageFileMainText"];
        PageFileBottomText = _d["PageFileBottomText"];
        PageFaultTopText = _d["PageFaultTopText"];
        PageFaultMainText = _d["PageFaultMainText"];
        PageFaultBottomText = _d["PageFaultBottomText"];
        BandwidthPeakTopText = _d["BandwidthPeakTopText"];
        BandwidthPeakMainText = _d["BandwidthPeakMainText"];
        BandwidthPeakBottomText = _d["BandwidthPeakBottomText"];
        MemoryPressureTopText = _d["MemoryPressureTopText"];
        MemoryPressureMediumText = _d["MemoryPressureMediumText"];
        MemoryPressureHighText = _d["MemoryPressureHighText"];
        MemoryPressureBottomText = _d["MemoryPressureBottomText"];
        BlockTooltipFormat = _d["BlockTooltipFormat"];
        UpstreamStdoutHeaderFormat = _d["UpstreamStdoutHeaderFormat"];
        DownstreamStdoutHeaderFormat = _d["DownstreamStdoutHeaderFormat"];
        UpstreamLabel = _d["UpstreamLabel"];
        ExecutableLabel = _d["ExecutableLabel"];
        InputLabel = _d["InputLabel"];
        ArgumentsLabel = _d["ArgumentsLabel"];
        EncoderLabel = _d["EncoderLabel"];
        OutputLabel = _d["OutputLabel"];
        PipeErrorPrefix = _d["PipeErrorPrefix"];
        ReadyToStartText = _d["ReadyToStartText"];
        EncodingText = _d["EncodingText"];
        InterruptedText = _d["InterruptedText"];
        FailedText = _d["FailedText"];
        CompletedText = _d["CompletedText"];
        ResetUsageStatusText = _d["ResetUsageStatusText"];
        InterruptingText = _d["InterruptingText"];
        ForcedExitStatusText = _d["ForcedExitText"];
        ModeText = _d["ModeText"];
        NotAvailableText = _d["NotAvailableText"];
        ABRText = _d["ABRText"];
        CRFText = _d["CRFText"];
        LargePagePrivilegeCheckTitle = _d["LargePagePrivilegeCheckTitle"];
        NoPrivilegeDiagnosticText = _d["NoPrivilegeDiagnosticText"];
    }
}
