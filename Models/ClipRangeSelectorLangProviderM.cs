namespace OneColumnEncoder.Models;

public class ClipRangeSelectorLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WindowTitle"] = "1cenc Sample Clip",
            ["TimelineSectionTitle"] = "Time Axis Segment",
            ["SelectionHintText"] = "Drag the handle to choose the segment position",
            ["DurationSectionTitle"] = "Duration Settings",
            ["ClipLengthLabel"] = "Duration (s)",
            ["StartTimeLabel"] = "Start Time",
            ["ClipDurationLabel"] = "Segment Duration",
            ["EndTimeLabel"] = "End Time",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "Start Frame/Field",
            ["ClipFrameCountLabel"] = "Frame/Field Duration",
            ["EndFrameLabel"] = "End Frame/Field",
            ["FrameFormatText"] = "frames|fields",
            ["Note1Text"] = "Total frames & frame rate of interlaced sources are really field count & field rates (2 fields/frame)",
            ["Note2Text"] = "Mismatched clip durations, time-bases prevents alignment, thus can't do quality metric benchmarks",
            ["CancelButtonText"] = "Cancel",
            ["ConfirmButtonText"] = "Encode Sample",
            ["SummaryDurationLabel"] = "Duration",
            ["SummaryTotalFramesLabel"] = "Total Frames",
            ["SummaryFrameRateLabel"] = "Frame Rate",
            ["SummarySecondsUnit"] = "s",
            ["SummaryProgressive"] = "Progressive",
            ["SummaryInterlaced"] = "Interlaced",
            ["SummaryUnknown"] = "Unknown",
            ["SummaryConstantFrameRate"] = "Constant",
            ["SummaryVariableFrameRate"] = "Variable / Unknown"
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "1cenc 片段打样",
            ["TimelineSectionTitle"] = "时间轴取段",
            ["SelectionHintText"] = "拖拽滑块以选择取段位置",
            ["DurationSectionTitle"] = "时长设定",
            ["ClipLengthLabel"] = "片长（秒）",
            ["StartTimeLabel"] = "起始时间",
            ["ClipDurationLabel"] = "片段时长",
            ["EndTimeLabel"] = "结束时间",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "起始帧数/场数",
            ["ClipFrameCountLabel"] = "帧数/场数时长",
            ["EndFrameLabel"] = "结束帧数/场数",
            ["FrameFormatText"] = "frames|fields",
            ["Note1Text"] = "隔行扫描的总帧数和帧率其实是“总场数”和“场率”（一帧两场）",
            ["Note2Text"] = "由于取段时长与源视频不同，且难以对齐，因此不适合进行画质跑分",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "开始打样",
            ["SummaryDurationLabel"] = "总时长",
            ["SummaryTotalFramesLabel"] = "总帧数",
            ["SummaryFrameRateLabel"] = "帧率",
            ["SummarySecondsUnit"] = "秒",
            ["SummaryProgressive"] = "逐行扫描",
            ["SummaryInterlaced"] = "隔行扫描",
            ["SummaryUnknown"] = "未知",
            ["SummaryConstantFrameRate"] = "恒定帧率",
            ["SummaryVariableFrameRate"] = "可变/未知帧率"
        },
        ["zh-tw"] = new()
        {
            ["WindowTitle"] = "1cenc 片段打樣",
            ["TimelineSectionTitle"] = "時間軸取段",
            ["SelectionHintText"] = "拖曳滑塊以選擇取段位置",
            ["DurationSectionTitle"] = "時長設定",
            ["ClipLengthLabel"] = "片長（秒）",
            ["StartTimeLabel"] = "起始時間",
            ["ClipDurationLabel"] = "片段時長",
            ["EndTimeLabel"] = "結束時間",
            ["TimeFormatText"] = "hh:mm:ss.sss",
            ["StartFrameLabel"] = "起始幀數/場數",
            ["ClipFrameCountLabel"] = "幀數/場數時長",
            ["EndFrameLabel"] = "結束幀數/場數",
            ["FrameFormatText"] = "frames|fields",
            ["Note1Text"] = "原行掃描的總幀數和幀率其實是「總場數」和「場率」（一幀兩場）",
            ["Note2Text"] = "由於取段時長與源影片不同，且難以對齊，因此不適合進行畫質跑分",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "開始打樣",
            ["SummaryDurationLabel"] = "總時長",
            ["SummaryTotalFramesLabel"] = "總幀數",
            ["SummaryFrameRateLabel"] = "幀率",
            ["SummarySecondsUnit"] = "秒",
            ["SummaryProgressive"] = "逐行掃描",
            ["SummaryInterlaced"] = "隔行掃描",
            ["SummaryUnknown"] = "未知",
            ["SummaryConstantFrameRate"] = "恆定幀率",
            ["SummaryVariableFrameRate"] = "可變/未知幀率"
        }
    };

    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public string WindowTitle { get; }
    public string TimelineSectionTitle { get; }
    public string SelectionHintText { get; }
    public string DurationSectionTitle { get; }
    public string ClipLengthLabel { get; }
    public string StartTimeLabel { get; }
    public string ClipDurationLabel { get; }
    public string EndTimeLabel { get; }
    public string TimeFormatText { get; }
    public string StartFrameLabel { get; }
    public string ClipFrameCountLabel { get; }
    public string EndFrameLabel { get; }
    public string FrameFormatText { get; }
    public string Note1Text { get; }
    public string Note2Text { get; }
    public string CancelButtonText { get; }
    public string ConfirmButtonText { get; }
    public string SummaryDurationLabel { get; }
    public string SummaryTotalFramesLabel { get; }
    public string SummaryFrameRateLabel { get; }
    public string SummarySecondsUnit { get; }
    public string SummaryProgressive { get; }
    public string SummaryInterlaced { get; }
    public string SummaryUnknown { get; }
    public string SummaryConstantFrameRate { get; }
    public string SummaryVariableFrameRate { get; }

    public ClipRangeSelectorLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        WindowTitle = _d["WindowTitle"];
        TimelineSectionTitle = _d["TimelineSectionTitle"];
        SelectionHintText = _d["SelectionHintText"];
        DurationSectionTitle = _d["DurationSectionTitle"];
        ClipLengthLabel = _d["ClipLengthLabel"];
        StartTimeLabel = _d["StartTimeLabel"];
        ClipDurationLabel = _d["ClipDurationLabel"];
        EndTimeLabel = _d["EndTimeLabel"];
        TimeFormatText = _d["TimeFormatText"];
        StartFrameLabel = _d["StartFrameLabel"];
        ClipFrameCountLabel = _d["ClipFrameCountLabel"];
        EndFrameLabel = _d["EndFrameLabel"];
        FrameFormatText = _d["FrameFormatText"];
        Note1Text = _d["Note1Text"];
        Note2Text = _d["Note2Text"];
        CancelButtonText = _d["CancelButtonText"];
        ConfirmButtonText = _d["ConfirmButtonText"];
        SummaryDurationLabel = _d["SummaryDurationLabel"];
        SummaryTotalFramesLabel = _d["SummaryTotalFramesLabel"];
        SummaryFrameRateLabel = _d["SummaryFrameRateLabel"];
        SummarySecondsUnit = _d["SummarySecondsUnit"];
        SummaryProgressive = _d["SummaryProgressive"];
        SummaryInterlaced = _d["SummaryInterlaced"];
        SummaryUnknown = _d["SummaryUnknown"];
        SummaryConstantFrameRate = _d["SummaryConstantFrameRate"];
        SummaryVariableFrameRate = _d["SummaryVariableFrameRate"];
    }
}
