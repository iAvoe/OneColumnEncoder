using Microsoft.Win32;

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
                InitialDirectory = OutputPath.GetInitialDirectory(_item.P2TextData)
            };

            Window? owner = Application.Current.MainWindow;
            bool? result = owner is null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (result != true) return;

            _item.P2TextData = dialog.FolderName;
            Application.Current.MainWindow?.Activate();
        }
    }
}
