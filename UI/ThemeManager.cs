using System.Windows;
using System.Windows.Media;

namespace OneColumnEncoder.UI;

/// <summary>
/// Manages application theme switching using XAML resource dictionaries.
/// </summary>
public static class ThemeManager
{
    private const string LightThemeUri = "Views/Themes/Light.xaml";
    private const string DarkThemeUri = "Views/Themes/Dark.xaml";
    private const string DarkControlsUri = "Views/Themes/DarkControls.xaml";

    public static event Action? ThemeChanged;

    /// <summary>
    /// Applies a theme by loading its XAML resource dictionary.
    /// </summary>
    public static void ApplyTheme(string themeName)
    {
        if (Application.Current == null) return;

        var mergedDicts = Application.Current.Resources.MergedDictionaries;
        
        // Remove existing theme dictionaries
        RemoveThemeDictionaries(mergedDicts);

        // Load new theme dictionary
        switch (themeName)
        {
            case "Light":
                LoadResourceDictionary(mergedDicts, LightThemeUri);
                break;

            case "Dark":
                LoadResourceDictionary(mergedDicts, DarkThemeUri);
                LoadResourceDictionary(mergedDicts, DarkControlsUri);
                break;

            default:
                // Default to Light theme
                LoadResourceDictionary(mergedDicts, LightThemeUri);
                break;
        }

        ThemeChanged?.Invoke();
    }

    private static void RemoveThemeDictionaries(System.Collections.ObjectModel.Collection<ResourceDictionary> mergedDicts)
    {
        // Remove all theme-related dictionaries
        var toRemove = mergedDicts
            .Where(dict => dict.Source != null && (
                dict.Source.OriginalString.Contains("Light.xaml") ||
                dict.Source.OriginalString.Contains("Dark.xaml") ||
                dict.Source.OriginalString.Contains("DarkControls.xaml")))
            .ToList();

        foreach (var dict in toRemove)
        {
            mergedDicts.Remove(dict);
        }
    }

    private static void LoadResourceDictionary(System.Collections.ObjectModel.Collection<ResourceDictionary> mergedDicts, string uri)
    {
        try
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri(uri, UriKind.Relative)
            };
            mergedDicts.Add(dict);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load theme resource dictionary '{uri}': {ex.Message}");
        }
    }

    private static void ApplyHighContrastAdjustments()
    {
        // Apply high contrast color adjustments to existing Light theme
        if (Application.Current?.Resources == null) return;

        try
        {
            // Increase contrast for primary elements
            Application.Current.Resources["GlobalPrimary"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
            Application.Current.Resources["Background"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            Application.Current.Resources["GlobalSecondary"] = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40));
            Application.Current.Resources["GlobalTertiary"] = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));
            Application.Current.Resources["ButtonPrimary"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to apply high contrast adjustments: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current theme name by checking loaded resource dictionaries.
    /// </summary>
    public static string GetCurrentThemeName()
    {
        if (Application.Current == null) return "Light";

        var mergedDicts = Application.Current.Resources.MergedDictionaries;
        
        if (mergedDicts.Any(d => d.Source?.OriginalString.Contains("Dark.xaml") == true))
            return "Dark";
        
        return "Light";
    }

    /// <summary>
    /// Gets the current color of a resource key.
    /// </summary>
    public static Color? GetColor(string key)
    {
        if (Application.Current?.Resources[key] is SolidColorBrush brush)
        {
            return brush.Color;
        }
        return null;
    }

    /// <summary>
    /// Updates a single color resource (for custom adjustments).
    /// </summary>
    public static void UpdateColorResource(string key, Color color)
    {
        if (Application.Current?.Resources[key] is SolidColorBrush)
        {
            Application.Current.Resources[key] = new SolidColorBrush(color);
            ThemeChanged?.Invoke();
        }
    }
}
