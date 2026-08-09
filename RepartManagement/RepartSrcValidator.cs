using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.RepartManagement;

// These are videos that cannot be added to repart mode queue, because repartition involves video concatenation.
public enum RepartExclusionReason
{
    SourceMissing,
    ProbeFailed,
    NoVideoStream,
    NoDimensions,
    Interlaced,
    NotCfr,
    NoFrameCount,
    SourceChanged,
    SignatureMismatch
}

public sealed record RepartExcludedSrcInfo(
    string FilePath,
    string DisplayName,
    RepartExclusionReason Reason,
    string? Detail);

public sealed record RepartInterlacedSrcInfo(
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

public sealed record RepartSrcFile(
    string FilePath,
    string DisplayName,
    long InitialLength,
    long InitialWriteTicks);

public sealed record RepartSrcFileOutcome(
    RepartExclusionReason? RejectionReason,
    string? Detail,
    RepartSrcFile? SrcFile);

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
    RepartSrcProbe? Probe);

// Data collected from the probe stage, needed for the expensive frame-count scan.
public sealed record RepartSrcProbe(
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
public static partial class RepartSrcValidator
{
    private const string ShowEntries =
        "stream=codec_name,profile,codec_tag_string,level,width,height,coded_width,coded_height," +
        "pix_fmt,bits_per_raw_sample,field_order,sample_aspect_ratio,avg_frame_rate,r_frame_rate," +
        "time_base,color_range,color_space,color_transfer,color_primaries,chroma_location," +
        "nb_frames,nb_read_frames,duration,start_time,extradata:format=duration,start_time";
    private const int MaxMetadataProbeAdjustmentFrames = 300;
    private const double FrameProbeSeekMarginSeconds = 2d;

    // Stage 1: filters that do not need ffprobe.
    public static RepartSrcFileOutcome CheckWithoutFfprobe(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        string displayName = Path.GetFileName(fullPath);
        if (!File.Exists(fullPath))
            return RejectedFile(RepartExclusionReason.SourceMissing);

        FileInfo file = new(fullPath);
        return new RepartSrcFileOutcome(
            null,
            null,
            new RepartSrcFile(
                fullPath,
                displayName,
                file.Length,
                file.LastWriteTimeUtc.Ticks));
    }

    // Stage 2: simple ffprobe filtering. This mirrors queue-mode behavior: run a
    // short metadata-only probe and exclude sources ffprobe cannot analyze before
    // the heavier Repart-specific ffprobe analysis is attempted.
    public static async Task<RepartSrcFileOutcome> ProbeCanAnalyzeAsync(
        string ffprobePath,
        RepartSrcFile srcFile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await FFProbeVideoAnalysis.AnalyzeAsync(
                ffprobePath,
                srcFile.FilePath,
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

        return new RepartSrcFileOutcome(null, null, srcFile);
    }

    // Stage 3: Repart-specific ffprobe analysis. Only sources that passed the
    // simple probe can reach this point.
    public static async Task<RepartRawProbeOutcome> AnalyzeWithFfprobeAsync(
        string ffprobePath,
        RepartSrcFile srcFile,
        CancellationToken cancellationToken = default)
    {
        string rawJson;
        try
        {
            rawJson = await ProbeAsync(ffprobePath, srcFile.FilePath, cancellationToken);
            return new RepartRawProbeOutcome(
                null,
                null,
                new RepartRawProbe(
                    rawJson,
                    srcFile.InitialLength,
                    srcFile.InitialWriteTicks));
        }
        catch (OperationCanceledException) { throw; }
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
                new RepartSrcProbe(
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
    // with ffmpeg's exact null remux as a fallback and a full ffprobe count as the
    // final fallback. This order is shared by folder and chapter imports.
    public static async Task<RepartScanOutcome> ScanFramesAsync(
        string ffprobePath,
        string? ffmpegPath,
        string filePath,
        RepartSrcProbe probe,
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
            return RejectedScan(RepartExclusionReason.NoFrameCount, ex.Message);
        }
        if (frameCount <= 0)
            return RejectedScan(RepartExclusionReason.NoFrameCount);

        FileInfo file = new(filePath);
        file.Refresh();
        if (file.Length != probe.InitialLength || file.LastWriteTimeUtc.Ticks != probe.InitialWriteTicks)
            return RejectedScan(RepartExclusionReason.SourceChanged);

        return new RepartScanOutcome(null, null, frameCount, file.Length, file.LastWriteTimeUtc.Ticks);
    }

    private static RepartProbeOutcome Rejected(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, null);

    private static RepartSrcFileOutcome RejectedFile(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, null);

    private static RepartRawProbeOutcome RejectedRaw(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, null);

    private static RepartScanOutcome RejectedScan(RepartExclusionReason reason, string? detail = null) =>
        new(reason, detail, 0, 0, 0);

    private static async Task<string> ProbeAsync(
        string ffprobePath,
        string srcPath,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-v", "error", "-hide_banner", "-select_streams", "v:0",
            "-show_data", "-show_entries", ShowEntries, "-of", "json", srcPath
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
        RepartSrcProbe probe,
        string srcPath,
        string displayName,
        Func<RepartFrameCountFallbackInfo, bool>? confirmExpandFrameCountSearch,
        CancellationToken cancellationToken)
    {
        long? estimatedCount = EstimateFrameCount(probe.DurationSeconds, probe.FrameRateNumerator, probe.FrameRateDenominator);

        if (estimatedCount is > 0)
        {
            long? metadataCount = await TryResolveEstimatedFrameCountWithFfprobeAsync(
                ffprobePath,
                srcPath,
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
                long? exactCount = await CountFramesWithFfmpegAsync(ffmpegPath!, srcPath, cancellationToken);
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

        // Full frame counting is the slowest fallback. It is deliberately kept
        // after the ffmpeg attempt so normal imports remain on the estimate or
        // ffmpeg paths.
        try
        {
            long? probedCount = await CountFramesWithFfprobeAsync(
                ffprobePath,
                srcPath,
                cancellationToken);
            if (probedCount is > 0)
                return probedCount.Value;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
        }

        // No exact count (no ffmpeg) and the duration-based estimate could not be
        // verified. Ask the user whether to run an extended search that expands the
        // frame-count range by 10x per step to locate the real boundary, or to
        // cancel the import. Without a prompt callback the raw estimate is kept.
        if (estimatedCount is > 0 && !ffmpegAvailable && confirmExpandFrameCountSearch != null)
        {
            bool retry = confirmExpandFrameCountSearch(new(
                srcPath,
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
                srcPath,
                probe,
                estimatedCount.Value,
                cancellationToken);
            if (expandedCount is > 0)
                return expandedCount.Value;
        }

        return estimatedCount is > 0 ? estimatedCount.Value : 0;
    }

    private static async Task<long?> CountFramesWithFfprobeAsync(
        string ffprobePath,
        string srcPath,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-v", "error",
            "-hide_banner",
            "-count_frames",
            "-select_streams", "v:0",
            "-show_entries", "stream=nb_read_frames,nb_frames",
            "-of", "json",
            srcPath
        ];

        FFprobeProcessResult result = await FFprobeProcessRunner.RunAsync(
            ffprobePath,
            arguments,
            TimeSpan.FromMinutes(30),
            cancellationToken);
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Stdout))
            return null;

        using JsonDocument document = JsonDocument.Parse(result.Stdout);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
            return null;

        long? readFrames = TryGetLong(stream, "nb_read_frames");
        return readFrames is > 0
            ? readFrames.Value
            : TryGetFrameCount(stream);
    }

    // Brackets the real frame count by probing at frame indices that grow by 10x
    // each step, then narrows the boundary with a binary search. Used only when
    // ffmpeg is unavailable and the estimate-based probe walk (±300 frames) could
    // not find the end of the video.
    private static async Task<long?> SearchFrameCountWithExpansionAsync(
        string ffprobePath,
        string srcPath,
        RepartSrcProbe probe,
        long estimatedCount,
        CancellationToken cancellationToken)
    {
        if (estimatedCount <= 0
            || probe.FrameRateNumerator <= 0
            || probe.FrameRateDenominator <= 0)
            return null;

        bool? estimateExists = await ProbeFrameExistsAsync(
            ffprobePath,
            srcPath,
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
                bool? exists = await ProbeFrameExistsAsync(ffprobePath, srcPath, probe, hi, cancellationToken);
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
                bool? exists = await ProbeFrameExistsAsync(ffprobePath, srcPath, probe, lo, cancellationToken);
                if (exists == null)
                    return null;
                if (exists.Value)
                    break;
                hi = lo;
                lo = Math.Max(0, hi / 10);
            }
            if (lo <= 0)
            {
                bool? first = await ProbeFrameExistsAsync(ffprobePath, srcPath, probe, 0, cancellationToken);
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
            bool? exists = await ProbeFrameExistsAsync(ffprobePath, srcPath, probe, mid, cancellationToken);
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
        string srcPath,
        CancellationToken cancellationToken)
    {
        string[] arguments =
        [
            "-hide_banner",
            "-i", srcPath,
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

        MatchCollection matches = FFmpegTotalFramesMatcher().Matches(stderr);
        if (matches.Count == 0) return null;

        string text = matches[^1].Groups[1].Value;
        return long.TryParse(text, out long value) && value > 0 ? value : null;
    }

    private static async Task<long?> TryResolveEstimatedFrameCountWithFfprobeAsync(
        string ffprobePath,
        string srcPath,
        RepartSrcProbe probe,
        long estimatedCount,
        CancellationToken cancellationToken)
    {
        if (estimatedCount <= 0
            || probe.FrameRateNumerator <= 0
            || probe.FrameRateDenominator <= 0)
            return null;

        bool? leftExists = await ProbeFrameExistsAsync(
            ffprobePath,
            srcPath,
            probe,
            estimatedCount - 1,
            cancellationToken);
        bool? centerExists = await ProbeFrameExistsAsync(
            ffprobePath,
            srcPath,
            probe,
            estimatedCount,
            cancellationToken);
        bool? rightExists = await ProbeFrameExistsAsync(
            ffprobePath,
            srcPath,
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
            return await ProbeInDirectionForFrameCountAsync(ffprobePath, srcPath, probe, estimatedCount - 2, -1, cancellationToken);
        if (leftExists.Value && centerExists.Value && rightExists.Value)
            return await ProbeInDirectionForFrameCountAsync(ffprobePath, srcPath, probe, estimatedCount + 2, 1, cancellationToken);

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
        string srcPath,
        RepartSrcProbe probe,
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
            bool? exists = await ProbeFrameExistsAsync(ffprobePath, srcPath, probe, index, cancellationToken);
            if (exists == null) return null;
            if (step < 0 && exists.Value) return index + 1;
            if (step > 0 && !exists.Value) return index;
        }

        return null;
    }

    private static async Task<bool?> ProbeFrameExistsAsync(
        string ffprobePath,
        string srcPath,
        RepartSrcProbe probe,
        long frameIndex,
        CancellationToken cancellationToken)
    {
        if (frameIndex < 0) return false;

        double frameDuration = (double)probe.FrameRateDenominator / probe.FrameRateNumerator;
        double targetSeconds = probe.StartTimeSeconds + frameIndex * frameDuration;
        double seekMargin = Math.Max(frameDuration * 2d, FrameProbeSeekMarginSeconds);
        double startSeconds = Math.Max(0d, targetSeconds - seekMargin);
        double endSeconds = Math.Max(startSeconds + frameDuration, targetSeconds + seekMargin);
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
            srcPath
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

    private static bool? FfprobeFrameOutputContainsIndex(string stdout, RepartSrcProbe probe, long targetFrameIndex)
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
        catch { return null; }
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
    [GeneratedRegex(@"frame=\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex FFmpegTotalFramesMatcher();
}

// Formats an exclusion into either a bare reason line or a full per-source dialog
// message (source path + reason + will-exclude notice).
public static class RepartExclusionMessages
{
    public static string FormatReason(RepartExcludedSrcInfo info)
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
                lang["InterlacedSrcRejected"],
                info.DisplayName,
                info.Detail ?? string.Empty),
            RepartExclusionReason.NotCfr => string.Format(lang.CfrRequired, info.DisplayName),
            RepartExclusionReason.NoFrameCount => string.IsNullOrWhiteSpace(info.Detail)
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

    public static string FormatExcludedMessage(RepartExcludedSrcInfo info) =>
        string.Join(
            Environment.NewLine,
            string.Format(RepartLangProvider.Current["SrcLabel"], info.FilePath),
            FormatReason(info),
            string.Empty,
            RepartLangProvider.Current["WillExcludeSource"]);
}

// Shared confirm prompt for interlaced sources, used by every Repart Mode import
// entry point (pre-open import and in-window re-import).
public static class RepartInterlacedPrompt
{
    public static bool Confirm(ModalNavS modalNavS, string windowTitle, RepartInterlacedSrcInfo source)
    {
        RepartLangProvider lang = RepartLangProvider.Current;
        string message = string.Join(
            Environment.NewLine,
            string.Format(lang["SrcLabel"], source.FilePath),
            string.Format(lang["InterlacedSrcRejected"], source.DisplayName, source.FieldOrder),
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
            string.Format(lang["SrcLabel"], source.FilePath),
            string.Format(lang["FrameCountFallbackPrompt"], source.DisplayName, source.EstimatedCount),
            string.Empty,
            lang["FrameCountFallbackPromptChoices"]);
        OpenWarnModalCmd cmd = new(modalNavS, windowTitle, message);
        cmd.Execute(null);
        return cmd.DialogResult == true;
    }
}
