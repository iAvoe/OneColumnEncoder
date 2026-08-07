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
    private readonly TaskCompletionSource<bool> _completed = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _disposed;

    private KeyframeIndex(Process process, CancellationToken scanToken, double? intervalStartSec, double? intervalEndSec)
    {
        _process = process;
        _scanToken = scanToken;
        IntervalStartSec = intervalStartSec;
        IntervalEndSec = intervalEndSec;
    }

    // When the scan was restricted with -read_intervals, these are the absolute
    // seconds the probe was asked to read. A full-file scan reports null.
    public double? IntervalStartSec { get; }
    public double? IntervalEndSec { get; }

    // True when this index's scanned window covers the requested [start, end]
    // range (with a tolerance), or when it was a full-file scan (no interval).
    public bool CoversRange(double rangeStart, double rangeEnd, double toleranceSeconds = 0d)
    {
        double start = IntervalStartSec ?? double.NegativeInfinity;
        double end = IntervalEndSec ?? double.PositiveInfinity;
        return rangeStart >= start - toleranceSeconds && rangeEnd <= end + toleranceSeconds;
    }

    public int Count
    {
        get
        {
            lock (_sync) return _times.Count;
        }
    }

    public Task Completion => _completed.Task;

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
        CancellationToken scanToken,
        double? intervalStartSec = null,
        double? intervalEndSec = null)
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

        foreach (string arg in BuildArgs(filePath, intervalStartSec, intervalEndSec))
            psi.ArgumentList.Add(arg);

        Process process = new() { StartInfo = psi, EnableRaisingEvents = true };
        process.Start();

        KeyframeIndex index = new(process, scanToken, intervalStartSec, intervalEndSec);
        _ = index.ReadOutputAsync();
        return index;
    }

    public async Task WaitForFirstAsync(CancellationToken token)
    {
        await _firstTimeReady.Task.WaitAsync(token).ConfigureAwait(false);
    }

    private async Task ReadOutputAsync()
    {
        using CancellationTokenRegistration killRegistration = _scanToken.Register(TryKillProcess);
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
            if (_process.ExitCode != 0)
            {
                InvalidOperationException ex = new(
                    string.IsNullOrWhiteSpace(stderr)
                        ? "ffprobe keyframe scan failed."
                        : stderr.Trim());
                _firstTimeReady.TrySetException(ex);
                _completed.TrySetException(ex);
            }
            else
            {
                _firstTimeReady.TrySetResult(true);
                _completed.TrySetResult(true);
            }
        }
        catch (OperationCanceledException)
        {
            _firstTimeReady.TrySetCanceled(_scanToken);
            _completed.TrySetCanceled(_scanToken);
            TryKillProcess();
        }
        catch (Exception ex)
        {
            _firstTimeReady.TrySetException(ex);
            _completed.TrySetException(ex);
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

    private static string[] BuildArgs(string filePath, double? intervalStartSec, double? intervalEndSec)
    {
        List<string> args =
        [
            "-v", "error",
            "-skip_frame", "nokey",
            "-select_streams", "v:0",
            "-show_frames",
            "-show_entries", "frame=pts_time",
            "-of", "csv=p=0"
        ];

        if (intervalStartSec.HasValue
            && intervalEndSec.HasValue
            && intervalEndSec > intervalStartSec)
        {
            string interval = string.Create(
                CultureInfo.InvariantCulture,
                $"{intervalStartSec.Value:0.#########}%{intervalEndSec.Value:0.#########}");
            args.Add("-read_intervals");
            args.Add(interval);
        }

        args.Add(filePath);
        return [.. args];
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
        }

        TryKillProcess();
        _firstTimeReady.TrySetCanceled();
        _completed.TrySetCanceled();
    }
}
