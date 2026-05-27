namespace OneColumnEncoder.Models;

public class ParallelismConfLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["WindowTitle"] = "Parallelism Configuration",
            ["IntroText"] = "This program ignores the different parallelism implementations among tools and uses the following scheduling strategy:\n· Use CPU Sets to guide thread affinity (the system can still migrate threads temporarily)\n· Prefer allocating some memory on the specified NUMA node when possible",
            ["PriorityText"] = "This program does not adjust process priority, to avoid encoder stalls causing the system to wait indefinitely.",
            ["CacheGroupTitle"] = "Detected core cache groups (more cross-grouping lowers cache hit rate)",
            ["CacheGroupHint"] = "· Every 8 cores share one 32 MB L3 cache",
            ["UpstreamNumaTitle"] = "NUMA node soft binding: upstream pipeline",
            ["DownstreamNumaTitle"] = "NUMA node soft binding: downstream pipeline (encoder)",
            ["NumaGuidanceText"] = "If the upstream process uses high-cost filters and the source content is complex, assigning upstream and downstream to different nodes may improve theoretical compute throughput; otherwise, sharing one node may reduce communication overhead.",
            ["ThreadStrategyTitle"] = "Hyper-threading and heterogeneous processor scheduling",
            ["PreferPhysicalCoresText"] = "Try assigning encoder threads to physical cores one by one",
            ["PreferPerformanceCoresText"] = "Prefer performance cores (P-Core)",
            ["MemoryStrategyTitle"] = "Advanced memory allocation strategy (requires locking memory pages permission)",
            ["UseLargePagesText"] = "Enable large page allocation above 2K resolution (Large Pages)",
            ["CancelButtonText"] = "Cancel",
            ["ConfirmButtonText"] = "Confirm"
        },
        ["zh-cn"] = new()
        {
            ["WindowTitle"] = "并行计算配置",
            ["IntroText"] = "本程序会忽略不同工具中各异的并行功能，并使用以下并行计算控制策略：\n· 使用 CPU Sets 引导线程活动范围（系统仍可临时外迁）\n· 尝试优先在指定 NUMA 节点分配部分内存",
            ["PriorityText"] = "本程序不会调整进程优先级，以避免编码器卡死时系统无限等待的问题",
            ["CacheGroupTitle"] = "检测到的核心缓存分组（跨组越多，缓存命中率越低）",
            ["CacheGroupHint"] = "· 每 8 核共用一组 32MB 的 L3 缓存",
            ["UpstreamNumaTitle"] = "NUMA 节点软绑定：管道上游程序",
            ["DownstreamNumaTitle"] = "NUMA 节点软绑定：管道下游程序（编码器）",
            ["NumaGuidanceText"] = "若上游程序使用了高占用滤镜且视频源内容复杂，则上下游各占一个节点的压制理论速度更快（算力瓶颈），否则共用节点更快（通信瓶颈）",
            ["ThreadStrategyTitle"] = "超线程与异构处理器的调度策略",
            ["PreferPhysicalCoresText"] = "尝试逐物理核心分配编码器线程",
            ["PreferPerformanceCoresText"] = "优先使用性能核心（P-Core）",
            ["MemoryStrategyTitle"] = "高级内存分配策略（需锁定内存页权限）",
            ["UseLargePagesText"] = "超过 2K 分辨率时启用大页内存分区（Large Pages）",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "确认"
        },
        ["zh-tw"] = new()
        {
            ["WindowTitle"] = "平行計算配置",
            ["IntroText"] = "本程式會忽略不同工具中各異的平行功能，並使用以下平行計算控制策略：\n· 使用 CPU Sets 引導執行緒活動範圍（系統仍可臨時外遷）\n· 嘗試優先在指定 NUMA 節點分配部分記憶體",
            ["PriorityText"] = "本程式不會調整程序優先級，以避免編碼器卡死時系統無限等待的問題",
            ["CacheGroupTitle"] = "檢測到的核心快取分組（跨組越多，快取命中率越低）",
            ["CacheGroupHint"] = "· 每 8 核共用一組 32MB 的 L3 快取",
            ["UpstreamNumaTitle"] = "NUMA 節點軟綁定：管道上游程式",
            ["DownstreamNumaTitle"] = "NUMA 節點軟綁定：管道下游程式（編碼器）",
            ["NumaGuidanceText"] = "若上游程式使用了高占用濾鏡且影片源內容複雜，則上下游各占一個節點的壓制理論速度更快（算力瓶頸），否則共用節點更快（通信瓶頸）",
            ["ThreadStrategyTitle"] = "超執行緒與異構處理器的調度策略",
            ["PreferPhysicalCoresText"] = "嘗試逐物理核心分配編碼器執行緒",
            ["PreferPerformanceCoresText"] = "優先使用性能核心（P-Core）",
            ["MemoryStrategyTitle"] = "高級記憶體分配策略（需鎖定記憶體頁權限）",
            ["UseLargePagesText"] = "超過 2K 解析度時啟用大頁記憶體分區（Large Pages）",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認"
        }
    };

    public string WindowTitle { get; }
    public string IntroText { get; }
    public string PriorityText { get; }
    public string CacheGroupTitle { get; }
    public string CacheGroupHint { get; }
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

    public ParallelismConfLangProviderM(string languageCode)
    {
        var d = Data.TryGetValue(languageCode, out var lang) ? lang : Data["en"];
        WindowTitle = d["WindowTitle"];
        IntroText = d["IntroText"];
        PriorityText = d["PriorityText"];
        CacheGroupTitle = d["CacheGroupTitle"];
        CacheGroupHint = d["CacheGroupHint"];
        UpstreamNumaTitle = d["UpstreamNumaTitle"];
        DownstreamNumaTitle = d["DownstreamNumaTitle"];
        NumaGuidanceText = d["NumaGuidanceText"];
        ThreadStrategyTitle = d["ThreadStrategyTitle"];
        PreferPhysicalCoresText = d["PreferPhysicalCoresText"];
        PreferPerformanceCoresText = d["PreferPerformanceCoresText"];
        MemoryStrategyTitle = d["MemoryStrategyTitle"];
        UseLargePagesText = d["UseLargePagesText"];
        CancelButtonText = d["CancelButtonText"];
        ConfirmButtonText = d["ConfirmButtonText"];
    }
}
