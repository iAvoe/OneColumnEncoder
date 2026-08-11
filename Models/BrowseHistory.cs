using System.IO;

namespace OneColumnEncoder.Models;

// Stable per-card keys for the last-browse directory history stored in AppDataM.BrowseHistory.
// Keys must stay independent of localized display names so history survives language switches.
/// <summary>
/// Browse-history keys and lookup helpers.
/// </summary>
public static class BrowseHistoryKeys
{
    // Video / script single-file source cards
    public const string VideoSrc = "source.video.single";
    public const string AviSynthScript = "source.script.avs";
    public const string VapourSynthScript = "source.script.vpy";
    public const string SvfiIni = "source.script.svfi";
    // Video / script queue source cards
    public const string VideoSrcQueue = "source.video.queue";
    public const string VideoSrcConcat = "source.video.concat";
    public const string AviSynthScriptQueue = "source.script.avs.queue";
    public const string VapourSynthScriptQueue = "source.script.vpy.queue";

    public static string ForSingleSource(SrcFileKind kind) => kind switch
    {
        SrcFileKind.Video => VideoSrc,
        SrcFileKind.AviSynthScript => AviSynthScript,
        SrcFileKind.VapourSynthScript => VapourSynthScript,
        SrcFileKind.SvfiIni => SvfiIni,
        _ => VideoSrc
    };

    public static string ForScriptQueue(SrcFileKind kind) => kind switch
    {
        SrcFileKind.AviSynthScript => AviSynthScriptQueue,
        SrcFileKind.VapourSynthScript => VapourSynthScriptQueue,
        _ => VideoSrcQueue
    };

    public static string ForTool(string exeName) => $"tool.{exeName}";
}

/// <summary>
/// Resolves and stores last-used browse directories.
/// </summary>
public static class BrowseHistory
{
    public static string? GetDirectory(AppDataM appData, string key)
    {
        if (appData == null || string.IsNullOrEmpty(key)) return null;
        return appData.BrowseHistory.TryGetValue(key, out string? directory) ? directory : null;
    }

    // History wins; otherwise fall back to the existing per-card path heuristic.
    public static string ResolveInitialDirectory(AppDataM appData, string key, string? fallbackPath)
    {
        string? historyDirectory = GetDirectory(appData, key);
        if (!string.IsNullOrWhiteSpace(historyDirectory) && Directory.Exists(historyDirectory))
            return historyDirectory;
        return OutputPath.GetInitialDirectory(fallbackPath);
    }

    public static void Remember(AppDataM appData, string key, string fileOrFolderPath)
    {
        if (appData == null || string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(fileOrFolderPath)) return;

        string? directory = Directory.Exists(fileOrFolderPath)
            ? fileOrFolderPath
            : Path.GetDirectoryName(fileOrFolderPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;

        appData.BrowseHistory[key] = directory;
        appData.Save();
    }
}
