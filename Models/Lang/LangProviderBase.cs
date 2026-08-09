namespace OneColumnEncoder.Models.Lang;

public abstract class LangProviderBase
{
    protected readonly Dictionary<string, string> _d;

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

#if DEBUG
            throw new MissingTranslationException(GetType().Name, LanguageCode, key);
#else
            return "!NO TEXT!";
#endif
        }
    }
}
