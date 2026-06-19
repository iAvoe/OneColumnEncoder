# ItemCard Management Guide

This document explains how `ToolItemCardVM` / `ItemCard` instances are managed throughout the application. It covers the data model, zone system, queue routing, command patterns, and display text layering.

---

## 1. ToolItemCardVM — The Card ViewModel

**File:** `ViewModels/Cards/ToolItemCardVM.cs`

Each card wraps an `EncItemM` model and exposes bindable properties for the `ItemCard.xaml` view.

### 1.1 Core Display Properties

```
P1TextData (primary display text)
  +---> P1Text (get-only) = P1TextData     → visible on card Row 1
  +---> P1TooltipText (get) = _p1TooltipText ?? P1TextData
           When _p1TooltipText is null  → tooltip shows P1TextData (same as card)
           When _p1TooltipText is set   → tooltip shows the longer text

P2TextData (file path / folder path)
  +---> P2Text (get-only) = P2TextData     → visible on card Row 2
```

- **P1TextData** — Short display text. For video sources: the filename. For scripts: `"Import custom script"`. For queue items: a truncated summary like `"firstFile..lastFile"`. For encoder settings: version strings or configuration summaries.
- **P1TooltipText** — Optional longer text for the tooltip, used by queue items to show a comma-separated list of all file names (up to 512 chars). Falls back to `P1TextData` when `null`.
- **P2TextData** — Backed by `EncItemM.Path`. For source items: the full file path or folder path. For tools: the path to the binary.
- **P1Name / P2Name** — Static field labels (e.g. `"Name"`, `"Path"`, `"Mode"`, `"Sequence"`). Set from `ToolDefinitionM`.

### 1.2 Button Properties

| Property | Purpose |
|----------|---------|
| `R1Text` | Label for the first right-side button |
| `R2Text` | Label for the second right-side button |
| `DisplayR1Text` | When `UseAutoAddReplaceText` is true, shows `"Add"`/`"Replace"` dynamically; otherwise returns `R1Text` |
| `R1Command` | Command bound to the first button |
| `R2Command` | Command bound to the second button |

### 1.3 State Flags

| Flag | Purpose |
|------|---------|
| `IsSelected` | Single-select per zone; green styling when true |
| `IsCancel` | Set by compatibility checks when a dependency conflict forces deselection; red styling |
| `IsReal` | For imported tools: checks if the binary exists and its size matches the stored fingerprint. Red styling when false. |
| `EnableRealCheck` | Disabled for source import zones and enc settings zones (they don't validate binaries) |
| `IsEnabled` | Grayed out when false (e.g. incompatible upstream selection) |

### 1.4 Validation

`Validate()` is triggered automatically each time `P2TextData` changes. For imported tools (`.exe`/`.dll`), it checks:
1. The file exists on disk
2. The file is a known binary type
3. The file size matches the stored fingerprint

If validation fails, `IsReal` becomes `false` and `P1TextData` is cleared (the version string disappears from the card).

---

## 2. Zone System

All zones are `ObservableCollection<ToolItemCardVM>` declared in `MainVM`. Cards are created from `ToolDefinitionM` records via `LoadZoneFromDefinitions()`.

### 2.1 Zone Overview

| Zone | Content | Count | Auto-Add/Replace | Real Check |
|------|---------|-------|-----------------|------------|
| `VideoSrcImportZone` | Video Source, Video Source Queue | 2 | Yes | Yes |
| `ScriptSrcImportZone` | AVS, VPY, SVFI (single-file) | 3 | Yes | Yes |
| `QueueScriptSrcImportZone` | AVS Queue, VPY Queue, SVFI Queue | 3 | No | No |
| `ActiveScriptSrcImportZone` | Points to `ScriptSrcImportZone` or `QueueScriptSrcImportZone` | 3 | — | — |
| `EncSettingsZone` | Output Setting, Parallelism, Enc Params | 3 | No | No |
| `UpstreamsZone` | Imported upstream tools (ffmpeg, vspipe, etc.) | Dynamic | No | Yes |
| `EncodersZone` | Imported encoders (x264, x265, svtav1encapp) | Dynamic | No | Yes |
| `AnalyticsZone` | Imported analytics (ffprobe) | Dynamic | No | Yes |
| `DependenciesZone` | Imported dependencies (avisynth.dll) | Dynamic | No | Yes |

### 2.2 Zone Routing (Queue Mode)

```
VideoSrcImportZone[0] = "Video Source"         → single-file mode
VideoSrcImportZone[1] = "Video Src. Queue"     → queue mode

When "Video Src. Queue" is selected:
  ActiveScriptSrcImportZone = QueueScriptSrcImportZone
  ActiveSrcValidationCard   = QueueSrcFilterCard

Otherwise:
  ActiveScriptSrcImportZone = ScriptSrcImportZone
  ActiveSrcValidationCard   = SrcValidationCard
```

The routing is controlled by `_videoSourceQueue.IsActive` which checks `VideoSrcImportZone[1].IsSelected`. This means all downstream operations (script generation, encoding) automatically use the correct zone.

The `_videoSourceQueue` state (`VideoSourceQueueState`) maintains a `Dictionary<ToolItemCardVM, string[]>` mapping queue items to their file paths. Key methods:

| Method | When Called | Effect |
|--------|-------------|--------|
| `ApplyImportedFiles(item, paths)` | After folder browse | Stores paths, updates card title with count |
| `ApplyAcceptedFiles(paths)` | After queue analysis | Replaces paths with only accepted files, updates P1Text and P1TooltipText |
| `Clear(item)` | On clear | Removes dictionary entry, resets title |
| `RefreshLanguage()` | On language change | Re-applies localized strings |

### 2.3 Wire-Up Process

After zones are initialized in the constructor, commands are wired up:

1. **`WireUpZoneDeleteCmds()`** — Replaces the initial `RemoveZoneItemCmd` on dynamically-populated zones (`UpstreamsZone`, `EncodersZone`, `AnalyticsZone`, `DependenciesZone`) with `DeleteToolCmd`.

2. **`WireUpSourceCmd(item)`** — Sets commands on source import cards:
   - Queue video source → `BrowseSourceQueueCmd` + `ClearToolItemCmd`
   - Queue script source → `BrowseSourceScriptQueueCmd` + `ClearToolItemCmd`
   - Single-file source → `BrowseSourcePathCmd` + `ClearToolItemCmd`

3. **`WireUpEncSettingsCmds()`** — Sets commands on encoder settings cards (e.g. `OpenParallelismConfCmd`, `OpenFilenameScribeCmd`, `OpenEncoderConfCmd`).

4. **`WireUpSourceCmd` is called per-item** during `LoadSourcesFromAppDataM()` and `OnToolsImported`.

---

## 3. Text Formatting for Queue Items

### 3.1 Short Text (P1TextData)

`BrowseSourceQueueCmd.FormatQueueP1Text()` produces the compact card display:

- 0 files → `""`
- 1 file → first 12 characters of the filename (without extension)
- 2+ files → `"first12chars..last12chars"`

### 3.2 Long Text (P1TooltipText)

`BrowseSourceQueueCmd.FormatQueueP1TooltipText()` produces the hover tooltip content:

- Comma-separated list of full file names (with extensions)
- Truncated with `"..."` when exceeding 512 characters
- Only set for queue-mode items; non-queue items fall back to `P1TextData`

### 3.3 Where P1TooltipText Gets Set

| Location | Context |
|----------|---------|
| `BrowseSourceQueueCmd.Execute()` | After browsing a video queue folder |
| `BrowseSourceScriptQueueCmd.Execute()` | After browsing a script queue folder |
| `VideoSourceQueueState.ApplyAcceptedFiles()` | After queue analysis accepts files |
| `VideoSourceQueueState.RefreshLanguage()` | After language change |
| `FilterScribeVM.ExecuteQueueSaveAndImport()` | After saving queue scripts via Filter Scribe |
| `OneClickScriptGenCmd.Execute()` (queue mode) | After one-click script generation |

### 3.4 Where P1TooltipText Gets Cleared

| Location | Context |
|----------|---------|
| `ClearToolItemCmd.Execute()` | Generic clear command for all items |
| `MainVM.ClearScriptSourceZone()` | When switching between queue and non-queue modes |

---

## 4. Command Summary

| Command | Used By | P1TextData | P2TextData | P1TooltipText |
|---------|---------|-----------|-----------|--------------|
| `BrowseSourcePathCmd` | Single-file sources | `GetPrimaryText()` (filename or "Custom Script") | Full file path | Not set (falls back) |
| `BrowseSourceQueueCmd` | Queue video source | `FormatQueueP1Text()` (short summary) | Folder path | `FormatQueueP1TooltipText()` (full list) |
| `BrowseSourceScriptQueueCmd` | Queue script sources | `FormatQueueP1Text()` (short summary) | Folder path | `FormatQueueP1TooltipText()` (full list) |
| `ClearToolItemCmd` | All clear buttons | `""` | `""` | `null` |
| `RemoveZoneItemCmd` | Dynamic zones (initial) | — | — | — |
| `DeleteToolCmd` | Dynamic zones (rewired) | — | — | — |

---

## 5. Visual State & Styling

The `ItemCard.xaml` uses a two-layer border approach:

1. **Shadow layer** — Renders the drop shadow. On hover, blur radius and opacity increase.
2. **Content layer** — Renders text and buttons. On hover, opacity drops to 0.65.

State-driven styling (applied identically to both layers via triggers):

| Condition | Border | Background | Text |
|-----------|--------|------------|------|
| Selected (no cancel) | Green | Green tint | Default |
| Cancelled | Red | Red tint | Default |
| Invalid binary (`IsReal=false`) | Red | Red tint | Default |
| Disabled (`IsEnabled=false`) | Gray | Default | Gray |
| Mouse over | Default | Default | 0.65 opacity |

---

## 6. ToolTip

The tooltip (lines 102-120 of `ItemCard.xaml`) displays two rows:

```
P1Name: P1TooltipText    ← uses P1TooltipText (full file list for queue items)
P2Name: P2Text           ← always shows the full path / folder path
```

---

## 7. Typical Data Flow Examples

### 7.1 Single-File Video Source

```
User clicks "Browse" on "Video Source" card
  → BrowseSourcePathCmd.Execute()
    → P2TextData = "C:\videos\video.mkv"
    → P1TextData = "video.mkv"                     (from GetPrimaryText)
    → P1TooltipText = null                          (falls back to "video.mkv")
    → Tooltip shows: "Name: video.mkv / Path: C:\videos\video.mkv"
```

### 7.2 Queue Video Source

```
User clicks "Import" on "Video Src. Queue" card
  → BrowseSourceQueueCmd.Execute()
    → P2TextData = "C:\videos\"
    → P1TextData = "myvideo..15 - episode"          (short summary)
    → P1TooltipText = "myvideo.mkv, 15 - episode.mkv, 16 - episode.mkv, ..."  (full list)
    → Tooltip shows the full comma-separated list
```

### 7.3 Clear

```
User clicks "Clear"
  → ClearToolItemCmd.Execute()
    → P2TextData = ""
    → P1TextData = ""
    → P1TooltipText = null
    → IsSelected = false
    → Card resets to empty state
```
