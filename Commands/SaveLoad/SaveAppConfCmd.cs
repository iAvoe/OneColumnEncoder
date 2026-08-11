using System.Diagnostics;

namespace OneColumnEncoder.Commands.SaveLoad;

public class SaveAppConfCmd(AppConfM appConfS, Action closeAction) : AppConfCmdBase(appConfS)
{
    protected override async Task ExecuteAsync(object? parameter)
    {
        UILangProvider.ValidateMissingTranslations(_appConfStore.Lang.LanguageCode);
        ApplyLangAndFont();
        _appConfStore.Save();
        await Task.CompletedTask;
        Debug.WriteLine($"[{nameof(SaveAppConfCmd)}] Translation self-check passed for '{UILangProvider.Current.LanguageCode}'.");
        closeAction();
    }
}
