using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class LoadAppConfCmd : AsyncBaseCmd
    {
        private readonly AppConfM _appConfStore;
        public LoadAppConfCmd(AppConfM appConfS)
        {
            _appConfStore = appConfS;
        }
        protected override async Task ExecuteAsync(object? parameter)
        {
            var loadedConfig = AppConfM.Load();
            // Update the store with loaded config
            _appConfStore.General = loadedConfig.General;
            _appConfStore.Overwrite = loadedConfig.Overwrite;
            _appConfStore.Smtp = loadedConfig.Smtp;
            await Task.CompletedTask;
        }
    }
}
