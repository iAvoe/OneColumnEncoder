# Queue Mode Encoding

> For a high-level overview of queue mode, see [Queue Mode](ConceptsAndFeatures-EN.md#queue-mode). This document covers runtime implementation details.

## 1. What Queue Mode Is
Queue mode is the `Video Src. Queue` card in `VideoSrcImportZone[1]`. `VideoSourceQueueState.IsActive` becomes true when that card is selected, and `MainVM.RefreshActiveSourceRoute()` switches the app to the queue-specific script zone and validation card.

When queue mode is active:
- The queue card title shows the current item count.
- The active script zone switches to `QueueScriptSrcImportZone`.
- `Sample Clip` is disabled.

## 2. Importing Queue Sources
`BrowseSourceQueueCmd` imports a folder of video files. `BrowseSourceScriptQueueCmd` imports a folder of matching script files for the selected script kind.

After import:
- `P2TextData` stores the folder path.
- `P1TextData` stores a compact label such as `firstFile..lastFile`.
- `P1TooltipText` stores the full file list for hover text.

`VideoSourceQueueState` keeps the accepted file paths in a `Dictionary<ToolItemCardVM, string[]>` keyed by the queue card. `ApplyAcceptedFiles()` replaces that list after analysis and refreshes the queue label.

Script queue imports are validated against the current video queue before they are accepted. `MainVM.ValidateScriptQueueImport()` checks basename matches and embedded source paths so queue scripts stay aligned with the selected source list.

## 3. Queue Source Analysis
When `AnalyzeSrcVideoCmd` runs in queue mode, it analyzes each file with ffprobe, keeps frame counts unknown when ffprobe does not provide them, and builds a signature for consistency checks. If queue filtering is enabled, the user chooses one of two reference strategies:
- `FirstStream`
- `WeightedVoteThenFirstStream`

If the queue card is bypassed, the signature filter is skipped. Files that do not match the chosen reference are excluded. Failures on individual files are reported, but the rest of the queue continues.

The analysis writes timestamped JSON files under the app config directory (`1cenc`):
- `source_queue_*.json` for accepted files
- `source_queue_excluded_*.json` when any files are filtered out

The queue JSON keeps the reference file path, each accepted file path, and raw ffprobe JSON. That data is reused later for duration filtering and queue request building.

### Explain: FirstStream strategy
The first stream strategy maximizes queue source video similarity by the video source validation card checklist, which means
- Video w/ different frame rate, resolution will be kept, since automatic encoding parameter generation can handle it
- Video lacks color-matrix, tansfer, etc may be kept, as long as the checklist success-fail pattern is the same as the first video's pattern
- This strategy makes ffmpeg-VS-AVS filter-fixing mechanism universally applicable for this queue

1. Run FFProbe analysis on the first stream
2. Recoed validation result patterns
3. Continue to run analysis, exclude sources that has a different validation pattern

### Explain: WeightedVote strategy
Consider a typical BluRay stream folder structure:
- The “real video source” we want is 1 single stream (movie) or multiple streams (TV episodes)
- Multiple short video for intros, branding logos, trailers, ads (and some of them are not even readable), clustered before and after “real video source”
- All files are using `.m2ts` format

1. Group by basic video attributes (frame width, real frame rate, average frame rate)
2. Find the representing group, which has the highest duration or framecount
  - **Weighting**: Using squared value to boost differences, which combats the case w/ a large # of small streams, and the growing number of these streams due to rising production $$$s
  - See `CalculateQueueVoteWeight()`
3. Exclude irrelevant groups
4. Go back to FirstStream strategy to perform cleanup checks

## 4. Starting Queue Encoding
`StartEncCmd` reads the queue JSON path from `MainVM.GetCurrentQueueJsonPath()`. In queue mode it:
- rejects unsupported upstreams
- loads the accepted file list from the queue JSON
- optionally applies the duration filter
- builds one `EncodingPipelineRequest` and one `EncodingPipelineCommand` per source file
- checks for missing inputs and duplicate output paths
- shows the overwrite confirmation flow before starting

Queue encoding is supported only for `ffmpeg.exe`, `vspipe.exe`, `avs2yuv.exe`, and `avs2pipemod.exe`. `one_line_shot_args.exe` is rejected in queue mode.

For `ffmpeg`, the source video itself is the upstream input. For `vspipe` and `avs2yuv`/`avs2pipemod`, `MainVM.BuildQueueEncodingPipelineRequests()` pairs each source with a script file in the selected queue script folder and verifies that the script exists and points back to the same source.

Each job gets its output name from the source basename.

## 5. Batch Execution
After confirmation, `StartEncCmd` opens `EncodingMonitorModal` with the full batch. The modal calls `EncodingMonitorVM.Start()` on load.

In queue mode, `EncodingMonitorVM.RunQueueEncodingAsync()`:
- adds all jobs to the queue sidebar
- saves the sidebar state to disk
- runs jobs sequentially
- marks each job Pending, Encoding, Completed, Failed, or Interrupted
- stops on the first failure
- lets the user stop the remaining queue after an interruption

## 6. End-to-End Flow
`Video Src. Queue` selected -> import folder -> analyze queue -> accept filtered files -> write queue JSON -> press `Start Encode` -> load queue JSON -> build per-file requests -> confirm overwrite -> open monitor -> run the batch one job at a time.

## Key Files
- `QueueManagement/VideoSourceQueue.cs`
- `Commands/BrowseSourceQueueCmd.cs`
- `Commands/BrowseSourceScriptQueueCmd.cs`
- `Commands/AnalyzeSrcVideoCmd.cs`
- `Commands/StartEncCmd.cs`
- `ViewModels/EncodingMonitorVM.cs`
