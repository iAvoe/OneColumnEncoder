using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.Helpers
{
    public static class SourceFileKindH
    {
        public static SourceFileKind ResolveSourceFileKind(string displayName)
        {
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.VideoSource"], StringComparison.OrdinalIgnoreCase) ||
                displayName.Equals(UILangProviderM.Current["Tool.Source.VideoSrcQueue"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.Video;
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.AviSynth"], StringComparison.OrdinalIgnoreCase) ||
                displayName.Equals(UILangProviderM.Current["Tool.Source.AviSynthQueue"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.AviSynthScript;
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.VapourSynth"], StringComparison.OrdinalIgnoreCase) ||
                displayName.Equals(UILangProviderM.Current["Tool.Source.VapourSynthQueue"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.VapourSynthScript;
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.Svfi"], StringComparison.OrdinalIgnoreCase) ||
                displayName.Equals(UILangProviderM.Current["Tool.Source.SvfiQueue"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.SvfiIni;

            throw new ArgumentException($"Unknown source type: {displayName}");
        }

        public static SourceFileKind? GetPreferredScriptSourceKind(IEnumerable<ToolItemCardVM> upstreamsZone)
        {
            ToolItemCardVM? selectedUpstream = upstreamsZone.FirstOrDefault(t => t.IsSelected);
            string? upstreamExeName = selectedUpstream == null
                ? null
                : ToolCatalogProviderM.ResolveExeFromDisplayName(selectedUpstream.Name);

            return GetPreferredScriptSourceKind(upstreamExeName);
        }

        public static SourceFileKind? GetPreferredScriptSourceKind(string? upstreamExeName) => upstreamExeName switch
        {
            "vspipe.exe" => SourceFileKind.VapourSynthScript,
            "avs2yuv.exe" or "avs2pipemod.exe" => SourceFileKind.AviSynthScript,
            _ => null
        };

        public static bool IsQueueRouteSupportedUpstream(string? upstreamExeName) =>
            upstreamExeName?.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase) == true ||
            upstreamExeName?.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase) == true ||
            upstreamExeName?.Equals("avs2yuv.exe", StringComparison.OrdinalIgnoreCase) == true ||
            upstreamExeName?.Equals("avs2pipemod.exe", StringComparison.OrdinalIgnoreCase) == true;
    }
}
