namespace OneColumnEncoder.FFmpeg;

public static class CropCalculator
{
    public static (int width, int height)? GetCropDimensions(string? pixelFormat, int width, int height, bool isProgressive)
    {
        if (width <= 0 || height <= 0)
            return null;

        int widthMod = GetWidthMod(pixelFormat);
        int heightMod = GetHeightMod(pixelFormat, isProgressive);

        if (widthMod <= 1 && heightMod <= 1)
            return null;

        int croppedWidth = width - width % widthMod;
        int croppedHeight = height - height % heightMod;

        if (croppedWidth <= 0 || croppedHeight <= 0)
            return null;

        return croppedWidth == width && croppedHeight == height
            ? null
            : (croppedWidth, croppedHeight);
    }

    public static int GetWidthMod(string? pixelFormat)
    {
        if (IsRgbLike(pixelFormat) || IsYv24Like(pixelFormat)) return 1;
        if (IsYv411Like(pixelFormat)) return 4;
        return 2;
    }

    public static int GetHeightMod(string? pixelFormat, bool isProgressive)
    {
        if (IsRgbLike(pixelFormat) || IsYv24Like(pixelFormat) || IsYuy2Like(pixelFormat) || IsYv16Like(pixelFormat))
            return isProgressive ? 1 : 2;

        if (IsYv411Like(pixelFormat))
            return isProgressive ? 1 : 2;

        if (IsYv12Like(pixelFormat))
            return isProgressive ? 2 : 4;

        return isProgressive ? 1 : 2;
    }

    private static bool IsRgbLike(string? pixelFormat) =>
        !string.IsNullOrWhiteSpace(pixelFormat)
        && (pixelFormat.Contains("rgb", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gbr", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("gray", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("444", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("400", StringComparison.OrdinalIgnoreCase));

    private static bool IsYuy2Like(string? pixelFormat) =>
        !string.IsNullOrWhiteSpace(pixelFormat)
        && (pixelFormat.Contains("422", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("yuy", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("uyvy", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv16", StringComparison.OrdinalIgnoreCase));

    private static bool IsYv12Like(string? pixelFormat) =>
        !string.IsNullOrWhiteSpace(pixelFormat)
        && (pixelFormat.Contains("420", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv12", StringComparison.OrdinalIgnoreCase));

    private static bool IsYv16Like(string? pixelFormat) =>
        !string.IsNullOrWhiteSpace(pixelFormat)
        && (pixelFormat.Contains("422", StringComparison.OrdinalIgnoreCase)
            || pixelFormat.Contains("nv16", StringComparison.OrdinalIgnoreCase));

    private static bool IsYv24Like(string? pixelFormat) =>
        !string.IsNullOrWhiteSpace(pixelFormat)
        && pixelFormat.Contains("444", StringComparison.OrdinalIgnoreCase);

    private static bool IsYv411Like(string? pixelFormat) =>
        !string.IsNullOrWhiteSpace(pixelFormat)
        && pixelFormat.Contains("411", StringComparison.OrdinalIgnoreCase);
}
