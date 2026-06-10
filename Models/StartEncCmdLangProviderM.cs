namespace OneColumnEncoder.Models;

public class StartEncCmdLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WarnTitle"] = "Encoding",
            ["MissingUpstreamMsg"] = "Missing upstream input path. Make sure a video source or script source is selected for the chosen upstream tool.",
            ["ConfirmTitle"] = "Encoding Command",
            ["OverwriteTitle"] = "Overwrite Output",
            ["OverwriteMsg"] = "The following output file(s) already exist and will be overwritten.",
            ["EncodedOutputLabel"] = "Encoder output",
            ["MuxOutputLabel"] = "Mux output",
            ["OverwriteTargetLabel"] = "{0}: {1} ({2})",
            ["LargestExistingSizeLabel"] = "Largest existing size: {0}",
            ["ConfirmDelayLabel"] = "Confirm button unlocks after {0} seconds.",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        },
        ["zh-cn"] = new()
        {
            ["WarnTitle"] = "编码",
            ["MissingUpstreamMsg"] = "缺少上游输入路径。请确保已为所选上游工具选择了视频源或脚本源。",
            ["ConfirmTitle"] = "编码命令",
            ["OverwriteTitle"] = "覆盖输出",
            ["OverwriteMsg"] = "以下输出文件已存在，将被覆盖。",
            ["EncodedOutputLabel"] = "编码器输出",
            ["MuxOutputLabel"] = "封装输出",
            ["OverwriteTargetLabel"] = "{0}：{1}（{2}）",
            ["LargestExistingSizeLabel"] = "最大现有大小：{0}",
            ["ConfirmDelayLabel"] = "确认按钮将在 {0} 秒后解锁。",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        },
        ["zh-tw"] = new()
        {
            ["WarnTitle"] = "編碼",
            ["MissingUpstreamMsg"] = "缺少上游輸入路徑。請確保已為所選上游工具選擇了影片源或腳本源。",
            ["ConfirmTitle"] = "編碼命令",
            ["OverwriteTitle"] = "覆蓋輸出",
            ["OverwriteMsg"] = "以下輸出檔案已存在，將被覆蓋。",
            ["EncodedOutputLabel"] = "編碼器輸出",
            ["MuxOutputLabel"] = "封裝輸出",
            ["OverwriteTargetLabel"] = "{0}：{1}（{2}）",
            ["LargestExistingSizeLabel"] = "最大現有大小：{0}",
            ["ConfirmDelayLabel"] = "確認按鈕將在 {0} 秒後解鎖。",
            ["GbSuffix"] = " GB",
            ["MbSuffix"] = " MB",
        }
    };

    public string WarnTitle { get; }
    public string MissingUpstreamMsg { get; }
    public string ConfirmTitle { get; }
    public string OverwriteTitle { get; }
    public string OverwriteMsg { get; }
    public string EncodedOutputLabel { get; }
    public string MuxOutputLabel { get; }
    public string OverwriteTargetLabel { get; }
    public string LargestExistingSizeLabel { get; }
    public string ConfirmDelayLabel { get; }
    public string GbSuffix { get; }
    public string MbSuffix { get; }
    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public StartEncCmdLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        WarnTitle = _d["WarnTitle"];
        MissingUpstreamMsg = _d["MissingUpstreamMsg"];
        ConfirmTitle = _d["ConfirmTitle"];
        OverwriteTitle = _d["OverwriteTitle"];
        OverwriteMsg = _d["OverwriteMsg"];
        EncodedOutputLabel = _d["EncodedOutputLabel"];
        MuxOutputLabel = _d["MuxOutputLabel"];
        OverwriteTargetLabel = _d["OverwriteTargetLabel"];
        LargestExistingSizeLabel = _d["LargestExistingSizeLabel"];
        ConfirmDelayLabel = _d["ConfirmDelayLabel"];
        GbSuffix = _d["GbSuffix"];
        MbSuffix = _d["MbSuffix"];
    }
}
