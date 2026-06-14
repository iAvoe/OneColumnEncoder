using System.Text.Json;
using OneColumnEncoder.Models;

namespace OneColumnEncoder.Helpers;

public static class ColorSpaceConverterH
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
            return CreateResult(null, null, null, ColorSpaceStrategy.Unknown);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(ffprobeJson);
            if (!FrameRateH.TryGetFirstVideoStream(doc.RootElement, out JsonElement stream))
                return CreateResult(null, null, null, ColorSpaceStrategy.Unknown, "No video stream found.");

            return Analyze(stream);
        }
        catch
        {
            return CreateResult(null, null, null, ColorSpaceStrategy.Unknown, "Failed to parse ffprobe JSON.");
        }
    }

    public static ColorSpaceAnalysisM Analyze(JsonElement stream)
    {
        string? primaries = Normalize(JsonElementHelper.TryGetString(stream, "color_primaries"));
        string? transfer = Normalize(JsonElementHelper.TryGetString(stream, "color_transfer"));
        string? matrix = Normalize(JsonElementHelper.TryGetString(stream, "color_space"));

        ColorSpaceStrategy strategy = Classify(primaries, transfer);

        return CreateResult(primaries, transfer, matrix, strategy);
    }

    public static ColorSpaceStrategy Classify(string? primaries, string? transfer)
    {
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

    public static bool IsStrategyApplicable(ColorSpaceStrategy strategy, string? primaries, string? transfer)
    {
        primaries = Normalize(primaries);
        transfer = Normalize(transfer);

        return strategy switch
        {
            ColorSpaceStrategy.LowToHigh => IsSdrNarrowGamut(primaries),
            ColorSpaceStrategy.HighToLow => IsWideGamut(primaries),
            ColorSpaceStrategy.HdrToSdr => IsHdrTransfer(transfer),
            ColorSpaceStrategy.HighHdrToSdr => IsHdrTransfer(transfer) && IsWideGamut(primaries),
            ColorSpaceStrategy.NativeBt709 => IsBt709(primaries),
            _ => false
        };
    }

    #endregion

    #region Filter chain generation

    public static string? BuildFfmpegFilter(ColorSpaceStrategy strategy)
    {
        return strategy switch
        {
            ColorSpaceStrategy.HdrToSdr or ColorSpaceStrategy.HighHdrToSdr
                => "zscale=transfer=linear,tonemap=hable:desat=3:peak=<nits>,zscale=transfer=bt709:matrix=bt709:primaries=bt709",
            ColorSpaceStrategy.LowToHigh or ColorSpaceStrategy.HighToLow
                => "zscale=matrix=bt709:primaries=bt709:transfer=bt709",
            _ => null
        };
    }

    #endregion

    #region Private helpers

    private static ColorSpaceAnalysisM CreateResult(
        string? primaries, string? transfer, string? matrix,
        ColorSpaceStrategy strategy, string? descriptionOverride = null)
    {
        return new ColorSpaceAnalysisM
        {
            ColorPrimaries = primaries,
            ColorTransfer = transfer,
            ColorMatrix = matrix,
            H273Primaries = primaries != null && H273Primaries.TryGetValue(primaries, out int pv) ? pv : null,
            H273Transfer = transfer != null && H273Transfer.TryGetValue(transfer, out int tv) ? tv : null,
            H273Matrix = matrix != null && H273Matrix.TryGetValue(matrix, out int mv) ? mv : null,
            Strategy = strategy,
            FfmpegColorFilter = BuildFfmpegFilter(strategy),
            StrategyDisplayName = GetDisplayName(strategy),
            Description = descriptionOverride ?? BuildDescription(strategy, primaries, transfer, matrix)
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
        transfer is "smpte2084" or "arib-std-b67" or "hlg";

    private static bool IsBt709(string? primaries) =>
        primaries == "bt709";

    private static bool IsSdrNarrowGamut(string? primaries) =>
        primaries is "bt470m" or "bt470bg" or "smpte170m" or "bt601"
            or "smpte240m" or "ebu3213";

    private static bool IsWideGamut(string? primaries) =>
        primaries is "bt2020" or "smpte431" or "smpte432" or "smpte428";

    private static string GetDisplayName(ColorSpaceStrategy strategy) => strategy switch
    {
        ColorSpaceStrategy.NativeBt709 => "N/A - 已是 bt709",
        ColorSpaceStrategy.LowToHigh => "低转高（SDR 窄色域 → bt709）",
        ColorSpaceStrategy.HighToLow => "高转低（WCG → bt709）",
        ColorSpaceStrategy.HdrToSdr => "HDR 转 SDR",
        ColorSpaceStrategy.HighHdrToSdr => "高 HDR 转低 SDR",
        _ => "UNKNOWN"
    };

    private static string BuildDescription(
        ColorSpaceStrategy strategy,
        string? primaries, string? transfer, string? matrix)
    {
        string pStr = primaries ?? "未指定";
        string tStr = transfer ?? "未指定";
        string mStr = matrix ?? "未指定";

        string classification = strategy switch
        {
            ColorSpaceStrategy.NativeBt709 => "源已是 bt709，无需色彩转换。",
            ColorSpaceStrategy.LowToHigh => $"源 {DescribeColorMeta(pStr, tStr, mStr)} 色域小于 bt709，执行低转高。",
            ColorSpaceStrategy.HighToLow => $"源 {DescribeColorMeta(pStr, tStr, mStr)} 色域大于 bt709，执行高转低。",
            ColorSpaceStrategy.HdrToSdr => $"源 {DescribeColorMeta(pStr, tStr, mStr)} 为 HDR 内容，执行 HDR→SDR 色调映射。",
            ColorSpaceStrategy.HighHdrToSdr => $"源 {DescribeColorMeta(pStr, tStr, mStr)} 为宽色域 HDR 内容，执行高 HDR→低 SDR 色调映射。",
            _ => "无法识别的色彩空间。"
        };

        string? filter = strategy switch
        {
            ColorSpaceStrategy.NativeBt709 => string.Empty,
            ColorSpaceStrategy.Unknown => "请手动检查源文件色彩元数据。",
            _ => BuildFfmpegFilter(strategy)
        };

        if (string.IsNullOrEmpty(filter))
            return classification;

        if (strategy is ColorSpaceStrategy.HdrToSdr or ColorSpaceStrategy.HighHdrToSdr)
            return $"{classification}\n请检查视频文件名或可靠元数据中的真实峰值亮度 nits，并替换 peak=<nits>。\n滤镜: {filter}";

        return $"{classification}\n滤镜: {filter}";
    }

    private static string DescribeColorMeta(string primaries, string transfer, string matrix) =>
        $"原色={primaries} 传输={transfer} 矩阵={matrix}";

    #endregion
}
