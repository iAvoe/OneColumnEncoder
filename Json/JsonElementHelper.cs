using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OneColumnEncoder.Json;

internal static class JsonElementHelper
{
    public static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }

    // Tries to get an integer. Returns false if failed, otherwise returns true
    public static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return false;
        if (property.ValueKind == JsonValueKind.Number) return property.TryGetInt32(out value);
        return property.ValueKind == JsonValueKind.String
            && int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static long? TryGetLong(JsonElement element, string propertyName, string? parentPropertyName = null)
    {
        JsonElement container = element;
        if (parentPropertyName != null
            && (!element.TryGetProperty(parentPropertyName, out container) || container.ValueKind != JsonValueKind.Object))
            return null;

        if (!container.TryGetProperty(propertyName, out JsonElement property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out long number)) return number;
        if (property.ValueKind != JsonValueKind.String) return null;

        string? text = property.GetString();
        return !string.IsNullOrWhiteSpace(text)
            && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
                ? parsed
                : null;
    }

    public static long? TryGetFrameCount(JsonElement stream)
    {
        long? frameCount = TryGetLong(stream, "nb_frames");
        if (frameCount is > 0) return frameCount;

        if (!stream.TryGetProperty("tags", out JsonElement tags) || tags.ValueKind != JsonValueKind.Object)
            return null;

        foreach (JsonProperty tag in tags.EnumerateObject())
        {
            if (!IsFrameCountTagName(tag.Name) || !TryGetLong(tag.Value, out long value)) continue;
            if (value > 0) return value;
        }

        return null;
    }
    private static bool IsFrameCountTagName(string name) =>
        name.Equals("NUMBER_OF_FRAMES", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("NUMBER_OF_FRAMES-", StringComparison.OrdinalIgnoreCase) &&
        name.Length > "NUMBER_OF_FRAMES-".Length;

    private static bool TryGetLong(JsonElement element, out long value)
    {
        value = 0;
        if (element.ValueKind == JsonValueKind.Number) return element.TryGetInt64(out value);
        return element.ValueKind == JsonValueKind.String
            && long.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static double? TryGetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out double value)) return value;
        return double.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            ? value
            : null;
    }

    public static int? GetInt(JsonNode? node)
    {
        if (node is not JsonValue value) return null;
        if (value.TryGetValue<int>(out int intValue)) return intValue;
        if (value.TryGetValue<long>(out long longValue)
            && longValue >= int.MinValue
            && longValue <= int.MaxValue)
            return (int)longValue;
        if (value.TryGetValue<string>(out string? stringValue)
            && int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            return parsed;
        return null;
    }

    public static string? GetString(JsonNode? node)
    {
        if (node is not JsonValue value) return null;
        return value.TryGetValue<string>(out string? stringValue) ? stringValue : null;
    }
}
