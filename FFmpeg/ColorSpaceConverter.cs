using OneColumnEncoder.Models.Analysis;
using static OneColumnEncoder.Models.JsonProviderM;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Maps ffprobe color metadata to color space analysis and ffmpeg filters
/// </summary>
public static class ColorSpaceConverter
{
    #region H.273 mapping tables

    public static readonly IReadOnlyDictionary<string, int> H273Primaries = new Dictionary<string, int>
    {
        ["bt709"] = 1,
        ["unknown"] = 2,
        ["unspec"] = 2,
        ["bt470m"] = 4,
        ["bt470bg"] = 5,
        ["bt601"] = 6,
        ["smpte170m"] = 6,
        ["smpte240m"] = 7,
        ["film"] = 8,
        ["bt2020"] = 9,
        ["smpte428"] = 10,
        ["smpte431"] = 11,
        ["smpte432"] = 12,
        ["ebu3213"] = 22,
    };

    public static readonly IReadOnlyDictionary<string, int> H273Transfer = new Dictionary<string, int>
    {
        ["bt709"] = 1,
        ["unknown"] = 2,
        ["bt470m"] = 4,
        ["bt470bg"] = 5,
        ["bt601"] = 6,
        ["smpte170m"] = 6,
        ["smpte240m"] = 7,
        ["linear"] = 8,
        ["log100"] = 9,
        ["log100_sqrt10"] = 10,
        ["iec61966-2-4"] = 11,
        ["iec61966-2-1"] = 13,
        ["bt2020-10"] = 14,
        ["bt2020-12"] = 15,
        ["smpte2084"] = 16,
        ["smpte428"] = 17,
        ["hlg"] = 18,
        ["arib-std-b67"] = 18,
    };

    public static readonly IReadOnlyDictionary<string, int> H273Matrix = new Dictionary<string, int>
    {
        ["bt709"] = 1,
        ["unknown"] = 2,
        ["unspec"] = 2,
        ["fcc"] = 4,
        ["bt470bg"] = 5,
        ["bt601"] = 6,
        ["smpte170m"] = 6,
        ["smpte240m"] = 7,
        ["ycgco"] = 8,
        ["bt2020nc"] = 9,
        ["bt2020-ncl"] = 9,
        ["bt2020cl"] = 10,
        ["bt2020-cl"] = 10,
        ["smpte2085"] = 11,
        ["chroma-derived-nc"] = 12,
        ["chroma-derived-c"] = 13,
        ["ictcp"] = 14,
    };

    #endregion

    #region Public API

    public static ColorSpaceAnalysisM Analyze(string? ffprobeJson)
    {
        if (string.IsNullOrWhiteSpace(ffprobeJson))
            return CreateResult(null, null, null, null, null, ColorSpaceStrategy.Unknown);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(ffprobeJson);
            return AnalyzeRoot(doc.RootElement);
        }
        catch
        {
            return CreateResult(null, null, null, null, null, ColorSpaceStrategy.Unknown, FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.FailedToParse"]);
        }
    }

    public static ColorSpaceAnalysisM AnalyzeRoot(JsonElement root)
    {
        if (!FrameRate.TryGetFirstVideoStream(root, out JsonElement stream))
            return CreateResult(null, null, null, null, null, ColorSpaceStrategy.Unknown, FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.NoVideoStream"]);

        return Analyze(stream);
    }

    public static ColorSpaceAnalysisM Analyze(JsonElement stream)
    {
        string? primaries = Normalize(TryGetString(stream, "color_primaries"));
        string? transfer = Normalize(TryGetString(stream, "color_transfer"));
        string? matrix = Normalize(TryGetString(stream, "color_space"));
        string? chromaLocation = Normalize(TryGetString(stream, "chroma_location"));
        string? pixelFormat = Normalize(TryGetString(stream, "pix_fmt"));
        decimal? frameRate = ReadFrameRate(stream);

        bool hasDolbyVision = HasDolbyVisionMetadata(stream);
        ColorSpaceStrategy strategy = Classify(primaries, transfer, hasDolbyVision);

        return CreateResult(primaries, transfer, matrix, chromaLocation, pixelFormat, strategy,
            frameRate: frameRate, hasDolbyVision: hasDolbyVision);
    }

    public static ColorSpaceStrategy Classify(
        string? primaries,
        string? transfer,
        bool hasDolbyVision = false)
    {
        if (hasDolbyVision)
            return IsHlgTransfer(transfer) || IsHdrTransfer(transfer)
                ? ColorSpaceStrategy.DoviHdrToSdr
                : ColorSpaceStrategy.DoviSdrTo709;

        if (IsHlgTransfer(transfer))
            return ColorSpaceStrategy.HlgToSdr;

        if (IsHdrTransfer(transfer))
            return IsWideGamut(primaries)
                ? ColorSpaceStrategy.HighHdrToSdr
                : ColorSpaceStrategy.HdrToSdr;

        if (primaries == null || !IsKnown(primaries))
            return ColorSpaceStrategy.Unknown;

        if (IsBt709(primaries))
            return ColorSpaceStrategy.NativeBt709;

        if (IsSdrNarrowGamut(primaries))
            return ColorSpaceStrategy.LowToHigh;

        if (IsWideGamut(primaries))
            return ColorSpaceStrategy.HighToLow;

        return ColorSpaceStrategy.Unknown;
    }

    public static bool IsStrategyApplicable(
        ColorSpaceStrategy strategy,
        string? primaries,
        string? transfer,
        bool hasDolbyVision = false)
    {
        primaries = Normalize(primaries);
        transfer = Normalize(transfer);

        // Known Bt601 (narrow gamut) source: only the LowToHigh conversion makes sense,
        // every WCG / HDR / HLG / DoVi strategy must report N/A
        if (IsSdrNarrowGamut(primaries))
            return strategy == ColorSpaceStrategy.LowToHigh;

        return strategy switch
        {
            ColorSpaceStrategy.LowToHigh => IsSdrNarrowGamut(primaries),
            ColorSpaceStrategy.HighToLow => IsWideGamut(primaries),
            ColorSpaceStrategy.HdrToSdr => IsHdrTransfer(transfer),
            ColorSpaceStrategy.HighHdrToSdr => IsHdrTransfer(transfer) && IsWideGamut(primaries),
            ColorSpaceStrategy.HlgToSdr => IsHlgTransfer(transfer),
            ColorSpaceStrategy.DoviSdrTo709 => hasDolbyVision,
            ColorSpaceStrategy.DoviHdrToSdr => hasDolbyVision,
            ColorSpaceStrategy.NativeBt709 => IsBt709(primaries),
            _ => false
        };
    }

    #endregion

    #region Filter chain generation

    public static string? BuildVapourSynthFilter(
        ColorSpaceStrategy strategy,
        string? transfer = null) // Currently uneeded: string? primaries = null
    {
        // Determine source color space type
        int srcCsp = GetVapourSynthSourceCsp(transfer);

        return strategy switch
        {
            ColorSpaceStrategy.LowToHigh => null, // VapourSynth placebo does not support Bt601
            ColorSpaceStrategy.HighToLow => BuildVapourSynthTonemapFilter(srcCsp, 0, 3),
            ColorSpaceStrategy.HdrToSdr => BuildVapourSynthTonemapFilter(srcCsp, 0, 3, "<nits>", "100", "spline"),
            ColorSpaceStrategy.HighHdrToSdr => BuildVapourSynthTonemapFilter(srcCsp, 0, 3, "<nits>", "100", "spline", 1),
            ColorSpaceStrategy.HlgToSdr => BuildVapourSynthTonemapFilter(2, 0, 3, "<nits>", "100", "spline"),
            ColorSpaceStrategy.DoviSdrTo709 => BuildVapourSynthTonemapFilter(3, 0, 3),
            ColorSpaceStrategy.DoviHdrToSdr => BuildVapourSynthTonemapFilter(3, 0, 3, "<nits>", "100", "spline"),
            _ => null
        };
    }

    /// <summary>
    /// Create various types of AviSynth filters
    /// </summary>
    /// <param name="strategy">Type of gamut or HDR to SDR</param>
    /// <param name="transfer">Transfer characteristics string</param>
    /// <param name="primaries">ffprobe color_primaries, used to tell a Bt601 source is NTSC or PAL</param>
    /// <param name="matrix">ffprobe color_space, used to tell a Bt601 source is NTSC or PAL</param>
    /// <param name="framerate">Fallback used to tell a Bt601 source is NTSC or PAL when metadata is ambiguous</param>
    /// <returns>Filter string, or <see cref="LangProviderBase.NAText"/> when a Bt601 source is neither NTSC nor PAL</returns>
    public static string? BuildAviSynthFilter(
        ColorSpaceStrategy strategy,
        string? transfer = null,
        string? primaries = null,
        string? matrix = null,
        decimal? framerate = null)
    {
        // Determine source color space type
        string srcCsp = GetAviSynthSourceCsp(transfer);

        return strategy switch
        {
            ColorSpaceStrategy.LowToHigh => BuildAvsBt601Filter(primaries, matrix, framerate),
            ColorSpaceStrategy.HighToLow => BuildAvsPlaceboFilter(srcCsp, "sdr", "709"),
            ColorSpaceStrategy.HdrToSdr => BuildAvsPlaceboFilter(srcCsp, "sdr", "709", "<nits>", "100", "spline"),
            ColorSpaceStrategy.HighHdrToSdr => BuildAvsPlaceboFilter(srcCsp, "sdr", "709", "<nits>", "100", "spline", "perceptual"),
            ColorSpaceStrategy.HlgToSdr => BuildAvsPlaceboFilter("hlg", "sdr", "709", "<nits>", "100", "spline"),
            ColorSpaceStrategy.DoviSdrTo709 => BuildAvsPlaceboFilter("dovi", "sdr", "709"),
            ColorSpaceStrategy.DoviHdrToSdr => BuildAvsPlaceboFilter("dovi", "sdr", "709", "<nits>", "100", "spline"),
            _ => null
        };
    }

    /// <summary>
    /// Build the Bt601 (601_525 NTSC / 601_625 PAL) source preset filter
    /// </summary>
    /// <returns>Filter with the Bt601 preset in src_csp, or <see cref="LangProviderBase.NAText"/> when the standard cannot be determined</returns>
    private static string? BuildAvsBt601Filter(string? primaries, string? matrix, decimal? framerate)
    {
        string? standard = ResolveBt601Standard(primaries, matrix, framerate);
        return standard == null
            ? LangProviderBase.NAText
            : BuildAvsPlaceboFilter(standard, "sdr", "709");
    }

    /// <summary>
    /// Tell PAL/SECAM (BT.601-625) vs NTSC/film (BT.601-525), preferring ffprobe metadata
    /// </summary>
    /// <returns>"pal" / "ntsc", or null when neither standard can be determined</returns>
    private static string? ResolveBt601Standard(string? primaries, string? matrix, decimal? framerate) =>
        ClassifyBt601Tag(primaries)
        ?? ClassifyBt601Tag(matrix)
        ?? ClassifyBt601Framerate(framerate);

    /// <summary>
    /// Map an ffprobe color_primaries / color_space value to the Bt601 standard it belongs to
    /// </summary>
    /// <remarks>
    /// Values describing neither standard (bt601, unknown, unspec, ...) stay ambiguous and go to the framerate fallback
    /// </remarks>
    private static string? ClassifyBt601Tag(string? tag) => Normalize(tag) switch
    {
        "bt470bg" or "ebu3213" => "pal",
        "bt470m" or "smpte170m" or "smpte240m" or "fcc" => "ntsc",
        _ => null
    };

    /// <summary>
    /// Judge by framerate to tell PAL/SECAM (BT.601-625) vs NTSC/film (BT.601-525)
    /// </summary>
    /// <remarks>
    /// 25 / 50 / 100 → PAL, 24p / 30p / 48p / 60p / 120p and the NTSC drop-frame rates → NTSC, else → neither
    /// </remarks>
    private static string? ClassifyBt601Framerate(decimal? framerate)
    {
        if (framerate is not > 0) return null;
        double fps = (double)framerate.Value;

        if (IsNearRate(fps, PalRates)) return "pal";
        if (IsNearRate(fps, NtscRates)) return "ntsc";
        return null;
    }

    private static readonly double[] PalRates = [25, 50, 100];

    private static readonly double[] NtscRates = [23.976, 24, 29.97, 30, 47.952, 48, 59.94, 60, 119.88, 120];

    private const double FramerateTolerance = 0.1;

    private static bool IsNearRate(double fps, double[] rates) =>
        rates.Any(rate => Math.Abs(fps - rate) < FramerateTolerance);

    private static decimal? ReadFrameRate(JsonElement stream)
    {
        (int num, int den)? rate = FrameRate.GetRFrameRate(stream) ?? FrameRate.GetAvgFrameRate(stream);
        return rate is { num: > 0, den: > 0 } ? (decimal)rate.Value.num / rate.Value.den : null;
    }

    public static string? BuildFFmpegFilter(ColorSpaceStrategy strategy) => strategy switch
    {
        ColorSpaceStrategy.LowToHigh
            or ColorSpaceStrategy.HighToLow
            or ColorSpaceStrategy.DoviSdrTo709 => FFmpegToBt709,
        ColorSpaceStrategy.HdrToSdr
            or ColorSpaceStrategy.HlgToSdr
            or ColorSpaceStrategy.DoviHdrToSdr => FFmpegHdrToSdr,
        ColorSpaceStrategy.HighHdrToSdr => FFmpegHdrWcgToSdr,
        _ => null
    };

    // Tone mapping peak is taken from the source metadata, unlike AviSynth/VapourSynth which expose it as <nits>
    private const string FFmpegToBt709 = "libplacebo=color_primaries=bt709:color_trc=bt709:colorspace=bt709";

    private const string FFmpegHdrToSdr = FFmpegToBt709 + ":tonemapping=spline";

    private const string FFmpegHdrWcgToSdr = FFmpegHdrToSdr + ":gamut_mode=perceptual";

    #endregion

    #region Private helpers

    private static ColorSpaceAnalysisM CreateResult(
        string? primaries, string? transfer, string? matrix, string? chromaLocation, string? pixelFormat,
        ColorSpaceStrategy strategy, string? descriptionOverride = null,
        bool hasDolbyVision = false, decimal? frameRate = null)
    {
        return new ColorSpaceAnalysisM
        {
            ColorPrimaries = primaries,
            ColorTransfer = transfer,
            ColorMatrix = matrix,
            ColorChromaLocation = chromaLocation,
            PixelFormat = pixelFormat,
            HasDolbyVision = hasDolbyVision,
            H273Primaries = primaries != null && H273Primaries.TryGetValue(primaries, out int pv) ? pv : null,
            H273Transfer = transfer != null && H273Transfer.TryGetValue(transfer, out int tv) ? tv : null,
            H273Matrix = matrix != null && H273Matrix.TryGetValue(matrix, out int mv) ? mv : null,
            Strategy = strategy,
            FrameRate = frameRate,
            FFmpegColorFilter = BuildFFmpegFilter(strategy),
            VapourSynthColorFilter = BuildVapourSynthFilter(strategy, transfer),
            AviSynthColorFilter = BuildAviSynthFilter(strategy, transfer, primaries, matrix, frameRate),
            StrategyDisplayName = GetDisplayName(strategy),
            Description = descriptionOverride ?? BuildDescription(strategy, primaries, transfer, matrix, chromaLocation, pixelFormat)
        };
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static bool IsKnown(string? value) =>
        value != null
        && value != "unknown"
        && value != "unspec"
        && value != "unspecified"
        && value != "reserved";

    private static bool IsHdrTransfer(string? transfer) =>
        transfer == "smpte2084";

    private static bool IsHlgTransfer(string? transfer) =>
        transfer is "arib-std-b67" or "hlg";

    private static bool IsBt709(string? primaries) =>
        primaries == "bt709";

    private static bool IsSdrNarrowGamut(string? primaries) =>
        primaries is "bt470m" or "bt470bg" or "smpte170m" or "bt601"
            or "smpte240m" or "ebu3213";

    private static bool IsWideGamut(string? primaries) =>
        primaries is "bt2020" or "smpte431" or "smpte432" or "smpte428";

    private static int GetVapourSynthSourceCsp(string? transfer) =>
        IsHdrTransfer(transfer) ? 1 : IsHlgTransfer(transfer) ? 2 : 0;

    private static string GetAviSynthSourceCsp(string? transfer) =>
        IsHdrTransfer(transfer) ? "hdr10" : IsHlgTransfer(transfer) ? "hlg" : "sdr";

    private static string BuildVapourSynthTonemapFilter(
        int srcCsp,
        int dstCsp,
        int dstPrim,
        string? srcMax = null,
        string? dstMax = null,
        string? toneMappingFunction = null,
        int? gamutMapping = null)
    {
        var parts = new List<string>
        {
            $"src_csp={srcCsp}",
            $"dst_csp={dstCsp}",
            $"dst_prim={dstPrim}"
        };

        if (!string.IsNullOrWhiteSpace(srcMax))
            parts.Add($"src_max={srcMax}");

        if (!string.IsNullOrWhiteSpace(dstMax))
            parts.Add($"dst_max={dstMax}");

        if (!string.IsNullOrWhiteSpace(toneMappingFunction))
            parts.Add($"tone_mapping_function_s=\"{toneMappingFunction}\"");

        if (gamutMapping.HasValue)
            parts.Add($"gamut_mapping={gamutMapping.Value}");

        return $"src = core.placebo.Tonemap(src, {string.Join(", ", parts)})";
    }

    private static bool HasDolbyVisionMetadata(JsonElement stream)
    {
        if (!stream.TryGetProperty("side_data_list", out JsonElement sideDataList)
            || sideDataList.ValueKind != JsonValueKind.Array)
            return false;

        return sideDataList.EnumerateArray().Any(entry =>
            string.Equals(TryGetString(entry, "side_data_type"),
                "DOVI configuration record", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildAvsPlaceboFilter(
        string srcCsp,
        string dstCsp,
        string dstPrim,
        string? srcMax = null,
        string? dstMax = null,
        string? toneMappingFunction = null,
        string? gamutMapping = null)
    {
        var parts = new List<string>
        {
            $"src_csp=\"{srcCsp}\"",
            $"dst_csp=\"{dstCsp}\"",
            $"dst_prim=\"{dstPrim}\""
        };

        if (!string.IsNullOrWhiteSpace(srcMax))
            parts.Add($"src_max={srcMax}");

        if (!string.IsNullOrWhiteSpace(dstMax))
            parts.Add($"dst_max={dstMax}");

        if (!string.IsNullOrWhiteSpace(toneMappingFunction))
            parts.Add($"tone_mapping_function=\"{toneMappingFunction}\"");

        if (!string.IsNullOrWhiteSpace(gamutMapping))
            parts.Add($"gamut_mapping=\"{gamutMapping}\"");

        return $"libplacebo_Render({string.Join(", ", parts)})";
    }

    private static string GetDisplayName(ColorSpaceStrategy strategy) => strategy switch
    {
        ColorSpaceStrategy.NativeBt709 => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DisplayNativeBt709"],
        ColorSpaceStrategy.LowToHigh => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DisplayLowToHigh"],
        ColorSpaceStrategy.HighToLow => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DisplayHighToLow"],
        ColorSpaceStrategy.HdrToSdr => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DisplayHdrToSdr"],
        ColorSpaceStrategy.HighHdrToSdr => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DisplayHighHdrToSdr"],
        ColorSpaceStrategy.HlgToSdr => "HLG to Bt.709",
        ColorSpaceStrategy.DoviSdrTo709 => "Dolby Vision SDR to Bt.709",
        ColorSpaceStrategy.DoviHdrToSdr => "Dolby Vision HDR to SDR Bt.709",
        _ => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DisplayUnknown"]
    };

    private static string BuildDescription(
        ColorSpaceStrategy strategy,
        string? primaries, string? transfer, string? matrix, string? chromaLocation, string? pixelFormat)
    {
        string def = FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DefaultNullValue"];
        string pStr = primaries ?? def;
        string tStr = transfer ?? def;
        string mStr = matrix ?? def;
        string cStr = chromaLocation ?? def;
        string pfStr = pixelFormat ?? def;

        string colorMeta = DescribeColorMeta(pStr, tStr, mStr, cStr, pfStr);

        string classification = strategy switch
        {
            ColorSpaceStrategy.NativeBt709 => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescNativeBt709"],
            ColorSpaceStrategy.LowToHigh => string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescLowToHigh"], colorMeta),
            ColorSpaceStrategy.HighToLow => string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescHighToLow"], colorMeta),
            ColorSpaceStrategy.HdrToSdr => string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescHdrToSdr"], colorMeta),
            ColorSpaceStrategy.HighHdrToSdr => string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescHighHdrToSdr"], colorMeta),
            ColorSpaceStrategy.HlgToSdr => $"Source {colorMeta} is HLG content, performing HLG to Bt.709 tone mapping.",
            ColorSpaceStrategy.DoviSdrTo709 => $"Source {colorMeta} is Dolby Vision SDR content, mapping to Bt.709.",
            ColorSpaceStrategy.DoviHdrToSdr => $"Source {colorMeta} is Dolby Vision HDR content, performing HDR to SDR tone mapping.",
            _ => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescUnknown"]
        };

        string? filter = strategy switch
        {
            ColorSpaceStrategy.NativeBt709 => string.Empty,
            ColorSpaceStrategy.Unknown => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.UnknownFilterHint"],
            _ => BuildFFmpegFilter(strategy)
        };

        if (string.IsNullOrEmpty(filter))
            return classification;

        string filterLine = string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.FilterLine"], filter);

        if (strategy is ColorSpaceStrategy.HdrToSdr
            or ColorSpaceStrategy.HighHdrToSdr
            or ColorSpaceStrategy.HlgToSdr
            or ColorSpaceStrategy.DoviHdrToSdr)
        {
            string hdrHint = FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.HdrHint"];
            return $"{classification}\n{hdrHint}{filterLine}";
        }

        return $"{classification}{filterLine}";
    }

    private static string DescribeColorMeta(string primaries, string transfer, string matrix, string chromaLocation, string pixelFormat) =>
        string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ColorSpace.DescribeColorMeta"], primaries, transfer, matrix, chromaLocation, pixelFormat);

    #endregion
}
