using System.IO;

namespace OneColumnEncoder.FFmpeg;

public static class FFprobeProcessRunner
{
    public static async Task<FFprobeProcessResult> RunAsync(
        string ffprobePath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = CreateStartInfo(ffprobePath, arguments);
        using Process process = new() { StartInfo = startInfo };
        using CancellationTokenRegistration killRegistration = cancellationToken.Register(() => TryKill(process));
        try
        {
            process.Start();

            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            await process.WaitForExitAsync(timeoutSource.Token);

            return new FFprobeProcessResult(await stdoutTask, await stderrTask, process.ExitCode);
        }
        catch
        {
            TryKill(process);
            throw;
        }
    }

    public static ProcessStartInfo CreateStartInfo(string ffprobePath, IReadOnlyList<string> arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = ffprobePath,
            WorkingDirectory = Path.GetDirectoryName(ffprobePath),
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

public readonly record struct FFprobeProcessResult(string Stdout, string Stderr, int ExitCode);
