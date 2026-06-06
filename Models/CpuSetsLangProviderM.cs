namespace OneColumnEncoder.Models;

public class CpuSetsLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["UnavailableOnNonWindows"] = "CPU Sets are only available on Windows.",
            ["NoCpuSetsFound"] = "No CPU Sets found for NUMA node {0}.",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets failed: {0}.",
            ["BoundSuccess"] = "Bound PID {0} to NUMA node {1} w/ {2} CPU Set(s);\nUpdated {3} existing thread(s).",
            ["BindingFailed"] = "CPU Sets binding failed: {0}",
            ["SkippedPrefix"] = "Skipped CPU Sets binding. ",
        },
        ["zh-cn"] = new()
        {
            ["UnavailableOnNonWindows"] = "CPU Sets 仅在 Windows 上可用。",
            ["NoCpuSetsFound"] = "未找到 NUMA 节点 {0} 的 CPU Sets。",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets 失败：{0}。",
            ["BoundSuccess"] = "已将进程 {0} 绑定到 NUMA 节点 {1}，占 {2} 个 CPU Set；\n已更新 {3} 条现有线程。",
            ["BindingFailed"] = "CPU Sets 绑定失败：{0}",
            ["SkippedPrefix"] = "已跳过 CPU Sets 绑定。",

        },
        ["zh-tw"] = new()
        {
            ["UnavailableOnNonWindows"] = "CPU Sets 僅在 Windows 上可用。",
            ["NoCpuSetsFound"] = "未找到 NUMA 節點 {0} 的 CPU Sets。",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets 失敗：{0}。",
            ["BoundSuccess"] = "已將進程 {0} 綁定到 NUMA 節點 {1}，占 {2} 個 CPU Set；\n已更新 {3} 條現有執行緒。",
            ["BindingFailed"] = "CPU Sets 綁定失敗：{0}",
            ["SkippedPrefix"] = "已跳過 CPU Sets 綁定。",
        }
    };

    public string UnavailableOnNonWindows { get; }
    public string NoCpuSetsFound { get; }
    public string SetProcessDefaultCpuSetsFailed { get; }
    public string BoundSuccess { get; }
    public string BindingFailed { get; }
    public string SkippedPrefix { get; }
    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public CpuSetsLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        UnavailableOnNonWindows = _d["UnavailableOnNonWindows"];
        NoCpuSetsFound = _d["NoCpuSetsFound"];
        SetProcessDefaultCpuSetsFailed = _d["SetProcessDefaultCpuSetsFailed"];
        BoundSuccess = _d["BoundSuccess"];
        BindingFailed = _d["BindingFailed"];
        SkippedPrefix = _d["SkippedPrefix"];
    }
}
