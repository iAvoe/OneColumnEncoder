using Microsoft.Win32;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.ConcatManagement;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;
using System.Linq;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourceConcatCmd(
        ToolItemCardVM item,
        ModalNavS modalNavS,
        Action<ToolItemCardVM, string[]>? afterImport = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Action<ToolItemCardVM, string[]>? _afterImport = afterImport;

        public override void Execute(object? parameter)
        {
            OpenFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SourceConcat.SelectFilesTitle"],
                Multiselect = true,
                Filter = UILangProviderM.Current["Dialog.Filter.All"],
                InitialDirectory = OutputPath.GetInitialDirectory(_item.P2TextData)
            };

            Window? owner = Application.Current.MainWindow;
            bool? result = owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (result != true) return;

            string[] filePaths = dialog.FileNames;
            if (filePaths.Length == 0) return;

            string? extensionError = GetExtensionMismatchMessage(filePaths);
            if (extensionError != null)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UICaptionProviderM.SourceInspect.WarnTitle,
                    extensionError).Execute(null);
                Application.Current.MainWindow?.Activate();
                return;
            }

            string parentDir = Path.GetDirectoryName(filePaths[0]) ?? string.Empty;
            string[] fileNames = filePaths.Select(Path.GetFileName).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n!).ToArray();

            _item.P2TextData = parentDir;
            _item.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(fileNames);
            _item.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(fileNames);
            _afterImport?.Invoke(_item, filePaths);
            Application.Current.MainWindow?.Activate();
        }

        private static string? GetExtensionMismatchMessage(string[] filePaths)
        {
            if (filePaths.Length < 2) return null;

            string expectedExtension = Path.GetExtension(filePaths[0]) ?? string.Empty;
            string[] mismatched = [.. filePaths
                .Where(path => !string.Equals(
                    Path.GetExtension(path) ?? string.Empty,
                    expectedExtension,
                    StringComparison.OrdinalIgnoreCase))];
            if (mismatched.Length == 0) return null;

            string expectedLabel = FormatExtension(expectedExtension);
            string mismatchedList = string.Join(
                Environment.NewLine,
                mismatched.Select(path => $"- {Path.GetFileName(path)} ({FormatExtension(Path.GetExtension(path) ?? string.Empty)})"));
            return string.Format(UILangProviderM.Current["SourceConcat.ExtensionMismatch"], expectedLabel, mismatchedList);
        }

        private static string FormatExtension(string extension) =>
            string.IsNullOrWhiteSpace(extension) ? "(none)" : extension;
    }
}
