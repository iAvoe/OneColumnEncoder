namespace OneColumnEncoder.ViewModels;

public class AppUsageVM : BaseVM
{
    public string WindowTitle => AppUsageLangProvider.WindowTitle;
    public CloseModalCmd? CloseCmd { get; }
    private AppUsageLangProvider _lang;
    public AppUsageLangProvider Lang
    {
        get => _lang;
        private set => SetProperty(ref _lang, value);
    }
    public AppUsageVM(AppConfM appConfM, Action closeAction)
    {
        CloseCmd = new CloseModalCmd(closeAction);
        _lang = new AppUsageLangProvider(appConfM.Lang.LanguageCode);
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged() =>
        Lang = new AppUsageLangProvider(UILangProvider.Current.LanguageCode);

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
    }
}
