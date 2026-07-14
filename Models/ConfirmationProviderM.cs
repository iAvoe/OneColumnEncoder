namespace OneColumnEncoder.Models;

public static class ConfirmationProviderM
{
    public static class ConfirmForceImport
    {
        public static string GetSuspiciousImportTitle(string toolName) =>
            string.Format(UILangProvider.Current["ConfirmProvider.SuspiciousImportTitle"], toolName);
        public static string GetPorceedToRunMessage(string toolName) =>
            string.Format(UILangProvider.Current["ConfirmProvider.ProceedToRun"], toolName);
        public static string GetWrongToolMessage(string toolName, string supposedName) =>
            string.Format(UILangProvider.Current["ConfirmProvider.WrongTool"], toolName, supposedName);
    }
}