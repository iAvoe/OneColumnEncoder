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
        // Pass navigation to MainVM
        private readonly ModalNavS _modalNavS;
        private readonly AppConfS _appConfS;
        public App()
        {
            _modalNavS = new ModalNavS();
            _appConfS = new AppConfS();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new MainWindow()
            {
                DataContext = new MainVM(_modalNavS, _appConfS)
            };
            MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
