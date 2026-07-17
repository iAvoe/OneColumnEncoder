using OneColumnEncoder.Json;
using OneColumnEncoder.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.FFmpeg;

public static class FFProbeFPSReviser
{
    public static FPSReviserResult Apply(string rawJson, FPSReviserRequest request)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            throw new ArgumentException("ffprobe JSON cannot be empty.", nameof(rawJson));
        if (request.OutputFrameRateNumerator <= 0 || request.OutputFrameRateDenominator <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "Output frame rate must be positive.");

        VideoAnalysisHypothesisKind kind = VideoAnalysisHypothesisCatalog.ParseKind(request.HypothesisId);
        if (kind is VideoAnalysisHypothesisKind.MixedPip or VideoAnalysisHypothesisKind.Spliced)
            throw new InvalidOperationException("The selected cadence cannot be represented by one video JSON structure.");

        using JsonDocument document = JsonDocument.Parse(rawJson);
        if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
            throw new InvalidOperationException("No video stream found in ffprobe JSON.");

        (int numerator, int denominator)? sourceRate = FrameRate.GetAvgFrameRate(stream) ?? FrameRate.GetRFrameRate(stream);
        double? duration = TryGetDouble(stream, "duration")
            ?? (document.RootElement.TryGetProperty("format", out JsonElement format)
                ? TryGetDouble(format, "duration")
                : null);
        long? sourceFrames = TryGetFrameCount(stream);
        bool sameFrameRate = sourceRate.HasValue
            && (long)sourceRate.Value.numerator * request.OutputFrameRateDenominator
                == (long)request.OutputFrameRateNumerator * sourceRate.Value.denominator;

        VideoAnalysisFrameCountKind frameCountKind;
        long? outputFrames;
        if (kind == VideoAnalysisHypothesisKind.ProgressiveSource)
        {
            outputFrames = sourceFrames ?? EstimateOutputFrameCount(
                duration,
                sourceFrames,
                sourceRate,
                request.OutputFrameRateNumerator,
                request.OutputFrameRateDenominator);
            frameCountKind = sourceFrames.HasValue
                ? VideoAnalysisFrameCountKind.Exact
                : outputFrames.HasValue
                    ? VideoAnalysisFrameCountKind.Estimated
                    : VideoAnalysisFrameCountKind.Unknown;
        }
        else if (kind == VideoAnalysisHypothesisKind.NativeDeinterlace && sameFrameRate)
        {
            outputFrames = sourceFrames;
            frameCountKind = sourceFrames.HasValue
                ? VideoAnalysisFrameCountKind.Exact
                : VideoAnalysisFrameCountKind.Unknown;
        }
        else if (kind == VideoAnalysisHypothesisKind.Pal22 && sameFrameRate && sourceFrames.HasValue)
        {
            outputFrames = sourceFrames;
            frameCountKind = VideoAnalysisFrameCountKind.Exact;
        }
        else if (kind is VideoAnalysisHypothesisKind.Telecine3232
            or VideoAnalysisHypothesisKind.Telecine2323
            or VideoAnalysisHypothesisKind.Telecine3223
            or VideoAnalysisHypothesisKind.Telecine2332
            or VideoAnalysisHypothesisKind.Telecine3322
            or VideoAnalysisHypothesisKind.FourField2224
            or VideoAnalysisHypothesisKind.FourField2242
            or VideoAnalysisHypothesisKind.FourField2422
            or VideoAnalysisHypothesisKind.FourField4222)
        {
            outputFrames = CalculateCadenceFrameCount(sourceFrames, kind);
            frameCountKind = outputFrames.HasValue
                ? VideoAnalysisFrameCountKind.Exact
                : VideoAnalysisFrameCountKind.Unknown;
        }
        else
        {
            outputFrames = EstimateOutputFrameCount(
                duration,
                sourceFrames,
                sourceRate,
                request.OutputFrameRateNumerator,
                request.OutputFrameRateDenominator);
            frameCountKind = outputFrames.HasValue
                ? VideoAnalysisFrameCountKind.Estimated
                : VideoAnalysisFrameCountKind.Unknown;
        }

        JsonNode? rootNode = JsonNode.Parse(rawJson);
        if (rootNode is not JsonObject rootObject)
            throw new InvalidOperationException("ffprobe JSON root is not an object.");

        JsonObject videoStream = GetFirstVideoStream(rootObject);
        videoStream["field_order"] = "progressive";
        string fps = string.Create(
            CultureInfo.InvariantCulture,
            $"{request.OutputFrameRateNumerator}/{request.OutputFrameRateDenominator}");
        videoStream["avg_frame_rate"] = fps;
        videoStream["r_frame_rate"] = fps;
        videoStream["time_base"] = $"{request.OutputFrameRateDenominator}/{request.OutputFrameRateNumerator}";
        RemoveFrameCountTags(videoStream);

        if (outputFrames.HasValue)
        {
            videoStream["nb_frames"] = outputFrames.Value.ToString(CultureInfo.InvariantCulture);
            videoStream["nb_frames_by_1cenc"] = frameCountKind == VideoAnalysisFrameCountKind.Exact
                ? "exact"
                : "estimated";
        }
        else
        {
            videoStream.Remove("nb_frames");
            RemoveFrameCountTags(videoStream);
            videoStream["nb_frames_by_1cenc"] = "unknown";
        }

        return new(
            rootObject.ToJsonString(FFProbeJsonFormatting.Options),
            kind,
            request.OutputFrameRateNumerator,
            request.OutputFrameRateDenominator,
            outputFrames,
            frameCountKind,
            IsProgressive: true);
    }

    public static (int numerator, int denominator)? ReadSourceFrameRate(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            return FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)
                ? FrameRate.GetAvgFrameRate(stream) ?? FrameRate.GetRFrameRate(stream)
                : null;
        }
        catch
        {
            return null;
        }
    }

    public static (int numerator, int denominator) GetDefaultOutputFrameRate(
        string rawJson,
        VideoAnalysisHypothesisKind kind)
    {
        (int numerator, int denominator)? source = ReadSourceFrameRate(rawJson);
        if (kind is VideoAnalysisHypothesisKind.ProgressiveSource)
            return source ?? (30, 1);

        if (kind is VideoAnalysisHypothesisKind.Telecine3232
            or VideoAnalysisHypothesisKind.Telecine2323
            or VideoAnalysisHypothesisKind.Telecine3223
            or VideoAnalysisHypothesisKind.Telecine2332
            or VideoAnalysisHypothesisKind.Telecine3322
            or VideoAnalysisHypothesisKind.FourField2224
            or VideoAnalysisHypothesisKind.FourField2242
            or VideoAnalysisHypothesisKind.FourField2422
            or VideoAnalysisHypothesisKind.FourField4222)
        {
            return source.HasValue && source.Value.numerator / (double)source.Value.denominator > 25d
                ? (24000, 1001)
                : (24, 1);
        }

        if (kind == VideoAnalysisHypothesisKind.EuroPulldown)
            return (24, 1);

        return source ?? (30, 1);
    }

    private static long? CalculateCadenceFrameCount(long? sourceFrames, VideoAnalysisHypothesisKind kind)
    {
        if (sourceFrames is not > 0)
            return null;

        (int offset, int k) = kind switch
        {
            VideoAnalysisHypothesisKind.Telecine3232 => (3, 4),
            VideoAnalysisHypothesisKind.Telecine2323 => (4, 5),
            VideoAnalysisHypothesisKind.Telecine3223 => (4, 5),
            VideoAnalysisHypothesisKind.Telecine2332 => (3, 4),
            VideoAnalysisHypothesisKind.Telecine3322 => (2, 3),
            VideoAnalysisHypothesisKind.FourField2224 => (4, 5),
            VideoAnalysisHypothesisKind.FourField2242 => (3, 4),
            VideoAnalysisHypothesisKind.FourField2422 => (2, 3),
            VideoAnalysisHypothesisKind.FourField4222 => (1, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        long adjusted = sourceFrames.Value - offset;
        if (adjusted < 0)
            return null;

        long fullCycles = adjusted / 5;
        long remainder = adjusted % 5;

        long remainderValid = remainder < k ? remainder : remainder - 1;

        return fullCycles * 4 + remainderValid;
    }

    private static long? EstimateOutputFrameCount(
        double? duration,
        long? sourceFrames,
        (int numerator, int denominator)? sourceRate,
        int outputNumerator,
        int outputDenominator)
    {
        if (duration is > 0)
        {
            double value = duration.Value * outputNumerator / outputDenominator;
            return value > 0 && value < long.MaxValue ? (long)Math.Round(value) : null;
        }

        if (sourceFrames is > 0 && sourceRate.HasValue)
        {
            double value = sourceFrames.Value
                * (outputNumerator / (double)outputDenominator)
                / (sourceRate.Value.numerator / (double)sourceRate.Value.denominator);
            return value > 0 && value < long.MaxValue ? (long)Math.Round(value) : null;
        }

        return null;
    }

    private static JsonObject GetFirstVideoStream(JsonObject rootObject)
    {
        if (rootObject["streams"] is not JsonArray streams)
            throw new InvalidOperationException("ffprobe JSON has no streams array.");

        foreach (JsonNode? node in streams)
        {
            if (node is not JsonObject stream) continue;
            string? codecType = GetString(stream["codec_type"]);
            if (codecType == null || codecType.Equals("video", StringComparison.OrdinalIgnoreCase))
                return stream;
        }

        throw new InvalidOperationException("No video stream found in ffprobe JSON.");
    }

    private static void RemoveFrameCountTags(JsonObject stream)
    {
        if (stream["tags"] is not JsonObject tags) return;
        foreach (string name in tags.Select(property => property.Key).Where(IsFrameCountTagName).ToArray())
            tags.Remove(name);
    }

    private static bool IsFrameCountTagName(string name) =>
        name.Equals("NUMBER_OF_FRAMES", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("NUMBER_OF_FRAMES-", StringComparison.OrdinalIgnoreCase);
}
