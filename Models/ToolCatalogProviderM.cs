using System.IO;

namespace OneColumnEncoder.Models;

/// <summary>
/// Catalog of tool card definitions and lookup helpers.
/// </summary>
public static class ToolCatalogProviderM
{
    // Video Source Import zone (4 items)
    public static List<ToolDefinitionM> GetVideoSrcImportDefs() =>
    [
        new(UILangProvider.Current["Tool.Source.VideoSrc"],
            UILangProvider.Current["Replace"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.Name"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Source.VideoSrc"),
        new(UILangProvider.Current["SrcQueue"],
            UILangProvider.Current["Import"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["SourceQueue.Sequence"],
            UILangProvider.Current["ToolField.Path"], Key: "SrcQueue"),
        new(UILangProvider.Current["SrcConcat"],
            UILangProvider.Current["Import"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["SourceQueue.Sequence"],
            UILangProvider.Current["ToolField.Path"], Key: "SrcConcat"),
        new(RepartLangProvider.Current.ToolSourceSrcRepart,
            UILangProvider.Current["Import"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["SourceQueue.Sequence"],
            UILangProvider.Current["ToolField.Path"], Key: "SrcRepart"),
    ];
    // Script Source Import zone (3 items)
    public static List<ToolDefinitionM> GetScriptSrcImportDefs() =>
    [
        new(UILangProvider.Current["Tool.Source.AviSynth"],
            UILangProvider.Current["Replace"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.Mode"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Source.AviSynth"),
        new(UILangProvider.Current["Tool.Source.VapourSynth"],
            UILangProvider.Current["Replace"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.Mode"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Source.VapourSynth"),
        new(UILangProvider.Current["Tool.Source.Svfi"],
            UILangProvider.Current["Replace"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.Mode"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Source.Svfi"),
    ];
    // Script Source Import Queue zone (3 items, distinct labels)
    public static List<ToolDefinitionM> GetScriptSrcImportQueueDefs() =>
    [
        new(UILangProvider.Current["Tool.Source.AviSynthQueue"],
            UILangProvider.Current["Import"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["SourceQueue.Sequence"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Source.AviSynthQueue"),
        new(UILangProvider.Current["Tool.Source.VapourSynthQueue"],
            UILangProvider.Current["Import"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["SourceQueue.Sequence"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Source.VapourSynthQueue"),
    ];

    // Enc Settings zone (4 items)
    public static List<ToolDefinitionM> GetEncSettingsDefinitions() =>
    [
        new(UILangProvider.Current["Tool.Enc.OutputSetting"],
            UILangProvider.Current["Edit"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.FileName"],
            UILangProvider.Current["ToolField.Path"], Key: "Tool.Enc.OutputSetting"),
        new(UILangProvider.Current["Tool.Enc.Parallelism"],
            UILangProvider.Current["Edit"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.NumaNodes"],
            UILangProvider.Current["ToolField.Threads"], Key: "Tool.Enc.Parallelism"),
        new(UILangProvider.Current["Tool.Enc.EncParams"],
            UILangProvider.Current["Edit"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.Strategy"],
            UILangProvider.Current["ToolField.MaxKeyframeGap"], Key: "Tool.Enc.EncParams"),
        new(UILangProvider.Current["Tool.Enc.MuxTracks"],
            UILangProvider.Current["Edit"],
            UILangProvider.Current["Clear"],
            UILangProvider.Current["ToolField.Path"],
            UILangProvider.Current["ToolField.Value"],
            Key: "Tool.Enc.MuxTracks"),
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

    public static string? ResolveExeFromDefinitionKey(string? definitionKey) =>
        definitionKey == null ? null : ToolDefinitionProviderM.GetByKey(definitionKey)?.ExeName;

    public static string? ResolveExeFromCard(ToolItemCardVM item) =>
        ResolveExeFromDefinitionKey(item.DefinitionKey);

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
            ["ffmpeg.exe"] = (t, p) => t.FFmpegPath = p,
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
            ["ffmpeg.exe"] = (t, s) => t.FFmpegSize = s,
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
        ["ffmpeg.exe"] = (t, v) => t.FFmpegVer = v,
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

private static IEnumerable<string> GetBaseDirectoryUpstreamSearchPaths()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string configDirectory = SaveLoadBase<AppConfM>.GetConfigDirectory();

        if (Environment.Is64BitProcess)
        {
            yield return Path.Combine(configDirectory, "x64-upstreams-encoders");
            yield return Path.Combine(baseDirectory, "x64-upstreams-encoders");
        }
        else
        {
            yield return Path.Combine(configDirectory, "x86-upstreams-encoders");
            yield return Path.Combine(baseDirectory, "x86-upstreams-encoders");
        }

        yield return configDirectory;
    }

    public static string? TryFindToolDirectory(string exeName)
    {
        List<string> directories = [];
        if (ToolExtraSearchPaths.TryGetValue(exeName, out var extraDirectories))
            directories.AddRange(extraDirectories);

        directories.AddRange(GetBaseDirectoryUpstreamSearchPaths());

        foreach (string dir in directories)
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
            new(UILangProvider.Current["Import.NoSelection"], isPlaceholder: true),
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
