using System.Text.Encodings.Web;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Normalizes ffprobe JSON formatting for stable comparisons and output
/// </summary>
internal static class FFProbeJsonFormatting
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Normalize(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, Options);
    }
}
