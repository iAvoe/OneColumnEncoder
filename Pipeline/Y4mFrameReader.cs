using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneColumnEncoder.Pipeline;

public static class Y4mFrameReader
{
    public static BitmapSource LoadFirstFrame(string path)
    {
        using FileStream stream = File.OpenRead(path);
        string header = ReadLineAscii(stream);
        Y4mFormat format = ParseHeader(header);

        string frameHeader = ReadLineAscii(stream);
        if (!frameHeader.StartsWith("FRAME", StringComparison.Ordinal))
            throw new InvalidDataException("Y4M frame header missing.");

        ushort[] yPlane = ReadPlane(stream, format.Width, format.Height, format.BitDepth);
        ushort[] uPlane;
        ushort[] vPlane;
        int chromaWidth = Math.Max(1, (format.Width + format.ChromaSubsampleX - 1) / format.ChromaSubsampleX);
        int chromaHeight = Math.Max(1, (format.Height + format.ChromaSubsampleY - 1) / format.ChromaSubsampleY);

        if (format.IsMonochrome)
        {
            int neutral = 1 << (format.BitDepth - 1);
            uPlane = Enumerable.Repeat((ushort)neutral, 1).ToArray();
            vPlane = uPlane;
        }
        else
        {
            uPlane = ReadPlane(stream, chromaWidth, chromaHeight, format.BitDepth);
            vPlane = ReadPlane(stream, chromaWidth, chromaHeight, format.BitDepth);
        }

        byte[] pixels = new byte[checked(format.Width * format.Height * 4)];
        for (int y = 0; y < format.Height; y++)
        {
            for (int x = 0; x < format.Width; x++)
            {
                int yValue = ScaleTo8Bit(yPlane[y * format.Width + x], format.BitDepth);
                int chromaIndex = format.IsMonochrome
                    ? 0
                    : (y / format.ChromaSubsampleY) * chromaWidth + (x / format.ChromaSubsampleX);
                int uValue = ScaleTo8Bit(uPlane[chromaIndex], format.BitDepth);
                int vValue = ScaleTo8Bit(vPlane[chromaIndex], format.BitDepth);

                WriteBgraPixel(pixels, (y * format.Width + x) * 4, yValue, uValue, vValue);
            }
        }

        BitmapSource bitmap = BitmapSource.Create(
            format.Width,
            format.Height,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            pixels,
            format.Width * 4);
        bitmap.Freeze();
        return bitmap;
    }

    private static Y4mFormat ParseHeader(string header)
    {
        string[] tokens = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0 || tokens[0] != "YUV4MPEG2")
            throw new InvalidDataException("Invalid Y4M header.");

        int width = 0;
        int height = 0;
        string chroma = "420";

        foreach (string token in tokens.Skip(1))
        {
            if (token.Length < 2) continue;
            if (token[0] == 'W') width = int.Parse(token[1..]);
            else if (token[0] == 'H') height = int.Parse(token[1..]);
            else if (token[0] == 'C') chroma = token[1..];
        }

        if (width <= 0 || height <= 0)
            throw new InvalidDataException("Y4M frame size missing.");

        string normalizedChroma = chroma.ToLowerInvariant();
        int bitDepth = ParseBitDepth(normalizedChroma);

        if (normalizedChroma.StartsWith("mono", StringComparison.Ordinal))
            return new(width, height, bitDepth, 1, 1, true);
        if (normalizedChroma.StartsWith("444", StringComparison.Ordinal))
            return new(width, height, bitDepth, 1, 1, false);
        if (normalizedChroma.StartsWith("422", StringComparison.Ordinal))
            return new(width, height, bitDepth, 2, 1, false);
        if (normalizedChroma.StartsWith("420", StringComparison.Ordinal))
            return new(width, height, bitDepth, 2, 2, false);

        throw new InvalidDataException($"Unsupported Y4M chroma format: {chroma}");
    }

    private static int ParseBitDepth(string chroma)
    {
        foreach (int bitDepth in new[] { 16, 14, 12, 10, 9 })
        {
            string text = bitDepth.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (chroma.Contains("p" + text, StringComparison.Ordinal) ||
                chroma.EndsWith(text, StringComparison.Ordinal))
                return bitDepth;
        }

        return 8;
    }

    private static ushort[] ReadPlane(Stream stream, int width, int height, int bitDepth)
    {
        int sampleCount = checked(width * height);
        ushort[] samples = new ushort[sampleCount];

        if (bitDepth <= 8)
        {
            byte[] bytes = new byte[sampleCount];
            stream.ReadExactly(bytes);
            for (int i = 0; i < bytes.Length; i++)
                samples[i] = bytes[i];
            return samples;
        }

        byte[] raw = new byte[checked(sampleCount * 2)];
        stream.ReadExactly(raw);
        for (int i = 0; i < sampleCount; i++)
            samples[i] = (ushort)(raw[i * 2] | (raw[i * 2 + 1] << 8));
        return samples;
    }

    private static string ReadLineAscii(Stream stream)
    {
        List<byte> bytes = [];
        while (true)
        {
            int value = stream.ReadByte();
            if (value < 0) throw new EndOfStreamException("Unexpected end of Y4M file.");
            if (value == '\n') break;
            if (value != '\r') bytes.Add((byte)value);
        }

        return Encoding.ASCII.GetString([.. bytes]);
    }

    private static int ScaleTo8Bit(ushort sample, int bitDepth)
    {
        if (bitDepth <= 8) return sample;
        int maxValue = (1 << bitDepth) - 1;
        return (sample * 255 + maxValue / 2) / maxValue;
    }

    private static void WriteBgraPixel(byte[] pixels, int offset, int yValue, int uValue, int vValue)
    {
        int c = yValue - 16;
        int d = uValue - 128;
        int e = vValue - 128;

        int r = ClampToByte((298 * c + 459 * e + 128) >> 8);
        int g = ClampToByte((298 * c - 55 * d - 136 * e + 128) >> 8);
        int b = ClampToByte((298 * c + 541 * d + 128) >> 8);

        pixels[offset] = (byte)b;
        pixels[offset + 1] = (byte)g;
        pixels[offset + 2] = (byte)r;
        pixels[offset + 3] = 255;
    }

    private static int ClampToByte(int value) => Math.Clamp(value, 0, 255);

    private readonly record struct Y4mFormat(
        int Width,
        int Height,
        int BitDepth,
        int ChromaSubsampleX,
        int ChromaSubsampleY,
        bool IsMonochrome);
}
