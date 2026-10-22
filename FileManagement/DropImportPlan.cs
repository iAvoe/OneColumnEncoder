using System.IO;

namespace OneColumnEncoder.FileManagement;

public enum DropImportProblem
{
    None,
    NoVideoFile,
    UnsupportedFiles
}

/// <summary>
/// Immutable description of a drag &amp; drop import request.
///
/// One existing file is routed to single-file mode, several files to queue mode.
/// Single-file mode deliberately skips the second extension check, queue mode
/// rejects the whole import when any file is outside the video whitelist.
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
    /// Classifies the dropped paths without touching any UI state.
    /// </summary>
    public static DropImportResult Create(IEnumerable<string> droppedPaths)
    {
        string[] filePaths = [.. droppedPaths.Where(File.Exists)];
        if (filePaths.Length == 0)
        {
            return new DropImportResult(
                Plan: null,
                Problem: DropImportProblem.NoVideoFile,
                Message: Lang.NoVideoFile,
                IsError: false);
        }

        if (filePaths.Length > 1 && SrcFilePicker.GetUnsupportedVideoFiles(filePaths).Length > 0)
        {
            return new DropImportResult(
                Plan: null,
                Problem: DropImportProblem.UnsupportedFiles,
                Message: SrcFilePicker.GetUnsupportedVideoFilesMessage(filePaths) ?? string.Empty,
                IsError: true);
        }

        return new DropImportResult(
            Plan: new DropImportPlan(filePaths),
            Problem: DropImportProblem.None,
            Message: string.Empty,
            IsError: false);
    }
}

/// <summary>
/// Outcome of <see cref="DropImportPlan.Create"/>: either a plan to apply, or a
/// ready-to-show message describing why the drop was refused.
/// </summary>
public readonly record struct DropImportResult(
    DropImportPlan? Plan,
    DropImportProblem Problem,
    string Message,
    bool IsError)
{
    public bool Succeeded => Plan != null;
}
