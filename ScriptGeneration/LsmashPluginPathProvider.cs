using OneColumnEncoder.Models;
using OneColumnEncoder.Persistence;
using System.IO;

namespace OneColumnEncoder.ScriptGeneration;

/// <summary>
/// Resolves the bundled L-SMASH Works plugin. LSMASHSource.dll is a dual
/// AviSynth/VapourSynth plugin shipped in the 1cenc plugin folders, so generated
/// scripts must import it explicitly instead of relying on a system-wide
/// AviSynth+ or VapourSynth plugin directory.
/// </summary>
public static class LsmashPluginPathProvider
{
    public const string FileName = "LSMASHSource.dll";
    private const string VapourSynthOnlyFileName = "libvslsmashsource.dll";

    private static string? _cachedVspipePath;
    private static long _cachedAppDataStamp = long.MinValue;
    private static bool _vspipePathCached;

    public static string PluginsDirectory => Environment.Is64BitProcess
        ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
        : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");

    public static string PluginPath => Path.Combine(PluginsDirectory, FileName);

    public static bool PluginExists => File.Exists(PluginPath);

    /// <summary>
    /// VapourSynth autoload plugin folder derived from the configured vspipe.exe
    /// path in appdata.json: "...\VapourSynth\core\vspipe.exe" becomes
    /// "...\VapourSynth\plugins". Returns null when vspipe is not configured.
    /// </summary>
    public static string? ResolveVapourSynthPluginsDirectory()
    {
        string? vspipePath = GetConfiguredVspipePath();
        if (string.IsNullOrWhiteSpace(vspipePath)) return null;

        string? vspipeDirectory = Path.GetDirectoryName(vspipePath);
        if (string.IsNullOrEmpty(vspipeDirectory)) return null;

        // Modern VapourSynth installs keep vspipe.exe in a "core" subfolder of the
        // install root; portable layouts keep it next to the plugins folder.
        if (Path.GetFileName(vspipeDirectory).Equals("core", StringComparison.OrdinalIgnoreCase))
        {
            string? installRoot = Path.GetDirectoryName(vspipeDirectory);
            if (!string.IsNullOrEmpty(installRoot))
                vspipeDirectory = installRoot;
        }

        return Path.Combine(vspipeDirectory, "plugins");
    }

    /// <summary>
    /// True when the VapourSynth install already autoloads an L-SMASH source
    /// plugin. Loading the bundled copy then raises "plugin already loaded".
    /// </summary>
    public static bool VapourSynthHasLsmashPlugin
    {
        get
        {
            try
            {
                string? pluginsDirectory = ResolveVapourSynthPluginsDirectory();
                if (string.IsNullOrEmpty(pluginsDirectory)) return false;

                return File.Exists(Path.Combine(pluginsDirectory, FileName))
                    || File.Exists(Path.Combine(pluginsDirectory, VapourSynthOnlyFileName));
            }
            catch { return false; }
        }
    }

    public static string BuildAvsLoadPluginLine() => $"LoadPlugin(\"{PluginPath}\")";

    public static string BuildVpyLoadPluginLine() => $"core.std.LoadPlugin(r\"{PluginPath}\")";

    /// <summary>VapourSynth load line followed by a newline, or empty when not needed.</summary>
    public static string BuildVpyLoadPluginSection() =>
        VapourSynthHasLsmashPlugin ? string.Empty : $"{BuildVpyLoadPluginLine()}\r\n";

    /// <summary>Prepends the AviSynth LoadPlugin line to a source statement.</summary>
    public static string WithAvsLoadPlugin(string sourceLine) =>
        string.IsNullOrEmpty(sourceLine)
            ? sourceLine
            : $"{BuildAvsLoadPluginLine()}\r\n{sourceLine}";

    /// <summary>
    /// Inserts the VapourSynth LoadPlugin line right after the "core = vs.core"
    /// initialisation line so the lsmas namespace exists before LWLibavSource is
    /// called. Does nothing when VapourSynth already autoloads L-SMASH.
    /// </summary>
    public static string WithVpyLoadPlugin(string sourcePrefix)
    {
        if (string.IsNullOrEmpty(sourcePrefix) || VapourSynthHasLsmashPlugin)
            return sourcePrefix;

        const string coreInit = "core = vs.core";
        int index = sourcePrefix.IndexOf(coreInit, StringComparison.Ordinal);
        return index < 0
            ? $"{BuildVpyLoadPluginLine()}\r\n{sourcePrefix}"
            : sourcePrefix.Insert(index + coreInit.Length, $"\r\n{BuildVpyLoadPluginLine()}");
    }

    private static string? GetConfiguredVspipePath()
    {
        try
        {
            string configFile = Path.Combine(
                SaveLoadBase<AppDataM>.GetConfigDirectory(), "appdata.json");

            long stamp = File.Exists(configFile)
                ? File.GetLastWriteTimeUtc(configFile).Ticks
                : -1;

            if (!_vspipePathCached || stamp != _cachedAppDataStamp)
            {
                _cachedVspipePath = AppDataM.Load().Tools.VspipePath;
                _cachedAppDataStamp = stamp;
                _vspipePathCached = true;
            }

            return _cachedVspipePath;
        }
        catch { return null; }
    }
}
