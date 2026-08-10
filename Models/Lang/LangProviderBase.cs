namespace OneColumnEncoder.Models.Lang;

public abstract class LangProviderBase
{
    protected readonly Dictionary<string, string> _d;
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
        },
    };

    protected LangProviderBase(
        string languageCode,
        Dictionary<string, Dictionary<string, string>> data)
    {
        LanguageCode = data.ContainsKey(languageCode) ? languageCode : "en";
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
