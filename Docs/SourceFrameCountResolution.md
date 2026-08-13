# Source Frame Count Resolution Flow

This document describes how Repart Mode resolves the exact frame count of each source video, even when entries like `nb_frames` are missing from the ffprobe metadata.

## Purpose

`RepartCompatibilityAnalyzer` builds the plan that is loaded into `RepartConfModal`. Every output segment in the modal is expressed in source frame ranges, so the entire timeline depends on each source's total frame count.

The frame-count resolver must:

1. prefer an authoritative count when the metadata already contains one,
2. fall back to a duration × FPS estimate when `nb_frames` is missing,
3. verify that estimate by probing ffprobe at the predicted frame boundary,
4. escalate to heavier counting methods only when the cheap paths fail,
5. reject the source if no count can be produced.

## Main entry points

- `RepartCompatibilityAnalyzer.AnalyzeAndFilterAsync()` runs `ScanFramesAsync()` per accepted candidate in parallel and sums the results into `RepartPlanM.TotalFrames`.
- `RepartSrcValidator.ScanFramesAsync()` is the shared per-file entry point, also used by chapter imports.
- `RepartConfVM` reads the resolved count from `_analysis.TotalFrames` for all divider and output math.

## Resolution order

`ScanFramesAsync()` first checks whether the probe already carried a reliable count:

```
frameCount = probe.FrameCount is > 0
    ? probe.FrameCount.Value
    : await CountFramesAsync(...);
```

`probe.FrameCount` is the `nb_frames` entry read from the stream JSON. When it is missing, zero, or negative, control passes to `CountFramesAsync()`, which tries the following strategies in order:

1. **Estimate + verify with seek-probing** — the default fast path.
2. **Exact count via ffmpeg null remux** — used when the estimate path fails.
3. **Full ffprobe count** — the slowest fallback (`-count_frames`).
4. **Expanded 10× search** — only when ffmpeg is missing and the user confirms.
5. **Raw estimate** — returned if nothing better could be found.

If the final count is not positive, the source is rejected with `NoFrameCount`.

### 1. Estimate and verify with seek-probing

`EstimateFrameCount()` computes the estimate as:

```
exactFrames = durationSeconds * (frameRateNumerator / frameRateDenominator)
```

The duration comes from the stream `duration` entry, falling back to the format `duration`. The frame rate is the `avg_frame_rate` fraction (CFR is validated earlier in `AnalyzeProbe()`). The estimate is rounded to the nearest long with `MidpointRounding.AwayFromZero`.

`TryResolveEstimatedFrameCountWithFfprobeAsync()` then verifies the boundary by probing three frames near the estimate:

- `estimatedCount - 1`
- `estimatedCount`
- `estimatedCount + 1`

Each probe runs `ProbeFrameExistsAsync()`, which executes:

```
ffprobe -v error -hide_banner -select_streams v:0
  -read_intervals "<start>%<end>" -show_frames
  -show_entries frame=best_effort_timestamp_time,pts_time,pkt_pts_time,pkt_dts_time
  -of json <src>
```

The interval is centered on the target frame's predicted timestamp with a seek margin, so a short window is read instead of the whole file.

The result is decided from the existence pattern:

| left | center | right | meaning |
| ---- | ------ | ----- | ------- |
| 1 | 0 | 0 | count = estimate |
| 1 | 1 | 0 | count = estimate + 1 |
| 0 | 0 | 0 | count is below; probe ← |
| 1 | 1 | 1 | count is above; probe → |

`ProbeInDirectionForFrameCountAsync()` walks in the needed direction within ±300 frames (`MaxMetadataProbeAdjustmentFrames`) until the boundary is crossed.

### 2. Exact count via ffmpeg null remux

When the estimate cannot be verified, ffmpeg decodes the whole stream to count frames exactly:

```
ffmpeg -hide_banner -i <src> -map 0:v:0 -c copy -f null -
```

The `frame=N` line is parsed from stderr. This is only attempted when an ffmpeg path is available and the file exists.

### 3. Full ffprobe count

A full `-count_frames` scan is the slowest fallback and is kept after the ffmpeg attempt:

```
ffprobe -v error -hide_banner -count_frames -select_streams v:0
  -show_entries stream=nb_read_frames,nb_frames -of json <src>
```

`nb_read_frames` is preferred over `nb_frames` when both are present.

### 4. Expanded 10× search

If ffmpeg is missing and the estimate still could not be verified, `confirmExpandFrameCountSearch` asks the user whether to keep searching. On confirm, `SearchFrameCountWithExpansionAsync()`:

1. probes whether the estimated index exists,
2. expands the bracket by 10× per step in the correct direction until one probe is past the end,
3. binary-searches the boundary between the last existing and first non-existing frame,
4. returns the last existing index plus one.

Without a prompt callback, the raw estimate is kept instead.

### 5. Raw estimate fallback

If every strategy failed, the original `duration × fps` estimate is returned when positive; otherwise the source is rejected.

## Frame existence probing

`ProbeFrameExistsAsync()` converts a frame index into a timestamp:

```
frameDuration = frameRateDenominator / frameRateNumerator
targetSeconds = startTime + frameIndex * frameDuration
seekMargin = max(frameDuration * 2, 2s)
```

The probe window is then `[target - margin, target + margin]`. `FfprobeFrameOutputContainsIndex()` computes each returned frame's index from its best timestamp and checks whether the target index appears.

- A matching index means the frame exists.
- Timestamps present but no match means the frame does not exist.
- No usable timestamps at all returns `null` (unknown).

## File stability check

`ScanFramesAsync()` finishes by re-checking the source against the values captured at the start of analysis:

- `File.Length`
- `LastWriteTimeUtc`

If either changed, the source is rejected with `SourceChanged` so a mid-analysis replacement cannot silently corrupt the plan.

## Callers and result flow

`ScanFramesAsync()` returns a `RepartScanOutcome` containing the resolved `FrameCount` plus the refreshed file length and write ticks.

In `RepartCompatibilityAnalyzer.AnalyzeAndFilterAsync()`, every accepted candidate is scanned in parallel, and the counts accumulate into `cumulativeFrames`:

- each source stores its own `FrameCount` and its first/last timeline frame,
- the plan's `TotalFrames` is the sum of every accepted source.

`RepartConfVM` consumes `_analysis.TotalFrames` for divider positions, output segment ranges, and selection clamping, so the whole modal stays consistent with the resolved count.

**Important:** The resolution order is intentionally conservative — cheap metadata checks run first, and full-file counting is reserved for sources whose cheap results could not be verified. Normal imports therefore stay on the estimate or ffmpeg paths, while `-count_frames` and the expanded search remain last resorts.
