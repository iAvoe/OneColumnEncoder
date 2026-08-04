namespace OneColumnEncoder.Models.Lang;

public sealed class RepartLangProvider
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Tool"] = "Video Source Repart",
            ["ToolCount"] = "Video Source Repart ({0} -> {1})",
            ["WindowTitle"] = "Repart Mode",
            ["SelectFolder"] = "Select Repart video source folder",
            ["AppendFiles"] = "Append Files",
            ["ImportFolder"] = "Import Folder",
            ["ImportChapters"] = "Import Chapters",
            ["ImportMpls"] = "Read MPLS",
            ["Unavailable"] = "Unavailable",
            ["InputSources"] = "Input Sources",
            ["OutputEpisodes"] = "Output Episodes",
            ["Timeline"] = "Virtual Video Timeline",
            ["ValidationTitle"] = "Repart Source Validation",
            ["ValidationSubtitle"] = "Requires identical CFR video streams",
            ["Unallocated"] = "Unallocated",
            ["OutputName"] = "Output name",
            ["StartTime"] = "Start time",
            ["EndTime"] = "End time",
            ["FirstFrame"] = "First frame",
            ["LastFrame"] = "Last frame",
            ["AddEpisode"] = "Add Episode",
            ["ApplyEdit"] = "Apply Edit",
            ["DeleteEpisode"] = "Delete",
            ["MergeEpisodes"] = "Merge Selected",
            ["Remove"] = "Remove",
            ["MoveUp"] = "Move Up",
            ["MoveDown"] = "Move Down",
            ["Apply"] = "Apply",
            ["Cancel"] = "Cancel",
            ["Analyzing"] = "Analyzing source video streams...",
            ["Ready"] = "Sources are compatible. Add at least one output episode.",
            ["Summary"] = "{0} sources | {1:N0} frames | {2}/{3} fps | {4}",
            ["FfprobeRequired"] = "Repart Mode requires ffprobe.",
            ["FfmpegRequired"] = "Repart Mode requires ffmpeg to create video-only MKV outputs.",
            ["SourceRequired"] = "Import at least one video source.",
            ["SourceMissing"] = "Source file does not exist: {0}",
            ["SourceChanged"] = "Source file changed while it was being analyzed: {0}",
            ["NoVideoStream"] = "No video stream was found in {0}.",
            ["CfrRequired"] = "Repart Mode currently requires CFR video: {0}",
            ["FrameCountRequired"] = "A reliable frame count is required: {0}",
            ["FormatMismatch"] = "Source #{0} ({1}) does not exactly match the first video stream.\nExpected: {2}\nActual: {3}",
            ["ProbeFailed"] = "ffprobe failed or returned no data.",
            ["InvalidRange"] = "Enter a valid frame or time range inside the virtual timeline.",
            ["Overlap"] = "Output episodes cannot overlap.",
            ["AdjacentRequired"] = "Only directly adjacent output episodes can be merged.",
            ["SelectMerge"] = "Select at least two adjacent output episodes.",
            ["UniqueName"] = "Output episode names must be valid and unique.",
            ["OutputsRequired"] = "Add at least one output episode before applying.",
            ["RevisionDisabled"] = "Source Reviser is disabled for an active Repart plan. Re-import the sources to change source metadata.",
            ["SourceChangeWarning"] = "Changing the source order clears all configured output episodes. Continue?",
            ["SourceChangeTitle"] = "Reset Repart Outputs"
        },
        ["zh-cn"] = new()
        {
            ["Tool"] = "视频源重分集",
            ["ToolCount"] = "视频源重分集 ({0} → {1})",
            ["WindowTitle"] = "重分集模式",
            ["SelectFolder"] = "选择重分集视频源文件夹",
            ["AppendFiles"] = "追加文件",
            ["ImportFolder"] = "导入文件夹",
            ["ImportChapters"] = "导入章节",
            ["ImportMpls"] = "读取 MPLS",
            ["Unavailable"] = "暂不可用",
            ["InputSources"] = "输入视频源",
            ["OutputEpisodes"] = "输出分集",
            ["Timeline"] = "虚拟视频时间轴",
            ["ValidationTitle"] = "重分集视频源验证",
            ["ValidationSubtitle"] = "要求视频流格式完全一致且为 CFR",
            ["Unallocated"] = "未分配",
            ["OutputName"] = "输出名称",
            ["StartTime"] = "开始时间",
            ["EndTime"] = "结束时间",
            ["FirstFrame"] = "首帧",
            ["LastFrame"] = "末帧",
            ["AddEpisode"] = "添加分集",
            ["ApplyEdit"] = "应用修改",
            ["DeleteEpisode"] = "删除",
            ["MergeEpisodes"] = "合并所选分集",
            ["Remove"] = "移除",
            ["MoveUp"] = "上移",
            ["MoveDown"] = "下移",
            ["Apply"] = "应用",
            ["Cancel"] = "取消",
            ["Analyzing"] = "正在分析视频源流……",
            ["Ready"] = "视频源完全兼容，请添加至少一个输出分集。",
            ["Summary"] = "{0} 个视频源 | {1:N0} 帧 | {2}/{3} fps | {4}",
            ["FfprobeRequired"] = "重分集模式需要 ffprobe。",
            ["FfmpegRequired"] = "重分集模式需要 ffmpeg 以生成 video-only MKV。",
            ["SourceRequired"] = "请至少导入一个视频源。",
            ["SourceMissing"] = "视频源文件不存在：{0}",
            ["SourceChanged"] = "视频源在分析过程中发生了变化：{0}",
            ["NoVideoStream"] = "未在 {0} 中找到视频流。",
            ["CfrRequired"] = "重分集模式首版只接受 CFR 视频：{0}",
            ["FrameCountRequired"] = "视频源必须具有可靠的帧数：{0}",
            ["FormatMismatch"] = "第 {0} 个源（{1}）与首个视频流格式不完全一致。\n预期：{2}\n实际：{3}",
            ["ProbeFailed"] = "ffprobe 执行失败或没有返回数据。",
            ["InvalidRange"] = "请输入位于虚拟时间轴内的有效时间或帧范围。",
            ["Overlap"] = "输出分集不能相互重叠。",
            ["AdjacentRequired"] = "只能合并首尾直接相邻的输出分集。",
            ["SelectMerge"] = "请至少选择两个相邻的输出分集。",
            ["UniqueName"] = "输出分集名称必须有效且不能重复。",
            ["OutputsRequired"] = "应用前请至少添加一个输出分集。",
            ["RevisionDisabled"] = "重分集计划不允许使用视频源修订；如需修改源信息，请重新导入视频源。",
            ["SourceChangeWarning"] = "修改视频源顺序会清除全部已配置输出分集，是否继续？",
            ["SourceChangeTitle"] = "重置重分集输出"
        }
    };

    static RepartLangProvider()
    {
        Data["zh-tw"] = new(Data["zh-cn"])
        {
            ["Tool"] = "影片來源重分集",
            ["ToolCount"] = "影片來源重分集 ({0} → {1})",
            ["WindowTitle"] = "重分集模式",
            ["InputSources"] = "輸入影片來源",
            ["OutputEpisodes"] = "輸出分集"
        };
        foreach (string language in new[] { "fr", "es" })
            Data[language] = new(Data["en"]);
        Data["ja"] = new(Data["en"])
        {
            ["Tool"] = "映像ソース再分割",
            ["ToolCount"] = "映像ソース再分割 ({0} -> {1})",
            ["WindowTitle"] = "再分割モード",
            ["ValidationTitle"] = "再分割ソース検証",
            ["OutputEpisodes"] = "再分割出力"
        };
        Data["ru"] = new(Data["en"])
        {
            ["Tool"] = "Репарт видеоисточника",
            ["ToolCount"] = "Репарт видеоисточника ({0} -> {1})",
            ["WindowTitle"] = "Режим Репарт",
            ["ValidationTitle"] = "Проверка источника Репарт",
            ["OutputEpisodes"] = "Выходы Репарт"
        };
    }

    private readonly Dictionary<string, string> _data;
    public static RepartLangProvider Current => new(UILangProvider.Current.LanguageCode);
    public string this[string key] => _data.TryGetValue(key, out string? value) ? value : key;

    public RepartLangProvider(string languageCode) =>
        _data = Data.TryGetValue(languageCode, out Dictionary<string, string>? data) ? data : Data["en"];

    public string ToolSourceVideoSrcRepart => this["Tool"];
    public string ToolSourceVideoSrcRepartWithCount => this["ToolCount"];
    public string FfprobeRequired => this["FfprobeRequired"];
    public string FfmpegRequired => this["FfmpegRequired"];
    public string SourceRequired => this["SourceRequired"];
    public string SourceMissing => this["SourceMissing"];
    public string SourceChangedDuringAnalysis => this["SourceChanged"];
    public string NoVideoStream => this["NoVideoStream"];
    public string CfrRequired => this["CfrRequired"];
    public string FrameCountRequired => this["FrameCountRequired"];
    public string FormatMismatch => this["FormatMismatch"];
    public string ProbeFailed => this["ProbeFailed"];
}
