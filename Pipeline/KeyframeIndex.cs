using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OneColumnEncoder.Pipeline;

public sealed class KeyframeIndex
{
    private readonly double[] _times;

    private KeyframeIndex(double[] times)
    {
        _times = times;
    }

    public int Count => _times.Length;
    public double FirstTime => _times[0];

    public bool TryFindNearestBefore(double targetTime, out double keyframeTime)
    {
        keyframeTime = 0d;
        if (_times.Length == 0) return false;

        int lo = 0;
        int hi = _times.Length - 1;
        int best = -1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_times[mid] <= targetTime)
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

        List<double> times = [];

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync(token).ConfigureAwait(false)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string value = line.Split(',', 2)[0].Trim();
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double ptsTime))
                times.Add(ptsTime);
        }

        await process.WaitForExitAsync(token).ConfigureAwait(false);
        string stderr = await process.StandardError.ReadToEndAsync(token).ConfigureAwait(false);
        if (process.ExitCode != 0 || times.Count == 0)
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(stderr) ? "ffprobe keyframe scan failed." : stderr.Trim());

        return new KeyframeIndex([.. times]);
    }

    private static string[] BuildArgs(string filePath) =>
        [
            "-v", "error",
            "-skip_frame", "nokey",
            "-select_streams", "v:0",
            "-show_frames",
            "-show_entries", "frame=pts_time",
            "-of", "csv=p=0",
            filePath
        ];
}
