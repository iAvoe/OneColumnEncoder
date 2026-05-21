using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class AppUsageVM : BaseVM
    {
        public CloseModalCmd? CloseCmd { get; }
        private AppUsageLangProviderM _lang;
        public AppUsageLangProviderM Lang
        {
            get => _lang;
            private set => SetProperty(ref _lang, value);
        }
        public AppUsageVM(ModalNavS modelNavS, AppConfM appConfM, Action closeAction)
        {
            CloseCmd = new CloseModalCmd(modelNavS, closeAction);
            _lang = new AppUsageLangProviderM(appConfM.Lang.LanguageCode);
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            Lang = new AppUsageLangProviderM(UILangProviderM.Current.LanguageCode);
        }

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
        }
    }
}
