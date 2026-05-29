namespace OneColumnEncoder.Models;

public class ParallelismConfLangProviderM
{
    public static ParallelismConfLangProviderM Current { get; private set; } = null!;
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WindowTitle"] = "Parallelism Configuration",
            ["IntroText"] = "This program ignores the various parallelism impls among tools & tends w/:\n· CPU Sets to suggest thread affinity (Allowing temp. thread-node disloc.)\n· Try allocate RAM to encoding thread ranged on the their NUMA node",
            ["PriorityText"] = "This program avoids increase of task priority, nor declare encoding tasks to be latency-sensitive,\nthereby preventing unresponsive tasks hanging the OS indefinitely",
            ["CacheGroupTitle"] = "Detected core cache groups (more cross-grouping lowers cache hit rate)",
            ["UpstreamNumaTitle"] = "NUMA Soft Binding: Pipe Upstream",
            ["DownstreamNumaTitle"] = "NUMA Soft Binding: Pipe Downstream (encoder)",
            ["NumaGuidanceText"] = "If upstream tool has slow filters, assign encoder to other nodes might be faster—compute bottleneck\notherwise, sharing might be faster—latency bottleneck",
            ["ThreadStrategyTitle"] = "Hyper-threading & P-E Core Scheduling",
            ["PreferPhysicalCoresText"] = "Try assigning encoder threads to physical cores one by one",
            ["PreferPerformanceCoresText"] = "Prefer performance cores (P-Core)",
            ["MemoryStrategyTitle"] = "Advanced Memory Allocation (memory locking permission required)",
            ["UseLargePagesText"] = "Enable Large Pages for source videos with resolution above 2K",
            ["CancelButtonText"] = "Cancel",
            ["ConfirmButtonText"] = "Confirm",
            ["CorePerGroup"] = "· Every ",
            ["CorePerGroup1"] = " cores is/are directly connected to a ",
            ["CorePerGroup1alt"] = " cores (",
            ["CorePerGroup1alt1"] = " threads) is/are directly connected to a ",
            ["CorePerGroup2"] = " MB L3 cache",
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "并行计算配置",
            ["IntroText"] = "本程序会忽略不同工具中各异的并行实现，并使用以下控制策略：\n· CPU Sets—引荐线程活动范围（允许系统临时外迁线程）\n· 尝试优先在指定 NUMA 节点分配部分内存",
            ["PriorityText"] = "本程序不调整进程优先级，或设置声明编码任务为延时敏感的类型，以避免系统无限等待无响应编码器的问题",
            ["CacheGroupTitle"] = "检测到的核心缓存分组（跨组越多，缓存命中率越低）",
            ["UpstreamNumaTitle"] = "NUMA 软绑定：管道上游程序",
            ["DownstreamNumaTitle"] = "NUMA 软绑定：管道下游程序（编码器）",
            ["NumaGuidanceText"] = "若上游程序使用了高占用滤镜且视频源内容复杂，则上下游各占一个节点的压制理论速度更快（算力瓶颈），否则共用节点更快（通信瓶颈）",
            ["ThreadStrategyTitle"] = "超线程与 P-E 架构处理器调度",
            ["PreferPhysicalCoresText"] = "尝试逐物理核心分配编码器线程",
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
            ["WindowTitle"] = "平行計算配置",
            ["IntroText"] = "本程式會忽略不同工具中各異的平行實現，並使用以下控制策略：\n· CPU Sets—引薦執行緒活動範圍（允許系統臨時外遷執行緒）\n· 嘗試優先在指定 NUMA 節點分配部分記憶體",
            ["PriorityText"] = "本程式不調整進程優先度，或設置聲明編碼任務為延時敏感的類型，以避免系統無限等待無響應編碼器的問題",
            ["CacheGroupTitle"] = "檢測到的核心快取分組（跨組越多，快取命中率越低）",
            ["UpstreamNumaTitle"] = "NUMA 軟綁定：管道上游程式",
            ["DownstreamNumaTitle"] = "NUMA 軟綁定：管道下游程式（編碼器）",
            ["NumaGuidanceText"] = "若上游程式使用了高占用濾鏡且影片源內容複雜，則上下游各占一個節點的壓制理論速度更快（算力瓶頸），否則共用節點更快（通信瓶頸）",
            ["ThreadStrategyTitle"] = "超執行緒與 P-E 架構處理器調度",
            ["PreferPhysicalCoresText"] = "嘗試逐物理核心分配編碼器執行緒",
            ["PreferPerformanceCoresText"] = "優先使用性能核心（P-Core）",
            ["MemoryStrategyTitle"] = "高級記憶體分配策略（需鎖定記憶體頁權限）",
            ["UseLargePagesText"] = "超過 2K 解析度時啟用大頁記憶體分區（Large Pages）",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認",
            ["CorePerGroup"] = "· 每 ",
            ["CorePerGroup1"] = " 核心直連 ",
            ["CorePerGroup1alt"] = " 核心（",
            ["CorePerGroup1alt1"] = " 執行緒）直連 ",
            ["CorePerGroup2"] = " MB 的 L3 快取",
        }
    };

    public string WindowTitle { get; }
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
    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public ParallelismConfLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        WindowTitle = _d["WindowTitle"];
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
        Current = this;
    }
}
