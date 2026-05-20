namespace OneColumnEncoder.Models;

public static class ConfirmationProviderM
{
    public static class ConfirmForceImport
    {
        public static string GetSuspiciousImportTitle(string toolName) =>
            $"Suspicious import for {toolName}";
        public static string GetPorceedToRunMessage(string toolName) =>
            $"Proceed to run {toolName} to get its version?";
        public static string GetWrongToolMessage(string toolName, string supposedName) =>
            $"Importing {toolName} for {supposedName}?";
    }
}
