using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;

namespace OneColumnEncoder.Helpers
{
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
            ReferenceEquals(item, videoSourceQueueCard);

        public static void RefreshSourceQueueTitle(
            ToolItemCardVM item,
            int queueCount)
        {
            item.Name = queueCount > 0
                ? string.Format(Lang.Tool.Source.VideoSrcQueueWithCount, queueCount)
                : Lang.Tool.Source.VideoSrcQueue;
        }

        public static void ApplyQueueScriptSourceCardStyle(
            ObservableCollection<ToolItemCardVM> queueScriptSrcImportZone)
        {
            foreach (ToolItemCardVM item in queueScriptSrcImportZone)
            {
                item.UseAutoAddReplaceText = false;
                item.R1Text = Lang.Buttons.Import;
                item.P1Name = Lang.SourceQueue.Sequence;
                item.P2Name = Lang.ToolField.Path;
            }
        }

        public static string GetQueueP1Text(string[] fileNames) =>
            BrowseSourceQueueCmd.FormatQueueP1Text(fileNames);
    }
}