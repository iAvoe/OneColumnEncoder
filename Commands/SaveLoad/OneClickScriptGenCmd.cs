using Microsoft.Win32;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System;
using System.IO;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class OneClickScriptGenCmd(Func<string> getSourcePath, ToolItemVM avsItem, ToolItemVM vpyItem, ModalNavS modalNavS) : BaseCmd
    {
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly ToolItemVM _avsItem = avsItem;
        private readonly ToolItemVM _vpyItem = vpyItem;
        private readonly ModalNavS _modalNavS = modalNavS;

        public override bool CanExecute(object? parameter) =>
            !string.IsNullOrWhiteSpace(_getSourcePath());

        public override void Execute(object? parameter)
        {
            string sourcePath = _getSourcePath();
            if (!CanExecute(null))
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    UILangProviderM.Current["SrcScribe.WindowTitle"],
                    UILangProviderM.Current["SrcScribe.NoVidSrcWarning"]).Execute(null);
                return;
            }

            string avsScript = ScriptTemplateH.BuildAvsExportScript(
                sourcePath,
                ScriptSrcScribeModalVM.AvsPrefix,
                ScriptSrcScribeModalVM.AvsPrefix2,
                ScriptSrcScribeModalVM.AvsSuffix,
                ""); // No user input (extra filter lines) in one click gen
            string vpyScript = ScriptTemplateH.BuildVpyExportScript(
                sourcePath,
                ScriptSrcScribeModalVM.VpyPrefix,
                ScriptSrcScribeModalVM.VpyPrefix2,
                ScriptSrcScribeModalVM.VpySuffix,
                "");

            SaveFileDialog dialog = new()
            {
                Title = "Saving all scripts...",
                Filter = "AviSynth Script (*.avs)|*.avs", // Script files (*.avs, *.vpy)|*.avs;*.vpy
                FileName = "script.avs"
            };

            if (dialog.ShowDialog() != true) return;

            string avsPath = dialog.FileName;
            string directory = Path.GetDirectoryName(avsPath) ?? ".";
            string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");

            try
            {
                File.WriteAllText(avsPath, avsScript);
                File.WriteAllText(vpyPath, vpyScript);
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProviderM.Current["SrcScribe.WindowTitle"],
                    $"Failed to save scripts: {ex.Message}").Execute(null);
                return;
            }

            _avsItem.Path = avsPath;
            _avsItem.VersionText = SourceFilePickerH.GetPrimaryText(SourceFileKind.AviSynthScript, avsPath);

            _vpyItem.Path = vpyPath;
            _vpyItem.VersionText = SourceFilePickerH.GetPrimaryText(SourceFileKind.VapourSynthScript, vpyPath);

            new OpenInfoOrDbgModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Scripts saved:\n{avsPath}\n{vpyPath}").Execute(null);
        }
    }
}
