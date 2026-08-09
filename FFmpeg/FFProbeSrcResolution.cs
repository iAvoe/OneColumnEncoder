namespace OneColumnEncoder.FFmpeg;

public static class FFProbeSrcResolution
{
    public static (int width, int height)? Read(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (!FrameRate.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                return null;

            if (stream.TryGetProperty("width", out JsonElement w) && w.TryGetInt32(out int width)
                && stream.TryGetProperty("height", out JsonElement h) && h.TryGetInt32(out int height))
                return (width, height);
        }
        catch { return null; }

        return null;
    }
}
