namespace OneColumnEncoder.Models;

public class AnalyzeSrcVideoCmdLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["QueueItemProgress"] = "Queue item {0}/{1}",
            ["SourceFilePath"] = "Source: {0}",
            ["QueueItemSkipMsg"] = "Skipping this source item. Close this dialog to move on",
            ["AllQueueItemsFailed"] = "Source queue analysis failed: all {0} queue item(s) were skipped because they could not be analyzed.",
            ["SkippedItemsLabel"] = "Skipped failed queue item(s): {0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "...and {0} more.",
            ["TotalFramesFormat"] = "{0}: {1}",
        },
        ["zh-cn"] = new()
        {
            ["QueueItemProgress"] = "队列项目 {0}/{1}",
            ["SourceFilePath"] = "源文件：{0}",
            ["QueueItemSkipMsg"] = "将跳过此视频源，关闭继续分析其余项目",
            ["AllQueueItemsFailed"] = "队列源分析失败：所有 {0} 个队列项目均因无法分析而被跳过。",
            ["SkippedItemsLabel"] = "跳过的失败队列项目：{0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "……以及另外 {0} 项。",
            ["TotalFramesFormat"] = "{0}：{1}",
        },
        ["zh-tw"] = new()
        {
            ["QueueItemProgress"] = "隊列項目 {0}/{1}",
            ["SourceFilePath"] = "來源檔案：{0}",
            ["QueueItemSkipMsg"] = "將跳過此影片源，關閉繼續分析其餘項目",
            ["AllQueueItemsFailed"] = "隊列來源分析失敗：所有 {0} 個隊列項目均因無法分析而被跳過。",
            ["SkippedItemsLabel"] = "跳過的失敗隊列項目：{0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "……以及另外 {0} 項。",
            ["TotalFramesFormat"] = "{0}：{1}",
        },
        ["fr"] = new()
        {
            ["QueueItemProgress"] = "Élément de file {0}/{1}",
            ["SourceFilePath"] = "Source : {0}",
            ["QueueItemSkipMsg"] = "Cet élément source sera ignoré. Fermez cette boîte de dialogue pour continuer.",
            ["AllQueueItemsFailed"] = "Échec de l'analyse de la file d'attente : les {0} élément(s) de la file ont été ignorés car ils n'ont pas pu être analysés.",
            ["SkippedItemsLabel"] = "Élément(s) de file ignoré(s) : {0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "...et {0} autre(s).",
            ["TotalFramesFormat"] = "{0} : {1}",
        },
        ["es"] = new()
        {
            ["QueueItemProgress"] = "Elemento de cola {0}/{1}",
            ["SourceFilePath"] = "Fuente: {0}",
            ["QueueItemSkipMsg"] = "Se omitirá este elemento de origen. Cierre este diálogo para continuar.",
            ["AllQueueItemsFailed"] = "Error de análisis de cola: los {0} elemento(s) de la cola se omitieron porque no se pudieron analizar.",
            ["SkippedItemsLabel"] = "Elemento(s) de cola omitido(s): {0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "...y {0} más.",
            ["TotalFramesFormat"] = "{0}: {1}",
        },
        ["ja"] = new()
        {
            ["QueueItemProgress"] = "キュー項目 {0}/{1}",
            ["SourceFilePath"] = "ソース: {0}",
            ["QueueItemSkipMsg"] = "このソース項目はスキップされます。このダイアログを閉じて、残りの項目の解析を続行してください。",
            ["AllQueueItemsFailed"] = "キューのソース解析に失敗しました: すべての {0} 個のキュー項目を解析できなかったためスキップしました。",
            ["SkippedItemsLabel"] = "スキップされた失敗キュー項目: {0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "...他 {0} 件。",
            ["TotalFramesFormat"] = "{0}: {1}",
        },
        ["ru"] = new()
        {
            ["QueueItemProgress"] = "Элемент очереди {0}/{1}",
            ["SourceFilePath"] = "Источник: {0}",
            ["QueueItemSkipMsg"] = "Этот исходный элемент будет пропущен. Закройте этот диалог, чтобы продолжить.",
            ["AllQueueItemsFailed"] = "Сбой анализа очереди: все {0} элементов очереди пропущены, так как их не удалось проанализировать.",
            ["SkippedItemsLabel"] = "Пропущенные элементы очереди: {0}",
            ["ListItemPrefix"] = "- {0}",
            ["AndMoreLabel"] = "...и ещё {0}.",
            ["TotalFramesFormat"] = "{0}: {1}",
        },
    };

    public string QueueItemProgress { get; }
    public string SourceFilePath { get; }
    public string QueueItemSkipMsg { get; }
    public string AllQueueItemsFailed { get; }
    public string SkippedItemsLabel { get; }
    public string ListItemPrefix { get; }
    public string AndMoreLabel { get; }
    public string TotalFramesFormat { get; }
    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public AnalyzeSrcVideoCmdLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        QueueItemProgress = _d["QueueItemProgress"];
        SourceFilePath = _d["SourceFilePath"];
        QueueItemSkipMsg = _d["QueueItemSkipMsg"];
        AllQueueItemsFailed = _d["AllQueueItemsFailed"];
        SkippedItemsLabel = _d["SkippedItemsLabel"];
        ListItemPrefix = _d["ListItemPrefix"];
        AndMoreLabel = _d["AndMoreLabel"];
        TotalFramesFormat = _d["TotalFramesFormat"];
    }
}
