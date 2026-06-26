using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Commands;
using System.Collections.Generic;

namespace OneColumnEncoder.Helpers
{
    public sealed class VideoSourceQueueState
    {
        private readonly ToolItemCardVM? _videoSourceQueueCard;
        private readonly Dictionary<ToolItemCardVM, string[]> _sourceQueueFilePaths = [];

        public VideoSourceQueueState(IEnumerable<ToolItemCardVM> videoSrcImportZone)
        {
            _videoSourceQueueCard = videoSrcImportZone.FirstOrDefault(item =>
                item.Name.Equals(UILangProviderM.Current["Tool.Source.VideoSrcQueue"], StringComparison.OrdinalIgnoreCase));
            if (_videoSourceQueueCard != null)
                _videoSourceQueueCard.UseAutoAddReplaceText = false;
        }

        public bool IsActive => VideoSourceQueueH.IsQueueRouteActive(_videoSourceQueueCard);

        public string[] CurrentFilePaths => VideoSourceQueueH.GetCurrentQueueFilePaths(
            _videoSourceQueueCard,
            _sourceQueueFilePaths);

        public bool IsQueueItem(ToolItemCardVM item) =>
            VideoSourceQueueH.IsVideoSourceQueueItem(item, _videoSourceQueueCard);

        public void ApplyImportedFiles(ToolItemCardVM item, string[] filePaths)
        {
            _sourceQueueFilePaths[item] = filePaths;
            VideoSourceQueueH.RefreshSourceQueueTitle(item, filePaths.Length);
        }

        public void Clear(ToolItemCardVM item)
        {
            _sourceQueueFilePaths.Remove(item);
            VideoSourceQueueH.RefreshSourceQueueTitle(item, 0);
        }

        public void ApplyAcceptedFiles(string[] acceptedFilePaths)
        {
            if (_videoSourceQueueCard == null) return;

            _sourceQueueFilePaths[_videoSourceQueueCard] = acceptedFilePaths;
            _videoSourceQueueCard.P1TextData = VideoSourceQueueH.GetQueueP1Text(acceptedFilePaths);
            _videoSourceQueueCard.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(acceptedFilePaths); // Show full file list on hover
            VideoSourceQueueH.RefreshSourceQueueTitle(_videoSourceQueueCard, acceptedFilePaths.Length);
        }

        public void RefreshLanguage()
        {
            if (_videoSourceQueueCard == null) return;

            _videoSourceQueueCard.UseAutoAddReplaceText = false;
            if (!_sourceQueueFilePaths.TryGetValue(_videoSourceQueueCard, out string[]? filePaths)) return;

            VideoSourceQueueH.RefreshSourceQueueTitle(_videoSourceQueueCard, filePaths.Length);
            if (filePaths.Length > 0)
            {
                _videoSourceQueueCard.P1TextData = VideoSourceQueueH.GetQueueP1Text(filePaths);
                _videoSourceQueueCard.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(filePaths); // Show full file list on hover
            }
        }
    }

    public static class VideoSourceQueueH
    {
        private static VideoSourceQueueLangProviderM Lang =>
            new(UILangProviderM.Current.LanguageCode);

        public static bool IsQueueRouteActive(ToolItemCardVM? videoSourceQueueCard) =>
            videoSourceQueueCard != null && videoSourceQueueCard.IsSelected;

        public static string[] GetCurrentQueueFilePaths(
            ToolItemCardVM? videoSourceQueueCard,
            Dictionary<ToolItemCardVM, string[]> sourceQueueFilePaths)
        {
            return videoSourceQueueCard != null &&
                   sourceQueueFilePaths.TryGetValue(videoSourceQueueCard, out string[]? filePaths)
                ? filePaths
                : [];
        }

        public static bool IsVideoSourceQueueItem(
            ToolItemCardVM item,
            ToolItemCardVM? videoSourceQueueCard) =>
            item != null && ReferenceEquals(item, videoSourceQueueCard);

        public static void RefreshSourceQueueTitle(
            ToolItemCardVM item,
            int queueCount)
        {
            if (item == null) return;

            item.Name = queueCount > 0
                ? string.Format(UILangProviderM.Current["Tool.Source.VideoSrcQueueWithCount"], queueCount)
                : UILangProviderM.Current["Tool.Source.VideoSrcQueue"];
        }

        public static string GetQueueP1Text(string[] fileNames) =>
            fileNames == null
                ? string.Empty
                : BrowseSourceQueueCmd.FormatQueueP1Text(fileNames);
    }
}
