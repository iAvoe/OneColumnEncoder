using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class SaveAppConfCmd : AsyncBaseCmd
    {
        private readonly AppConfS _appConfStore;
        public SaveAppConfCmd(AppConfS appConfS)
        {
            _appConfStore = appConfS;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            _appConfStore.Save();
            await Task.CompletedTask;
        }
    }
}
