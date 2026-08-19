using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;

namespace OneColumnEncoder.UI;

/// <summary>
/// Central source of UI and code fonts.
/// System-installed fonts come from <see cref="Fonts.SystemFontFamilies"/>.
/// Custom fonts are loaded recursively from the "CustomFont/Default" and
/// "CustomFont/Code" folders under the settings storage directory.
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

    private static readonly Dictionary<string, FontFamily> _uiSystemFonts = [];
    private static readonly Dictionary<string, FontFamily> _uiCustomFonts = [];
    private static readonly Dictionary<string, FontFamily> _codeSystemFonts = [];
    private static readonly Dictionary<string, FontFamily> _codeCustomFonts = [];

    public static bool HasCustomFontLoadIssues { get; private set; }

    public static IReadOnlyList<FontFamily> UiFonts =>
        [.. UiCustomFonts, .. UiSystemFonts];

    public static IReadOnlyList<FontFamily> UiSystemFonts =>
        BuildVisibleFamilies(_uiSystemFonts, _uiCustomFonts);

    public static IReadOnlyList<FontFamily> UiCustomFonts =>
        BuildVisibleFamilies(_uiCustomFonts);

    public static IReadOnlyList<FontFamily> CodeFonts =>
        [.. CodeCustomFonts, .. CodeSystemFonts];

    public static IReadOnlyList<FontFamily> CodeSystemFonts =>
        BuildVisibleFamilies(_codeSystemFonts, _codeCustomFonts);

    public static IReadOnlyList<FontFamily> CodeCustomFonts =>
        BuildVisibleFamilies(_codeCustomFonts);

    /// <summary>
    /// Catch unusable font families by using them, this is only needed to enhance robustness
    /// </summary>
    /// <param name="family">A group of font files belongs to one, in Regular, Bold, Italic files</param>
    /// <returns>true: font is valid</returns>
    private static bool IsValidFontFamily(FontFamily family)
    {
        if (family == null || string.IsNullOrWhiteSpace(family.Source))
            return false;
        try
        {
            var typeface = new Typeface(
                family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            return typeface.TryGetGlyphTypeface(out _);
        }
        catch { return false; }
    }
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
        _uiSystemFonts.Clear();
        _uiCustomFonts.Clear();
        _codeSystemFonts.Clear();
        _codeCustomFonts.Clear();
        HasCustomFontLoadIssues = false;

        string configDir = SaveLoadBase<AppConfM>.GetConfigDirectory();
        string uiFolder = Path.Combine(configDir, CustomFontRoot, DefaultFolderName);
        string codeFolder = Path.Combine(configDir, CustomFontRoot, CodeFolderName);

        Directory.CreateDirectory(uiFolder);
        Directory.CreateDirectory(codeFolder);

        AddSystemFamilies(_uiSystemFonts);
        AddSystemFamilies(_codeSystemFonts);
        AddFolderFamilies(_uiCustomFonts, uiFolder);
        AddFolderFamilies(_codeCustomFonts, codeFolder);
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

    public static FontFamily ResolveUiFont(string? name) =>
        ResolveFont(name, _uiSystemFonts, _uiCustomFonts, DefaultUiFont);

    public static FontFamily ResolveCodeFont(string? name) =>
        ResolveFont(name, _codeSystemFonts, _codeCustomFonts, DefaultCodeFont);

    /// <summary>
    /// Show font names to UI (A dropdown menu)
    /// </summary>
    /// <param name="family">A group of font files belongs to one, in Regular, Bold, Italic files</param>
    /// <returns>Name string to display</returns>
    public static string GetFontDisplayName(FontFamily family)
    {
        // family.FamilyNames is null does not mean count is 0... somehow
        if (family == null || family.FamilyNames == null)
            return string.Empty;

        try
        {
            foreach (XmlLanguage language in PreferredFontNameLanguages)
            {
                if (family.FamilyNames.TryGetValue(language, out string? name)
                    && !string.IsNullOrWhiteSpace(name))
                    return name;
            }

            // Use the first one available to display if no primary language exists
            if (family.FamilyNames.Count > 0)
            {
                var firstNameUsable = family.FamilyNames.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstNameUsable.Value))
                    return firstNameUsable.Value;
            }

            // Extract from source (last resort)
            string extractedName = ExtractReadableSource(family.Source);
            return string.IsNullOrWhiteSpace(extractedName)
                ? "!NAME OF FONT"
                : extractedName;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetFontDisplayName: {ex.Message}");
            return ExtractReadableSource(family.Source);
        }
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
            if (!names.Contains(zhCulture)) names.Add(zhCulture);

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

    private static FontFamily ResolveFont(
        string? name,
        Dictionary<string, FontFamily> primary,
        Dictionary<string, FontFamily> secondary,
        FontFamily fallback)
    {
        if (string.IsNullOrWhiteSpace(name))
            return fallback;

        string normalized = Normalize(name);
        return secondary.TryGetValue(normalized, out FontFamily? customFamily)
            ? customFamily
            : primary.TryGetValue(normalized, out FontFamily? systemFamily)
                ? systemFamily
                : fallback;
    }

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

    /// <summary>
    /// Scan for supported font files, and adds their font families to the specified dictionary
    /// </summary>
    /// <param name="target">The directionay to edit</param>
    /// <param name="folderPath">Folder to scan for font files</param>
    private static void AddFolderFamilies(Dictionary<string, FontFamily> target, string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;

        foreach (string file in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                     .Where(IsSupportedFontFile))
        {
            try
            {
                Uri fileUri = new(file);
                GlyphTypeface typeface = new(fileUri);
                string? fragmentName = GetTypefaceName(typeface);

                FontFamily family = new(fileUri.AbsoluteUri + "#" + fragmentName);
                string displayName = GetFontDisplayName(family);

                // Invalid font are written to debug log
                if (string.IsNullOrWhiteSpace(displayName) ||
                    string.IsNullOrWhiteSpace(family.Source))
                {
                    HasCustomFontLoadIssues = true;
                    Debug.WriteLine($"Skipping bad font: {file}");
                    continue;
                }

                // Validate FontFamily (this might be slow
                try
                {
                    var testTypeface = new Typeface(family, FontStyles.Normal,
                        FontWeights.Normal, FontStretches.Normal);
                    if (!testTypeface.TryGetGlyphTypeface(out _))
                    {
                        HasCustomFontLoadIssues = true;
                        Debug.WriteLine($"Font not usable: {displayName}");
                        continue;
                    }
                }
                catch
                {
                    HasCustomFontLoadIssues = true;
                    Debug.WriteLine($"Font validation failed: {displayName}");
                    continue;
                }

                target[Normalize(displayName)] = family;
            }
            catch (Exception ex)
            {
                HasCustomFontLoadIssues = true;
                Debug.WriteLine($"Possibly corrupted font {file}: {ex.Message}");
            }
        }
    }

    private static bool IsSupportedFontFile(string file)
    {
        string extension = Path.GetExtension(file);
        return extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".otf", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ttc", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<FontFamily> BuildVisibleFamilies(
        Dictionary<string, FontFamily> source,
        Dictionary<string, FontFamily>? excluded = null)
        => [.. source.Values.Where(f => IsValidFontFamily(f) && !IsHiddenByExcludedFamilies(f, excluded))];

    private static bool IsHiddenByExcludedFamilies(
        FontFamily family,
        Dictionary<string, FontFamily>? excluded)
    {
        if (excluded is null) return false;

        string name = GetFontDisplayName(family);
        return !string.IsNullOrWhiteSpace(name) && excluded.ContainsKey(Normalize(name));
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
