namespace OneColumnEncoder.Models.Lang;

public class FFProbeVideoAnalysisLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe does not exist: {0}",
            ["InputVideoNotFound"] = "Input video does not exist: {0}",
            ["FfprobeTimedOut"] = "ffprobe timed out while analyzing the source video",
            ["FfprobeFailedOrEmpty"] = "ffprobe failed or returned no valid data",
            ["NoVideoStreamInfo"] = "ffprobe returned no video stream information",
        },
        ["zh-cn"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe 不存在：{0}",
            ["InputVideoNotFound"] = "输入视频不存在：{0}",
            ["FfprobeTimedOut"] = "ffprobe 分析源视频超时",
            ["FfprobeFailedOrEmpty"] = "ffprobe 执行失败或未返回有效数据",
            ["NoVideoStreamInfo"] = "ffprobe 未返回任何视频流信息",
        },
        ["zh-tw"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe 不存在：{0}",
            ["InputVideoNotFound"] = "輸入影片不存在：{0}",
            ["FfprobeTimedOut"] = "ffprobe 分析來源影片逾時。",
            ["FfprobeFailedOrEmpty"] = "ffprobe 執行失敗或未傳回有效資料",
            ["NoVideoStreamInfo"] = "ffprobe 未傳回任何視訊串流資訊",
        },
        ["fr"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe introuvable : {0}",
            ["InputVideoNotFound"] = "Vidéo source introuvable : {0}",
            ["FfprobeTimedOut"] = "ffprobe a expiré lors de l'analyse de la vidéo source",
            ["FfprobeFailedOrEmpty"] = "ffprobe a échoué ou n'a retourné aucune donnée valide",
            ["NoVideoStreamInfo"] = "ffprobe n'a retourné aucune information de flux vidéo",
        },
        ["es"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe no existe: {0}",
            ["InputVideoNotFound"] = "El video de entrada no existe: {0}",
            ["FfprobeTimedOut"] = "ffprobe agotó el tiempo de espera al analizar el video de origen",
            ["FfprobeFailedOrEmpty"] = "ffprobe falló o no devolvió datos válidos",
            ["NoVideoStreamInfo"] = "ffprobe no devolvió información de flujo de video",
        },
        ["ja"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe が見つかりません: {0}",
            ["InputVideoNotFound"] = "入力動画が見つかりません: {0}",
            ["FfprobeTimedOut"] = "ffprobe がソース動画の解析中にタイムアウトしました",
            ["FfprobeFailedOrEmpty"] = "ffprobe が失敗したか、有効なデータを返しませんでした",
            ["NoVideoStreamInfo"] = "ffprobe がビデオストリーム情報を返しませんでした",
        },
        ["ru"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe не найден: {0}",
            ["InputVideoNotFound"] = "Входное видео не найдено: {0}",
            ["FfprobeTimedOut"] = "ffprobe превысил время ожидания при анализе исходного видео",
            ["FfprobeFailedOrEmpty"] = "ffprobe завершился ошибкой или не вернул допустимых данных",
            ["NoVideoStreamInfo"] = "ffprobe не вернул информацию о видеопотоке"
        },
        ["de"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe existiert nicht: {0}",
            ["InputVideoNotFound"] = "Eingabevideo existiert nicht: {0}",
            ["FfprobeTimedOut"] = "ffprobe beim Analysieren des Quellvideos zeitüberschritten",
            ["FfprobeFailedOrEmpty"] = "ffprobe fehlgeschlagen oder keine gültigen Daten zurückgegeben",
            ["NoVideoStreamInfo"] = "ffprobe hat keine Videostream-Informationen zurückgegeben",
        },
        ["ko"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe가 존재하지 않습니다: {0}",
            ["InputVideoNotFound"] = "입력 비디오가 존재하지 않습니다: {0}",
            ["FfprobeTimedOut"] = "ffprobe가 소스 비디오를 분석하는 동안 시간 초과되었습니다",
            ["FfprobeFailedOrEmpty"] = "ffprobe가 실패했거나 유효한 데이터를 반환하지 않았습니다",
            ["NoVideoStreamInfo"] = "ffprobe가 비디오 스트림 정보를 반환하지 않았습니다",
        },
        ["pt-br"] = new()
        {
            ["FfprobeNotFound"] = "ffprobe.exe não existe: {0}",
            ["InputVideoNotFound"] = "Vídeo de entrada não existe: {0}",
            ["FfprobeTimedOut"] = "ffprobe esgotou o tempo ao analisar o vídeo fonte",
            ["FfprobeFailedOrEmpty"] = "ffprobe falhou ou não retornou dados válidos",
            ["NoVideoStreamInfo"] = "ffprobe não retornou informações de stream de vídeo",
        },
    };

    public string FfprobeNotFound { get; }
    public string InputVideoNotFound { get; }
    public string FfprobeTimedOut { get; }
    public string FfprobeFailedOrEmpty { get; }
    public string NoVideoStreamInfo { get; }
    public FFProbeVideoAnalysisLangProvider(string languageCode) : base(languageCode, Data)
    {
        FfprobeNotFound = this["FfprobeNotFound"];
        InputVideoNotFound = this["InputVideoNotFound"];
        FfprobeTimedOut = this["FfprobeTimedOut"];
        FfprobeFailedOrEmpty = this["FfprobeFailedOrEmpty"];
        NoVideoStreamInfo = this["NoVideoStreamInfo"];
    }
}
