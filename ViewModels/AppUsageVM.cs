using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class AppUsageVM : BaseVM
    {
        public CloseModalCmd? CloseCmd { get; }
        public AppUsageLangProviderM Lang { get; }
        public AppUsageVM(ModalNavS modelNavS, AppConfM appConfM, Action closeAction)
        {
            CloseCmd = new CloseModalCmd(modelNavS, closeAction);
            Lang = new AppUsageLangProviderM(appConfM.Lang.LanguageCode);
        }
    }
}
