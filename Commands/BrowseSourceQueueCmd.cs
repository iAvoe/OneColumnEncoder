using Microsoft.Win32;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourceQueueCmd(
        ToolItemCardVM item,
        Action<ToolItemCardVM, string, string[]>? afterImport = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly Action<ToolItemCardVM, string, string[]>? _afterImport = afterImport;

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
            string[] filePaths = SourceFilePickerH.GetVideoFilesInFolder(folderPath);

            _item.P2TextData = folderPath;
            _item.P1TextData = FormatQueueP1Text(
                filePaths.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!));
            _afterImport?.Invoke(_item, folderPath, filePaths);
            Application.Current.MainWindow?.Activate();
        }

        public static string FormatQueueP1Text(IEnumerable<string> fileNames)
        {
            string[] names = [.. fileNames];
            if (names.Length == 0) return string.Empty;

            static string Prefix(string fileName)
            {
                const int maxLength = 12;
                string name = Path.GetFileNameWithoutExtension(fileName) ?? fileName;
                return name.Length <= maxLength ? name : name[..maxLength];
            }

            if (names.Length == 1) return Prefix(names[0]);

            return $"{Prefix(names[0])}..{Prefix(names[^1])}";
        }
    }
}
