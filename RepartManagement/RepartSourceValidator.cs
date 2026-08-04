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

// Outcome of one per-file Repart Mode check pass. When rejected, the reason tells
// the caller why the source was excluded so it can be reported or filtered.
public sealed record RepartSourceValidation(
    bool IsAccepted,
    RepartExclusionReason? ExclusionReason,
    string? Detail,
    string RawJson = "",
    long FrameCount = 0,
    int FrameRateNumerator = 0,
    int FrameRateDenominator = 0,
    long FileLength = 0,
    long LastWriteUtcTicks = 0,
    RepartVideoFormatSignature? Signature = null);

// Modular per-file source checks for Repart Mode. Every check is an independent,
// reusable unit producing a structured rejection or acceptance outcome, so that
// any entry point (pre-open import, in-window re-import) applies the exact same
// validation and can report or filter exclusions consistently.
public static class RepartSourceValidator
{
    private const string ShowEntries =
        "stream=codec_name,profile,codec_tag_string,level,width,height,coded_width,coded_height," +
        "pix_fmt,bits_per_raw_sample,field_order,sample_aspect_ratio,avg_frame_rate,r_frame_rate," +
        "time_base,color_range,color_space,color_transfer,color_primaries,chroma_location," +
        "nb_frames,nb_read_frames,duration,extradata:format=duration";

    public static async Task<RepartSourceValidation> ValidateAsync(
        string ffprobePath,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return Rejected(RepartExclusionReason.SourceMissing, filePath);

        FileInfo beforeAnalysis = new(filePath);
        long initialLength = beforeAnalysis.Length;
        long initialWriteTicks = beforeAnalysis.LastWriteTimeUtc.Ticks;

        string rawJson;
        try
        {
            rawJson = await ProbeAsync(ffprobePath, filePath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Rejected(RepartExclusionReason.ProbeFailed, filePath, ex.Message);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return Rejected(RepartExclusionReason.NoVideoStream, filePath);

            string fieldOrder = Get(stream, "field_order");
            if (!IsProgressiveFieldOrder(fieldOrder))
                return Rejected(RepartExclusionReason.Interlaced, filePath, fieldOrder);

            if (GetInt(stream, "width") <= 0 || GetInt(stream, "height") <= 0)
                return Rejected(RepartExclusionReason.NoDimensions, filePath);

            if (!TryResolveCfrFrameRate(stream, out (int num, int den) frameRate))
                return Rejected(RepartExclusionReason.NotCfr, filePath);

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
                return Rejected(RepartExclusionReason.FrameCountUnavailable, filePath);
            }
            if (frameCount <= 0)
                return Rejected(RepartExclusionReason.FrameCountUnavailable, filePath);

            RepartVideoFormatSignature signature = BuildSignature(stream, frameRate.num, frameRate.den);

            FileInfo file = new(filePath);
            file.Refresh();
            if (file.Length != initialLength || file.LastWriteTimeUtc.Ticks != initialWriteTicks)
                return Rejected(RepartExclusionReason.SourceChanged, filePath);

            return new RepartSourceValidation(
                IsAccepted: true,
                ExclusionReason: null,
                Detail: null,
                RawJson: rawJson,
                FrameCount: frameCount,
                FrameRateNumerator: frameRate.num,
                FrameRateDenominator: frameRate.den,
                FileLength: file.Length,
                LastWriteUtcTicks: file.LastWriteTimeUtc.Ticks,
                Signature: signature);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Rejected(RepartExclusionReason.ProbeFailed, filePath, ex.Message);
        }
    }

    private static RepartSourceValidation Rejected(
        RepartExclusionReason reason,
        string filePath,
        string? detail = null) =>
        new(false, reason, detail);

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
