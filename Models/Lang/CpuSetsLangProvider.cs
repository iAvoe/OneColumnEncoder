namespace OneColumnEncoder.Models.Lang;

public class CpuSetsLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["UnavailableOnNonWindows"] = "CPU Sets are only available on Windows.",
            ["NoCpuSetsFound"] = "No CPU Sets found for NUMA node {0}.",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets failed: {0}.",
            ["BoundSuccess"] = "Bound PID {0} to NUMA node {1} w/ {2} CPU Set(s);\nUpdated {3} existing thread(s). Pipe buffer size: {4}KB.",
            ["BindingFailed"] = "CPU Sets binding failed: {0}",
            ["SkippedPrefix"] = "Skipped CPU Sets binding. ",
        },
        ["zh-cn"] = new()
        {
            ["UnavailableOnNonWindows"] = "CPU Sets 仅在 Windows 上可用。",
            ["NoCpuSetsFound"] = "未找到 NUMA 节点 {0} 的 CPU Sets。",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets 失败：{0}。",
            ["BoundSuccess"] = "已将进程 {0} 绑定到 NUMA 节点 {1}，占 {2} 个 CPU Set；\n已更新 {3} 条现有线程。管道缓冲区大小：{4}KB。",
            ["BindingFailed"] = "CPU Sets 绑定失败：{0}",
            ["SkippedPrefix"] = "已跳过 CPU Sets 绑定。",

        },
        ["zh-tw"] = new()
        {
            ["UnavailableOnNonWindows"] = "CPU Sets 僅在 Windows 上可用。",
            ["NoCpuSetsFound"] = "未找到 NUMA 節點 {0} 的 CPU Sets。",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets 失敗：{0}。",
            ["BoundSuccess"] = "已將進程 {0} 綁定到 NUMA 節點 {1}，占 {2} 個 CPU Set；\n已更新 {3} 條現有執行緒。管道緩衝區大小：{4}KB。",
            ["BindingFailed"] = "CPU Sets 綁定失敗：{0}",
            ["SkippedPrefix"] = "已跳過 CPU Sets 綁定。",
        }
    };

    static CpuSetsLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["UnavailableOnNonWindows"] = "CPU Sets n'est disponible que sous Windows.",
            ["NoCpuSetsFound"] = "Aucun CPU Set trouvé pour le noeud NUMA {0}.",
            ["SetProcessDefaultCpuSetsFailed"] = "Échec de SetProcessDefaultCpuSets : {0}.",
            ["BoundSuccess"] = "PID {0} lié au noeud NUMA {1} avec {2} CPU Set(s);\n{3} thread(s) existant(s) mis à jour. Taille du tampon du pipeline : {4}KB.",
            ["BindingFailed"] = "Échec de liaison CPU Sets : {0}",
            ["SkippedPrefix"] = "Liaison CPU Sets ignorée. "
        };
        Data["es"] = new(Data["en"])
        {
            ["UnavailableOnNonWindows"] = "CPU Sets solo está disponible en Windows.",
            ["NoCpuSetsFound"] = "No se hallaron CPU Sets para el nodo NUMA {0}.",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets falló: {0}.",
            ["BoundSuccess"] = "PID {0} vinculado al nodo NUMA {1} con {2} CPU Set(s);\n{3} hilo(s) existentes actualizados. Tamaño del búfer de tubería: {4}KB.",
            ["BindingFailed"] = "Falló la vinculación de CPU Sets: {0}",
            ["SkippedPrefix"] = "Vinculación de CPU Sets omitida. "
        };
        Data["ja"] = new(Data["en"])
        {
            ["UnavailableOnNonWindows"] = "CPU Sets は Windows でのみ利用できます。",
            ["NoCpuSetsFound"] = "NUMA ノード {0} の CPU Sets が見つかりません。",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets に失敗: {0}。",
            ["BoundSuccess"] = "PID {0} を NUMA ノード {1} へ {2} CPU Set でバインド;\n既存スレッド {3} 件を更新しました。パイプバッファサイズ: {4}KB。",
            ["BindingFailed"] = "CPU Sets バインド失敗: {0}",
            ["SkippedPrefix"] = "CPU Sets バインドをスキップしました。 "
        };
        Data["ru"] = new(Data["en"])
        {
            ["UnavailableOnNonWindows"] = "CPU Sets доступны только в Windows.",
            ["NoCpuSetsFound"] = "CPU Sets для NUMA-узла {0} не найдены.",
            ["SetProcessDefaultCpuSetsFailed"] = "Сбой SetProcessDefaultCpuSets: {0}.",
            ["BoundSuccess"] = "PID {0} привязан к NUMA-узлу {1} с {2} CPU Set(s);\nобновлено существующих потоков: {3}. Размер буфера канала: {4}KB.",
            ["BindingFailed"] = "Сбой привязки CPU Sets: {0}",
            ["SkippedPrefix"] = "Привязка CPU Sets пропущена. "
        };
        Data["de"] = new(Data["en"])
        {
            ["UnavailableOnNonWindows"] = "CPU Sets sind nur unter Windows verfügbar.",
            ["NoCpuSetsFound"] = "Keine CPU Sets für NUMA-Knoten {0} gefunden.",
            ["SetProcessDefaultCpuSetsFailed"] = "SetProcessDefaultCpuSets fehlgeschlagen: {0}.",
            ["BoundSuccess"] = "PID {0} an NUMA-Knoten {1} mit {2} CPU Set(s) gebunden;\n{3} vorhandene Threads aktualisiert. Puffergröße: {4}KB.",
            ["BindingFailed"] = "CPU Sets-Bindung fehlgeschlagen: {0}",
            ["SkippedPrefix"] = "CPU Sets-Bindung übersprungen. "
        };
    }

    public string UnavailableOnNonWindows { get; }
    public string NoCpuSetsFound { get; }
    public string SetProcessDefaultCpuSetsFailed { get; }
    public string BoundSuccess { get; }
    public string BindingFailed { get; }
    public string SkippedPrefix { get; }
    public CpuSetsLangProvider(string languageCode) : base(languageCode, Data)
    {
        UnavailableOnNonWindows = this["UnavailableOnNonWindows"];
        NoCpuSetsFound = this["NoCpuSetsFound"];
        SetProcessDefaultCpuSetsFailed = this["SetProcessDefaultCpuSetsFailed"];
        BoundSuccess = this["BoundSuccess"];
        BindingFailed = this["BindingFailed"];
        SkippedPrefix = this["SkippedPrefix"];
    }
}
