using OneColumnEncoder.Converters;
using OneColumnEncoder.Json;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OneColumnEncoder.FFmpeg;

public readonly record struct AspectRatioFraction(long Numerator, long Denominator)
{
    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{Numerator}:{Denominator}");

    public static AspectRatioFraction Simplify(long numerator, long denominator)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(denominator);
        if (numerator == 0) return new AspectRatioFraction(0, 1);

        long gcd = MathUtilities.GreatestCommonDivisor(Math.Abs(numerator), Math.Abs(denominator));
        return new AspectRatioFraction(numerator / gcd, denominator / gcd);
    }
}

public readonly record struct FFProbeAspectRatio(
    AspectRatioFraction StAR,
    AspectRatioFraction Sar,
    AspectRatioFraction Dar)
{
    public bool HasSquarePixels => Sar == FFProbeAspectRatioResolver.Square;
}

public static class FFProbeAspectRatioResolver
{
    public static readonly AspectRatioFraction Square = new(1, 1);

    public static FFProbeAspectRatio Resolve(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return Default();

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            return FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream)
                ? Resolve(stream)
                : Default();
        }
        catch { return Default(); }
    }

    public static FFProbeAspectRatio Resolve(JsonElement stream) => new(ReadStAR(stream), ReadSar(stream), ReadDar(stream));

    public static FFProbeAspectRatio Resolve(JsonObject stream) => new(ReadStAR(stream), ReadSar(stream), ReadDar(stream));

    public static AspectRatioFraction ReadStAR(JsonElement stream) =>
        TryReadCalculated(stream, "width", "height")
        ?? Square;

    public static AspectRatioFraction ReadStAR(JsonObject stream) =>
        TryReadCalculated(stream, "width", "height")
        ?? Square;

    public static AspectRatioFraction ReadSar(JsonElement stream) =>
        TryReadMetadata(stream, "sar", "sample_aspect_ratio")
        ?? TryReadCalculated(stream, "width", "coded_width")
        ?? Square;

    public static AspectRatioFraction ReadSar(JsonObject stream) =>
        TryReadMetadata(stream, "sar", "sample_aspect_ratio")
        ?? TryReadCalculated(stream, "width", "coded_width")
        ?? Square;

    public static AspectRatioFraction ReadDar(JsonElement stream) =>
        TryReadMetadata(stream, "dar", "display_aspect_ratio")
        ?? TryReadCalculated(stream, "coded_width", "coded_height")
        ?? Square;

    public static AspectRatioFraction ReadDar(JsonObject stream) =>
        TryReadMetadata(stream, "dar", "display_aspect_ratio")
        ?? TryReadCalculated(stream, "coded_width", "coded_height")
        ?? Square;

    public static bool HasSquarePixels(JsonElement stream) => Resolve(stream).HasSquarePixels;

    public static bool HasSquarePixels(JsonObject stream) => Resolve(stream).HasSquarePixels;

    private static FFProbeAspectRatio Default() => new(Square, Square, Square);

    private static AspectRatioFraction? TryReadMetadata(JsonElement stream, string firstProperty, string secondProperty) =>
        TryParseMetadata(JsonElementHelper.TryGetString(stream, firstProperty))
        ?? TryParseMetadata(JsonElementHelper.TryGetString(stream, secondProperty));

    private static AspectRatioFraction? TryReadMetadata(JsonObject stream, string firstProperty, string secondProperty) =>
        TryParseMetadata(JsonElementHelper.GetString(stream[firstProperty]))
        ?? TryParseMetadata(JsonElementHelper.GetString(stream[secondProperty]));

    private static AspectRatioFraction? TryReadCalculated(JsonElement stream, string numeratorProperty, string denominatorProperty)
    {
        return JsonElementHelper.TryGetInt(stream, numeratorProperty, out int numerator)
            && JsonElementHelper.TryGetInt(stream, denominatorProperty, out int denominator)
            && numerator >= 0
            && denominator > 0
                ? AspectRatioFraction.Simplify(numerator, denominator)
                : null;
    }

    private static AspectRatioFraction? TryReadCalculated(JsonObject stream, string numeratorProperty, string denominatorProperty)
    {
        int? numerator = JsonElementHelper.GetInt(stream[numeratorProperty]);
        int? denominator = JsonElementHelper.GetInt(stream[denominatorProperty]);
        return numerator >= 0 && denominator > 0
            ? AspectRatioFraction.Simplify(numerator.Value, denominator.Value)
            : null;
    }

    private static AspectRatioFraction? TryParseMetadata(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        char separator = value.Contains(':') ? ':' : '/';
        string[] parts = value.Split(separator);
        if (parts.Length != 2) return null;

        return long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long numerator)
            && long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long denominator)
            && numerator >= 0
            && denominator > 0
                ? AspectRatioFraction.Simplify(numerator, denominator)
                : null;
    }
}
