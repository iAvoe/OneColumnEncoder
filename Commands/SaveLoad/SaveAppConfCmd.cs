namespace OneColumnEncoder.Commands.SaveLoad;

public class SaveAppConfCmd(AppConfM appConfS, Action closeAction) : AsyncBaseCmd
{
    private readonly AppConfM _appConfStore = appConfS;

    protected override async Task ExecuteAsync(object? parameter)
    {
        UILangProvider.SetLanguage(_appConfStore.Lang.LanguageCode);
        AppFontProvider.ApplyFrom(_appConfStore);
        _appConfStore.Save();
        await Task.CompletedTask;
        closeAction();
    }
}
