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
    public class ToolVersionDetectH
    {
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
            catch {}
            return null;
        }

        public static async Task<string> RunAndCaptureAsync(string filePath, string exeArgs, bool useUtf8 = false)
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

            if (useUtf8)
            {
                psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
                psi.StandardErrorEncoding = System.Text.Encoding.UTF8;
            }

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
    }
}
