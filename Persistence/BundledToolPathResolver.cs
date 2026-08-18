using System.IO;

namespace OneColumnEncoder.Persistence;

/// <summary>
/// Resolves paths of bundled tools/folders that ship inside the config directory
/// (1cenc). The config directory is preferred; the app base directory is used as
/// a fallback so installs that keep these folders next to the executable keep
/// working.
/// </summary>
public static class BundledToolPathResolver
{
    /// <summary>
    /// Returns the folder under the config directory for the given folder name,
    /// falling back to the app base directory when a legacy copy exists there.
    /// </summary>
    public static string ResolveFolder(string folderName)
    {
        string configPath = Path.Combine(SaveLoadBase<AppConfM>.GetConfigDirectory(), folderName);
        if (Directory.Exists(configPath)) return configPath;

        string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
        return Directory.Exists(basePath) ? basePath : configPath;
    }
}
