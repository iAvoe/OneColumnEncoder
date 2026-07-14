using OneColumnEncoder.Converters;
using static OneColumnEncoder.Json.JsonElementHelper;
using System.Globalization;
using System.Text.Json;

namespace OneColumnEncoder.FFmpeg;

public static class FrameRate
{
    public static (int num, int den)? ParseFraction(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string[] parts = value.Split('/');
        if (parts.Length == 2
            && int.TryParse(parts[0], out int n)
            && int.TryParse(parts[1], out int d)
            && n > 0 && d > 0)
            return (n, d);
        return null;
    }

    public static bool IsUsable(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Equals("0/0", StringComparison.OrdinalIgnoreCase);

    public static bool TryParseFrameRate(string? value, out double frameRate)
    {
        frameRate = 0d;
        if (string.IsNullOrWhiteSpace(value)) return false;

        string text = value.Trim();
        if (text.Equals("0", StringComparison.OrdinalIgnoreCase)
            || text.Equals("0/0", StringComparison.OrdinalIgnoreCase)
            || text.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            return false;

        string[] parts = text.Split('/');
        if (parts.Length == 2)
        {
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double numerator)
                || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double denominator)
                || denominator == 0d)
                return false;

            frameRate = numerator / denominator;
            return frameRate > 0d;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out frameRate)
            && frameRate > 0d;
    }

    public static (int num, int den)? GetRFrameRate(string rawJson) =>
        GetFrameRate(rawJson, "r_frame_rate");

    public static (int num, int den)? GetAvgFrameRate(string rawJson) =>
        GetFrameRate(rawJson, "avg_frame_rate");

    private static (int num, int den)? GetFrameRate(string rawJson, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(rawJson);
            if (!TryGetFirstVideoStream(doc.RootElement, out JsonElement stream))
                return null;
            string? val = TryGetString(stream, propertyName);
            return IsUsable(val) ? ParseFraction(val) : null;
        }
        catch
        {
            return null;
        }
    }

    public static bool? IsVariableFrameRate(string rawJson)
    {
        (int num, int den)? avg = GetAvgFrameRate(rawJson);
        (int num, int den)? r = GetRFrameRate(rawJson);
        if (avg == null || r == null) return null;
        return avg.Value.num * r.Value.den != r.Value.num * avg.Value.den;
    }

    public static (bool isVariable, int num, int den)? GetVariableFrameRateInfo(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (!TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return null;

            bool isVariable = IsVariableFrameRate(stream) == true;
            (int num, int den)? rate = isVariable ? GetRFrameRate(stream) : null;
            return (isVariable, rate?.num ?? 0, rate?.den ?? 0);
        }
        catch
        {
            return null;
        }
    }

    public static (int num, int den)? GetRFrameRate(JsonElement stream)
    {
        string? val = TryGetString(stream, "r_frame_rate");
        return IsUsable(val) ? ParseFraction(val) : null;
    }

    public static (int num, int den)? GetAvgFrameRate(JsonElement stream)
    {
        string? val = TryGetString(stream, "avg_frame_rate");
        return IsUsable(val) ? ParseFraction(val) : null;
    }

    public static bool? IsVariableFrameRate(JsonElement stream)
    {
        (int num, int den)? avg = GetAvgFrameRate(stream);
        (int num, int den)? r = GetRFrameRate(stream);
        if (avg == null || r == null) return null;
        return avg.Value.num * r.Value.den != r.Value.num * avg.Value.den;
    }

    public static bool TryGetFirstVideoStream(JsonElement root, out JsonElement stream)
    {
        stream = default;
        if (!root.TryGetProperty("streams", out JsonElement streams) || streams.ValueKind != JsonValueKind.Array)
            return false;
        foreach (JsonElement item in streams.EnumerateArray())
        {
            string? codecType = null;
            if (item.TryGetProperty("codec_type", out JsonElement ct))
                codecType = ct.GetString();
            if (codecType is null or "video")
            {
                stream = item;
                return true;
            }
        }
        return false;
    }

    public static string NormalizeFrameRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("0/0", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        string text = value.Trim();
        string[] parts = text.Split('/');
        if (parts.Length == 2
            && long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long numerator)
            && long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long denominator)
            && denominator != 0)
        {
            long gcd = MathUtilities.GreatestCommonDivisor(Math.Abs(numerator), Math.Abs(denominator));
            return string.Create(CultureInfo.InvariantCulture, $"{numerator / gcd}/{denominator / gcd}");
        }

        return text;
    }
}
