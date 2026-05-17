using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class SaveAppConfCmd(AppConfS appConfS, ModalNavS modalNavS) : AsyncBaseCmd
    {
        private readonly AppConfS _appConfStore = appConfS;
        private readonly ModalNavS _modalNavS = modalNavS;

        protected override async Task ExecuteAsync(object? parameter)
        {
            _appConfStore.Save();
            await Task.CompletedTask;
            _modalNavS.Close();
        }
    }
}
