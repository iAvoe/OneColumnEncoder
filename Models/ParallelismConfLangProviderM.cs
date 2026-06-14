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
            ["NumaTopologyHintText"] = "When a CPU contains multiple NUMA nodes, the encoder will only occupy 1 → only a portion of CPU resources can be allocated",
            ["UpstreamNumaTitle"] = "NUMA Soft Binding: Pipe Upstream",
            ["DownstreamNumaTitle"] = "NUMA Soft Binding: Pipe Downstream (encoder)",
            ["NumaGuidanceText"] = "Assign encoder to other nodes might be faster when upstream tool has slow filters\notherwise, sharing same node might be faster—compute vs. latency bottleneck",
            ["ThreadStrategyTitle"] = "Hyper-threading & P-E Core Scheduling",
            ["EncoderThreadCountText"] = "Encoder threads",
            ["PreferUpstreamPhysCoresText"] = "Map upstream tool threads to phys. cores",
            ["PreferDownstreamPhysCoresText"] = "Map encoder threads to phys. cores",
            ["PreferPCoreComputeText"] = "TODO: Prefer P-Cores for encoder's compute threads",
            ["PreferECoreLookaheadText"] = "TODO: Prefer E-Cores for encoder's lookahead threads",
            ["MemoryStrategyTitle"] = "Advanced RAM Allocation",
            ["LargePagesUnavailableHintText"] = "No tool provides large-page RAM allocation setting, therefore cannot boost high-res. encoding performance.",
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
            ["NumaTopologyHintText"] = "单 CPU 内含多个 NUMA 节点时，编码器只会占用一个，此时只能分配部分 CPU 资源运行压制",
            ["UpstreamNumaTitle"] = "NUMA 软绑定：管道上游程序",
            ["DownstreamNumaTitle"] = "NUMA 软绑定：管道下游程序（编码器）",
            ["NumaGuidanceText"] = "若上游程序使用了高占用滤镜且视频源内容复杂，则上下游各占一节点的速度大概更快（算力瓶颈），\n否则共用节点的速度大概更快（通信瓶颈）",
            ["ThreadStrategyTitle"] = "超线程与 P-E 架构处理器调度",
            ["EncoderThreadCountText"] = "编码器线程数",
            ["PreferUpstreamPhysCoresText"] = "限制上游程序线程到物理核心数",
            ["PreferDownstreamPhysCoresText"] = "限制下游程序线程到物理核心数",
            ["PreferPCoreComputeText"] = "TODO：尝试分配编码任务到性能核心（P-Core）",
            ["PreferECoreLookaheadText"] = "TODO：尝试分配前瞻进程任务到能效核心（E-Core）",
            ["MemoryStrategyTitle"] = "高级内存分配策略",
            ["LargePagesUnavailableHintText"] = "所有编码工具都不提供大内存页分配设置，因此无法提高高分辨率视频编码性能",
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
            ["NumaTopologyHintText"] = "單 CPU 內含多個 NUMA 節點時，編碼器只會占用一個，此時只能分配部分 CPU 資源運行壓制",
            ["UpstreamNumaTitle"] = "NUMA 軟綁定：管道上游程式",
            ["DownstreamNumaTitle"] = "NUMA 軟綁定：管道下游程式（編碼器）",
            ["NumaGuidanceText"] = "若上遊程序使用了高占用濾鏡且影片源內容複雜，則上下游各占一節點的速度大概更快（算力瓶頸），\n否則共用節點的速度大概更快（通信瓶頸）",
            ["ThreadStrategyTitle"] = "超執行緒與 P-E 架構處理器調度",
            ["EncoderThreadCountText"] = "編碼器執行緒數",
            ["PreferUpstreamPhysCoresText"] = "限制上遊程序執行緒到物理核心數",
            ["PreferDownstreamPhysCoresText"] = "限制下遊程序執行緒到物理核心數",
            ["PreferPCoreComputeText"] = "TODO：嘗試分配編碼任務到性能核心（P-Core）",
            ["PreferECoreLookaheadText"] = "TODO：嘗試分配前瞻進程任務到能效核心（E-Core）",
            ["MemoryStrategyTitle"] = "高級記憶體分配策略",
            ["LargePagesUnavailableHintText"] = "所有編碼工具都不提供大記憶體頁分配設置，因此無法提高高解析度影片編碼性能",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認",
            ["CorePerGroup"] = "· 每 ",
            ["CorePerGroup1"] = " 核心直連 ",
            ["CorePerGroup1alt"] = " 核心（",
            ["CorePerGroup1alt1"] = " 執行緒）直連 ",
            ["CorePerGroup2"] = " MB 的 L3",
        },
        ["fr"] = new()
        {
            ["IntroText"] = "Ce programme ignore les modèles de parallélisme propres à chaque outil et utilise :\n· CPU Sets pour suggérer l'affinité des threads (migration temporaire autorisée)\n· Une tentative d'allocation RAM sur le noeud NUMA des threads d'encodage",
            ["PriorityText"] = "Ce programme n'élève pas la priorité et ne déclare pas l'encodage sensible à la latence,\nafin d'éviter qu'une tâche bloquée ne suspende l'OS indéfiniment.",
            ["CacheGroupTitle"] = "Groupes de cache L3 détectés (↑ franchissements, ↓ hits cache)",
            ["NumaTopologyHintText"] = "Si un CPU contient plusieurs noeuds NUMA, l'encodeur n'en occupe qu'un; seule une partie des ressources CPU est alors disponible.",
            ["UpstreamNumaTitle"] = "Liaison NUMA souple : amont du pipeline",
            ["DownstreamNumaTitle"] = "Liaison NUMA souple : aval du pipeline (encodeur)",
            ["NumaGuidanceText"] = "Placer l'encodeur sur un autre noeud peut être plus rapide si l'amont utilise des filtres lents;\nsinon, partager le même noeud peut être plus rapide — goulot calcul vs latence.",
            ["ThreadStrategyTitle"] = "Hyper-threading et planification des cœurs P/E",
            ["EncoderThreadCountText"] = "Threads encodeur",
            ["PreferUpstreamPhysCoresText"] = "Limiter l'amont aux cœurs physiques",
            ["PreferDownstreamPhysCoresText"] = "Limiter l'encodeur aux cœurs physiques",
            ["PreferPCoreComputeText"] = "TODO : préférer les P-Cores pour les threads de calcul de l'encodeur",
            ["PreferECoreLookaheadText"] = "TODO : préférer les E-Cores pour les threads lookahead de l'encodeur",
            ["MemoryStrategyTitle"] = "Allocation RAM avancée",
            ["LargePagesUnavailableHintText"] = "Aucun outil n'expose de réglage d'allocation par grandes pages; impossible d'accélérer l'encodage haute résolution par ce biais.",
            ["CancelButtonText"] = "Annuler",
            ["ConfirmButtonText"] = "Confirmer",
            ["CorePerGroup"] = "· Chaque groupe de ",
            ["CorePerGroup1"] = " cœurs est directement relié à ",
            ["CorePerGroup1alt"] = " cœurs (",
            ["CorePerGroup1alt1"] = " threads) est directement relié à ",
            ["CorePerGroup2"] = " Mo de L3"
        },
        ["es"] = new()
        {
            ["IntroText"] = "Este programa ignora los modelos de paralelismo propios de cada herramienta y usa:\n· CPU Sets para sugerir afinidad de hilos (permite migración temporal)\n· Un intento de asignar RAM en el nodo NUMA de los hilos de codificación",
            ["PriorityText"] = "No eleva la prioridad ni declara la codificación como sensible a latencia,\npara evitar que una tarea sin respuesta bloquee el SO indefinidamente.",
            ["CacheGroupTitle"] = "Grupos de caché L3 detectados (↑ cruces, ↓ aciertos de caché)",
            ["NumaTopologyHintText"] = "Si una CPU contiene varios nodos NUMA, el codificador solo ocupa uno; por tanto solo puede usar parte de los recursos de CPU.",
            ["UpstreamNumaTitle"] = "Vinculación NUMA suave: upstream del pipeline",
            ["DownstreamNumaTitle"] = "Vinculación NUMA suave: downstream del pipeline (codificador)",
            ["NumaGuidanceText"] = "Asignar el codificador a otro nodo puede ser más rápido si upstream usa filtros lentos;\nsi no, compartir nodo puede ser más rápido — cuello de botella de cómputo frente a latencia.",
            ["ThreadStrategyTitle"] = "Hyper-threading y planificación de núcleos P/E",
            ["EncoderThreadCountText"] = "Hilos del codificador",
            ["PreferUpstreamPhysCoresText"] = "Limitar upstream a núcleos físicos",
            ["PreferDownstreamPhysCoresText"] = "Limitar codificador a núcleos físicos",
            ["PreferPCoreComputeText"] = "TODO: preferir P-Cores para los hilos de cálculo del codificador",
            ["PreferECoreLookaheadText"] = "TODO: preferir E-Cores para los hilos de lookahead del codificador",
            ["MemoryStrategyTitle"] = "Asignación avanzada de RAM",
            ["LargePagesUnavailableHintText"] = "Ninguna herramienta expone asignación con páginas grandes; no se puede acelerar la codificación de alta resolución por esa vía.",
            ["CancelButtonText"] = "Cancelar",
            ["ConfirmButtonText"] = "Confirmar",
            ["CorePerGroup"] = "· Cada grupo de ",
            ["CorePerGroup1"] = " núcleos está en conexión directa con ",
            ["CorePerGroup1alt"] = " núcleos (",
            ["CorePerGroup1alt1"] = " hilos) está en conexión directa con ",
            ["CorePerGroup2"] = " MB de L3"
        },
        ["ja"] = new()
        {
            ["IntroText"] = "このプログラムは各ツール固有の並列実装を無視し、次を使用します:\n· CPU Sets によるスレッド親和性の提案（一時的なノード移動は許可）\n· エンコードスレッドの NUMA ノード上での RAM 割り当て試行",
            ["PriorityText"] = "タスク優先度を上げず、エンコードタスクをレイテンシ重視とも宣言しません。\n無応答タスクが OS を長時間停止させる事態を避けます。",
            ["CacheGroupTitle"] = "検出された L3 キャッシュグループ (↑グループ越境, ↓キャッシュヒット)",
            ["NumaTopologyHintText"] = "1 つの CPU に複数の NUMA ノードがある場合、エンコーダは 1 ノードのみを使用するため、利用できる CPU リソースは一部に限られます。",
            ["UpstreamNumaTitle"] = "NUMA ソフトバインド: パイプライン上流",
            ["DownstreamNumaTitle"] = "NUMA ソフトバインド: パイプライン下流 (エンコーダ)",
            ["NumaGuidanceText"] = "上流ツールのフィルタが重い場合、エンコーダを別ノードへ割り当てる方が高速なことがあります。\nそれ以外では同一ノード共有の方が高速なことがあります — 計算ボトルネックと遅延ボトルネックの違いです。",
            ["ThreadStrategyTitle"] = "ハイパースレッディングと P/E コアスケジューリング",
            ["EncoderThreadCountText"] = "エンコーダスレッド",
            ["PreferUpstreamPhysCoresText"] = "上流ツールのスレッドを物理コア数に制限",
            ["PreferDownstreamPhysCoresText"] = "エンコーダのスレッドを物理コア数に制限",
            ["PreferPCoreComputeText"] = "TODO: エンコーダの計算スレッドを P-Core 優先にする",
            ["PreferECoreLookaheadText"] = "TODO: エンコーダの lookahead スレッドを E-Core 優先にする",
            ["MemoryStrategyTitle"] = "高度な RAM 割当",
            ["LargePagesUnavailableHintText"] = "ラージページ RAM 割り当て設定を提供するツールがないため、この方法で高解像度エンコード性能を高めることはできません。",
            ["CancelButtonText"] = "取消",
            ["ConfirmButtonText"] = "確認",
            ["CorePerGroup"] = "· ",
            ["CorePerGroup1"] = " コアは ",
            ["CorePerGroup1alt"] = " コア (",
            ["CorePerGroup1alt1"] = " スレッド) は ",
            ["CorePerGroup2"] = "MB L3 に直結"
        },
        ["ru"] = new()
        {
            ["IntroText"] = "Программа игнорирует особенности параллелизма отдельных инструментов и использует:\n· CPU Sets для подсказки привязки потоков (временная миграция разрешена)\n· Попытку выделять RAM на NUMA-узле потоков кодирования",
            ["PriorityText"] = "Программа не повышает приоритет и не объявляет задачи кодирования чувствительными к задержке,\nчтобы зависшая задача не подвесила ОС на неопределенное время.",
            ["CacheGroupTitle"] = "Обнаруженные группы L3-кэша (↑ переходы между группами, ↓ cache hits)",
            ["NumaTopologyHintText"] = "Если CPU содержит несколько NUMA-узлов, кодер занимает только один; поэтому доступна лишь часть ресурсов CPU.",
            ["UpstreamNumaTitle"] = "Мягкая NUMA-привязка: upstream конвейера",
            ["DownstreamNumaTitle"] = "Мягкая NUMA-привязка: downstream конвейера (кодер)",
            ["NumaGuidanceText"] = "Кодер на другом узле может быть быстрее, если upstream использует медленные фильтры;\nиначе общий узел может быть быстрее — вычислительное узкое место против задержки.",
            ["ThreadStrategyTitle"] = "Hyper-threading и планирование P/E-ядер",
            ["EncoderThreadCountText"] = "Потоки кодера",
            ["PreferUpstreamPhysCoresText"] = "Ограничить upstream физическими ядрами",
            ["PreferDownstreamPhysCoresText"] = "Ограничить кодер физическими ядрами",
            ["PreferPCoreComputeText"] = "TODO: предпочитать P-Cores для вычислительных потоков кодера",
            ["PreferECoreLookaheadText"] = "TODO: предпочитать E-Cores для lookahead-потоков кодера",
            ["MemoryStrategyTitle"] = "Расширенное выделение RAM",
            ["LargePagesUnavailableHintText"] = "Инструменты не предоставляют настройку выделения large pages; этим способом нельзя ускорить кодирование высокого разрешения.",
            ["CancelButtonText"] = "Отмена",
            ["ConfirmButtonText"] = "Подтвердить",
            ["CorePerGroup"] = "· Каждая группа из ",
            ["CorePerGroup1"] = " ядер напрямую связана с ",
            ["CorePerGroup1alt"] = " ядер (",
            ["CorePerGroup1alt1"] = " потоков) напрямую связана с ",
            ["CorePerGroup2"] = " MB L3"
        }
    };

    public const string WindowTitle = "1cenc Parallelism Settings";
    public string IntroText { get; }
    public string PriorityText { get; }
    public string CacheGroupTitle { get; }
    public string NumaTopologyHintText { get; }
    public string UpstreamNumaTitle { get; }
    public string DownstreamNumaTitle { get; }
    public string NumaGuidanceText { get; }
    public string ThreadStrategyTitle { get; }
    public string PreferUpstreamPhysCoresText { get; }
    public string PreferDownstreamPhysCoresText { get; }
    public string PreferPCoreComputeText { get; }
    public string PreferECoreLookaheadText { get; }
    public string MemoryStrategyTitle { get; }
    public string LargePagesUnavailableHintText { get; }
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
        NumaTopologyHintText = _d["NumaTopologyHintText"];
        UpstreamNumaTitle = _d["UpstreamNumaTitle"];
        DownstreamNumaTitle = _d["DownstreamNumaTitle"];
        NumaGuidanceText = _d["NumaGuidanceText"];
        ThreadStrategyTitle = _d["ThreadStrategyTitle"];
        PreferUpstreamPhysCoresText = _d["PreferUpstreamPhysCoresText"];
        PreferDownstreamPhysCoresText = _d["PreferDownstreamPhysCoresText"];
        PreferPCoreComputeText = _d["PreferPCoreComputeText"];
        PreferECoreLookaheadText = _d["PreferECoreLookaheadText"];
        MemoryStrategyTitle = _d["MemoryStrategyTitle"];
        LargePagesUnavailableHintText = _d["LargePagesUnavailableHintText"];
        CancelButtonText = _d["CancelButtonText"];
        ConfirmButtonText = _d["ConfirmButtonText"];
        EncoderThreadCountText = _d["EncoderThreadCountText"];
    }
}
