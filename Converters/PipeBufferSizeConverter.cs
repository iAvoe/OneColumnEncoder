using static OneColumnEncoder.Json.JsonElementHelper;
using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace OneColumnEncoder.Converters
{
    public class PipeBufferSizeConverter : IMultiValueConverter
    {
        private const int DefaultPipeBufferSizeKb = 80;
        private const int MinPipeBufferSizeKb = 80;
        private const int MaxPipeBufferSizeKb = 16 * 1024;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool optimize = values.Length > 0 && values[0] is bool enabled && enabled;
            string? sourceFfprobeJson = values.Length > 1 ? values[1]?.ToString() : null;
            return $"{CalculatePipeBufferSizeKb(optimize, sourceFfprobeJson)}KB";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object[] results = new object[targetTypes.Length];
            for (int index = 0; index < results.Length; index++)
                results[index] = Binding.DoNothing;

            return results;
        }

        public static int CalculatePipeBufferSizeKb(bool optimize, string? sourceFfprobeJson)
        {
            if (!optimize)
                return DefaultPipeBufferSizeKb;

            if (string.IsNullOrWhiteSpace(sourceFfprobeJson))
                return DefaultPipeBufferSizeKb;

            try
            {
                using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
                if (!document.RootElement.TryGetProperty("streams", out JsonElement streams) ||
                    streams.ValueKind != JsonValueKind.Array)
                    return DefaultPipeBufferSizeKb;

                foreach (JsonElement item in streams.EnumerateArray())
                {
                    string? codecType = TryGetString(item, "codec_type");
                    if (codecType is not (null or "video"))
                        continue;

                    if (!TryGetInt(item, "width", out int width) || width < 1
                        || !TryGetInt(item, "height", out int height) || height < 1)
                        return DefaultPipeBufferSizeKb;

                    string? pixelFormat = TryGetString(item, "pix_fmt");
                    double bpp = GetBytesPerPixel(pixelFormat);
                    double frameBytes = width * height * bpp;
                    double bufferRaw = frameBytes * 0.1;
                    int bufferKb = (int)Math.Round(bufferRaw / 1024d);
                    return Math.Clamp(bufferKb, MinPipeBufferSizeKb, MaxPipeBufferSizeKb);
                }

                return DefaultPipeBufferSizeKb;
            }
            catch
            {
                return DefaultPipeBufferSizeKb;
            }
        }

        private static double GetBytesPerPixel(string? pixelFormat)
        {
            if (string.IsNullOrWhiteSpace(pixelFormat)) return 1.5;

            string fmt = pixelFormat.ToLowerInvariant();

            int bitDepth = 8;
            if (fmt.Contains("10le") || fmt.Contains("10be")) bitDepth = 10;
            else if (fmt.Contains("12le") || fmt.Contains("12be")) bitDepth = 12;
            else if (fmt.Contains("14le") || fmt.Contains("14be")) bitDepth = 14;
            else if (fmt.Contains("16le") || fmt.Contains("16be")) bitDepth = 16;

            double bpc = bitDepth / 8.0;

            if (fmt.Contains("gray") || fmt.Contains("yuv400p")) return bpc;
            if (fmt.Contains("rgb") || fmt.Contains("gbr") || fmt.Contains("bgr") || fmt.Contains("444")) return 3 * bpc;
            if (fmt.Contains("422") || fmt.Contains("nv16")) return 2 * bpc;
            return 1.5 * bpc;
        }
    }
}
