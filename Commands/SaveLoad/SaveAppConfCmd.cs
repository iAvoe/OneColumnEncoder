using OneColumnEncoder.Models;
using System;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class SaveAppConfCmd(AppConfM appConfS, Action closeAction) : AsyncBaseCmd
    {
        private readonly AppConfM _appConfStore = appConfS;

        protected override async Task ExecuteAsync(object? parameter)
        {
            UILangProviderM.SetLanguage(_appConfStore.Lang.LanguageCode);
            _appConfStore.Save();
            await Task.CompletedTask;
            closeAction();
        }
    }
}
