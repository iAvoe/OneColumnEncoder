namespace OneColumnEncoder.Commands.SaveLoad;

public class LoadAppConfCmd(AppConfM appConfS) : AsyncBaseCmd
{
    private readonly AppConfM _appConfStore = appConfS;

    protected override async Task ExecuteAsync(object? parameter)
    {
        AppConfM loadedConfig = AppConfM.Load();
        _appConfStore.Overwrite = loadedConfig.Overwrite;
        _appConfStore.Lang = loadedConfig.Lang;
        _appConfStore.Font = loadedConfig.Font;
        _appConfStore.Logs = loadedConfig.Logs;
        UILangProvider.SetLanguage(_appConfStore.Lang.LanguageCode);
        AppFontProvider.ApplyFrom(_appConfStore);
        await Task.CompletedTask;
    }
}
