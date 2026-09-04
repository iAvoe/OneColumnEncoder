namespace OneColumnEncoder.QueueManagement;

public sealed class SrcQueueState
{
    private readonly ToolItemCardVM? _SrcQueueCard;
    private readonly Dictionary<ToolItemCardVM, string[]> _sourceQueueFilePaths = [];

    public SrcQueueState(IEnumerable<ToolItemCardVM> videoSrcImportZone)
    {
        _SrcQueueCard = videoSrcImportZone.FirstOrDefault(item =>
            item.DefinitionKey == "SrcQueue");
        if (_SrcQueueCard != null)
            _SrcQueueCard.UseAutoAddReplaceText = false;
    }

    public bool IsActive => SrcQueue.IsQueueRouteActive(_SrcQueueCard);

    public string[] CurrentFilePaths => SrcQueue.GetCurrentQueueFilePaths(
        _SrcQueueCard,
        _sourceQueueFilePaths);

    public bool IsQueueItem(ToolItemCardVM item) =>
        SrcQueue.IsSrcQueueItem(item, _SrcQueueCard);

    public void ApplyImportedFiles(ToolItemCardVM item, string[] filePaths)
    {
        _sourceQueueFilePaths[item] = filePaths;
        SrcQueue.RefreshSourceQueueTitle(item, filePaths.Length);
    }

    public void Clear(ToolItemCardVM item)
    {
        _sourceQueueFilePaths.Remove(item);
        SrcQueue.RefreshSourceQueueTitle(item, 0);
    }

    public void ApplyAcceptedFiles(string[] acceptedFilePaths)
    {
        if (_SrcQueueCard == null) return;

        _sourceQueueFilePaths[_SrcQueueCard] = acceptedFilePaths;
        _SrcQueueCard.P1TextData = SrcQueue.GetQueueP1Text(acceptedFilePaths);
        _SrcQueueCard.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(acceptedFilePaths); // Show full file list on hover
        SrcQueue.RefreshSourceQueueTitle(_SrcQueueCard, acceptedFilePaths.Length);
    }

    public void RefreshLanguage()
    {
        if (_SrcQueueCard == null) return;

        _SrcQueueCard.UseAutoAddReplaceText = false;
        if (!_sourceQueueFilePaths.TryGetValue(_SrcQueueCard, out string[]? filePaths)) return;

        SrcQueue.RefreshSourceQueueTitle(_SrcQueueCard, filePaths.Length);
        if (filePaths.Length > 0)
        {
            _SrcQueueCard.P1TextData = SrcQueue.GetQueueP1Text(filePaths);
            _SrcQueueCard.P1TooltipText = BrowseSrcQueueCmd.FormatQueueP1TooltipText(filePaths); // Show full file list on hover
        }
    }
}

public static class SrcQueue
{
    private static SrcQueueLangProvider Lang =>
        new(UILangProvider.Current.LanguageCode);

    public static bool IsQueueRouteActive(ToolItemCardVM? SrcQueueCard) =>
        SrcQueueCard != null && SrcQueueCard.IsSelected;

    public static string[] GetCurrentQueueFilePaths(
        ToolItemCardVM? SrcQueueCard,
        Dictionary<ToolItemCardVM, string[]> sourceQueueFilePaths)
    {
        return SrcQueueCard != null &&
               sourceQueueFilePaths.TryGetValue(SrcQueueCard, out string[]? filePaths)
            ? filePaths
            : [];
    }

    public static bool IsSrcQueueItem(
        ToolItemCardVM item,
        ToolItemCardVM? SrcQueueCard) =>
        item != null && ReferenceEquals(item, SrcQueueCard);

    public static void RefreshSourceQueueTitle(
        ToolItemCardVM item,
        int queueCount)
    {
        if (item == null) return;

        item.Name = queueCount > 0
            ? string.Format(UILangProvider.Current["SrcQueueWithCount"], queueCount)
            : UILangProvider.Current["SrcQueue"];
    }

    public static string GetQueueP1Text(string[] fileNames) =>
        fileNames == null
            ? string.Empty
            : BrowseSrcQueueCmd.FormatQueueP1Text(fileNames);
}
