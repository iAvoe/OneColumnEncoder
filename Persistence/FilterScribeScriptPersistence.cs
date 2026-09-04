using System.IO;

namespace OneColumnEncoder.Persistence;

/// <summary>
/// Persistence helpers for AviSynth/VapourSynth script export.
/// Keeps file-name derivation and disk writes out of the view model so
/// FilterScribeVM only orchestrates the save/import flow.
/// </summary>
public static class FilterScribeScriptPersistence
{
    // Determine script filename from source filename
    public static string GetScriptFileName(string srcPath, string extension) =>
        Path.GetFileNameWithoutExtension(srcPath) + extension;

    public static bool TryWriteScripts(
        string avsPath,
        string avsScript,
        string vpyPath,
        string vpyScript,
        Action<Exception> onError)
    {
        try
        {
            File.WriteAllText(avsPath, avsScript);
            File.WriteAllText(vpyPath, vpyScript);
            return true;
        }
        catch (Exception ex)
        {
            onError(ex);
            return false;
        }
    }
}
