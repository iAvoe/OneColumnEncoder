namespace OneColumnEncoder.Models.Lang;

/// <summary>
/// Thrown when a language provider is asked for a key that the current language does not define.
/// Language dictionaries are required to be complete for every supported language — turned out to be a bad idea,
/// some missed translations are never found, changing the dictionary is risky and exception title is often unclear,
/// so this custom exception is thrown instead
/// </summary>
/// <remarks>
/// This exception should help to find missing translations, and explain what to do about them.
/// </remarks>
public sealed class MissingTranslationException(string providerName, string languageCode, string key) : Exception(
        $"!Translation: provider '{providerName}', language '{languageCode}', key '{key}'. " +
            "Every language must have translation key defined — translate it or use \"!NO TEXT!\".")
{
    public string ProviderName { get; } = providerName;
    public string LanguageCode { get; } = languageCode;
    public string Key { get; } = key;
}
