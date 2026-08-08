using OneColumnEncoder.Models;
using OneColumnEncoder.Persistence;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace OneColumnEncoder.UI
{
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

        private static readonly Dictionary<string, FontFamily> _uiFonts = new();
        private static readonly Dictionary<string, FontFamily> _codeFonts = new();

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
                if (family.FamilyNames.TryGetValue(XmlLanguage.GetLanguage("en-us"), out string? enUs)
                    && !string.IsNullOrWhiteSpace(enUs))
                {
                    return enUs;
                }

                if (family.FamilyNames.TryGetValue(XmlLanguage.GetLanguage("en"), out string? en)
                    && !string.IsNullOrWhiteSpace(en))
                {
                    return en;
                }
            }
            catch
            {
                // Fall through to Source.
            }

            return family.Source;
        }

        private static FontFamily ResolveFont(string? name, Dictionary<string, FontFamily> families, FontFamily fallback)
        {
            if (!string.IsNullOrWhiteSpace(name)
                && families.TryGetValue(Normalize(name), out FontFamily? family))
            {
                return family;
            }

            return fallback;
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

        private static void AddFolderFamilies(Dictionary<string, FontFamily> target, string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            string[] files = Directory.GetFiles(folderPath, "*.*")
                .Where(f => f.EndsWith(".ttf", System.StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".otf", System.StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".ttc", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string file in files)
            {
                try
                {
                    GlyphTypeface typeface = new(new Uri(file));
                    string? name = GetTypefaceName(typeface);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    // Composite URI form ("file:///...#FamilyName") pins the custom file,
                    // unlike the (baseUri, name) constructor which falls back to a
                    // same-named system font.
                    target[Normalize(name)] = new FontFamily(new Uri(file).AbsoluteUri + "#" + name);
                }
                catch
                {
                    // Skip unreadable font files.
                }
            }
        }

        private static string? GetTypefaceName(GlyphTypeface typeface)
        {
            foreach (KeyValuePair<CultureInfo, string> pair in typeface.FamilyNames)
            {
                if (pair.Key.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
                    return pair.Value;
            }

            return null;
        }

        private static string Normalize(string name) => name.Trim().ToLowerInvariant();
    }
}
