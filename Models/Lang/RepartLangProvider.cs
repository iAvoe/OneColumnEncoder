namespace OneColumnEncoder.Models.Lang;

public sealed class RepartLangProvider
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Tool"] = "Video Source Repart",
            ["ToolCount"] = "Video Source Repart ({0} -> {1})",
            ["WindowTitle"] = "1cenc Episode Repartition",
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
            ["ValidationSubtitle"] = "Requires identical CFR video streams with scan-stable frame counts",
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
            ["ReadyWithExcluded"] = "Sources are compatible. {0} incompatible source item(s) were excluded.",
            ["InterlacedSourcePrompt"] = "Interlaced source detected: {0} (field_order={1}).\n\nRepart Mode requires progressive, scan-stable CFR sources for frame-exact output.\n\nConfirm: discard this source and continue importing.\nCancel: cancel this import.",
            ["InterlacedSourceRejected"] = "Interlaced source is not supported in Repart Mode: {0} (field_order={1}).",
            ["Summary"] = "{0} sources | {1:N0} frames | {2}/{3} fps | {4}",
            ["FfprobeRequired"] = "Repart Mode requires ffprobe.",
            ["FfmpegRequired"] = "Repart Mode requires ffmpeg to create video-only MKV outputs.",
            ["SourceRequired"] = "Import at least one video source.",
            ["SourceMissing"] = "Source file does not exist: {0}",
            ["SourceChanged"] = "Source file changed while it was being analyzed: {0}",
            ["NoVideoStream"] = "No video stream was found in {0}.",
            ["CfrRequired"] = "Repart Mode requires scan-stable CFR video: {0}",
            ["FrameCountRequired"] = "Repart Mode could not scan a reliable frame count: {0}",
            ["FormatMismatch"] = "Source #{0} ({1}) does not exactly match the first video stream.\nExpected: {2}\nActual: {3}",
            ["ProbeFailed"] = "ffprobe failed or returned no data.",
            ["InvalidRange"] = "Enter a valid frame or time range inside the virtual timeline.",
            ["Overlap"] = "Output episodes cannot overlap.",
            ["AdjacentRequired"] = "Only directly adjacent output episodes can be merged.",
            ["SelectMerge"] = "Select at least two adjacent output episodes.",
            ["UniqueName"] = "Output episode names must be valid and unique.",
            ["OutputsRequired"] = "Add at least one output episode before applying.",
            ["RevisionDisabled"] = "Source Reviser is disabled for an active Repart plan. Re-import the sources to change source metadata.",
            ["FrameChangingFiltersWarning"] = "Repart outputs are frame-exact. Avoid filters that change frame count or timing, such as IVTC, decimation, interpolation, or CFR/VFR conversion.",
            ["SourceChangeWarning"] = "Changing sources will remap unaffected output episodes and remove affected ones. Continue?",
            ["SourceChangeTitle"] = "Reset Repart Outputs"
        },
        ["zh-cn"] = new()
        {
            ["Tool"] = "视频源重分集",
            ["ToolCount"] = "视频源重分集 ({0} → {1})",
            ["WindowTitle"] = "1cenc Episode Repartition",
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
            ["ValidationSubtitle"] = "要求视频流格式完全一致、为 CFR，且可稳定扫描帧数",
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
            ["ReadyWithExcluded"] = "视频源完全兼容，已排除 {0} 个不兼容源项目。",
            ["InterlacedSourcePrompt"] = "检测到隔行扫描源：{0}（field_order={1}）。\n\n重分集模式要求逐行扫描、可稳定扫描帧数的 CFR 源，以保证按帧精确输出。\n\n确认：丢弃此源并继续导入。\n取消：取消本次导入。",
            ["InterlacedSourceRejected"] = "重分集模式不支持隔行扫描源：{0}（field_order={1}）。",
            ["Summary"] = "{0} 个视频源 | {1:N0} 帧 | {2}/{3} fps | {4}",
            ["FfprobeRequired"] = "重分集模式需要 ffprobe。",
            ["FfmpegRequired"] = "重分集模式需要 ffmpeg 以生成 video-only MKV。",
            ["SourceRequired"] = "请至少导入一个视频源。",
            ["SourceMissing"] = "视频源文件不存在：{0}",
            ["SourceChanged"] = "视频源在分析过程中发生了变化：{0}",
            ["NoVideoStream"] = "未在 {0} 中找到视频流。",
            ["CfrRequired"] = "重分集模式要求可稳定扫描的 CFR 视频：{0}",
            ["FrameCountRequired"] = "重分集模式无法可靠扫描视频帧数：{0}",
            ["FormatMismatch"] = "第 {0} 个源（{1}）与首个视频流格式不完全一致。\n预期：{2}\n实际：{3}",
            ["ProbeFailed"] = "ffprobe 执行失败或没有返回数据。",
            ["InvalidRange"] = "请输入位于虚拟时间轴内的有效时间或帧范围。",
            ["Overlap"] = "输出分集不能相互重叠。",
            ["AdjacentRequired"] = "只能合并首尾直接相邻的输出分集。",
            ["SelectMerge"] = "请至少选择两个相邻的输出分集。",
            ["UniqueName"] = "输出分集名称必须有效且不能重复。",
            ["OutputsRequired"] = "应用前请至少添加一个输出分集。",
            ["RevisionDisabled"] = "重分集计划不允许使用视频源修订；如需修改源信息，请重新导入视频源。",
            ["FrameChangingFiltersWarning"] = "重分集输出按源帧号精确切分。请避免使用会改变帧数或帧时序的滤镜，例如 IVTC、抽帧、补帧、CFR/VFR 转换。",
            ["SourceChangeWarning"] = "修改视频源会重映射不受影响的输出分集，并删除受影响的分集，是否继续？",
            ["SourceChangeTitle"] = "重置重分集输出"
        }
    };

    static RepartLangProvider()
    {
        Data["zh-tw"] = new(Data["zh-cn"])
        {
            ["Tool"] = "影片來源重分集",
            ["ToolCount"] = "影片來源重分集 ({0} → {1})",
            ["WindowTitle"] = "1cenc Episode Repartition",
            ["InputSources"] = "輸入影片來源",
            ["OutputEpisodes"] = "輸出分集",
            ["ValidationSubtitle"] = "要求影片流格式完全一致、為 CFR，且可穩定掃描影格數",
            ["CfrRequired"] = "重分集模式要求可穩定掃描的 CFR 影片：{0}",
            ["FrameCountRequired"] = "重分集模式無法可靠掃描影片影格數：{0}",
            ["FrameChangingFiltersWarning"] = "重分集輸出按來源影格號精確切分。請避免使用會改變影格數或影格時序的濾鏡，例如 IVTC、抽幀、補幀、CFR/VFR 轉換。",
            ["SourceChangeWarning"] = "修改影片來源會重映射不受影響的輸出分集，並刪除受影響的分集，是否繼續？"
        };
        Data["fr"] = new(Data["en"])
        {
            ["ValidationTitle"] = "Validation de la source Répartition",
            ["ValidationSubtitle"] = "Nécessite des flux vidéo CFR identiques avec des comptes d'images stables à l'analyse",
            ["CfrRequired"] = "Le mode Répartition nécessite une vidéo CFR stable à l'analyse : {0}",
            ["FrameCountRequired"] = "Le mode Répartition n'a pas pu analyser un nombre d'images fiable : {0}",
            ["FrameChangingFiltersWarning"] = "Les sorties Répartition sont découpées exactement sur les images source. Évitez les filtres qui modifient le nombre d'images ou le timing, comme IVTC, la décimation, l'interpolation ou la conversion CFR/VFR.",
            ["SourceChangeWarning"] = "La modification des sources remappera les sorties non affectées et supprimera celles qui le sont. Continuer ?"
        };
        Data["es"] = new(Data["en"])
        {
            ["ValidationTitle"] = "Validación de fuente Repart",
            ["ValidationSubtitle"] = "Requiere flujos de vídeo CFR idénticos con recuentos de fotogramas estables al analizar",
            ["CfrRequired"] = "El modo Repart requiere vídeo CFR estable al analizar: {0}",
            ["FrameCountRequired"] = "El modo Repart no pudo analizar un recuento de fotogramas fiable: {0}",
            ["FrameChangingFiltersWarning"] = "Las salidas de Repart se cortan exactamente sobre los fotogramas de origen. Evita filtros que cambien el número de fotogramas o el tiempo, como IVTC, decimación, interpolación o conversión CFR/VFR.",
            ["SourceChangeWarning"] = "Al cambiar las fuentes se reasignarán las salidas no afectadas y se eliminarán las afectadas. ¿Continuar?"
        };
        Data["ja"] = new(Data["en"])
        {
            ["Tool"] = "映像ソース再分割",
            ["ToolCount"] = "映像ソース再分割 ({0} -> {1})",
            ["WindowTitle"] = "1cenc Episode Repartition",
            ["ValidationTitle"] = "再分割ソース検証",
            ["ValidationSubtitle"] = "同一の CFR 映像ストリームで、解析時に安定してフレーム数を取得できる必要があります",
            ["CfrRequired"] = "再分割モードでは、解析時に安定してフレーム数を取得できる CFR 映像が必要です: {0}",
            ["FrameCountRequired"] = "再分割モードで信頼できるフレーム数を取得できませんでした: {0}",
            ["FrameChangingFiltersWarning"] = "再分割出力はソースのフレーム番号に厳密に合わせて切り出します。IVTC、間引き、補間、CFR/VFR 変換など、フレーム数やタイミングを変えるフィルタは使わないでください。",
            ["SourceChangeWarning"] = "ソースを変更すると、影響を受けない出力は再マップされ、影響を受けるものは削除されます。続行しますか？"
        };
        Data["ru"] = new(Data["en"])
        {
            ["Tool"] = "Репарт видеоисточника",
            ["ToolCount"] = "Репарт видеоисточника ({0} -> {1})",
            ["WindowTitle"] = "1cenc Episode Repartition",
            ["ValidationTitle"] = "Проверка источника Repart",
            ["ValidationSubtitle"] = "Требуются идентичные CFR-видеопотоки, для которых анализ стабильно получает число кадров",
            ["CfrRequired"] = "Режим Repart требует CFR-видео со стабильным анализом: {0}",
            ["FrameCountRequired"] = "Режим Repart не смог получить надёжное число кадров: {0}",
            ["FrameChangingFiltersWarning"] = "Выходы Repart режутся точно по кадрам источника. Избегайте фильтров, которые меняют число кадров или тайминг, например IVTC, декимации, интерполяции или преобразования CFR/VFR.",
            ["SourceChangeWarning"] = "При изменении источников не затронутые выходы будут переназначены, а затронутые удалены. Продолжить?"
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
