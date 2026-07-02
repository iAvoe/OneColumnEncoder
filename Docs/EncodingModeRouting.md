# Encoding Mode Routing: Single Source vs Queue Mode

This document covers the two encoding modes — **Single Video Source** (`Video Src.`) and **Video Source Queue** (`Video Src. Queue`) — including their state management, zone/validation-card routing, command branching, refresh logic, and UI element enable/disable associations.

---

## 1. Mode Activation

### 1.1 Core State

**File:** `QueueManagement/VideoSourceQueue.cs`

| Member | Type | Description |
|--------|------|-------------|
| `VideoSourceQueueState.IsActive` | `bool` (get) | `true` when `VideoSrcImportZone[1]` (the queue card) is selected |
| `VideoSourceQueue.CurrentFilePaths` | `string[]` | Stored file paths for the queue card |
| `VideoSourceQueue.IsQueueItem(item)` | `bool` | Reference-equality check against the queue card |

Initialized in `MainVM` constructor (`MainVM.cs:528-529`):

```csharp
VideoSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetVideoSrcImportDefs(), true, false);
_videoSourceQueue = new(VideoSrcImportZone);
```

### 1.2 Mode Check Entry Points

All mode checks flow through `MainVM.IsQueueRouteActive()` (`MainVM.cs:1787-1788`):

```csharp
private bool IsQueueRouteActive() => _videoSourceQueue.IsActive;
```

Referenced at: lines 347, 570, 588, 591, 598, 617, 622, 625, 963, 1120, 1214, 1244, 1729, 1758, 1787, 1818.

---

## 2. Zone Routing

### 2.1 Zone Definitions

**File:** `Models/ToolCatalogProviderM.cs`

| Zone | Index 0 (Single) | Index 1 (Queue) |
|------|-------------------|------------------|
| `VideoSrcImportZone` | `Video Source` (R1: Replace, R2: Clear) | `Video Src. Queue` (R1: Import, R2: Clear) |
| `ScriptSrcImportZone` | AviSynth, VapourSynth, SVFI (single-file) | — |
| `QueueScriptSrcImportZone` | — | AviSynthQueue, VapourSynthQueue (no SVFI) |

Note: `QueueScriptSrcImportZone` has only **2 items** (no SVFI Queue).

### 2.2 Active Zone Switching

**Method:** `MainVM.RefreshActiveSourceRoute()` (`MainVM.cs:1756-1785`)

```csharp
private void RefreshActiveSourceRoute()
{
    bool queueActive = IsQueueRouteActive();
    ActiveSrcValidationCard = queueActive ? QueueSrcFilterCard : SrcValidationCard;
    ActiveScriptSrcImportZone = queueActive ? QueueScriptSrcImportZone : ScriptSrcImportZone;
    ToolCompatibility.RefreshSourceSelectionState(UpstreamsZone, ActiveScriptSrcImportZone, () => { });
    RefreshScriptSourceEnabledState();
    ToolCompatibility.RefreshVideoSourceSelectionState(UpstreamsZone, VideoSrcImportZone);
    RefreshOutputSettingCommand();
    OnPropertyChanged(nameof(IsDurationFilterVisible));

    if (_outputSettingCard != null)
    {
        if (queueActive)
            _outputSettingCard.RefreshOutputSetting(true, _modalNavS);
        else
            SyncOutputFilenameWithVideoSource();
    }
}
```

**Effect:**

| Aspect | Single Source Mode | Queue Mode |
|--------|--------------------|------------|
| `ActiveSrcValidationCard` | `SrcValidationCard` | `QueueSrcFilterCard` |
| `ActiveScriptSrcImportZone` | `ScriptSrcImportZone` (3 items) | `QueueScriptSrcImportZone` (2 items) |
| `IsDurationFilterVisible` | `false` | `true` |
| Output setting refresh | `SyncOutputFilenameWithVideoSource()` | `RefreshOutputSetting(true, ...)` |

Called from `RefreshSelectedSourceStatus()` (`MainVM.cs:1664`) on every source selection change.

---

## 3. Validation Cards

| Card | Class | File | Used In |
|------|-------|------|---------|
| `SrcValidationCard` | `SourceCheckCardVM` | `ViewModels/Cards/SourceCheckCardVM.cs` | Single Source |
| `QueueSrcFilterCard` | `QueueSrcFilterCardVM` (extends `SourceCheckCardVM`) | `ViewModels/Cards/QueueSrcFilterCardVM.cs` | Queue Mode |

`QueueSrcFilterCardVM` adds:

| Property | Type | Description |
|----------|------|-------------|
| `IncludedCount` | `int` | Files accepted after analysis |
| `ExcludedCount` | `int` | Files filtered out |
| `QueueJsonPath` | `string` | Path to the accepted-queue JSON |
| `ExcludedJsonPath` | `string` | Path to the excluded-queue JSON |

Both cards' checklists are subscribed in `MainVM.SubToToolsChecklist()` (`MainVM.cs:997-1016`) and unsubscribed in `UnsubFromToolsChecklist()` (`MainVM.cs:1017-1035`).

---

## 4. Command Branching

### 4.1 Commands That Branch on Mode

| Command | File | Single Source Path | Queue Mode Path | Queue-Specific Logic |
|---------|------|--------------------|-----------------|----------------------|
| `AnalyzeSrcVideoCmd` | `Commands/AnalyzeSrcVideoCmd.cs` | Single ffprobe analysis | `ExecuteQueueAnalysisAsync()` | Batch analysis with signature grouping, reference candidate selection, JSON output |
| `OneClickScriptGenCmd` | `Commands/SaveLoad/OneClickScriptGenCmd.cs` | Single script generation | `ExecuteQueueScriptGen()` | Iterates queue files, generates one script per source |
| `OpenSampleClipCmd` | `Commands/OpenClose/OpenSampleClipCmd.cs` | Opens `SampleClipModal` | Shows warning: *"Queue mode does not support sample clipping"* | Rejects immediately |
| `StartEncCmd` | `Commands/StartEncCmd.cs` | Standard encode | `ExecuteQueueRoute()` | Loads queue JSON, applies duration filter, builds per-file requests, checks missing/overwrite |
| `CopyRawAnalysisCmd` | `MainVM.cs:590-591` | Normal copy | Injected `IsQueueRouteActive` | Behavior differs via delegate |

### 4.2 CanExecute Mode Checks

| Command | Single Source Check | Queue Mode Check |
|---------|--------------------|-------------------|
| `AnalyzeSrcVideoCmd.CanExecute` | `!string.IsNullOrWhiteSpace(getSourcePath())` | `(getQueueFilePaths?.Length ?? 0) > 0` |
| `OneClickScriptGenCmd.CanExecute` | `!string.IsNullOrWhiteSpace(getSourcePath())` | `(getQueueFilePaths?.Length ?? 0) > 0` |

### 4.3 Command Wiring (MainVM Constructor)

Wiring at `MainVM.cs:564-626` passes `IsQueueRouteActive` delegate to:

- `OneClickScriptGenCmd` (line 570)
- `OpenFilterScribeCmd` (line 588)
- `CopyRawAnalysisCmd` (line 591)
- `AnalyzeSrcVideoCmd` (line 598)
- `OpenSampleClipCmd` (line 617)
- `StartEncCmd` (lines 622-625)

---

## 5. Button & UI Element Enable/Disable

### 5.1 EncStartButtons (ThreeButtonGroup)

**File:** `Views/MainUI.xaml:1061-1069` → bound to `EncStartButtons`

**Updated in:** `MainVM.UpdateEncStartButtonsState()` (`MainVM.cs:1090-1123`)

| Button | Property | Disabled When | Line |
|--------|----------|---------------|------|
| B3_1 (Re-Evaluate) | `B3_1IsEnabled` | Always `true` | — |
| B3_2 (Sample Clip) | `B3_2IsEnabled` | `allReady && !oneLineShotSelected && !IsQueueRouteActive()` | 1120 |
| B3_3 (Start Encode) | `B3_3IsEnabled` | `allReady` | 1121 |

**Queue mode disables the Sample Clip button** (`EncStartButtons.B3_2IsEnabled`).

### 5.2 SVFI Clip Disabled Hint

**File:** `Views/MainUI.xaml:1071-1077`

```xml
<comps:HintPanel Visibility="{Binding SVFIClipDisabledHintVisible, Converter={StaticResource BoolToVisibility}}"
                 Text="{Binding SVFIClipDisabledHintText}" .../>
```

Bound to `MainVM.SVFIClipDisabledHintVisible` — this is `true` when `one_line_shot_args.exe` is selected (not queue-specific, but related to the same button).

### 5.3 Duration Filter (Queue-Only)

**File:** `Views/MainUI.xaml:910-942`

```xml
<Grid Visibility="{Binding IsDurationFilterVisible, Converter={StaticResource BoolToVisibility}}" ...>
```

Bound to `MainVM.IsDurationFilterVisible` (`MainVM.cs:347`):

```csharp
public bool IsDurationFilterVisible => IsQueueRouteActive();
```

This entire Grid is **invisible in Single Source mode**.

Inside the Grid:

| Element | Binding | Behavior |
|---------|---------|----------|
| CheckBox | `IsChecked="{Binding IsDurationFilterEnabled}"` | Toggle duration filter on/off |
| IntegerSlider | `Value="{Binding MinVideoDurationSeconds}"` | Slider 10s–310s, step 10 |
| HintPanel | `Text="{Binding DurationFilterStatusText}"` | Shows "X of Y removed" or "All filtered" |

Persistence: `AppDataM.IsDurationFilterEnabled` and `AppDataM.MinVideoDurationSeconds` (lines 40, 42).

### 5.4 AnalyzeSrcButtons

**Updated in:** `MainVM.UpdateAnalyzeSrcButtonsState()` (`MainVM.cs:1686-1698`)

Both B2_1 (Copy Raw) and B2_2 (Analyze Source) are enabled/disabled based on `CanRunSourceAnalysis()` which calls `HasSelectedVideoSource()` — a mode-aware check (`MainVM.cs:1817-1820`):

```csharp
private bool HasSelectedVideoSource() =>
    IsQueueRouteActive()
        ? GetCurrentQueueFilePaths().Length > 0
        : GetSelectedSingleVideoSource() != null;
```

### 5.5 FilterScbButtons (One-Click Script Gen & Open Filter Scribe)

**Updated in:** `MainVM.UpdateFilterScbButtonsState()` (`MainVM.cs:1048-1088`)

Uses `HasSelectedVideoSource()` (mode-aware) at line 1054. Both buttons disabled when no video source available.

### 5.6 InspBypsChkButtons (Inspect & Bypass)

**Updated in:** `MainVM.UpdateInspBypsChkButtonsState()` (`MainVM.cs:1699-1711`)

Uses `ActiveSrcValidationCard` — which switches between `SrcValidationCard` and `QueueSrcFilterCard` based on mode.

---

## 6. Script Source State

### 6.1 RefreshScriptSourceEnabledState()

**File:** `MainVM.cs:961-973`

```csharp
private void RefreshScriptSourceEnabledState()
{
    if (IsQueueRouteActive()) return;  // Skip in queue mode

    bool hasVideoSource = !string.IsNullOrWhiteSpace(GetCurrentVideoSourcePath());
    if (hasVideoSource) return;

    foreach (ToolItemCardVM item in ScriptSrcImportZone)
    {
        item.IsSelected = false;
        item.IsEnabled = false;
    }
}
```

In **queue mode**, single-source script zone state is **not touched** (the queue has its own `QueueScriptSrcImportZone`).

### 6.2 Queue Script Import Validation

**File:** `MainVM.cs:1585-1628` — `ValidateScriptQueueImport()`

When importing scripts in queue mode, validates:
1. Each script's basename matches a video in the queue
2. The embedded source path inside the script matches the expected video path

---

## 7. Output Setting Mode Sensitivity

### 7.1 RefreshOutputSettingCommand()

**File:** `MainVM.cs:1209-1215`

```csharp
private void RefreshOutputSettingCommand(ToolItemCardVM? outputSetting = null)
{
    outputSetting ??= _outputSettingCard;
    if (outputSetting == null) return;
    outputSetting.RefreshOutputSetting(IsQueueRouteActive(), _modalNavS, GetSelectedVideoSourcePath());
}
```

Passes `IsQueueRouteActive()` to `RefreshOutputSetting()`, which changes the output filename strategy:
- **Queue mode:** Uses source file basename for each job
- **Single mode:** Derives output filename from single source path

### 7.2 GetPreviewSourceVideoPath()

**File:** `MainVM.cs:1243-1246`

```csharp
private string GetPreviewSourceVideoPath() =>
    IsQueueRouteActive()
        ? GetCurrentQueueFilePaths().FirstOrDefault() ?? string.Empty
        : GetSelectedVideoSourcePath();
```

Used by encoder configuration preview — in queue mode, uses the first queue file.

---

## 8. Source Path Resolution

| Method | Single Mode | Queue Mode | Line |
|--------|-------------|------------|------|
| `GetCurrentVideoSourcePath()` | First non-queue card's `P2TextData` | Same (ignores queue card) | 1721-1724 |
| `GetSelectedVideoSourcePath()` | Selected non-queue card's `P2TextData` | Same | 1805-1809 |
| `GetCurrentSourceImportPath()` | Same as `GetCurrentVideoSourcePath()` | Queue card's `P2TextData` (folder path) | 1727-1736 |
| `GetUpstreamInputPath()` | Single source path or script | Uses `ActiveScriptSrcImportZone` | 2170-2183 |
| `GetCurrentQueueFilePaths()` | — | Returns `_videoSourceQueue.CurrentFilePaths` | 1790-1791 |
| `GetCurrentQueueJsonPath()` | — | Returns `QueueSrcFilterCard.QueueJsonPath` | 1793-1794 |

---

## 9. Card Selection in Queue Mode

### 9.1 OnSourceQueueImported()

**File:** `MainVM.cs:1532-1554`

When a queue folder is imported:
1. Clears all card selections in `VideoSrcImportZone`
2. Clears both `ScriptSrcImportZone` and `QueueScriptSrcImportZone`
3. Selects the queue card
4. Prompts for source analysis

### 9.2 OnSourceImported() for Single Source

**File:** `MainVM.cs:1380-1423`

When a single source is imported:
1. Clears selections in `VideoSrcImportZone` (deselects queue card if active)
2. Clears the matching script zone (single or queue depending on mode)
3. Selects the imported card

---

## 10. Upstream Compatibility

### 10.1 Queue Mode Supported Upstreams

**File:** `SourceFileKind.cs:43-47`

```csharp
public static bool IsQueueRouteSupportedUpstream(string? upstreamExeName) =>
    upstreamExeName?.Equals("ffmpeg.exe", ...) == true ||
    upstreamExeName?.Equals("vspipe.exe", ...) == true ||
    upstreamExeName?.Equals("avs2yuv.exe", ...) == true ||
    upstreamExeName?.Equals("avs2pipemod.exe", ...) == true;
```

**`one_line_shot_args.exe` is rejected** in queue mode.

### 10.2 Video Source Queue Card Disabling

**File:** `ToolCompatibility.RefreshVideoSourceSelectionState()`

When `one_line_shot_args.exe` is selected:
- The queue card (`VideoSrcImportZone[1]`) is **disabled** (`IsEnabled = false`)
- Re-enabled for all other upstreams or none

### 10.3 Script Zone Compat in Queue Mode

`RefreshSourceSelectionState()` works on `ActiveScriptSrcImportZone`, which is the queue script zone when in queue mode. The same upstream→script-kind rules apply but use queue-specific card labels (`AviSynthQueue`, `VapourSynthQueue`).

---

## 11. Duration Filter (Queue-Only)

**File:** `MainVM.cs:2083-2160`

| Method | Lines | Purpose |
|--------|-------|---------|
| `FilterSourcePathsByDuration()` | 2083-2094 | Called from `StartEncCmd`; excludes files below threshold |
| `LoadQueueFfprobeJsonByPath()` | 2060-2081 | Reads the queue JSON to get cached ffprobe data |
| `GetDurationFilterStats()` | 2118-2141 | Computes remaining/removed/total counts |
| `RefreshDurationFilterStatus()` | 2143-2159 | Updates `DurationFilterStatusText` and `IsDurationFilterStatusVisible` |

Properties:

| Property | Type | Line | Description |
|----------|------|------|-------------|
| `IsDurationFilterEnabled` | `bool` | 319-320 | Toggle on/off |
| `MinVideoDurationSeconds` | `int` | 333-344 | Threshold (10-310s) |
| `IsDurationFilterVisible` | `bool` | 347 | **Always `false` in single mode** |
| `DurationTickLabels` | `string[]` | 349 | ["10s", "70s", "130s", "190s", "250s", "310s"] |

---

## 12. Source Event Flow

### 12.1 Queue Source Lifecycle

```
Import folder → OnSourceQueueImported()
                  ↓
        PromptRunSourceAnalysisAfterReplace()
                  ↓
        AnalyzeSrcVideoCmd (queue branch)
                  ↓
        OnSourceQueueAccepted()
                  ↓
        VideoSourceQueueState.ApplyAcceptedFiles()
                  ↓
        RefreshSelectedSourceStatus() → RefreshActiveSourceRoute()
```

### 12.2 Single Source Lifecycle

```
Browse file → OnVideoSourceImported()
                  ↓
        OnSourceImported()
                  ↓
        RefreshSelectedSourceStatus() → RefreshActiveSourceRoute()
```

---

## 13. XAML UI Bindings Summary

**File:** `Views/MainUI.xaml`

| XAML Element | Lines | Binding | Mode Sensitivity |
|-------------|-------|---------|------------------|
| Duration Filter Grid | 910-936 | `Visibility="{Binding IsDurationFilterVisible}"` | Visible only in **queue mode** |
| Duration Filter Status | 937-942 | `Visibility="{Binding IsDurationFilterStatusVisible}"` | Queue-only, managed by `RefreshDurationFilterStatus()` |
| ThreeButtonGroup (EncStart) | 1061-1069 | `Button2IsEnabled="{Binding B3_2IsEnabled}"` | B3_2 disabled in queue mode |
| SVFI Clip Disabled Hint | 1071-1077 | `Visibility="{Binding SVFIClipDisabledHintVisible}"` | Shown when `one_line_shot_args` is selected (affects both modes but interacts with queue) |

---

## 14. Key Files Index

| File | Role |
|------|------|
| `QueueManagement/VideoSourceQueue.cs` | Core state: `VideoSourceQueueState.IsActive`, file path storage |
| `ViewModels/MainVM.cs` | All refresh logic, button state updates, mode branching |
| `Views/MainUI.xaml` | UI bindings: duration filter visibility, button enables |
| `Models/ToolCatalogProviderM.cs` | Zone definitions: `GetVideoSrcImportDefs()`, `GetScriptSrcImportDefs()`, `GetScriptSrcImportQueueDefs()` |
| `Commands/AnalyzeSrcVideoCmd.cs` | Queue batch analysis with signature grouping & filtering |
| `Commands/StartEncCmd.cs` | Queue route: load JSON, duration filter, per-file requests |
| `Commands/SaveLoad/OneClickScriptGenCmd.cs` | Queue script generation per source file |
| `Commands/OpenClose/OpenSampleClipCmd.cs` | Blocks sample clip in queue mode |
| `ViewModels/Cards/QueueSrcFilterCardVM.cs` | Queue-specific validation card with JSON path tracking |
| `FileManagement/SourceFileKind.cs` | `IsQueueRouteSupportedUpstream()` — allowed executables |
| `Models/AppDataM.cs` | Duration filter persistence (`IsDurationFilterEnabled`, `MinVideoDurationSeconds`) |
