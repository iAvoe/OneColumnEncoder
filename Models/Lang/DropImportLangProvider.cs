namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Localized strings for importing sources by drag and drop.
/// </summary>
public class DropImportLangProvider : LangProviderBase
{
    private static readonly Dictionary<string, Dictionary<string, string>> Data = new()
    {
        ["en"] = new()
        {
            ["Hint"] = "Release to import · {0}",
            ["HintPlain"] = "Release to import",
            ["Mode.Single"] = "Single-file mode",
            ["Mode.Queue"] = "Queue mode ({0} files)",
            ["ReplaceTitle"] = "Replace Video Source",
            ["ReplaceMessage"] = "A video source is already imported.\n\nConfirm: replace it and import as {0}\nCancel: keep the current source",
            ["NoVideoFile"] = "No video file was found in the dropped items.",
            ["ImportDisabled"] = "Video source import is currently disabled.\n\nImport ffprobe first; OneLineShotArgs upstream does not support queue mode."
        },
        ["zh-cn"] = new()
        {
            ["Hint"] = "松开以导入 · {0}",
            ["HintPlain"] = "松开以导入",
            ["Mode.Single"] = "单文件模式",
            ["Mode.Queue"] = "队列模式（{0} 个文件）",
            ["ReplaceTitle"] = "替换视频源",
            ["ReplaceMessage"] = "已经导入了视频源。\n\n确认：替换现有视频源，按 {0} 导入\n取消：保留当前视频源",
            ["NoVideoFile"] = "拖入的项目中没有找到视频文件。",
            ["ImportDisabled"] = "视频源导入当前处于禁用状态。\n\n请先导入 ffprobe；OneLineShotArgs 上游不支持队列模式。"
        },
        ["zh-tw"] = new()
        {
            ["Hint"] = "放開以匯入 · {0}",
            ["HintPlain"] = "放開以匯入",
            ["Mode.Single"] = "單文件模式",
            ["Mode.Queue"] = "隊列模式（{0} 個檔案）",
            ["ReplaceTitle"] = "替換影片源",
            ["ReplaceMessage"] = "已經匯入了影片源。\n\n確認：替換現有影片源，依 {0} 匯入\n取消：保留目前影片源",
            ["NoVideoFile"] = "拖入的項目中沒有找到影片檔案。",
            ["ImportDisabled"] = "影片源匯入目前處於停用狀態。\n\n請先匯入 ffprobe；OneLineShotArgs 上游不支援隊列模式。"
        }
    };

    static DropImportLangProvider()
    {
        Data["fr"] = new(Data["en"])
        {
            ["Hint"] = "Relâcher pour importer · {0}",
            ["HintPlain"] = "Relâcher pour importer",
            ["Mode.Single"] = "Mode fichier unique",
            ["Mode.Queue"] = "Mode file d'attente ({0} fichiers)",
            ["ReplaceTitle"] = "Remplacer la source vidéo",
            ["ReplaceMessage"] = "Une source vidéo est déjà importée.\n\nConfirmer : la remplacer et importer en {0}\nAnnuler : conserver la source actuelle",
            ["NoVideoFile"] = "Aucun fichier vidéo trouvé dans les éléments déposés.",
            ["ImportDisabled"] = "L'import de source vidéo est actuellement désactivé.\n\nImportez d'abord ffprobe ; l'outil amont OneLineShotArgs ne prend pas en charge le mode file d'attente."
        };
        Data["es"] = new(Data["en"])
        {
            ["Hint"] = "Suelta para importar · {0}",
            ["HintPlain"] = "Suelta para importar",
            ["Mode.Single"] = "Modo archivo único",
            ["Mode.Queue"] = "Modo cola ({0} archivos)",
            ["ReplaceTitle"] = "Reemplazar fuente de vídeo",
            ["ReplaceMessage"] = "Ya hay una fuente de vídeo importada.\n\nConfirmar: reemplazarla e importar como {0}\nCancelar: conservar la fuente actual",
            ["NoVideoFile"] = "No se encontró ningún archivo de vídeo en los elementos soltados.",
            ["ImportDisabled"] = "La importación de fuentes de vídeo está desactivada.\n\nImporta ffprobe primero; el upstream OneLineShotArgs no admite el modo cola."
        };
        Data["ja"] = new(Data["en"])
        {
            ["Hint"] = "ドロップして取り込み · {0}",
            ["HintPlain"] = "ドロップして取り込み",
            ["Mode.Single"] = "単一ファイルモード",
            ["Mode.Queue"] = "キューモード（{0} ファイル）",
            ["ReplaceTitle"] = "動画ソースを置換",
            ["ReplaceMessage"] = "動画ソースはすでに取り込まれています。\n\n確認：置換して {0} として取り込む\nキャンセル：現在のソースを保持",
            ["NoVideoFile"] = "ドロップされた項目に動画ファイルが見つかりません。",
            ["ImportDisabled"] = "動画ソースの取り込みは現在無効です。\n\n先に ffprobe を取り込んでください。OneLineShotArgs 上流はキューモードに対応していません。"
        };
        Data["ru"] = new(Data["en"])
        {
            ["Hint"] = "Отпустите для импорта · {0}",
            ["HintPlain"] = "Отпустите для импорта",
            ["Mode.Single"] = "Режим одного файла",
            ["Mode.Queue"] = "Режим очереди ({0} файлов)",
            ["ReplaceTitle"] = "Заменить источник видео",
            ["ReplaceMessage"] = "Источник видео уже импортирован.\n\nПодтвердить: заменить и импортировать как {0}\nОтмена: оставить текущий источник",
            ["NoVideoFile"] = "В перетащенных элементах не найден видеофайл.",
            ["ImportDisabled"] = "Импорт источника видео сейчас отключён.\n\nСначала импортируйте ffprobe; вышестоящий инструмент OneLineShotArgs не поддерживает режим очереди."
        };
        Data["de"] = new(Data["en"])
        {
            ["Hint"] = "Zum Importieren loslassen · {0}",
            ["HintPlain"] = "Zum Importieren loslassen",
            ["Mode.Single"] = "Einzeldatei-Modus",
            ["Mode.Queue"] = "Warteschlangen-Modus ({0} Dateien)",
            ["ReplaceTitle"] = "Videoquelle ersetzen",
            ["ReplaceMessage"] = "Es ist bereits eine Videoquelle importiert.\n\nBestätigen: ersetzen und als {0} importieren\nAbbrechen: aktuelle Quelle behalten",
            ["NoVideoFile"] = "In den abgelegten Elementen wurde keine Videodatei gefunden.",
            ["ImportDisabled"] = "Der Import von Videoquellen ist derzeit deaktiviert.\n\nImportieren Sie zuerst ffprobe; das Upstream OneLineShotArgs unterstützt den Warteschlangen-Modus nicht."
        };
        Data["ko"] = new(Data["en"])
        {
            ["Hint"] = "놓아서 가져오기 · {0}",
            ["HintPlain"] = "놓아서 가져오기",
            ["Mode.Single"] = "단일 파일 모드",
            ["Mode.Queue"] = "큐 모드 ({0}개 파일)",
            ["ReplaceTitle"] = "비디오 소스 바꾸기",
            ["ReplaceMessage"] = "비디오 소스가 이미 가져와져 있습니다.\n\n확인: 기존 소스를 바꾸고 {0}(으)로 가져오기\n취소: 현재 소스 유지",
            ["NoVideoFile"] = "끌어온 항목에서 비디오 파일을 찾지 못했습니다.",
            ["ImportDisabled"] = "비디오 소스 가져오기가 현재 비활성화되어 있습니다.\n\n먼저 ffprobe를 가져오세요. OneLineShotArgs 업스트림은 큐 모드를 지원하지 않습니다."
        };
        Data["pt-br"] = new(Data["en"])
        {
            ["Hint"] = "Solte para importar · {0}",
            ["HintPlain"] = "Solte para importar",
            ["Mode.Single"] = "Modo arquivo único",
            ["Mode.Queue"] = "Modo fila ({0} arquivos)",
            ["ReplaceTitle"] = "Substituir fonte de vídeo",
            ["ReplaceMessage"] = "Já existe uma fonte de vídeo importada.\n\nConfirmar: substituir e importar como {0}\nCancelar: manter a fonte atual",
            ["NoVideoFile"] = "Nenhum arquivo de vídeo foi encontrado nos itens soltos.",
            ["ImportDisabled"] = "A importação de fontes de vídeo está desativada no momento.\n\nImporte o ffprobe primeiro; o upstream OneLineShotArgs não suporta o modo fila."
        };
    }

    public string Hint { get; }
    public string HintPlain { get; }
    public string ModeSingle { get; }
    public string ModeQueue { get; }
    public string ReplaceTitle { get; }
    public string ReplaceMessage { get; }
    public string NoVideoFile { get; }
    public string ImportDisabled { get; }

    public DropImportLangProvider(string languageCode) : base(languageCode, Data)
    {
        Hint = this["Hint"];
        HintPlain = this["HintPlain"];
        ModeSingle = this["Mode.Single"];
        ModeQueue = this["Mode.Queue"];
        ReplaceTitle = this["ReplaceTitle"];
        ReplaceMessage = this["ReplaceMessage"];
        NoVideoFile = this["NoVideoFile"];
        ImportDisabled = this["ImportDisabled"];
    }
}
