# Encoding Mode Implementation: Single, Queue, Concat, and Repart

> For a high-level overview of the four encoding modes, see [Encoding Modes](ConceptsAndFeatures-EN.md#encoding-modes). This document covers runtime implementation details for each route.

## 1. Route Activation

`FileManagement/SourceRouteKind.cs` defines the route enum:

```csharp
public enum SourceRouteKind { Single, Queue, Concat, Repart }
```

`MainVM.GetActiveSourceRoute()` is the central route resolver:

```csharp
private SourceRouteKind GetActiveSourceRoute()
{
    if (_videoSourceQueue.IsActive) return SourceRouteKind.Queue;
    if (_videoSourceConcat.IsActive) return SourceRouteKind.Concat;
    if (_videoSourceRepart.IsActive) return SourceRouteKind.Repart;
    return SourceRouteKind.Single;
}
```

`VideoSourceQueueState` lives in `QueueManagement/VideoSourceQueue.cs`; `VideoSourceConcatState` lives in `ConcatManagement/VideoSourceConcat.cs`; `VideoSourceRepartState` lives in `RepartManagement/VideoSourceRepartState.cs`.

## 2. Import Zone Layout

`ToolCatalogProviderM.GetVideoSrcImportDefs()` returns four video source cards:

| Index | Route | Card | R1 | R2 | State Owner |
|-------|-------|------|----|----|-------------|
| 0 | Single | `Tool.Source.VideoSource` | Replace | Clear | `AppDataM.Tools.VideoSourcePath` |
| 1 | Queue | `Tool.Source.VideoSrcQueue` | Import | Clear | `VideoSourceQueueState` |
| 2 | Concat | `Tool.Source.VideoSrcConcat` | Import | Clear | `VideoSourceConcatState` |
| 3 | Repart | `Video Source Repart` | Import | Clear | `VideoSourceRepartState` |

When `one_line_shot_args.exe` is selected, Queue, Concat, and Repart are deselected and disabled.

## 3. Active UI Routing

`MainVM.RefreshActiveSourceRoute()` switches validation cards and script zones by route.

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

`BrowseSourcePathCmd` imports one source path. `OnVideoSourceImported()` clears competing source selections and stores the selected path in app data.

### 4.2 Queue

`BrowseSourceQueueCmd` imports a folder of video files. `BrowseSourceScriptQueueCmd` imports a folder of matching script files for the selected script kind.

After import:
- `P2TextData` stores the folder path.
- `P1TextData` stores a compact label such as `firstFile..lastFile`.
- `P1TooltipText` stores the full comma-separated file list for hover text (truncated at 512 chars).

`VideoSourceQueueState` keeps the accepted file paths in a `Dictionary<ToolItemCardVM, string[]>` keyed by the queue card. `ApplyAcceptedFiles()` replaces that list after analysis and refreshes the queue label.

Script queue imports are validated against the current video queue before they are accepted. `MainVM.ValidateScriptQueueImport()` checks basename matches and embedded source paths so queue scripts stay aligned with the selected source list.

### 4.3 Concat

`BrowseSourceConcatCmd` opens a multi-select `OpenFileDialog` (minimum two files). Before accepting the selection, it checks that all selected files use supported video extensions and have the **same extension**. If any selected file differs, import aborts and opens `OpenErrModalCmd` with the expected extension and mismatched files.

The import command also runs a compatibility pre-analysis with ffprobe before accepting the files. This pre-analysis is an import gate only: it can reject or warn about the selection, but the accepted result is not stored into `_srcVideoAnalysis`. After import, the normal source-analysis flow still runs.

After import:
- `P2TextData` stores the first file's parent directory.
- `P1TextData` stores a compact label such as `firstFile..lastFile`.
- `P1TooltipText` stores the full selected file list for hover text.
- `VideoSourceConcatState` stores the ordered full file paths.
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

On success, the first fragment's raw ffprobe JSON is stored in `_srcVideoAnalysis.RawJson` and drives encoder config previews and FilterScribe helpers. The full per-fragment analysis set is serialized into `_srcVideoAnalysis.QueueRawJson` so `CopyRawAnalysisCmd` can copy all fragment JSON.

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

### 6.2 Concat-Specific Flow

`OpenFilterScribeCmd` passes concat delegates into `FilterScribeVM`:
- `isConcatRoute`
- `getConcatFilePaths`
- `applyConcatFilePaths`

In concat mode, `FilterScribeVM` loads `ConcatSourceListVM`, and `FilterScribeModal` displays `ConcatSidebarPanel` on the left with support for:
- Remove fragment
- Move fragment up / down

Every reorder/remove operation updates `VideoSourceConcatState` and regenerates `source_concat_filelist.txt` via `applyConcatFilePaths`. When the list changes, `MainVM` clears previous source analysis so stale representative JSON and `ConcatTotalFrames` cannot be used for encoding.

The concat VFR→CFR switch works over a list of sources: when enabled, generated concat scripts pass `fpsnum` and `fpsden` to every `LWLibavVideoSource` / `LWLibavSource` call.

**Script Import Validation:** `MainVM.OnSourceImported()` skips the single-source path check when concat mode is active. Single-source mode validates `script → one video path`; concat mode validates the fragment list earlier, then accepts the generated script without forcing it into the single-source check.

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
1. Concat route: one request from `BuildConcatEncodingPipelineRequest()`.
2. Queue route: load queue JSON, build one request per accepted source.
3. Single route: one request from `BuildEncodingPipelineRequest()`.

Concat requests set `IsConcatMode: true`, `ConcatFileListPath`, and `ConcatTotalFrames`.

Concat encoding is supported for `ffmpeg.exe`, `vspipe.exe`, `avs2yuv.exe`, `avs2pipemod.exe`. `one_line_shot_args.exe` is rejected (concat has multiple source files and no SVFI concat route). Queue encoding uses the same supported set and also rejects `one_line_shot_args.exe`.

### 7.2 Upstream Arguments

| Route | ffmpeg | vspipe / avs2yuv / avs2pipemod |
|-------|--------|----------------------------------|
| Single | Source path | Single script path |
| Queue | Per-file source path | Per-file script path (validated against source) |
| Concat | `-f concat -safe 0 -i source_concat_filelist.txt -f yuv4mpegpipe -an -strict unofficial -` | Concat script path |

During auto encoder parameter generation, concat requests pass `ConcatTotalFrames` into `GetFrameCount()` so x264/x265/SVT-AV1 receive the sum of all fragments rather than only the first fragment's `nb_frames`.

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
| Repart | No source-stream input; only the encoded video is muxed into MKV. |

Concat never muxes audio from `SourceVideoPath` (set to `null`). After video encoding, the mux command uses:
```text
-i encoded_video -f concat -safe 0 -i source_concat_filelist.txt -map 0:v:0 -map 1:a? -c:v copy -c:a copy ... output
```
Audio is intentionally not loaded by AVS/VPY concat scripts. For VFR sources or complex boundary fragments, audio duration may exceed video duration — post-processing check is recommended.

Repart uses `EncodingMuxMode.VideoOnly`. The mux step maps only the encoded video stream; source audio, subtitles, chapters, attachments, and metadata are intentionally excluded.

## 9. Path Resolution Summary

| Method | Single | Queue | Concat |
|--------|--------|-------|--------|
| `GetCurrentVideoSourcePath()` | Selected single source path | Non-queue/non-concat path only | Non-queue/non-concat path only |
| `GetSelectedVideoSourcePath()` | Selected single source path | Same helper, usually empty | Same helper, usually empty |
| `GetCurrentSourceImportPath()` | Current single source path | Queue folder path | Concat first-file parent folder |
| `GetPreviewSourceVideoPath()` | Single source path | First queue file | First concat file |
| `GetCurrentQueueFilePaths()` | Not used | Accepted queue files | Not used |
| `GetConcatFilePaths()` | Not used | Not used | Ordered concat fragment paths |

## 10. Route-Specific Constraints

| Constraint | Single | Queue | Concat |
|------------|--------|-------|--------|
| `one_line_shot_args.exe` | Supported | Rejected | Rejected |
| Sample Clip | Supported | Disabled | Disabled |
| Duration filter | Not shown | Supported | Not shown |
| Partial source acceptance | N/A | Yes, after queue filtering | No |
| Import method | Single file | Folder | Multi-select files |
| Output cardinality | One output | Many outputs | One output |

Repart-specific constraints:

- One or more input files produce one or more independently named outputs.
- CFR and complete frame timestamps are required.
- Video stream format fields must match exactly; container, audio, and subtitle layouts are not part of the signature.
- Chapter and MPLS reading are not implemented; episode boundaries are manual.
- Source Reviser and Filter Scribe are disabled for an active plan because they could invalidate frame offsets.
- ffmpeg is required for the final video-only MKV even when the upstream is vspipe or AviSynth.

### Repart Runtime Implementation

Clicking `Video Source Repart` imports a naturally sorted folder and opens `RepartConfModal`. The modal contains a read-only source queue, a disabled chapter/MPLS placeholder, a proportional partition map, synchronized time/frame fields, and an output queue. Source changes are handled by clearing and re-importing the Repart source, so the modal remains focused on repartition editing. Output ranges use inclusive first/last frames; ranges cannot overlap, gaps are allowed, and merge accepts only directly adjacent outputs.

`RepartCompatibilityAnalyzer` performs a full ffprobe frame timestamp scan for each source. It requires CFR, derives the actual frame count, compares a strict first-video-stream signature, and records source size/modification-time fingerprints. A plan is rejected if a source changes during analysis or before encoding.

Each encoding start creates execution-specific ffconcat and private AVS/VPY paths. Script upstreams use a generated virtual-source script, so a stale or reordered external script cannot invalidate planned frame offsets. The ffmpeg route opens every source separately, maps only `v:0`, joins the streams with the video concat filter, and applies frame-based `trim` plus a regenerated CFR PTS sequence. Audio and subtitle layouts therefore do not participate in compatibility.

One `EncodingPipelineRequest` is created per output. Requests share the virtual source but carry distinct `EncodingClipRequest` ranges, output paths, and clip frame totals. `EncodingMonitorVM` executes them through the existing sequential batch engine with a non-persistent `RepartOutputSidebarPanel`; mux is locked on and writes a video-only MKV for each output.

## 11. End-to-End Flows

**Queue:** `Video Src. Queue` selected → import folder → analyze queue → accept filtered files → write queue JSON → press `Start Encode` → load queue JSON → build per-file requests → confirm overwrite → open monitor → run the batch one job at a time.

**Concat:** `Video Src. Concat` selected → import multiple files → extension and compatibility precheck → write filelist → analyze all fragments (sum concat total frame count and store all raw JSON) → optionally reorder/remove in FilterScribe → regenerate filelist and clear stale analysis if list changed → rerun analysis if needed → save/import concat script if needed → press `Start Encode` → build one concat request with `ConcatTotalFrames` → confirm command/overwrite → encode one output (progress uses summed frame count) → mux audio from filelist.

**Repart:** `Video Source Repart` selected → import a folder → strictly analyze and order sources → open `RepartConfModal` → manually allocate output frame ranges → optionally leave unallocated gaps or merge adjacent outputs → apply the plan → select output directory and encoding settings → press `Start Encode` → create an execution-specific virtual source → build one Clip request per output → confirm overwrite targets → run sequentially with `RepartOutputSidebarPanel` → mux each encoded video into a video-only MKV.

## Key Files

- `FileManagement/SourceRouteKind.cs` — Route enum
- `QueueManagement/VideoSourceQueue.cs` — Queue state
- `ConcatManagement/VideoSourceConcat.cs` — Concat state
- `ConcatManagement/ConcatFileListGenerator.cs` — Filelist generation
- `ConcatManagement/ConcatCompatibilityAnalyzer.cs` — Per-fragment analysis
- `RepartManagement/RepartCompatibilityAnalyzer.cs` — Strict CFR and frame-timeline analysis
- `RepartManagement/VideoSourceRepartState.cs` — Committed Repart plan state
- `Models/RepartPlanM.cs` — Repart sources, stream signature, and output ranges
- `Views/RepartConfModal.xaml` / `ViewModels/RepartConfVM.cs` — Partition-style configuration window
- `Components/RepartOutputSidebarPanel.xaml` — Repart monitor sidebar
- `Commands/BrowseSourcePathCmd.cs` — Single source import
- `Commands/BrowseSourceQueueCmd.cs` — Queue folder import
- `Commands/BrowseSourceConcatCmd.cs` — Concat multi-select import
- `Commands/BrowseSourceScriptQueueCmd.cs` — Queue script import
- `Commands/AnalyzeSrcVideoCmd.cs` — Source analysis (all routes)
- `Commands/StartEncCmd.cs` — Encoding startup (all routes)
- `Commands/CopyRawAnalysisCmd.cs` — Raw JSON copy
- `Commands/OpenClose/OpenFilterScribeCmd.cs` — FilterScribe launch
- `Commands/OpenClose/OpenDebugModalCmd.cs` — Concat frame count debug
- `Commands/SaveLoad/OneClickScriptGenCmd.cs` — One-click script generation
- `ViewModels/FilterScribeVM.cs` — Filter editor
- `ViewModels/EncodingMonitorVM.cs` — Encoding monitor
- `ViewModels/QueueJobItemVM.cs` — Queue job item
- `Components/ConcatSidebarPanel.xaml` — Concat reorder sidebar
- `Pipeline/EncodingPipeline.cs` — Command building, upstream args, frame count, mux
- `Models/VideoAnalysisM.cs` — Source analysis model
- `ScriptGeneration/ScriptTemplate.cs` — AVS/VPY script templates
