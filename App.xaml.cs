using System.IO;

namespace OneColumnEncoder;

/// <summary>
/// Application entry point: loads persisted app settings and data, wires up
/// unhandled-exception and startup-language handling, and hosts the main
/// window with its top-level ViewModel.
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

        // Fresh installs need one startup language probe; existing configs keep their saved state.
        if (!File.Exists(Path.Combine(AppConfM.GetConfigDirectory(), "appconfig.json")))
            _appConfM.InitLang = true;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            DispatcherUnhandledException += (_, ex) =>
            {
                new OpenErrModalCmd(_modalNavM, "Unhandled UI Exception", ex.Exception.ToString()).Execute(null);
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
            new OpenErrModalCmd(_modalNavM, "Startup Exception", ex.ToString()).Execute(null);
            Shutdown(-1);
        }
    }

    private void ApplyStartupLanguageOnce()
    {
        if (!_appConfM.InitLang) return;

        _appConfM.Lang.LanguageCode = ResolveSupportedLanguageCode(CultureInfo.CurrentUICulture);
        _appConfM.InitLang = false;
        _appConfM.Save();
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
