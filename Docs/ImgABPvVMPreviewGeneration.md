# ImgABPvVM Preview Generation Flow

This document describes how `ImgABPvVM` builds the A-B preview pair used by the encoder preview window.

## Purpose

`ImgABPvVM` builds a side-by-side preview by:

1. extracting one frame from the source video,
2. optionally converting that frame into the selected display mode,
3. encoding that frame with the selected preview encoder,
4. decoding the encoded result back to PNG,
5. loading both images into the viewer,
6. optionally computing quality scores with external tools.

## Main entry points

- `EncoderConfVM` creates `ImgABPvVM` and exposes it as `PreviewVM`.
- `PreviewCommand` in `ImgABPvVM` starts or cancels preview generation.
- `ImgABPvViewer` listens for `SourceImage` and `EncodedImage` changes and refits the view.

## State setup

When `ImgABPvVM` is constructed, it:

1. stores references to `EncoderConfVM`, the modal navigator, the ffmpeg path, and the source video path,
2. creates a unique working directory under the system temp folder,
3. initializes encoder choices and display mode buttons,
4. reads source ffprobe data and color space analysis,
5. sets the initial preview position and tick labels,
6. initializes status text and busy state,
7. subscribes to language change notifications.

## Preview generation flow

`GeneratePreviewAsync()` performs the preview workflow.

### 1. Validate prerequisites

The method first checks that:

1. `ffmpeg` exists
2. Source video path exists

If either check fails, it updates `StatusText` and stops.

### 2. Prepare cancellation and busy state

Before starting work, the VM:

- disposes the previous `CancellationTokenSource`,
- creates a new one,
- stores its token,
- sets `IsBusy = true`.

`IsBusy` also changes the preview button text to the cancel label and notifies the parent configuration VM that preview work is active.

### 3. Build the preview model

The preview does not reuse the persisted encoder model directly.

Instead, `EncoderConfVM.CreatePreviewModel()` builds a temporary `EncoderConfM` instance and forces CRF mode for preview generation.

### 4. Select the preview encoder

The encoder is read from the preview dropdown.

Supported preview encoders are:

- `libx264`
- `libx265`
- `libsvtav1`
- `libvvenc` (preview only)

If the selected encoder is SVT-AV1 and the source appears to be 12-bit, the preview is blocked and an error modal is shown.

### 5. Build the display conversion path

The selected preview display mode controls whether the source frame is shown as-is or converted before encoding.

The pipeline builds a display filter from the source color metadata:

- `Raw`
- `Low -> BT.709`
- `WCG -> BT.709`
- `HDR -> SDR`
- `HDRWCG -> SDR709`

If no filter is needed, the extracted source frame is used directly.

### 6. Extract the source frame

The VM creates temporary file paths in the work directory:

- `source-raw.png` for the extracted frame,
- `source-<mode>.png` when a display conversion is applied,
- encoder-specific encoded output files,
- encoder-specific decoded PNG files.

It then runs ffmpeg to extract a single frame at `PreviewPositionSeconds`.

If a display filter exists, ffmpeg is run a second time to convert the raw frame into the selected display mode.

The final source image is loaded into `SourceImage`.

### 7. Encode the preview frame

The VM runs ffmpeg again using `PreviewPipeline.BuildEncodeArgs()`.

For each encoder:

- x264 and x265 use `-crf` plus any custom preview parameters,
- SVT-AV1 uses `-crf` plus custom preview parameters,
- VVenC uses `-qp` and fixed preview options.

The output is a single-frame encoded stream saved into the working directory.

### 8. Decode the encoded output

The encoded file is then decoded back to PNG with `PreviewPipeline.BuildDecodeArgs()`.

The decoded image is loaded into `EncodedImage`.

### 9. Run quality scoring

After both images are ready, the VM optionally runs external quality analyzers:

- `ssimulacra2.exe` if it is present,
- `butteraugli.exe` if it is present.

Each tool receives the source PNG and decoded PNG.

The resulting score or error text is written into the corresponding status field.

**Important:** The score tools run **without** cancellation support. They do not receive the cancellation token and their processes are not tracked by `_currentProcess`. If the user cancels during scoring, the external tools still run to completion.

### 10. Finish

If everything succeeds, `StatusText` becomes a ready message containing:

- the encoder name,
- the active quality value.

If the work is cancelled, `StatusText` becomes the cancelled message.

If an error occurs, the exception message is shown, and ffmpeg stderr is forwarded to the error modal when available.

## ffmpeg command rules

`PreviewPipeline` is responsible for building the actual command lines.

### Source extraction

`BuildSourceArgs()` generates a command that:

1. hides the banner,
2. forces overwrite,
3. seeks to the requested timestamp,
4. opens the source video,
5. optionally applies a display filter,
6. extracts one frame as PNG.

### Encoding

`BuildEncodeArgs()` generates a command that:

1. reads the source PNG,
2. selects the chosen encoder,
3. applies the preview quality value,
4. appends encoder-specific custom parameters,
5. writes one encoded frame in the encoder-specific output format.

### Decoding

`BuildDecodeArgs()` reads the encoded file and converts one frame back to PNG.

## Output files

Each preview run uses a dedicated temp directory.

The file names encode both encoder and display mode so the preview can be refreshed without crossing state between modes.

Examples:

- `source-raw.png`
- `source-hdrsdr.png`
- `x264-raw.h264`
- `x264-raw.png`
- `svtav1-hdrsdr.obu`
- `svtav1-hdrsdr.png`

## Viewer behavior

`ImgABPvViewer` reacts to image updates by refitting the viewport after `SourceImage` or `EncodedImage` changes.

It also supports:

- zoom in/out,
- fine zoom steps,
- actual size,
- double size,
- fit-to-window,
- pan and split comparison.

## CancellationTokenSource lifecycle

`_previewCts` is a nullable field that starts as `null`. It is **only** used for ffmpeg operations — the external score tools do not observe it.

### Creation

A fresh `CancellationTokenSource` is created at the **start** of every `GeneratePreviewAsync()` call. The previous instance (if any) is disposed first:

```
_previewCts?.Dispose();
_previewCts = new CancellationTokenSource();
```

This ensures a clean cancellation scope per preview run and prevents resource leaks from stale CTS instances.

### Scope — what the token covers

The token is passed to every ffmpeg call inside `GeneratePreviewAsync()`:

1. **Extract** — `RunFfmpegAsync(BuildSourceArgs(...), token)`
2. **Convert** (optional) — `RunFfmpegAsync(BuildSourceArgs(... + displayFilter), token)`
3. **Encode** — `RunFfmpegAsync(BuildEncodeArgs(...), token)`
4. **Decode** — `RunFfmpegAsync(BuildDecodeArgs(...), token)`

The score tools (`ssimulacra2`, `butteraugli`) run **after** all ffmpeg work finishes and **do not** accept the cancellation token. If the user cancels during scoring, those processes still run to completion, then `IsBusy` resets normally.

### Process tracking

`_currentProcess` only tracks the **most recent** ffmpeg process. Score-tool processes are not tracked and cannot be force-killed via `TryKillCurrentProcess()`.

### Triggering cancellation

Cancellation is triggered from two call sites:

- **`PreviewOrCancel()`** — when the user clicks the button while `IsBusy`, the CTS is cancelled and the tracked ffmpeg process is killed immediately.
- **`Dispose()`** — when the VM is torn down, cancellation and cleanup follow a strict order.

The null-conditional operator (`?.`) on `Cancel()` is safe even if `_previewCts` is still `null` (e.g., `Dispose()` called before any preview has started).

### Propagation through ffmpeg calls

Inside `RunFfmpegAsync()`:

1. The token is passed to `Process.StandardOutput.ReadToEndAsync(token)` and `Process.WaitForExitAsync(token)`.
2. If cancellation fires during `WaitForExitAsync`, the method catches `OperationCanceledException`, calls `PreviewPipeline.TryKillProcess(process)` to terminate ffmpeg, then rethrows.
3. Back in `GeneratePreviewAsync()`, the `OperationCanceledException` handler sets `StatusText = Lang.StatusCancelled`.
4. The `finally` block clears `_currentProcess`, then `IsBusy = false` runs after the try-catch-finally.

### Cleanup on disposal

`Dispose()` follows a fixed order:

1. Unsubscribe from language change events.
2. Cancel the CTS (signals any in-flight ffmpeg).
3. Kill the ffmpeg process if still running.
4. Dispose the CTS (release resources).
5. Notify `EncoderConfVM` that preview is no longer busy.
6. Delete the temporary working directory (errors silently ignored).

This ordering guarantees that cancellation is signalled before the CTS is disposed, avoiding a race with an in-flight `OperationCanceledException`.

## External score tools

The score tools are optional and only run when present in the application directory.

- `Ssimulacra2` parses the tool output as a floating-point score.
- `Butteraugli` parses the tool output as a floating-point score.

If a tool is missing, the related score text is left empty or marked as unavailable by the UI layer.

**Cancellation note:** Neither score tool accepts a `CancellationToken`. If the user cancels preview while scoring is in progress, the external process will still finish before `IsBusy` resets. The ffmpeg pipeline (extract → encode → decode) is always complete by that point, so the source and encoded images remain valid.
