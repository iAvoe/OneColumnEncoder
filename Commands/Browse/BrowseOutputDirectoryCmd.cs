namespace OneColumnEncoder.Commands.Browse;

public class BrowseOutputDirectoryCmd(ToolItemCardVM item) : BrowseCmdBase(item)
{
    public override void Execute(object? parameter)
    {
        OpenFolderDialog dialog = new()
        {
            Title = FilenameScribeVM.WindowTitle,
            InitialDirectory = FileManagementProviderM.GetInitialDirectory(_item.P2TextData)
        };

        if (ShowDialog(dialog) != true) return;

        _item.P2TextData = dialog.FolderName;
        ActivateMainWindow();
    }
}
