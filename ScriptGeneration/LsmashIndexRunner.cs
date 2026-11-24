using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.ScriptGeneration;

/// <summary>
/// Progress snapshot for the L-SMASH index prefetch.
/// </summary>
public readonly record struct LsmashIndexProgress(
    int CompletedVideos,
    int TotalVideos,
    string CurrentFileName,
    int FilePercent,
    string CapturedLine);

/// <summary>
/// Pre-builds .lwi indexes with the bundled LSMASHSource_indexing.exe so the
/// first frame-server run after importing a source does not have to build them.
/// The runner writes indexes into the same cache folder used by generated
/// scripts (1cenc/LSMASHCaches).
/// </summary>
public static partial class LsmashIndexRunner
{
    private static readonly Regex ProgressPattern = CreateProgressPattern();
    private static bool _isRunning;

    public static bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value) return;
            _isRunning = value;
            RunningStateChanged?.Invoke();
        }
    }

    public static event Action? RunningStateChanged;

    public static string ExecutablePath => LsmashPluginPathProvider.IndexingExecutablePath;

    public static bool CanRun =>
        LwiCacheDirectoryProvider.IsEnabled && File.Exists(ExecutablePath);

    /// <summary>
    /// Indexes the given sources sequentially. Missing sources are skipped so the
    /// progress count still advances. Cancellation kills the active indexing process.
    /// </summary>
    public static async Task RunAsync(
        IReadOnlyList<string> videoPaths,
        IProgress<LsmashIndexProgress> progress,
        CancellationToken cancellationToken = default)
    {
        if (IsRunning || videoPaths.Count == 0) return;

        string executablePath = ExecutablePath;
        string? outputDirectory = LwiCacheDirectoryProvider.EnsureDirectory();
        if (!File.Exists(executablePath) || string.IsNullOrWhiteSpace(outputDirectory))
            return;

        IsRunning = true;
        try
        {
            int total = videoPaths.Count;
            for (int i = 0; i < total; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string videoPath = videoPaths[i];
                string fileName = Path.GetFileName(videoPath);
                if (string.IsNullOrWhiteSpace(videoPath) || !File.Exists(videoPath))
                {
                    progress.Report(new(i + 1, total, fileName, 100, string.Empty));
                    continue;
                }

                string indexPath = Path.Combine(
                    outputDirectory,
                    Path.GetFileNameWithoutExtension(videoPath) + ".lwi");

                try
                {
                    await RunSingleAsync(
                        executablePath,
                        videoPath,
                        indexPath,
                        i,
                        total,
                        progress,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LsmashIndexRunner] Indexing failed for '{videoPath}': {ex.Message}");
                }

                progress.Report(new(i + 1, total, fileName, 100, string.Empty));
            }
        }
        finally
        {
            IsRunning = false;
        }
    }

    private static async Task RunSingleAsync(
        string executablePath,
        string videoPath,
        string indexPath,
        int completedVideos,
        int totalVideos,
        IProgress<LsmashIndexProgress> progress,
        CancellationToken cancellationToken)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(indexPath) ?? Environment.CurrentDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(videoPath);
        startInfo.ArgumentList.Add(indexPath);

        using Process process = new() { StartInfo = startInfo };
        process.Start();

        string fileName = Path.GetFileName(videoPath);
        object percentGate = new();
        int lastPercent = -1;

        void ReportLine(string line)
        {
            Match match = ProgressPattern.Match(line);
            if (!match.Success
                || !int.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int percent))
                return;

            percent = Math.Clamp(percent, 0, 100);
            lock (percentGate)
            {
                // L-SMASH prints increasing values; ignore stale/duplicate updates.
                if (percent < lastPercent) return;
                if (percent == lastPercent) return;
                lastPercent = percent;
            }

            progress.Report(new(
                completedVideos,
                totalVideos,
                fileName,
                percent,
                line.Trim()));
        }

        Task stdoutTask = PumpAsync(process.StandardOutput, ReportLine, cancellationToken);
        Task stderrTask = PumpAsync(process.StandardError, ReportLine, cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(stdoutTask, stderrTask);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch { }
            throw;
        }
    }

    // The indexing tool reports progress with carriage returns instead of
    // newlines, so split on both characters while reading incrementally.
    private static async Task PumpAsync(
        StreamReader reader,
        Action<string> onLine,
        CancellationToken cancellationToken)
    {
        char[] buffer = new char[512];
        StringBuilder pending = new();

        while (true)
        {
            int read = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0) break;

            for (int i = 0; i < read; i++)
            {
                char value = buffer[i];
                if (value is '\r' or '\n')
                {
                    if (pending.Length == 0) continue;
                    onLine(pending.ToString());
                    pending.Clear();
                }
                else
                {
                    pending.Append(value);
                }
            }
        }

        if (pending.Length > 0)
            onLine(pending.ToString());
    }

    private static Regex CreateProgressPattern() =>
        new(@"Creating lwi index file\s+(\d+)\s*%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
