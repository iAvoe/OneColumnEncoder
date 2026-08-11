namespace OneColumnEncoder.Commands.SaveLoad;

public abstract class AppConfCmdBase(AppConfM appConfStore) : AsyncBaseCmd
{
    protected readonly AppConfM _appConfStore = appConfStore;

    protected void ApplyLangAndFont()
    {
        UILangProvider.SetLanguage(_appConfStore.Lang.LanguageCode);
        AppFontProvider.ApplyFrom(_appConfStore);
    }
}