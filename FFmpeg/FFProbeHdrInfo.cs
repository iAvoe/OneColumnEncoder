using static OneColumnEncoder.Json.JsonElementHelper;

namespace OneColumnEncoder.FFmpeg;

public readonly record struct FFProbeHdrInfo(
    bool HasHdr10,
    bool HasHlg,
    bool HasDovi,
    string DoviProfile,
    string Summary,
    FFProbeMasteringDisplayMetadata? MasteringDisplay,
    FFProbeContentLightLevel? ContentLightLevel,
    string? ColorSpace,
    string? ColorTransfer,
    string? ColorPrimaries,
    string? ColorRange);

public readonly record struct FFProbeMasteringDisplayMetadata(
    HdrRational GreenX,
    HdrRational GreenY,
    HdrRational BlueX,
    HdrRational BlueY,
    HdrRational RedX,
    HdrRational RedY,
    HdrRational WhitePointX,
    HdrRational WhitePointY,
    HdrRational MinLuminance,
    HdrRational MaxLuminance)
{
    public string ToX265String() =>
        $"G({GreenX.ToScaledInt(50000)},{GreenY.ToScaledInt(50000)})" +
        $"B({BlueX.ToScaledInt(50000)},{BlueY.ToScaledInt(50000)})" +
        $"R({RedX.ToScaledInt(50000)},{RedY.ToScaledInt(50000)})" +
        $"WP({WhitePointX.ToScaledInt(50000)},{WhitePointY.ToScaledInt(50000)})" +
        $"L({MaxLuminance.ToScaledInt(10000)},{MinLuminance.ToScaledInt(10000)})";

    public string ToSvtAv1String() =>
        $"G({GreenX.ToDecimalString()},{GreenY.ToDecimalString()})" +
        $"B({BlueX.ToDecimalString()},{BlueY.ToDecimalString()})" +
        $"R({RedX.ToDecimalString()},{RedY.ToDecimalString()})" +
        $"WP({WhitePointX.ToDecimalString()},{WhitePointY.ToDecimalString()})" +
        $"L({MaxLuminance.ToDecimalString()},{MinLuminance.ToDecimalString()})";
}

public readonly record struct FFProbeContentLightLevel(long MaxContent, long MaxAverage)
{
    public bool HasValue => MaxContent > 0 || MaxAverage > 0;
}

public readonly record struct FFProbeDoviMetadata(int Profile, int CompatibilityId)
{
    public string ProfileText => CompatibilityId > 0 ? $"P{Profile}.{CompatibilityId}" : $"P{Profile}";
}

public readonly record struct HdrRational(long Numerator, long Denominator)
{
    public double ToDouble() => Denominator > 0 ? (double)Numerator / Denominator : 0d;

    public long ToScaledInt(int scale) => Denominator > 0
        ? (long)Math.Round(ToDouble() * scale, MidpointRounding.AwayFromZero)
        : 0L;

    public string ToDecimalString() => ToDouble().ToString("0.####", CultureInfo.InvariantCulture);

    public static bool TryParse(string? text, out HdrRational value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        string trimmed = text.Trim();
        string[] parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2
            && long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long numerator)
            && long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long denominator)
            && denominator > 0)
        {
            value = new(numerator, denominator);
            return true;
        }

        if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integerValue))
        {
            value = new(integerValue, 1);
            return true;
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
        {
            const long scaleDenominator = 1000000L;
            value = new((long)Math.Round(doubleValue * scaleDenominator, MidpointRounding.AwayFromZero), scaleDenominator);
            return true;
        }

        return false;
    }
}

public static class FFProbeHdrInfoReader
{
    public static FFProbeHdrInfo Read(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return default;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            return Read(document.RootElement);
        }
        catch
        {
            return default;
        }
    }

    public static FFProbeHdrInfo Read(JsonElement root)
    {
        if (!FrameRate.TryGetFirstVideoStream(root, out JsonElement stream))
            return default;

        string? colorSpace = Normalize(TryGetString(stream, "color_space"));
        string? colorTransfer = Normalize(TryGetString(stream, "color_transfer"));
        string? colorPrimaries = Normalize(TryGetString(stream, "color_primaries"));
        string? colorRange = Normalize(TryGetString(stream, "color_range"));

        FFProbeDoviMetadata? dovi = TryReadDovi(stream);
        FFProbeMasteringDisplayMetadata? masteringDisplay = TryReadMasteringDisplay(root);
        FFProbeContentLightLevel? contentLightLevel = TryReadContentLightLevel(root);

        bool hasHdr10 = masteringDisplay != null
            || colorTransfer is "smpte2084";
        bool hasHlg = colorTransfer is "arib-std-b67" or "hlg";
        bool hasDovi = dovi != null;
        string doviProfile = dovi?.ProfileText ?? string.Empty;

        string summary = BuildSummary(hasHdr10, hasHlg, hasDovi, doviProfile, masteringDisplay, contentLightLevel);

        return new(
            hasHdr10,
            hasHlg,
            hasDovi,
            doviProfile,
            summary,
            masteringDisplay,
            contentLightLevel,
            colorSpace,
            colorTransfer,
            colorPrimaries,
            colorRange);
    }

    public static string ToX265MasterDisplay(FFProbeMasteringDisplayMetadata metadata) => metadata.ToX265String();

    public static string ToX264MasteringDisplay(FFProbeMasteringDisplayMetadata metadata) => metadata.ToX265String();

    public static string ToSvtAv1MasteringDisplay(FFProbeMasteringDisplayMetadata metadata) => metadata.ToSvtAv1String();

    public static string ToX264ContentLight(FFProbeContentLightLevel contentLightLevel) =>
        $"{contentLightLevel.MaxContent},{contentLightLevel.MaxAverage}";

    public static string ToX265ContentLight(FFProbeContentLightLevel contentLightLevel) =>
        $"{contentLightLevel.MaxContent},{contentLightLevel.MaxAverage}";

    public static string ToSvtAv1ContentLight(FFProbeContentLightLevel contentLightLevel) =>
        $"{contentLightLevel.MaxContent},{contentLightLevel.MaxAverage}";

    private static FFProbeDoviMetadata? TryReadDovi(JsonElement stream)
    {
        if (!TryFindSideData(stream, "DOVI configuration record", out JsonElement sideData))
            return null;

        if (!TryGetInt(sideData, "dv_profile", out int profile))
            return null;

        int compatibilityId = TryGetInt(sideData, "dv_bl_signal_compatibility_id", out int compatibility)
            ? compatibility
            : 0;

        return new(profile, compatibilityId);
    }

    private static FFProbeMasteringDisplayMetadata? TryReadMasteringDisplay(JsonElement root)
    {
        if (!TryFindSideData(root, "Mastering display metadata", out JsonElement sideData))
            return null;

        if (!TryReadRational(sideData, "red_x", out HdrRational redX)
            || !TryReadRational(sideData, "red_y", out HdrRational redY)
            || !TryReadRational(sideData, "green_x", out HdrRational greenX)
            || !TryReadRational(sideData, "green_y", out HdrRational greenY)
            || !TryReadRational(sideData, "blue_x", out HdrRational blueX)
            || !TryReadRational(sideData, "blue_y", out HdrRational blueY)
            || !TryReadRational(sideData, "white_point_x", out HdrRational whitePointX)
            || !TryReadRational(sideData, "white_point_y", out HdrRational whitePointY)
            || !TryReadRational(sideData, "min_luminance", out HdrRational minLuminance)
            || !TryReadRational(sideData, "max_luminance", out HdrRational maxLuminance))
        {
            return null;
        }

        return new(
            greenX,
            greenY,
            blueX,
            blueY,
            redX,
            redY,
            whitePointX,
            whitePointY,
            minLuminance,
            maxLuminance);
    }

    private static FFProbeContentLightLevel? TryReadContentLightLevel(JsonElement root)
    {
        if (!TryFindSideData(root, "Content light level metadata", out JsonElement sideData))
            return null;

        long maxContent = TryGetLongValue(sideData, "max_content") ?? 0L;
        long maxAverage = TryGetLongValue(sideData, "max_average") ?? 0L;
        return maxContent > 0 || maxAverage > 0
            ? new(maxContent, maxAverage)
            : null;
    }

    private static bool TryFindSideData(JsonElement root, string sideDataType, out JsonElement sideData)
    {
        if (TryFindSideDataInElement(root, sideDataType, out sideData))
            return true;

        if (TryGetFirstVideoFrame(root, out JsonElement frame)
            && TryFindSideDataInElement(frame, sideDataType, out sideData))
        {
            return true;
        }

        sideData = default;
        return false;
    }

    private static bool TryFindSideDataInElement(JsonElement element, string sideDataType, out JsonElement sideData)
    {
        if (!element.TryGetProperty("side_data_list", out JsonElement list)
            || list.ValueKind != JsonValueKind.Array)
        {
            sideData = default;
            return false;
        }

        foreach (JsonElement entry in list.EnumerateArray())
        {
            if (!string.Equals(TryGetString(entry, "side_data_type"), sideDataType, StringComparison.OrdinalIgnoreCase))
                continue;

            sideData = entry;
            return true;
        }

        sideData = default;
        return false;
    }

    private static bool TryGetFirstVideoFrame(JsonElement root, out JsonElement frame)
    {
        if (root.TryGetProperty("frames", out JsonElement frames)
            && frames.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement candidate in frames.EnumerateArray())
            {
                string? mediaType = Normalize(TryGetString(candidate, "media_type"));
                if (!string.IsNullOrWhiteSpace(mediaType) && !string.Equals(mediaType, "video", StringComparison.OrdinalIgnoreCase))
                    continue;

                frame = candidate;
                return true;
            }
        }

        frame = default;
        return false;
    }

    private static bool TryReadRational(JsonElement element, string propertyName, out HdrRational value)
    {
        value = default;
        string? text = TryGetString(element, propertyName);
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return HdrRational.TryParse(text, out value);
    }

    private static long? TryGetLongValue(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)) return null;
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out long number)) return number;
        return long.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
            ? parsed
            : null;
    }

    private static string BuildSummary(
        bool hasHdr10,
        bool hasHlg,
        bool hasDovi,
        string doviProfile,
        FFProbeMasteringDisplayMetadata? masteringDisplay,
        FFProbeContentLightLevel? contentLightLevel)
    {
        List<string> parts = [];

        if (!hasHdr10 && !hasHlg && !hasDovi)
            return string.Empty;

        if (hasHdr10) parts.Add("HDR10");
        else if (hasHlg) parts.Add("HLG");
        else if (hasDovi)
            parts.Add(string.IsNullOrWhiteSpace(doviProfile) ? "Dolby Vision" : $"Dolby Vision {doviProfile}");

        if (hasHdr10 && hasDovi && !string.IsNullOrWhiteSpace(doviProfile))
            parts.Add($"Dolby Vision {doviProfile}");

        if (masteringDisplay is { } mastering)
            parts.Add($"{mastering.MaxLuminance.ToDouble().ToString("0.###", CultureInfo.InvariantCulture)} nits");

        if (contentLightLevel is { HasValue: true } cll)
            parts.Add($"CLL {cll.MaxContent}/{cll.MaxAverage}");

        if (hasDovi && !hasHdr10 && masteringDisplay is null)
            parts.Add("no HDR10 base");

        return string.Join(" · ", parts);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
