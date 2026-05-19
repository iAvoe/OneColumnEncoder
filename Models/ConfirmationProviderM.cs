namespace OneColumnEncoder.Models;

public static class ConfirmationProviderM
{
    public static class ConfirmSuspiciousImport
    {
        public static string GetTitle(string toolName) =>
            $"Suspicious import for {toolName}";
        public static string GetMessage(string toolName) =>
            $"Proceed to run {toolName} to get its version?";
    }
}
