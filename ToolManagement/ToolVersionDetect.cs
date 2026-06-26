using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.ToolManagement
{
    public class ToolVersionDetectTimeoutException(string exeName)
        : TimeoutException($"Tool version detection timed out for {exeName}.")
    {
    }

    public partial class ToolVersionDetect
    {
        private static readonly TimeSpan VersionDetectTimeout = TimeSpan.FromSeconds(3);

        [GeneratedRegex(@"\bver\s+\S+", RegexOptions.IgnoreCase)]
        private static partial Regex Avs2pipemodVersion();

        [GeneratedRegex(@"version\s+(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)]
        private static partial Regex X265Version();

        private static string RemoveToolNamePrefix(string version, string toolName)
        {
            string prefix = toolName + " ";
            return version.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? version[prefix.Length..]
                : version;
        }

        public static async Task<string?> TryDetectAsync(string exeName, string filePath)
        {
            if (string.IsNullOrWhiteSpace(exeName)
                || string.IsNullOrWhiteSpace(filePath)
                || !File.Exists(filePath)) return null;

            if (exeName.Equals("avisynth.dll", StringComparison.OrdinalIgnoreCase)
                || exeName.Equals("one_line_shot_args.exe", StringComparison.OrdinalIgnoreCase))
                return TryReadProductVersion(filePath);

            string exeArgs = exeName.ToLowerInvariant() switch
            {
                "ffmpeg.exe" => "-version",
                "ffprobe.exe" => "-version",
                "vspipe.exe" => "-v",
                "x264.exe" => "-V",
                "x265.exe" => "-V",
                "svtav1encapp.exe" => "--version",
                "avs2yuv.exe" => "",
                "avs2pipemod.exe" => "",
                _ => "",
            };

            Stopwatch stopwatch = Stopwatch.StartNew();
            string exePrints = await RunAndCaptureAsync(filePath, exeArgs, GetRemainingTimeout(stopwatch), outputEncoding: GetSystemTextEncoding());
            string? version = ParseVersion(exeName, exePrints);
            if (version != null) return version;

            exePrints = await RunAndCaptureAsync(filePath, exeArgs, GetRemainingTimeout(stopwatch), useUtf8: true);
            return ParseVersion(exeName, exePrints);
        }

        // one_line_shot_args.exe only provides version in exe properties
        public static string? TryReadProductVersion(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

                if (!string.IsNullOrWhiteSpace(versionInfo.ProductVersion))
                    return versionInfo.ProductVersion.Trim();

                if (!string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                    return versionInfo.FileVersion.Trim();
            }
            catch { }
            return null;
        }

        public static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, bool useUtf8 = false, Encoding? outputEncoding = null)
        {
            return await RunAndCaptureAsync(filePath, exeArgs, TimeSpan.FromSeconds(5), false, useUtf8, outputEncoding);
        }

        public static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, TimeSpan timeout, bool useUtf8 = false, Encoding? outputEncoding = null)
        {
            return await RunAndCaptureAsync(filePath, exeArgs, timeout, true, useUtf8, outputEncoding);
        }

        private static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, TimeSpan timeout, bool throwOnTimeout, bool useUtf8 = false, Encoding? outputEncoding = null)
        {
            ProcessStartInfo psi = new()
            {
                FileName = filePath,
                Arguments = exeArgs,
                WorkingDirectory = Path.GetDirectoryName(filePath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (outputEncoding != null)
            {
                psi.StandardOutputEncoding = outputEncoding;
                psi.StandardErrorEncoding = outputEncoding;
            }
            else if (useUtf8)
            {
                psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
                psi.StandardErrorEncoding = System.Text.Encoding.UTF8;
            }

            // Execute and fetch printed text
            using Process process = new() { StartInfo = psi };
            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            using CancellationTokenSource cts = new(timeout);
            try { await process.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException)
            {
                // Maybe something else was opened (user can import any exe by ignoring warnings)
                try { if (!process.HasExited) process.Kill(true); }
                catch { }
                if (throwOnTimeout)
                    throw new ToolVersionDetectTimeoutException(Path.GetFileName(filePath));
            }

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            return string.Join(
                Environment.NewLine,
                new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private static Encoding? GetSystemTextEncoding()
        {
            try { return Console.OutputEncoding; }
            catch { return null; }
        }

        private static TimeSpan GetRemainingTimeout(Stopwatch stopwatch)
        {
            TimeSpan remaining = VersionDetectTimeout - stopwatch.Elapsed;
            if (remaining <= TimeSpan.Zero) throw new ToolVersionDetectTimeoutException("tool");
            return remaining;
        }

        public static string? ParseVersion(string exeName, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            string[] lines = text
                .Replace("\r", "")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries
                           | StringSplitOptions.TrimEntries);
            string firstLine = lines.FirstOrDefault() ?? text.Trim();

            switch (exeName.ToLowerInvariant())
            {
                case "ffmpeg.exe": // "ffmpeg version yyyy-mm-dd" to "version yyyy-mm-dd"
                    return firstLine.StartsWith("ffmpeg version", StringComparison.OrdinalIgnoreCase)
                        ? RemoveToolNamePrefix(firstLine[..Math.Min(25, firstLine.Length)], "ffmpeg")
                        : null;
                case "ffprobe.exe":
                    return firstLine.StartsWith("ffprobe version", StringComparison.OrdinalIgnoreCase)
                        ? RemoveToolNamePrefix(firstLine[..Math.Min(26, firstLine.Length)], "ffprobe")
                        : null;
                case "vspipe.exe":
                    return lines.FirstOrDefault(l =>
                        l.Contains("Core R", StringComparison.OrdinalIgnoreCase));
                case "avs2yuv.exe":
                    return text.Contains("avs2yuv", StringComparison.OrdinalIgnoreCase) ? firstLine : null;
                case "avs2pipemod.exe":
                    {
                        if (!text.Contains("avs2pipemod", StringComparison.OrdinalIgnoreCase)) return null;
                        Match m = Avs2pipemodVersion().Match(firstLine);
                        return m.Success ? m.Value : firstLine;
                    }

                case "x264.exe":
                    return text.Contains("x264", StringComparison.OrdinalIgnoreCase) ? firstLine : null;
                case "x265.exe":
                    {
                        if (!text.Contains("x265", StringComparison.OrdinalIgnoreCase)) return null;
                        Match m = X265Version().Match(text);
                        return m.Success ? m.Groups[1].Value : firstLine;
                    }
                case "svtav1encapp.exe":
                    return text.Contains("svt", StringComparison.OrdinalIgnoreCase) ? firstLine : null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Detect which --container y4m argument format vspipe.exe supports.
        /// VapourSynth API changes have caused the argument to change over time:
        ///   old: -c y4m  ->  --container y4m  ->  --y4m
        /// Tries each and returns the first that produces "No script file specified".
        /// Returns null if no format is recognized.
        /// </summary>
        public static async Task<string?> DetectVspipeY4mArgAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            string[][] testArgs =
            [
                ["-c", "y4m"],
                ["--container", "y4m"],
                ["--y4m"]
            ];

            foreach (var args in testArgs)
            {
                string argString = string.Join(" ", args);
                string output = await RunAndCaptureAsync(filePath, argString, useUtf8: true);

                if (output.Contains("No script file specified", StringComparison.OrdinalIgnoreCase))
                    return argString;
            }

            return null;
        }

        public static bool HasValidVspipeY4mArg(string? vspipePath, string? vspipeY4mArg)
        {
            return !string.IsNullOrWhiteSpace(vspipePath) &&
                   !string.IsNullOrWhiteSpace(vspipeY4mArg);
        }

        public static async Task DetectAndStoreVspipeY4mArgAsync(
            string exeName,
            string filePath,
            Action<string?> store)
        {
            if (!exeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase)) return;
            store(await DetectVspipeY4mArgAsync(filePath));
        }
    }
}
