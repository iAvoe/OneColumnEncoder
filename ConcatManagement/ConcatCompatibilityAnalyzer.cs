using System.Text.Json;
using OneColumnEncoder.Json;

namespace OneColumnEncoder.ConcatManagement
{
    public static class ConcatCompatibilityAnalyzer
    {
        public static bool AnalyzeAllFiles(string[] filePaths, string? ffprobePath, out string resultJson)
        {
            var results = new System.Text.StringBuilder();
            results.AppendLine("[");

            bool allValid = true;
            int? firstWidth = null, firstHeight = null;
            string? firstPixFmt = null;

            for (int i = 0; i < filePaths.Length; i++)
            {
                string path = filePaths[i];
                string? json = RunFfprobeGetJson(ffprobePath, path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    allValid = false;
                    results.AppendLine($"  {{ \"index\": {i}, \"path\": \"{EscapeJson(path)}\", \"error\": \"ffprobe failed\" }},");
                    continue;
                }

                int width = 0, height = 0;
                string? codec = null, pixFmt = null, fps = null;
                bool hasVideo = TryParseVideoStreamInfo(json, out width, out height, out codec, out pixFmt, out fps);

                if (!hasVideo)
                {
                    allValid = false;
                    results.AppendLine($"  {{ \"index\": {i}, \"path\": \"{EscapeJson(path)}\", \"error\": \"no video stream\" }},");
                    continue;
                }

                if (firstWidth == null)
                {
                    firstWidth = width;
                    firstHeight = height;
                    firstPixFmt = pixFmt;
                }
                else if (firstWidth != width || firstHeight != height || firstPixFmt != pixFmt)
                {
                    allValid = false;
                    results.AppendLine($"  {{ \"index\": {i}, \"path\": \"{EscapeJson(path)}\", \"error\": \"incompatible video params\", \"details\": {{ \"width\": {width}, \"height\": {height}, \"codec\": \"{codec}\", \"pix_fmt\": \"{pixFmt}\" }} }},");
                }

                results.AppendLine($"  {{ \"index\": {i}, \"path\": \"{EscapeJson(path)}\", \"width\": {width}, \"height\": {height}, \"codec\": \"{codec}\", \"pix_fmt\": \"{pixFmt}\", \"fps\": \"{fps}\" }},");
            }

            results.AppendLine("]");
            resultJson = results.ToString();
            return allValid;
        }

        private static string? RunFfprobeGetJson(string? ffprobePath, string videoPath)
        {
            if (string.IsNullOrWhiteSpace(ffprobePath) || !System.IO.File.Exists(ffprobePath))
                return null;
            if (string.IsNullOrWhiteSpace(videoPath) || !System.IO.File.Exists(videoPath))
                return null;

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ffprobePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("-v");
                psi.ArgumentList.Add("quiet");
                psi.ArgumentList.Add("-hide_banner");
                psi.ArgumentList.Add("-select_streams");
                psi.ArgumentList.Add("v:0");
                psi.ArgumentList.Add("-show_entries");
                psi.ArgumentList.Add("stream=index,width,height,codec_name,pix_fmt,avg_frame_rate,r_frame_rate");
                psi.ArgumentList.Add("-of");
                psi.ArgumentList.Add("json");
                psi.ArgumentList.Add(videoPath);

                using var process = new System.Diagnostics.Process { StartInfo = psi };
                process.Start();
                string stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);
                return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(stdout) ? stdout : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryParseVideoStreamInfo(string json, out int width, out int height,
            out string? codec, out string? pixFmt, out string? fps)
        {
            width = height = 0;
            codec = pixFmt = fps = null;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("streams", out JsonElement streams) ||
                    streams.ValueKind != JsonValueKind.Array)
                    return false;

                foreach (JsonElement stream in streams.EnumerateArray())
                {
                    string? codecType = JsonElementHelper.TryGetString(stream, "codec_type");
                    if (codecType is null || codecType.Equals("video", System.StringComparison.OrdinalIgnoreCase))
                    {
                        width = JsonElementHelper.TryGetInt(stream, "width", out int w) ? w : 0;
                        height = JsonElementHelper.TryGetInt(stream, "height", out int h) ? h : 0;
                        codec = JsonElementHelper.TryGetString(stream, "codec_name");
                        pixFmt = JsonElementHelper.TryGetString(stream, "pix_fmt");

                        string? avgFps = JsonElementHelper.TryGetString(stream, "avg_frame_rate");
                        string? rFps = JsonElementHelper.TryGetString(stream, "r_frame_rate");
                        fps = !string.IsNullOrWhiteSpace(avgFps) && avgFps != "0/0" ? avgFps : rFps;

                        return width > 0 && height > 0;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string EscapeJson(string value) =>
            value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
