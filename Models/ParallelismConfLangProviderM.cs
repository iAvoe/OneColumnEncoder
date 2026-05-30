namespace OneColumnEncoder.Models;

public class ParallelismConfLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["IntroText"] = "This program ignores the various parallel-impls among tools, and uses:\n· CPU Sets to suggest thread affinity (Allowing temp. thread-node disloc.)\n· Try allocate RAM to encoding thread ranged on the their NUMA node",
            ["PriorityText"] = "This program avoids raising task priority, nor declare encoding tasks as latency-sens.,\nthereby preventing unresponsive tasks hanging the OS indefinitely",
            ["CacheGroupTitle"] = "Detected L3 cache groups (↑crossings, ↓cache hits)",
            ["UpstreamNumaTitle"] = "NUMA Soft Binding: Pipe Upstream",
            ["DownstreamNumaTitle"] = "NUMA Soft Binding: Pipe Downstream (encoder)",
            ["NumaGuidanceText"] = "Assign encoder to other nodes might be faster when upstream tool has slow filters\notherwise, sharing same node might be faster—compute vs. latency bottleneck",
            ["ThreadStrategyTitle"] = "Hyper-threading & P-E Core Scheduling",
            ["EncoderThreadCountText"] = "Encoder threads",
            ["PreferPhysicalCoresText"] = "Map encoder threads to phys. cores (clamps max slider range)",
            ["PreferPerformanceCoresText"] = "Prefer performance cores (P-Core)",
            ["MemoryStrategyTitle"] = "Advanced Memory Allocation (memory locking permission required)",
            ["UseLargePagesText"] = "Enable Large Pages for source videos with resolution above 2K",
            ["CancelButtonText"] = "Cancel",
            ["ConfirmButtonText"] = "Confirm",
            ["CorePerGroup"] = "· Every ",
            ["CorePerGroup1"] = " cores is/are directly connected to a ",
            ["CorePerGroup1alt"] = " cores (",
            ["CorePerGroup1alt1"] = " threads) is/are directly connected to a ",
            ["CorePerGroup2"] = "MB L3",
        },
        ["zh-cn"] = new()
        {
            ["IntroText"] = "本程序会忽略不同工具中各异的并行实现，并使用以下控制策略：\n· CPU Sets—引荐线程活动范围（允许系统临时外迁线程）\n· 尝试优先在指定 NUMA 节点分配部分内存",
            ["PriorityText"] = "本程序不调整进程优先级，或设置声明编码任务为延时敏感类型，以避免系统无限等待无响应编码器的问题",
            ["CacheGroupTitle"] = "检测到的核心缓存分组（跨组越多，缓存命中率越低）",
            ["UpstreamNumaTitle"] = "NUMA 软绑定：管道上游程序",
            ["DownstreamNumaTitle"] = "NUMA 软绑定：管道下游程序（编码器）",
            ["NumaGuidanceText"] = "若上游程序使用了高占用滤镜且视频源内容复杂，则上下游各占一节点的速度大概更快（算力瓶颈），\n否则共用节点的速度大概更快（通信瓶颈）",
            ["ThreadStrategyTitle"] = "超线程与 P-E 架构处理器调度",
            ["EncoderThreadCountText"] = "编码器线程数",
            ["PreferPhysicalCoresText"] = "逐物理核心分配编码器线程（会缩限线程数滑块范围）",
            ["PreferPerformanceCoresText"] = "优先使用性能核心（P-Core）",
            ["MemoryStrategyTitle"] = "高级内存分配策略（需锁定内存页权限）",
            ["UseLargePagesText"] = "超过 2K 分辨率时启用大页内存分区（Large Pages）",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "确认",
            ["CorePerGroup"] = "· 每 ",
            ["CorePerGroup1"] = " 核心直连 ",
            ["CorePerGroup1alt"] = " 核心（",
            ["CorePerGroup1alt1"] = " 线程）直连 ",
            ["CorePerGroup2"] = " MB 的 L3",
        },
        ["zh-tw"] = new()
        {
            ["IntroText"] = "本程式會忽略不同工具中各異的平行實現，並使用以下控制策略：\n· CPU Sets—引薦執行緒活動範圍（允許系統臨時外遷執行緒）\n· 嘗試優先在指定 NUMA 節點分配部分記憶體",
            ["PriorityText"] = "本程式不調整進程優先度，或設置聲明編碼任務為延時敏感類型，以避免系統無限等待無響應編碼器的問題",
            ["CacheGroupTitle"] = "檢測到的核心快取分組（跨組越多，快取命中率越低）",
            ["UpstreamNumaTitle"] = "NUMA 軟綁定：管道上游程式",
            ["DownstreamNumaTitle"] = "NUMA 軟綁定：管道下游程式（編碼器）",
            ["NumaGuidanceText"] = "若上遊程序使用了高占用濾鏡且影片源內容複雜，則上下游各占一節點的速度大概更快（算力瓶頸），\n否則共用節點的速度大概更快（通信瓶頸）",
            ["ThreadStrategyTitle"] = "超執行緒與 P-E 架構處理器調度",
            ["EncoderThreadCountText"] = "編碼器執行緒數",
            ["PreferPhysicalCoresText"] = "逐物理核心分配編碼器執行緒（會縮限執行緒數滑塊範圍）",
            ["PreferPerformanceCoresText"] = "優先使用性能核心（P-Core）",
            ["MemoryStrategyTitle"] = "高級記憶體分配策略（需鎖定記憶體頁權限）",
            ["UseLargePagesText"] = "超過 2K 解析度時啟用大頁記憶體分區（Large Pages）",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認",
            ["CorePerGroup"] = "· 每 ",
            ["CorePerGroup1"] = " 核心直連 ",
            ["CorePerGroup1alt"] = " 核心（",
            ["CorePerGroup1alt1"] = " 執行緒）直連 ",
            ["CorePerGroup2"] = " MB 的 L3",
        }
    };

    public string IntroText { get; }
    public string PriorityText { get; }
    public string CacheGroupTitle { get; }
    public string UpstreamNumaTitle { get; }
    public string DownstreamNumaTitle { get; }
    public string NumaGuidanceText { get; }
    public string ThreadStrategyTitle { get; }
    public string PreferPhysicalCoresText { get; }
    public string PreferPerformanceCoresText { get; }
    public string MemoryStrategyTitle { get; }
    public string UseLargePagesText { get; }
    public string CancelButtonText { get; }
    public string ConfirmButtonText { get; }
    public string EncoderThreadCountText { get; }
    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public ParallelismConfLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        IntroText = _d["IntroText"];
        PriorityText = _d["PriorityText"];
        CacheGroupTitle = _d["CacheGroupTitle"];
        UpstreamNumaTitle = _d["UpstreamNumaTitle"];
        DownstreamNumaTitle = _d["DownstreamNumaTitle"];
        NumaGuidanceText = _d["NumaGuidanceText"];
        ThreadStrategyTitle = _d["ThreadStrategyTitle"];
        PreferPhysicalCoresText = _d["PreferPhysicalCoresText"];
        PreferPerformanceCoresText = _d["PreferPerformanceCoresText"];
        MemoryStrategyTitle = _d["MemoryStrategyTitle"];
        UseLargePagesText = _d["UseLargePagesText"];
        CancelButtonText = _d["CancelButtonText"];
        ConfirmButtonText = _d["ConfirmButtonText"];
        EncoderThreadCountText = _d["EncoderThreadCountText"];
    }
}
