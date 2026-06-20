namespace OneColumnEncoder.Models;

public class VideoSourceQueueLangProviderM
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Video Source Queue ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Video Source Queue",
            ["Buttons.Import"] = "Import",
            ["SourceQueue.Sequence"] = "Sequence",
            ["ToolField.Path"] = "Path",
            ["SourceQueue.SelectFolderTitle"] = "Select video source queue folder",
            ["SourceQueue.Analyzed"] = "Queue source analysis completed. Filtered out {0} video(s) due to excessive differences.\n\nQueue data JSON:\n{1}\n\nExclusion list:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Queue source analysis completed. No videos were filtered out.\n\nQueue data JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copy Queue JSON Path",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copy Exclusion JSON Path",
            ["SourceQueue.OpenQueueJson"] = "Open Queue JSON",
            ["SourceQueue.OpenExcludedJson"] = "Open Exclusion JSON",
        },
        ["zh-cn"] = new()
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "视频源队列 ({0})",
            ["Tool.Source.VideoSrcQueue"] = "视频源队列",
            ["Buttons.Import"] = "导入",
            ["SourceQueue.Sequence"] = "序列",
            ["ToolField.Path"] = "路径",
            ["SourceQueue.SelectFolderTitle"] = "选择视频源队列文件夹",
            ["SourceQueue.Analyzed"] = "队列视频源分析已完成。因差异过大过滤掉 {0} 个视频。\n\n队列数据 JSON：\n{1}\n\n排除列表：\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "队列视频源分析已完成。未过滤掉视频。\n\n队列数据 JSON：\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "复制队列 JSON 路径",
            ["SourceQueue.CopyExcludedJsonPath"] = "复制排除列表 JSON 路径",
            ["SourceQueue.OpenQueueJson"] = "打开队列 JSON",
            ["SourceQueue.OpenExcludedJson"] = "打开排除列表 JSON",
        },
        ["zh-tw"] = new()
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "影片來源隊列 ({0})",
            ["Tool.Source.VideoSrcQueue"] = "影片來源隊列",
            ["Buttons.Import"] = "導入",
            ["SourceQueue.Sequence"] = "序列",
            ["ToolField.Path"] = "路徑",
            ["SourceQueue.SelectFolderTitle"] = "選擇視訊來源序列資料夾",
            ["SourceQueue.Analyzed"] = "隊列視訊來源分析已完成。因差異過大過濾掉 {0} 個視訊。\n\n隊列資料 JSON：\n{1}\n\n排除列表：\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "隊列視訊來源分析已完成。未過濾掉視訊。\n\n隊列資料 JSON：\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "複製隊列 JSON 路徑",
            ["SourceQueue.CopyExcludedJsonPath"] = "複製排除列表 JSON 路徑",
            ["SourceQueue.OpenQueueJson"] = "開啟隊列 JSON",
            ["SourceQueue.OpenExcludedJson"] = "開啟排除列表 JSON",
        }
    };

    static VideoSourceQueueLangProviderM()
    {
        Data["fr"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "File source vidéo ({0})",
            ["Tool.Source.VideoSrcQueue"] = "File source vidéo",
            ["Buttons.Import"] = "Importer",
            ["SourceQueue.Sequence"] = "Séquence",
            ["ToolField.Path"] = "Chemin",
            ["SourceQueue.SelectFolderTitle"] = "Sélectionner le dossier de la file d'attente",
            ["SourceQueue.Analyzed"] = "Analyse de la file d'attente terminée. {0} vidéo(s) exclue(s) en raison de différences excessives.\n\nJSON des données de file d'attente :\n{1}\n\nListe d'exclusion :\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Analyse de la file d'attente terminée. Aucune vidéo exclue.\n\nJSON des données de file d'attente :\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copier le chemin du JSON de file d'attente",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copier le chemin du JSON d'exclusion",
            ["SourceQueue.OpenQueueJson"] = "Ouvrir le JSON de file d'attente",
            ["SourceQueue.OpenExcludedJson"] = "Ouvrir le JSON d'exclusion",
        };
        Data["es"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Cola de fuentes de vídeo ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Cola de fuentes de vídeo",
            ["Buttons.Import"] = "Importar",
            ["SourceQueue.Sequence"] = "Secuencia",
            ["ToolField.Path"] = "Ruta",
            ["SourceQueue.SelectFolderTitle"] = "Seleccionar carpeta de cola de fuente de vídeo",
            ["SourceQueue.Analyzed"] = "Análisis de cola completado. Se filtraron {0} video(s) por diferencias excesivas.\n\nJSON de datos de cola:\n{1}\n\nLista de exclusión:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Análisis de cola completado. No se filtraron vídeos.\n\nJSON de datos de cola:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copiar ruta del JSON de cola",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copiar ruta del JSON de exclusión",
            ["SourceQueue.OpenQueueJson"] = "Abrir JSON de cola",
            ["SourceQueue.OpenExcludedJson"] = "Abrir JSON de exclusión",
        };
        Data["ja"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "ビデオソースキュー ({0})",
            ["Tool.Source.VideoSrcQueue"] = "ビデオソースキュー",
            ["Buttons.Import"] = "インポート",
            ["SourceQueue.Sequence"] = "シーケンス",
            ["ToolField.Path"] = "パス",
            ["SourceQueue.SelectFolderTitle"] = "ビデオソースキューのフォルダを選択",
            ["SourceQueue.Analyzed"] = "キューソース分析が完了しました。差異が大きいため {0} 個の動画を除外しました。\n\nキューデータ JSON:\n{1}\n\n除外リスト:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "キューソース分析が完了しました。除外された動画はありません。\n\nキューデータ JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "キュー JSON パスをコピー",
            ["SourceQueue.CopyExcludedJsonPath"] = "除外 JSON パスをコピー",
            ["SourceQueue.OpenQueueJson"] = "キュー JSON を開く",
            ["SourceQueue.OpenExcludedJson"] = "除外 JSON を開く",
        };
        Data["ru"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Очередь видеоисточников ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Очередь видеоисточников",
            ["Buttons.Import"] = "Импортировать",
            ["SourceQueue.Sequence"] = "Последовательность",
            ["ToolField.Path"] = "Путь",
            ["SourceQueue.SelectFolderTitle"] = "Выберите папку очереди видеоисточников",
            ["SourceQueue.Analyzed"] = "Анализ очереди завершён. Отфильтровано {0} видео из-за чрезмерных различий.\n\nJSON данных очереди:\n{1}\n\nСписок исключённых:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Анализ очереди завершён. Видео не отфильтрованы.\n\nJSON данных очереди:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Копировать путь к JSON очереди",
            ["SourceQueue.CopyExcludedJsonPath"] = "Копировать путь к JSON исключений",
            ["SourceQueue.OpenQueueJson"] = "Открыть JSON очереди",
            ["SourceQueue.OpenExcludedJson"] = "Открыть JSON исключений",
        };
    }

    private readonly Dictionary<string, string> _d;

    public string LanguageCode { get; }
    public string this[string key] => _d.TryGetValue(key, out var value) ? value : key;

    public string ToolSourceVideoSrcQueueWithCount { get; }
    public string ToolSourceVideoSrcQueue { get; }
    public string ButtonsImport { get; }
    public string SourceQueueSequence { get; }
    public string ToolFieldPath { get; }
    public string SourceQueueSelectFolderTitle { get; }
    public string SourceQueueAnalysisCompleted { get; }
    public string SourceQueueAnalysisCompletedNoExcluded { get; }
    public string SourceQueueCopyQueueJsonPath { get; }
    public string SourceQueueCopyExcludedJsonPath { get; }
    public string SourceQueueOpenQueueJson { get; }
    public string SourceQueueOpenExcludedJson { get; }

    public VideoSourceQueueLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        ToolSourceVideoSrcQueueWithCount = _d["Tool.Source.VideoSrcQueueWithCount"];
        ToolSourceVideoSrcQueue = _d["Tool.Source.VideoSrcQueue"];
        ButtonsImport = _d["Buttons.Import"];
        SourceQueueSequence = _d["SourceQueue.Sequence"];
        ToolFieldPath = _d["ToolField.Path"];
        SourceQueueSelectFolderTitle = _d["SourceQueue.SelectFolderTitle"];
        SourceQueueAnalysisCompleted = _d["SourceQueue.Analyzed"];
        SourceQueueAnalysisCompletedNoExcluded = _d["SourceQueue.AnalyzedNoEx"];
        SourceQueueCopyQueueJsonPath = _d["SourceQueue.CopyQueueJsonPath"];
        SourceQueueCopyExcludedJsonPath = _d["SourceQueue.CopyExcludedJsonPath"];
        SourceQueueOpenQueueJson = _d["SourceQueue.OpenQueueJson"];
        SourceQueueOpenExcludedJson = _d["SourceQueue.OpenExcludedJson"];
    }
}
