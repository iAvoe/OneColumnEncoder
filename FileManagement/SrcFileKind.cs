namespace OneColumnEncoder.FileManagement;

public static class SrcFileKindResolver
{
    public static SrcFileKind ResolveSrcFileKind(string displayName)
    {
        if (displayName.Equals(UILangProvider.Current["Tool.Source.VideoSrc"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["Tool.Source.VideoSrcQueue"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["Tool.Source.VideoSrcConcatState"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(RepartLangProvider.Current.ToolSourceVideoSrcRepart, StringComparison.OrdinalIgnoreCase))
            return SrcFileKind.Video;
        if (displayName.Equals(UILangProvider.Current["Tool.Source.AviSynth"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["Tool.Source.AviSynthQueue"], StringComparison.OrdinalIgnoreCase))
            return SrcFileKind.AviSynthScript;
        if (displayName.Equals(UILangProvider.Current["Tool.Source.VapourSynth"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["Tool.Source.VapourSynthQueue"], StringComparison.OrdinalIgnoreCase))
            return SrcFileKind.VapourSynthScript;
        if (displayName.Equals(UILangProvider.Current["Tool.Source.Svfi"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["Tool.Source.SvfiQueue"], StringComparison.OrdinalIgnoreCase))
            return SrcFileKind.SvfiIni;

        throw new ArgumentException($"Unknown source type: {displayName}");
    }

    public static SrcFileKind? GetPreferredScriptSrcKind(IEnumerable<ToolItemCardVM> upstreamsZone)
    {
        ToolItemCardVM? selectedUpstream = upstreamsZone.FirstOrDefault(t => t.IsSelected);
        string? upstreamExeName = selectedUpstream == null
            ? null
            : ToolCatalogProviderM.ResolveExeFromDisplayName(selectedUpstream.Name);

        return GetPreferredScriptSrcKind(upstreamExeName);
    }

    public static SrcFileKind? GetPreferredScriptSrcKind(string? upstreamExeName) => upstreamExeName switch
    {
        "vspipe.exe" => SrcFileKind.VapourSynthScript,
        "avs2yuv.exe" or "avs2pipemod.exe" => SrcFileKind.AviSynthScript,
        _ => null
    };

    public static bool IsQueueRouteSupportedUpstream(string? upstreamExeName) =>
        upstreamExeName?.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase) == true ||
        upstreamExeName?.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase) == true ||
        upstreamExeName?.Equals("avs2yuv.exe", StringComparison.OrdinalIgnoreCase) == true ||
        upstreamExeName?.Equals("avs2pipemod.exe", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsConcatRouteSupportedUpstream(string? upstreamExeName) =>
        upstreamExeName?.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase) == true ||
        upstreamExeName?.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase) == true ||
        upstreamExeName?.Equals("avs2yuv.exe", StringComparison.OrdinalIgnoreCase) == true ||
        upstreamExeName?.Equals("avs2pipemod.exe", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsRepartRouteSupportedUpstream(string? upstreamExeName) =>
        IsConcatRouteSupportedUpstream(upstreamExeName);
}
