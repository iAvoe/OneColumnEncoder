using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;

namespace OneColumnEncoder.UI;

/// <summary>
/// Central source of UI and code fonts.
/// System-installed fonts come from <see cref="Fonts.SystemFontFamilies"/>.
/// Custom fonts are loaded only from the "CustomFont/Default" and "CustomFont/Code"
/// folders under the settings storage directory (no other locations are searched).
/// </summary>
public static class AppFontProvider
{
    public const string UiFontKey = "UiFont";
    public const string CodeFontKey = "CodeFont";

    private const string CustomFontRoot = "CustomFont";
    private const string DefaultFolderName = "Default";
    private const string CodeFolderName = "Code";

    private static readonly FontFamily DefaultUiFont = new("Segoe UI");
    private static readonly FontFamily DefaultCodeFont = new("Consolas");

    private static readonly Dictionary<string, FontFamily> _uiFonts = [];
    private static readonly Dictionary<string, FontFamily> _codeFonts = [];

    public static IReadOnlyList<FontFamily> UiFonts => [.. _uiFonts.Values];
    public static IReadOnlyList<FontFamily> CodeFonts => [.. _codeFonts.Values];

    public static FontFamily UiFont =>
        TryGetResource(UiFontKey) ?? DefaultUiFont;

    public static FontFamily CodeFont =>
        TryGetResource(CodeFontKey) ?? DefaultCodeFont;

    /// <summary>
    /// (Re)scans system fonts and the custom font folders, rebuilding the
    /// available families. Custom fonts win when they share a name with a
    /// system font.
    /// </summary>
    public static void Refresh()
    {
        _uiFonts.Clear();
        _codeFonts.Clear();

        string configDir = SaveLoadBase<AppConfM>.GetConfigDirectory();
        string uiFolder = Path.Combine(configDir, CustomFontRoot, DefaultFolderName);
        string codeFolder = Path.Combine(configDir, CustomFontRoot, CodeFolderName);

        Directory.CreateDirectory(uiFolder);
        Directory.CreateDirectory(codeFolder);

        AddSystemFamilies(_uiFonts);
        AddSystemFamilies(_codeFonts);
        AddFolderFamilies(_uiFonts, uiFolder);
        AddFolderFamilies(_codeFonts, codeFolder);
    }

    /// <summary>
    /// Applies the configured fonts to the application resources and all open windows.
    /// </summary>
    public static void ApplyFrom(AppConfM conf)
    {
        Application? app = Application.Current;
        if (app is null) return;

        app.Resources[UiFontKey] = ResolveUiFont(conf.Font.UiFontFamily);
        app.Resources[CodeFontKey] = ResolveCodeFont(conf.Font.CodeFontFamily);

        foreach (Window window in app.Windows)
            TextElement.SetFontFamily(window, UiFont);
    }

    public static FontFamily ResolveUiFont(string? name) => ResolveFont(name, _uiFonts, DefaultUiFont);

    public static FontFamily ResolveCodeFont(string? name) => ResolveFont(name, _codeFonts, DefaultCodeFont);

    public static string GetFontDisplayName(FontFamily family)
    {
        try
        {
            foreach (XmlLanguage language in PreferredFontNameLanguages)
            {
                if (family.FamilyNames.TryGetValue(language, out string? name)
                    && !string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
        }
        catch
        {
            // Fall through to ExtractReadableSource.
        }

        return ExtractReadableSource(family.Source);
    }

    /// <summary>
    /// Order in which a font's localized family names are preferred when building display titles.
    /// Single-component character titles come first, i.e., Chinese fonts → Chinese name
    /// (微软雅黑 instead of "Microsoft YaHei");
    /// When the UI is Chinese its own variant wins, otherwise ZH-CN → ZH-TW → others → current UI lang,
    /// English is used as the portable fallback for the remaining fonts.
    /// </summary>
    private static readonly XmlLanguage[] PreferredFontNameLanguages =
        BuildPreferredFontNameLanguages();

    private static XmlLanguage[] BuildPreferredFontNameLanguages()
    {
        string[] chineseCultures = ["zh-cn", "zh-tw", "zh-hk", "zh-mo", "zh-sg", "zh"];
        CultureInfo culture = CultureInfo.CurrentUICulture;
        string currentCulture = culture.Name.ToLowerInvariant();
        bool isChineseUi = culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase);

        List<string> names = [];
        if (isChineseUi && !string.IsNullOrWhiteSpace(currentCulture))
            names.Add(currentCulture);

        foreach (string zhCulture in chineseCultures)
        {
            if (!names.Contains(zhCulture)) names.Add(zhCulture);
        }

        if (!string.IsNullOrWhiteSpace(currentCulture) && !names.Contains(currentCulture))
            names.Add(currentCulture);

        names.AddRange("en-us", "en");
        return [.. names.Select(XmlLanguage.GetLanguage)];
    }

    /// <summary>
    /// Strips the composite URI form ("file:///...#FamilyName") used for custom
    /// fonts so only the readable family name remains.
    /// </summary>
    private static string ExtractReadableSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return source;
        int hashIndex = source.LastIndexOf('#');
        return hashIndex >= 0 && hashIndex < source.Length - 1
            ? source[(hashIndex + 1)..]
            : source;
    }

    private static FontFamily ResolveFont(string? name, Dictionary<string, FontFamily> families, FontFamily fallback)
        => !string.IsNullOrWhiteSpace(name) && families.TryGetValue(Normalize(name), out var family)
            ? family
            : fallback;

    private static FontFamily? TryGetResource(string key) =>
        Application.Current?.Resources[key] as FontFamily;

    private static void AddSystemFamilies(Dictionary<string, FontFamily> target)
    {
        foreach (FontFamily family in Fonts.SystemFontFamilies)
        {
            string name = GetFontDisplayName(family);
            if (string.IsNullOrWhiteSpace(name)) continue;
            target[Normalize(name)] = family;
        }
    }

    private static void AddFolderFamilies(Dictionary<string, FontFamily> target, string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        string[] files = [.. Directory.GetFiles(folderPath, "*.*")
            .Where(f => f.EndsWith(".ttf", System.StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".otf", System.StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".ttc", System.StringComparison.OrdinalIgnoreCase))];

        foreach (string file in files)
        {
            try
            {
                GlyphTypeface typeface = new(new Uri(file));
                string? fragmentName = GetTypefaceName(typeface);
                if (string.IsNullOrWhiteSpace(fragmentName)) continue;
                // Composite URI form ("file:///...#FamilyName") pins the custom file,
                // unlike the (baseUri, name) constructor which falls back to a
                // same-named system font. The dictionary key is the display title
                // (GetFontDisplayName) so it matches both what the picker stores and
                // what ResolveFont looks up.
                FontFamily family = new(new Uri(file).AbsoluteUri + "#" + fragmentName);
                string displayName = GetFontDisplayName(family);
                if (string.IsNullOrWhiteSpace(displayName)) continue;
                target[Normalize(displayName)] = family;
            }
            catch {} // Skip unreadable font files.
        }
    }

    private static string? GetTypefaceName(GlyphTypeface typeface)
    {
        string currentCulture =
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        foreach (KeyValuePair<CultureInfo, string> pair in typeface.FamilyNames)
        {
            if (pair.Key.TwoLetterISOLanguageName.Equals(
                "en", StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        // No English name (e.g. Chinese-only fonts): fall back to the current UI
        // language name, then to any available localized name so the font is not
        // silently dropped from the picker.
        string? fallback = null;
        foreach (KeyValuePair<CultureInfo, string> pair in typeface.FamilyNames)
        {
            if (pair.Key.TwoLetterISOLanguageName.Equals(
                currentCulture, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
            fallback ??= pair.Value;
        }

        return fallback;
    }

    private static string Normalize(string name) => name.Trim().ToLowerInvariant();
}
