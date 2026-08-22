using System.IO;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Starts ffmpeg and captures either text output or piped binary frames
/// </summary>
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
        bool started = false;
        using CancellationTokenRegistration killRegistration = cancellationToken.Register(() =>
        {
            if (started) TryKill(process);
        });
        try
        {
            started = process.Start();

            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            await process.WaitForExitAsync(timeoutSource.Token);

            return new FFmpegProcessResult(await stdoutTask, await stderrTask, process.ExitCode);
        }
        catch
        {
            if (started) TryKill(process);
            throw;
        }
    }

    /// <summary>Runs ffmpeg while buffering binary stdout (e.g. piped image frames) into memory.</summary>
    public static async Task<FFmpegProcessResultWithOutput> RunAsyncWithOutput(
        string ffmpegPath,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = CreateStartInfo(ffmpegPath, arguments);
        using Process process = new() { StartInfo = startInfo };
        bool started = false;
        using CancellationTokenRegistration killRegistration = cancellationToken.Register(() =>
        {
            if (started) TryKill(process);
        });
        try
        {
            started = process.Start();

            Task<MemoryStream> stdoutTask = ReadOutputToMemoryAsync(process.StandardOutput.BaseStream, cancellationToken);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            await process.WaitForExitAsync(timeoutSource.Token);

            return new FFmpegProcessResultWithOutput(await stdoutTask, await stderrTask, process.ExitCode);
        }
        catch
        {
            if (started) TryKill(process);
            throw;
        }
    }

    private static async Task<MemoryStream> ReadOutputToMemoryAsync(Stream stream, CancellationToken cancellationToken)
    {
        MemoryStream memory = new();
        try
        {
            byte[] buffer = new byte[1 << 16];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read <= 0) break;
                await memory.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
            return memory;
        }
        catch
        {
            memory.Dispose();
            throw;
        }
    }

    public static ProcessStartInfo CreateStartInfo(string ffmpegPath, IReadOnlyList<string> arguments)
    {
        string executablePath = Path.GetFullPath(ffmpegPath);
        string? workingDirectory = Path.GetDirectoryName(executablePath);
        ProcessStartInfo startInfo = new()
        {
            FileName = executablePath,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? Environment.CurrentDirectory : workingDirectory,
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

/// <summary>Captures ffmpeg stdout, stderr, and exit status.</summary>
/// <param name="Stdout">Standard output text.</param>
/// <param name="Stderr">Standard error text.</param>
/// <param name="ExitCode">Process exit code.</param>
public readonly record struct FFmpegProcessResult(string Stdout, string Stderr, int ExitCode);

/// <summary>Captures ffmpeg binary stdout, stderr, and exit status.</summary>
/// <param name="Output">Buffered standard output stream.</param>
/// <param name="Stderr">Standard error text.</param>
/// <param name="ExitCode">Process exit code.</param>
public readonly record struct FFmpegProcessResultWithOutput(MemoryStream Output, string Stderr, int ExitCode);
