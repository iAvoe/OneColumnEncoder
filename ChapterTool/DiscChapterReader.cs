using ChapterTool.Core.Editing;
using ChapterTool.Core.Importing;
using ChapterTool.Core.Models;
using System.IO;

namespace OneColumnEncoder.ChapterTool
{
    public sealed record DiscChapterMarker(
        int DisplayNumber,
        TimeSpan StartTime,
        TimeSpan? EndTime,
        string Name,
        bool IsSeparator);

    public sealed record DiscChapterReadResult(
        bool Success,
        bool IsPartial,
        string? SourceName,
        string? Title,
        TimeSpan Duration,
        double FramesPerSecond,
        string? ImportFormatCode,
        IReadOnlyList<DiscChapterMarker> Chapters,
        IReadOnlyList<string> ReferencedFilePaths,
        IReadOnlyList<string> Diagnostics)
    {
        public static DiscChapterReadResult Failed(IReadOnlyList<string> diagnostics) =>
            new(false, false, null, null, TimeSpan.Zero, 0, null, [], [], diagnostics);
    }

    public static class DiscChapterReader
    {
        private static readonly string[] SupportedExtensions = [".mpls", ".ifo", ".xpl"];

        public static bool IsSupportedExtension(string filePath) =>
            SupportedExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);

        public static async Task<DiscChapterReadResult> TryReadDirectoryAsync(
            string directoryPath,
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(directoryPath))
                return DiscChapterReadResult.Failed([$"Chapter source folder does not exist: {directoryPath}"]);

            string[] sourcePaths = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
                .Where(IsSupportedExtension)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (sourcePaths.Length == 0)
                return DiscChapterReadResult.Failed(["No supported chapter files were found in the selected folder."]);

            List<DiscChapterReadResult> candidates = [];
            List<string> diagnostics = [];
            foreach (string sourcePath in sourcePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DiscChapterReadResult result = await TryReadAsync(sourcePath, cancellationToken);
                if (result.Chapters.Count > 0
                    && result.ReferencedFilePaths.Count > 0
                    && (result.Success || result.IsPartial))
                {
                    candidates.Add(result);
                }
                else
                {
                    diagnostics.AddRange(result.Diagnostics.Select(message =>
                        $"{Path.GetFileName(sourcePath)}: {message}"));
                }
            }

            // Prefer the playlist that maps to the most distinct media files:
            // that is the strongest signal of a genuine multi-source episode set.
            // A looping playlist that repeats one file can otherwise win on raw
            // duration even though it only references a single source.
            return candidates
                .OrderByDescending(result => result.ReferencedFilePaths.Count)
                .ThenByDescending(result => result.Duration)
                .ThenByDescending(result => result.Chapters.Count)
                .FirstOrDefault()
                ?? DiscChapterReadResult.Failed(
                    diagnostics.Count > 0
                        ? diagnostics
                        : ["No usable chapter playlist with at least two source videos was found."]);
        }

        public static async Task<DiscChapterReadResult> TryReadCombinedAsync(
            IReadOnlyList<string> filePaths,
            CancellationToken cancellationToken = default)
        {
            if (filePaths.Count == 0)
                return DiscChapterReadResult.Failed(["No playlist files were provided."]);

            List<DiscChapterReadResult> results = [];
            List<string> diagnostics = [];
            foreach (string filePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DiscChapterReadResult result = await TryReadAsync(filePath, cancellationToken);
                if (result.Chapters.Count == 0 || result.ReferencedFilePaths.Count == 0)
                {
                    diagnostics.AddRange(result.Diagnostics);
                    continue;
                }
                results.Add(result);
            }

            if (results.Count == 0)
                return DiscChapterReadResult.Failed(
                    diagnostics.Count > 0 ? diagnostics : ["None of the selected playlists resolved usable chapters."]);

            DiscChapterReadResult first = results[0];
            TimeSpan totalDuration = TimeSpan.Zero;
            List<DiscChapterMarker> markers = [];
            List<string> referencedPaths = [];
            int displayNumber = 1;

            foreach (DiscChapterReadResult result in results)
            {
                foreach (DiscChapterMarker marker in result.Chapters)
                {
                    markers.Add(new DiscChapterMarker(
                        displayNumber++,
                        marker.StartTime + totalDuration,
                        marker.EndTime.HasValue ? marker.EndTime + totalDuration : null,
                        marker.Name,
                        marker.IsSeparator));
                }

                foreach (string path in result.ReferencedFilePaths)
                {
                    if (!referencedPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
                        referencedPaths.Add(path);
                }

                totalDuration += result.Duration;
            }

            return new DiscChapterReadResult(
                results.All(result => result.Success),
                results.Any(result => result.IsPartial),
                first.SourceName,
                first.Title,
                totalDuration,
                first.FramesPerSecond,
                first.ImportFormatCode,
                markers,
                referencedPaths,
                diagnostics);
        }

        public static async Task<DiscChapterReadResult> TryReadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (!IsSupportedExtension(filePath))
                return DiscChapterReadResult.Failed([$"Unsupported chapter source: {filePath}"]);

            try
            {
                var service = new ChapterContentService();
                byte[] content = await File.ReadAllBytesAsync(filePath, cancellationToken);
                ChapterImportResult result = await service.ImportAsync(filePath, content, cancellationToken);

                var diagnostics = result.Diagnostics
                    .Select(item => string.IsNullOrWhiteSpace(item.DisplayCode)
                        ? item.Message
                        : $"{item.DisplayCode}: {item.Message}")
                    .ToList();

                // Prefer a source group that maps to multiple media files (a
                // multi-section playlist). Combine its chapter sets so chapter
                // times stay relative to the start of the whole source, and
                // collect the media files referenced by every entry.
                foreach (ChapterImportSource source in result.Groups)
                {
                    if (source.Entries.Count == 0)
                        continue;

                    ChapterSet? chapterSet;
                    List<ReferencedMediaFile> mediaFiles = [];

                    if (source.Entries.Count > 1 && source.Entries.Any(entry => entry.CanCombine))
                    {
                        ChapterEditResult combined = ChapterSegmentService.Combine(source);
                        chapterSet = combined.ChapterSet;
                        foreach (ChapterImportEntry entry in source.Entries)
                        {
                            if (entry.ReferencedMediaFiles != null)
                                mediaFiles.AddRange(entry.ReferencedMediaFiles);
                        }
                    }
                    else
                    {
                        int index = Math.Clamp(source.DefaultEntryIndex, 0, source.Entries.Count - 1);
                        ChapterImportEntry entry = source.Entries[index];
                        chapterSet = entry.ChapterSet;
                        if (entry.ReferencedMediaFiles != null)
                            mediaFiles.AddRange(entry.ReferencedMediaFiles);
                    }

                    if (chapterSet == null)
                        continue;

                    IReadOnlyList<string> referencedFilePaths = ResolveReferencedPaths(mediaFiles, filePath);
                    if (referencedFilePaths.Count == 0)
                        continue;

                    var markers = chapterSet.Chapters
                        .Select(chapter => new DiscChapterMarker(
                            chapter.DisplayNumber,
                            chapter.StartTime,
                            chapter.EndTime,
                            chapter.Name,
                            chapter.IsSeparator))
                        .ToList();

                    return new DiscChapterReadResult(
                        result.Success,
                        result.IsPartial,
                        chapterSet.SourceName,
                        chapterSet.Title,
                        chapterSet.Duration,
                        chapterSet.FramesPerSecond,
                        ChapterImportFormats.Code(chapterSet.ImportFormat),
                        markers,
                        referencedFilePaths,
                        diagnostics);
                }

                diagnostics.Insert(0, "No chapter set was discovered in the source.");
                return DiscChapterReadResult.Failed(diagnostics);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return DiscChapterReadResult.Failed([exception.Message]);
            }
        }

        private static IReadOnlyList<string> ResolveReferencedPaths(
            IEnumerable<ReferencedMediaFile> mediaFiles,
            string chapterFilePath)
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(chapterFilePath));
            List<string> paths = [];
            foreach (ReferencedMediaFile media in mediaFiles)
            {
                string? candidate = null;
                if (!string.IsNullOrWhiteSpace(media.AbsolutePath) && File.Exists(media.AbsolutePath))
                    candidate = Path.GetFullPath(media.AbsolutePath);
                else if (directory != null && !string.IsNullOrWhiteSpace(media.RelativePath))
                {
                    string combined = Path.Combine(directory, media.RelativePath);
                    if (File.Exists(combined))
                        candidate = Path.GetFullPath(combined);
                }

                if (candidate != null && !paths.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                    paths.Add(candidate);
            }

            return paths;
        }
    }
}
