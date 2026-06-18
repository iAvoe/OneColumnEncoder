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
        },
        ["zh-cn"] = new()
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "视频源队列 ({0})",
            ["Tool.Source.VideoSrcQueue"] = "视频源队列",
            ["Buttons.Import"] = "导入",
            ["SourceQueue.Sequence"] = "序列",
            ["ToolField.Path"] = "路径",
        },
        ["zh-tw"] = new()
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "影片來源佇列 ({0})",
            ["Tool.Source.VideoSrcQueue"] = "影片來源佇列",
            ["Buttons.Import"] = "導入",
            ["SourceQueue.Sequence"] = "序列",
            ["ToolField.Path"] = "路徑",
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
        };
        Data["es"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Cola de fuentes de vídeo ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Cola de fuentes de vídeo",
            ["Buttons.Import"] = "Importar",
            ["SourceQueue.Sequence"] = "Secuencia",
            ["ToolField.Path"] = "Ruta",
        };
        Data["ja"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "ビデオソースキュー ({0})",
            ["Tool.Source.VideoSrcQueue"] = "ビデオソースキュー",
            ["Buttons.Import"] = "インポート",
            ["SourceQueue.Sequence"] = "シーケンス",
            ["ToolField.Path"] = "パス",
        };
        Data["ru"] = new(Data["en"])
        {
            ["Tool.Source.VideoSrcQueueWithCount"] = "Очередь видеоисточников ({0})",
            ["Tool.Source.VideoSrcQueue"] = "Очередь видеоисточников",
            ["Buttons.Import"] = "Импортировать",
            ["SourceQueue.Sequence"] = "Последовательность",
            ["ToolField.Path"] = "Путь",
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

    public VideoSourceQueueLangProviderM(string languageCode)
    {
        LanguageCode = Data.ContainsKey(languageCode) ? languageCode : "en";
        _d = Data[LanguageCode];

        ToolSourceVideoSrcQueueWithCount = _d["Tool.Source.VideoSrcQueueWithCount"];
        ToolSourceVideoSrcQueue = _d["Tool.Source.VideoSrcQueue"];
        ButtonsImport = _d["Buttons.Import"];
        SourceQueueSequence = _d["SourceQueue.Sequence"];
        ToolFieldPath = _d["ToolField.Path"];
    }
}
