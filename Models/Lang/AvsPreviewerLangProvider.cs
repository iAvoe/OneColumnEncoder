namespace OneColumnEncoder.Models.Lang;

public class AvsPreviewerLangProvider(string languageCode) : LangProviderBase(languageCode, Data)
{
    public static string WindowTitle => $"{LangProviderBase.AviSynth} Preview";
    public static string DebugWindowTitle => $"{LangProviderBase.AviSynth} Preview frame data";

    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["AvsPreview.Ready"] = "Ready",
            ["AvsPreview.ExtractingSource"] = "Extracting frame from source (original)...",
            ["AvsPreview.ExtractingFiltered"] = "Extracting frame from source (filtered)...",
            ["AvsPreview.FrameRendered"] = "Frame {0} rendered",
            ["AvsPreview.Cancelled"] = "Cancelled",
            ["AvsPreview.ScriptError"] = "Script error: {0}",
            ["AvsPreview.PreviewFrameFileMissing"] = "Preview frame file missing",
            ["AvsPreview.LogExitCode"] = "{0} exit code {1}",
            ["AvsPreview.LogErrorPrefix"] = "Error: ",
            ["AvsPreview.NoneText"] = "<none>",
            ["AvsPreview.DebugSourceVideo"] = "Source video: {0}",
            ["AvsPreview.DebugToolPath"] = "Tool path: {0}",
            ["AvsPreview.DebugTotalFrames"] = "TotalFrames: {0}",
            ["AvsPreview.DebugMaxPositionSeconds"] = "MaxPositionSeconds: {0}",
            ["AvsPreview.DebugPreviewScript"] = "Preview script:",
        },
        ["zh-cn"] = new()
        {
            ["AvsPreview.Ready"] = "就绪",
            ["AvsPreview.ExtractingSource"] = "正在从源（原始）提取帧...",
            ["AvsPreview.ExtractingFiltered"] = "正在从源（滤镜后）提取帧...",
            ["AvsPreview.FrameRendered"] = "已渲染第 {0} 帧",
            ["AvsPreview.Cancelled"] = "已取消",
            ["AvsPreview.ScriptError"] = "脚本错误：{0}",
            ["AvsPreview.PreviewFrameFileMissing"] = "预览帧文件缺失",
            ["AvsPreview.LogExitCode"] = "{0} 退出码 {1}",
            ["AvsPreview.LogErrorPrefix"] = "错误：",
            ["AvsPreview.NoneText"] = "<无>",
            ["AvsPreview.DebugSourceVideo"] = "源视频：{0}",
            ["AvsPreview.DebugToolPath"] = "工具路径：{0}",
            ["AvsPreview.DebugTotalFrames"] = "总帧数：{0}",
            ["AvsPreview.DebugMaxPositionSeconds"] = "最大帧数：{0}",
            ["AvsPreview.DebugPreviewScript"] = "预览脚本：",
        },
        ["zh-tw"] = new()
        {
            ["AvsPreview.Ready"] = "就緒",
            ["AvsPreview.ExtractingSource"] = "正在從來源（原始）提取幀...",
            ["AvsPreview.ExtractingFiltered"] = "正在從來源（濾鏡後）提取幀...",
            ["AvsPreview.FrameRendered"] = "已渲染第 {0} 幀",
            ["AvsPreview.Cancelled"] = "已取消",
            ["AvsPreview.ScriptError"] = "腳本錯誤：{0}",
            ["AvsPreview.PreviewFrameFileMissing"] = "預覽幀檔案缺失",
            ["AvsPreview.LogExitCode"] = "{0} 結束代碼 {1}",
            ["AvsPreview.LogErrorPrefix"] = "錯誤：",
            ["AvsPreview.NoneText"] = "<無>",
            ["AvsPreview.DebugSourceVideo"] = "來源影片：{0}",
            ["AvsPreview.DebugToolPath"] = "工具路徑：{0}",
            ["AvsPreview.DebugTotalFrames"] = "總幀數：{0}",
            ["AvsPreview.DebugMaxPositionSeconds"] = "最大幀數：{0}",
            ["AvsPreview.DebugPreviewScript"] = "預覽腳本：",
        },
    };

    public static AvsPreviewerLangProvider Current => new(UILangProvider.Current.LanguageCode);
    public string StatusReady => this["AvsPreview.Ready"];
    public string StatusExtractingSource => this["AvsPreview.ExtractingSource"];
    public string StatusExtractingFiltered => this["AvsPreview.ExtractingFiltered"];
    public string StatusFrameRendered => this["AvsPreview.FrameRendered"];
    public string StatusCancelled => this["AvsPreview.Cancelled"];
    public string StatusScriptError => this["AvsPreview.ScriptError"];
    public string PreviewFrameFileMissing => this["AvsPreview.PreviewFrameFileMissing"];
    public string LogExitCode => this["AvsPreview.LogExitCode"];
    public string LogErrorPrefix => this["AvsPreview.LogErrorPrefix"];
    public string NoneText => this["AvsPreview.NoneText"];
    public string DebugSourceVideo => this["AvsPreview.DebugSourceVideo"];
    public string DebugToolPath => this["AvsPreview.DebugToolPath"];
    public string DebugTotalFrames => this["AvsPreview.DebugTotalFrames"];
    public string DebugMaxPositionSeconds => this["AvsPreview.DebugMaxPositionSeconds"];
    public string DebugPreviewScript => this["AvsPreview.DebugPreviewScript"];
}
