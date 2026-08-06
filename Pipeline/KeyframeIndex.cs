using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace OneColumnEncoder.Pipeline;

public sealed class KeyframeIndex : IDisposable
{
    private readonly object _sync = new();
    private readonly Process _process;
    private readonly CancellationToken _scanToken;
    private readonly List<double> _times = [];
    private readonly TaskCompletionSource<bool> _firstTimeReady = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _disposed;

    private KeyframeIndex(Process process, CancellationToken scanToken)
    {
        _process = process;
        _scanToken = scanToken;
    }

    public int Count
    {
        get
        {
            lock (_sync) return _times.Count;
        }
    }

    public double FirstTime
    {
        get
        {
            lock (_sync)
            {
                if (_times.Count == 0)
                    throw new InvalidOperationException("No keyframe timestamp is available.");
                return _times[0];
            }
        }
    }

    public bool TryFindNearestBefore(double targetTime, out double keyframeTime)
    {
        lock (_sync)
        {
            keyframeTime = 0d;
            if (_times.Count == 0) return false;

            int lo = 0;
            int hi = _times.Count - 1;
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
    }

    public static KeyframeIndex Start(
        string ffprobePath,
        string filePath,
        CancellationToken scanToken)
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

        Process process = new() { StartInfo = psi, EnableRaisingEvents = true };
        process.Start();

        KeyframeIndex index = new(process, scanToken);
        _ = index.ReadOutputAsync();
        return index;
    }

    public async Task WaitForFirstAsync(CancellationToken token)
    {
        await _firstTimeReady.Task.WaitAsync(token).ConfigureAwait(false);
    }

    private async Task ReadOutputAsync()
    {
        Task<string> stderrTask = _process.StandardError.ReadToEndAsync(_scanToken);

        try
        {
            string? line;
            while ((line = await _process.StandardOutput.ReadLineAsync(_scanToken).ConfigureAwait(false)) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string value = line.Split(',', 2)[0].Trim();
                if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double ptsTime))
                    continue;

                bool first;
                lock (_sync)
                {
                    first = _times.Count == 0;
                    _times.Add(ptsTime);
                }

                if (first)
                    _firstTimeReady.TrySetResult(true);
            }

            await _process.WaitForExitAsync(_scanToken).ConfigureAwait(false);
            string stderr = await stderrTask.ConfigureAwait(false);
            bool hasTimes = Count > 0;
            if (_process.ExitCode != 0 || !hasTimes)
            {
                _firstTimeReady.TrySetException(new InvalidOperationException(
                    string.IsNullOrWhiteSpace(stderr)
                        ? "ffprobe keyframe scan failed."
                        : stderr.Trim()));
            }
            else
            {
                _firstTimeReady.TrySetResult(true);
            }
        }
        catch (OperationCanceledException)
        {
            _firstTimeReady.TrySetCanceled(_scanToken);
            TryKillProcess();
        }
        catch (Exception ex)
        {
            _firstTimeReady.TrySetException(ex);
        }
        finally
        {
            _process.Dispose();
        }
    }

    private void TryKillProcess()
    {
        try
        {
            if (!_process.HasExited)
                _process.Kill(true);
        }
        catch { }
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

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
        }

        TryKillProcess();
        _firstTimeReady.TrySetCanceled();
    }
}
