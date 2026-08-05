using System.Diagnostics;
using System.IO;
using System.Text;

namespace OneColumnEncoder.FFmpeg;

public static class FFmpegProcessRunner
{
    public static async Task<FFmpegProcessResult> RunAsync(
        string ffmpegPath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = CreateStartInfo(ffmpegPath, arguments);
        using Process process = new() { StartInfo = startInfo };
        process.Start();

        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch
        {
            TryKill(process);
            throw;
        }

        return new FFmpegProcessResult(await stdoutTask, await stderrTask, process.ExitCode);
    }

    public static ProcessStartInfo CreateStartInfo(string ffmpegPath, IReadOnlyList<string> arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = ffmpegPath,
            WorkingDirectory = Path.GetDirectoryName(ffmpegPath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        return startInfo;
    }

    public static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(true); }
        catch { }
    }
}

public readonly record struct FFmpegProcessResult(string Stdout, string Stderr, int ExitCode);
