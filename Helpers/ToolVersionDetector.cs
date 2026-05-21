using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace OneColumnEncoder.Helpers
{
    public class ToolVersionDetector
    {
        public static async Task<string?> TryDetectAsync(string exeName, string filePath)
        {
            if (string.IsNullOrWhiteSpace(exeName)
                || string.IsNullOrWhiteSpace(filePath)
                || !File.Exists(filePath)) return null;

            string exeArgs = exeName.ToLowerInvariant() switch
            {
                "ffmpeg.exe" => "-version",
                "vspipe.exe" => "-v",
                "x264.exe" => "-V",
                "x265.exe" => "-V",
                "svtav1encapp.exe" => "--version",
                "avs2yuv.exe" => "",
                "avs2pipemod.exe" => "",
                _ => "",
                // No need to check for for ffprobe.exe
            };

            string exePrints = await RunAndCaptureAsync(filePath, exeArgs);
            return ParseVersion(exeName, exePrints);
        }

        public static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs)
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

            // Execute and fetch printed text
            using Process process = new() { StartInfo = psi };
            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            // Timeout
            using CancellationTokenSource cts = new (TimeSpan.FromSeconds(5));
            try { await process.WaitForExitAsync(cts.Token); }
            catch {
                // Maybe something else was opened (user can import any exe by ignoring warnings)
                try { if (!process.HasExited) process.Kill(true); }
                catch {}
            }

            string stdout = await stdoutTask;
            string stderr = await stderrTask;
            return string.Join(
                Environment.NewLine,
                new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s)));
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
                case "ffmpeg.exe":
                    return firstLine[..Math.Min(25, firstLine.Length)];
                case "ffprobe.exe":
                    return firstLine;
                case "vspipe.exe":
                    return lines.FirstOrDefault(l =>
                        l.Contains("Core R", StringComparison.OrdinalIgnoreCase));
                case "avs2yuv.exe":
                    return firstLine;
                case "avs2pipemod.exe":
                    {
                        Match m = Regex.Match(firstLine, @"\bver\s+\S+", RegexOptions.IgnoreCase);
                        return m.Success ? m.Value : firstLine;
                    }

                case "x264.exe":
                    return firstLine;
                case "x265.exe":
                    {
                        Match m = Regex.Match(text, @"version\s+(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase);
                        return m.Success ? m.Groups[1].Value : firstLine;
                    }
                case "svtav1encapp.exe":
                    return firstLine;

                default:
                    return null;
            }
        }
    }
}
