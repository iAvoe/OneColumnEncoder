namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for video source queues.
/// </summary>
public class VideoSrcQueueLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Video Source Queue ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Video Source Queue",
            ["SourceQueue.Sequence"] = "Sequence",
            ["ToolField.Path"] = "Path",
            ["SourceQueue.SelectFolderTitle"] = "Select video source queue folder",
            ["SourceQueue.EmptyFolderWarnMessage"] = "No video files were found in the selected folder. Please choose a folder that contains at least one video.",
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
            ["SourceQueue.Sequence"] = "序列",
            ["ToolField.Path"] = "路径",
            ["SourceQueue.SelectFolderTitle"] = "选择视频源队列文件夹",
            ["SourceQueue.EmptyFolderWarnMessage"] = "所选文件夹中没有找到视频文件。请选择至少包含一个视频的文件夹。",
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
            ["SourceQueue.Sequence"] = "序列",
            ["ToolField.Path"] = "路徑",
            ["SourceQueue.SelectFolderTitle"] = "選擇視訊來源序列資料夾",
            ["SourceQueue.EmptyFolderWarnMessage"] = "所選資料夾中沒有找到視訊檔。請選擇至少包含一個視訊的資料夾。",
            ["SourceQueue.Analyzed"] = "隊列視訊來源分析已完成。因差異過大過濾掉 {0} 個視訊。\n\n隊列資料 JSON：\n{1}\n\n排除列表：\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "隊列視訊來源分析已完成。未過濾掉視訊。\n\n隊列資料 JSON：\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "複製隊列 JSON 路徑",
            ["SourceQueue.CopyExcludedJsonPath"] = "複製排除列表 JSON 路徑",
            ["SourceQueue.OpenQueueJson"] = "開啟隊列 JSON",
            ["SourceQueue.OpenExcludedJson"] = "開啟排除列表 JSON",
        }
    };

    static VideoSrcQueueLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "File source vidéo ({0})",
            ["Tool.Source.VideoSrcQueue"] = "File source vidéo",
            ["SourceQueue.Sequence"] = "Séquence",
            ["ToolField.Path"] = "Chemin",
            ["SourceQueue.SelectFolderTitle"] = "Sélectionner le dossier de la file d'attente",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Aucun fichier vidéo n'a été trouvé dans le dossier sélectionné. Veuillez choisir un dossier contenant au moins une vidéo.",
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
            ["SourceQueue.Sequence"] = "Secuencia",
            ["ToolField.Path"] = "Ruta",
            ["SourceQueue.SelectFolderTitle"] = "Seleccionar carpeta de cola de fuente de vídeo",
            ["SourceQueue.EmptyFolderWarnMessage"] = "No se encontraron archivos de vídeo en la carpeta seleccionada. Elige una carpeta que contenga al menos un vídeo.",
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
            ["SourceQueue.Sequence"] = "シーケンス",
            ["ToolField.Path"] = "パス",
            ["SourceQueue.SelectFolderTitle"] = "ビデオソースキューのフォルダを選択",
            ["SourceQueue.EmptyFolderWarnMessage"] = "選択したフォルダに動画ファイルが見つかりませんでした。少なくとも1つの動画を含むフォルダを選択してください。",
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
            ["SourceQueue.Sequence"] = "Последовательность",
            ["ToolField.Path"] = "Путь",
            ["SourceQueue.SelectFolderTitle"] = "Выберите папку очереди видеоисточников",
            ["SourceQueue.EmptyFolderWarnMessage"] = "В выбранной папке не найдено видеофайлов. Выберите папку, содержащую как минимум одно видео.",
            ["SourceQueue.Analyzed"] = "Анализ очереди завершён. Отфильтровано {0} видео из-за чрезмерных различий.\n\nJSON данных очереди:\n{1}\n\nСписок исключённых:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Анализ очереди завершён. Видео не отфильтрованы.\n\nJSON данных очереди:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Копировать путь к JSON очереди",
            ["SourceQueue.CopyExcludedJsonPath"] = "Копировать путь к JSON исключений",
            ["SourceQueue.OpenQueueJson"] = "Открыть JSON очереди",
            ["SourceQueue.OpenExcludedJson"] = "Открыть JSON исключений",
        };
        Data["de"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Videoquellen-Warteschlange ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Videoquellen-Warteschlange",
            ["Buttons.Import"] = "Importieren",
            ["SourceQueue.Sequence"] = "Sequenz",
            ["ToolField.Path"] = "Pfad",
            ["SourceQueue.SelectFolderTitle"] = "Videoquellen-Warteschlangenordner wählen",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Keine Videodateien im gewählten Ordner. Bitte einen Ordner mit mindestens einem Video wählen.",
            ["SourceQueue.Analyzed"] = "Warteschlangenanalyse abgeschlossen. {0} Video(s) wegen übermäßiger Unterschiede gefiltert.\n\nJSON-Daten:\n{1}\n\nAusschlussliste:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Warteschlangenanalyse abgeschlossen. Keine Videos gefiltert.\n\nJSON-Daten:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "JSON-Pfad kopieren",
            ["SourceQueue.CopyExcludedJsonPath"] = "Ausschluss-JSON-Pfad kopieren",
            ["SourceQueue.OpenQueueJson"] = "JSON öffnen",
            ["SourceQueue.OpenExcludedJson"] = "Ausschluss-JSON öffnen",
        };
        Data["ko"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "비디오 소스 대기열 ({0})",
            ["Tool.Source.VideoSrcQueue"] = "비디오 소스 대기열",
            ["Buttons.Import"] = "가져오기",
            ["SourceQueue.Sequence"] = "순번",
            ["ToolField.Path"] = "경로",
            ["SourceQueue.SelectFolderTitle"] = "비디오 소스 대기열 폴더 선택",
            ["SourceQueue.EmptyFolderWarnMessage"] = "선택한 폴더에서 동영상 파일을 찾을 수 없습니다. 동영상이 하나 이상 포함된 폴더를 선택하세요.",
            ["SourceQueue.Analyzed"] = "대기열 소스 분석 완료. 차이가 너무 커서 동영상 {0}개를 걸러냈습니다.\n\n대기열 데이터 JSON:\n{1}\n\n제외 목록:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "대기열 소스 분석 완료. 걸러낸 동영상이 없습니다.\n\n대기열 데이터 JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "대기열 JSON 경로 복사",
            ["SourceQueue.CopyExcludedJsonPath"] = "제외 JSON 경로 복사",
            ["SourceQueue.OpenQueueJson"] = "대기열 JSON 열기",
            ["SourceQueue.OpenExcludedJson"] = "제외 JSON 열기",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Fila de fontes de vídeo ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Fila de fontes de vídeo",
            ["SourceQueue.Sequence"] = "Sequência",
            ["ToolField.Path"] = "Caminho",
            ["SourceQueue.SelectFolderTitle"] = "Selecionar pasta da fila de fontes de vídeo",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Nenhum arquivo de vídeo foi encontrado na pasta selecionada. Escolha uma pasta que contenha pelo menos um vídeo.",
            ["SourceQueue.Analyzed"] = "Análise da fila concluída. {0} vídeo(s) filtrado(s) por diferenças excessivas.\n\nJSON de dados da fila:\n{1}\n\nLista de exclusão:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Análise da fila concluída. Nenhum vídeo foi filtrado.\n\nJSON de dados da fila:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copiar caminho do JSON da fila",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copiar caminho do JSON de exclusão",
            ["SourceQueue.OpenQueueJson"] = "Abrir JSON da fila",
            ["SourceQueue.OpenExcludedJson"] = "Abrir JSON de exclusão",
        };
    }

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

    public VideoSrcQueueLangProvider(string languageCode) : base(languageCode, Data)
    {
        ToolSourceVideoSrcQueueWithCount = this["Tool.Source.VideoSrcQueueWithCount"];
        ToolSourceVideoSrcQueue = this["Tool.Source.VideoSrcQueue"];
        ButtonsImport = this["Buttons.Import"];
        SourceQueueSequence = this["SourceQueue.Sequence"];
        ToolFieldPath = this["ToolField.Path"];
        SourceQueueSelectFolderTitle = this["SourceQueue.SelectFolderTitle"];
        SourceQueueAnalysisCompleted = this["SourceQueue.Analyzed"];
        SourceQueueAnalysisCompletedNoExcluded = this["SourceQueue.AnalyzedNoEx"];
        SourceQueueCopyQueueJsonPath = this["SourceQueue.CopyQueueJsonPath"];
        SourceQueueCopyExcludedJsonPath = this["SourceQueue.CopyExcludedJsonPath"];
        SourceQueueOpenQueueJson = this["SourceQueue.OpenQueueJson"];
        SourceQueueOpenExcludedJson = this["SourceQueue.OpenExcludedJson"];
    }
}
