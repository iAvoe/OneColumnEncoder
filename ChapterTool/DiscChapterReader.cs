using System.IO;
using ChapterTool.Core.Importing;
using ChapterTool.Core.Models;

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

                (ChapterImportEntry? entry, ChapterSet? chapterSet) = PickDefaultChapterSet(result);
                if (entry == null || chapterSet == null)
                {
                    diagnostics.Insert(0, "No chapter set was discovered in the source.");
                    return DiscChapterReadResult.Failed(diagnostics);
                }

                var markers = chapterSet.Chapters
                    .Select(chapter => new DiscChapterMarker(
                        chapter.DisplayNumber,
                        chapter.StartTime,
                        chapter.EndTime,
                        chapter.Name,
                        chapter.IsSeparator))
                    .ToList();

                IReadOnlyList<string> referencedFilePaths = ResolveReferencedPaths(entry, filePath);

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
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                return DiscChapterReadResult.Failed([exception.Message]);
            }
        }

        private static (ChapterImportEntry? Entry, ChapterSet? ChapterSet) PickDefaultChapterSet(ChapterImportResult result)
        {
            foreach (ChapterImportSource source in result.Groups)
            {
                if (source.Entries.Count == 0)
                    continue;

                int index = Math.Clamp(source.DefaultEntryIndex, 0, source.Entries.Count - 1);
                ChapterImportEntry entry = source.Entries[index];
                return (entry, entry.ChapterSet);
            }

            return (null, null);
        }

        private static IReadOnlyList<string> ResolveReferencedPaths(ChapterImportEntry entry, string chapterFilePath)
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(chapterFilePath));
            List<string> paths = [];
            foreach (ReferencedMediaFile media in entry.ReferencedMediaFiles ?? [])
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
