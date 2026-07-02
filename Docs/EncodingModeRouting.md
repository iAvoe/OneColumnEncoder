# Encoding Mode Routing: Single, Queue, and Concat

This document covers the three source routes used by OneColumnEncoder:

- **Single Source**: one imported video or script source becomes one output.
- **Queue Source**: many independent sources become many outputs.
- **Concat Source**: many video fragments become one logical source and one output.

## 1. Route Activation

`FileManagement/SourceRouteKind.cs` defines the route enum:

```csharp
public enum SourceRouteKind
{
    Single,
    Queue,
    Concat
}
```

`MainVM.GetActiveSourceRoute()` is the central route resolver:

```csharp
private SourceRouteKind GetActiveSourceRoute()
{
    if (_videoSourceQueue.IsActive) return SourceRouteKind.Queue;
    if (_videoSourceConcat.IsActive) return SourceRouteKind.Concat;
    return SourceRouteKind.Single;
}
```

`VideoSourceQueueState` lives in `QueueManagement/VideoSourceQueue.cs`; `VideoSourceConcatState` lives in `ConcatManagement/VideoSourceConcat.cs`.

## 2. Import Zone Layout

`ToolCatalogProviderM.GetVideoSrcImportDefs()` returns three video source cards:

| Index | Route | Card | R1 | R2 | State Owner |
|-------|-------|------|----|----|-------------|
| 0 | Single | `Tool.Source.VideoSource` | Replace | Clear | `AppDataM.Tools.VideoSourcePath` |
| 1 | Queue | `Tool.Source.VideoSrcQueue` | Import | Clear | `VideoSourceQueueState` |
| 2 | Concat | `Tool.Source.VideoSrcConcat` | Import | Clear | `VideoSourceConcatState` |

`ToolCompatibility.RefreshVideoSourceSelectionState()` expects all three cards. When `one_line_shot_args.exe` is selected, both Queue and Concat cards are deselected and disabled.

## 3. Active UI Routing

`MainVM.RefreshActiveSourceRoute()` switches validation cards and script zones by route.

| Aspect | Single | Queue | Concat |
|--------|--------|-------|--------|
| Validation card | `SrcValidationCard` | `QueueSrcFilterCard` | `ConcatCheckCard` |
| Script zone | `ScriptSrcImportZone` | `QueueScriptSrcImportZone` | `ScriptSrcImportZone` |
| Duration filter | Hidden | Visible | Hidden |
| Sample Clip | Enabled when checks pass | Disabled | Disabled |
| Output setting | Filename scribe | Directory-only queue output | Filename scribe for one concat output |

`RefreshScriptSourceEnabledState()` only mutates the single script zone while the active route is `Single`; Queue and Concat leave script zone state under their route-specific flows.

## 4. Source Import Behavior

### Single

`BrowseSourcePathCmd` imports one source path. `OnVideoSourceImported()` clears competing source selections and stores the selected path in app data.

### Queue

`BrowseSourceQueueCmd` imports a folder. Queue mode accepts all discovered video files first, then `AnalyzeSrcVideoCmd` may filter the batch and write queue JSON files.

### Concat

`BrowseSourceConcatCmd` opens a multi-select `OpenFileDialog`. Before state is updated or ffprobe analysis is prompted, it validates that every selected file has the same extension. A mismatch opens `OpenErrModalCmd` and aborts import.

After a successful import:

- `VideoSourceConcatState.ApplyImportedFiles()` stores the full ordered file list.
- The concat card title shows `Tool.Source.VideoSrcConcatWithCount`.
- `source_concat_filelist.txt` is generated under the app config directory.
- The app prompts for source analysis.

## 5. Source Analysis

| Route | Analysis Behavior |
|-------|-------------------|
| Single | Analyze one source with ffprobe, supplement frame count, update `SrcValidationCard`. |
| Queue | Analyze all queue files, optionally filter by first-stream or weighted vote, write accepted/excluded JSON. Individual failures can be skipped. |
| Concat | Analyze all concat files. Any ffprobe failure or key video-parameter mismatch rejects the whole batch. No partial acceptance. |

Concat compares each file to the first file using checklist signature plus width, height, pixel format, codec, and frame-rate strings. The first file's raw ffprobe JSON becomes `_srcVideoAnalysis.RawJson`, so encoder config generation and FilterScribe use one representative source analysis.

## 6. Script Generation and FilterScribe

| Route | One-Click Script Gen | FilterScribe Save & Import |
|-------|----------------------|-----------------------------|
| Single | Writes one AVS and one VPY script for the selected source. | Saves/imports one AVS and one VPY script. |
| Queue | Writes one AVS and one VPY per queue file into a folder. | Saves/imports per-file scripts into a folder. |
| Concat | Writes one AVS and one VPY script containing all fragments. | Saves/imports one concat AVS and one concat VPY script. |

Concat FilterScribe displays `ConcatSourceSidebarPanel` on the left. Reorder/remove actions update `VideoSourceConcatState` and regenerate `source_concat_filelist.txt`.

AVS concat scripts are video-only and preserve `src`:

```avs
v1 = LWLibavVideoSource("part1.mkv")
v2 = LWLibavVideoSource("part2.mkv")
src = v1 ++ v2
# user edit section follows
```

VPY concat scripts use `core.std.Splice` and preserve `src`.

## 7. Encoding Request Routing

`StartEncCmd` branches before queue handling:

1. Concat route: build one request from `BuildConcatEncodingPipelineRequest()`.
2. Queue route: load queue JSON, build one request per accepted source.
3. Single route: build one request from `BuildEncodingPipelineRequest()`.

Concat requests set:

```csharp
IsConcatMode: true,
ConcatFileListPath: _videoSourceConcat.RegenerateFileList()
```

For `ffmpeg.exe`, `EncodingPipeline.BuildUpstreamArgs()` uses:

```text
-hide_banner -f concat -safe 0 -i filelist.txt -f yuv4mpegpipe -an -strict unofficial -
```

For `vspipe.exe`, `avs2yuv.exe`, and `avs2pipemod.exe`, the upstream input is the selected concat script path.

## 8. Audio Muxing

Concat does not put audio into generated AVS/VPY scripts. Audio is muxed after encoding from `ConcatFileListPath`:

```text
ffmpeg -i encoded_video -f concat -safe 0 -i source_concat_filelist.txt -map 0:v:0 -map 1:a? -c:v copy -c:a copy ... output
```

Single mode uses `SourceVideoPath` for muxing. Queue mode uses each request's individual `SourceVideoPath`. Concat mode never uses `SourceVideoPath` for muxing.

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
