namespace OneColumnEncoder.ViewModels;

public class AppUsageVM : BaseVM
{
    public static string WindowTitle => AppUsageLangProvider.WindowTitle; // 0 references is not true
    private AppUsageLangProvider _lang;
    public AppUsageLangProvider Lang
    {
        get => _lang;
        private set => SetProperty(ref _lang, value);
    }
    public AppUsageVM(AppConfM appConfM)
    {
        _lang = new AppUsageLangProvider(appConfM.Lang.LanguageCode);
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged() =>
        Lang = new AppUsageLangProvider(UILangProvider.Current.LanguageCode);

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
