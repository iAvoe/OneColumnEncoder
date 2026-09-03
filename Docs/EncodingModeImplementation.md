# Encoding Mode Implementation: Single, Queue, Concat, and Repart

> For a high-level overview of the four encoding modes, see [Encoding Modes](ConceptsAndFeatures-EN.md#encoding-modes). This document covers runtime details for each route.

## 1. Route Activation

`FileManagement/SrcRouteKind.cs` defines the route enum:

```csharp
public enum SrcRouteKind { Single, Queue, Concat, Repart }
```

`MainVM.GetActiveSrcRoute()` is the central route resolver:

```csharp
private SrcRouteKind GetActiveSrcRoute()
{
    if (_videoSrcQueue.IsActive) return SrcRouteKind.Queue;
    if (_videoSrcConcat.IsActive) return SrcRouteKind.Concat;
    if (_videoSrcRepart.IsActive) return SrcRouteKind.Repart;
    return SrcRouteKind.Single;
}
```

`VideoSrcQueueState` lives in `QueueManagement/VideoSrcQueue.cs`; `VideoSrcConcatState` lives in `ConcatManagement/VideoSrcConcat.cs`; `VideoSrcRepartState` lives in `RepartManagement/VideoSrcRepartState.cs`.

## 2. Import Zone Layout

`ToolCatalogProviderM.GetVideoSrcImportDefs()` returns four video source cards:

| Index | Route | Card | R1 | R2 | State Owner |
|-------|-------|------|----|----|-------------|
| 0 | Single | `Tool.Source.VideoSrc` | Replace | Clear | `AppDataM.Tools.VideoSourcePath` |
| 1 | Queue | `Tool.Source.VideoSrcQueue` | Import | Clear | `VideoSrcQueueState` |
| 2 | Concat | `Tool.Source.VideoSrcConcat` | Import | Clear | `VideoSrcConcatState` |
| 3 | Repart | `Tool.Source.VideoSrcRepart` | Import | Clear | `VideoSrcRepartState` |

When `one_line_shot_args.exe` is selected, Queue, Concat, and Repart are deselected and disabled.

## 3. Active UI Routing

`MainVM.RefreshActiveSrcRoute()` switches validation cards and script zones by route.

| Aspect | Single | Queue | Concat |
|--------|--------|-------|--------|
| Validation card | `SrcValidationCard` | `QueueSrcFilterCard` | `ConcatCheckCard` |
| Script zone | `ScriptSrcImportZone` | `QueueScriptSrcImportZone` | `ScriptSrcImportZone` |
| Duration filter | Hidden | Visible | Hidden |
| Sample Clip | Enabled when checks pass | Disabled | Disabled |
| Output setting | Filename scribe | Directory-only queue output | Filename scribe for one concat output |
| Card title | Filename | Item count | Fragment count |

`RefreshScriptSourceEnabledState()` only mutates the single script zone while the active route is `Single`; Queue and Concat leave script zone state under their route-specific flows.

## 4. Source Import Behavior

### 4.1 Single

`BrowseSrcPathCmd` imports one source path. `OnSrcImported()` clears competing source selections and stores the selected path in app data.

### 4.2 Queue

`BrowseSrcQueueCmd` imports a folder of video files. `BrowseSrcScriptQueueCmd` imports a folder of matching script files for the selected script kind.

After import:
- `P2TextData` stores the folder path.
- `P1TextData` stores a compact label such as `firstFile..lastFile`.
- `P1TooltipText` stores the full comma-separated file list for hover text (truncated at 512 chars).

`VideoSrcQueueState` keeps the accepted file paths in a `Dictionary<ToolItemCardVM, string[]>` keyed by the queue card. `ApplyAcceptedFiles()` replaces that list after analysis and refreshes the queue label.

Script queue imports are validated against the current video queue before they are accepted. `MainVM.ValidateScriptQueueImport()` checks basename matches and embedded source paths so queue scripts stay aligned with the selected source list.

### 4.3 Concat

`BrowseSrcConcatCmd` opens a multi-select `OpenFileDialog` (minimum two files). Before accepting the selection, it checks that all selected files use supported video extensions and have the **same extension**. If any selected file differs, import aborts and opens `OpenErrModalCmd` with the expected extension and mismatched files.

The import command also runs a compatibility pre-analysis with ffprobe before accepting the files. This pre-analysis is an import gate only: it can reject or warn about the selection, but the accepted result is not stored into `_srcVideoAnalysis`. After import, the normal source-analysis flow still runs.

After import:
- `P2TextData` stores the first file's parent directory.
- `P1TextData` stores a compact label such as `firstFile..lastFile`.
- `P1TooltipText` stores the full selected file list for hover text.
- `VideoSrcConcatState` stores the ordered full file paths.
- `source_concat_filelist.txt` is generated under the app config directory (`1cenc`), using ffmpeg concat-demuxer syntax with absolute paths:

```text
file 'G:/media/part01.mkv'
file 'G:/media/part02.mkv'
```

## 5. Source Analysis

### 5.1 Per-Route Overview

| Route | Analysis Behavior |
|-------|-------------------|
| Single | Analyze one source with ffprobe, keep frame count unknown when ffprobe does not provide one, update `SrcValidationCard`. |
| Queue | Analyze all queue files with ffprobe, optionally filter by first-stream or weighted vote, write accepted/excluded JSON. Individual failures can be skipped. |
| Concat | Analyze all concat files. ffprobe failures reject the batch, but VFR and frame-rate mismatches surface as warnings. Concat additionally sums every fragment's frame count into a **concat total frame count** only when every fragment has a real frame count. |

### 5.2 Queue Analysis Strategies

When `AnalyzeSrcVideoCmd` runs in queue mode with filtering enabled, the user chooses one of two reference strategies:

**FirstStream** — Use the first video stream as reference, excluding files whose validation results differ. This maximizes similarity by the video source validation card checklist:
- Videos with different frame rate or resolution are kept (auto-parameter generation can handle it).
- Videos lacking color metadata are kept as long as the checklist success-fail pattern matches.
- The ffmpeg-VS-AVS filter-fixing mechanism remains universally applicable.

**WeightedVote** — Useful for BDMV directories mixing main features, intros, trailers, and menu clips:
1. Group by basic video attributes (width, real frame rate, average frame rate).
2. Find the representative group with the highest duration or frame count. Uses squared weighting to combat large numbers of short streams.
3. Exclude irrelevant groups.
4. Apply FirstStream cleanup checks on the remaining group.

The analysis writes timestamped JSON files under `1cenc/`:
- `source_queue_yyyyMMdd_HHmmss.json` for accepted files.
- `source_queue_excluded_yyyyMMdd_HHmmss.json` when files are filtered out.

### 5.3 Concat Analysis Details

`AnalyzeSrcVideoCmd` requires at least two selected fragments, analyzes every selected fragment with ffprobe, and only uses a concat total frame count when every fragment provides a real frame count.

**Hard failures** (abort the whole concat source):
- Any file fails ffprobe.
- Any file lacks a readable video stream.
- Any file differs from the first file's resolution.

**Soft compatibility warnings** (emit `OpenWarnModalCmd` and continue):
- Codec, pixel format, source validation checklist signature, or CFR frame-rate signature differs.

The comparison signature includes:
- source validation checklist signature
- width, height
- pixel format, codec
- normalized `avg_frame_rate`, normalized `r_frame_rate`

On success, the first fragment's raw ffprobe JSON is stored in `_srcVideoAnalysis.RawJson` and drives encoder config previews and FilterScribe helpers. The full per-fragment analysis set is serialized into `_srcVideoAnalysis.BatchRawJson` so `CopyRawAnalysisCmd` can copy all fragment JSON.

**VFR handling:** Concat analysis accepts VFR sources. If ffprobe detects VFR on any fragment, `OpenWarnModalCmd` is shown instead of an error. If later fragments do not match the first fragment's frame-rate signature, a warning is emitted instead of aborting. Both warnings point the user to Filter Scribe and its VFR→CFR repair option.

### 5.4 Concat Total Frame Count

Calculated in `ConcatCompatibilityAnalyzer.AnalyzeAsync()` by summing each fragment's first video stream frame count. If any fragment lacks a real frame count, the total is treated as unknown rather than guessed.

The value flows through:
1. `ConcatCompatibilityAnalysisResult.ConcatTotalFrames`
2. `VideoAnalysisM.ConcatTotalFrames`
3. `EncodingPipelineRequest.ConcatTotalFrames`
4. `EncodingPipeline.GetSourceTotalFrames()` — `concatTotalFrames` takes priority over the first fragment's count.
5. `EncodingPipeline.BuildAutoGeneratedEncoderParams()` — frame-count arguments (`--frames` for x264/x265, `-n` for SVT-AV1).
6. `EncodingMonitorVM` (progress bar) and `QueueJobItemVM` (frame count display).

After a successful concat analysis, if `ConcatTotalFrames > 0`, an `OpenDebugModalCmd` is shown with title `Concat {totalFramesLabel}` to help verify correctness.

### 5.5 Frame Count Reliability Policy

When total frame count is missing from video metadata, the system leaves it unknown instead of synthesizing a value from duration and average frame rate. This avoids feeding guessed counts into encoder frame limits or the encoding monitor's progress display. When the total is unknown, frame-based progress is omitted rather than showing a possibly wrong percentage.

## 6. Script Generation & FilterScribe

### 6.1 Per-Route Overview

| Route | One-Click Script Gen | FilterScribe Save & Import |
|-------|----------------------|-----------------------------|
| Single | Writes one AVS and one VPY script for the selected source. | Saves/imports one AVS and one VPY script. |
| Queue | Writes one AVS and one VPY per queue file into a folder. | Saves/imports per-file scripts into a folder. |
| Concat | Writes one AVS and one VPY script containing all fragments. | Saves/imports one concat AVS and one concat VPY script. |
| Repart | Writes execution-specific concat-and-trim sources for each output. | Saves/imports the configured Repart plan; source ordering is chosen before Repart analysis. |

### 6.2 Concat-Specific Flow

`OpenFilterScribeCmd` passes concat delegates into `FilterScribeVM`:
- `isConcatRoute`
- `getConcatFilePaths`

In concat mode, `BrowseSrcConcatCmd` opens the shared `QueueEditorModal` immediately after file selection. The confirmed order is then passed into concat compatibility analysis and `VideoSrcConcatState`, so generated file lists and later scripts use the selected order.

The editor preserves concat's minimum of two sources. When the imported list changes, `MainVM` clears previous source analysis so stale representative JSON and `ConcatTotalFrames` cannot be used for encoding.

The concat VFR→CFR switch works over a list of sources: when enabled, generated concat scripts pass `fpsnum` and `fpsden` to every `LWLibavVideoSource` / `LWLibavSource` call.

**Script Import Validation:** `MainVM.OnSrcImported()` skips the single-source path check when concat mode is active. Single-source mode validates `script → one video path`; concat mode validates the fragment list earlier, then accepts the generated script without forcing it into the single-source check.

**Generated script examples:**

AVS (video-only):
```avs
v1 = LWLibavVideoSource("G:\media\part01.mkv")
v2 = LWLibavVideoSource("G:\media\part02.mkv")
v1 ++ v2
```

VPY uses `core.std.Splice` to combine clips, assigning the result to `src` before the user-edit section and `src.set_output()`.

## 7. Encoding Requests & Execution

### 7.1 Request Routing

`StartEncCmd` branches before queue handling:
1. Repart route: one request per output from `BuildRepartEncodingPipelineRequests()`.
2. Concat route: one request from `BuildConcatEncodingPipelineRequest()`.
3. Queue route: load queue JSON, build one request per accepted source.
4. Single route: one request from `BuildEncodingPipelineRequest()`.

Concat requests set `IsConcatMode: true`, `ConcatFileListPath`, `ConcatVideoSourcePaths`, `ConcatTotalFrames`, and the configured concat `AudioMuxMode`. Repart requests additionally set a per-output `Clip` (time + frame range), `MuxMode: VideoOnly`, `IsRepartMode: true`, `ConcatTotalFrames: plan.TotalFrames`, and the configured repart `AudioMuxMode`.

Concat encoding is supported for `ffmpeg.exe`, `vspipe.exe`, `avs2yuv.exe`, `avs2pipemod.exe`. `one_line_shot_args.exe` is rejected (concat has multiple source files and no SVFI concat route). Queue encoding uses the same supported set and also rejects `one_line_shot_args.exe`.

### 7.2 Upstream Arguments

| Route | ffmpeg | vspipe / avs2yuv / avs2pipemod |
|-------|--------|----------------------------------|
| Single | Source path | Single script path |
| Queue | Per-file source path | Per-file script path (validated against source) |
| Concat | `-i src1 -i src2 ...` + `-filter_complex` concat filter + `-map "[catv]" -fps_mode passthrough -f yuv4mpegpipe -an -strict unofficial -` | Concat script path |
| Repart | `BuildFFmpegRepartArgs` — `-i src1 -i src2 ...` + `-filter_complex` concat filter + frame-exact `trim` + regenerated CFR `setpts`, mapped `[repartv]`, `-f yuv4mpegpipe -an -strict unofficial -` (single source omits concat and uses `-vf "trim=...,setpts=..."`) | Execution-specific virtual source concat script, sliced per output range (`-s/-e` for vspipe, `-seek/-frames` for avs2yuv, `-trim=` for avs2pipemod) |

During auto encoder parameter generation, concat requests pass `ConcatTotalFrames` into `GetFrameCount()` so x264/x265/SVT-AV1 receive the sum of all fragments rather than only the first fragment's `nb_frames`. Repart requests do the same via `ConcatTotalFrames: plan.TotalFrames`.

### 7.3 Queue Batch Execution

After confirmation, `EncodingMonitorVM.RunQueueEncodingAsync()`:
- Adds all jobs to the queue sidebar.
- Saves sidebar state to disk.
- Runs jobs sequentially.
- Marks each job Pending → Encoding → Completed/Failed/Interrupted.
- Stops on the first failure.
- Lets the user stop remaining queue jobs after an interruption.

### 7.4 Progress Detection Flow

The monitor does not fabricate `nb_frames` or guess a total frame count from `duration × avg_frame_rate`. When the total is unknown, progress UI stays unavailable.

1. **`StartEncCmd.Execute()`** builds the `EncodingPipelineRequest` and opens the `EncodingMonitorModal`.
2. **`EncodingMonitorModal.OnLoaded()`** calls **`EncodingMonitorVM.Start()`**, which starts a `DispatcherTimer` (500ms tick) and launches `RunEncodingAsync`.
3. **`RunEncodingAsync`** spawns upstream (e.g., ffmpeg/vspipe) and downstream (encoder) processes, pipes upstream stdout to encoder stdin, and concurrently reads stderr from both processes via `ReadStreamAsync`.
4. **`ReadStreamAsync`** processes stderr character-by-character, splits on `\r`/`\n`, and enqueues lines to a `ConcurrentQueue<ProcessLogEntry>`.
5. **`ProcessQueuedLogs`** (called by the timer) dequeues lines, calls `AppendLogWithOverwrite`, which calls `UpdateProgressFromLogLine`.
6. **`UpdateProgressFromLogLine`** only runs when a reliable total frame count is available. Uses `IsProgressLine` + `InferProgress` (percentage regex) and `TryParseEncoderFrame` (6 frame-extraction regexes) to update `ProgressValue` and `_writtenFrames`.
7. **`UpdateProgressDetails`** (called every 1 second) updates the current output file size.
8. **`UpdateFooterTimes`** estimates remaining time via linear extrapolation only when progress tracking is available: `total = elapsed / (progress/100)`.

## 8. Audio Muxing

| Route | Mux Source |
|-------|------------|
| Single | `SourceVideoPath` |
| Queue | Each request's individual `SourceVideoPath` |
| Concat | Concat filelist (`source_concat_filelist.txt`) as second ffmpeg input |
| Repart | Concat filelist as second ffmpeg input, clipped to the output time range when audio muxing is enabled. |

Concat never muxes audio from `SourceVideoPath` (set to `null`). After video encoding, the mux command uses:
```text
-i encoded_video -f concat -safe 0 -i source_concat_filelist.txt -map 0:v:0 {audio-map-and-codec} -c:v copy ... output
```
Audio is intentionally not loaded by AVS/VPY concat scripts. `EncodingAudioMuxMode` controls `{audio-map-and-codec}` for concat and repart source timelines:

| `EncodingAudioMuxMode` | Concat/Repart audio args |
|------------------------|--------------------------|
| `Disable` | `-an` |
| `Copy` | `-map 1:a:0? -c:a copy` |
| AAC/Opus re-encode modes | `-map 1:a?` plus the selected codec and bitrate args |

This means multi-audio concat/repart sources keep only the first audio track in `Copy` mode, but keep all audio tracks when re-encoding. For VFR sources or complex boundary fragments, audio duration may exceed video duration — post-processing check is recommended.

Repart still uses `EncodingMuxMode.VideoOnly` for request routing and for fallback behavior. When `AudioMuxMode` is `Disable`, or when a clip time range is unavailable, the fallback mux maps only the encoded video stream. Otherwise the repart mux step opens the concat filelist with `-ss`/`-to`, maps audio according to `EncodingAudioMuxMode`, and intentionally excludes source subtitles, chapters, attachments, and metadata.

## 9. Path Resolution Summary

| Method | Single | Queue | Concat | Repart |
|--------|--------|-------|--------|--------|
| `GetCurrentVideoSrcPath()` | Selected single source path | Non-queue/non-concat/non-repart path only | Non-queue/non-concat/non-repart path only | Non-queue/non-concat/non-repart path only |
| `GetSelectedVideoSrcPath()` | Selected single source path | Same helper, usually empty | Same helper, usually empty | Same helper, usually empty |
| `GetCurrentSrcImportPath()` | Current single source path | Queue folder path | Concat first-file parent folder | Repart first-source path |
| `GetPreviewSourceVideoPath()` | Single source path | First queue file | First concat file | First Repart source |
| `GetCurrentQueueFilePaths()` | Not used | Accepted queue files | Not used | Not used |
| `GetConcatFilePaths()` | Not used | Not used | Ordered concat fragment paths | Not used |
| `GetRepartFilePaths()` | Not used | Not used | Not used | Committed plan source paths |

## 10. Route-Specific Constraints

| Constraint | Single | Queue | Concat | Repart |
|------------|--------|-------|--------|--------|
| `one_line_shot_args.exe` | Supported | Rejected | Rejected | Rejected |
| Sample Clip | Supported | Disabled | Disabled | Disabled |
| Duration filter | Not shown | Supported | Not shown | Not shown |
| Partial source acceptance | N/A | Yes, after queue filtering | No | No (source excluded by strict filters) |
| Import method | Single file | Folder | Multi-select files | Folder |
| Output cardinality | One output | Many outputs | One output | Many outputs |

Repart-specific constraints:

- One or more input files produce one or more independently named outputs.
- CFR and complete frame timestamps are required.
- Video stream format fields must match exactly; container, audio, and subtitle layouts are not part of the signature. Per-stream `time_base` is a container detail and is likewise excluded (see `time_base` handling below).
- Chapter-folder import is implemented for disc playlists. Multi-entry MPLS sources are combined into one virtual source, and mixed STREAM folders resolve to the dominant matching episode group instead of the first file.
- Source Reviser remains disabled for an active plan because changing source metadata could invalidate frame offsets. Filter Scribe is available through the Repart-specific concat-style workflow. Imported sources are never modified.
- ffmpeg is required for the final MKV mux step even when the upstream is vspipe or AviSynth.

### Repart Runtime Implementation

Clicking `Tool.Source.VideoSrcRepart` opens an import flow that can read either a plain STREAM folder or a chapter-folder/PLAYLIST folder. The shared `QueueEditorModal` first lets the user confirm the source order, before the slow Repart analysis starts. The Repart configuration modal then contains a read-only source queue, a proportional partition map, synchronized time/frame fields, and an output queue. Source changes are handled by clearing and re-importing the Repart source, so the modal remains focused on repartition editing. Output ranges use inclusive first/last frames; ranges cannot overlap, gaps are allowed, and merge accepts only directly adjacent outputs.

`RepartCompatibilityAnalyzer` performs a full ffprobe frame timestamp scan for each source. It requires CFR, derives the actual frame count, compares a strict first-video-stream signature, and records source size/modification-time fingerprints. When several signature groups are present, the dominant group by total source size is treated as the reference so menu/trailer files do not displace the episode set. A plan is rejected if a source changes during analysis or before encoding.

Rare edge case: some long-GOP BDMV titles can make the short seek-based frame verification miss the last frame near the tail of the title. When that happens, the implementation widens the seek window first, then falls back to ffmpeg, and only uses full `ffprobe -count_frames` as the slowest last resort.

**`time_base` handling.** The per-source signature deliberately omits `time_base`. It is the container's tick resolution (e.g. `1/24000` vs `1/96000` for the same 24000/1001 fps), a muxing detail rather than a video property, and identical encode batches can mux episodes at different tick resolutions. Sources that differ only in `time_base` are therefore accepted as one virtual timeline. Every downstream stage normalizes it: ffmpeg resets each input's PTS with `setpts=PTS-STARTPTS` and rebuilds a CFR PTS sequence with `setpts=N*den/(num*TB)` — TB is ffmpeg's internal output timebase, independent of the source's — VapourSynth/AviSynth splice frame-accurately by frames, and the final mux writes `-video_track_timescale` from the reference source's `time_base` denominator uniformly across every output.

Each encoding start creates execution-specific ffconcat and private AVS/VPY paths. `BuildRepartEncodingPipelineRequests()` emits one `EncodingPipelineRequest` per committed output (`RepartPlanM.Outputs`); every request carries an `EncodingClipRequest` with both time (`StartTime`/`EndTime`) and frame (`FirstFrame`/`LastFrame`) ranges plus the plan frame rate, `ConcatFileListPath`, `ConcatVideoSourcePaths`, `ConcatTotalFrames: plan.TotalFrames`, and `MuxMode: EncodingMuxMode.VideoOnly`.

**Concat-then-split commands per upstream** (frame ranges are global indices over the concatenated virtual timeline):

- **ffmpeg** — `EncodingPipeline.BuildFFmpegRepartArgs()` opens every source with its own `-i`, maps each source's `v:0` only, chains `[i:v:0]setpts=PTS-STARTPTS[rv i]`, joins the segments in import order with `concat=n=N:v=1:a=0`, crops with frame-exact `trim=start_frame={first}:end_frame={last+1}`, regenerates the CFR PTS sequence with `setpts=N*den/(num*TB)`, then pipes `-map "[repartv]" -f yuv4mpegpipe -an -strict unofficial -`. A single source omits the concat stage and uses `-vf "trim=...,setpts=..."` directly. Audio is intentionally absent from the upstream video pipe; the later mux step handles audio from the concat filelist when `AudioMuxMode` is not `Disable`.
- **vspipe** — writes a private `.vpy` whose source header splices all sources via `core.std.Splice` (`ScriptTemplate.BuildConcatVpySourceHeader`), then slices with `-s {first} -e {last}`.
- **avs2yuv / avs2pipemod** — writes a private `.avs` whose source header joins all sources with AviSynth `++` UnalignedSplice (`ScriptTemplate.BuildConcatAvsSourceHeader`), then slices with `-seek {first} -frames {count}` (avs2yuv) or `-trim={first},{last}` (avs2pipemod).

**Filter placement rule.** Imported sources are never modified. Repart Filter Scribe shows and orders the output episodes generated by the Repart plan, while using the Repart source list internally to build a temporary concat-style source and apply the selected filters. Source Reviser remains disabled while a Repart plan is active because changing source metadata requires rebuilding the plan. Repart ffmpeg requests retain `FFmpegFilterArgs`, while AVS/VPY execution scripts include the saved Filter Scribe body.

`EncodingMonitorVM` executes the requests through the existing sequential batch engine with a non-persistent `RepartOutputSidebarPanel`; mux is locked on and writes the final MKV for each output. Requests share the virtual source but carry distinct `EncodingClipRequest` ranges, output paths, clip frame totals, and repart audio mux mode.

## 11. End-to-End Flows

**Queue:** `Video Src. Queue` selected → import folder → analyze queue → accept filtered files → write queue JSON → press `Start Encode` → load queue JSON → build per-file requests → confirm overwrite → open monitor → run the batch one job at a time.

**Concat:** `Video Src. Concat` selected → import multiple files → extension and compatibility precheck → write filelist → analyze all fragments (sum concat total frame count and store all raw JSON) → optionally reorder/remove in FilterScribe → regenerate filelist and clear stale analysis if list changed → rerun analysis if needed → save/import concat script if needed → press `Start Encode` → build one concat request with `ConcatTotalFrames` → confirm command/overwrite → encode one output (progress uses summed frame count) → mux audio from filelist.

**Repart:** `Tool.Source.VideoSrcRepart` selected → import a folder → strictly analyze and order sources → open `RepartConfModal` → manually allocate output frame ranges → optionally leave unallocated gaps or merge adjacent outputs → apply the plan → select output directory and encoding settings → press `Start Encode` → create an execution-specific virtual source → build one Clip request per output → confirm overwrite targets → run sequentially with `RepartOutputSidebarPanel` → mux each encoded video into the final MKV, with audio handled by the configured repart `AudioMuxMode`.

## Key Files

- `FileManagement/SrcRouteKind.cs` — Route enum
- `QueueManagement/VideoSrcQueue.cs` — Queue state
- `ConcatManagement/VideoSrcConcat.cs` — Concat state
- `ConcatManagement/ConcatFileListGenerator.cs` — Filelist generation
- `ConcatManagement/ConcatCompatibilityAnalyzer.cs` — Per-fragment analysis
- `RepartManagement/RepartCompatibilityAnalyzer.cs` — Strict CFR and frame-timeline analysis
- `RepartManagement/VideoSrcRepartState.cs` — Committed Repart plan state
- `Models/RepartPlanM.cs` — Repart sources, stream signature, and output ranges
- `Views/RepartConfModal.xaml` / `ViewModels/RepartConfVM.cs` — Partition-style configuration window
- `Components/RepartOutputSidebarPanel.xaml` — Repart monitor sidebar
- `Commands/BrowseSrcPathCmd.cs` — Single source import
- `Commands/BrowseSrcQueueCmd.cs` — Queue folder import
- `Commands/BrowseSrcConcatCmd.cs` — Concat multi-select import
- `Commands/BrowseSrcScriptQueueCmd.cs` — Queue script import
- `Commands/AnalyzeSrcVideoCmd.cs` — Source analysis (all routes)
- `Commands/StartEncCmd.cs` — Encoding startup (all routes)
- `Commands/CopyRawAnalysisCmd.cs` — Raw JSON copy
- `Commands/OpenClose/OpenFilterScribeCmd.cs` — FilterScribe launch
- `Commands/OpenClose/OpenDebugModalCmd.cs` — Concat frame count debug
- `Commands/SaveLoad/OneClickScriptGenCmd.cs` — One-click script generation
- `ViewModels/FilterScribeVM.cs` — Filter editor
- `ViewModels/EncodingMonitorVM.cs` — Encoding monitor
- `ViewModels/QueueJobItemVM.cs` — Queue job item
- `Commands/OpenClose/OpenQueueEditorCmd.cs` — Shared queue ordering modal
- `Pipeline/EncodingPipeline.cs` — Command building, upstream args, frame count, mux
- `Models/VideoAnalysisM.cs` — Source analysis model
- `ScriptGeneration/ScriptTemplate.cs` — AVS/VPY script templates
