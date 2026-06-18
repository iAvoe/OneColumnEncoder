using Microsoft.Win32;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourceScriptQueueCmd(
        ToolItemCardVM item,
        SourceFileKind kind,
        Action<ToolItemCardVM, SourceFileKind, string, string[]>? afterImport = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly SourceFileKind _kind = kind;
        private readonly Action<ToolItemCardVM, SourceFileKind, string, string[]>? _afterImport = afterImport;

        public override void Execute(object? parameter)
        {
            OpenFolderDialog dialog = new()
            {
                Title = UILangProviderM.Current["SourceQueue.SelectFolderTitle"],
                InitialDirectory = OutputPathH.GetInitialDirectory(_item.P2TextData)
            };

            Window? owner = Application.Current.MainWindow;
            bool? result = owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (result != true) return;

            string folderPath = dialog.FolderName;
            string[] filePaths = SourceFilePickerH.GetSourceFilesInFolder(folderPath, _kind);

            _item.P2TextData = folderPath;
            _item.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(
                filePaths.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!));
            _afterImport?.Invoke(_item, _kind, folderPath, filePaths);
            Application.Current.MainWindow?.Activate();
        }
    }
}
