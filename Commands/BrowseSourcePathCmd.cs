using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.IO;

namespace OneColumnEncoder.Commands
{
    public class BrowseSourcePathCmd(ToolItemCardVM item,
                                     SourceFileKind fileKind,
                                     AppDataM appDataM,
                                     ModalNavS modalNavS,
                                     Action<ToolItemCardVM, SourceFileKind, string, bool>? afterImport = null,
                                     Func<string>? getFoundPath = null) : BaseCmd
    {
        private readonly ToolItemCardVM _item = item;
        private readonly SourceFileKind _fileKind = fileKind;
        private readonly AppDataM _appDataM = appDataM;
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Action<ToolItemCardVM, SourceFileKind, string, bool>? _afterImport = afterImport;
        private readonly Func<string>? _getFoundPath = getFoundPath;

        public override void Execute(object? parameter)
        {
            bool wasReplaced = !string.IsNullOrWhiteSpace(_item.P2TextData);
            string dialogTitle =
                string.Format(UILangProviderM.Current["Dialog.SelectTitle"], _item.Name);

            string? foundPath = _fileKind == SourceFileKind.SvfiIni && !string.IsNullOrWhiteSpace(_appDataM.Tools.OneLineShotArgsPath)
                ? Path.Combine(Path.GetDirectoryName(_appDataM.Tools.OneLineShotArgsPath) ?? "", "Configs")
                : _getFoundPath?.Invoke();

            if (_fileKind == SourceFileKind.Video)
                foundPath = null;

            string? filePath = SourceFilePickerH.GetSource(
                _fileKind,
                dialogTitle,
                _modalNavS,
                foundPath: foundPath,
                currentPath: _fileKind == SourceFileKind.Video ? _item.P2TextData : null);

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            _item.P2TextData = filePath;
            _item.P1TextData = SourceFilePickerH.GetPrimaryText(_fileKind, filePath);
            _afterImport?.Invoke(_item, _fileKind, filePath, wasReplaced);
        }
    }
}
