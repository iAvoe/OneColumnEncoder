namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Base class for language providers, including shared short operation words and fallback lookup for reusable UI labels.
/// 
/// To add a new language, follow the following table:
/// | File | Keys Translated |
/// |------|----------------|
/// | `LangProviderBase.cs` (this file) | 25 common UI terms + shared encoding mode labels(Confirm, Cancel, Add, Delete, Clear, Enable, Disable, On, Off, Collapse, Expand, etc.) |
/// | `AnalyzeSrcVideoCmdLangProvider.cs` | 9 keys |
/// | `AppUsageLangProvider.cs` | 38 keys |
/// | `AppConfLangProvider.cs` | 31 keys |
/// | `ConfirmDialogLangProvider.cs` | 7 keys |
/// | `ClipRangeSelectorLangProvider.cs` | 22 keys |
/// | `CpuSetsLangProvider.cs` | 6 keys |
/// | `SrcQueueLangProvider.cs` | 12 keys |
/// | `UILangProvider.cs` | ~100 keys(full UI) |
/// | `StartEncCmdLangProvider.cs` | 21 keys |
/// | `SrcReviserLangProvider.cs` | 11 keys |
/// | `SrcFilePickerLangProvider.cs` | 6 keys |
/// | `RepartLangProvider.cs` | 80 keys |
/// | `QueueSidebarLangProvider.cs` | 5 keys |
/// | `QueueEditorLangProvider.cs` | 1 key |
/// | `ParallelismConfLangProvider.cs` | 23 keys |
/// | `ImgABPvLangProvider.cs` | 38 keys |
/// | `VpyPreviewLangProvider.cs` | 18 keys |
/// | `FilterScribeModalLangProvider.cs` | 74 keys |
/// | `FilenameScribeModalLangProvider.cs` | 22 keys |
/// | `FFProbeVideoAnalysisLangProvider.cs` | 5 keys |
/// | `FFProbeSrcRevisionLangProvider.cs` | 6 keys |
/// | `EncodingMonitorModalLangProvider.cs` | 65 keys |
/// | `EncoderConfLangProvider.cs` | 42 keys |
/// | `UICaptionProvider.cs` | Add language code |
/// </summary>
/// <remarks>
/// Translation guidelines:
/// 
/// ! DO NOT TRANSLATE WINDOW TITLE, THEY MUST BE HARDCODED IN ENGLISH !
/// - The window title helps debugging, logging, lowering user support difficulty, and web searching for solutions
/// - Changing window title breaks simplicity and will give everyone a hard time
///
/// User profile:
/// - 100%: understand computer basics
/// - 50%: pro PC users ("live on their PC")
/// - 30%: developers, programmers, experienced IT & networking people
/// - ±5%: familiar with video encoding
///
/// Core principles:
/// - Avoid explaining to absolute beginners.
/// - Preserve key words and technical meaning; do not turn UI text into tutorials.
/// - Prefer the shortest natural and technically unambiguous wording.
///
/// Button & compact-control text:
/// - Stay close to or shorter than the English length; avoid exceeding it by 5+ letters where practical.
/// - Prefer standard abbreviations over dropping meaningful words.
/// - Keep the action and object recognizable.
/// - Never sacrifice grammatical correctness just to match English length.
/// - Take advantage noun as verb, verb as noun rules to compress where natural, i.e., "Export a sample clip" → "Clip sampling".
/// - Use concise wording where natural, e.g. "Operations" → "Ops.".
/// - Window size may increase (and can be the top pick); shortening is not the only option.
/// - If an ambiguous control lacks its expected HintPanel, report it to the project owner instead of forcing the translation to compensate.
///
/// Terminology:
/// - Prefer established technical terms over beginner-oriented paraphrases.
/// - Keep the same concept consistently translated across the UI.
/// - Keep common operation words consistent; do not vary them stylistically (e.g. choose "Add" or "Append", not both for the same operation).
/// - Use the most specific natural term when context requires it, e.g. "source" → "video source" / "audio source".
///
/// Context & explanations:
/// - Use the surrounding UI, button icons, group heading, and especially HintPanel to resolve ambiguity before adding words.
/// - Do not repeat information already conveyed elsewhere in the UI.
/// - Do not add definitions, parenthetical explanations, or "101" wording
///   for common computer/UI operations or terms clear from context.
/// - Encoding-specific concepts that may be unfamiliar belong in the HintPanel, not in an unnecessarily long control label.
///
/// Button icons:
/// - Despite testing shows button icons (from UI/SvgIconProvider) are very effective for conveying operations,
///   but this does not mean to cut the verb part of the button text away
/// </remarks>
public abstract class LangProviderBase
{
    protected readonly Dictionary<string, string> _d;
    public static string FFmpeg => "ffmpeg";
    public static string VapourSynth => "VapourSynth";
    public static string VS => "VS";
    public static string AviSynth => "AviSynth";
    public static string AVS => "AVS";
    public static string RemoveText => "🗙";
    public static string MoveUpText => "↑↑";
    public static string MoveDownText => "↓↓";
    public static string NAText => "N/A";
    public static string Hms => "h:m:s";
    public string Default => this["Default"];
    public string Enable => this["Enable"];
    public string Disable => this["Disable"];
    public string On => this["On"];
    public string Off => this["Off"];
    public string Confirm => this["Confirm"];
    public string Cancel => this["Cancel"];
    public string UndoText => this["Undo"];
    public string RedoText => this["Redo"];
    public string CutText => this["Cut"];
    public string CopyText => this["Copy"];
    public string PasteText => this["Paste"];
    public string DeleteText => this["Delete"];
    public string SelectAllText => this["Select All"];
    private static readonly Dictionary<string, Dictionary<string, string>> CommonData = new()
    {
        ["en"] = new()
        {
            ["Confirm"] = "Confirm",
            ["Cancel"] = "Cancel",
            ["Add"] = "Add",
            ["Delete"] = "Delete",
            ["Cut"] = "Cut",
            ["Copy"] = "Copy",
            ["Paste"] = "Paste",
            ["Clear"] = "Clear",
            ["Clear All"] = "Clear All",
            ["Edit"] = "Edit",
            ["Replace"] = "Replace",
            ["Import"] = "Import",
            ["Save"] = "Save",
            ["Close"] = "Close",
            ["Preview"] = "Preview",
            ["Fit"] = "Fit",
            ["Remove"] = "Remove",
            ["Reset"] = "Reset",
            ["Stop"] = "Stop",
            ["Undo"] = "Undo",
            ["Redo"] = "Redo",
            ["Select All"] = "Select All",
            ["Refresh"] = "Refresh",
            ["Enable"] = "Enable",
            ["Disable"] = "Disable",
            ["On"] = "On",
            ["Off"] = "Off",
            ["Collapse"] = "▲ Collapse",
            ["Expand"] = "▼ Expand",
            ["Default"] = "Default",
            ["Width"] = "Width",
            ["Height"] = "Height",
            ["EncMode.Single"] = "Single-file mode",
            ["EncMode.Queue"] = "Queue mode",
            ["EncMode.Concat"] = "Concat mode",
            ["EncMode.Repart"] = "Repart mode",
            ["SrcQueue"] = "📁 Video Src. Queue",
            ["SrcQueueWithCount"] = "📁 Video Src. Queue ({0})",
            ["SrcConcat"] = "∪ Video Src. Concat",
            ["SrcConcatWithCount"] = "∪ Video Src. Concat ({0})",
            ["SrcRepart"] = "📁 Video Src. Repart",
        },
        ["zh-cn"] = new()
        {
            ["Confirm"] = "确认",
            ["Cancel"] = "取消",
            ["Add"] = "添加",
            ["Delete"] = "删除",
            ["Cut"] = "剪切",
            ["Copy"] = "复制",
            ["Paste"] = "粘贴",
            ["Clear"] = "清空",
            ["Clear All"] = "全部移除",
            ["Edit"] = "编辑",
            ["Replace"] = "替换",
            ["Import"] = "导入",
            ["Save"] = "保存",
            ["Close"] = "关闭",
            ["Preview"] = "预览",
            ["Fit"] = "适应",
            ["Remove"] = "移除",
            ["Reset"] = "重置",
            ["Stop"] = "停",
            ["Undo"] = "撤销",
            ["Redo"] = "重做",
            ["Select All"] = "全选",
            ["Refresh"] = "刷新",
            ["Enable"] = "启用",
            ["Disable"] = "禁用",
            ["On"] = "开",
            ["Off"] = "关",
            ["Collapse"] = "▲ 折叠",
            ["Expand"] = "▼ 展开",
            ["Default"] = "默认",
            ["Width"] = "宽度",
            ["Height"] = "高度",
            ["EncMode.Single"] = "单文件模式",
            ["EncMode.Queue"] = "队列模式（Queue）",
            ["EncMode.Concat"] = "拼接模式（Concat）",
            ["EncMode.Repart"] = "重分集模式（Repart）",
            ["SrcQueue"] = "📁 视频源队列",
            ["SrcQueueWithCount"] = "📁 视频源队列 ({0})",
            ["SrcConcat"] = "∪ 视频源拼接",
            ["SrcConcatWithCount"] = "∪ 视频源拼接 ({0})",
            ["SrcRepart"] = "📁 视频源重分集",
        },
        ["zh-tw"] = new()
        {
            ["Confirm"] = "確認",
            ["Cancel"] = "取消",
            ["Add"] = "添加",
            ["Delete"] = "刪除",
            ["Cut"] = "剪下",
            ["Copy"] = "複製",
            ["Paste"] = "貼上",
            ["Clear"] = "清空",
            ["Clear All"] = "全部移除",
            ["Edit"] = "編輯",
            ["Replace"] = "替換",
            ["Import"] = "導入",
            ["Save"] = "保存",
            ["Close"] = "關閉",
            ["Preview"] = "預覽",
            ["Fit"] = "適應",
            ["Remove"] = "移除",
            ["Reset"] = "重置",
            ["Stop"] = "停",
            ["Undo"] = "撤銷",
            ["Redo"] = "重做",
            ["Select All"] = "全選",
            ["Refresh"] = "刷新",
            ["Enable"] = "啟用",
            ["Disable"] = "停用",
            ["On"] = "開",
            ["Off"] = "關",
            ["Collapse"] = "▲ 折叠",
            ["Expand"] = "▼ 展开",
            ["Default"] = "默認",
            ["Width"] = "寬度",
            ["Height"] = "高度",
            ["EncMode.Single"] = "單文件模式",
            ["EncMode.Queue"] = "隊列模式（Queue）",
            ["EncMode.Concat"] = "拼接模式（Concat）",
            ["EncMode.Repart"] = "重分集模式（Repart）",
            ["SrcQueue"] = "📁 視訊源隊列",
            ["SrcQueueWithCount"] = "📁 視訊源隊列 ({0})",
            ["SrcConcat"] = "∪ 視訊源拼接",
            ["SrcConcatWithCount"] = "∪ 視訊源拼接 ({0})",
            ["SrcRepart"] = "📁 視訊源重分集",
        },
        ["fr"] = new()
        {
            ["Confirm"] = "Confirmer",
            ["Cancel"] = "Annuler",
            ["Add"] = "Ajouter",
            ["Delete"] = "Supprimer",
            ["Cut"] = "Couper",
            ["Copy"] = "Copier",
            ["Paste"] = "Coller",
            ["Clear"] = "Effacer",
            ["Clear All"] = "Tout effacer",
            ["Edit"] = "Modifier",
            ["Replace"] = "Remplacer",
            ["Import"] = "Importer",
            ["Save"] = "Enregistrer",
            ["Close"] = "Fermer",
            ["Preview"] = "Aperçu",
            ["Fit"] = "Ajuster",
            ["Remove"] = "Retirer",
            ["Reset"] = "Réinitialiser",
            ["Stop"] = "Arrêt",
            ["Undo"] = "Annuler",
            ["Redo"] = "Rétablir",
            ["Select All"] = "Tout sélectionner",
            ["Refresh"] = "Rafraîchir",
            ["Enable"] = "Activer",
            ["Disable"] = "Désactiver",
            ["On"] = "Activé",
            ["Off"] = "Désactivé",
            ["Collapse"] = "▲ Réduire",
            ["Expand"] = "▼ Développer",
            ["Default"] = "Défaut",
            ["Width"] = "Largeur",
            ["Height"] = "Hauteur",
            ["EncMode.Single"] = "Mode fichier unique",
            ["EncMode.Queue"] = "Mode file d'attente (Queue)",
            ["EncMode.Concat"] = "Mode concat (Concat)",
            ["EncMode.Repart"] = "Mode repart (Repart)",
            ["SrcQueue"] = "📁 File d'attente vidéo",
            ["SrcQueueWithCount"] = "📁 File d'attente vidéo ({0})",
            ["SrcConcat"] = "∪ Concat source vidéo",
            ["SrcConcatWithCount"] = "∪ Concat source vidéo ({0})",
            ["SrcRepart"] = "📁 Répart. vidéo",
        },
        ["es"] = new()
        {
            ["Confirm"] = "Confirmar",
            ["Cancel"] = "Cancelar",
            ["Add"] = "Añadir",
            ["Delete"] = "Eliminar",
            ["Cut"] = "Cortar",
            ["Copy"] = "Copiar",
            ["Paste"] = "Pegar",
            ["Clear"] = "Limpiar",
            ["Clear All"] = "Borrar todo",
            ["Edit"] = "Editar",
            ["Replace"] = "Reemplazar",
            ["Import"] = "Importar",
            ["Save"] = "Guardar",
            ["Close"] = "Cerrar",
            ["Preview"] = "Vista previa",
            ["Fit"] = "Ajustar",
            ["Remove"] = "Quitar",
            ["Reset"] = "Restablecer",
            ["Stop"] = "Detener",
            ["Undo"] = "Deshacer",
            ["Redo"] = "Rehacer",
            ["Select All"] = "Seleccionar todo",
            ["Refresh"] = "Refrescar",
            ["Enable"] = "Habilitar",
            ["Disable"] = "Deshabilitar",
            ["On"] = "Activado",
            ["Off"] = "Desactivado",
            ["Collapse"] = "▲ Contraer",
            ["Expand"] = "▼ Expandir",
            ["Default"] = "Defecto",
            ["Width"] = "Ancho",
            ["Height"] = "Alto",
            ["EncMode.Single"] = "Modo de archivo único",
            ["EncMode.Queue"] = "Modo de cola (Queue)",
            ["EncMode.Concat"] = "Modo concat (Concat)",
            ["EncMode.Repart"] = "Modo repart (Repart)",
            ["SrcQueue"] = "📁 Cola de vídeo",
            ["SrcQueueWithCount"] = "📁 Cola de vídeo ({0})",
            ["SrcConcat"] = "∪ Concat vídeo",
            ["SrcConcatWithCount"] = "∪ Concat vídeo ({0})",
            ["SrcRepart"] = "📁 Repart. vídeo",
        },
        ["ja"] = new()
        {
            ["Confirm"] = "確認",
            ["Cancel"] = "キャンセル",
            ["Add"] = "追加",
            ["Delete"] = "削除",
            ["Cut"] = "切り取り",
            ["Copy"] = "コピー",
            ["Paste"] = "貼り付け",
            ["Clear"] = "クリア",
            ["Clear All"] = "すべてクリア",
            ["Edit"] = "編集",
            ["Replace"] = "置換",
            ["Import"] = "インポート",
            ["Save"] = "保存",
            ["Close"] = "閉じる",
            ["Preview"] = "プレビュー",
            ["Fit"] = "フィット",
            ["Remove"] = "削除",
            ["Reset"] = "リセット",
            ["Stop"] = "停止",
            ["Undo"] = "元に戻す",
            ["Redo"] = "やり直す",
            ["Select All"] = "すべて選択",
            ["Refresh"] = "更新",
            ["Enable"] = "有効化",
            ["Disable"] = "無効化",
            ["On"] = "オン",
            ["Off"] = "オフ",
            ["Collapse"] = "▲ 折りたたむ",
            ["Expand"] = "▼ 展開",
            ["Default"] = "デフォルト",
            ["Width"] = "幅",
            ["Height"] = "高さ",
            ["EncMode.Single"] = "単一ファイルモード",
            ["EncMode.Queue"] = "キューモード (Queue)",
            ["EncMode.Concat"] = "連結モード (Concat)",
            ["EncMode.Repart"] = "再分割モード (Repart)",
            ["SrcQueue"] = "📁 動画ソースキュー",
            ["SrcQueueWithCount"] = "📁 動画ソースキュー ({0})",
            ["SrcConcat"] = "∪ 動画ソース連結",
            ["SrcConcatWithCount"] = "∪ 動画ソース連結 ({0})",
            ["SrcRepart"] = "📁 映像ソース再分割",
        },
        ["ru"] = new()
        {
            ["Confirm"] = "Подтвердить",
            ["Cancel"] = "Отмена",
            ["Add"] = "Добавить",
            ["Delete"] = "Удалить",
            ["Cut"] = "Вырезать",
            ["Copy"] = "Копировать",
            ["Paste"] = "Вставить",
            ["Clear"] = "Очистить",
            ["Clear All"] = "Очистить всё",
            ["Edit"] = "Редактировать",
            ["Replace"] = "Заменить",
            ["Import"] = "Импортировать",
            ["Save"] = "Сохранить",
            ["Close"] = "Закрыть",
            ["Preview"] = "Предпросмотр",
            ["Fit"] = "По размеру",
            ["Remove"] = "Удалить",
            ["Reset"] = "Сброс",
            ["Stop"] = "Стоп",
            ["Undo"] = "Отменить",
            ["Redo"] = "Вернуть",
            ["Select All"] = "Выделить все",
            ["Refresh"] = "Обновить",
            ["Enable"] = "Включить",
            ["Disable"] = "Отключить",
            ["On"] = "Вкл.",
            ["Off"] = "Выкл.",
            ["Collapse"] = "▲ Свернуть",
            ["Expand"] = "▼ Развернуть",
            ["Default"] = "Умолчание",
            ["Width"] = "Ширина",
            ["Height"] = "Высота",
            ["EncMode.Single"] = "Режим одного файла",
            ["EncMode.Queue"] = "Режим очереди (Queue)",
            ["EncMode.Concat"] = "Режим конкатенации (Concat)",
            ["EncMode.Repart"] = "Режим репарта (Repart)",
            ["SrcQueue"] = "📁 Очередь видеоисточника",
            ["SrcQueueWithCount"] = "📁 Очередь видеоисточника ({0})",
            ["SrcConcat"] = "∪ Конкатенация видео",
            ["SrcConcatWithCount"] = "∪ Конкатенация видео ({0})",
            ["SrcRepart"] = "📁 Репарт видео",
        },
        ["de"] = new()
        {
            ["Confirm"] = "Bestätigen",
            ["Cancel"] = "Abbrechen",
            ["Add"] = "Hinzufügen",
            ["Delete"] = "Löschen",
            ["Cut"] = "Ausschneiden",
            ["Copy"] = "Kopieren",
            ["Paste"] = "Einfügen",
            ["Clear"] = "Leeren",
            ["Clear All"] = "Alles leeren",
            ["Edit"] = "Bearbeiten",
            ["Replace"] = "Ersetzen",
            ["Import"] = "Importieren",
            ["Save"] = "Speichern",
            ["Close"] = "Schließen",
            ["Preview"] = "Vorschau",
            ["Fit"] = "Anpassen",
            ["Remove"] = "Entfernen",
            ["Reset"] = "Zurücksetzen",
            ["Stop"] = "Stopp",
            ["Undo"] = "Zurück",
            ["Redo"] = "Wiederh.",
            ["Select All"] = "Alles auswählen",
            ["Refresh"] = "Aktualisieren",
            ["Enable"] = "Aktivieren",
            ["Disable"] = "Deaktivieren",
            ["On"] = "Ein",
            ["Off"] = "Aus",
            ["Collapse"] = "▲ Einklappen",
            ["Expand"] = "▼ Ausklappen",
            ["Default"] = "Standard",
            ["Width"] = "Breite",
            ["Height"] = "Höhe",
            ["EncMode.Single"] = "Einzeldatei-Modus",
            ["EncMode.Queue"] = "Warteschlangenmodus (Queue)",
            ["EncMode.Concat"] = "Concat-Modus",
            ["EncMode.Repart"] = "Repart-Modus",
            ["SrcQueue"] = "📁 Video-Wart.",
            ["SrcQueueWithCount"] = "📁 Video-Wart. ({0})",
            ["SrcConcat"] = "∪ Video-Concat",
            ["SrcConcatWithCount"] = "∪ Video-Concat ({0})",
            ["SrcRepart"] = "📁 Video-Neuteilung",
        },
        ["ko"] = new()
        {
            ["Confirm"] = "확인",
            ["Cancel"] = "취소",
            ["Add"] = "추가",
            ["Delete"] = "삭제",
            ["Cut"] = "잘라내기",
            ["Copy"] = "복사",
            ["Paste"] = "붙여넣기",
            ["Clear"] = "지우기",
            ["Clear All"] = "모두 지우기",
            ["Edit"] = "편집",
            ["Replace"] = "바꾸기",
            ["Import"] = "가져오기",
            ["Save"] = "저장",
            ["Close"] = "닫기",
            ["Preview"] = "미리보기",
            ["Fit"] = "맞추기",
            ["Remove"] = "제거",
            ["Reset"] = "초기화",
            ["Stop"] = "중지",
            ["Undo"] = "실행취소",
            ["Redo"] = "다시실행",
            ["Select All"] = "모두 선택",
            ["Refresh"] = "새로 고치다",
            ["Enable"] = "활성화",
            ["Disable"] = "비활성화",
            ["On"] = "켬",
            ["Off"] = "끔",
            ["Collapse"] = "▲ 접기",
            ["Expand"] = "▼ 펼치기",
            ["Default"] = "기본",
            ["Width"] = "너비",
            ["Height"] = "높이",
            ["EncMode.Single"] = "단일 파일 모드",
            ["EncMode.Queue"] = "대기열 모드 (Queue)",
            ["EncMode.Concat"] = "연결 모드 (Concat)",
            ["EncMode.Repart"] = "재분할 모드 (Repart)",
            ["SrcQueue"] = "📁 비디오 소스 큐",
            ["SrcQueueWithCount"] = "📁 비디오 소스 큐 ({0})",
            ["SrcConcat"] = "∪ 비디오 소스 연결",
            ["SrcConcatWithCount"] = "∪ 비디오 소스 연결 ({0})",
            ["SrcRepart"] = "📁 비디오 소스 재분할",
        },
        ["pt-br"] = new()
        {
            ["Confirm"] = "Confirmar",
            ["Cancel"] = "Cancelar",
            ["Add"] = "Adicionar",
            ["Delete"] = "Excluir",
            ["Cut"] = "Cortar",
            ["Copy"] = "Copiar",
            ["Paste"] = "Colar",
            ["Clear"] = "Limpar",
            ["Clear All"] = "Limpar tudo",
            ["Edit"] = "Editar",
            ["Replace"] = "Substituir",
            ["Import"] = "Importar",
            ["Save"] = "Salvar",
            ["Close"] = "Fechar",
            ["Preview"] = "Visualizar",
            ["Fit"] = "Ajustar",
            ["Remove"] = "Remover",
            ["Reset"] = "Redefinir",
            ["Stop"] = "Parar",
            ["Undo"] = "Desfazer",
            ["Redo"] = "Refazer",
            ["Select All"] = "Selecionar tudo",
            ["Refresh"] = "Atualizar",
            ["Enable"] = "Ativar",
            ["Disable"] = "Desativar",
            ["On"] = "Ativado",
            ["Off"] = "Desativado",
            ["Collapse"] = "▲ Recolher",
            ["Expand"] = "▼ Expandir",
            ["Default"] = "Padrão",
            ["Width"] = "Largura",
            ["Height"] = "Altura",
            ["EncMode.Single"] = "Modo de arquivo único",
            ["EncMode.Queue"] = "Modo de fila (Queue)",
            ["EncMode.Concat"] = "Modo concat (Concat)",
            ["EncMode.Repart"] = "Modo repart (Repart)",
            ["SrcQueue"] = "📁 Fila de fontes de vídeo",
            ["SrcQueueWithCount"] = "📁 Fila de fontes de vídeo ({0})",
            ["SrcConcat"] = "∪ Concatenação de fontes de vídeo",
            ["SrcConcatWithCount"] = "∪ Concatenação de fontes de vídeo ({0})",
            ["SrcRepart"] = "📁 Repart. vídeo",
        },
    };

    protected LangProviderBase(
        string languageCode,
        Dictionary<string, Dictionary<string, string>> data)
    {
        LanguageCode = string.IsNullOrWhiteSpace(languageCode) || !data.ContainsKey(languageCode)
            ? "en"
            : languageCode;
        _d = data[LanguageCode];
    }

    public string LanguageCode { get; }

    public string this[string key]
    {
        get
        {
            if (_d.TryGetValue(key, out string? value)) return value;
            if (TryGetCommonValue(key, out value)) return value;
#if DEBUG
            throw new MissingTranslationException(GetType().Name, LanguageCode, key);
#else
            return "!NO TEXT!";
#endif
        }
    }

    private bool TryGetCommonValue(string key, out string value)
    {
        if (!CommonData.TryGetValue(LanguageCode, out Dictionary<string, string>? common))
        {
            value = string.Empty;
            return false;
        }

        if (common.TryGetValue(key, out value!)) return true;

        int lastDot = key.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < key.Length - 1)
        {
            string shortKey = key[(lastDot + 1)..];
            if (common.TryGetValue(shortKey, out value!)) return true;
        }

        const string buttonTextSuffix = "ButtonText";
        if (key.EndsWith(buttonTextSuffix, StringComparison.Ordinal) && key.Length > buttonTextSuffix.Length)
        {
            string stem = key[..^buttonTextSuffix.Length];
            if (common.TryGetValue(stem, out value!)) return true;
        }

        value = string.Empty;
        return false;
    }
}
