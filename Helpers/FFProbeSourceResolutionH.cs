using System.Text.Json;

namespace OneColumnEncoder.Helpers;

public static class FFProbeSourceResolutionH
{
    public static (int width, int height)? Read(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (!FrameRateH.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return null;

            if (stream.TryGetProperty("width", out JsonElement w) && w.TryGetInt32(out int width)
                && stream.TryGetProperty("height", out JsonElement h) && h.TryGetInt32(out int height))
                return (width, height);
        }
        catch
        {
            return null;
        }

        return null;
    }
}
