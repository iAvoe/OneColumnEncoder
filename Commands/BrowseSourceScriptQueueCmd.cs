using Microsoft.Win32;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourceScriptQueueCmd(
        ToolItemCardVM item,
        SourceFileKind kind,
        Action<ToolItemCardVM, SourceFileKind, string, string[]>? afterImport = null,
        Func<string>? getInitialPath = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly SourceFileKind _kind = kind;
        private readonly Action<ToolItemCardVM, SourceFileKind, string, string[]>? _afterImport = afterImport;
        private readonly Func<string>? _getInitialPath = getInitialPath;

        public override void Execute(object? parameter)
        {
            OpenFolderDialog dialog = new()
            {
                Title = UILangProvider.Current["SourceQueue.SelectFolderTitle"],
                InitialDirectory = OutputPath.GetInitialDirectory(_getInitialPath?.Invoke() ?? _item.P2TextData)
            };

            Window? owner = Application.Current.MainWindow;
            bool? result = owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (result != true) return;

            string folderPath = dialog.FolderName;
            string[] filePaths = SourceFilePicker.GetSourceFilesInFolder(folderPath, _kind);
            // Extract file names for both short card display and long tooltip display
            string[] fileNames = filePaths.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!).ToArray();

            _item.P2TextData = folderPath;
            _item.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(fileNames);
            _item.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(fileNames);
            _afterImport?.Invoke(_item, _kind, folderPath, filePaths);
            Application.Current.MainWindow?.Activate();
        }
    }
}
