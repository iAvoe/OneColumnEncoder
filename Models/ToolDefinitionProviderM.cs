namespace OneColumnEncoder.Models;

/// <summary>
/// Registry of imported tool definitions.
/// </summary>
public static class ToolDefinitionProviderM
{
    public static Dictionary<string, ToolDefinitionM> ToolDefs => BuildToolDefinitions();

    private static Dictionary<string, ToolDefinitionM> BuildToolDefinitions()
    {
        string replace = UILangProvider.Current["Replace"];
        string deleteText = UILangProvider.Current["Delete"];
        string version = UILangProvider.Current["ToolField.Version"];
        string path = UILangProvider.Current["ToolField.Path"];

        return new()
        {
            // Upstream
            ["Ffmpeg"] = new("FFMPEG", replace, deleteText, version, path, ToolZone.Upstream, "ffmpeg.exe", "Ffmpeg"),
            ["Vspipe"] = new("VSPipe", replace, deleteText, version, path, ToolZone.Upstream, "vspipe.exe", "Vspipe"),
            ["Avs2yuv"] = new("Avs2yuv", replace, deleteText, version, path, ToolZone.Upstream, "avs2yuv.exe", "Avs2yuv"),
            ["Avs2pipemod"] = new("Avs2pipemod", replace, deleteText, version, path, ToolZone.Upstream, "avs2pipemod.exe", "Avs2pipemod"),
            ["OneLineShotArgs"] = new("OneLineShotArgs", replace, deleteText, version, path, ToolZone.Upstream, "one_line_shot_args.exe", "OneLineShotArgs"),
            // Encoder
            ["X264"] = new("x264", replace, deleteText, version, path, ToolZone.Encoder, "x264.exe", "X264"),
            ["X265"] = new("x265", replace, deleteText, version, path, ToolZone.Encoder, "x265.exe", "X265"),
            ["SvtAv1"] = new("SVT-AV1", replace, deleteText, version, path, ToolZone.Encoder, "svtav1encapp.exe", "SvtAv1"),
            // Analytics
            ["Ffprobe"] = new("FFProbe", replace, deleteText, version, path, ToolZone.Analytics, "ffprobe.exe", "Ffprobe"),
            ["AviSynthDll"] = new("AviSynth.dll", replace, deleteText, version, path, ToolZone.Dependencies, "avisynth.dll", "AviSynthDll"),
        };
    }

    public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetUpstreamDefinitions() =>
        ToolDefs.Where(kvp => kvp.Value.Zone == ToolZone.Upstream);

    public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetEncoderDefinitions() =>
        ToolDefs.Where(kvp => kvp.Value.Zone == ToolZone.Encoder);

    public static IEnumerable<KeyValuePair<string, ToolDefinitionM>> GetAnalyticsDefinitions() =>
        ToolDefs.Where(kvp => kvp.Value.Zone == ToolZone.Analytics);

    public static ToolDefinitionM? GetByExeName(string exeName) =>
        ToolDefs.Values.FirstOrDefault(
            d => d.ExeName?.Equals(exeName, StringComparison.OrdinalIgnoreCase) == true);

    public static ToolDefinitionM? GetByKey(string key) =>
        ToolDefs.Values.FirstOrDefault(d => d.Key?.Equals(key, StringComparison.OrdinalIgnoreCase) == true);

    public static bool IsImportedToolByKey(string? definitionKey, string exeName) =>
        definitionKey != null
        && GetByKey(definitionKey)?.ExeName?.Equals(exeName, StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsImportedTool(ToolItemCardVM item, string exeName) =>
        IsImportedToolByKey(item.DefinitionKey, exeName);
    public static ToolZone ResolveToolZoneByKey(string definitionKey)
    {
        ToolDefinitionM? def = GetByKey(definitionKey);
        return def?.Zone ?? throw new ArgumentException($"Unknown tool key: {definitionKey}");
    }
}
