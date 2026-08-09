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

    public string this[string key] =>
        _d.TryGetValue(key, out string? value)
            ? value
            : throw new MissingTranslationException(GetType().Name, LanguageCode, key);
}
