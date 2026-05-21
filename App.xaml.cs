using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace OneColumnEncoder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Pass navigation, app settings and tools imported to MainVM
        internal readonly ModalNavS _modalNavM;
        private readonly AppConfM _appConfM;
        private readonly AppDataM _appDataM;
        public App()
        {
            _modalNavM = new ModalNavS();
            _appConfM = AppConfM.Load();
            _appDataM = AppDataM.Load();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _ = new UILangProviderM(_appConfM.Lang.LanguageCode);
            OpenAppConfCmd openAppConf = new(_modalNavM, _appConfM);
            OpenUsagesCmd openUsages = new(_modalNavM, _appConfM);

            MainWindow = new MainWindow()
            {
                DataContext = new MainVM(openAppConf, openUsages, _appDataM, _appConfM, _modalNavM)
            };
            MainWindow.Show();
            base.OnStartup(e);
        }
    }
}
