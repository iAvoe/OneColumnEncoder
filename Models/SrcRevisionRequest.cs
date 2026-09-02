namespace OneColumnEncoder.Models;

/// <summary>
/// Revise video source resolution, since ffprobe analyze result might be outdated after filtering,
/// which should update encoding parameter generation accordingly.
/// </summary>
/// <param name="Width">New video width</param>
/// <param name="Height">New video height</param>
public sealed record SrcRevisionRequest(int Width, int Height);
