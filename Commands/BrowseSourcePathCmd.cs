using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using System;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourcePathCmd(ToolItemVM item, SourceFileKind fileKind, AppDataM appDataM) : BaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly SourceFileKind _fileKind = fileKind;
        private readonly AppDataM _appDataM = appDataM;

        public override void Execute(object? parameter)
        {
            string dialogTitle =
                string.Format(UILangProviderM.Current["Dialog.SelectTitle"], _item.Name);

            string? foundPath = _fileKind == SourceFileKind.SvfiIni
                ? _appDataM.Tools.OneLineShotArgsPath
                : null;

            string? filePath = SourceFilePickerH.GetSource(
                _fileKind,
                dialogTitle,
                foundPath: foundPath,
                currentPath: _item.Path);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            _item.Path = filePath;
            _item.VersionText = SourceFilePickerH.GetPrimaryText(_fileKind, filePath);
        }
    }
}
