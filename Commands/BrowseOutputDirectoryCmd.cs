using Microsoft.Win32;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using System.Windows;

namespace OneColumnEncoder.Commands
{
    public class BrowseOutputDirectoryCmd(ToolItemCardVM item) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;

        public override void Execute(object? parameter)
        {
            OpenFolderDialog dialog = new()
            {
                Title = FilenameScribeVM.WindowTitle,
                InitialDirectory = OutputPathH.GetInitialDirectory(_item.P2TextData)
            };

            Window? owner = Application.Current.MainWindow;
            bool? result = owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (result != true) return;

            _item.P2TextData = dialog.FolderName;
            _item.P1TextData = "N/A";
            _item.P1TooltipText = null;
            Application.Current.MainWindow?.Activate();
        }
    }
}
