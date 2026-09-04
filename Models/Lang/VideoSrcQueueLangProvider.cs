namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for video source queues.
/// </summary>
public class SrcQueueLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["SourceQueue.SelectFolderTitle"] = "Select video source queue folder",
            ["SourceQueue.EmptyFolderWarnMessage"] = "No video files were selected. Please choose at least one video source.",
            ["SourceQueue.MixedFolderErrorMessage"] = "Selected files must be in the same folder.",
            ["SourceQueue.Analyzed"] = "Queue source analysis completed. Filtered out {0} video(s) due to excessive differences.\n\nQueue data JSON:\n{1}\n\nExclusion list:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Queue source analysis completed. No videos were filtered out.\n\nQueue data JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copy Queue JSON Path",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copy Exclusion JSON Path",
            ["SourceQueue.OpenQueueJson"] = "Open Queue JSON",
            ["SourceQueue.OpenExcludedJson"] = "Open Exclusion JSON",
        },
        ["zh-cn"] = new()
        {
            ["SourceQueue.SelectFolderTitle"] = "选择视频源队列文件夹",
            ["SourceQueue.EmptyFolderWarnMessage"] = "未选择视频文件。请选择至少一个视频源。",
            ["SourceQueue.MixedFolderErrorMessage"] = "所选文件必须位于同一文件夹。",
            ["SourceQueue.Analyzed"] = "队列视频源分析已完成。因差异过大过滤掉 {0} 个视频。\n\n队列数据 JSON：\n{1}\n\n排除列表：\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "队列视频源分析已完成。未过滤掉视频。\n\n队列数据 JSON：\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "复制队列 JSON 路径",
            ["SourceQueue.CopyExcludedJsonPath"] = "复制排除列表 JSON 路径",
            ["SourceQueue.OpenQueueJson"] = "打开队列 JSON",
            ["SourceQueue.OpenExcludedJson"] = "打开排除列表 JSON",
        },
        ["zh-tw"] = new()
        {
            ["SourceQueue.SelectFolderTitle"] = "選擇視訊來源序列資料夾",
            ["SourceQueue.EmptyFolderWarnMessage"] = "未選擇視訊檔。請至少選擇一個視訊來源。",
            ["SourceQueue.MixedFolderErrorMessage"] = "所選檔案必須位於同一資料夾。",
            ["SourceQueue.Analyzed"] = "隊列視訊來源分析已完成。因差異過大過濾掉 {0} 個視訊。\n\n隊列資料 JSON：\n{1}\n\n排除列表：\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "隊列視訊來源分析已完成。未過濾掉視訊。\n\n隊列資料 JSON：\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "複製隊列 JSON 路徑",
            ["SourceQueue.CopyExcludedJsonPath"] = "複製排除列表 JSON 路徑",
            ["SourceQueue.OpenQueueJson"] = "開啟隊列 JSON",
            ["SourceQueue.OpenExcludedJson"] = "開啟排除列表 JSON",
        }
    };

    static SrcQueueLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "Sélectionner le dossier de la file d'attente",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Aucun fichier vidéo n'a été sélectionné. Veuillez choisir au moins une source vidéo.",
            ["SourceQueue.MixedFolderErrorMessage"] = "Les fichiers sélectionnés doivent être dans le même dossier.",
            ["SourceQueue.Analyzed"] = "Analyse de la file d'attente terminée. {0} vidéo(s) exclue(s) en raison de différences excessives.\n\nJSON des données de file d'attente :\n{1}\n\nListe d'exclusion :\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Analyse de la file d'attente terminée. Aucune vidéo exclue.\n\nJSON des données de file d'attente :\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copier le chemin du JSON de file d'attente",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copier le chemin du JSON d'exclusion",
            ["SourceQueue.OpenQueueJson"] = "Ouvrir le JSON de file d'attente",
            ["SourceQueue.OpenExcludedJson"] = "Ouvrir le JSON d'exclusion",
        };
        Data["es"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "Seleccionar carpeta de cola de fuente de vídeo",
            ["SourceQueue.EmptyFolderWarnMessage"] = "No se seleccionaron archivos de vídeo. Elige al menos una fuente de vídeo.",
            ["SourceQueue.MixedFolderErrorMessage"] = "Los archivos seleccionados deben estar en la misma carpeta.",
            ["SourceQueue.Analyzed"] = "Análisis de cola completado. Se filtraron {0} video(s) por diferencias excesivas.\n\nJSON de datos de cola:\n{1}\n\nLista de exclusión:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Análisis de cola completado. No se filtraron vídeos.\n\nJSON de datos de cola:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copiar ruta del JSON de cola",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copiar ruta del JSON de exclusión",
            ["SourceQueue.OpenQueueJson"] = "Abrir JSON de cola",
            ["SourceQueue.OpenExcludedJson"] = "Abrir JSON de exclusión",
        };
        Data["ja"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "ビデオソースキューのフォルダを選択",
            ["SourceQueue.EmptyFolderWarnMessage"] = "動画ファイルが選択されていません。少なくとも1つの動画ソースを選択してください。",
            ["SourceQueue.MixedFolderErrorMessage"] = "選択したファイルは同じフォルダ内にある必要があります。",
            ["SourceQueue.Analyzed"] = "キューソース分析が完了しました。差異が大きいため {0} 個の動画を除外しました。\n\nキューデータ JSON:\n{1}\n\n除外リスト:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "キューソース分析が完了しました。除外された動画はありません。\n\nキューデータ JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "キュー JSON パスをコピー",
            ["SourceQueue.CopyExcludedJsonPath"] = "除外 JSON パスをコピー",
            ["SourceQueue.OpenQueueJson"] = "キュー JSON を開く",
            ["SourceQueue.OpenExcludedJson"] = "除外 JSON を開く",
        };
        Data["ru"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "Выберите папку очереди видеоисточников",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Видеофайлы не выбраны. Выберите хотя бы один источник видео.",
            ["SourceQueue.MixedFolderErrorMessage"] = "Выбранные файлы должны находиться в одной папке.",
            ["SourceQueue.Analyzed"] = "Анализ очереди завершён. Отфильтровано {0} видео из-за чрезмерных различий.\n\nJSON данных очереди:\n{1}\n\nСписок исключённых:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Анализ очереди завершён. Видео не отфильтрованы.\n\nJSON данных очереди:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Копировать путь к JSON очереди",
            ["SourceQueue.CopyExcludedJsonPath"] = "Копировать путь к JSON исключений",
            ["SourceQueue.OpenQueueJson"] = "Открыть JSON очереди",
            ["SourceQueue.OpenExcludedJson"] = "Открыть JSON исключений",
        };
        Data["de"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "Videoquellen-Warteschlangenordner wählen",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Keine Videodateien ausgewählt. Bitte mindestens eine Videoquelle wählen.",
            ["SourceQueue.MixedFolderErrorMessage"] = "Die ausgewählten Dateien müssen im selben Ordner liegen.",
            ["SourceQueue.Analyzed"] = "Warteschlangenanalyse abgeschlossen. {0} Video(s) wegen übermäßiger Unterschiede gefiltert.\n\nJSON-Daten:\n{1}\n\nAusschlussliste:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Warteschlangenanalyse abgeschlossen. Keine Videos gefiltert.\n\nJSON-Daten:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "JSON-Pfad kopieren",
            ["SourceQueue.CopyExcludedJsonPath"] = "Ausschluss-JSON-Pfad kopieren",
            ["SourceQueue.OpenQueueJson"] = "JSON öffnen",
            ["SourceQueue.OpenExcludedJson"] = "Ausschluss-JSON öffnen",
        };
        Data["ko"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "비디오 소스 대기열 폴더 선택",
            ["SourceQueue.EmptyFolderWarnMessage"] = "동영상 파일이 선택되지 않았습니다. 동영상 소스를 하나 이상 선택하세요.",
            ["SourceQueue.MixedFolderErrorMessage"] = "선택한 파일은 같은 폴더에 있어야 합니다.",
            ["SourceQueue.Analyzed"] = "대기열 소스 분석 완료. 차이가 너무 커서 동영상 {0}개를 걸러냈습니다.\n\n대기열 데이터 JSON:\n{1}\n\n제외 목록:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "대기열 소스 분석 완료. 걸러낸 동영상이 없습니다.\n\n대기열 데이터 JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "대기열 JSON 경로 복사",
            ["SourceQueue.CopyExcludedJsonPath"] = "제외 JSON 경로 복사",
            ["SourceQueue.OpenQueueJson"] = "대기열 JSON 열기",
            ["SourceQueue.OpenExcludedJson"] = "제외 JSON 열기",
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["SourceQueue.SelectFolderTitle"] = "Selecionar pasta da fila de fontes de vídeo",
            ["SourceQueue.EmptyFolderWarnMessage"] = "Nenhum arquivo de vídeo foi selecionado. Escolha pelo menos uma fonte de vídeo.",
            ["SourceQueue.MixedFolderErrorMessage"] = "Os arquivos selecionados devem estar na mesma pasta.",
            ["SourceQueue.Analyzed"] = "Análise da fila concluída. {0} vídeo(s) filtrado(s) por diferenças excessivas.\n\nJSON de dados da fila:\n{1}\n\nLista de exclusão:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Análise da fila concluída. Nenhum vídeo foi filtrado.\n\nJSON de dados da fila:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copiar caminho do JSON da fila",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copiar caminho do JSON de exclusão",
            ["SourceQueue.OpenQueueJson"] = "Abrir JSON da fila",
            ["SourceQueue.OpenExcludedJson"] = "Abrir JSON de exclusão",
        };
    }

    public string ToolSourceSrcQueueWithCount { get; }
    public string ToolSourceSrcQueue { get; }
    public string SourceQueueSequence { get; }
    public string ToolFieldPath { get; }
    public string SourceQueueSelectFolderTitle { get; }
    public string SourceQueueAnalysisCompleted { get; }
    public string SourceQueueAnalysisCompletedNoExcluded { get; }
    public string SourceQueueCopyQueueJsonPath { get; }
    public string SourceQueueCopyExcludedJsonPath { get; }
    public string SourceQueueOpenQueueJson { get; }
    public string SourceQueueOpenExcludedJson { get; }

    public SrcQueueLangProvider(string languageCode) : base(languageCode, Data)
    {
        ToolSourceSrcQueueWithCount = this["SrcQueueWithCount"];
        ToolSourceSrcQueue = this["SrcQueue"];
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
