# Concat Mode Encoding

> For a high-level overview of concat mode, see [Concat Mode](ConceptsAndFeatures-EN.md#concat-mode). This document covers runtime implementation details.

## 1. What Concat Mode Is

Concat mode is the `Video Src. Concat` card in `VideoSrcImportZone[2]`. `VideoSourceConcatState.IsActive` becomes true when that card is selected, and `MainVM.RefreshActiveSourceRoute()` switches the app to the concat validation card while keeping the normal script source zone.

Concat mode is not queue mode. Queue mode means many source files produce many jobs and many outputs. Concat mode means many fragments become one logical source, one `EncodingPipelineRequest`, and one output.

When concat mode is active:

- The concat card title shows the current fragment count.
- `ActiveSrcValidationCard` is `ConcatCheckCard`.
- `Sample Clip` is disabled for the button state; if invoked directly, the command shows a warning and returns.
- The duration filter is hidden.
- `FilterScribeModal` shows `ConcatSidebarPanel` for reorder/remove operations.

## 2. Importing Concat Sources

`BrowseSourceConcatCmd` opens a multi-select `OpenFileDialog`. It does not import a folder, and concat import requires at least two selected video files.

Before accepting the selection, it checks that all selected files use supported video extensions and have the same extension. If any selected file differs, import aborts and opens `OpenErrModalCmd` with the expected extension and mismatched files.

The import command also runs a compatibility pre-analysis with ffprobe before accepting the files. This pre-analysis is an import gate only: it can reject or warn about the selection, but the accepted result is not stored into `_srcVideoAnalysis`. After import, the normal source-analysis flow still runs to populate representative raw JSON, all-fragment raw JSON, and concat total frame count.

After import:

- `P2TextData` stores the first file's parent directory.
- `P1TextData` stores a compact label such as `firstFile..lastFile`.
- `P1TooltipText` stores the full selected file list for hover text.
- `VideoSourceConcatState` stores the ordered full file paths.
- `source_concat_filelist.txt` is generated under the app config directory (`1cenc`).

The filelist uses ffmpeg concat-demuxer syntax with absolute paths and `-safe 0` support:

```text
file 'G:/media/part01.mkv'
file 'G:/media/part02.mkv'
```

## 3. Concat Source Analysis

`AnalyzeSrcVideoCmd` has a concat branch. It requires at least two selected fragments, analyzes every selected fragment with ffprobe, and supplements frame counts the same way single and queue analysis do.

Concat analysis has hard failures and soft compatibility warnings:

- If any file fails ffprobe, the whole concat source fails.
- If any file lacks a readable video stream, the whole concat source fails.
- If any file differs from the first file's resolution, the whole concat source fails.
- If codec, pixel format, source validation checklist signature, or CFR frame-rate signature differs, analysis emits `OpenWarnModalCmd` and continues.

The comparison signature currently includes:

- source validation checklist signature
- width
- height
- pixel format
- codec
- normalized `avg_frame_rate`
- normalized `r_frame_rate`

On success, the first fragment's raw ffprobe JSON is stored in `_srcVideoAnalysis.RawJson` and applied to `ConcatCheckCard`. This representative analysis drives encoder config previews and FilterScribe helpers. The full per-fragment analysis set is also serialized into `_srcVideoAnalysis.QueueRawJson` so `CopyRawAnalysisCmd` can copy all fragment JSON in concat mode.

### 3.1 VFR Handling in Concat Mode

Concat analysis accepts variable frame rate (VFR) sources. If ffprobe detects VFR on any fragment, `BrowseSourceConcatCmd` and `AnalyzeSrcVideoCmd` show `OpenWarnModalCmd` instead of an error modal and continue.

If later fragments do not match the first fragment's frame-rate signature, the analysis also emits a warning instead of aborting. Both warnings point the user to Filter Scribe and its VFR→CFR repair option.

This is important because many source collections are mixed VFR and should still be importable for concat workflows.

### 3.2 Concat Total Frame Count

Concat mode calculates a **concat total frame count** — the sum of every fragment's individual frame count. This value is computed in `ConcatCompatibilityAnalyzer.AnalyzeAsync()` by iterating over each fragment's supplemented ffprobe JSON and summing the first video stream's frame count. The frame-count reader accepts `nb_frames`, `NUMBER_OF_FRAMES*` tags, and frame counts supplemented from duration × average frame rate.

The summed value flows through these stages:

1. Stored in `ConcatCompatibilityAnalysisResult.ConcatTotalFrames`.
2. Passed into `VideoAnalysisM.ConcatTotalFrames` after analysis.
3. Forwarded into `EncodingPipelineRequest.ConcatTotalFrames` when building the encoding request.
4. Used by `EncodingPipeline.GetSourceTotalFrames()` — the optional `concatTotalFrames` parameter takes priority over the first fragment's single-source frame count.
5. Used by `EncodingPipeline.BuildAutoGeneratedEncoderParams()` when generating encoder frame-count arguments (`--frames` for x264/x265 and `-n` for SVT-AV1).
6. Consumed in `EncodingMonitorVM` (progress tracking) and `QueueJobItemVM` (frame count display).

This ensures that encoder-side frame limits, encoding progress percentage, and frame-count display reflect the true total across all concatenated fragments, rather than only the first fragment's frame count.

### 3.3 Debug Modal for Concat Total Frame Count

After a successful concat analysis, if `ConcatTotalFrames > 0`, an `OpenDebugModalCmd` is shown with the title `Concat {totalFramesLabel}` and a localized message such as `Total Frames (Concat source total): 12345`.

This debug modal helps verify that the summed frame count is correct across all fragments.

## 4. Script Generation

Concat mode generates one AVS script and one VPY script for the whole fragment list. AVS/VPY generation requires at least two concat paths; `OneClickScriptGenCmd` and FilterScribe copy/save paths block generation below that count.

`OneClickScriptGenCmd` uses:

- `ScriptTemplate.BuildConcatAvsExportScript()`
- `ScriptTemplate.BuildConcatVpyExportScript()`

AVS scripts are video-only. The final concatenated clip is the last expression in the source header:

```avs
v1 = LWLibavVideoSource("G:\media\part01.mkv")
v2 = LWLibavVideoSource("G:\media\part02.mkv")
v1 ++ v2
# Add more filters below or leave empty...
# ...end of edit section
```

VPY scripts load each fragment with `LWLibavSource`, then splice clips with `core.std.Splice`. The final clip is also assigned to `src` before the user-edit section and `src.set_output()`.

Audio is intentionally not loaded by AVS/VPY concat scripts.

## 5. FilterScribe Flow

`OpenFilterScribeCmd` passes concat delegates into `FilterScribeVM`:

- `isConcatRoute`
- `getConcatFilePaths`
- `applyConcatFilePaths`

In concat mode, `FilterScribeVM` loads `ConcatSourceListVM`, and `FilterScribeModal` displays `ConcatSidebarPanel` on the left.

The concat AVS and VPY source builders also have `LWLibavVideoSource VFR→CFR` and `LWLibavSource VFR→CFR` switches working, but differently since it applies to a list of sources.
When the source analysis marks the inputs as VFR and the checkbox is enabled, the generated concat scripts pass `fpsnum` and `fpsden` through to every `LWLibavVideoSource` / `LWLibavSource` call, matching the behavior already used in single-source mode.

The sidebar supports:

- Remove fragment
- Move fragment up
- Move fragment down

Every reorder/remove operation updates `VideoSourceConcatState` and regenerates `source_concat_filelist.txt` through the `applyConcatFilePaths` callback. When the list changes, `MainVM` clears the previous source analysis so stale representative JSON and `ConcatTotalFrames` cannot be used for encoding. Run source analysis again after changing the fragment order or removing fragments. `Save & Import` also uses the current sidebar order when writing AVS/VPY scripts.

### 5.1 Script Import Validation

`MainVM.OnSourceImported()` still runs the normal script import path for concat scripts, but it skips the single-source path check when concat mode is active.

That guard exists because the regular source-check logic assumes a 1:1 relationship between a script and a single video file. Concat scripts are different: they intentionally embed multiple fragment paths, and the correct result is a multi-file script that maps to the current concat fragment list.

In other words:

- single-source mode validates `script -> one video path`
- concat mode validates the fragment list earlier, then accepts the generated script without trying to force it into the single-source check

This prevents a correct concat export/import from being rejected by `OpenErrModalCmd` as a false `SourcePathMismatch`.

## 6. Starting Concat Encoding

`StartEncCmd` checks concat before queue. In concat mode it calls `MainVM.BuildConcatEncodingPipelineRequest()` and starts a normal single request flow, including debug confirmation and overwrite confirmation. The request builder returns `null` unless at least two concat fragments are still present.

Concat encoding is supported for:

- `ffmpeg.exe`
- `vspipe.exe`
- `avs2yuv.exe`
- `avs2pipemod.exe`

`one_line_shot_args.exe` is rejected because concat mode has multiple source files and no SVFI concat route.

For `ffmpeg.exe`, `BuildConcatEncodingPipelineRequest()` sets a placeholder upstream input path but the actual upstream input is `ConcatFileListPath`. `EncodingPipeline.BuildUpstreamArgs()` emits:

```text
-hide_banner -f concat -safe 0 -i source_concat_filelist.txt -f yuv4mpegpipe -an -strict unofficial -
```

For `vspipe.exe` and AviSynth upstream tools, the upstream input is the selected concat script generated/imported through the normal script source zone.

## 7. Audio Muxing

Concat mode never muxes audio from `SourceVideoPath`; concat requests set `SourceVideoPath` to `null`.

After video encoding, `EncodingPipeline.BuildMuxCommand()` uses the concat filelist as the second ffmpeg input:

```text
-i encoded_video -f concat -safe 0 -i source_concat_filelist.txt -map 0:v:0 -map 1:a? -c:v copy -c:a copy ... output
```

This keeps upstream pipes video-only while allowing audio streams from the concatenated source list to be copied into the final output.

## 8. End-to-End Flow

`Video Src. Concat` selected -> import multiple files -> extension and compatibility precheck -> write filelist -> analyze all fragments (sum concat total frame count and store all raw JSON) -> optionally reorder/remove in FilterScribe -> regenerate filelist and clear stale analysis if the list changed -> rerun analysis if needed -> save/import concat script if needed -> press `Start Encode` -> build one concat request with `ConcatTotalFrames` -> confirm command/overwrite -> encode one output (progress uses summed frame count) -> mux audio from filelist.

## Key Files

- `ConcatManagement/VideoSourceConcat.cs`
- `ConcatManagement/ConcatFileListGenerator.cs`
- `ConcatManagement/ConcatCompatibilityAnalyzer.cs`
- `Commands/BrowseSourceConcatCmd.cs`
- `Commands/AnalyzeSrcVideoCmd.cs`
- `Commands/StartEncCmd.cs`
- `Commands/OpenClose/OpenDebugModalCmd.cs`
- `Commands/SaveLoad/OneClickScriptGenCmd.cs`
- `Commands/OpenClose/OpenFilterScribeCmd.cs`
- `Commands/CopyRawAnalysisCmd.cs`
- `ViewModels/FilterScribeVM.cs`
- `ViewModels/EncodingMonitorVM.cs`
- `ViewModels/QueueJobItemVM.cs`
- `Components/ConcatSidebarPanel.xaml`
- `Pipeline/EncodingPipeline.cs`
- `Models/VideoAnalysisM.cs`
- `ScriptGeneration/ScriptTemplate.cs`
