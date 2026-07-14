using OneColumnEncoder.Commands;
using OneColumnEncoder.Models;
using OneColumnEncoder.Persistence;
using OneColumnEncoder.ViewModels.Cards;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OneColumnEncoder.ConcatManagement
{
    public sealed class VideoSourceConcatState
    {
        private readonly ToolItemCardVM? _videoSourceConcatCard;
        private string[] _filePaths = [];
        private static readonly string DefaultFileListPath =
            Path.Combine(SaveLoadBase<ConcatFileListPathPlaceholder>.GetConfigDirectory(), "source_concat_filelist.txt");

        public VideoSourceConcatState(IEnumerable<ToolItemCardVM> videoSrcImportZone)
        {
            _videoSourceConcatCard = videoSrcImportZone.FirstOrDefault(item =>
                item.Name.Equals(UILangProvider.Current["Tool.Source.VideoSrcConcat"], StringComparison.OrdinalIgnoreCase));
            if (_videoSourceConcatCard != null)
                _videoSourceConcatCard.UseAutoAddReplaceText = false;
        }

        public bool IsActive => _videoSourceConcatCard != null && _videoSourceConcatCard.IsSelected;

        public string[] CurrentFilePaths => _filePaths;

        public string FileListPath => DefaultFileListPath;

        public bool IsConcatItem(ToolItemCardVM item) =>
            item != null && ReferenceEquals(item, _videoSourceConcatCard);

        public void ApplyImportedFiles(string[] filePaths)
        {
            _filePaths = filePaths ?? [];
            if (_videoSourceConcatCard == null) return;
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
            if (_videoSourceConcatCard == null) return;
            _videoSourceConcatCard.P1TextData = string.Empty;
            _videoSourceConcatCard.P1TooltipText = null;
            _videoSourceConcatCard.P2TextData = string.Empty;
            TryDeleteFileList();
            RefreshTitle();
        }

        private void RefreshCardSummary()
        {
            if (_videoSourceConcatCard == null) return;

            string[] fileNames = [.. _filePaths
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)];

            _videoSourceConcatCard.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(fileNames);
            _videoSourceConcatCard.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(fileNames);
            _videoSourceConcatCard.P2TextData = _filePaths.Length > 0
                ? Path.GetDirectoryName(_filePaths[0]) ?? string.Empty
                : string.Empty;
        }

        private void TryDeleteFileList()
        {
            try
            {
                if (File.Exists(FileListPath)) File.Delete(FileListPath);
            }
            catch { }
        }

        private void RefreshTitle()
        {
            if (_videoSourceConcatCard == null) return;
            if (_filePaths.Length > 0)
                _videoSourceConcatCard.Name = string.Format(
                    UILangProvider.Current["Tool.Source.VideoSrcConcatWithCount"], _filePaths.Length);
            else
                _videoSourceConcatCard.Name = UILangProvider.Current["Tool.Source.VideoSrcConcat"];
        }

        public void RefreshLanguage()
        {
            if (_videoSourceConcatCard == null) return;
            _videoSourceConcatCard.UseAutoAddReplaceText = false;
            RefreshTitle();
        }

        private sealed class ConcatFileListPathPlaceholder : SaveLoadBase<ConcatFileListPathPlaceholder>
        {
            protected override string FilePath => string.Empty;
        }
    }
}
