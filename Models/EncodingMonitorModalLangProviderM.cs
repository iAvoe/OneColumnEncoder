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
            ["ProgressReportTitle"] = "Progress stream",
            ["MemoryTitle"] = "RAM use",

            ["StderrTitle"] = "Process log",
            ["DragLogReportHint"] = "Drag the window to resize the log area; drag the log divider to adjust width",
            ["CurrentSizeLabel"] = "Current size / GB",
            ["EstimatedSizeLabel"] = "Estimated total / GB",
            ["WrittenFramesLabel"] = "Frames written",
            ["SampleIntervalLabel"] = "Sampling interval",
            ["StartedAtLabel"] = "Started At",
            ["ElapsedLabel"] = "Elapsed",
            ["RemainingLabel"] = "Remaining",
            ["CompleteAtLabel"] = "ETA",
            ["ArgsLabel"] = "Other preset name",
            ["SmallNoteText"] = "This program does not support progress quantization; interrupting will discard task progress.",
            ["EnableMuxText"] = "Multiplex after encoding (the 2nd part of commandline, ffmpeg required)",
            ["MuxTimebaseHint"] = "Muxed output inherits timebase from source, so quality metrics can align frames correctly, w/out manually calculating GCD of fractions",
            ["DistributionUpstreamLabel"] = "Upstream program",
            ["DistributionDownstreamLabel"] = "Downstream program",
            ["DistributionCacheLabel"] = "System cache",
            ["DistributionAvailableLabel"] = "Available Space",
            ["MemoryRangeLegendTitle"] = "Range legend",
            ["SampleIntervalTickLabels"] = "0 (RT)|60S|120S|180S|240S",
            ["ContinueMonitoringText"] = "Continue monitoring",
            ["FreezeContinueText"] = "Freeze / Continue",
            ["ResetUsageText"] = "Reset usage values",
            ["RotateLogFontSizeText"] = "Rotate log fontsize",
            ["SaveUpstreamStderrText"] = "Save upstream log",
            ["SaveDownstreamStderrText"] = "Save downstream log",

            ["OpenOutputDirectoryText"] = "Open output folder",
            ["ViewEncodingCommandText"] = "Revisit commandline",
            ["InterruptUpstreamText"] = "Interrupt upstream",
            ["InterruptEncoderText"] = "Interrupt encoder",
            ["CloseAfterDoneText"] = "Close",
            ["EncodingCommandTitle"] = "Encoding Command",
            ["PhysicalMemoryTopText"] = "Physical memory",
            ["PhysicalMemoryBottomText"] = "Total XX GB",
            ["CommittedMemoryTopText"] = "Committed",
            ["CommittedMemoryBottomText"] = "Limit XX GB",
            ["WorkingSetPeakTopText"] = "Working set peak",
            ["WorkingSetPeakBottomText"] = "Current XX GB",
            ["PageFileTopText"] = "Page file",
            ["PageFileBottomText"] = "Total XX GB",
            ["PageFaultTopText"] = "Page faults",
            ["PageFaultBottomText"] = "Hard & soft",
            ["RAMStressTopText"] = "RAM stress",
            ["RAMStressMediumText"] = "Mid",
            ["RAMStressHighText"] = "High",
            ["BlockTooltipFormat"] = "Range block {0}",
            ["PipeErrorPrefix"] = "Pipe error: ",
            ["ReadyToStartText"] = "Ready to start",
            ["EncodingText"] = "Encoding",
            ["MuxingText"] = "Muxing",
            ["InterruptedText"] = "Interrupted",
            ["FailedText"] = "Encoding failed",
            ["CompletedText"] = "Encoding completed",
            ["ResetUsageStatusText"] = "Usage values reset",
            ["InterruptingUpstreamText"] = "Interrupting upstream",
            ["InterruptingEncoderText"] = "Interrupting encoder",
            ["ModeText"] = "mode",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "1cenc 编码监视器",
            ["WindowTitleSampleMode"] = "1cenc 编码监视器（取样模式）",
            ["ProgressTitle"] = "进度",
            ["ProgressReportTitle"] = "进度流",
            ["MemoryTitle"] = "内存占用",

            ["StderrTitle"] = "进程日志",
            ["DragLogReportHint"] = "拖拽窗口以调整日志显示面积；拖拽日志分界线以调整宽度",
            ["CurrentSizeLabel"] = "当前大小/GB",
            ["EstimatedSizeLabel"] = "预计总大小/GB",
            ["WrittenFramesLabel"] = "已写入帧数",
            ["SampleIntervalLabel"] = "采样间隔",
            ["StartedAtLabel"] = "开始时间（24h）",
            ["ElapsedLabel"] = "已用时",
            ["RemainingLabel"] = "预计剩余",
            ["CompleteAtLabel"] = "预计完成（24h）",
            ["ArgsLabel"] = "其他参数预设名",
            ["SmallNoteText"] = "本程序不支持进度量化；中断将丢弃任务进度。",
            ["EnableMuxText"] = "压制完成后封装视频流（先前命令行的第二部分，需导入 ffmpeg）",
            ["MuxTimebaseHint"] = "自动封装会使用与源视频相同的时间基，以便画质指标跑分时正确对齐帧，而无需计算两个分数的最大公约数。",
            ["DistributionUpstreamLabel"] = "上游程序",
            ["DistributionDownstreamLabel"] = "下游程序",
            ["DistributionCacheLabel"] = "系统缓存",
            ["DistributionAvailableLabel"] = "可用空间",
            ["MemoryRangeLegendTitle"] = "范围图例",
            ["SampleIntervalTickLabels"] = "0（实时）|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "继续监测",
            ["FreezeContinueText"] = "冻结 / 继续监测",
            ["ResetUsageText"] = "重置占用值",
            ["RotateLogFontSizeText"] = "轮换日志字号",
            ["SaveUpstreamStderrText"] = "保存上游日志",
            ["SaveDownstreamStderrText"] = "保存下游日志",

            ["OpenOutputDirectoryText"] = "打开输出目录",
            ["ViewEncodingCommandText"] = "查看编码参数",
            ["InterruptUpstreamText"] = "中断上游程序",
            ["InterruptEncoderText"] = "中断编码器",
            ["CloseAfterDoneText"] = "关闭",
            ["EncodingCommandTitle"] = "编码命令",
            ["PhysicalMemoryTopText"] = "物理内存",
            ["PhysicalMemoryBottomText"] = "共 XX GB",
            ["CommittedMemoryTopText"] = "已提交",
            ["CommittedMemoryBottomText"] = "限额 XX GB",
            ["WorkingSetPeakTopText"] = "工作集峰值",
            ["WorkingSetPeakBottomText"] = "当前 XX GB",
            ["PageFileTopText"] = "页文件",
            ["PageFileBottomText"] = "总计 XX GB",
            ["PageFaultTopText"] = "页错误",
            ["PageFaultBottomText"] = "硬+软",
            ["RAMStressTopText"] = "内存压力",
            ["RAMStressMediumText"] = "中",
            ["RAMStressHighText"] = "高",
            ["BlockTooltipFormat"] = "范围块 {0}",
            ["PipeErrorPrefix"] = "管道错误：",
            ["ReadyToStartText"] = "准备启动",
            ["EncodingText"] = "正在压制",
            ["MuxingText"] = "正在封装",
            ["InterruptedText"] = "已中断",
            ["FailedText"] = "压制失败",
            ["CompletedText"] = "压制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingUpstreamText"] = "正在中断上游程序",
            ["InterruptingEncoderText"] = "正在中断编码器",
            ["ModeText"] = "模式",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
        },
        ["zh-tw"] = new()
        {
            ["WindowTitle"] = "1cenc 編碼監視器",
            ["WindowTitleSampleMode"] = "1cenc 編碼監視器（取樣模式）",
            ["ProgressTitle"] = "進度",
            ["ProgressReportTitle"] = "進度流",
            ["MemoryTitle"] = "記憶占用",

            ["StderrTitle"] = "進程日誌",
            ["DragLogReportHint"] = "拖曳視窗以調整日誌顯示面積；拖曳日誌分界線以調整寬度",
            ["CurrentSizeLabel"] = "目前大小/GB",
            ["EstimatedSizeLabel"] = "預計總大小/GB",
            ["WrittenFramesLabel"] = "已寫入幀數",
            ["SampleIntervalLabel"] = "取樣間隔",
            ["StartedAtLabel"] = "開始時間（24h）",
            ["ElapsedLabel"] = "已用時",
            ["RemainingLabel"] = "預計剩餘",
            ["CompleteAtLabel"] = "預計完成（24h）",
            ["ArgsLabel"] = "其他參數預設名",
            ["SmallNoteText"] = "本程式不支援進度量化；中斷將丟棄任務進度。",
            ["EnableMuxText"] = "壓製完成後封裝影片串流（先前命令行的第二部分，需導入 ffmpeg）",
            ["MuxTimebaseHint"] = "自動封裝會使用與源影片相同的時間基，以便畫質指標跑分時正確對齊幀，而無需計算兩個分數的最大公約數。",
            ["DistributionUpstreamLabel"] = "上游程式",
            ["DistributionDownstreamLabel"] = "下游程式",
            ["DistributionCacheLabel"] = "系統快取",
            ["DistributionAvailableLabel"] = "可用空間",
            ["MemoryRangeLegendTitle"] = "範圍圖例",
            ["SampleIntervalTickLabels"] = "0（即時）|60秒|120秒|180秒|240秒",
            ["ContinueMonitoringText"] = "繼續監測",
            ["FreezeContinueText"] = "凍結 / 繼續監測",
            ["ResetUsageText"] = "重置占用值",
            ["RotateLogFontSizeText"] = "輪換日誌字型大小",
            ["SaveUpstreamStderrText"] = "保存上游日誌",
            ["SaveDownstreamStderrText"] = "保存下游日誌",

            ["OpenOutputDirectoryText"] = "開啟輸出資料夾",
            ["ViewEncodingCommandText"] = "檢視編碼參數",
            ["InterruptUpstreamText"] = "中斷上游程式",
            ["InterruptEncoderText"] = "中斷編碼器",
            ["CloseAfterDoneText"] = "關閉",
            ["EncodingCommandTitle"] = "編碼命令",
            ["PhysicalMemoryTopText"] = "實體記憶體",
            ["PhysicalMemoryBottomText"] = "共 XX GB",
            ["CommittedMemoryTopText"] = "已提交",
            ["CommittedMemoryBottomText"] = "上限 XX GB",
            ["WorkingSetPeakTopText"] = "工作集峰值",
            ["WorkingSetPeakBottomText"] = "目前 XX GB",
            ["PageFileTopText"] = "分頁檔",
            ["PageFileBottomText"] = "總計 XX GB",
            ["PageFaultTopText"] = "分頁錯誤",
            ["PageFaultBottomText"] = "硬+軟",
            ["RAMStressTopText"] = "記憶體壓力",
            ["RAMStressMediumText"] = "中",
            ["RAMStressHighText"] = "高",
            ["BlockTooltipFormat"] = "範圍塊 {0}",
            ["PipeErrorPrefix"] = "管道錯誤：",
            ["ReadyToStartText"] = "準備啟動",
            ["EncodingText"] = "正在壓制",
            ["MuxingText"] = "正在封裝",
            ["InterruptedText"] = "已中斷",
            ["FailedText"] = "壓制失敗",
            ["CompletedText"] = "壓制完成",
            ["ResetUsageStatusText"] = "已重置占用值",
            ["InterruptingUpstreamText"] = "正在中斷上游程式",
            ["InterruptingEncoderText"] = "正在中斷編碼器",
            ["ModeText"] = "模式",
            ["NotAvailableText"] = "N/A",
            ["ABRText"] = "ABR",
            ["CRFText"] = "CRF",
        }
    };

    public string WindowTitle { get; }
    public string WindowTitleSampleMode { get; }
    public string ProgressTitle { get; }
    public string ProgressReportTitle { get; }
    public string MemoryTitle { get; }
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
    public string ArgsLabel { get; }
    public string SmallNoteText { get; }
    public string EnableMuxText { get; }
    public string MuxTimebaseHint { get; }
    public string DistributionUpstreamLabel { get; }
    public string DistributionDownstreamLabel { get; }
    public string DistributionCacheLabel { get; }
    public string DistributionAvailableLabel { get; }
    public string MemoryRangeLegendTitle { get; }
    public string[] SampleIntervalTickLabels { get; }
    public string ContinueMonitoringText { get; }
    public string FreezeContinueText { get; }
    public string ResetUsageText { get; }
    public string RotateLogFontSizeText { get; }
    public string SaveUpstreamStderrText { get; }
    public string SaveDownstreamStderrText { get; }
    public string OpenOutputDirectoryText { get; }
    public string ViewEncodingCommandText { get; }
    public string InterruptUpstreamText { get; }
    public string InterruptEncoderText { get; }
    public string CloseAfterDoneText { get; }
    public string EncodingCommandTitle { get; }
    public string PhysicalMemoryTopText { get; }
    public string PhysicalMemoryBottomText { get; }
    public string CommittedMemoryTopText { get; }
    public string CommittedMemoryBottomText { get; }
    public string WorkingSetPeakTopText { get; }
    public string WorkingSetPeakBottomText { get; }
    public string PageFileTopText { get; }
    public string PageFileBottomText { get; }
    public string PageFaultTopText { get; }
    public string PageFaultBottomText { get; }
    public string RAMStressTopText { get; }
    public string RAMStressMediumText { get; }
    public string RAMStressHighText { get; }
    public string BlockTooltipFormat { get; }
    public string PipeErrorPrefix { get; }
    public string ReadyToStartText { get; }
    public string EncodingText { get; }
    public string MuxingText { get; }
    public string InterruptedText { get; }
    public string FailedText { get; }
    public string CompletedText { get; }
    public string ResetUsageStatusText { get; }
    public string InterruptingUpstreamText { get; }
    public string InterruptingEncoderText { get; }
    public string ModeText { get; }
    public string NotAvailableText { get; }
    public string ABRText { get; }
    public string CRFText { get; }
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
        ProgressReportTitle = _d["ProgressReportTitle"];
        MemoryTitle = _d["MemoryTitle"];
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
        ArgsLabel = _d["ArgsLabel"];
        SmallNoteText = _d["SmallNoteText"];
        EnableMuxText = _d["EnableMuxText"];
        MuxTimebaseHint = _d["MuxTimebaseHint"];
        DistributionUpstreamLabel = _d["DistributionUpstreamLabel"];
        DistributionDownstreamLabel = _d["DistributionDownstreamLabel"];
        DistributionCacheLabel = _d["DistributionCacheLabel"];
        DistributionAvailableLabel = _d["DistributionAvailableLabel"];
        MemoryRangeLegendTitle = _d["MemoryRangeLegendTitle"];
        SampleIntervalTickLabels = _d["SampleIntervalTickLabels"].Split('|');
        ContinueMonitoringText = _d["ContinueMonitoringText"];
        FreezeContinueText = _d["FreezeContinueText"];
        ResetUsageText = _d["ResetUsageText"];
        RotateLogFontSizeText = _d["RotateLogFontSizeText"];
        SaveUpstreamStderrText = _d["SaveUpstreamStderrText"];
        SaveDownstreamStderrText = _d["SaveDownstreamStderrText"];
        OpenOutputDirectoryText = _d["OpenOutputDirectoryText"];
        ViewEncodingCommandText = _d["ViewEncodingCommandText"];
        InterruptUpstreamText = _d["InterruptUpstreamText"];
        InterruptEncoderText = _d["InterruptEncoderText"];
        CloseAfterDoneText = _d["CloseAfterDoneText"];
        EncodingCommandTitle = _d["EncodingCommandTitle"];
        PhysicalMemoryTopText = _d["PhysicalMemoryTopText"];
        PhysicalMemoryBottomText = _d["PhysicalMemoryBottomText"];
        CommittedMemoryTopText = _d["CommittedMemoryTopText"];
        CommittedMemoryBottomText = _d["CommittedMemoryBottomText"];
        WorkingSetPeakTopText = _d["WorkingSetPeakTopText"];
        WorkingSetPeakBottomText = _d["WorkingSetPeakBottomText"];
        PageFileTopText = _d["PageFileTopText"];
        PageFileBottomText = _d["PageFileBottomText"];
        PageFaultTopText = _d["PageFaultTopText"];
        PageFaultBottomText = _d["PageFaultBottomText"];
        RAMStressTopText = _d["RAMStressTopText"];
        RAMStressMediumText = _d["RAMStressMediumText"];
        RAMStressHighText = _d["RAMStressHighText"];
        BlockTooltipFormat = _d["BlockTooltipFormat"];
        PipeErrorPrefix = _d["PipeErrorPrefix"];
        ReadyToStartText = _d["ReadyToStartText"];
        EncodingText = _d["EncodingText"];
        MuxingText = _d["MuxingText"];
        InterruptedText = _d["InterruptedText"];
        FailedText = _d["FailedText"];
        CompletedText = _d["CompletedText"];
        ResetUsageStatusText = _d["ResetUsageStatusText"];
        InterruptingUpstreamText = _d["InterruptingUpstreamText"];
        InterruptingEncoderText = _d["InterruptingEncoderText"];
        ModeText = _d["ModeText"];
        NotAvailableText = _d["NotAvailableText"];
        ABRText = _d["ABRText"];
        CRFText = _d["CRFText"];
    }
}
