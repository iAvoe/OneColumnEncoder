using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OneColumnEncoder.Pipeline;

public sealed class KeyframeIndex
{
    private readonly long[] _frames;
    private readonly double[] _times;

    private KeyframeIndex(long[] frames, double[] times)
    {
        _frames = frames;
        _times = times;
    }

    public int Count => _frames.Length;

    public bool TryFindNearestBefore(long targetFrame, out long keyframeFrame, out double keyframeTime)
    {
        keyframeFrame = 0;
        keyframeTime = 0d;
        if (_frames.Length == 0) return false;

        int lo = 0;
        int hi = _frames.Length - 1;
        int best = -1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_frames[mid] <= targetFrame)
            {
                best = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (best < 0) return false;
        keyframeFrame = _frames[best];
        keyframeTime = _times[best];
        return true;
    }

    public static async Task<KeyframeIndex> BuildAsync(
        string ffprobePath,
        string filePath,
        CancellationToken token)
    {
        ProcessStartInfo psi = new()
        {
            FileName = ffprobePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        foreach (string arg in BuildArgs(filePath))
            psi.ArgumentList.Add(arg);

        using Process process = new() { StartInfo = psi, EnableRaisingEvents = true };
        process.Start();

        List<long> frames = [];
        List<double> times = [];
        long frameCount = 0;

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync(token).ConfigureAwait(false)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            bool isKey = parts.Length >= 2 && parts[1].Contains('K', StringComparison.Ordinal);
            if (isKey)
            {
                if (double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double ptsTime))
                {
                    frames.Add(frameCount);
                    times.Add(ptsTime);
                }
            }
            frameCount++;
        }

        await process.WaitForExitAsync(token).ConfigureAwait(false);
        string stderr = await process.StandardError.ReadToEndAsync(token).ConfigureAwait(false);
        if (process.ExitCode != 0 || frames.Count == 0)
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(stderr) ? "ffprobe keyframe scan failed." : stderr.Trim());

        return new KeyframeIndex([.. frames], [.. times]);
    }

    private static string[] BuildArgs(string filePath) =>
        [
            "-v", "error",
            "-select_streams", "v:0",
            "-show_packets",
            "-show_entries", "packet=pts_time,flags",
            "-of", "csv=p=0",
            filePath
        ];
}
