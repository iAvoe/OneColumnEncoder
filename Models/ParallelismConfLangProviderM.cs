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
            ["ThreadStrategyTitle"] = "Processor Scheduling",
            ["EncoderThreadCountText"] = "Encoder threads",
            ["PreferUpstreamPhysCoresText"] = "Map upstream tool threads to phys. cores",
            ["PreferDownstreamPhysCoresText"] = "Map encoder threads to phys. cores",
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
            ["ThreadStrategyTitle"] = "处理器调度",
            ["EncoderThreadCountText"] = "编码器线程数",
            ["PreferUpstreamPhysCoresText"] = "限制上游程序线程到物理核心数",
            ["PreferDownstreamPhysCoresText"] = "限制下游程序线程到物理核心数",
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
            ["ThreadStrategyTitle"] = "處理器調度",
            ["EncoderThreadCountText"] = "編碼器執行緒數",
            ["PreferUpstreamPhysCoresText"] = "限制上遊程序執行緒到物理核心數",
            ["PreferDownstreamPhysCoresText"] = "限制下遊程序執行緒到物理核心數",
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
            ["IntroText"] = "Ce programme ignore les diverses implémentations parallèles entre outils et utilise :\n· CPU Sets pour suggérer l'affinité des threads (migration temporaire autorisée)\n· Tente d'allouer la RAM sur le nœud NUMA des threads d'encodage",
            ["PriorityText"] = "Ce programme n'élève pas la priorité des tâches et ne déclare pas l'encodage comme sensible à la latence,\nempêchant ainsi les tâches sans réponse de bloquer l'OS indéfiniment.",
            ["CacheGroupTitle"] = "Groupes de cache L3 détectés (↑ franchissements, ↓ hits)",
            ["NumaTopologyHintText"] = "Lorsqu'un CPU contient plusieurs nœuds NUMA, l'encodeur n'en occupe qu'un → seule une partie des ressources CPU peut être allouée.",
            ["UpstreamNumaTitle"] = "Liaison NUMA logicielle : amont du pipeline",
            ["DownstreamNumaTitle"] = "Liaison NUMA logicielle : aval du pipeline (encodeur)",
            ["NumaGuidanceText"] = "Attribuer l'encodeur à un autre nœud peut être plus rapide si l'outil amont a des filtres lents ;\nsinon, partager le même nœud peut être plus rapide—goulot calcul vs latence.",
            ["ThreadStrategyTitle"] = "Ordonnancement processeur",
            ["EncoderThreadCountText"] = "Threads encodeur",
            ["PreferUpstreamPhysCoresText"] = "Limiter les threads amont aux cœurs physiques",
            ["PreferDownstreamPhysCoresText"] = "Limiter les threads encodeur aux cœurs physiques",
            ["MemoryStrategyTitle"] = "Allocation RAM avancée",
            ["LargePagesUnavailableHintText"] = "Aucun outil n'expose de réglage d'allocation RAM en grandes pages, donc impossible d'améliorer l'encodage haute résolution par ce moyen.",
            ["CancelButtonText"] = "Annuler",
            ["ConfirmButtonText"] = "Confirmer",
            ["CorePerGroup"] = "· Tous les ",
            ["CorePerGroup1"] = " cœurs sont directement reliés à ",
            ["CorePerGroup1alt"] = " cœurs (",
            ["CorePerGroup1alt1"] = " threads) sont directement reliés à ",
            ["CorePerGroup2"] = " Mo L3",
        },
        ["es"] = new()
        {
            ["IntroText"] = "Este programa ignora las diversas implementaciones paralelas entre herramientas y usa:\n· CPU Sets para sugerir afinidad de hilos (se permite migración temporal)\n· Intenta asignar RAM en el nodo NUMA de los hilos de codificación",
            ["PriorityText"] = "Este programa no eleva la prioridad de tareas ni declara la codificación como sensible a latencia,\nevita así que tareas sin respuesta bloqueen el SO indefinidamente.",
            ["CacheGroupTitle"] = "Grupos de caché L3 detectados (↑ cruces, ↓ aciertos)",
            ["NumaTopologyHintText"] = "Cuando una CPU contiene varios nodos NUMA, el codificador solo ocupa 1 → solo se puede asignar una parte de los recursos de CPU.",
            ["UpstreamNumaTitle"] = "Enlace NUMA suave: aguas arriba del pipeline",
            ["DownstreamNumaTitle"] = "Enlace NUMA suave: aguas abajo del pipeline (codificador)",
            ["NumaGuidanceText"] = "Asignar el codificador a otro nodo puede ser más rápido si la herramienta aguas arriba tiene filtros lentos;\nsi no, compartir el mismo nodo puede ser más rápido—cuello de botella cómputo vs latencia.",
            ["ThreadStrategyTitle"] = "Planificación del procesador",
            ["EncoderThreadCountText"] = "Hilos codificador",
            ["PreferUpstreamPhysCoresText"] = "Limitar hilos aguas arriba a núcleos físicos",
            ["PreferDownstreamPhysCoresText"] = "Limitar hilos codificador a núcleos físicos",
            ["MemoryStrategyTitle"] = "Asignación avanzada de RAM",
            ["LargePagesUnavailableHintText"] = "Ninguna herramienta ofrece ajuste de asignación de RAM en páginas grandes, por lo tanto no se puede mejorar el rendimiento de codificación HD.",
            ["CancelButtonText"] = "Cancelar",
            ["ConfirmButtonText"] = "Confirmar",
            ["CorePerGroup"] = "· Cada ",
            ["CorePerGroup1"] = " núcleos están conectados directamente a ",
            ["CorePerGroup1alt"] = " núcleos (",
            ["CorePerGroup1alt1"] = " hilos) están conectados directamente a ",
            ["CorePerGroup2"] = " MB L3",
        },
        ["ja"] = new()
        {
            ["IntroText"] = "このプログラムはツール間の様々な並列実装を無視し、以下を使用します:\n· CPU Sets によるスレッド親和性の提案（一時的なスレッド・ノード移動は許可）\n· エンコードスレッドの NUMA ノード上の RAM 割り当てを試行",
            ["PriorityText"] = "このプログラムはタスク優先度を上げず、エンコードタスクをレイテンシ重視とも宣言しません。\nそれにより、無応答タスクが OS を長時間停止させる事態を防ぎます。",
            ["CacheGroupTitle"] = "検出された L3 キャッシュグループ（↑グループ越境、↓キャッシュヒット）",
            ["NumaTopologyHintText"] = "CPU に複数の NUMA ノードが含まれる場合、エンコーダーは 1 つのノードのみ占有します → CPU リソースの一部のみ割り当て可能です。",
            ["UpstreamNumaTitle"] = "NUMA ソフトバインディング: パイプライン上流",
            ["DownstreamNumaTitle"] = "NUMA ソフトバインディング: パイプライン下流（エンコーダー）",
            ["NumaGuidanceText"] = "上流ツールのフィルターが遅い場合、エンコーダーを別のノードに割り当てる方が高速なことがあります。\nそれ以外では、同じノードを共有する方が高速なことがあります—計算 vs レイテンシボトルネック。",
            ["ThreadStrategyTitle"] = "プロセッサスケジューリング",
            ["EncoderThreadCountText"] = "エンコーダスレッド数",
            ["PreferUpstreamPhysCoresText"] = "上流ツールのスレッドを物理コアに制限",
            ["PreferDownstreamPhysCoresText"] = "エンコーダのスレッドを物理コアに制限",
            ["MemoryStrategyTitle"] = "高度な RAM 割り当て",
            ["LargePagesUnavailableHintText"] = "ラージページ RAM 割り当て設定を提供するツールがないため、この方法で高解像度エンコード性能を向上させることはできません。",
            ["CancelButtonText"] = "キャンセル",
            ["ConfirmButtonText"] = "確認",
            ["CorePerGroup"] = "· ",
            ["CorePerGroup1"] = " コアは ",
            ["CorePerGroup1alt"] = " コア（",
            ["CorePerGroup1alt1"] = " スレッド）は ",
            ["CorePerGroup2"] = "MB L3 に直結",
        },
        ["ru"] = new()
        {
            ["IntroText"] = "Эта программа игнорирует различные реализации параллелизма между инструментами и использует:\n· CPU Sets для предложения привязки потоков (разрешена временная миграция)\n· Пытается выделить RAM на NUMA-узле потоков кодирования",
            ["PriorityText"] = "Эта программа не повышает приоритет задач и не объявляет кодирование чувствительным к задержке,\nпредотвращая зависание ОС из-за неотвечающих задач.",
            ["CacheGroupTitle"] = "Обнаруженные группы L3-кэша (↑ переходы, ↓ попадания)",
            ["NumaTopologyHintText"] = "Если CPU содержит несколько NUMA-узлов, кодер занимает только 1 → можно выделить лишь часть ресурсов CPU.",
            ["UpstreamNumaTitle"] = "Программная NUMA-привязка: апстрим конвейера",
            ["DownstreamNumaTitle"] = "Программная NUMA-привязка: даунстрим конвейера (кодер)",
            ["NumaGuidanceText"] = "Назначение кодера на другой узел может быть быстрее, если апстрим-инструмент использует медленные фильтры;\nв противном случае общий узел может быть быстрее — вычисления vs задержка.",
            ["ThreadStrategyTitle"] = "Планирование процессора",
            ["EncoderThreadCountText"] = "Потоков кодера",
            ["PreferUpstreamPhysCoresText"] = "Ограничить потоки апстрима физическими ядрами",
            ["PreferDownstreamPhysCoresText"] = "Ограничить потоки кодера физическими ядрами",
            ["MemoryStrategyTitle"] = "Расширенное выделение RAM",
            ["LargePagesUnavailableHintText"] = "Ни один инструмент не предоставляет настройку выделения RAM большими страницами, поэтому невозможно повысить производительность HD-кодирования этим способом.",
            ["CancelButtonText"] = "Отмена",
            ["ConfirmButtonText"] = "Подтвердить",
            ["CorePerGroup"] = "· Каждые ",
            ["CorePerGroup1"] = " ядер напрямую связаны с ",
            ["CorePerGroup1alt"] = " ядер (",
            ["CorePerGroup1alt1"] = " потоков) напрямую связаны с ",
            ["CorePerGroup2"] = " MB L3",
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
        MemoryStrategyTitle = _d["MemoryStrategyTitle"];
        LargePagesUnavailableHintText = _d["LargePagesUnavailableHintText"];
        CancelButtonText = _d["CancelButtonText"];
        ConfirmButtonText = _d["ConfirmButtonText"];
        EncoderThreadCountText = _d["EncoderThreadCountText"];
    }
}
