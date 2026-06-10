using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System.Collections.ObjectModel;

namespace OneColumnEncoder.Helpers
{
    public static class ToolCompatibilityH
    {
        /// <summary>
        /// Refreshes the selection state of dependencies based on the selection state of upstream tools.
        /// Only mark IsCancel for auto-selected items, user manual selection won't be reverted
        /// </summary>
        /// <param name="upstreamsZone">The collection of upstream tool item cards.</param>
        /// <param name="dependenciesZone">The collection of dependency tool item cards.</param>
        /// <param name="updateEncodingStartButtons">Action to update the encoding start buttons.</param>
        public static void RefreshDependencySelectionState(
            IEnumerable<ToolItemCardVM> upstreamsZone,
            IEnumerable<ToolItemCardVM> dependenciesZone,
            Action updateEncodingStartButtons)
        {
            ToolItemCardVM? avs2pipemod = upstreamsZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            ToolItemCardVM? avisynth = dependenciesZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avisynth.dll"));

            bool avsSelected = avs2pipemod?.IsSelected ?? false;
            bool aviSelected = avisynth?.IsSelected ?? false;
            bool bothSelectedOrNeither = avsSelected == aviSelected;

            if (avs2pipemod != null)
                avs2pipemod.IsCancel = avsSelected && !bothSelectedOrNeither;

            if (avisynth != null)
                avisynth.IsCancel = aviSelected && !bothSelectedOrNeither;

            foreach (ToolItemCardVM upstream in upstreamsZone.Where(t => !ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe") && t.IsCancel))
            {
                upstream.IsCancel = false;
            }

            updateEncodingStartButtons();
        }

        public static void RefreshSourceSelectionState(
            IEnumerable<ToolItemCardVM> upstreamsZone,
            ObservableCollection<ToolItemCardVM> scriptSrcImportZone,
            Action refreshSelectedSourceStatus)
        {
            ToolItemCardVM? upstream = upstreamsZone.FirstOrDefault(t => t.IsSelected);

            string? allowedName = null;
            bool allDisabled = false;

            switch (upstream)
            {
                case null:
                    break;
                case var u when ToolDefinitionProviderM.IsImportedTool(u.Name, "ffmpeg.exe"):
                    allDisabled = true;
                    break;
                case var u when ToolDefinitionProviderM.IsImportedTool(u.Name, "vspipe.exe"):
                    allowedName = UILangProviderM.Current["Tool.Source.VapourSynth"];
                    break;
                case var u when ToolDefinitionProviderM.IsImportedTool(u.Name, "avs2yuv.exe")
                           || ToolDefinitionProviderM.IsImportedTool(u.Name, "avs2pipemod.exe"):
                    allowedName = UILangProviderM.Current["Tool.Source.AviSynth"];
                    break;
                case var u when ToolDefinitionProviderM.IsImportedTool(u.Name, "one_line_shot_args.exe"):
                    allowedName = UILangProviderM.Current["Tool.Source.Svfi"];
                    break;
            }

            foreach (ToolItemCardVM item in scriptSrcImportZone)
            {
                bool shouldEnable = allDisabled switch
                {
                    true => false,
                    _ when allowedName == null => true,
                    _ => item.Name.Equals(allowedName, StringComparison.OrdinalIgnoreCase)
                };

                if (!shouldEnable) item.IsSelected = false;
                item.IsEnabled = shouldEnable;
            }

            refreshSelectedSourceStatus();
        }
    }
}
