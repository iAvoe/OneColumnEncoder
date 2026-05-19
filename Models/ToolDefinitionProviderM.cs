using System;
using System.Collections.Generic;
using System.Linq;

namespace OneColumnEncoder.Models
{
    public static class ToolDefinitionProviderM
    {
        public static readonly Dictionary<string, ToolDefinitionM> ToolDefinitions = new()
        {
            // Upstream
            ["Ffmpeg"] = new("FFmpeg", "Replace", "Delete", "Version", "Path", ToolZone.Upstream, "ffmpeg.exe"),
            ["Vspipe"] = new("VSPipe", "Replace", "Delete", "Version", "Path", ToolZone.Upstream, "vspipe.exe"),
            ["Avs2yuv"] = new("Avs2yuv", "Replace", "Delete", "Version", "Path", ToolZone.Upstream, "avs2yuv.exe"),
            ["Avs2pipemod"] = new("Avs2pipemod", "Replace", "Delete", "Version", "Path", ToolZone.Upstream, "avs2pipemod.exe"),
            ["OneLineShotArgs"] = new("OneLineShotArgs", "Replace", "Delete", "Version", "Path", ToolZone.Upstream, "one_line_shot_args.exe"),
            // Encoder
            ["X264"] = new("x264", "Replace", "Delete", "Version", "Path", ToolZone.Encoder, "x264.exe"),
            ["X265"] = new("x265", "Replace", "Delete", "Version", "Path", ToolZone.Encoder, "x265.exe"),
            ["SvtAv1"] = new("SVT-AV1", "Replace", "Delete", "Version", "Path", ToolZone.Encoder, "svtav1encapp.exe"),
            // Analytics
            ["Ffprobe"] = new("FFprobe", "Replace", "Delete", "Version", "Path", ToolZone.Analytics, "ffprobe.exe"),
            ["AviSynthDll"] = new("AviSynth DLL (for VapourSynth)", "Replace", "Delete", "Version", "Path", ToolZone.Analytics, "avisynth.dll"),
        };

        public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetUpstreamDefinitions() =>
            ToolDefinitions.Where(kvp => kvp.Value.Zone == ToolZone.Upstream);

        public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetEncoderDefinitions() =>
            ToolDefinitions.Where(kvp => kvp.Value.Zone == ToolZone.Encoder);

        public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetAnalyticsDefinitions() =>
            ToolDefinitions.Where(kvp => kvp.Value.Zone == ToolZone.Analytics);

        public static ToolDefinitionM? GetByExeName(string exeName) =>
            ToolDefinitions.Values.FirstOrDefault(d => d.ExeName?.Equals(exeName, StringComparison.OrdinalIgnoreCase) == true);

        public static ToolDefinitionM? GetByDisplayName(string displayName) =>
            ToolDefinitions.Values.FirstOrDefault(d => d.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));
    }
}
