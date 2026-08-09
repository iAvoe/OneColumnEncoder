namespace OneColumnEncoder.QueueManagement;

public sealed class VideoSrcQueueState
{
    private readonly ToolItemCardVM? _videoSrcQueueCard;
    private readonly Dictionary<ToolItemCardVM, string[]> _sourceQueueFilePaths = [];

    public VideoSrcQueueState(IEnumerable<ToolItemCardVM> videoSrcImportZone)
    {
        _videoSrcQueueCard = videoSrcImportZone.FirstOrDefault(item =>
            item.Name.Equals(UILangProvider.Current["Tool.Source.VideoSrcQueue"], StringComparison.OrdinalIgnoreCase));
        if (_videoSrcQueueCard != null)
            _videoSrcQueueCard.UseAutoAddReplaceText = false;
    }

    public bool IsActive => VideoSrcQueue.IsQueueRouteActive(_videoSrcQueueCard);

    public string[] CurrentFilePaths => VideoSrcQueue.GetCurrentQueueFilePaths(
        _videoSrcQueueCard,
        _sourceQueueFilePaths);

    public bool IsQueueItem(ToolItemCardVM item) =>
        VideoSrcQueue.IsVideoSrcQueueItem(item, _videoSrcQueueCard);

    public void ApplyImportedFiles(ToolItemCardVM item, string[] filePaths)
    {
        _sourceQueueFilePaths[item] = filePaths;
        VideoSrcQueue.RefreshSourceQueueTitle(item, filePaths.Length);
    }

    public void Clear(ToolItemCardVM item)
    {
        _sourceQueueFilePaths.Remove(item);
        VideoSrcQueue.RefreshSourceQueueTitle(item, 0);
    }

    public void ApplyAcceptedFiles(string[] acceptedFilePaths)
    {
        if (_videoSrcQueueCard == null) return;

        _sourceQueueFilePaths[_videoSrcQueueCard] = acceptedFilePaths;
        _videoSrcQueueCard.P1TextData = VideoSrcQueue.GetQueueP1Text(acceptedFilePaths);
        _videoSrcQueueCard.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(acceptedFilePaths); // Show full file list on hover
        VideoSrcQueue.RefreshSourceQueueTitle(_videoSrcQueueCard, acceptedFilePaths.Length);
    }

    public void RefreshLanguage()
    {
        if (_videoSrcQueueCard == null) return;

        _videoSrcQueueCard.UseAutoAddReplaceText = false;
        if (!_sourceQueueFilePaths.TryGetValue(_videoSrcQueueCard, out string[]? filePaths)) return;

        VideoSrcQueue.RefreshSourceQueueTitle(_videoSrcQueueCard, filePaths.Length);
        if (filePaths.Length > 0)
        {
            _videoSrcQueueCard.P1TextData = VideoSrcQueue.GetQueueP1Text(filePaths);
            _videoSrcQueueCard.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(filePaths); // Show full file list on hover
        }
    }
}

public static class VideoSrcQueue
{
    private static VideoSrcQueueLangProvider Lang =>
        new(UILangProvider.Current.LanguageCode);

    public static bool IsQueueRouteActive(ToolItemCardVM? videoSrcQueueCard) =>
        videoSrcQueueCard != null && videoSrcQueueCard.IsSelected;

    public static string[] GetCurrentQueueFilePaths(
        ToolItemCardVM? videoSrcQueueCard,
        Dictionary<ToolItemCardVM, string[]> sourceQueueFilePaths)
    {
        return videoSrcQueueCard != null &&
               sourceQueueFilePaths.TryGetValue(videoSrcQueueCard, out string[]? filePaths)
            ? filePaths
            : [];
    }

    public static bool IsVideoSrcQueueItem(
        ToolItemCardVM item,
        ToolItemCardVM? videoSrcQueueCard) =>
        item != null && ReferenceEquals(item, videoSrcQueueCard);

    public static void RefreshSourceQueueTitle(
        ToolItemCardVM item,
        int queueCount)
    {
        if (item == null) return;

        item.Name = queueCount > 0
            ? string.Format(UILangProvider.Current["Tool.Source.VideoSrcQueueWithCount"], queueCount)
            : UILangProvider.Current["Tool.Source.VideoSrcQueue"];
    }

    public static string GetQueueP1Text(string[] fileNames) =>
        fileNames == null
            ? string.Empty
            : BrowseSrcQueueCmd.FormatQueueP1Text(fileNames);
}
