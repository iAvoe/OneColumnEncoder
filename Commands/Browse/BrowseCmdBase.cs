using System.IO;

namespace OneColumnEncoder.Commands.Browse;

public abstract class BrowseCmdBase(ToolItemCardVM item) : BaseCmd
{
    protected readonly ToolItemCardVM _item = item;

    protected static bool? ShowDialog(CommonDialog dialog)
    {
        Window? owner = Application.Current.MainWindow;
        return owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
    }

    protected void SetQueueCardText(string displayPath, string[] filePaths)
    {
        string[] fileNames = [.. filePaths
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];
        _item.P2TextData = displayPath;
        _item.P1TextData = FormatQueueP1Text(fileNames);
        _item.P1TooltipText = FormatQueueP1TooltipText(fileNames);
    }

    protected static void Remember(AppDataM? appDataM, string? browseKey, string path)
    {
        if (browseKey != null && appDataM != null)
            BrowseHistory.Remember(appDataM, browseKey, path);
    }

    protected static void ActivateMainWindow() => Application.Current.MainWindow?.Activate();

    public static string FormatQueueP1Text(IEnumerable<string> fileNames)
    {
        string[] names = [.. fileNames];
        if (names.Length == 0) return string.Empty;

        static string Prefix(string fileName)
        {
            const int maxLength = 12;
            string name = Path.GetFileNameWithoutExtension(fileName) ?? fileName;
            return name.Length <= maxLength ? name : name[..maxLength];
        }

        if (names.Length == 1) return Prefix(names[0]);

        return $"{Prefix(names[0])}..{Prefix(names[^1])}";
    }

    public static string FormatConcatFileName(IEnumerable<string> fileNames)
    {
        string[] names = [.. fileNames
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];

        if (names.Length == 0) return "concat";
        if (names.Length == 1) return names[0];

        string first = names[0];
        string last = names[^1];
        return first.Equals(last, StringComparison.OrdinalIgnoreCase)
            ? first
            : $"{first}..{last}";
    }

    // Generates a comma-separated file list for tooltip display (up to maxLength chars).
    // Unlike FormatQueueP1Text which truncates each name to 12 chars, this preserves
    // full file names so users can see the complete queue contents on hover.
    public static string FormatQueueP1TooltipText(IEnumerable<string> fileNames, int maxLength = 512)
    {
        string[] names = [.. fileNames
            .Select(f => Path.GetFileName(f))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)];
        if (names.Length == 0 || maxLength <= 0) return string.Empty;

        string result = string.Join(", ", names);
        if (result.Length <= maxLength) return result;
        if (maxLength <= 3) return result[..maxLength];
        return result[..(maxLength - 3)] + "...";
    }
}
