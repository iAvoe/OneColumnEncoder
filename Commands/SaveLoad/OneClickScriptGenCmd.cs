using Microsoft.Win32;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;

namespace OneColumnEncoder.Commands.SaveLoad
{
    public class OneClickScriptGenCmd(
        Func<string> getSourcePath,
        Func<ToolItemCardVM> getAvsItem,
        Func<ToolItemCardVM> getVpyItem,
        IEnumerable<ToolItemCardVM> upstreamsZone,
        ModalNavS modalNavS) : BaseCmd
    {
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly Func<ToolItemCardVM> _getAvsItem = getAvsItem;
        private readonly Func<ToolItemCardVM> _getVpyItem = getVpyItem;
        private readonly IEnumerable<ToolItemCardVM> _upstreamsZone = upstreamsZone; // For making auto selection
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
                    UILangProviderM.SrcScribeWindowTitle,
                    UILangProviderM.Current["SrcScribe.NoVidSrcWarning"]).Execute(null);
                return;
            }
            string avsScript = ScriptTemplateH.BuildAvsExportScript(
                sourcePath,
                FilterScribeVM.AvsPrefix2,
                FilterScribeVM.AvsSuffix); // No user input (extra filter lines) in one click gen
            string vpyScript = ScriptTemplateH.BuildVpyExportScript(
                sourcePath,
                FilterScribeVM.VpyPrefix2,
                FilterScribeVM.VpySuffix);

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"],
                Filter = "AviSynth Script (*.avs)|*.avs", // Script files (*.avs, *.vpy)|*.avs;*.vpy
                FileName = Path.GetFileNameWithoutExtension(sourcePath) + ".avs"
            };

            if (dialog.ShowDialog() != true) return;

            string avsPath = dialog.FileName;
            string directory = Path.GetDirectoryName(avsPath) ?? ".";
            string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");
            ToolItemCardVM avsItem = _getAvsItem();
            ToolItemCardVM vpyItem = _getVpyItem();

            try
            {
                File.WriteAllText(avsPath, avsScript);
                File.WriteAllText(vpyPath, vpyScript);
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProviderM.SrcScribeWindowTitle,
                    $"Failed to save scripts: {ex.Message}").Execute(null);
                return;
            }

            avsItem.P2TextData = avsPath;
            avsItem.P1TextData = SourceFilePickerH.GetPrimaryText(SourceFileKind.AviSynthScript, avsPath);

            vpyItem.P2TextData = vpyPath;
            vpyItem.P1TextData = SourceFilePickerH.GetPrimaryText(SourceFileKind.VapourSynthScript, vpyPath);

            // Auto ScriptSrcImportZone selection: try select script import when upstream program relates
            ToolItemCardVM? selectedUpstream = _upstreamsZone.FirstOrDefault(t => t.IsSelected);
            if (selectedUpstream != null)
            {
                if (ToolDefinitionProviderM.IsImportedTool(selectedUpstream.Name, "vspipe.exe"))
                {
                    avsItem.IsSelected = false;
                    if (vpyItem.IsEnabled) vpyItem.IsSelected = true;
                }
                else if (ToolDefinitionProviderM.IsImportedTool(selectedUpstream.Name, "avs2yuv.exe") ||
                         ToolDefinitionProviderM.IsImportedTool(selectedUpstream.Name, "avs2pipemod.exe"))
                {
                    vpyItem.IsSelected = false;
                    if (avsItem.IsEnabled) avsItem.IsSelected = true;
                }
            }

            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.SrcScribeWindowTitle,
                $"Scripts saved:\n{avsPath}\n{vpyPath}").Execute(null);
        }
    }
}
