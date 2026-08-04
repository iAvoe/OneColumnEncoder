using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.RepartManagement;

public static class RepartCompatibilityAnalyzer
{
    private const string ShowEntries =
        "stream=codec_name,profile,codec_tag_string,level,width,height,coded_width,coded_height," +
        "pix_fmt,bits_per_raw_sample,field_order,sample_aspect_ratio,avg_frame_rate,r_frame_rate," +
        "time_base,color_range,color_space,color_transfer,color_primaries,chroma_location," +
        "nb_frames,nb_read_frames,duration,extradata:format=duration";

    public static async Task<RepartPlanM> AnalyzeAsync(
        string ffprobePath,
        IReadOnlyList<string> filePaths,
        Func<RepartInterlacedSourceInfo, bool>? confirmDiscardInterlacedSource = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffprobePath) || !File.Exists(ffprobePath))
            throw new FileNotFoundException(RepartLangProvider.Current.FfprobeRequired, ffprobePath);
        if (filePaths.Count == 0)
            throw new InvalidOperationException(RepartLangProvider.Current.SourceRequired);

        List<RepartSourceM> sources = [];
        RepartVideoFormatSignature? referenceSignature = null;
        string referenceJson = string.Empty;
        int frameRateNumerator = 0;
        int frameRateDenominator = 0;
        long cumulativeFrames = 0;
        Exception? firstSourceFailure = null;

        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = Path.GetFullPath(filePaths[i]);
            SourceAnalysisResult source;
            try
            {
                source = await AnalyzeSourceAsync(ffprobePath, path, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InterlacedSourceException ex)
            {
                bool shouldDiscard = confirmDiscardInterlacedSource?.Invoke(new(
                    path,
                    Path.GetFileName(path),
                    ex.FieldOrder)) == true;
                if (shouldDiscard) continue;

                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            catch (Exception ex) when (filePaths.Count > 1)
            {
                firstSourceFailure ??= ex;
                continue;
            }

            if (referenceSignature == null)
            {
                referenceSignature = source.Signature;
                referenceJson = source.RawJson;
                frameRateNumerator = source.FrameRateNumerator;
                frameRateDenominator = source.FrameRateDenominator;
            }
            else if (referenceSignature != source.Signature)
            {
                continue;
            }

            long firstFrame = cumulativeFrames;
            checked { cumulativeFrames += source.FrameCount; }
            sources.Add(new RepartSourceM(
                path,
                source.RawJson,
                source.FrameCount,
                firstFrame,
                cumulativeFrames - 1,
                source.FileLength,
                source.LastWriteUtcTicks));
        }

        if (sources.Count == 0)
        {
            if (firstSourceFailure != null)
                throw new InvalidOperationException(firstSourceFailure.Message, firstSourceFailure);
            throw new InvalidOperationException(RepartLangProvider.Current.SourceRequired);
        }

        return new RepartPlanM
        {
            FfprobePath = ffprobePath,
            ReferenceRawJson = referenceJson,
            FormatSignature = referenceSignature,
            FrameRateNumerator = frameRateNumerator,
            FrameRateDenominator = frameRateDenominator,
            TotalFrames = cumulativeFrames,
            Sources = sources
        };
    }

    private static async Task<SourceAnalysisResult> AnalyzeSourceAsync(
        string ffprobePath,
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(string.Format(RepartLangProvider.Current.SourceMissing, path), path);
        FileInfo beforeAnalysis = new(path);
        long initialLength = beforeAnalysis.Length;
        long initialWriteTicks = beforeAnalysis.LastWriteTimeUtc.Ticks;

        string rawJson = await ProbeAsync(ffprobePath, path, cancellationToken);
        using JsonDocument document = JsonDocument.Parse(rawJson);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
            throw new InvalidOperationException(string.Format(RepartLangProvider.Current.NoVideoStream, Path.GetFileName(path)));

        string fieldOrder = Get(stream, "field_order");
        if (!IsProgressiveFieldOrder(fieldOrder))
            throw new InterlacedSourceException(path, fieldOrder);

        (int num, int den)? averageRate = FrameRate.GetAvgFrameRate(stream);
        (int num, int den)? realRate = FrameRate.GetRFrameRate(stream);
        // Collect the authoritative frame-rate declarations to validate as CFR candidates.
        List<(int num, int den)> candidateRates = [];
        if (averageRate != null) candidateRates.Add(averageRate.Value);
        if (realRate != null && !candidateRates.Contains(realRate.Value)) candidateRates.Add(realRate.Value);
        if (candidateRates.Count == 0)
            throw new InvalidOperationException(string.Format(RepartLangProvider.Current.CfrRequired, Path.GetFileName(path)));

        FrameScanResult frameScan = await ScanFramesAsync(
            ffprobePath,
            path,
            candidateRates,
            FrameRate.ParseFraction(Get(stream, "time_base")),
            cancellationToken);
        if (!frameScan.IsConstant || frameScan.FrameCount <= 0)
            throw new InvalidOperationException(string.Format(RepartLangProvider.Current.CfrRequired, Path.GetFileName(path)));
        long frameCount = frameScan.FrameCount;
        if (frameCount <= 0)
            throw new InvalidOperationException(string.Format(RepartLangProvider.Current.FrameCountRequired, Path.GetFileName(path)));

        RepartVideoFormatSignature signature = BuildSignature(stream, frameScan.FrameRateNumerator, frameScan.FrameRateDenominator);
        FileInfo file = new(path);
        file.Refresh();
        if (file.Length != initialLength || file.LastWriteTimeUtc.Ticks != initialWriteTicks)
            throw new IOException(string.Format(RepartLangProvider.Current.SourceChangedDuringAnalysis, path));

        return new SourceAnalysisResult(
            rawJson,
            frameCount,
            file.Length,
            file.LastWriteTimeUtc.Ticks,
            frameScan.FrameRateNumerator,
            frameScan.FrameRateDenominator,
            signature);
    }

    private static async Task<string> ProbeAsync(
        string ffprobePath,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-v", "error", "-hide_banner", "-select_streams", "v:0",
            "-show_data", "-show_entries", ShowEntries, "-of", "json", sourcePath
        ];

        FFprobeProcessResult result = await FFprobeProcessRunner.RunAsync(
            ffprobePath,
            arguments,
            TimeSpan.FromSeconds(60),
            cancellationToken);

        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Stdout))
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Stderr)
                ? RepartLangProvider.Current.ProbeFailed
                : result.Stderr.Trim());
        return result.Stdout;
    }

    private static async Task<FrameScanResult> ScanFramesAsync(
        string ffprobePath,
        string sourcePath,
        IReadOnlyList<(int num, int den)> candidateRates,
        (int num, int den)? timeBase,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-v", "error", "-hide_banner", "-select_streams", "v:0",
            "-show_entries", "frame=best_effort_timestamp_time", "-of", "csv=p=0", sourcePath
        ];

        ProcessStartInfo startInfo = FFprobeProcessRunner.CreateStartInfo(ffprobePath, arguments);
        using Process process = new() { StartInfo = startInfo };
        process.Start();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        double? firstTimestamp = null;
        long frameCount = 0;
        bool[] constantCandidates = Enumerable.Repeat(true, candidateRates.Count).ToArray();
        bool rejectedAsNonConstant = false;
        double timeBaseTick = timeBase is { den: > 0 }
            ? (double)timeBase.Value.num / timeBase.Value.den
            : 0d;
        double tolerance = Math.Max(0.000001d, timeBaseTick * 1.5d);

        try
        {
            while (await process.StandardOutput.ReadLineAsync(cancellationToken) is string line)
            {
                string timestampText = line.Split(',')[0].Trim();
                if (!double.TryParse(timestampText, NumberStyles.Float, CultureInfo.InvariantCulture, out double timestamp))
                {
                    Array.Fill(constantCandidates, false);
                    continue;
                }

                firstTimestamp ??= timestamp;
                // For each candidate rate, verify the observed timestamp matches the
                // expected position assuming a constant frame interval. A mismatch on any
                // frame disqualifies that rate (i.e. the source is not CFR at it).
                for (int i = 0; i < candidateRates.Count; i++)
                {
                    (int num, int den) = candidateRates[i];
                    double expected = firstTimestamp.Value + (double)frameCount * den / num;
                    if (Math.Abs(timestamp - expected) > tolerance) constantCandidates[i] = false;
                }
                frameCount++;
                if (constantCandidates.All(value => !value))
                {
                    rejectedAsNonConstant = true;
                    FFprobeProcessRunner.TryKill(process);
                    break;
                }
            }
            await process.WaitForExitAsync(cancellationToken);
        }
        catch
        {
            FFprobeProcessRunner.TryKill(process);
            throw;
        }
        string stderr = await stderrTask;
        if (!rejectedAsNonConstant && process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(stderr) ? RepartLangProvider.Current.ProbeFailed : stderr.Trim());
        int selectedRateIndex = Array.FindIndex(constantCandidates, value => value);
        (int num, int den) selectedRate = selectedRateIndex >= 0
            ? candidateRates[selectedRateIndex]
            : candidateRates[0];
        return new FrameScanResult(frameCount, selectedRateIndex >= 0, selectedRate.num, selectedRate.den);
    }

    private static RepartVideoFormatSignature BuildSignature(JsonElement stream, int frameRateNumerator, int frameRateDenominator)
    {
        string normalizedRate = FrameRate.NormalizeFrameRate($"{frameRateNumerator}/{frameRateDenominator}");
        return new(
        Get(stream, "codec_name"),
        Get(stream, "profile"),
        Get(stream, "codec_tag_string"),
        Get(stream, "level"),
        GetInt(stream, "width"),
        GetInt(stream, "height"),
        GetInt(stream, "coded_width"),
        GetInt(stream, "coded_height"),
        Get(stream, "pix_fmt"),
        Get(stream, "bits_per_raw_sample"),
        Get(stream, "field_order"),
        Get(stream, "sample_aspect_ratio"),
        normalizedRate,
        normalizedRate,
        Get(stream, "time_base"),
        Get(stream, "color_range"),
        Get(stream, "color_space"),
        Get(stream, "color_transfer"),
        Get(stream, "color_primaries"),
        Get(stream, "chroma_location"),
        Hash(Get(stream, "extradata")));
    }

    private static string Get(JsonElement element, string propertyName) =>
        (TryGetString(element, propertyName) ?? string.Empty).Trim().ToLowerInvariant();

    private static int GetInt(JsonElement element, string propertyName) =>
        TryGetInt(element, propertyName, out int value) ? value : 0;

    private static bool IsProgressiveFieldOrder(string fieldOrder) =>
        string.IsNullOrWhiteSpace(fieldOrder)
        || fieldOrder.Equals("progressive", StringComparison.OrdinalIgnoreCase)
        || fieldOrder.Equals("unknown", StringComparison.OrdinalIgnoreCase);

    private static string Hash(string value) => string.IsNullOrWhiteSpace(value)
        ? string.Empty
        : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private readonly record struct FrameScanResult(
        long FrameCount,
        bool IsConstant,
        int FrameRateNumerator,
        int FrameRateDenominator);

    private readonly record struct SourceAnalysisResult(
        string RawJson,
        long FrameCount,
        long FileLength,
        long LastWriteUtcTicks,
        int FrameRateNumerator,
        int FrameRateDenominator,
        RepartVideoFormatSignature Signature);

    private sealed class InterlacedSourceException(string filePath, string fieldOrder)
        : InvalidOperationException(string.Format(
            RepartLangProvider.Current["InterlacedSourceRejected"],
            Path.GetFileName(filePath),
            fieldOrder))
    {
        public string FieldOrder { get; } = fieldOrder;
    }
}

public sealed record RepartInterlacedSourceInfo(
    string FilePath,
    string DisplayName,
    string FieldOrder);
