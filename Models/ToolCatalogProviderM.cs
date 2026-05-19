using System;
using System.Collections.Generic;
using System.Linq;

namespace OneColumnEncoder.Models;

public static class ToolCatalogProviderM
{
    // Source Import zone (4 items)
    public static List<ToolDefinitionM> GetSourceImportDefinitions() =>
    [
        new("Video Source",            "Replace", "Clear", "Name", "Path"),
        new("AviSynth .avs Source",    "Replace", "Clear", "Mode", "Path"),
        new("VapourSynth .vpy Source", "Replace", "Clear", "Mode", "Path"),
        new("SVFI .ini Source",        "Replace", "Clear", "Mode", "Path"),
    ];

    // Enc Settings zone (5 items)
    public static List<ToolDefinitionM> GetEncSettingsDefinitions() =>
    [
        new("Output Setting",           "Edit", "Clear", "File name w/out extension", "Path"),
        new("Parallelism",              "Edit", "Clear", "CPU-RAM Nodes", "Threads"),
        new("Rate Control Mechanism",   "Edit", "Clear", "Mode", "Value"),
        new("Base Parameters",          "Edit", "Clear", "Stratagem"),
        new("Custom Parameters",        "Edit", "Clear", "Maximum keyframe gap", "Other custom params"),
    ];

    public static List<ToolDefinitionM> GetAllStaticDefinitions() =>
        [.. GetSourceImportDefinitions(), .. GetEncSettingsDefinitions()];

    // Importable tool registry derived from ToolDefinitionProviderM
    private static Dictionary<string, (string DisplayName, ToolZone Zone)> BuildToolsDict()
    {
        var dict = new Dictionary<string, (string, ToolZone)>(StringComparer.OrdinalIgnoreCase);
        foreach (var def in ToolDefinitionProviderM.ToolDefinitions.Values)
        {
            if (def.ExeName != null && def.Zone != null)
                dict[def.ExeName] = (def.DisplayName, def.Zone.Value);
        }
        return dict;
    }

    private static readonly Dictionary<string, (string DisplayName, ToolZone Zone)> _tools = BuildToolsDict();

    // Lookup helpers
    public static (string DisplayName, ToolZone Zone)? ResolveExe(string exeName) =>
        _tools.TryGetValue(exeName, out var entry) ? entry : null;

    public static string? ResolveExeFromDisplayName(string displayName) =>
        _tools.FirstOrDefault(kvp => kvp.Value.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase)).Key;

    public static string? GetDisplayName(string exeName) =>
        _tools.TryGetValue(exeName, out var entry) ? entry.DisplayName : null;

    public static ToolZone? GetZone(string exeName) =>
        _tools.TryGetValue(exeName, out var entry) ? entry.Zone : null;

    public static IReadOnlyDictionary<string, (string DisplayName, ToolZone Zone)> AllImportableTools => _tools;

    // Display-name sets used by UpdateEncodingStartButtonsState
    public static HashSet<string> UpstreamDisplayNames { get; } =
        _tools.Values.Where(v => v.Zone == ToolZone.Upstream).Select(v => v.DisplayName).ToHashSet();

    public static HashSet<string> EncoderDisplayNames { get; } =
        _tools.Values.Where(v => v.Zone == ToolZone.Encoder).Select(v => v.DisplayName).ToHashSet();

    public static HashSet<string> AnalyticsDisplayNames { get; } =
        _tools.Values.Where(v => v.Zone == ToolZone.Analytics).Select(v => v.DisplayName).ToHashSet();

    // AppDataM.Importables property mapping
    private static readonly Dictionary<string, Action<AppDataM.Importables, string>> _pathSetters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ffmpeg.exe"] = (t, p) => t.FfmpegPath = p,
        ["vspipe.exe"] = (t, p) => t.VspipePath = p,
        ["avs2yuv.exe"] = (t, p) => t.Avs2yuvPath = p,
        ["avs2pipemod.exe"] = (t, p) => t.Avs2pipemodPath = p,
        ["one_line_shot_args.exe"] = (t, p) => t.OneLineShotArgsPath = p,
        ["x264.exe"] = (t, p) => t.X264Path = p,
        ["x265.exe"] = (t, p) => t.X265Path = p,
        ["svtav1encapp.exe"] = (t, p) => t.SvtAv1Path = p,
        ["ffprobe.exe"] = (t, p) => t.FfprobePath = p,
        ["avisynth.dll"] = (t, p) => t.AviSynthDllPath = p,
    };

    private static readonly Dictionary<string, Action<AppDataM.Importables, string>> _versionSetters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ffmpeg.exe"] = (t, v) => t.FfmpegVer = v,
        ["vspipe.exe"] = (t, v) => t.VspipeVer = v,
        ["avs2yuv.exe"] = (t, v) => t.Avs2yuvVer = v,
        ["avs2pipemod.exe"] = (t, v) => t.Avs2pipemodVer = v,
        ["x264.exe"] = (t, v) => t.X264Ver = v,
        ["x265.exe"] = (t, v) => t.X265Ver = v,
        ["svtav1encapp.exe"] = (t, v) => t.SvtAv1Ver = v,
        ["ffprobe.exe"] = (t, v) => t.FfprobeVer = v,
    };

    public static bool TrySetPath(string exeName, AppDataM.Importables tools, string filePath)
    {
        if (_pathSetters.TryGetValue(exeName, out var setter))
        {
            setter(tools, filePath);
            return true;
        }
        return false;
    }

    public static bool TrySetVersion(string exeName, AppDataM.Importables tools, string version)
    {
        if (_versionSetters.TryGetValue(exeName, out var setter))
        {
            setter(tools, version);
            return true;
        }
        return false;
    }

    // Dropdown items for the import dropdown
    public static List<DropdownItemM> GetImportDropdownItems() =>
    [
        new("No Selection"),
        new("", true),
        .. ToolDefinitionProviderM.ToolDefinitions.Values
            .Where(d => d.ExeName != null)
            .Select(d => new DropdownItemM(d.ExeName!)),
    ];
}