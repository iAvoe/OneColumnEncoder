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
        private readonly ModalNavS _modalNavS;
        private readonly AppConfS _appConfS;
        private readonly AppDataS _appDataS;
        public App()
        {
            _modalNavS = new ModalNavS();
            _appConfS = AppConfS.Load();
            _appDataS = AppDataS.Load();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new MainWindow()
            {
                DataContext = new MainVM(_modalNavS, _appConfS, _appDataS)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
