using System.IO;

namespace OneColumnEncoder.Models;

public static class FileManagementProviderM
{
    public static string GetInitialFilename(string? versionText, string? filePath)
    {
        if (!string.IsNullOrWhiteSpace(versionText))
            return versionText;

        if (!string.IsNullOrWhiteSpace(filePath))
            return Path.GetFileNameWithoutExtension(filePath);

        return string.Empty;
    }

    public static string GetInitialDirectory(string? filePath)
    {
        string desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(filePath)) return desktopDirectory;

        if (Directory.Exists(filePath)) return filePath;

        string? directory = Path.GetDirectoryName(filePath);
        return Directory.Exists(directory) ? directory : desktopDirectory;
    }

    public static SrcFileKind ResolveSrcFileKind(string displayName)
    {
        if (displayName.Equals(UILangProvider.Current["Tool.Source.VideoSrc"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["SrcQueue"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(UILangProvider.Current["SrcConcat"], StringComparison.OrdinalIgnoreCase) ||
            displayName.Equals(RepartLangProvider.Current.ToolSourceSrcRepart, StringComparison.OrdinalIgnoreCase))
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
            : ToolCatalogProviderM.ResolveExeFromDefinitionKey(selectedUpstream.DefinitionKey);

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
