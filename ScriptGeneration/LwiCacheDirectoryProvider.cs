using System.IO;

namespace OneColumnEncoder.ScriptGeneration;

/// <summary>
/// Resolves and creates the folder where LwLibavVideoSource/LWLibavSource writes .lwi index files.
/// The enabled flag mirrors AppConfM.CacheLwiToAppFolder and is applied by the app/commands.
/// </summary>
public static class LwiCacheDirectoryProvider
{
    public const string FolderName = "LSMASHCaches";

    public static bool IsEnabled { get; set; }

    /// <summary>
    /// Creates the cache folder when enabled and returns its path.
    /// Returns null when the option is disabled or the folder cannot be created.
    /// </summary>
    public static string? EnsureDirectory()
    {
        if (!IsEnabled) return null;

        string directory = Path.Combine(SaveLoadBase<AppConfM>.GetConfigDirectory(), FolderName);
        try
        {
            Directory.CreateDirectory(directory);
            return directory;
        }
        catch
        {
            // Fall back to the default side-by-side .lwi behavior rather than
            // emitting a cachedir the frame server cannot write to.
            return null;
        }
    }
}
