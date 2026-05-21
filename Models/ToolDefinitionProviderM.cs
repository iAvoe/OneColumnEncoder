using System;
using System.Collections.Generic;
using System.Linq;

namespace OneColumnEncoder.Models
{
    public static class ToolDefinitionProviderM
    {
        public static Dictionary<string, ToolDefinitionM> ToolDefs => BuildToolDefinitions();

        private static Dictionary<string, ToolDefinitionM> BuildToolDefinitions()
        {
            string replace = UILangProviderM.Current["Buttons.Replace"];
            string deleteText = UILangProviderM.Current["Buttons.Delete"];
            string version = UILangProviderM.Current["ToolField.Version"];
            string path = UILangProviderM.Current["ToolField.Path"];

            return new()
            {
                // Upstream
                ["Ffmpeg"] = new("FFMPEG", replace, deleteText, version, path, ToolZone.Upstream, "ffmpeg.exe"),
                ["Vspipe"] = new("VSPipe", replace, deleteText, version, path, ToolZone.Upstream, "vspipe.exe"),
                ["Avs2yuv"] = new("Avs2yuv", replace, deleteText, version, path, ToolZone.Upstream, "avs2yuv.exe"),
                ["Avs2pipemod"] = new("Avs2pipemod", replace, deleteText, version, path, ToolZone.Upstream, "avs2pipemod.exe"),
                ["OneLineShotArgs"] = new("OneLineShotArgs", replace, deleteText, version, path, ToolZone.Upstream, "one_line_shot_args.exe"),
                // Encoder
                ["X264"] = new("x264", replace, deleteText, version, path, ToolZone.Encoder, "x264.exe"),
                ["X265"] = new("x265", replace, deleteText, version, path, ToolZone.Encoder, "x265.exe"),
                ["SvtAv1"] = new("SVT-AV1", replace, deleteText, version, path, ToolZone.Encoder, "svtav1encapp.exe"),
                // Analytics
                ["Ffprobe"] = new("FFProbe", replace, deleteText, version, path, ToolZone.Analytics, "ffprobe.exe"),
                ["AviSynthDll"] = new("AviSynth DLL (for VapourSynth)", replace, deleteText, version, path, ToolZone.Analytics, "avisynth.dll"),
            };
        }

        public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetUpstreamDefinitions() =>
            ToolDefs.Where(kvp => kvp.Value.Zone == ToolZone.Upstream);

        public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetEncoderDefinitions() =>
            ToolDefs.Where(kvp => kvp.Value.Zone == ToolZone.Encoder);

        public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetAnalyticsDefinitions() =>
            ToolDefs.Where(kvp => kvp.Value.Zone == ToolZone.Analytics);

        public static ToolDefinitionM? GetByExeName(string exeName) =>
            ToolDefs.Values.FirstOrDefault(d => d.ExeName?.Equals(exeName, StringComparison.OrdinalIgnoreCase) == true);

        public static ToolDefinitionM? GetByDisplayName(string displayName) =>
            ToolDefs.Values.FirstOrDefault(d => d.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
    }
}
