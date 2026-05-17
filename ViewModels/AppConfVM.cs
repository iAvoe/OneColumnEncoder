using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        // SSOT Store for app settings
        private readonly AppConfS _appConfStore;
        // Commands for UI interactions
        public ICommand CloseCmd { get; }
        public ICommand SaveCmd { get; }
        public ICommand LoadCmd { get; }
        // Expose settings for binding
        public AppConfS.GeneralSettings General => _appConfStore.General;
        public AppConfS.OverwriteSettings Overwrite => _appConfStore.Overwrite;
        public AppConfS.SmtpSettings Smtp => _appConfStore.Smtp;
        public AppConfVM(ModalNavS modalNavS, AppConfS appConfS)
        {
            CloseCmd = new CloseModalCmd(modalNavS);
            SaveCmd = new SaveAppConfCmd(appConfS);
            LoadCmd = new LoadAppConfCmd(appConfS);
            _appConfStore = appConfS;
        }
    }
}
