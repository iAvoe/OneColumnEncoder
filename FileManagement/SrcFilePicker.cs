using System.IO;
namespace OneColumnEncoder.FileManagement;

public enum SrcFileKind
{
    Video,
    AviSynthScript,
    VapourSynthScript,
    SvfiIni
}

public static partial class SrcFilePicker
{
    private static SrcFilePickerLangProvider Lang =>
        new(UILangProvider.Current.LanguageCode);

    public static string? GetSource(
        SrcFileKind fileKind,
        string windowTitle,
        string? foundPath = null,
        string? currentPath = null)
    {
        string initialDirectory = ResolveInitialDirectory(foundPath, currentPath);
        string filter = GetFilter(fileKind);

        return SelectFile(windowTitle, filter, initialDirectory);
    }

    public static string GetPrimaryText(SrcFileKind fileKind, string filePath)
    {
        return fileKind == SrcFileKind.Video
            ? Path.GetFileName(filePath)
            : GetCustomScriptModeText();
    }

    public static string[] GetVideoFilesInFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return [];

        string[] extensions = [.. SrcFilePickerLangProvider.VideoExtensions
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(extension => extension.TrimStart('*').ToLowerInvariant())];

        return [.. Directory.EnumerateFiles(folderPath)
            .Where(filePath => extensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()))
            .OrderBy(filePath => filePath, NaturalFilePathComparer.Instance)];
    }

    public static string[] GetSourceFilesInFolder(string folderPath, SrcFileKind fileKind)
    {
        if (fileKind == SrcFileKind.Video)
            return GetVideoFilesInFolder(folderPath);

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return [];

        string extension = fileKind switch
        {
            SrcFileKind.AviSynthScript => ".avs",
            SrcFileKind.VapourSynthScript => ".vpy",
            SrcFileKind.SvfiIni => ".ini",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(extension)) return [];

        return [.. Directory.EnumerateFiles(folderPath)
            .Where(filePath => Path.GetExtension(filePath).Equals(extension, StringComparison.OrdinalIgnoreCase))
            .OrderBy(filePath => filePath, NaturalFilePathComparer.Instance)];
    }

    private sealed partial class NaturalFilePathComparer : IComparer<string>
    {
        public static NaturalFilePathComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            string xName = Path.GetFileName(x ?? string.Empty);
            string yName = Path.GetFileName(y ?? string.Empty);
            int result = LibImportProvider.CompareLogical(xName, yName);
            return result != 0
                ? result
                : StringComparer.OrdinalIgnoreCase.Compare(x, y);
        }
    }

    private static string? SelectFile(string title, string filter, string? initialDirectory)
    {
        OpenFileDialog dialog = new()
        {
            Title = title,
            Filter = filter,
            InitialDirectory = NormalizeInitialDirectory(initialDirectory),
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true
            ? dialog.FileName
            : null;
    }

    // Note: SrcFileKind fileKind, is nolonger used
    private static string ResolveInitialDirectory(string? foundPath, string? currentPath)
    {
        if (!string.IsNullOrWhiteSpace(currentPath))
            return currentPath;

        if (!string.IsNullOrWhiteSpace(foundPath))
            return foundPath;

        return string.Empty;
    }

    private static string NormalizeInitialDirectory(string? initialDirectory)
    {
        string fallbackDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        if (string.IsNullOrWhiteSpace(initialDirectory))
            return fallbackDirectory;

        if (File.Exists(initialDirectory))
        {
            string? parentDir = Path.GetDirectoryName(initialDirectory);
            return Directory.Exists(parentDir) ? parentDir : fallbackDirectory;
        }

        return Directory.Exists(initialDirectory) ? initialDirectory : fallbackDirectory;
    }

    private static string GetFilter(SrcFileKind fileKind)
    {
        SrcFilePickerLangProvider lang = Lang;

        return fileKind switch
        {
            SrcFileKind.Video => lang.VideoFilter,
            SrcFileKind.AviSynthScript => lang.AviSynthScriptFilter,
            SrcFileKind.VapourSynthScript => lang.VapourSynthScriptFilter,
            SrcFileKind.SvfiIni => lang.SvfiIniFilter,
            _ => lang.AllFilesFilter
        };
    }

    private static string GetCustomScriptModeText() => Lang.CustomScriptModeText;
}
