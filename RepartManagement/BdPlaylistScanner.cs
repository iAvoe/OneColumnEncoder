using ChapterTool.Core.Editing;
using ChapterTool.Core.Importing;
using ChapterTool.Core.Importing.Disc;
using ChapterTool.Core.Models;
using OneColumnEncoder.Models;
using System.Globalization;
using System.IO;

namespace OneColumnEncoder.RepartManagement;

public static class BdPlaylistScanner
{
    public static async Task<BdPlaylistScanResult> ScanAsync(
        string folderPath,
        CancellationToken cancellationToken = default)
    {
        string playlistDirectory = ResolvePlaylistDirectory(folderPath);
        if (!Directory.Exists(playlistDirectory))
            return BdPlaylistScanResult.Failed([$"PLAYLIST folder does not exist: {playlistDirectory}"]);

        string[] playlistPaths = Directory.EnumerateFiles(playlistDirectory, "*.mpls", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (playlistPaths.Length == 0)
            return BdPlaylistScanResult.Failed([$"No MPLS playlist files were found in: {playlistDirectory}"]);

        var importer = new MplsChapterImporter();
        List<BdPlaylistM> playlists = [];
        List<string> diagnostics = [];

        foreach (string playlistPath in playlistPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                ChapterImportResult result = await importer.ImportAsync(new ChapterImportRequest(playlistPath), cancellationToken);
                if (result.Groups.Count == 0)
                {
                    diagnostics.Add($"{Path.GetFileName(playlistPath)}: no chapter groups were discovered.");
                    continue;
                }

                foreach (ChapterImportSource source in result.Groups)
                {
                    BdPlaylistM? playlist = BuildPlaylist(playlistPath, source, diagnostics);
                    if (playlist != null)
                        playlists.Add(playlist);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                diagnostics.Add($"{Path.GetFileName(playlistPath)}: {exception.Message}");
            }
        }

        List<BdPlaylistClusterM> clusters = playlists
            .GroupBy(playlist => BuildClusterKey(playlist))
            .Select(group => new BdPlaylistClusterM(
                group.Key,
                group.First().Duration,
                [.. group.OrderBy(playlist => playlist.Id, StringComparer.OrdinalIgnoreCase)]))
            .OrderByDescending(cluster => cluster.Duration)
            .ThenByDescending(cluster => cluster.ClipCount)
            .ThenByDescending(cluster => cluster.ChapterCount)
            .ThenBy(cluster => cluster.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return clusters.Count > 0
            ? new BdPlaylistScanResult(true, clusters, diagnostics)
            : BdPlaylistScanResult.Failed(diagnostics.Count > 0 ? diagnostics : [$"No usable playlists were found in: {playlistDirectory}"]);
    }

    private static BdPlaylistM? BuildPlaylist(
        string playlistPath,
        ChapterImportSource source,
        ICollection<string> diagnostics)
    {
        if (source.Entries.Count == 0)
            return null;

        try
        {
            ChapterSet chapterSet = source.Entries.Count > 1 && source.Entries.Any(entry => entry.CanCombine)
                ? ChapterSegmentService.Combine(source).ChapterSet
                : source.Entries[Math.Clamp(source.DefaultEntryIndex, 0, source.Entries.Count - 1)].ChapterSet;

            List<string> clips = source.Entries
                .Select(GetClipId)
                .Where(clip => !string.IsNullOrWhiteSpace(clip))
                .ToList();
            if (clips.Count == 0)
                clips.Add(Path.GetFileNameWithoutExtension(playlistPath));

            List<TimeSpan> chapters = chapterSet.Chapters
                .Select(chapter => chapter.StartTime)
                .ToList();

            return new BdPlaylistM(
                Path.GetFileNameWithoutExtension(playlistPath),
                Path.GetFullPath(playlistPath),
                chapterSet.Duration,
                clips,
                chapters);
        }
        catch (Exception exception)
        {
            diagnostics.Add($"{Path.GetFileName(playlistPath)}: {exception.Message}");
            return null;
        }
    }

    private static string GetClipId(ChapterImportEntry entry)
    {
        string? sourceName = entry.ChapterSet.SourceName;
        if (!string.IsNullOrWhiteSpace(sourceName))
            return sourceName.Trim();

        string? referenced = entry.ReferencedMediaFiles?.FirstOrDefault()?.DisplayName;
        if (!string.IsNullOrWhiteSpace(referenced))
            return Path.GetFileNameWithoutExtension(referenced);

        return entry.Id;
    }

    private static string BuildClusterKey(BdPlaylistM playlist)
    {
        long durationSeconds = (long)Math.Round(playlist.Duration.TotalSeconds);
        return string.Join("|", [
            durationSeconds.ToString(CultureInfo.InvariantCulture),
            playlist.ClipCount.ToString(CultureInfo.InvariantCulture),
            playlist.ChapterCount.ToString(CultureInfo.InvariantCulture),
            playlist.ClipSequenceText]);
    }

    private static string ResolvePlaylistDirectory(string folderPath)
    {
        string fullPath = Path.GetFullPath(folderPath);
        if (string.Equals(Path.GetFileName(fullPath), "PLAYLIST", StringComparison.OrdinalIgnoreCase))
            return fullPath;

        string playlistPath = Path.Combine(fullPath, "PLAYLIST");
        if (Directory.Exists(playlistPath))
            return playlistPath;

        return fullPath;
    }
}
