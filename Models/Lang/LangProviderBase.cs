namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Base class for language providers, including shared short operation words and fallback lookup for reusable UI labels.
/// 
/// To add a new language, follow the following table:
/// | File | Keys Translated |
/// |------|----------------|
/// | `LangProviderBase.cs` (this file) | 16 common UI terms(Confirm, Cancel, Add, Delete, Clear, etc.) |
/// | `AnalyzeSrcVideoCmdLangProvider.cs` | 9 keys |
/// | `AppUsageLangProvider.cs` | 38 keys |
/// | `AppConfLangProvider.cs` | 31 keys |
/// | `ConfirmDialogLangProvider.cs` | 7 keys |
/// | `ClipRangeSelectorLangProvider.cs` | 22 keys |
/// | `CpuSetsLangProvider.cs` | 6 keys |
/// | `VideoSrcQueueLangProvider.cs` | 12 keys |
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
/// | `FilenameScribeModalLangProvider.cs` | 25 keys |
/// | `FFProbeVideoAnalysisLangProvider.cs` | 5 keys |
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
    public static string RemoveText => "🗙";
    public static string MoveUpText => "↑↑";
    public static string MoveDownText => "↓↓";
    private static readonly Dictionary<string, Dictionary<string, string>> CommonData = new()
    {
        ["en"] = new()
        {
            ["Confirm"] = "Confirm",
            ["Cancel"] = "Cancel",
            ["Add"] = "Add",
            ["Delete"] = "Delete",
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
            ["Refresh"] = "Refresh",
        },
        ["zh-cn"] = new()
        {
            ["Confirm"] = "确认",
            ["Cancel"] = "取消",
            ["Add"] = "添加",
            ["Delete"] = "删除",
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
            ["Refresh"] = "刷新",
        },
        ["zh-tw"] = new()
        {
            ["Confirm"] = "確認",
            ["Cancel"] = "取消",
            ["Add"] = "添加",
            ["Delete"] = "刪除",
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
            ["Refresh"] = "刷新",
        },
        ["fr"] = new()
        {
            ["Confirm"] = "Confirmer",
            ["Cancel"] = "Annuler",
            ["Add"] = "Ajouter",
            ["Delete"] = "Supprimer",
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
            ["Refresh"] = "Rafraîchir",
        },
        ["es"] = new()
        {
            ["Confirm"] = "Confirmar",
            ["Cancel"] = "Cancelar",
            ["Add"] = "Añadir",
            ["Delete"] = "Eliminar",
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
            ["Refresh"] = "Refrescar",
        },
        ["ja"] = new()
        {
            ["Confirm"] = "確認",
            ["Cancel"] = "キャンセル",
            ["Add"] = "追加",
            ["Delete"] = "削除",
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
            ["Refresh"] = "更新",
        },
        ["ru"] = new()
        {
            ["Confirm"] = "Подтвердить",
            ["Cancel"] = "Отмена",
            ["Add"] = "Добавить",
            ["Delete"] = "Удалить",
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
            ["Refresh"] = "Обновить",
        },
        ["de"] = new()
        {
            ["Confirm"] = "Bestätigen",
            ["Cancel"] = "Abbrechen",
            ["Add"] = "Hinzufügen",
            ["Delete"] = "Löschen",
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
            ["Refresh"] = "Aktualisieren",
        },
        ["ko"] = new()
        {
            ["Confirm"] = "확인",
            ["Cancel"] = "취소",
            ["Add"] = "추가",
            ["Delete"] = "삭제",
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
            ["Refresh"] = "새로 고치다",
        },
        ["pt-br"] = new()
        {
            ["Confirm"] = "Confirmar",
            ["Cancel"] = "Cancelar",
            ["Add"] = "Adicionar",
            ["Delete"] = "Excluir",
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
            ["Refresh"] = "Atualizar",
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
