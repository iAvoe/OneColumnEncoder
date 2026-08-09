using System.IO;

namespace OneColumnEncoder.ConcatManagement;

public sealed class VideoSrcConcatState
{
    private readonly ToolItemCardVM? _videoSrcConcatCard;
    private string[] _filePaths = [];
    private static readonly string DefaultFileListPath =
        Path.Combine(SaveLoadBase<ConcatFileListPathPlaceholder>.GetConfigDirectory(), "source_concat_filelist.txt");

    public VideoSrcConcatState(IEnumerable<ToolItemCardVM> videoSrcImportZone)
    {
        _videoSrcConcatCard = videoSrcImportZone.FirstOrDefault(item =>
            item.Name.Equals(UILangProvider.Current["Tool.Source.VideoSrcConcat"], StringComparison.OrdinalIgnoreCase));
        if (_videoSrcConcatCard != null)
            _videoSrcConcatCard.UseAutoAddReplaceText = false;
    }

    public bool IsActive => _videoSrcConcatCard != null && _videoSrcConcatCard.IsSelected;

    public string[] CurrentFilePaths => _filePaths;

    public static string FileListPath => DefaultFileListPath;

    public bool IsConcatItem(ToolItemCardVM item) =>
        item != null && ReferenceEquals(item, _videoSrcConcatCard);

    public void ApplyImportedFiles(string[] filePaths)
    {
        _filePaths = filePaths ?? [];
        if (_videoSrcConcatCard == null) return;
        RefreshCardSummary();
        RegenerateFileList();
        RefreshTitle();
    }

    public void ReplaceFilePaths(string[] filePaths)
    {
        _filePaths = filePaths ?? [];
        RefreshCardSummary();
        RegenerateFileList();
        RefreshTitle();
    }

    public string RegenerateFileList() =>
        ConcatFileListGenerator.GenerateFileList(_filePaths, FileListPath);

    public void Clear()
    {
        _filePaths = [];
        if (_videoSrcConcatCard == null) return;
        _videoSrcConcatCard.P1TextData = string.Empty;
        _videoSrcConcatCard.P1TooltipText = null;
        _videoSrcConcatCard.P2TextData = string.Empty;
        TryDeleteFileList();
        RefreshTitle();
    }

    private void RefreshCardSummary()
    {
        if (_videoSrcConcatCard == null) return;

        string[] fileNames = [.. _filePaths
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];

        _videoSrcConcatCard.P1TextData = BrowseSrcQueueCmd.FormatQueueP1Text(fileNames);
        _videoSrcConcatCard.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(fileNames);
        _videoSrcConcatCard.P2TextData = _filePaths.Length > 0
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

    private void RefreshTitle()
    {
        if (_videoSrcConcatCard == null) return;
        if (_filePaths.Length > 0)
            _videoSrcConcatCard.Name = string.Format(
                UILangProvider.Current["Tool.Source.VideoSrcConcatWithCount"], _filePaths.Length);
        else
            _videoSrcConcatCard.Name = UILangProvider.Current["Tool.Source.VideoSrcConcat"];
    }

    public void RefreshLanguage()
    {
        if (_videoSrcConcatCard == null) return;
        _videoSrcConcatCard.UseAutoAddReplaceText = false;
        RefreshTitle();
    }

    private sealed class ConcatFileListPathPlaceholder : SaveLoadBase<ConcatFileListPathPlaceholder>
    {
        protected override string FilePath => string.Empty;
    }
}
