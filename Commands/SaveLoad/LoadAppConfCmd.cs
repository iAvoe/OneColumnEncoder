using OneColumnEncoder.Models;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class LoadAppConfCmd(AppConfM appConfS) : AsyncBaseCmd
    {
        private readonly AppConfM _appConfStore = appConfS;

        protected override async Task ExecuteAsync(object? parameter)
        {
            AppConfM loadedConfig = AppConfM.Load();
            _appConfStore.Overwrite = loadedConfig.Overwrite;
            _appConfStore.Lang = loadedConfig.Lang;
            UILangProviderM.SetLanguage(_appConfStore.Lang.LanguageCode);
            await Task.CompletedTask;
        }
    }
}
