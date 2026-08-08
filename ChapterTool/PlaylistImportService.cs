using OneColumnEncoder.RepartManagement;
using System.IO;
using System.Threading;

namespace OneColumnEncoder.ChapterTool;

public sealed record PlaylistImportResult(
    DiscChapterReadResult Chapter,
    string PlaylistFolderPath,
    string[] SourcePaths);

public sealed record PlaylistImportStrings(
    string FolderDialogTitle,
    string ErrorWindowTitle,
    Func<string, IReadOnlyList<string>, string> ScanFailureFormatter,
    Func<string, string> ChapterImportFailedFormatter,
    string ChapterSourcesMissing);

public static class PlaylistImportService
{
    // Shared MPLS import pipeline used by both Repart mode and Queue mode:
    // PLAYLIST folder picker -> cluster/playlist scan -> cluster selection modal
    // -> chapter read -> resolve referenced media file paths.
    // Returns null if the user cancels any step.
    public static async Task<PlaylistImportResult?> ImportAsync(
        ModalNavS modalNavS,
        PlaylistImportStrings strings,
        CancellationToken cancellationToken = default)
    {
        OpenFolderDialog dialog = new()
        {
            Title = strings.FolderDialogTitle,
            Multiselect = false
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return null;

        string folderPath = dialog.FolderName;
        BdPlaylistScanResult playlistScan = await BdPlaylistScanner.ScanAsync(folderPath, cancellationToken);
        if (playlistScan.Clusters.Count == 0)
        {
            new OpenErrModalCmd(
                modalNavS,
                strings.ErrorWindowTitle,
                strings.ScanFailureFormatter(folderPath, playlistScan.Diagnostics)).Execute(null);
            return null;
        }

        while (true)
        {
            IReadOnlyList<string>? selectedPlaylistPaths = SelectPlaylistPaths(modalNavS, playlistScan);
            if (selectedPlaylistPaths == null || selectedPlaylistPaths.Count == 0) return null;

            DiscChapterReadResult chapterResult = await DiscChapterReader.TryReadCombinedAsync(selectedPlaylistPaths, cancellationToken);
            if ((!chapterResult.Success && !chapterResult.IsPartial) || chapterResult.Chapters.Count == 0)
            {
                new OpenErrModalCmd(
                    modalNavS,
                    strings.ErrorWindowTitle,
                    strings.ChapterImportFailedFormatter(string.Join(", ", selectedPlaylistPaths.Select(Path.GetFileName)))).Execute(null);
                continue;
            }

            string[] sourcePaths = chapterResult.ReferencedFilePaths.ToArray();
            if (sourcePaths.Length == 0)
            {
                new OpenErrModalCmd(
                    modalNavS,
                    strings.ErrorWindowTitle,
                    strings.ChapterSourcesMissing).Execute(null);
                continue;
            }

            return new PlaylistImportResult(chapterResult, folderPath, sourcePaths);
        }
    }

    public static IReadOnlyList<string>? SelectPlaylistPaths(ModalNavS modalNavS, BdPlaylistScanResult scan)
    {
        BdPlaylistSelectModal window = new();
        BdPlaylistSelectVM? vm = null;
        vm = new BdPlaylistSelectVM(
            scan,
            cancelAction: () =>
            {
                window.DialogResult = false;
                window.Close();
            },
            confirmAction: () =>
            {
                if (vm?.FinalPlaylistPaths is { Count: > 0 })
                {
                    window.DialogResult = true;
                    window.Close();
                }
            });

        window.DataContext = vm;
        window.Owner = Application.Current.MainWindow;
        window.Closed += (_, _) => modalNavS.Close();
        modalNavS.CurrentModalVM = vm;
        window.ShowDialog();
        return window.DialogResult == true ? vm?.FinalPlaylistPaths : null;
    }
}
