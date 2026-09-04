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
            ["Resolution"] = "Resolution",
            ["FrameRate"] = "Frame rate",
            ["BitDepth"] = "Bit depth",
            ["ChromaSubsampling"] = "Chroma subsampling",
            ["ColorMatrix"] = "Color matrix",
            ["Transfer"] = "Transfer",
            ["Primaries"] = "Primaries",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "Video codec",
            ["AudioCodec"] = "Audio codec",
            ["ContainerFormat"] = "Container format",
            ["EncMode.Single"] = "Single-file mode",
            ["EncMode.Queue"] = "Queue mode",
            ["EncMode.Concat"] = "Concat mode",
            ["EncMode.Repart"] = "Repart mode",
            ["SrcQueue"] = "📁 Video Src. Queue",
            ["SrcQueueWithCount"] = "📁 Video Src. Queue ({0})",
            ["SrcConcat"] = "∪ Video Src. Concat",
            ["SrcConcatWithCount"] = "∪ Video Src. Concat ({0})",
            ["SrcRepart"] = "📁 Video Src. Repart",
            ["ToolField.Path"] = "Path",
            ["SourceQueue.Sequence"] = "Sequence",
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
            ["Resolution"] = "分辨率",
            ["FrameRate"] = "帧率",
            ["BitDepth"] = "位深",
            ["ChromaSubsampling"] = "色度采样",
            ["ColorMatrix"] = "色彩矩阵",
            ["Transfer"] = "传递函数",
            ["Primaries"] = "原色",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "视频编码",
            ["AudioCodec"] = "音频编码",
            ["ContainerFormat"] = "容器格式",
            ["EncMode.Single"] = "单文件模式",
            ["EncMode.Queue"] = "队列模式（Queue）",
            ["EncMode.Concat"] = "拼接模式（Concat）",
            ["EncMode.Repart"] = "重分集模式（Repart）",
            ["SrcQueue"] = "📁 视频源队列",
            ["SrcQueueWithCount"] = "📁 视频源队列 ({0})",
            ["SrcConcat"] = "∪ 视频源拼接",
            ["SrcConcatWithCount"] = "∪ 视频源拼接 ({0})",
            ["SrcRepart"] = "📁 视频源重分集",
            ["ToolField.Path"] = "路径",
            ["SourceQueue.Sequence"] = "序列",
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
            ["Resolution"] = "解析度",
            ["FrameRate"] = "幀率",
            ["BitDepth"] = "位深",
            ["ChromaSubsampling"] = "色度取樣",
            ["ColorMatrix"] = "色彩矩陣",
            ["Transfer"] = "傳遞函數",
            ["Primaries"] = "原色",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "影片編碼",
            ["AudioCodec"] = "音訊編碼",
            ["ContainerFormat"] = "容器格式",
            ["EncMode.Single"] = "單文件模式",
            ["EncMode.Queue"] = "隊列模式（Queue）",
            ["EncMode.Concat"] = "拼接模式（Concat）",
            ["EncMode.Repart"] = "重分集模式（Repart）",
            ["SrcQueue"] = "📁 視訊源隊列",
            ["SrcQueueWithCount"] = "📁 視訊源隊列 ({0})",
            ["SrcConcat"] = "∪ 視訊源拼接",
            ["SrcConcatWithCount"] = "∪ 視訊源拼接 ({0})",
            ["SrcRepart"] = "📁 視訊源重分集",
            ["ToolField.Path"] = "路徑",
            ["SourceQueue.Sequence"] = "序列",
            ["SourceQueue.SelectFolderTitle"] = "選擇視訊來源序列資料夾",
            ["SourceQueue.Analyzed"] = "隊列視訊來源分析已完成。因差異過大過濾掉 {0} 個視訊。\n\n隊列資料 JSON：\n{1}\n\n排除列表：\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "隊列視訊來源分析已完成。未過濾掉視訊。\n\n隊列資料 JSON：\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "複製隊列 JSON 路徑",
            ["SourceQueue.CopyExcludedJsonPath"] = "複製排除列表 JSON 路徑",
            ["SourceQueue.OpenQueueJson"] = "開啟隊列 JSON",
            ["SourceQueue.OpenExcludedJson"] = "開啟排除列表 JSON",
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
            ["Resolution"] = "Résolution",
            ["FrameRate"] = "Fréquence d'images",
            ["BitDepth"] = "Profondeur de bits",
            ["ChromaSubsampling"] = "Sous-échantillonnage chroma",
            ["ColorMatrix"] = "Matrice couleur",
            ["Transfer"] = "Transfert",
            ["Primaries"] = "Primaires",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "Codec vidéo",
            ["AudioCodec"] = "Codec audio",
            ["ContainerFormat"] = "Format conteneur",
            ["EncMode.Single"] = "Mode fichier unique",
            ["EncMode.Queue"] = "Mode file d'attente (Queue)",
            ["EncMode.Concat"] = "Mode concat (Concat)",
            ["EncMode.Repart"] = "Mode repart (Repart)",
            ["SrcQueue"] = "📁 File d'attente vidéo",
            ["SrcQueueWithCount"] = "📁 File d'attente vidéo ({0})",
            ["SrcConcat"] = "∪ Concat source vidéo",
            ["SrcConcatWithCount"] = "∪ Concat source vidéo ({0})",
            ["SrcRepart"] = "📁 Répart. vidéo",
            ["ToolField.Path"] = "Chemin",
            ["SourceQueue.Sequence"] = "Séquence",
            ["SourceQueue.SelectFolderTitle"] = "Sélectionner le dossier de la file d'attente",
            ["SourceQueue.Analyzed"] = "Analyse de la file d'attente terminée. {0} vidéo(s) exclue(s) en raison de différences excessives.\n\nJSON des données de file d'attente :\n{1}\n\nListe d'exclusion :\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Analyse de la file d'attente terminée. Aucune vidéo exclue.\n\nJSON des données de file d'attente :\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copier le chemin du JSON de file d'attente",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copier le chemin du JSON d'exclusion",
            ["SourceQueue.OpenQueueJson"] = "Ouvrir le JSON de file d'attente",
            ["SourceQueue.OpenExcludedJson"] = "Ouvrir le JSON d'exclusion",
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
            ["Resolution"] = "Resolución",
            ["FrameRate"] = "Velocidad de fotogramas",
            ["BitDepth"] = "Profundidad de bits",
            ["ChromaSubsampling"] = "Submuestreo de croma",
            ["ColorMatrix"] = "Matriz de color",
            ["Transfer"] = "Transferencia",
            ["Primaries"] = "Primarias",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "Códec de vídeo",
            ["AudioCodec"] = "Códec de audio",
            ["ContainerFormat"] = "Formato de contenedor",
            ["EncMode.Single"] = "Modo de archivo único",
            ["EncMode.Queue"] = "Modo de cola (Queue)",
            ["EncMode.Concat"] = "Modo concat (Concat)",
            ["EncMode.Repart"] = "Modo repart (Repart)",
            ["SrcQueue"] = "📁 Cola de vídeo",
            ["SrcQueueWithCount"] = "📁 Cola de vídeo ({0})",
            ["SrcConcat"] = "∪ Concat vídeo",
            ["SrcConcatWithCount"] = "∪ Concat vídeo ({0})",
            ["SrcRepart"] = "📁 Repart. vídeo",
            ["ToolField.Path"] = "Ruta",
            ["SourceQueue.Sequence"] = "Secuencia",
            ["SourceQueue.SelectFolderTitle"] = "Seleccionar carpeta de cola de fuente de vídeo",
            ["SourceQueue.Analyzed"] = "Análisis de cola completado. Se filtraron {0} video(s) por diferencias excesivas.\n\nJSON de datos de cola:\n{1}\n\nLista de exclusión:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Análisis de cola completado. No se filtraron vídeos.\n\nJSON de datos de cola:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copiar ruta del JSON de cola",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copiar ruta del JSON de exclusión",
            ["SourceQueue.OpenQueueJson"] = "Abrir JSON de cola",
            ["SourceQueue.OpenExcludedJson"] = "Abrir JSON de exclusión",
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
            ["Resolution"] = "解像度",
            ["FrameRate"] = "フレームレート",
            ["BitDepth"] = "ビット深度",
            ["ChromaSubsampling"] = "クロマサブサンプリング",
            ["ColorMatrix"] = "カラーマトリクス",
            ["Transfer"] = "伝達特性",
            ["Primaries"] = "原色",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "映像コーデック",
            ["AudioCodec"] = "音声コーデック",
            ["ContainerFormat"] = "コンテナ形式",
            ["EncMode.Single"] = "単一ファイルモード",
            ["EncMode.Queue"] = "キューモード (Queue)",
            ["EncMode.Concat"] = "連結モード (Concat)",
            ["EncMode.Repart"] = "再分割モード (Repart)",
            ["SrcQueue"] = "📁 動画ソースキュー",
            ["SrcQueueWithCount"] = "📁 動画ソースキュー ({0})",
            ["SrcConcat"] = "∪ 動画ソース連結",
            ["SrcConcatWithCount"] = "∪ 動画ソース連結 ({0})",
            ["SrcRepart"] = "📁 映像ソース再分割",
            ["ToolField.Path"] = "パス",
            ["SourceQueue.Sequence"] = "シーケンス",
            ["SourceQueue.SelectFolderTitle"] = "ビデオソースキューのフォルダを選択",
            ["SourceQueue.Analyzed"] = "キューソース分析が完了しました。差異が大きいため {0} 個の動画を除外しました。\n\nキューデータ JSON:\n{1}\n\n除外リスト:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "キューソース分析が完了しました。除外された動画はありません。\n\nキューデータ JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "キュー JSON パスをコピー",
            ["SourceQueue.CopyExcludedJsonPath"] = "除外 JSON パスをコピー",
            ["SourceQueue.OpenQueueJson"] = "キュー JSON を開く",
            ["SourceQueue.OpenExcludedJson"] = "除外 JSON を開く",
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
            ["Resolution"] = "Разрешение",
            ["FrameRate"] = "Частота кадров",
            ["BitDepth"] = "Глубина цвета",
            ["ChromaSubsampling"] = "Субдискретизация хромы",
            ["ColorMatrix"] = "Цветовая матрица",
            ["Transfer"] = "Передаточная функция",
            ["Primaries"] = "Первичные цвета",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "Видеокодек",
            ["AudioCodec"] = "Аудиокодек",
            ["ContainerFormat"] = "Формат контейнера",
            ["EncMode.Single"] = "Режим одного файла",
            ["EncMode.Queue"] = "Режим очереди (Queue)",
            ["EncMode.Concat"] = "Режим конкатенации (Concat)",
            ["EncMode.Repart"] = "Режим репарта (Repart)",
            ["SrcQueue"] = "📁 Очередь видеоисточника",
            ["SrcQueueWithCount"] = "📁 Очередь видеоисточника ({0})",
            ["SrcConcat"] = "∪ Конкатенация видео",
            ["SrcConcatWithCount"] = "∪ Конкатенация видео ({0})",
            ["SrcRepart"] = "📁 Репарт видео",
            ["ToolField.Path"] = "Путь",
            ["SourceQueue.Sequence"] = "Последовательность",
            ["SourceQueue.SelectFolderTitle"] = "Выберите папку очереди видеоисточников",
            ["SourceQueue.Analyzed"] = "Анализ очереди завершён. Отфильтровано {0} видео из-за чрезмерных различий.\n\nJSON данных очереди:\n{1}\n\nСписок исключённых:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Анализ очереди завершён. Видео не отфильтрованы.\n\nJSON данных очереди:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Копировать путь к JSON очереди",
            ["SourceQueue.CopyExcludedJsonPath"] = "Копировать путь к JSON исключений",
            ["SourceQueue.OpenQueueJson"] = "Открыть JSON очереди",
            ["SourceQueue.OpenExcludedJson"] = "Открыть JSON исключений",
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
            ["Resolution"] = "Auflösung",
            ["FrameRate"] = "Bildrate",
            ["BitDepth"] = "Bittiefe",
            ["ChromaSubsampling"] = "Farbunterabtastung",
            ["ColorMatrix"] = "Farbmatrix",
            ["Transfer"] = "Übertragungsfunktion",
            ["Primaries"] = "Primärfarben",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "Videocodec",
            ["AudioCodec"] = "Audiocodec",
            ["ContainerFormat"] = "Containerformat",
            ["EncMode.Single"] = "Einzeldatei-Modus",
            ["EncMode.Queue"] = "Warteschlangenmodus (Queue)",
            ["EncMode.Concat"] = "Concat-Modus",
            ["EncMode.Repart"] = "Repart-Modus",
            ["SrcQueue"] = "📁 Video-Wart.",
            ["SrcQueueWithCount"] = "📁 Video-Wart. ({0})",
            ["SrcConcat"] = "∪ Video-Concat",
            ["SrcConcatWithCount"] = "∪ Video-Concat ({0})",
            ["SrcRepart"] = "📁 Video-Neuteilung",
            ["ToolField.Path"] = "Pfad",
            ["SourceQueue.Sequence"] = "Sequenz",
            ["SourceQueue.SelectFolderTitle"] = "Videoquellen-Warteschlangenordner wählen",
            ["SourceQueue.Analyzed"] = "Warteschlangenanalyse abgeschlossen. {0} Video(s) wegen übermäßiger Unterschiede gefiltert.\n\nJSON-Daten:\n{1}\n\nAusschlussliste:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Warteschlangenanalyse abgeschlossen. Keine Videos gefiltert.\n\nJSON-Daten:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "JSON-Pfad kopieren",
            ["SourceQueue.CopyExcludedJsonPath"] = "Ausschluss-JSON-Pfad kopieren",
            ["SourceQueue.OpenQueueJson"] = "JSON öffnen",
            ["SourceQueue.OpenExcludedJson"] = "Ausschluss-JSON öffnen",
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
            ["Resolution"] = "해상도",
            ["FrameRate"] = "프레임률",
            ["BitDepth"] = "비트 깊이",
            ["ChromaSubsampling"] = "크로마 서브샘플링",
            ["ColorMatrix"] = "색상 매트릭스",
            ["Transfer"] = "전달 함수",
            ["Primaries"] = "원색",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "비디오 코덱",
            ["AudioCodec"] = "오디오 코덱",
            ["ContainerFormat"] = "컨테이너 형식",
            ["EncMode.Single"] = "단일 파일 모드",
            ["EncMode.Queue"] = "대기열 모드 (Queue)",
            ["EncMode.Concat"] = "연결 모드 (Concat)",
            ["EncMode.Repart"] = "재분할 모드 (Repart)",
            ["SrcQueue"] = "📁 비디오 소스 큐",
            ["SrcQueueWithCount"] = "📁 비디오 소스 큐 ({0})",
            ["SrcConcat"] = "∪ 비디오 소스 연결",
            ["SrcConcatWithCount"] = "∪ 비디오 소스 연결 ({0})",
            ["SrcRepart"] = "📁 비디오 소스 재분할",
            ["ToolField.Path"] = "경로",
            ["SourceQueue.Sequence"] = "순번",
            ["SourceQueue.SelectFolderTitle"] = "비디오 소스 대기열 폴더 선택",
            ["SourceQueue.Analyzed"] = "대기열 소스 분석 완료. 차이가 너무 커서 동영상 {0}개를 걸러냈습니다.\n\n대기열 데이터 JSON:\n{1}\n\n제외 목록:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "대기열 소스 분석 완료. 걸러낸 동영상이 없습니다.\n\n대기열 데이터 JSON:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "대기열 JSON 경로 복사",
            ["SourceQueue.CopyExcludedJsonPath"] = "제외 JSON 경로 복사",
            ["SourceQueue.OpenQueueJson"] = "대기열 JSON 열기",
            ["SourceQueue.OpenExcludedJson"] = "제외 JSON 열기",
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
            ["Resolution"] = "Resolução",
            ["FrameRate"] = "Taxa de quadros",
            ["BitDepth"] = "Profundidade de bits",
            ["ChromaSubsampling"] = "Subamostragem de croma",
            ["ColorMatrix"] = "Matriz de cor",
            ["Transfer"] = "Transferência",
            ["Primaries"] = "Primárias",
            ["HDR"] = "HDR",
            ["VideoCodec"] = "Codec de vídeo",
            ["AudioCodec"] = "Codec de áudio",
            ["ContainerFormat"] = "Formato de contêiner",
            ["EncMode.Single"] = "Modo de arquivo único",
            ["EncMode.Queue"] = "Modo de fila (Queue)",
            ["EncMode.Concat"] = "Modo concat (Concat)",
            ["EncMode.Repart"] = "Modo repart (Repart)",
            ["SrcQueue"] = "📁 Fila de fontes de vídeo",
            ["SrcQueueWithCount"] = "📁 Fila de fontes de vídeo ({0})",
            ["SrcConcat"] = "∪ Concatenação de fontes de vídeo",
            ["SrcConcatWithCount"] = "∪ Concatenação de fontes de vídeo ({0})",
            ["SrcRepart"] = "📁 Repart. vídeo",
            ["ToolField.Path"] = "Caminho",
            ["SourceQueue.Sequence"] = "Sequência",
            ["SourceQueue.SelectFolderTitle"] = "Selecionar pasta da fila de fontes de vídeo",
            ["SourceQueue.Analyzed"] = "Análise da fila concluída. {0} vídeo(s) filtrado(s) por diferenças excessivas.\n\nJSON de dados da fila:\n{1}\n\nLista de exclusão:\n{2}",
            ["SourceQueue.AnalyzedNoEx"] = "Análise da fila concluída. Nenhum vídeo foi filtrado.\n\nJSON de dados da fila:\n{0}",
            ["SourceQueue.CopyQueueJsonPath"] = "Copiar caminho do JSON da fila",
            ["SourceQueue.CopyExcludedJsonPath"] = "Copiar caminho do JSON de exclusão",
            ["SourceQueue.OpenQueueJson"] = "Abrir JSON da fila",
            ["SourceQueue.OpenExcludedJson"] = "Abrir JSON de exclusão",
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
