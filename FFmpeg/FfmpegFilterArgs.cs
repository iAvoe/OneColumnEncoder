namespace OneColumnEncoder.FFmpeg;

public static class FfmpegFilterArgs
{
    public static string Build(bool includeSwsFlags, bool includeCsp709Flags, string? pixelFormat, params string?[] filters)
    {
        string filterChain = string.Join(",", filters.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
        if (string.IsNullOrWhiteSpace(filterChain)) return string.Empty;

        string filterArgs = filterChain.Contains(',', StringComparison.Ordinal)
            ? $"-filter:v \"{filterChain}\""
            : $"-filter:v {filterChain}";

        string csp709Flags = includeCsp709Flags
            ? " -color_primaries bt709 -color_trc bt709 -colorspace bt709"
            : string.Empty;

        string pixelFormatFlag = includeCsp709Flags && !string.IsNullOrWhiteSpace(pixelFormat)
            ? $" -pix_fmt {pixelFormat}"
            : string.Empty;

        string swsFlags = includeSwsFlags
            ? " -sws_flags bicubic+full_chroma_int+full_chroma_inp+accurate_rnd"
            : string.Empty;

        return $"{filterArgs}{csp709Flags}{pixelFormatFlag}{swsFlags}";
    }
}
