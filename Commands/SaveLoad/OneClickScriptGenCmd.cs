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
        ModalNavS modalNavS,
        Func<bool>? isQueueRoute = null,
        Func<string[]>? getQueueFilePaths = null) : BaseCmd
    {
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly Func<ToolItemCardVM> _getAvsItem = getAvsItem;
        private readonly Func<ToolItemCardVM> _getVpyItem = getVpyItem;
        private readonly IEnumerable<ToolItemCardVM> _upstreamsZone = upstreamsZone; // For making auto selection
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Func<bool>? _isQueueRoute = isQueueRoute;
        private readonly Func<string[]>? _getQueueFilePaths = getQueueFilePaths;

        public override bool CanExecute(object? parameter) =>
            IsQueueRoute()
                ? (_getQueueFilePaths?.Invoke().Length ?? 0) > 0
                : !string.IsNullOrWhiteSpace(_getSourcePath());

        public override void Execute(object? parameter)
        {
            if (!CanExecute(null))
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    UILangProviderM.SrcScribeWindowTitle,
                    UILangProviderM.Current["SrcScribe.NoVidSrcWarning"]).Execute(null);
                return;
            }

            if (IsQueueRoute())
            {
                ExecuteQueueScriptGen();
                return;
            }

            string sourcePath = _getSourcePath();
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
                    string.Format(UILangProviderM.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
                return;
            }

            avsItem.P2TextData = avsPath;
            avsItem.P1TextData = SourceFilePickerH.GetPrimaryText(SourceFileKind.AviSynthScript, avsPath);

            vpyItem.P2TextData = vpyPath;
            vpyItem.P1TextData = SourceFilePickerH.GetPrimaryText(SourceFileKind.VapourSynthScript, vpyPath);

            ApplyUpstreamScriptSelection(avsItem, vpyItem);

            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.SrcScribeWindowTitle,
                string.Format(UILangProviderM.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, [avsPath, vpyPath]))).Execute(null);
        }

        private bool IsQueueRoute() => _isQueueRoute?.Invoke() == true;

        private void ExecuteQueueScriptGen()
        {
            string[] sourcePaths = _getQueueFilePaths?.Invoke() ?? [];
            if (sourcePaths.Length == 0) return;

            OpenFolderDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"]
            };

            if (dialog.ShowDialog() != true) return;

            string directory = dialog.FolderName;
            List<string> savedPaths = [];

            try
            {
                foreach (string sourcePath in sourcePaths)
                {
                    string baseName = Path.GetFileNameWithoutExtension(sourcePath);
                    string avsPath = Path.Combine(directory, baseName + ".avs");
                    string vpyPath = Path.Combine(directory, baseName + ".vpy");

                    File.WriteAllText(avsPath, ScriptTemplateH.BuildAvsExportScript(
                        sourcePath,
                        FilterScribeVM.AvsPrefix2,
                        FilterScribeVM.AvsSuffix));
                    File.WriteAllText(vpyPath, ScriptTemplateH.BuildVpyExportScript(
                        sourcePath,
                        FilterScribeVM.VpyPrefix2,
                        FilterScribeVM.VpySuffix));
                    savedPaths.Add(avsPath);
                    savedPaths.Add(vpyPath);
                }
            }
            catch (Exception ex)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    UILangProviderM.SrcScribeWindowTitle,
                    string.Format(UILangProviderM.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
                return;
            }

            ToolItemCardVM avsItem = _getAvsItem();
            ToolItemCardVM vpyItem = _getVpyItem();
            // Extract saved script file names for card display and hover tooltip
            string[] avsFileNames = savedPaths.Where(path => path.EndsWith(".avs", StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!).ToArray();
            string[] vpyFileNames = savedPaths.Where(path => path.EndsWith(".vpy", StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!).ToArray();
            avsItem.P2TextData = directory;
            avsItem.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(avsFileNames);
            avsItem.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(avsFileNames);
            vpyItem.P2TextData = directory;
            vpyItem.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(vpyFileNames);
            vpyItem.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(vpyFileNames);

            ApplyUpstreamScriptSelection(avsItem, vpyItem);

            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.SrcScribeWindowTitle,
                string.Format(UILangProviderM.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, savedPaths))).Execute(null);
        }

        private void ApplyUpstreamScriptSelection(ToolItemCardVM avsItem, ToolItemCardVM vpyItem)
        {
            ToolItemCardVM? selectedUpstream = _upstreamsZone.FirstOrDefault(t => t.IsSelected);
            if (selectedUpstream == null) return;

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
    }
}
