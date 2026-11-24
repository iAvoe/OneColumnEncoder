using System.IO;

namespace OneColumnEncoder.FileManagement;

/// <summary>
/// Manages file locks during encoding to prevent accidental deletion or modification.
/// Uses write-lock (read-shared) so multiple processes can read while preventing writes.
/// </summary>
public class FileLockManager : IDisposable
{
    private readonly Dictionary<string, FileStream> _lockedFiles = new();
    private bool _disposed;

    /// <summary>
    /// Attempts to lock a file with read access and read-only sharing.
    /// Only locks files on local drives (not network drives or unsupported file systems).
    /// </summary>
    /// <param name="filePath">Path to the file to lock</param>
    /// <returns>True if locked successfully or skipped (network drive); false if lock failed</returns>
    public bool TryLockFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        // Skip network drives and unsupported file systems
        if (!IsLocalDrive(filePath))
            return true; // Return true to indicate "handled" (skipped intentionally)

        try
        {
            // FileAccess.Read with FileShare.Read allows other processes to read but not write/delete
            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            
            _lockedFiles[filePath] = stream;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Unlocks a specific file by disposing its stream.
    /// </summary>
    public void UnlockFile(string filePath)
    {
        if (_lockedFiles.TryGetValue(filePath, out var stream))
        {
            stream?.Dispose();
            _lockedFiles.Remove(filePath);
        }
    }

    /// <summary>
    /// Unlocks all currently locked files.
    /// </summary>
    public void UnlockAll()
    {
        foreach (var stream in _lockedFiles.Values)
        {
            stream?.Dispose();
        }
        _lockedFiles.Clear();
    }

    /// <summary>
    /// Checks if a path is on a local drive (not a network drive).
    /// </summary>
    private static bool IsLocalDrive(string filePath)
    {
        try
        {
            string? root = Path.GetPathRoot(filePath);
            if (string.IsNullOrWhiteSpace(root))
                return false;

            DriveInfo drive = new(root);
            return drive.DriveType != DriveType.Network && drive.DriveType != DriveType.Unknown;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        UnlockAll();
        _disposed = true;
    }
}
