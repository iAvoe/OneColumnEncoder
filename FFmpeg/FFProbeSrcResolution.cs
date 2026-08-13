namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Reads the first video stream resolution from ffprobe JSON
/// </summary>
public static class FFProbeSrcResolution
{
    /// <summary>
    /// Reads the first video stream resolution from ffprobe JSON
    /// </summary>
    /// <param name="rawJson">The raw JSON output produced by ffprobe.</param>
    /// <returns>
    /// The width and height of the first video stream, or <see langword="null"/>
    /// if no valid video stream or resolution is found.
    /// </returns>
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
