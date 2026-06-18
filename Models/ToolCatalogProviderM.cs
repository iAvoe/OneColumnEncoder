using System.IO;

namespace OneColumnEncoder.Models;

public static class ToolCatalogProviderM
{
    // Video Source Import zone (2 items)
    public static List<ToolDefinitionM> GetVideoSrcImportDefs() =>
    [
        new(UILangProviderM.Current["Tool.Source.VideoSource"],
            UILangProviderM.Current["Buttons.Replace"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.Name"],
            UILangProviderM.Current["ToolField.Path"]),
        new(UILangProviderM.Current["Tool.Source.VideoSrcQueue"],
            UILangProviderM.Current["Buttons.Import"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["SourceQueue.Queue"],
            UILangProviderM.Current["ToolField.Path"]),
    ];
    // Script Source Import zone (3 items)
    public static List<ToolDefinitionM> GetScriptSrcImportDefs() =>
    [
        new(UILangProviderM.Current["Tool.Source.AviSynth"],
            UILangProviderM.Current["Buttons.Replace"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.Mode"],
            UILangProviderM.Current["ToolField.Path"]),
        new(UILangProviderM.Current["Tool.Source.VapourSynth"],
            UILangProviderM.Current["Buttons.Replace"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.Mode"],
            UILangProviderM.Current["ToolField.Path"]),
        new(UILangProviderM.Current["Tool.Source.Svfi"],
            UILangProviderM.Current["Buttons.Replace"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.Mode"],
            UILangProviderM.Current["ToolField.Path"]),
    ];

    // Enc Settings zone (3 items)
    public static List<ToolDefinitionM> GetEncSettingsDefinitions() =>
    [
        new(UILangProviderM.Current["Tool.Enc.OutputSetting"],
            UILangProviderM.Current["Buttons.Edit"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.FileName"],
            UILangProviderM.Current["ToolField.Path"]),
        new(UILangProviderM.Current["Tool.Enc.Parallelism"],
            UILangProviderM.Current["Buttons.Edit"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.NumaNodes"],
            UILangProviderM.Current["ToolField.Threads"]),
        new(UILangProviderM.Current["Tool.Enc.EncParams"],
            UILangProviderM.Current["Buttons.Edit"],
            UILangProviderM.Current["Buttons.Clear"],
            UILangProviderM.Current["ToolField.Strategy"],
            UILangProviderM.Current["ToolField.MaxKeyframeGap"]),
    ];

    public static List<ToolDefinitionM> GetAllStaticDefinitions() =>
        [.. GetVideoSrcImportDefs(), .. GetScriptSrcImportDefs(), .. GetEncSettingsDefinitions()];

    // Importable tool registry derived from ToolDefinitionProviderM
    private static Dictionary<string, (string DisplayName, ToolZone Zone)> BuildToolsDict()
    {
        var dict = new Dictionary<string, (string, ToolZone)>(StringComparer.OrdinalIgnoreCase);
        foreach (var def in ToolDefinitionProviderM.ToolDefs.Values)
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
        [.. _tools.Values.Where(v => v.Zone == ToolZone.Upstream).Select(v => v.DisplayName)];

    public static HashSet<string> EncoderDisplayNames { get; } =
        [.. _tools.Values.Where(v => v.Zone == ToolZone.Encoder).Select(v => v.DisplayName)];

    public static HashSet<string> AnalyticsDisplayNames { get; } =
        [.. _tools.Values.Where(v => v.Zone == ToolZone.Analytics).Select(v => v.DisplayName)];

    public static HashSet<string> DependenciesDisplayNames { get; } =
        [.. _tools.Values.Where(v => v.Zone == ToolZone.Dependencies).Select(v => v.DisplayName)];

    // AppDataM.Importables property mapping
    private static readonly Dictionary<string, Action<AppDataM.Importables, string>> _pathSetters =
        new(StringComparer.OrdinalIgnoreCase)
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

    private static readonly Dictionary<string, Action<AppDataM.Importables, long?>> _sizeSetters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ffmpeg.exe"] = (t, s) => t.FfmpegSize = s,
            ["vspipe.exe"] = (t, s) => t.VspipeSize = s,
            ["avs2yuv.exe"] = (t, s) => t.Avs2yuvSize = s,
            ["avs2pipemod.exe"] = (t, s) => t.Avs2pipemodSize = s,
            ["one_line_shot_args.exe"] = (t, s) => t.OneLineShotArgsSize = s,
            ["x264.exe"] = (t, s) => t.X264Size = s,
            ["x265.exe"] = (t, s) => t.X265Size = s,
            ["svtav1encapp.exe"] = (t, s) => t.SvtAv1Size = s,
            ["ffprobe.exe"] = (t, s) => t.FfprobeSize = s,
            ["avisynth.dll"] = (t, s) => t.AviSynthDllSize = s,
        };

    private static readonly Dictionary<string, Action<AppDataM.Importables, string>> _versionSetters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ffmpeg.exe"] = (t, v) => t.FfmpegVer = v,
        ["vspipe.exe"] = (t, v) => t.VspipeVer = v,
        ["avs2yuv.exe"] = (t, v) => t.Avs2yuvVer = v,
        ["avs2pipemod.exe"] = (t, v) => t.Avs2pipemodVer = v,
        ["one_line_shot_args.exe"] = (t, v) => t.OneLineShotArgsVer = v,
        ["x264.exe"] = (t, v) => t.X264Ver = v,
        ["x265.exe"] = (t, v) => t.X265Ver = v,
        ["svtav1encapp.exe"] = (t, v) => t.SvtAv1Ver = v,
        ["ffprobe.exe"] = (t, v) => t.FfprobeVer = v,
        ["avisynth.dll"] = (t, v) => t.AviSynthDllVer = v,
    };

    public static readonly Dictionary<string, string[]> ToolExtraSearchPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vspipe.exe"] =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VapourSynth", "core"),
        ],
        ["one_line_shot_args.exe"] =
        [
            .. DriveInfo.GetDrives().Select(d =>
                Path.Combine(d.RootDirectory.FullName, "SteamLibrary", "steamapps", "common", "SVFI"))
        ],
    };

    public static string? TryFindToolDirectory(string exeName)
    {
        if (!ToolExtraSearchPaths.TryGetValue(exeName, out var directories))
            return null;
        foreach (var dir in directories)
        {
            if (Directory.Exists(dir))
            {
                string filePath = Path.Combine(dir, exeName);
                if (File.Exists(filePath))
                    return dir;
            }
        }
        return null;
    }

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

    public static bool TrySetSize(string exeName, AppDataM.Importables tools, long? fileSize)
    {
        if (_sizeSetters.TryGetValue(exeName, out var setter))
        {
            setter(tools, fileSize);
            return true;
        }
        return false;
    }

    public static long? GetFileSize(string filePath)
    {
        try
        {
            if (File.Exists(filePath)) return new FileInfo(filePath).Length;
        }
        catch { }
        return null;
    }

    // Dropdown items for the import dropdown (grouped by zone)
    public static List<DropdownItemM> GetImportDropdownItems()
    {
        var items = new List<DropdownItemM>
        {
            new(UILangProviderM.Current["Import.NoSelection"], isPlaceholder: true),
            new("", true)
        };

        ToolZone? prevZone = null;
        foreach (var def in ToolDefinitionProviderM.ToolDefs.Values)
        {
            if (def.ExeName == null || def.Zone == null) continue;

            if (prevZone != null && def.Zone != prevZone)
                items.Add(new("", true));

            items.Add(new DropdownItemM(def.ExeName));
            prevZone = def.Zone;
        }

        return items;
    }
}
