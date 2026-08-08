using System.IO;

namespace OneColumnEncoder.Models;

public sealed record BdPlaylistM(
    string Id,
    string FilePath,
    TimeSpan Duration,
    IReadOnlyList<string> Clips,
    IReadOnlyList<TimeSpan> Chapters)
{
    public string FileName => Path.GetFileName(FilePath);
    public string TitleText => $"{Id}.mpls";
    public int ClipCount => Clips.Count;
    public int ChapterCount => Chapters.Count;
    public string DurationText => FormatTimeSpan(Duration);
    public string ClipSequenceText => string.Join(" > ", Clips);
    public string ChapterTimestampsText => string.Join(", ", Chapters.Select(FormatTimeSpan));

    private static string FormatTimeSpan(TimeSpan value) => value.ToString(@"hh\:mm\:ss\.fff");
}

public sealed record BdPlaylistClusterM(
    string Key,
    TimeSpan Duration,
    IReadOnlyList<BdPlaylistM> Playlists)
{
    public int PlaylistCount => Playlists.Count;
    public int ClipCount => Playlists.Count > 0 ? Playlists[0].ClipCount : 0;
    public int ChapterCount => Playlists.Count > 0 ? Playlists[0].ChapterCount : 0;
    public string DurationText => FormatTimeSpan(Duration);
    public string ClipSequenceText => Playlists.Count > 0 ? Playlists[0].ClipSequenceText : string.Empty;
    public string SamplePlaylistIdsText => BuildSamplePlaylistIds();
    public string PlaylistIdsText => string.Join(", ", Playlists.Select(playlist => playlist.Id));

    private string BuildSamplePlaylistIds()
    {
        if (Playlists.Count == 0)
            return string.Empty;

        string[] sample = Playlists.Take(3).Select(playlist => playlist.Id).ToArray();
        if (Playlists.Count <= sample.Length)
            return string.Join(", ", sample);

        return string.Join(", ", sample) + $" (+{Playlists.Count - sample.Length})";
    }

    private static string FormatTimeSpan(TimeSpan value) => value.ToString(@"hh\:mm\:ss\.fff");
}

public sealed record BdPlaylistScanResult(
    bool Success,
    IReadOnlyList<BdPlaylistClusterM> Clusters,
    IReadOnlyList<string> Diagnostics)
{
    public static BdPlaylistScanResult Failed(IReadOnlyList<string> diagnostics) =>
        new(false, [], diagnostics);
}
