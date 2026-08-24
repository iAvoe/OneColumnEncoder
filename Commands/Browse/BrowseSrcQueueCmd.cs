using OneColumnEncoder.ChapterTool;
using System.IO;

namespace OneColumnEncoder.Commands.Browse;

public class BrowseSrcQueueCmd(
    ToolItemCardVM item,
    ModalNavS modalNavS,
    AppDataM appDataM,
    string browseKey,
    Action<ToolItemCardVM, string, string[]>? afterImport = null) : BrowseCmdBase(item)
{
    private readonly ModalNavS _modalNavS = modalNavS;
    private readonly AppDataM _appDataM = appDataM;
    private readonly string _browseKey = browseKey;
    private readonly Action<ToolItemCardVM, string, string[]>? _afterImport = afterImport;

    public override async void Execute(object? parameter)
    {
        // Ask how the user wants to import: confirm = resolve a BluRay playlist
        // (MPLS), cancel = pick multiple video source files directly.
        bool importAsPlaylist = QueueChapterImportPrompt.Confirm(_modalNavS);

        string pathText;
        string[] filePaths;
        if (importAsPlaylist)
        {
            PlaylistImportResult? import = await PlaylistImportService.ImportAsync(
                _modalNavS,
                new PlaylistImportStrings(
                    UILangProvider.Current["SourceQueue.SelectPlaylistFolder"],
                    UILangProvider.Current["SourceQueue.ImportTitle"],
                    BuildPlaylistScanFailureMessage,
                    fileName => string.Format(UILangProvider.Current["SourceQueue.ChapterImportFailed"], fileName),
                    UILangProvider.Current["SourceQueue.ChapterSourcesMissing"]));
            if (import == null) return;

            pathText = import.PlaylistFolderPath;
            filePaths = import.srcPaths;
        }
        else
        {
            OpenFileDialog dialog = new()
            {
                Title = UILangProvider.Current["SourceQueue.SelectFilesTitle"],
                Filter = new SrcFilePickerLangProvider(UILangProvider.Current.LanguageCode).VideoFilter,
                InitialDirectory = BrowseHistory.ResolveInitialDirectory(_appDataM, _browseKey, _item.P2TextData),
                Multiselect = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ShowDialog(dialog) != true) return;

            filePaths = GetVideoFiles(dialog.FileNames);
            if (filePaths.Length == 0)
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    UICaptionProvider.SourceInspect.WarnTitle,
                    new VideoSrcQueueLangProvider(UILangProvider.Current.LanguageCode)["SourceQueue.EmptyFolderWarnMessage"])
                    .Execute(null);
                return;
            }

            pathText = GetSelectedFolderPath(filePaths);
            if (string.IsNullOrWhiteSpace(pathText))
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UICaptionProvider.SourceInspect.ErrorTitle,
                    new VideoSrcQueueLangProvider(UILangProvider.Current.LanguageCode)["SourceQueue.MixedFolderErrorMessage"])
                    .Execute(null);
                ActivateMainWindow();
                return;
            }
        }

        SetQueueCardText(pathText, filePaths);
        Remember(_appDataM, _browseKey, pathText);
        _afterImport?.Invoke(_item, pathText, filePaths);
        ActivateMainWindow();
    }

    private static string BuildPlaylistScanFailureMessage(string folderPath, IReadOnlyList<string> diagnostics)
    {
        List<string> lines = [$"No usable MPLS playlists were found in: {folderPath}"];
        lines.AddRange(diagnostics.Take(8));
        return string.Join(Environment.NewLine, lines);
    }

    private static string[] GetVideoFiles(IEnumerable<string> filePaths)
    {
        HashSet<string> extensions = new(
            SrcFilePickerLangProvider.VideoExtensions
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(extension => extension.TrimStart('*')),
            StringComparer.OrdinalIgnoreCase);

        return [.. filePaths.Where(filePath =>
            extensions.Contains(Path.GetExtension(filePath) ?? string.Empty))];
    }

    private static string GetSelectedFolderPath(string[] filePaths)
    {
        if (filePaths.Length == 0) return string.Empty;

        string? firstDirectory = Path.GetDirectoryName(filePaths[0]);
        if (string.IsNullOrWhiteSpace(firstDirectory)) return string.Empty;

        foreach (string filePath in filePaths.Skip(1))
        {
            if (!string.Equals(firstDirectory, Path.GetDirectoryName(filePath), StringComparison.OrdinalIgnoreCase))
                return string.Empty;
        }

        return firstDirectory;
    }
}
