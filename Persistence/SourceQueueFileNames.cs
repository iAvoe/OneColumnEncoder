using System.IO;

namespace OneColumnEncoder.Persistence;

/// <summary>
/// Single place that names the working files stored in the SourceQueues folder
/// (concat/repart file lists and repart input scripts).
/// All names follow "{prefix}_{yyyy-MM-dd_HH-mm-ss}[_{n}].{ext}"; when the plain
/// timestamp name is already taken, _1, _2, ... is appended until it is free.
/// </summary>
internal static class SourceQueueFileNames
{
    public static string CreateConcatFileListPath() =>
        CreateUniquePath("concat", ".txt");

    public static string CreateRepartFileListPath() =>
        CreateUniquePath("repart", ".txt");

    public static string CreateQueueJsonPath() =>
        CreateUniquePath("queue", ".json");

    public static string CreateQueueExcludedJsonPath() =>
        CreateUniquePath("queue_exc", ".json");

    /// <summary>
    /// Builds "{prefix}_{timestamp}{extension}" inside the SourceQueues folder,
    /// appending _1, _2, ... while any file already uses that base name.
    /// Base names are compared without extension so a repart .txt/.vpy/.avs
    /// group always shares one timestamp.
    /// </summary>
    private static string CreateUniquePath(string prefix, string extension)
    {
        string directory = SaveLoadBase<AppConfM>.GetSourceQueuesDirectory();
        string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

        HashSet<string> takenNames = Directory.Exists(directory)
            ? new(Directory.EnumerateFiles(directory)
                    .Select(file => Path.GetFileNameWithoutExtension(file) ?? string.Empty),
                StringComparer.OrdinalIgnoreCase)
            : [];

        string baseName = $"{prefix}_{stamp}";
        for (int attempt = 1; takenNames.Contains(baseName); attempt++)
            baseName = $"{prefix}_{stamp}_{attempt}";

        return Path.Combine(directory, baseName + extension);
    }
}
