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
        },
        ["zh-cn"] = new()
        {
            ["Confirm"] = "确认",
            ["Cancel"] = "取消",
        },
        ["zh-tw"] = new()
        {
            ["Confirm"] = "確認",
            ["Cancel"] = "取消",
        },
        ["fr"] = new()
        {
            ["Confirm"] = "Confirmer",
            ["Cancel"] = "Annuler",
        },
        ["es"] = new()
        {
            ["Confirm"] = "Confirmar",
            ["Cancel"] = "Cancelar",
        },
        ["ja"] = new()
        {
            ["Confirm"] = "確認",
            ["Cancel"] = "キャンセル",
        },
        ["ru"] = new()
        {
            ["Confirm"] = "Подтвердить",
            ["Cancel"] = "Отмена",
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
