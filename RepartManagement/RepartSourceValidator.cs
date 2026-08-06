using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.FFmpeg;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

// Info passed to the frame-count fallback prompt. Raised when no exact frame
// count could be obtained (no ffmpeg) and the duration-based estimate failed to
// verify, so the user can decide whether to keep searching.
public sealed record RepartFrameCountFallbackInfo(
    string FilePath,
    string DisplayName,
    long EstimatedCount);

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
    long InitialWriteTicks,
    double? DurationSeconds,
    double StartTimeSeconds,
    long? FrameCount);

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
        "nb_frames,nb_read_frames,duration,start_time,extradata:format=duration,start_time";
    private const int MaxMetadataProbeAdjustmentFrames = 300;

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

    // Stage 2: simple ffprobe filtering. This mirrors queue-mode behavior: run a
    // short metadata-only probe and exclude sources ffprobe cannot analyze before
    // the heavier Repart-specific ffprobe analysis is attempted.
    public static async Task<RepartSourceFileOutcome> ProbeCanAnalyzeAsync(
        string ffprobePath,
        RepartSourceFile sourceFile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await FFProbeVideoAnalysis.AnalyzeAsync(
                ffprobePath,
                sourceFile.FilePath,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return RejectedFile(RepartExclusionReason.ProbeFailed, ex.Message);
        }

        return new RepartSourceFileOutcome(null, null, sourceFile);
    }

    // Stage 3: Repart-specific ffprobe analysis. Only sources that passed the
    // simple probe can reach this point.
    public static async Task<RepartRawProbeOutcome> AnalyzeWithFfprobeAsync(
        string ffprobePath,
        RepartSourceFile sourceFile,
        CancellationToken cancellationToken = default)
    {
        string rawJson;
        try
        {
            rawJson = await ProbeAsync(ffprobePath, sourceFile.FilePath, cancellationToken);
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

    // Stage 4 helper: analyze already-collected ffprobe data. This method does
    // not run ffprobe; it only converts raw JSON into Repart-specific facts used
    // by the analysis-based filters.
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
                    probe.InitialWriteTicks,
                    TryGetDurationSeconds(document.RootElement, stream),
                    TryGetStartTimeSeconds(document.RootElement, stream),
                    TryGetFrameCount(stream)));
        }
        catch (Exception ex)
        {
            return Rejected(RepartExclusionReason.ProbeFailed, ex.Message);
        }
    }

    // Stage 2: frame-count acquisition plus file-stability check.
    // Prefer cached nb_frames, then duration-based estimation verified by seek-probing,
    // with ffmpeg's exact null remux as a last resort.
    public static async Task<RepartScanOutcome> ScanFramesAsync(
        string ffprobePath,
        string? ffmpegPath,
        string filePath,
        RepartSourceProbe probe,
        string displayName,
        Func<RepartFrameCountFallbackInfo, bool>? confirmExpandFrameCountSearch = null,
        CancellationToken cancellationToken = default)
    {
        long frameCount;
        try
        {
            frameCount = probe.FrameCount is > 0
                ? probe.FrameCount.Value
                : await CountFramesAsync(
                    ffprobePath,
                    ffmpegPath,
                    probe,
                    filePath,
                    displayName,
                    confirmExpandFrameCountSearch,
                    cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return RejectedScan(RepartExclusionReason.FrameCountUnavailable, ex.Message);
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

    private static RepartScanOutcome RejectedScan(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, 0, 0, 0);

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
        string? ffmpegPath,
        RepartSourceProbe probe,
        string sourcePath,
        string displayName,
        Func<RepartFrameCountFallbackInfo, bool>? confirmExpandFrameCountSearch,
        CancellationToken cancellationToken)
    {
        long? estimatedCount = EstimateFrameCount(probe.DurationSeconds, probe.FrameRateNumerator, probe.FrameRateDenominator);

        if (estimatedCount is > 0)
        {
            long? metadataCount = await TryResolveEstimatedFrameCountWithFfprobeAsync(
                ffprobePath,
                sourcePath,
                probe,
                estimatedCount.Value,
                cancellationToken);
            if (metadataCount is > 0)
                return metadataCount.Value;
        }

        bool ffmpegAvailable = !string.IsNullOrWhiteSpace(ffmpegPath) && File.Exists(ffmpegPath);
        if (ffmpegAvailable)
        {
            try
            {
                long? exactCount = await CountFramesWithFfmpegAsync(ffmpegPath!, sourcePath, cancellationToken);
                if (exactCount is > 0)
                    return exactCount.Value;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
            }
        }

        // No exact count (no ffmpeg) and the duration-based estimate could not be
        // verified. Ask the user whether to run an extended search that expands the
        // frame-count range by 10x per step to locate the real boundary, or to
        // cancel the import. Without a prompt callback the raw estimate is kept.
        if (estimatedCount is > 0 && !ffmpegAvailable && confirmExpandFrameCountSearch != null)
        {
            bool retry = confirmExpandFrameCountSearch(new(
                sourcePath,
                displayName,
                estimatedCount.Value));
            if (!retry)
                throw new OperationCanceledException(
                    string.Format(
                        RepartLangProvider.Current["FrameCountFallbackCancelled"],
                        displayName),
                    cancellationToken);

            long? expandedCount = await SearchFrameCountWithExpansionAsync(
                ffprobePath,
                sourcePath,
                probe,
                estimatedCount.Value,
                cancellationToken);
            if (expandedCount is > 0)
                return expandedCount.Value;
        }

        return estimatedCount is > 0 ? estimatedCount.Value : 0;
    }

    // Brackets the real frame count by probing at frame indices that grow by 10x
    // each step, then narrows the boundary with a binary search. Used only when
    // ffmpeg is unavailable and the estimate-based probe walk (±300 frames) could
    // not find the end of the video.
    private static async Task<long?> SearchFrameCountWithExpansionAsync(
        string ffprobePath,
        string sourcePath,
        RepartSourceProbe probe,
        long estimatedCount,
        CancellationToken cancellationToken)
    {
        if (estimatedCount <= 0
            || probe.FrameRateNumerator <= 0
            || probe.FrameRateDenominator <= 0)
            return null;

        bool? estimateExists = await ProbeFrameExistsAsync(
            ffprobePath,
            sourcePath,
            probe,
            estimatedCount,
            cancellationToken);
        if (estimateExists == null)
            return null;

        long lo;
        long hi;
        if (estimateExists.Value)
        {
            // Real count is above the estimate: expand upward by x10 until a probe
            // past the end brackets the true boundary. lo stays an existing index,
            // hi becomes the first non-existing one.
            lo = estimatedCount;
            hi = estimatedCount;
            while (true)
            {
                if (hi > long.MaxValue / 10)
                    return null;
                hi *= 10;
                bool? exists = await ProbeFrameExistsAsync(ffprobePath, sourcePath, probe, hi, cancellationToken);
                if (exists == null)
                    return null;
                if (!exists.Value)
                    break;
                lo = hi;
            }
        }
        else
        {
            // Real count is at or below the estimate: expand downward by x10 until
            // a probe inside the video brackets the true boundary.
            hi = estimatedCount;
            lo = Math.Max(0, hi / 10);
            while (lo > 0)
            {
                bool? exists = await ProbeFrameExistsAsync(ffprobePath, sourcePath, probe, lo, cancellationToken);
                if (exists == null)
                    return null;
                if (exists.Value)
                    break;
                hi = lo;
                lo = Math.Max(0, hi / 10);
            }
            if (lo <= 0)
            {
                bool? first = await ProbeFrameExistsAsync(ffprobePath, sourcePath, probe, 0, cancellationToken);
                if (first == null)
                    return null;
                if (!first.Value)
                    return 0L;
                lo = 0;
            }
        }

        // lo is an existing frame index, hi is a non-existing one; binary search
        // the boundary. The frame count is the last existing index plus one.
        while (hi - lo > 1)
        {
            long mid = lo + (hi - lo) / 2;
            bool? exists = await ProbeFrameExistsAsync(ffprobePath, sourcePath, probe, mid, cancellationToken);
            if (exists == null)
                return null;
            if (exists.Value)
                lo = mid;
            else
                hi = mid;
        }

        return lo + 1;
    }

    private static async Task<long?> CountFramesWithFfmpegAsync(
        string ffmpegPath,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-hide_banner",
            "-i", sourcePath,
            "-map", "0:v:0",
            "-c", "copy",
            "-f", "null",
            "-"
        ];

        FFmpegProcessResult result = await FFmpegProcessRunner.RunAsync(
            ffmpegPath,
            arguments,
            TimeSpan.FromMinutes(30),
            cancellationToken);

        if (result.ExitCode != 0)
            return null;

        return TryParseFfmpegFrameCount(result.Stderr);
    }

    private static long? TryParseFfmpegFrameCount(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr)) return null;

        MatchCollection matches = Regex.Matches(
            stderr,
            @"frame=\s*(\d+)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        if (matches.Count == 0) return null;

        string text = matches[^1].Groups[1].Value;
        return long.TryParse(text, out long value) && value > 0 ? value : null;
    }

    private static async Task<long?> TryResolveEstimatedFrameCountWithFfprobeAsync(
        string ffprobePath,
        string sourcePath,
        RepartSourceProbe probe,
        long estimatedCount,
        CancellationToken cancellationToken)
    {
        if (estimatedCount <= 0
            || probe.FrameRateNumerator <= 0
            || probe.FrameRateDenominator <= 0)
            return null;

        bool? leftExists = await ProbeFrameExistsAsync(
            ffprobePath,
            sourcePath,
            probe,
            estimatedCount - 1,
            cancellationToken);
        bool? centerExists = await ProbeFrameExistsAsync(
            ffprobePath,
            sourcePath,
            probe,
            estimatedCount,
            cancellationToken);
        bool? rightExists = await ProbeFrameExistsAsync(
            ffprobePath,
            sourcePath,
            probe,
            estimatedCount + 1,
            cancellationToken);
        if (leftExists == null || centerExists == null || rightExists == null)
            return null;

        if (leftExists.Value && !centerExists.Value && !rightExists.Value)
            return estimatedCount;
        if (leftExists.Value && centerExists.Value && !rightExists.Value)
            return estimatedCount + 1;

        if (!leftExists.Value && !centerExists.Value && !rightExists.Value)
            return await ProbeInDirectionForFrameCountAsync(ffprobePath, sourcePath, probe, estimatedCount - 2, -1, cancellationToken);
        if (leftExists.Value && centerExists.Value && rightExists.Value)
            return await ProbeInDirectionForFrameCountAsync(ffprobePath, sourcePath, probe, estimatedCount + 2, 1, cancellationToken);

        throw new InvalidOperationException(FormatUnexpectedFrameProbePattern(
            estimatedCount,
            leftExists.Value,
            centerExists.Value,
            rightExists.Value));
    }

    private static string FormatUnexpectedFrameProbePattern(
        long estimatedCount,
        bool leftExists,
        bool centerExists,
        bool rightExists) =>
        "Unexpected ffprobe frame metadata pattern around estimated frame count " +
        $"{estimatedCount}: [{Bit(leftExists)}, {Bit(centerExists)}, {Bit(rightExists)}].";

    private static string Bit(bool value) => value ? "1" : "0";

    private static async Task<long?> ProbeInDirectionForFrameCountAsync(
        string ffprobePath,
        string sourcePath,
        RepartSourceProbe probe,
        long firstCandidateIndex,
        int step,
        CancellationToken cancellationToken)
    {
        if (step != -1 && step != 1) return null;

        long limit = step < 0
            ? Math.Max(0, firstCandidateIndex - MaxMetadataProbeAdjustmentFrames)
            : firstCandidateIndex + MaxMetadataProbeAdjustmentFrames;
        for (long index = firstCandidateIndex;
             step < 0 ? index >= limit : index <= limit;
             index += step)
        {
            bool? exists = await ProbeFrameExistsAsync(ffprobePath, sourcePath, probe, index, cancellationToken);
            if (exists == null) return null;
            if (step < 0 && exists.Value) return index + 1;
            if (step > 0 && !exists.Value) return index;
        }

        return null;
    }

    private static async Task<bool?> ProbeFrameExistsAsync(
        string ffprobePath,
        string sourcePath,
        RepartSourceProbe probe,
        long frameIndex,
        CancellationToken cancellationToken)
    {
        if (frameIndex < 0) return false;

        double frameDuration = (double)probe.FrameRateDenominator / probe.FrameRateNumerator;
        double targetSeconds = probe.StartTimeSeconds + frameIndex * frameDuration;
        double startSeconds = Math.Max(0d, targetSeconds - frameDuration * 2d);
        double endSeconds = Math.Max(startSeconds + frameDuration, targetSeconds + frameDuration * 2d);
        string interval = string.Create(
            CultureInfo.InvariantCulture,
            $"{startSeconds:0.#########}%{endSeconds:0.#########}");

        string[] arguments =
        [
            "-v", "error", "-hide_banner",
            "-select_streams", "v:0",
            "-read_intervals", interval,
            "-show_frames",
            "-show_entries", "frame=best_effort_timestamp_time,pts_time,pkt_pts_time,pkt_dts_time",
            "-of", "json",
            sourcePath
        ];

        FFprobeProcessResult result = await FFprobeProcessRunner.RunAsync(
            ffprobePath,
            arguments,
            TimeSpan.FromSeconds(15),
            cancellationToken);
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Stdout))
            return null;

        return FfprobeFrameOutputContainsIndex(result.Stdout, probe, frameIndex);
    }

    private static bool? FfprobeFrameOutputContainsIndex(string stdout, RepartSourceProbe probe, long targetFrameIndex)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(stdout);
            if (!document.RootElement.TryGetProperty("frames", out JsonElement frames)
                || frames.ValueKind != JsonValueKind.Array)
                return false;

            bool hasTimestamp = false;
            double fps = (double)probe.FrameRateNumerator / probe.FrameRateDenominator;
            foreach (JsonElement frame in frames.EnumerateArray())
            {
                double? timestamp = TryGetFrameTimestampSeconds(frame);
                if (timestamp == null) continue;
                hasTimestamp = true;

                long frameIndex = (long)Math.Round(
                    (timestamp.Value - probe.StartTimeSeconds) * fps,
                    MidpointRounding.AwayFromZero);
                if (frameIndex == targetFrameIndex)
                    return true;
            }

            return hasTimestamp ? false : null;
        }
        catch
        {
            return null;
        }
    }

    private static double? TryGetFrameTimestampSeconds(JsonElement frame) =>
        TryGetDouble(frame, "best_effort_timestamp_time")
        ?? TryGetDouble(frame, "pts_time")
        ?? TryGetDouble(frame, "pkt_pts_time")
        ?? TryGetDouble(frame, "pkt_dts_time");

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

    private static double? TryGetDurationSeconds(JsonElement root, JsonElement stream)
    {
        double? streamDuration = TryGetDouble(stream, "duration");
        if (streamDuration is > 0) return streamDuration;

        return root.TryGetProperty("format", out JsonElement format)
            ? TryGetDouble(format, "duration")
            : null;
    }

    private static double TryGetStartTimeSeconds(JsonElement root, JsonElement stream)
    {
        double? streamStart = TryGetDouble(stream, "start_time");
        if (streamStart != null) return streamStart.Value;

        return root.TryGetProperty("format", out JsonElement format)
            ? TryGetDouble(format, "start_time") ?? 0d
            : 0d;
    }

    private static long? EstimateFrameCount(double? durationSeconds, int frameRateNumerator, int frameRateDenominator)
    {
        if (durationSeconds is not > 0
            || frameRateNumerator <= 0
            || frameRateDenominator <= 0)
            return null;

        double fps = (double)frameRateNumerator / frameRateDenominator;
        if (!(fps > 0d) || double.IsNaN(fps) || double.IsInfinity(fps))
            return null;

        double exactFrames = durationSeconds.Value * fps;
        if (!(exactFrames > 0d) || double.IsNaN(exactFrames) || double.IsInfinity(exactFrames))
            return null;

        return Math.Max(1L, (long)Math.Round(exactFrames, MidpointRounding.AwayFromZero));
    }

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
            RepartExclusionReason.FrameCountUnavailable => string.IsNullOrWhiteSpace(info.Detail)
                ? string.Format(lang.FrameCountRequired, info.DisplayName)
                : string.Join(
                    Environment.NewLine,
                    string.Format(lang.FrameCountRequired, info.DisplayName),
                    info.Detail.Trim()),
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
        RepartLangProvider lang = RepartLangProvider.Current;
        string message = string.Join(
            Environment.NewLine,
            string.Format(lang["SourceLabel"], source.FilePath),
            string.Format(lang["InterlacedSourceRejected"], source.DisplayName, source.FieldOrder),
            string.Empty,
            lang["InterlacedSourcePrompt"]);
        OpenWarnModalCmd cmd = new(modalNavS, windowTitle, message);
        cmd.Execute(null);
        return cmd.DialogResult == true;
    }
}

// Shared confirm prompt shown when no exact frame count is available (ffmpeg is
// missing) and the duration-based estimate could not be verified. Confirm keeps
// searching with a 10x-expanding frame range; cancel aborts the whole import.
public static class RepartFrameCountPrompt
{
    public static bool Confirm(ModalNavS modalNavS, string windowTitle, RepartFrameCountFallbackInfo source)
    {
        RepartLangProvider lang = RepartLangProvider.Current;
        string message = string.Join(
            Environment.NewLine,
            string.Format(lang["SourceLabel"], source.FilePath),
            string.Format(lang["FrameCountFallbackPrompt"], source.DisplayName, source.EstimatedCount),
            string.Empty,
            lang["FrameCountFallbackPromptChoices"]);
        OpenWarnModalCmd cmd = new(modalNavS, windowTitle, message);
        cmd.Execute(null);
        return cmd.DialogResult == true;
    }
}
