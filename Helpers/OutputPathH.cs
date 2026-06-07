using System;
using System.IO;

namespace OneColumnEncoder.Helpers;

public static class OutputPathH
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
}
