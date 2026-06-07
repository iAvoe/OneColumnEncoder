using Microsoft.Win32;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.IO;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class OneClickScriptGenCmd(Func<string> getSourcePath, ToolItemCardVM avsItem, ToolItemCardVM vpyItem, ModalNavS modalNavS) : BaseCmd
    {
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly ToolItemCardVM _avsItem = avsItem;
        private readonly ToolItemCardVM _vpyItem = vpyItem;
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
                ScriptScribeVM.AvsPrefix,
                ScriptScribeVM.AvsPrefix2,
                ScriptScribeVM.AvsSuffix); // No user input (extra filter lines) in one click gen
            string vpyScript = ScriptTemplateH.BuildVpyExportScript(
                sourcePath,
                ScriptScribeVM.VpyPrefix,
                ScriptScribeVM.VpyPrefix2,
                ScriptScribeVM.VpySuffix);

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"],
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

            _avsItem.P2TextData = avsPath;
            _avsItem.P1TextData = SourceFilePickerH.GetPrimaryText(SourceFileKind.AviSynthScript, avsPath);

            _vpyItem.P2TextData = vpyPath;
            _vpyItem.P1TextData = SourceFilePickerH.GetPrimaryText(SourceFileKind.VapourSynthScript, vpyPath);

            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Scripts saved:\n{avsPath}\n{vpyPath}").Execute(null);
        }
    }
}
