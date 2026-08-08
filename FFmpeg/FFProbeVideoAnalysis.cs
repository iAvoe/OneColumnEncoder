using System.IO;

namespace OneColumnEncoder.FFmpeg
{
    public static class FFProbeVideoAnalysis
    {
        private static FFProbeVideoAnalysisLangProvider Lang => new(UILangProvider.Current.LanguageCode);

        public static async Task<string> AnalyzeAsync(
            string ffprobePath,
            string videoSource,
            string showEntries = "stream",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
                throw new FileNotFoundException(string.Format(Lang.FfprobeNotFound, ffprobePath));
            if (string.IsNullOrWhiteSpace(videoSource) || !File.Exists(videoSource))
                throw new FileNotFoundException(string.Format(Lang.InputVideoNotFound, videoSource));

            string[] arguments =
            [
                "-v", "quiet", "-hide_banner", "-select_streams", "v:0",
                "-show_entries", showEntries, "-show_format", "-of", "json", videoSource
            ];

            FFprobeProcessResult result;
            try
            {
                result = await FFprobeProcessRunner.RunAsync(
                    ffprobePath,
                    arguments,
                    TimeSpan.FromSeconds(30),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested) throw;
                throw new TimeoutException(Lang.FfprobeTimedOut);
            }

            if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Stdout))
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Stderr)
                    ? Lang.FfprobeFailedOrEmpty
                    : result.Stderr.Trim());

            ValidateJson(result.Stdout);
            return FFProbeJsonFormatting.Normalize(result.Stdout);
        }

        private static void ValidateJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("streams", out JsonElement streams)
                || streams.ValueKind != JsonValueKind.Array
                || streams.GetArrayLength() < 1)
                throw new InvalidOperationException(Lang.NoVideoStreamInfo);
        }
    }
}
