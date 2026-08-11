using OneColumnEncoder.ChapterTool;

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
        // (MPLS), cancel = pick a folder of all video sources.
        bool importAsPlaylist = QueueChapterImportPrompt.Confirm(_modalNavS);

        string folderPath;
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

            folderPath = import.PlaylistFolderPath;
            filePaths = import.srcPaths;
        }
        else
        {
            OpenFolderDialog dialog = new()
            {
                Title = UILangProvider.Current["SourceQueue.SelectFolderTitle"],
                InitialDirectory = BrowseHistory.ResolveInitialDirectory(_appDataM, _browseKey, _item.P2TextData)
            };

            if (ShowDialog(dialog) != true) return;

            folderPath = dialog.FolderName;
            filePaths = SrcFilePicker.GetVideoFilesInFolder(folderPath);
            if (filePaths.Length == 0)
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    UICaptionProvider.SourceInspect.WarnTitle,
                    new VideoSrcQueueLangProvider(UILangProvider.Current.LanguageCode)["SourceQueue.EmptyFolderWarnMessage"])
                    .Execute(null);
                return;
            }
        }

        SetQueueCardText(folderPath, filePaths);
        Remember(_appDataM, _browseKey, folderPath);
        _afterImport?.Invoke(_item, folderPath, filePaths);
        ActivateMainWindow();
    }

    private static string BuildPlaylistScanFailureMessage(string folderPath, IReadOnlyList<string> diagnostics)
    {
        List<string> lines = [$"No usable MPLS playlists were found in: {folderPath}"];
        lines.AddRange(diagnostics.Take(8));
        return string.Join(Environment.NewLine, lines);
    }
}
