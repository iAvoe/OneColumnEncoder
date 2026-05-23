using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourcePathCmd(ToolItemVM item,
                                     SourceFileKind fileKind,
                                     AppDataM appDataM,
                                     ModalNavS modalNavS,
                                     Action<ToolItemVM, SourceFileKind, string>? afterImport = null) : BaseCmd
    {
        private readonly ToolItemVM _item = item;
        private readonly SourceFileKind _fileKind = fileKind;
        private readonly AppDataM _appDataM = appDataM;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Action<ToolItemVM, SourceFileKind, string>? _afterImport = afterImport;

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
                _modalNavS,
                foundPath: foundPath,
                currentPath: _item.Path);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            _item.Path = filePath;
            _item.VersionText = SourceFilePickerH.GetPrimaryText(_fileKind, filePath);
            _afterImport?.Invoke(_item, _fileKind, filePath);
        }
    }
}
