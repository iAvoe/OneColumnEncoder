namespace OneColumnEncoder.Models;

public enum ColorSpaceStrategy
{
    Unknown,
    NativeBt709,
    LowToHigh,
    HighToLow,
    HdrToSdr,
    HighHdrToSdr
}

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
    public string? FfmpegColorFilter { get; init; }
    public string StrategyDisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public bool IsApplicable =>
        Strategy is ColorSpaceStrategy.LowToHigh
            or ColorSpaceStrategy.HighToLow
            or ColorSpaceStrategy.HdrToSdr
            or ColorSpaceStrategy.HighHdrToSdr;
}
