using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OneColumnEncoder.Helpers
{
    public static class ToolCompatibilityH
    {
        public static void RefreshDependencySelectionState(
            IEnumerable<ToolItemVM> upstreamsZone,
            IEnumerable<ToolItemVM> dependenciesZone,
            Action updateEncodingStartButtons)
        {
            ToolItemVM? avs2pipemod = upstreamsZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            ToolItemVM? avisynth = dependenciesZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avisynth.dll"));

            bool avsSelected = avs2pipemod?.IsSelected ?? false;
            bool aviSelected = avisynth?.IsSelected ?? false;
            bool bothSelectedOrNeither = avsSelected == aviSelected;

            if (avs2pipemod != null)
                avs2pipemod.IsCancel = avsSelected && !bothSelectedOrNeither;

            if (avisynth != null)
                avisynth.IsCancel = aviSelected && !bothSelectedOrNeither;

            foreach (ToolItemVM upstream in upstreamsZone.Where(t => !ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe") && t.IsCancel))
            {
                upstream.IsCancel = false;
            }

            updateEncodingStartButtons();
        }

        public static void RefreshSourceSelectionState(
            IEnumerable<ToolItemVM> upstreamsZone,
            ObservableCollection<ToolItemVM> scriptSrcImportZone,
            Action refreshSelectedSourceStatus)
        {
            ToolItemVM? upstream = upstreamsZone.FirstOrDefault(t => t.IsSelected);

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

            foreach (ToolItemVM item in scriptSrcImportZone)
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
