using OneColumnEncoder.Models;
using OneColumnEncoder.Models.Lang;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneColumnEncoder.FFmpeg;

/// <summary>
/// Builds repart divider preview frames through ffprobe and ffmpeg.
/// </summary>
/// <remarks>
/// 1. Creates a new repart divider preview service.
/// 2. Rejects new requests before this previous is done.
/// 3. Caches keyframe index data for the lifetime of the service
/// </remarks>
/// <param name="ffmpegPath">Path to ffmpeg.</param>
/// <param name="ffprobePath">Path to ffprobe.</param>
public sealed class RepartDividerPreviewService(string? ffmpegPath, string? ffprobePath) : IDisposable
{
    private const double KeyframeIndexWindowMarginSeconds = 30d;
    private const double KeyframeIndexWindowLeadSeconds = 0.25d;
    private const double KeyframeIndexCacheReuseToleranceSeconds = 1d;

    private readonly string? _ffmpegPath = ffmpegPath;
    private readonly string? _ffprobePath = ffprobePath;
    private readonly string _workDirectory = CreateWorkDirectory("1cenc-repart-preview-");
    private readonly SemaphoreSlim _renderGate = new(1, 1);
    private readonly Lock _keyframeIndexCacheSync = new();
    private readonly Dictionary<string, KeyframeIndex> _keyframeIndexCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _keyframeIndexLifetimeCts = new();
    private bool _disposed;

    /// <summary>
    /// Builds the preview frames for the selected divider.
    /// </summary>
    /// <param name="analysis">Active repart plan.</param>
    /// <param name="selectedFrame">Selected divider frame.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Rendered preview frames and status text.</returns>
    public async Task<RepartDividerPreviewResult> BuildAsync(
        RepartPlanM analysis,
        long selectedFrame,
        CancellationToken token)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(_ffmpegPath) || !File.Exists(_ffmpegPath))
            return new([], RepartLangProvider.Current["DividerPreviewFfmpegUnavailable"]);

        await _renderGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            return await RenderRequestAsync(analysis, selectedFrame, token).ConfigureAwait(false);
        }
        finally
        {
            _renderGate.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="analysis"></param>
    /// <param name="selectedFrame"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<RepartDividerPreviewResult> RenderRequestAsync(RepartPlanM analysis, long selectedFrame, CancellationToken token)
    {
        if (analysis.TotalFrames <= 0)
            throw new InvalidOperationException("Divider preview source data missing."); // TODO: localize to RepartLangProvider.Current["DividerPreviewSourceDataMissing"]

        long windowFirst = Math.Max(0, selectedFrame - 3);
        long windowLast = Math.Min(analysis.TotalFrames - 1, selectedFrame + 3);
        double frameRate = (double)analysis.FrameRateNumerator / analysis.FrameRateDenominator;

        CleanupPreviewFiles();

        // The 7 preview frames: L3, L2, L1, selected, R1, R2, R3
        List<RepartDividerPreviewFrame> frames = [];
        // Finder of the L3-R3 frames from src video
        List<RepartSourceM> overlapping = [.. analysis.Sources
            .Where(source => source.LastFrame >= windowFirst && source.FirstFrame <= windowLast)
            .OrderBy(source => source.FirstFrame)];

        foreach (RepartSourceM src in overlapping)
        {
            token.ThrowIfCancellationRequested();
            if (!File.Exists(src.FilePath)) continue;

            long relFirst = Math.Max(0, Math.Max(windowFirst, src.FirstFrame) - src.FirstFrame);
            long relLast = Math.Max(relFirst, Math.Min(src.LastFrame, windowLast) - src.FirstFrame);

            double frameDuration = (double)analysis.FrameRateDenominator / analysis.FrameRateNumerator;
            double sourceStartTime = TryGetSourceStartTime(src.RawJson) ?? 0d;
            double targetTime = sourceStartTime + relFirst * frameDuration;

            KeyframeIndex? index = await BuildKeyframeIndexAsync(src, targetTime, frameRate, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            double keyframeTime = 0d;
            bool canSeek = index != null
                && index.TryFindNearestBefore(targetTime, out keyframeTime);
            long keyframeFrame = canSeek
                ? Math.Max(0, (long)Math.Round(
                    (keyframeTime - sourceStartTime) / frameDuration,
                    MidpointRounding.AwayFromZero))
                : 0;

            // Output image file name builder
            string patternPrefix = $"divider-preview-{Guid.NewGuid():N}-{src.FirstFrame}";
            string pattern = Path.Combine(_workDirectory, patternPrefix + "-%02d.png");
            // FFmpeg CMD builder
            string[] args = !canSeek
                ? BuildSourceFrameArgs(src.FilePath, relFirst, relLast, pattern)
                : BuildSourceFrameSeekArgs(
                    src.FilePath,
                    keyframeTime,
                    relFirst - keyframeFrame,
                    relLast - keyframeFrame,
                    pattern);

            // Ideally "await" should be avoided here, but FFmpegProcessorRunner.RunAsync has to be waited for getting the output files
            // Maybe just write bitmap to RAM, and access the RAM for pixel values to Previewer? So await hang can be avoided
            await FFmpegProcessRunner.RunAsync(_ffmpegPath!, args, TimeSpan.FromMinutes(1), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            // Collect the files and build preview frames
            string[] files = [.. Directory
                .GetFiles(_workDirectory, patternPrefix + "-*.png")
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

            for (int i = 0; i < files.Length; i++)
            {
                long frameNumber = src.FirstFrame + relFirst + i;
                if (frameNumber < windowFirst || frameNumber > windowLast) continue;
                bool isDivider = frameNumber == selectedFrame;
                frames.Add(new RepartDividerPreviewFrame(
                    frameNumber,
                    LoadBitmap(files[i]),
                    isDivider,
                    !isDivider));
            }
        }

        if (frames.Count == 0)
            throw new InvalidOperationException("Divider preview frame file missing.");

            string sourceName = analysis.Sources
                .Where(source => selectedFrame >= source.FirstFrame && selectedFrame <= source.LastFrame)
                .Select(source => source.DisplayName)
                .FirstOrDefault() ?? "?";

        return new RepartDividerPreviewResult(
            frames,
            string.Format(
                RepartLangProvider.Current["DividerPreviewSummary"],
                sourceName,
                selectedFrame,
                frames.Count));
    }

    /// <summary>
    /// Releases cached preview state and temporary files.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _keyframeIndexLifetimeCts.Cancel();
        DisposeKeyframeIndexCache();
        _keyframeIndexLifetimeCts.Dispose();
        _renderGate.Dispose();
        DeleteDirectoryQuietly(_workDirectory);
        // No GC.SuppressFinalize(this) here since there is no unmanaged resources
    }

    private async Task<KeyframeIndex?> BuildKeyframeIndexAsync(
        RepartSourceM source,
        double targetTime,
        double frameRate,
        CancellationToken token,
        bool widenWindow = false)
    {
        double margin = widenWindow ? KeyframeIndexWindowMarginSeconds * 2d : KeyframeIndexWindowMarginSeconds;
        double windowStart = Math.Max(0d, targetTime - margin);
        double windowEnd = targetTime + KeyframeIndexWindowLeadSeconds;

        KeyframeIndex? cached;
        lock (_keyframeIndexCacheSync)
            _keyframeIndexCache.TryGetValue(source.FilePath, out cached);

        if (cached != null && cached.CoversRange(windowStart, windowEnd, KeyframeIndexCacheReuseToleranceSeconds))
        {
            await cached.Completion.WaitAsync(token).ConfigureAwait(false);
            return cached.Count > 0 ? cached : null;
        }

        if (string.IsNullOrWhiteSpace(_ffprobePath) || !File.Exists(_ffprobePath))
            return null;

        if (cached != null)
        {
            if (RemoveCachedKeyframeIndex(source.FilePath, cached))
                cached.Dispose();
        }

        KeyframeIndex index;
        try
        {
            await WarmSourceWindowCacheAsync(source, targetTime, margin, frameRate, token).ConfigureAwait(false);
            index = KeyframeIndex.Start(
                _ffprobePath,
                source.FilePath,
                _keyframeIndexLifetimeCts.Token,
                windowStart,
                windowEnd);
        }
        catch
        {
            throw;
        }

        lock (_keyframeIndexCacheSync)
        {
            if (_keyframeIndexCache.TryGetValue(source.FilePath, out cached))
            {
                index.Dispose();
                index = cached;
            }
            else
            {
                _keyframeIndexCache[source.FilePath] = index;
            }
        }

        if (!ReferenceEquals(index, cached))
        {
            _ = index.Completion.ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    RemoveCachedKeyframeIndex(source.FilePath, index);
                }
                else if (task.IsFaulted)
                {
                    if (RemoveCachedKeyframeIndex(source.FilePath, index))
                        index.Dispose();
                }
            }, TaskScheduler.Default);
        }

        try
        {
            await index.Completion.WaitAsync(token).ConfigureAwait(false);

            if (index.Count == 0)
            {
                if (RemoveCachedKeyframeIndex(source.FilePath, index))
                    index.Dispose();
                if (!widenWindow && windowStart > 0d)
                    return await BuildKeyframeIndexAsync(
                        source, targetTime, frameRate, token, widenWindow: true).ConfigureAwait(false);

                return null;
            }

            return index;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return index;
        }
        catch
        {
            if (RemoveCachedKeyframeIndex(source.FilePath, index))
                index.Dispose();
            throw;
        }
    }

    private void CleanupPreviewFiles()
    {
        foreach (string file in Directory.GetFiles(_workDirectory, "divider-preview-*.png"))
        {
            try { File.Delete(file); }
            catch { }
        }

        foreach (string file in Directory.GetFiles(_workDirectory, "divider-preview-*.jpg"))
        {
            try { File.Delete(file); }
            catch { }
        }
    }

    private void DisposeKeyframeIndexCache()
    {
        foreach (KeyframeIndex index in ClearKeyframeIndexCache())
            index.Dispose();
    }

    private KeyframeIndex[] ClearKeyframeIndexCache()
    {
        lock (_keyframeIndexCacheSync)
        {
            KeyframeIndex[] indexes = [.. _keyframeIndexCache.Values];
            _keyframeIndexCache.Clear();
            return indexes;
        }
    }

    private bool RemoveCachedKeyframeIndex(string filePath, KeyframeIndex index)
    {
        lock (_keyframeIndexCacheSync)
        {
            if (!_keyframeIndexCache.TryGetValue(filePath, out KeyframeIndex? cached)
                || !ReferenceEquals(cached, index))
                return false;

            _keyframeIndexCache.Remove(filePath);
            return true;
        }
    }

    private static async Task WarmSourceWindowCacheAsync(
        RepartSourceM source,
        double targetTime,
        double margin,
        double frameRate,
        CancellationToken token)
    {
        try
        {
            long fileLength = source.FileLength;
            if (fileLength <= 0 || source.FrameCount <= 0) return;

            if (!(frameRate > 0d)) return;

            double duration = source.FrameCount / frameRate;
            if (!(duration > 0d)) return;

            double startSec = Math.Max(0d, targetTime - margin);
            double endSec = Math.Min(duration, targetTime + 1d);
            if (endSec <= startSec) return;

            long startByte = (long)(startSec / duration * fileLength);
            long endByte = Math.Min(fileLength, (long)(endSec / duration * fileLength));
            if (endByte <= startByte) return;

            using FileStream stream = new(
                source.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                1 << 20,
                FileOptions.SequentialScan);
            if (startByte > 0) stream.Seek(startByte, SeekOrigin.Begin);

            long remaining = endByte - startByte;
            byte[] buffer = new byte[1 << 20];
            while (remaining > 0)
            {
                token.ThrowIfCancellationRequested();
                int read = await stream.ReadAsync(
                    buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                    token).ConfigureAwait(false);
                if (read <= 0) break;
                remaining -= read;
            }
        }
        catch { }
    }

    private static double? TryGetSourceStartTime(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return null;

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            if (document.RootElement.TryGetProperty("streams", out JsonElement streams)
                && streams.ValueKind == JsonValueKind.Array
                && streams.GetArrayLength() > 0)
            {
                double? streamStart = TryGetJsonDouble(streams[0], "start_time");
                if (streamStart != null) return streamStart;
            }

            if (document.RootElement.TryGetProperty("format", out JsonElement format))
                return TryGetJsonDouble(format, "start_time");
        }
        catch (JsonException) { }

        return null;
    }

    private static double? TryGetJsonDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double number))
            return number;
        if (value.ValueKind == JsonValueKind.String
            && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double text))
            return text;
        return null;
    }

    private static string[] BuildSourceFrameArgs(
        string sourceVideoPath,
        long firstFrame,
        long lastFrame,
        string outputPattern,
        int targetHeight = 480,
        string scaleFlags = "lanczos")
    {
        long safeFirstFrame = Math.Max(0, firstFrame);
        long safeLastFrame = Math.Max(safeFirstFrame, lastFrame);
        return
        [
            "-hide_banner",
            "-y",
            "-strict",
            "unofficial",
            "-i",
            sourceVideoPath,
            "-vf",
            $"select=between(n\\,{safeFirstFrame}\\,{safeLastFrame}),scale=-2:{Math.Max(1, targetHeight)}:flags={scaleFlags}",
            "-vsync",
            "0",
            "-start_number",
            "0",
            "-frames:v",
            (safeLastFrame - safeFirstFrame + 1).ToString(CultureInfo.InvariantCulture),
            "-c:v",
            "png",
            outputPattern
        ];
    }

    private static string[] BuildSourceFrameSeekArgs(
        string sourceVideoPath,
        double keyframeTime,
        long firstOffsetFrame,
        long lastOffsetFrame,
        string outputPattern,
        int targetHeight = 480,
        string scaleFlags = "lanczos")
    {
        long safeFirstOffset = Math.Max(0, firstOffsetFrame);
        long safeLastOffset = Math.Max(safeFirstOffset, lastOffsetFrame);
        long frameCount = safeLastOffset - safeFirstOffset + 1;
        string keyframeTimestamp = FormatSeekSeconds(keyframeTime);
        return
        [
            "-hide_banner",
            "-y",
            "-strict",
            "unofficial",
            "-ss",
            keyframeTimestamp,
            "-seek_timestamp",
            "1",
            "-i",
            sourceVideoPath,
            "-vf",
            $"select=between(n\\,{safeFirstOffset}\\,{safeLastOffset}),scale=-2:{Math.Max(1, targetHeight)}:flags={scaleFlags}",
            "-vsync",
            "0",
            "-start_number",
            "0",
            "-frames:v",
            frameCount.ToString(CultureInfo.InvariantCulture),
            "-c:v",
            "png",
            outputPattern
        ];
    }

    private static string FormatSeekSeconds(double seconds) =>
        Math.Max(0d, seconds).ToString("0.######", CultureInfo.InvariantCulture);

    private static BitmapImage LoadBitmap(string path)
    {
        BitmapImage bitmap = new();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static string CreateWorkDirectory(string namePrefix)
    {
        string directory = Path.Combine(Path.GetTempPath(), namePrefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteDirectoryQuietly(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch { }
    }

    // if (_disposed) throw new ObjectDisposedException(nameof(RepartDividerPreviewService));
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

}

/// <summary>
/// One frame in a repart divider preview strip.
/// </summary>
public sealed record RepartDividerPreviewFrame(
    long Frame,
    ImageSource FrameImage,
    bool IsSelected,
    bool IsNeighbor)
{
    /// <summary>
    /// Gets the display text for the frame number.
    /// </summary>
    public string FrameText => $"{Frame:N0}";
}

/// <summary>
/// Result produced by repart divider preview rendering.
/// </summary>
public sealed record RepartDividerPreviewResult(
    IReadOnlyList<RepartDividerPreviewFrame> Frames,
    string StatusText);
