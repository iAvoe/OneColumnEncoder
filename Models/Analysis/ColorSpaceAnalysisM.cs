namespace OneColumnEncoder.Models.Analysis;

/// <summary>
/// Color-space strategy used for source conversion.
/// </summary>
public enum ColorSpaceStrategy
{
    Unknown,
    NativeBt709,
    LowToHigh,
    HighToLow,
    HdrToSdr,
    HighHdrToSdr
}

/// <summary>
/// Resolved color-space analysis and conversion metadata.
/// </summary>
public class ColorSpaceAnalysisM
{
    public string? ColorPrimaries { get; init; }
    public string? ColorTransfer { get; init; }
    public string? ColorMatrix { get; init; }
    public string? ColorChromaLocation { get; init; }
    public string? PixelFormat { get; init; }

    public int? H273Primaries { get; init; }
    public int? H273Transfer { get; init; }
    public int? H273Matrix { get; init; }

    public ColorSpaceStrategy Strategy { get; init; }
    public string? FFmpegColorFilter { get; init; }
    public string StrategyDisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public bool IsApplicable =>
        Strategy is ColorSpaceStrategy.LowToHigh
            or ColorSpaceStrategy.HighToLow
            or ColorSpaceStrategy.HdrToSdr
            or ColorSpaceStrategy.HighHdrToSdr;
}
