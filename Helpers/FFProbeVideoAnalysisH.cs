using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OneColumnEncoder.Helpers
{
    public static class FFProbeVideoAnalysisH
    {
        public static async Task<string> AnalyzeAsync(string ffprobePath, string videoSource, string showEntries = "stream")
        {
            if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
                throw new FileNotFoundException($"ffprobe.exe does not exist: {ffprobePath}");
            if (string.IsNullOrWhiteSpace(videoSource) || !File.Exists(videoSource))
                throw new FileNotFoundException($"Input video does not exist: {videoSource}");

            ProcessStartInfo psi = new()
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

            psi.ArgumentList.Add("-v");
            psi.ArgumentList.Add("quiet");
            psi.ArgumentList.Add("-hide_banner");
            psi.ArgumentList.Add("-select_streams");
            psi.ArgumentList.Add("v:0");
            psi.ArgumentList.Add("-show_entries");
            psi.ArgumentList.Add(showEntries);
            psi.ArgumentList.Add("-show_format");
            psi.ArgumentList.Add("-of");
            psi.ArgumentList.Add("json");
            psi.ArgumentList.Add(videoSource);

            using Process process = new() { StartInfo = psi };
            process.Start();

            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            try { await process.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException)
            {
                try { if (!process.HasExited) process.Kill(true); }
                catch { }
                throw new TimeoutException("ffprobe timed out while analyzing the source video.");
            }

            string json = await stdoutTask;
            string stderr = await stderrTask;

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr)
                    ? "ffprobe failed or returned no valid data."
                    : stderr.Trim());

            ValidateJson(json);
            return FFProbeJsonFormattingH.Normalize(json);
        }

        private static void ValidateJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("streams", out JsonElement streams)
                || streams.ValueKind != JsonValueKind.Array
                || streams.GetArrayLength() < 1)
                throw new InvalidOperationException("ffprobe returned no video stream information.");
        }
    }
}
