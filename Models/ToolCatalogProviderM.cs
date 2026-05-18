using System;
using System.Collections.Generic;
using System.Linq;

namespace OneColumnEncoder.Models;

public static class ToolCatalogProviderM
{
    // ── Source Import zone (4 items) ──────────────────────────────
    public static List<ToolDefinitionM> GetSourceImportDefinitions() =>
    [
        new("Video Source",            "Replace", "Clear", "Name", "Path"),
        new("AviSynth .avs Source",    "Replace", "Clear", "Mode", "Path"),
        new("VapourSynth .vpy Source", "Replace", "Clear", "Mode", "Path"),
        new("SVFI .ini Source",        "Replace", "Clear", "Mode", "Path"),
    ];

    // ── Enc Settings zone (5 items) ────────────────────────────────
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

    // ── Importable tool registry (exe name → display name + zone) ──
    private static readonly Dictionary<string, (string DisplayName, ToolZone Zone)> _tools = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ffmpeg.exe"] = ("FFMPEG", ToolZone.Upstream),
        ["vspipe.exe"] = ("VSPipe", ToolZone.Upstream),
        ["avs2yuv.exe"] = ("AVS2YUV", ToolZone.Upstream),
        ["avs2pipemod.exe"] = ("AVS2PipeMod", ToolZone.Upstream),
        ["one_line_shot_args.exe"] = ("OneLineShotArgs", ToolZone.Upstream),
        ["x264.exe"] = ("x264", ToolZone.Encoder),
        ["x265.exe"] = ("x265", ToolZone.Encoder),
        ["svtav1encapp.exe"] = ("SVT-AV1", ToolZone.Encoder),
        ["ffprobe.exe"] = ("FFProbe", ToolZone.Analytics),
        ["avisynth.dll"] = ("AviSynth.dll (for Avs2PipeMod)", ToolZone.Analytics),
    };

    // Importable tools also need a "Version" / "Path" label pattern,
    // but R1/R2Text depends on context (loaded vs. freshly imported),
    // so those stay in MainVM's AddTool / OnToolImported.

    // ── Lookup helpers ─────────────────────────────────────────────
    public static (string DisplayName, ToolZone Zone)? ResolveExe(string exeName) =>
        _tools.TryGetValue(exeName, out var entry) ? entry : null;

    public static string? GetDisplayName(string exeName) =>
        _tools.TryGetValue(exeName, out var entry) ? entry.DisplayName : null;

    public static ToolZone? GetZone(string exeName) =>
        _tools.TryGetValue(exeName, out var entry) ? entry.Zone : null;

    public static IReadOnlyDictionary<string, (string DisplayName, ToolZone Zone)> AllImportableTools => _tools;

    // ── Display-name sets used by UpdateEncodingStartButtonsState ──
    public static HashSet<string> UpstreamDisplayNames { get; } =
        _tools.Values.Where(v => v.Zone == ToolZone.Upstream).Select(v => v.DisplayName).ToHashSet();

    public static HashSet<string> EncoderDisplayNames { get; } =
        _tools.Values.Where(v => v.Zone == ToolZone.Encoder).Select(v => v.DisplayName).ToHashSet();

    public static HashSet<string> AnalyticsDisplayNames { get; } =
        _tools.Values.Where(v => v.Zone == ToolZone.Analytics).Select(v => v.DisplayName).ToHashSet();

    // ── AppDataM.Importables property mapping ──────────────────────
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

    public static bool TrySetPath(string exeName, AppDataM.Importables tools, string filePath)
    {
        if (_pathSetters.TryGetValue(exeName, out var setter))
        {
            setter(tools, filePath);
            return true;
        }
        return false;
    }

    // ── Dropdown items for the import dropdown ─────────────────────
    public static List<DropdownItemM> GetImportDropdownItems() =>
    [
        new("No Selection"),
        new("", true),
        new("ffmpeg.exe"),
        new("vspipe.exe"),
        new("avs2yuv.exe"),
        new("avs2pipemod.exe"),
        new("one_line_shot_args.exe"),
        new("", true),
        new("x264.exe"),
        new("x265.exe"),
        new("SvtAv1EncApp.exe"),
        new("", true),
        new("ffprobe.exe"),
        new("AviSynth.dll"),
    ];
}
