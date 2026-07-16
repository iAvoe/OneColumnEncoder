namespace OneColumnEncoder.Models.Lang;

public class FFProbeVideoAnalysisLangProvider
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
            ["NoVideoStreamInfo"] = "ffprobe не вернул информацию о видеопотоке",
        },
    };

    public string FfprobeNotFound { get; }
    public string InputVideoNotFound { get; }
    public string FfprobeTimedOut { get; }
    public string FfprobeFailedOrEmpty { get; }
    public string NoVideoStreamInfo { get; }
    public string LanguageCode { get; }
    private readonly Dictionary<string, string> _d;

    public string this[string key] => _d.TryGetValue(key, out var v) ? v : key;

    public FFProbeVideoAnalysisLangProvider(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];
        FfprobeNotFound = _d["FfprobeNotFound"];
        InputVideoNotFound = _d["InputVideoNotFound"];
        FfprobeTimedOut = _d["FfprobeTimedOut"];
        FfprobeFailedOrEmpty = _d["FfprobeFailedOrEmpty"];
        NoVideoStreamInfo = _d["NoVideoStreamInfo"];
    }
}
