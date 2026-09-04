using System.IO;

namespace OneColumnEncoder.ConcatManagement;

/// <summary>
/// Manage Concat ItemCard states and sync file lists (backend)
/// </summary>
public sealed class SrcConcatState
{
    // ItemCard View, path for each source, and filelist write path
    private readonly ToolItemCardVM? _SrcConcatCard;
    private string[] _filePaths = [];
    private static readonly string DefaultFileListPath =
        Path.Combine(SaveLoadBase<ConcatFileListPathPlaceholder>.GetConfigDirectory(),
            "source_concat_filelist.txt");

    // Constructor: bind state, set R1Text as Add or Replace
    public SrcConcatState(IEnumerable<ToolItemCardVM> videoSrcImportZone)
    {
        _SrcConcatCard = videoSrcImportZone.FirstOrDefault(item =>
            item.DefinitionKey == "SrcConcat");
        if (_SrcConcatCard != null)
            _SrcConcatCard.UseAutoAddReplaceText = false;
    }

    public bool IsActive => _SrcConcatCard != null && _SrcConcatCard.IsSelected;
    public string[] CurrentFilePaths => _filePaths;
    public static string FileListPath => DefaultFileListPath;

    /// <summary>
    /// Helping find source method in MainVM to distingulish mode
    /// </summary>
    /// <param name="item">ItemCard view model</param>
    /// <returns>true if requested item is a concat item</returns>
    public bool IsConcatItem(ToolItemCardVM item) =>
        item != null && ReferenceEquals(item, _SrcConcatCard);

    public void ApplyImportedFiles(string[] filePaths)
    {
        _filePaths = filePaths is null ? [] : [.. filePaths];
        SyncCardAndFileList();
    }

    public void ReplaceFilePaths(string[] filePaths)
    {
        _filePaths = filePaths is null ? [] : [.. filePaths];
        SyncCardAndFileList();
    }

    public string RegenerateFileList() =>
        _filePaths.Length == 0
            ? DeleteFileList()
            : ConcatFileListGenerator.GenerateFileList(_filePaths, FileListPath);

    public void Clear()
    {
        _filePaths = [];
        TryDeleteFileList();
        if (_SrcConcatCard == null) return;
        _SrcConcatCard.P1TextData = string.Empty;
        _SrcConcatCard.P1TooltipText = null;
        _SrcConcatCard.P2TextData = string.Empty;
        RefreshTitle();
    }

    private void SyncCardAndFileList()
    {
        if (_filePaths.Length == 0)
            TryDeleteFileList();
        else
            RegenerateFileList();

        if (_SrcConcatCard == null) return;
        RefreshCardSummary();
        RefreshTitle();
    }

    private void RefreshCardSummary()
    {
        if (_SrcConcatCard == null) return;

        string[] fileNames = [.. _filePaths
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];

        _SrcConcatCard.P1TextData = BrowseSrcQueueCmd.FormatQueueP1Text(fileNames);
        _SrcConcatCard.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(fileNames);
        _SrcConcatCard.P2TextData = _filePaths.Length > 0
            ? Path.GetDirectoryName(_filePaths[0]) ?? string.Empty
            : string.Empty;
    }

    private static void TryDeleteFileList()
    {
        try
        {
            if (File.Exists(FileListPath)) File.Delete(FileListPath);
        }
        catch {}
    }

    private static string DeleteFileList()
    {
        TryDeleteFileList();
        return FileListPath;
    }

    private void RefreshTitle()
    {
        if (_SrcConcatCard == null) return;
        if (_filePaths.Length > 0)
            _SrcConcatCard.Name = string.Format(
                UILangProvider.Current["SrcConcatWithCount"], _filePaths.Length);
        else
            _SrcConcatCard.Name = UILangProvider.Current["SrcConcat"];
    }

    public void RefreshLanguage()
    {
        if (_SrcConcatCard == null) return;
        _SrcConcatCard.UseAutoAddReplaceText = false;
        RefreshTitle();
    }

    private sealed class ConcatFileListPathPlaceholder : SaveLoadBase<ConcatFileListPathPlaceholder>
    {
        protected override string FilePath => string.Empty;
    }
}
