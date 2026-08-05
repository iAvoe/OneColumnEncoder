using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.RepartManagement;

public enum RepartExclusionReason
{
    SourceMissing,
    ProbeFailed,
    NoVideoStream,
    NoDimensions,
    Interlaced,
    NotCfr,
    FrameCountUnavailable,
    SourceChanged,
    SignatureMismatch
}

public sealed record RepartExcludedSourceInfo(
    string FilePath,
    string DisplayName,
    RepartExclusionReason Reason,
    string? Detail);

public sealed record RepartInterlacedSourceInfo(
    string FilePath,
    string DisplayName,
    string FieldOrder);

public sealed record RepartSourceFile(
    string FilePath,
    string DisplayName,
    long InitialLength,
    long InitialWriteTicks);

public sealed record RepartSourceFileOutcome(
    RepartExclusionReason? RejectionReason,
    string? Detail,
    RepartSourceFile? SourceFile);

public sealed record RepartRawProbe(
    string RawJson,
    long InitialLength,
    long InitialWriteTicks);

public sealed record RepartRawProbeOutcome(
    RepartExclusionReason? RejectionReason,
    string? Detail,
    RepartRawProbe? Probe);

// Outcome of checks based on already-collected ffprobe data. When rejected, the
// reason tells the caller why the source was excluded so it can be reported.
public sealed record RepartProbeOutcome(
    RepartExclusionReason? RejectionReason,
    string? Detail,
    RepartSourceProbe? Probe);

// Data collected from the probe stage, needed for the expensive frame-count scan.
public sealed record RepartSourceProbe(
    string RawJson,
    int FrameRateNumerator,
    int FrameRateDenominator,
    RepartVideoFormatSignature Signature,
    long InitialLength,
    long InitialWriteTicks);

// Outcome of the expensive frame-count scan. When rejected, the source must not
// be used in the plan.
public sealed record RepartScanOutcome(
    RepartExclusionReason? RejectionReason,
    string? Detail,
    long FrameCount,
    long FileLength,
    long LastWriteUtcTicks);

// Modular per-file source checks for Repart Mode. The import pipeline runs in the
// same order everywhere: no-ffprobe filtering, simple ffprobe analyzability
// filtering, ffprobe-data analysis, analysis-based filtering, then frame scanning.
public static class RepartSourceValidator
{
    private const string ShowEntries =
        "stream=codec_name,profile,codec_tag_string,level,width,height,coded_width,coded_height," +
        "pix_fmt,bits_per_raw_sample,field_order,sample_aspect_ratio,avg_frame_rate,r_frame_rate," +
        "time_base,color_range,color_space,color_transfer,color_primaries,chroma_location," +
        "nb_frames,nb_read_frames,duration,extradata:format=duration";

    // Stage 1: filters that do not need ffprobe.
    public static RepartSourceFileOutcome CheckWithoutFfprobe(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        string displayName = Path.GetFileName(fullPath);
        if (!File.Exists(fullPath))
            return RejectedFile(RepartExclusionReason.SourceMissing);

        FileInfo file = new(fullPath);
        return new RepartSourceFileOutcome(
            null,
            null,
            new RepartSourceFile(
                fullPath,
                displayName,
                file.Length,
                file.LastWriteTimeUtc.Ticks));
    }

    // Stage 2: simple ffprobe filtering. This only proves the file can be
    // analyzed and has a video stream; rule-based exclusions happen later.
    public static async Task<RepartRawProbeOutcome> ProbeCanAnalyzeAsync(
        string ffprobePath,
        RepartSourceFile sourceFile,
        CancellationToken cancellationToken = default)
    {
        string rawJson;
        try
        {
            rawJson = await ProbeAsync(ffprobePath, sourceFile.FilePath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return RejectedRaw(RepartExclusionReason.ProbeFailed, ex.Message);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return RejectedRaw(RepartExclusionReason.NoVideoStream);

            return new RepartRawProbeOutcome(
                null,
                null,
                new RepartRawProbe(
                    rawJson,
                    sourceFile.InitialLength,
                    sourceFile.InitialWriteTicks));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return RejectedRaw(RepartExclusionReason.ProbeFailed, ex.Message);
        }
    }

    // Stage 3: analyze already-collected ffprobe data. This method does not run
    // ffprobe; it only converts raw JSON into Repart-specific facts.
    public static RepartProbeOutcome AnalyzeProbe(RepartRawProbe probe)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(probe.RawJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return Rejected(RepartExclusionReason.NoVideoStream);

            string fieldOrder = Get(stream, "field_order");
            if (!IsProgressiveFieldOrder(fieldOrder))
                return Rejected(RepartExclusionReason.Interlaced, fieldOrder);

            if (GetInt(stream, "width") <= 0 || GetInt(stream, "height") <= 0)
                return Rejected(RepartExclusionReason.NoDimensions);

            if (!TryResolveCfrFrameRate(stream, out (int num, int den) frameRate))
                return Rejected(RepartExclusionReason.NotCfr);

            RepartVideoFormatSignature signature = BuildSignature(stream, frameRate.num, frameRate.den);

            return new RepartProbeOutcome(
                null,
                null,
                new RepartSourceProbe(
                    probe.RawJson,
                    frameRate.num,
                    frameRate.den,
                    signature,
                    probe.InitialLength,
                    probe.InitialWriteTicks));
        }
        catch (Exception ex)
        {
            return Rejected(RepartExclusionReason.ProbeFailed, ex.Message);
        }
    }

    // Stage 2: expensive full-file frame-count scan plus file-stability check.
    // Only call this for files that passed stage 1 AND matched the reference.
    public static async Task<RepartScanOutcome> ScanFramesAsync(
        string ffprobePath,
        string filePath,
        RepartSourceProbe probe,
        CancellationToken cancellationToken = default)
    {
        long frameCount;
        try
        {
            frameCount = await CountFramesAsync(ffprobePath, filePath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return RejectedScan(RepartExclusionReason.FrameCountUnavailable);
        }
        if (frameCount <= 0)
            return RejectedScan(RepartExclusionReason.FrameCountUnavailable);

        FileInfo file = new(filePath);
        file.Refresh();
        if (file.Length != probe.InitialLength || file.LastWriteTimeUtc.Ticks != probe.InitialWriteTicks)
            return RejectedScan(RepartExclusionReason.SourceChanged);

        return new RepartScanOutcome(null, null, frameCount, file.Length, file.LastWriteTimeUtc.Ticks);
    }

    private static RepartProbeOutcome Rejected(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, null);

    private static RepartSourceFileOutcome RejectedFile(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, null);

    private static RepartRawProbeOutcome RejectedRaw(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, null);

    private static RepartScanOutcome RejectedScan(RepartExclusionReason reason) =>
        new(reason, null, 0, 0, 0);

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

    private static async Task<long> CountFramesAsync(
        string ffprobePath,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-v", "error", "-hide_banner", "-count_frames", "-select_streams", "v:0",
            "-show_entries", "stream=nb_read_frames,nb_frames", "-of", "json", sourcePath
        ];

        FFprobeProcessResult result = await FFprobeProcessRunner.RunAsync(
            ffprobePath,
            arguments,
            TimeSpan.FromMinutes(30),
            cancellationToken);
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Stdout))
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Stderr)
                ? RepartLangProvider.Current.ProbeFailed
                : result.Stderr.Trim());

        using JsonDocument document = JsonDocument.Parse(result.Stdout);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)) return 0;
        long? readFrames = TryGetLong(stream, "nb_read_frames");
        if (readFrames is > 0) return readFrames.Value;
        return TryGetFrameCount(stream) ?? 0;
    }

    private static bool TryResolveCfrFrameRate(JsonElement stream, out (int num, int den) frameRate)
    {
        frameRate = default;
        (int num, int den)? averageRate = FrameRate.GetAvgFrameRate(stream);
        (int num, int den)? realRate = FrameRate.GetRFrameRate(stream);

        if (averageRate != null && realRate != null && !SameRate(averageRate.Value, realRate.Value))
            return false;

        (int num, int den)? selected = averageRate ?? realRate;
        if (selected == null) return false;
        frameRate = selected.Value;
        return true;
    }

    private static bool SameRate((int num, int den) left, (int num, int den) right) =>
        (long)left.num * right.den == (long)right.num * left.den;

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
}

// Formats an exclusion into either a bare reason line or a full per-source dialog
// message (source path + reason + will-exclude notice).
public static class RepartExclusionMessages
{
    public static string FormatReason(RepartExcludedSourceInfo info)
    {
        RepartLangProvider lang = RepartLangProvider.Current;
        return info.Reason switch
        {
            RepartExclusionReason.SourceMissing => string.Format(lang.SourceMissing, info.FilePath),
            RepartExclusionReason.ProbeFailed => string.IsNullOrWhiteSpace(info.Detail)
                ? lang.ProbeFailed
                : info.Detail.Trim(),
            RepartExclusionReason.NoVideoStream => string.Format(lang.NoVideoStream, info.DisplayName),
            RepartExclusionReason.NoDimensions => string.Format(lang["NoDimensions"], info.DisplayName),
            RepartExclusionReason.Interlaced => string.Format(
                lang["InterlacedSourceRejected"],
                info.DisplayName,
                info.Detail ?? string.Empty),
            RepartExclusionReason.NotCfr => string.Format(lang.CfrRequired, info.DisplayName),
            RepartExclusionReason.FrameCountUnavailable => string.Format(lang.FrameCountRequired, info.DisplayName),
            RepartExclusionReason.SourceChanged => string.Format(lang.SourceChangedDuringAnalysis, info.FilePath),
            RepartExclusionReason.SignatureMismatch => string.Format(lang["SignatureMismatch"], info.DisplayName),
            _ => info.DisplayName
        };
    }

    public static string FormatExcludedMessage(RepartExcludedSourceInfo info) =>
        string.Join(
            Environment.NewLine,
            string.Format(RepartLangProvider.Current["SourceLabel"], info.FilePath),
            FormatReason(info),
            string.Empty,
            RepartLangProvider.Current["WillExcludeSource"]);
}

// Shared confirm prompt for interlaced sources, used by every Repart Mode import
// entry point (pre-open import and in-window re-import).
public static class RepartInterlacedPrompt
{
    public static bool Confirm(ModalNavS modalNavS, string windowTitle, RepartInterlacedSourceInfo source)
    {
        OpenWarnModalCmd cmd = new(
            modalNavS,
            windowTitle,
            string.Format(
                RepartLangProvider.Current["InterlacedSourcePrompt"],
                source.DisplayName,
                source.FieldOrder));
        cmd.Execute(null);
        return cmd.DialogResult == true;
    }
}
