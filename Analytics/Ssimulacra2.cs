using System.IO;

namespace OneColumnEncoder.Analytics;

public static class Ssimulacra2
{
    private const string ToolDirName = "x64-CloudinarySSIMULACRA2.1";
    private const string ExeName = "ssimulacra2.exe";

    public static bool Is64Bit => Environment.Is64BitProcess;

    public static string? ToolDirPath =>
        Is64Bit ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ToolDirName) : null;

    public static string? ToolExePath =>
        ToolDirPath != null ? Path.Combine(ToolDirPath, ExeName) : null;

    public static bool IsSsimU2Present =>
        ToolExePath != null && File.Exists(ToolExePath);

    /// <summary>
    /// Runs ssimulacra2.exe on the two PNGs and returns the score.
    /// On success: returns (score, null).
    /// On failure: returns (null, errorMessage).
    /// </summary>
    public static async Task<(double? score, string? error)> RunScoreAsync(
        string srcPath, string distortedPath)
    {
        string? exe = ToolExePath;
        if (exe == null || !File.Exists(exe))
            return (null, "Tool not found.");

        ProcessStartInfo psi = new()
        {
            FileName = exe,
            WorkingDirectory = Path.GetDirectoryName(exe),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add(srcPath);
        psi.ArgumentList.Add(distortedPath);

        using Process process = new() { StartInfo = psi };
        process.Start();
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        if (process.ExitCode == 0 && double.TryParse(
            stdout.Trim(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double score))
        {
            return (score, null);
        }

        string msg = !string.IsNullOrWhiteSpace(stderr)
            ? TrimToolError(stderr)
            : stdout.Trim();
        return (null, msg);
    }

    private static string TrimToolError(string stderr)
    {
        // Stderr may contain multiple JXL_FAILURE lines; take the last meaningful line.
        string[] lines = stderr.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        // Find the last line that looks like "Could not load ..." or a relevant error.
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string line = lines[i].Trim();
            if (line.Contains("Could not load", StringComparison.Ordinal) ||
                line.Contains("JXL_FAILURE", StringComparison.Ordinal))
            {
                // Strip the leading path prefix like "D:/Desktop/ssimulacra2-main/..."
                int colon = line.IndexOf(": ", StringComparison.Ordinal);
                return colon > 0 ? line[(colon + 2)..] : line;
            }
        }
        // Fallback: return last non-empty line
        return lines.Length > 0 ? lines[^1].Trim() : stderr.Trim();
    }
}
