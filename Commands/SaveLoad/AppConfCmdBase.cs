namespace OneColumnEncoder.Commands.SaveLoad;

/// <summary>
/// Base command for R/W app settings. Currently provides:
/// - Saving settings
/// - Loading settings
/// - Applying language switch (to all stores) and the font provider
/// </summary>
/// <param name="appConfStore">Configuration store (settings instance)</param>
public abstract class AppConfCmdBase(AppConfM appConfStore) : AsyncBaseCmd
{
    protected readonly AppConfM _appConfStore = appConfStore;

    protected void SaveAppConf()
    {
        UILangProvider.ValidateMissingTranslations(_appConfStore.Lang.LanguageCode);
        ApplyLangFontAndTheme();
        _appConfStore.Save();
        Debug.WriteLine($"[{GetType().Name}] Translation self-check passed for '{UILangProvider.Current.LanguageCode}'.");
    }

    protected void LoadAppConf()
    {
        AppConfM loadedConfig = AppConfM.Load();
        ApplyToStore(loadedConfig);
        ApplyLangFontAndTheme();
    }

    protected void ApplyLangFontAndTheme()
    {
        UILangProvider.SetLanguage(_appConfStore.Lang.LanguageCode);
        AppFontProvider.ApplyFrom(_appConfStore);
        UI.ThemeManager.ApplyTheme(_appConfStore.Theme.ThemeName);
    }

    private void ApplyToStore(AppConfM loadedConfig)
    {
        _appConfStore.Reimport = loadedConfig.Reimport;
        _appConfStore.InitLang = loadedConfig.InitLang;
        _appConfStore.Overwrite = loadedConfig.Overwrite;
        _appConfStore.Lang = loadedConfig.Lang;
        _appConfStore.Theme = loadedConfig.Theme;
        _appConfStore.Font = loadedConfig.Font;
        _appConfStore.Logs = loadedConfig.Logs;
        _appConfStore.AudioMux = loadedConfig.AudioMux;
        _appConfStore.AutoMux = loadedConfig.AutoMux;
        _appConfStore.TextEditor = loadedConfig.TextEditor;
    }
}
