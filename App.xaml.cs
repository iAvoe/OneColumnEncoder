using OneColumnEncoder.Commands;
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
        internal readonly ModalNavS _modalNavS;
        private readonly AppConfM _appConfS;
        private readonly AppDataM _appDataS;
        public App()
        {
            _modalNavS = new ModalNavS();
            _appConfS = AppConfM.Load();
            _appDataS = AppDataM.Load();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var openAppConf = new OpenAppConfCmd(_modalNavS, _appConfS);
            var openUsages = new OpenUsagesCmd(_modalNavS);

            MainWindow = new MainWindow()
            {
                DataContext = new MainVM(openAppConf, openUsages, _appDataS)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
