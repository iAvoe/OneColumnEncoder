using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class LoadAppConfCmd(AppConfM appConfS) : AsyncBaseCmd
    {
        private readonly AppConfM _appConfStore = appConfS;

        protected override async Task ExecuteAsync(object? parameter)
        {
            var loadedConfig = AppConfM.Load();
            // Update the store with loaded config
            _appConfStore.General = loadedConfig.General;
            _appConfStore.Overwrite = loadedConfig.Overwrite;
            _appConfStore.Smtp = loadedConfig.Smtp;
            _appConfStore.Lang = loadedConfig.Lang;
            UILangProviderM.SetLanguage(_appConfStore.Lang.LanguageCode);
            await Task.CompletedTask;
        }
    }
}
