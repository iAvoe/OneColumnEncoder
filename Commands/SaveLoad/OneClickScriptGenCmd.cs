using OneColumnEncoder.ScriptGeneration;
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
        Func<string[]>? getQueueFilePaths = null,
        Func<bool>? isConcatRoute = null,
        Func<string[]>? getConcatFilePaths = null,
        Func<bool>? isRepartRoute = null,
        Func<string[]>? getRepartFilePaths = null) : BaseCmd
    {
        private readonly Func<string> _getSourcePath = getSourcePath;
        private readonly Func<ToolItemCardVM> _getAvsItem = getAvsItem;
        private readonly Func<ToolItemCardVM> _getVpyItem = getVpyItem;
        private readonly IEnumerable<ToolItemCardVM> _upstreamsZone = upstreamsZone; // For making auto selection
        private readonly ModalNavS _modalNavS = modalNavS;
        private readonly Func<bool>? _isQueueRoute = isQueueRoute;
        private readonly Func<string[]>? _getQueueFilePaths = getQueueFilePaths;
        private readonly Func<bool>? _isConcatRoute = isConcatRoute;
        private readonly Func<string[]>? _getConcatFilePaths = getConcatFilePaths;
        private readonly Func<bool>? _isRepartRoute = isRepartRoute;
        private readonly Func<string[]>? _getRepartFilePaths = getRepartFilePaths;

        public override bool CanExecute(object? parameter) =>
            IsQueueRoute()
                ? (_getQueueFilePaths?.Invoke().Length ?? 0) > 0
                : IsConcatRoute()
                    ? (_getConcatFilePaths?.Invoke().Length ?? 0) > 1
                : IsRepartRoute()
                    ? (_getRepartFilePaths?.Invoke().Length ?? 0) > 0
                : !string.IsNullOrWhiteSpace(_getSourcePath());

        public override void Execute(object? parameter)
        {
            if (!CanExecute(null))
            {
                new OpenWarnModalCmd(
                    _modalNavS,
                    FilterScribeModalLangProvider.WindowTitle,
                    FilterScribeModalLangProvider.Current["SrcScribe.NoVidSrcWarning"]).Execute(null);
                return;
            }

            if (IsQueueRoute())
            {
                ExecuteQueueScriptGen();
                return;
            }

            if (IsConcatRoute())
            {
                ExecuteConcatScriptGen();
                return;
            }

            if (IsRepartRoute())
            {
                ExecuteVirtualSourceScriptGen(_getRepartFilePaths?.Invoke() ?? [], "_repart");
                return;
            }

            string sourcePath = _getSourcePath();
            string avsScript = ScriptTemplate.BuildAvsExportScript(
                sourcePath,
                FilterScribeVM.AvsPrefix2,
                FilterScribeVM.AvsSuffix); // No user input (extra filter lines) in one click gen
            string vpyScript = ScriptTemplate.BuildVpyExportScript(
                sourcePath,
                FilterScribeVM.VpyPrefix2,
                FilterScribeVM.VpySuffix);

            SaveFileDialog dialog = new()
            {
                Title = FilterScribeModalLangProvider.Current["SrcScribe.SavingWindowTitle"],
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
                    FilterScribeModalLangProvider.WindowTitle,
                    string.Format(FilterScribeModalLangProvider.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
                return;
            }

            avsItem.P2TextData = avsPath;
            avsItem.P1TextData = SourceFilePicker.GetPrimaryText(SourceFileKind.AviSynthScript, avsPath);

            vpyItem.P2TextData = vpyPath;
            vpyItem.P1TextData = SourceFilePicker.GetPrimaryText(SourceFileKind.VapourSynthScript, vpyPath);

            ApplyUpstreamScriptSelection(avsItem, vpyItem);

            new OpenSuccModalCmd(
                _modalNavS,
                FilterScribeModalLangProvider.WindowTitle,
                string.Format(UILangProvider.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, [avsPath, vpyPath]))).Execute(null);
        }

        private bool IsQueueRoute() => _isQueueRoute?.Invoke() == true;
        private bool IsConcatRoute() => _isConcatRoute?.Invoke() == true;
        private bool IsRepartRoute() => _isRepartRoute?.Invoke() == true;

        private void ExecuteConcatScriptGen()
        {
            string[] sourcePaths = _getConcatFilePaths?.Invoke() ?? [];
            if (sourcePaths.Length < 2)
            {
                new OpenErrModalCmd(
                    _modalNavS,
                    FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSourcesTitle"],
                    FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSources"]).Execute(null);
                return;
            }

            ExecuteVirtualSourceScriptGen(sourcePaths, "_concat");
        }

        private void ExecuteVirtualSourceScriptGen(string[] sourcePaths, string suffix)
        {
            if (sourcePaths.Length == 0) return;

            string baseName = BrowseSourceQueueCmd.FormatConcatFileName(sourcePaths);
            string avsScript = sourcePaths.Length == 1
                ? ScriptTemplate.BuildAvsExportScript(sourcePaths[0], FilterScribeVM.AvsPrefix2, FilterScribeVM.AvsSuffix)
                : ScriptTemplate.BuildConcatAvsExportScript(sourcePaths, FilterScribeVM.AvsPrefix2, FilterScribeVM.AvsSuffix);
            string vpyScript = sourcePaths.Length == 1
                ? ScriptTemplate.BuildVpyExportScript(sourcePaths[0], FilterScribeVM.VpyPrefix2, FilterScribeVM.VpySuffix)
                : ScriptTemplate.BuildConcatVpyExportScript(sourcePaths, FilterScribeVM.VpyPrefix2, FilterScribeVM.VpySuffix);

            SaveFileDialog dialog = new()
            {
                Title = FilterScribeModalLangProvider.Current["SrcScribe.SavingWindowTitle"],
                Filter = "AviSynth Script (*.avs)|*.avs",
                FileName = baseName + suffix + ".avs"
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
                    FilterScribeModalLangProvider.WindowTitle,
                    string.Format(FilterScribeModalLangProvider.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
                return;
            }

            ToolItemCardVM avsItem = _getAvsItem();
            ToolItemCardVM vpyItem = _getVpyItem();
            avsItem.P2TextData = avsPath;
            avsItem.P1TextData = SourceFilePicker.GetPrimaryText(SourceFileKind.AviSynthScript, avsPath);
            vpyItem.P2TextData = vpyPath;
            vpyItem.P1TextData = SourceFilePicker.GetPrimaryText(SourceFileKind.VapourSynthScript, vpyPath);

            ApplyUpstreamScriptSelection(avsItem, vpyItem);

            new OpenSuccModalCmd(
                _modalNavS,
                FilterScribeModalLangProvider.WindowTitle,
                string.Format(UILangProvider.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, [avsPath, vpyPath]))).Execute(null);
        }

        private void ExecuteQueueScriptGen()
        {
            string[] sourcePaths = _getQueueFilePaths?.Invoke() ?? [];
            if (sourcePaths.Length == 0) return;

            OpenFolderDialog dialog = new()
            {
                Title = FilterScribeModalLangProvider.Current["SrcScribe.SavingWindowTitle"]
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

                    File.WriteAllText(avsPath, ScriptTemplate.BuildAvsExportScript(
                        sourcePath,
                        FilterScribeVM.AvsPrefix2,
                        FilterScribeVM.AvsSuffix));
                    File.WriteAllText(vpyPath, ScriptTemplate.BuildVpyExportScript(
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
                    FilterScribeModalLangProvider.WindowTitle,
                    string.Format(FilterScribeModalLangProvider.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
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
                FilterScribeModalLangProvider.WindowTitle,
                string.Format(UILangProvider.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, savedPaths))).Execute(null);
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
