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
            try
            {
                DispatcherUnhandledException += (_, ex) =>
                {
                    MessageBox.Show(ex.Exception.ToString(), "Unhandled UI Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    ex.Handled = true;
                };

                ApplyStartupLanguageOnce();
                _ = new UILangProvider(_appConfM.Lang.LanguageCode);
                AppFontProvider.Refresh();
                AppFontProvider.ApplyFrom(_appConfM);
                OpenAppConfCmd openAppConf = new(_modalNavM, _appConfM);
                OpenUsagesCmd openUsages = new(_modalNavM, _appConfM);

                MainWindow = new MainWindow()
                {
                    DataContext = new MainVM(openAppConf, openUsages, _appDataM, _appConfM, _modalNavM)
                };
                MainWindow.Show();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Startup Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private void ApplyStartupLanguageOnce()
        {
            if (!_appConfM.IsFirstLaunch) return;

            _appConfM.Lang.LanguageCode = ResolveSupportedLanguageCode(CultureInfo.CurrentUICulture);
        }

        private static string ResolveSupportedLanguageCode(CultureInfo culture)
        {
            string name = culture.Name.ToLowerInvariant();
            string twoLetterName = culture.TwoLetterISOLanguageName.ToLowerInvariant();

            return name switch
            {
                "zh-cn" or "zh-sg" or "zh-hans" => "zh-cn",
                "zh-tw" or "zh-hk" or "zh-mo" or "zh-hant" => "zh-tw",
                _ when UICaptionProvider.AppConf.LanguageOptions.Codes.Contains(twoLetterName) => twoLetterName,
                _ => "en"
            };
        }
    }
}
