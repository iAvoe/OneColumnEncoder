using Microsoft.Win32;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;

namespace OneColumnEncoder.Commands
{
    public class BrowseToolPathCmd(ToolItemVM item) : BaseCmd
    {
        private readonly ToolItemVM _item = item;

        public override void Execute(object? parameter)
        {
            OpenFileDialog dialog = new()
            {
                Filter = UILangProviderM.Current["Dialog.Filter.All"],
                Title = string.Format(UILangProviderM.Current["Dialog.SelectTitle"], _item.Name),
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog() == true)
                _item.Path = dialog.FileName;
        }
    }
}
