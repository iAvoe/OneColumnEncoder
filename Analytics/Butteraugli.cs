using System.IO;

namespace OneColumnEncoder.Analytics;

public static class Butteraugli
{
    private const string ToolDirName = "x64-GoogleButteraugli";
    private const string ExeName = "butteraugli.exe";

    public static bool Is64Bit => Environment.Is64BitProcess;

    public static string? ToolDirPath =>
        Is64Bit ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ToolDirName) : null;

    public static string? ToolExePath =>
        ToolDirPath != null ? Path.Combine(ToolDirPath, ExeName) : null;

    public static bool IsPresent =>
        ToolExePath != null && File.Exists(ToolExePath);

    public static async Task<(double? score, string? error)> RunScoreAsync(
        string sourcePath, string distortedPath)
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
        psi.ArgumentList.Add(sourcePath);
        psi.ArgumentList.Add(distortedPath);

        using Process process = new() { StartInfo = psi };
        process.Start();
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string stdout = await stdoutTask;
        string stderr = await stderrTask;
        string output = string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;

        if (process.ExitCode == 0 && double.TryParse(
            output.Trim(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double score))
        {
            return (score, null);
        }

        return (null, output.Trim());
    }
}
