# Import-Export Tasks Feature — Full Implementation Plan

> **Session date:** 2026-09-08
> **Purpose:** Re-enable export/import of encoding configurations across 1cenc instances, plus queue task splitting into multiple `.1cenc.json` files.

---

## 1. New files to create (6 files)

### 1.1 `Models/Encoding/EncodingConfigExportM.cs`
Versioned DTO for the `.1cenc.json` format.

```
Format = "1cenc.encoding-configuration"
FormatVersion = 1
```

Contains:
- `EncodingConfigToolSelectionM` — upstream, encoder, ffprobe, ffmpeg, avisynth paths + vspipe arg
- `EncoderConfM` — encoder settings (saved via `EncoderConfM.Save()`)
- `ParallelismConfM` — parallelism settings (saved via `ParallelismConfM.LoadEffective()`)
- `EncodingConfigFilterM` — ffmpeg filter args, repart filter text, embedded scripts (`List<EncodingConfigScriptM>`)
- `EncodingConfigMuxM` — mux mode, auto-mux enabled, audio mode, `Dictionary<string, List<MuxTrackM>>` per-source tracks
- `EncodingConfigOutputM` — output directory + filename
- `EncodingConfigSourceManifestM` — ordered source paths, queue accepted/excluded paths, queue filter mode, duration filter state
- `EncodingConfigAnalysisM` — representative ffprobe `JsonElement` (not string!), aggregate frame count, per-source analysis list
- `RepartPlanM?` — repart plan (only for Repart route)

`EncodingTaskSplitMode` enum: `ByCount`, `ByFileSize`, `ByPixelWork`

### 1.2 `Models/Encoding/EncodingTaskSplitter.cs`
Static splitter: `Split(source, taskCount, mode)` returns `IReadOnlyList<EncodingConfigExportM>`.

- `ByCount`: integer division (baseSize + remainder distributed)
- `ByFileSize` / `ByPixelWork`: greedy bin-packing using `GetWeight()` against each source's ffprobe `JsonElement`
- `ByPixelWork` weight = `W * H * avgFrameRate * duration`
- Each split task copies source manifest, mux tracks, analysis subset, recalculates `AggregateFrameCount` from subset

### 1.3 `ViewModels/ImportExportTasksVM.cs`
Modal ViewModel.

- Constructor receives: `buildExport`, `applyImport`, `canExport`, `getQueueCount` delegates
- Properties: `TaskCount` (clamped 1..queueCount), `IsQueueSplittingAvailable` (queueCount > 1), `CanExport`
- Three-button group: Cancel / Import / Export
- Import: `OpenFileDialog` → deserialize → validate format/version → `_applyImport(export)` → success modal
- Export: if queue + taskCount > 1 → `ExportQueueTasks()` using `EncodingTaskSplitter.Split()`, else single `SaveFileDialog`
- Split mode combo: "Divide by count" / "File-size weighted" / "W*H*fps*duration weighted"

### 1.4 `Views/ImportExportTasksModal.xaml` + `.xaml.cs`
XAML window:
- `AppConfContainer` with `IntegerSlider` (task count) and `ComboBox` (split mode)
- `HintPanel` shown when `IsQueueUnavailable`
- `ThreeButtonGroup` bound to `FinishButtons`
- `AdaptiveWindow`, `SizeToContent="Height"`, `WindowStartupLocation="CenterOwner"`

### 1.5 `Commands/OpenClose/OpenImportExportTasksCmd.cs`
Command: `OpenCloseBase` subclass. Creates window + VM, calls `ShowModal(window, viewModel, closeOpenStack: true)`.

---

## 2. Modified files (6 files)

### 2.1 `Models/Analysis/VideoAnalysisM.cs`
Add `QueueFilterMode` string property (persisted so queue filter mode survives export/import round-trip). Reset it in `ResetAnalysis()`.

### 2.2 `Commands/AnalyzeSrcVideoCmd.cs`
After `PromptQueueFilterMode()`, persist the result: `_analysis.QueueFilterMode = filterMode.ToString();`

### 2.3 `Models/Lang/LangProviderBase.cs`
Add `["Export"] = "Export"` (and translations for zh-CN, zh-TW, fr, es, ja, ru, de, ko, pt-BR) to every language block under `CommonData`.

### 2.4 `Models/Lang/UICaptionProvider.cs`
Add `public static string Export => UILangProvider.Current["Export"];` in `Buttons` class.

### 2.5 `Views/Themes/Generic.xaml`
Register DataTemplate:
```xml
<DataTemplate DataType="{x:Type vms:ImportExportTasksVM}">
    <views:ImportExportTasksModal />
</DataTemplate>
```

### 2.6 `ViewModels/MainVM.cs` (largest change)

#### 2.6.1 Wire-up (constructor ~line 629)
```csharp
ImportExportTasks = new OpenImportExportTasksCmd(
    modalNavS,
    BuildEncodingConfigExport,
    ApplyEncodingConfigExport,
    CanExportEncodingConfig,
    () => GetActiveSrcRoute() == SrcRouteKind.Queue ? GetCurrentQueueFilePaths().Length : 0);
```

Replace button group creation (~line 649):
- Old: `ReEvaluate` + `ActionCmd(_ => ReEvaluateAllChecks())`
- New: `"Import-Export Tasks"` + `ImportExportTasks`
- Icon: `GameSave` (was `GameRefresh`)

#### 2.6.2 Remove `ReEvaluateAllChecks()` method
Delete the old private method entirely.

#### 2.6.3 `UpdateEncStartButtonsState()` (~line 1197)
```csharp
EncStartButtons.B3_1IsEnabled = allReady;   // was always true
EncStartButtons.B3_3IsEnabled = allReady;
```

#### 2.6.4 `CanExportEncodingConfig()` — new method
Returns true when: both source slots selected, analysis complete, upstream/encoder/output selected.

#### 2.6.5 `BuildEncodingConfigExport()` — new ~100-line method
Builds the full export DTO by reading from:
- `UpstreamsZone`, `EncodersZone`, `AnalyticsZone` for tool paths
- `_scriptScribeFFmpegFilterArgs`, `_repartAvsFilterInput`, `_repartVpyFilterInput` for filters
- `_SrcQueue` / `_SrcConcat` for source paths
- `_srcVideoAnalysis` for ffprobe data
- `_muxTracksBySource` for mux tracks
- `_appConfM.AutoMux` + `EncodingAutoMuxResolver` for auto-mux state
- `_appConfM.AudioMux` for audio mode
- `_outputSettingCard` for output path

Helper methods:
- `BuildExportTool(ToolItemCardVM?)` / `BuildExportTool(exeName, path, version)`
- `GetExportSourcePaths(route)` → per-route path array
- `BuildExportScripts(route)` → reads `.avs`/`.vpy` files from disk, returns `List<EncodingConfigScriptM>`
- `BuildExportMuxTracks(sourcePaths)` → clones `_muxTracksBySource` entries
- `BuildExportAnalysis(route)` → parses `BatchRawJson` → `List<EncodingConfigSourceAnalysisM>` with `JsonElement` per source
- `BuildExportSourceAnalysis(path, displayName, ffprobeJson)` → creates entry with file size + last-write ticks
- `ReadQueueSourcePaths(path)` → reads queue `SrcData` JSON file

#### 2.6.6 `ApplyEncodingConfigExport(export)` — new ~80-line import method
Applies in this order:
1. Format/version validation
2. `ApplyImportedToolSelection()` for upstream, encoder, ffprobe zones
3. `export.Encoder.Save()`, `export.Parallelism.Save()`, save `VspipeY4mArg`, `_appDataM.Save()`
4. Set filter args (`_scriptScribeFFmpegFilterArgs`, `_repartAvsFilterInput`, `_repartVpyFilterInput`)
5. Set duration filter state (`_isDurationFilterEnabled`, `_minVideoDurationSeconds`)
6. `ApplyImportedSourceState(export)` — sets queue/concat/single card selection, writes queue JSON, applies repart plan
7. `ApplyImportedScripts(export)` — materializes embedded scripts to `imported-task-scripts/{guid}/` on disk, selects correct script source card
8. `ApplyImportedOutput(export)` — sets output directory/filename
9. `ApplyImportedMuxState(export)` — restores `_muxTracksBySource`, audio mode per route, auto-mux enabled state
10. `ApplyImportedSettingsCards()` — refreshes encoder and parallelism card display
11. `RefreshActiveSrcRoute()`, `RefreshSelectedSrcStatus(resetAnalysis: false)`, `UpdateEncStartButtonsState()`

`SetImportedAutoMux(route, encoderExeName, enabled)` — maps encoder exe name to the correct `AutoMux` property per route.

#### 2.6.7 `OnModalStateChanged()` (~line 3750)
Add `ImportExportTasksVM` to the `shouldHideMainWindow` list.

#### 2.6.8 `RefreshButtonsText()` (~line 3820)
```csharp
EncStartButtons.B3_1Text = "Import-Export Tasks";   // was UICaptionProvider.Buttons.ReEvaluate
```

---

## 3. Key design decisions

| Decision | Rationale |
|---|---|
| `JsonElement` (not `string`) for ffprobe data | Avoids escaped `\r\n` and `\u0022` in export JSON; preserves readable ffprobe output |
| `QueueFilterMode` persisted as string in `VideoAnalysisM` | Older analysis files remain readable if enum values change |
| Scripts materialized to disk on import | Queue encoding reads scripts from file paths, not in-memory strings |
| Import writes `source_queue_import_{guid}.json` | Queue source paths reference queue JSON files on disk; must exist for `QueueSrcData` deserialization |
| `B3_1IsEnabled = allReady` | Import-Export button disabled when tools/validation not ready, same as Start Encode |
| Button label hardcoded `"Import-Export Tasks"` | `UICaptionProvider.Buttons.Export` exists for localization but modal title and button label use English for now |
| `ReEvaluateAllChecks()` removed | Button repurposed; `RefreshNumaCpuCheck()` still triggers full check run |
| `EncodingConfigExportCmd.cs` deleted | Standalone export command replaced by modal's Export flow |

---

## 4. Localization keys added

| Key | EN | ZH-CN | ZH-TW | FR | ES | JA | RU | DE | KO | PT-BR |
|---|---|---|---|---|---|---|---|---|---|---|
| `Export` | Export | 导出 | 導出 | Exporter | Exportar | エクスポート | Экспортировать | Exportieren | 내보내기 | Exportar |

---

## 5. Serialized `.1cenc.json` format

```jsonc
{
  "Format": "1cenc.encoding-configuration",
  "FormatVersion": 1,
  "ApplicationVersion": "1.x.x.x",
  "ExportedAtUtc": "2026-09-08T...",
  "Route": "Queue",  // Single | Queue | Concat | Repart
  "Tools": { /* upstream, encoder, ffprobe, ffmpeg, avisynth paths */ },
  "Encoder": { /* EncoderConfM serialized */ },
  "Parallelism": { /* ParallelismConfM serialized */ },
  "Filters": { /* FFmpegFilterArgs, RepartAvsFilterText, RepartVpyFilterText, Scripts[] */ },
  "Mux": { "Mode": "Auto", "AutoMuxEnabled": true, "AudioMode": "First", "TracksBySource": { /* path → [MuxTrackM] */ } },
  "Output": { "Directory": "...", "FileName": "..." },
  "Sources": { "OrderedPaths": [], "QueueAcceptedPaths": [], "QueueExcludedPaths": [], "QueueFilterMode": "FirstStream", "QueueDurationFilterEnabled": false, "QueueMinimumDurationSeconds": 0 },
  "Analysis": { "Route": "Queue", "RepresentativeSourcePath": "...", "FfprobeOriginalPath": "...", "RepresentativeRawJson": { /* ffprobe JSON object, NOT escaped string */ }, "AggregateFrameCount": 12345, "Sources": [ { "FilePath": "...", "DisplayName": "...", "FfprobeJson": { /* raw ffprobe JSON object */ }, "FileLength": 123456789, "LastWriteUtcTicks": 123456789 } ] },
  "RepartPlan": null  // or RepartPlanM if Repart route
}
```

---

## 6. Build status

```
dotnet build OneColumnEncoder.csproj --no-restore
→ 0 warnings, 0 errors
```

---

## 7. Files changed summary

| File | Status | Lines changed |
|---|---|---|
| `Models/Encoding/EncodingConfigExportM.cs` | NEW | +100 |
| `Models/Encoding/EncodingTaskSplitter.cs` | NEW | +231 |
| `ViewModels/ImportExportTasksVM.cs` | NEW | +185 |
| `Views/ImportExportTasksModal.xaml` | NEW | +56 |
| `Views/ImportExportTasksModal.xaml.cs` | NEW | +9 |
| `Commands/OpenClose/OpenImportExportTasksCmd.cs` | NEW | +26 |
| `Commands/SaveLoad/ExportEncodingConfigCmd.cs` | DELETED | -N |
| `ViewModels/MainVM.cs` | MODIFIED | +574 / -35 |
| `Models/Analysis/VideoAnalysisM.cs` | MODIFIED | +7 |
| `Commands/AnalyzeSrcVideoCmd.cs` | MODIFIED | +1 |
| `Models/Lang/LangProviderBase.cs` | MODIFIED | +10 |
| `Models/Lang/UICaptionProvider.cs` | MODIFIED | +1 |
| `Views/Themes/Generic.xaml` | MODIFIED | +3 |

**Total: ~1,200 lines added/modified across 13 files**
