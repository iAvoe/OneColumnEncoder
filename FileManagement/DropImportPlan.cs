using System.IO;

namespace OneColumnEncoder.FileManagement;

/// <summary>
/// Immutable description of a drag &amp; drop import request.
///
/// One existing file is routed to single-file mode, several files to queue mode.
/// Both modes share the same rejection rule: an imported file is refused only
/// when ffprobe cannot read it, so no extension pre-filter runs here either.
/// </summary>
public sealed class DropImportPlan
{
    private static DropImportLangProvider Lang =>
        new(UILangProvider.Current.LanguageCode);

    private DropImportPlan(string[] filePaths)
    {
        FilePaths = filePaths;
        FileNames = [.. filePaths
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];
        // Dropped files may live in different folders; the card shows the first folder.
        Directory = Path.GetDirectoryName(filePaths[0]) ?? string.Empty;
    }

    public string[] FilePaths { get; }
    public string[] FileNames { get; }
    public string Directory { get; }
    public int FileCount => FilePaths.Length;
    public bool IsSingle => FilePaths.Length == 1;
    public SrcRouteKind Route => IsSingle ? SrcRouteKind.Single : SrcRouteKind.Queue;

    public static string ImportDisabledMessage => Lang.ImportDisabled;

    /// <summary>
    /// Target description used by the hover overlay and the replace confirmation,
    /// e.g. "Single-file mode" or "Queue mode (3 files)".
    /// </summary>
    public string ModeText => BuildModeText(FileCount);

    public string ReplaceTitle => Lang.ReplaceTitle;

    public string ReplaceMessage => string.Format(Lang.ReplaceMessage, ModeText);

    /// <summary>
    /// Overlay hint for a hovered payload. A non-positive count means the payload
    /// carries no usable file yet, so no target mode is advertised.
    /// </summary>
    public static string BuildTargetHint(int fileCount) =>
        fileCount <= 0
            ? Lang.HintPlain
            : string.Format(Lang.Hint, BuildModeText(fileCount));

    public static string BuildModeText(int fileCount) =>
        fileCount == 1
            ? Lang.ModeSingle
            : string.Format(Lang.ModeQueue, fileCount);

    /// <summary>
    /// Classifies the dropped paths without touching any UI state. Returns null and a
    /// ready-to-show message when the payload carries no usable file.
    /// </summary>
    public static DropImportPlan? TryCreate(
        IEnumerable<string> droppedPaths,
        out string refusalMessage)
    {
        string[] filePaths = [.. droppedPaths.Where(File.Exists)];
        if (filePaths.Length == 0)
        {
            refusalMessage = Lang.NoVideoFile;
            return null;
        }

        refusalMessage = string.Empty;
        return new DropImportPlan(filePaths);
    }

    /// <summary>
    /// Narrows the plan to the files that survived the ffprobe gate, so the card text,
    /// folder and file list all describe what is actually imported.
    /// </summary>
    public DropImportPlan RestrictTo(IReadOnlyList<string> readablePaths)
    {
        if (FilePaths.SequenceEqual(readablePaths)) return this;
        return new DropImportPlan([.. readablePaths]);
    }
}
