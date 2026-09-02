# Plan: Add Crop Filter Generator to FilterScribeModal

## Goal
Add a "Crop" filter section to FilterScribeModal that auto-generates crop filters for VapourSynth, AviSynth, and ffmpeg. The crop target dimensions are computed from ffprobe-analyzed pixel format and progressive/interlaced state, trimming width/height to satisfy colorspace mod constraints. Offsets are always 0.

## Files to Modify

### 1. New file: `FFmpeg/CropCalculator.cs`
Static utility class.

- **`GetCropDimensions(string? pixelFormat, int width, int height, bool isProgressive)`** → `(int croppedWidth, int croppedHeight)?`
  - Maps `pix_fmt` to colorspace category (YV12/YV16/YV24/YUY2/YV411/RGB)
  - Determines mod requirements for width and height per the user's table
  - Rounds down dimensions to satisfy mod constraints
  - Returns null if no crop needed (dimensions already valid)
- **`GetWidthMod(string? pixelFormat)`** and **`GetHeightMod(string? pixelFormat, bool isProgressive)`** helpers
- Mod rules table:
  | Colorspace | Width | Progressive Height | Interlaced Height |
  |-----------|-------|-------------------|-------------------|
  | RGB       | 1     | 1                 | 2                 |
  | YUY2      | 2     | 1                 | 2                 |
  | YV12      | 2     | 2                 | 4                 |
  | YV411     | 4     | 1                 | 2                 |
  | YV16      | 2     | 1                 | 2                 |
  | YV24      | 1     | 1                 | 2                 |

### 2. `ViewModels/FilterScribeVM.cs`
Add crop section properties and wiring.

- **New fields**: `_cropTargetWidth`, `_cropTargetHeight`
- **New properties**:
  - `HasCropFilter` (bool) — true when crop is needed (dimensions differ)
  - `CropTargetWidth`, `CropTargetHeight` (int)
  - `CropTargetDisplay` (string) — e.g. "1920x1078"
  - `FfmpegCropFilter` (string) — `-vf "crop=1920:1078:0:0"` or N/A
  - `VapourSynthCropFilter` (string) — `src = core.std.CropAbs(src, 1920, 1078)` or N/A
  - `AviSynthCropFilter` (string) — `Crop(0, 0, 1920, 1078)` or N/A
  - `CropModWidthHint`, `CropModHeightHint` (string) — hint like "mod-2" or "no restriction"
- **Recalculation**:
  - Call `RecomputeCrop()` at end of `ParseSourceResolution()` and `ParseColorSpaceInfo()`
  - `RecomputeCrop()` uses `CropCalculator.GetCropDimensions()` with `_colorSpaceAnalysis.PixelFormat`, `SourceWidth`, `SourceHeight`, and progressive state from ffprobe
- **Wire into language switch**: Add `OnPropertyChanged` for all crop properties in `OnLanguageChanged()`

### 3. `Views/FilterScribeModal.xaml`
Add a new "Crop" section in the bottom StackPanel (Grid.Row="3"), following the existing pattern (like ResolutionScale section).

- Section header: `CropTitle`
- Conditionally visible for all three tabs (AVS, VPY, ffmpeg)
- Shows:
  - Crop target display (e.g. "1920x1078")
  - Mod requirement hint
  - Read-only TextBoxes for each of the 3 filter formats (ffmpeg, VPY, AVS) — same pattern as Denoise section
- Visibility: collapsed when `HasCropFilter` is false

### 4. `Models/Lang/FilterScribeModalLangProvider.cs`
Add localized strings for all supported languages:

- `SrcScribe.CropTitle` — "Crop for Colorspace Compliance" / "色彩空间合规裁切" / etc.
- `SrcScribe.CropModHint` — "Width {0}, Height {1}" (mod requirements)
- `SrcScribe.CropNoRestriction` — "none"

## Implementation Order

1. Create `FFmpeg/CropCalculator.cs`
2. Add crop properties and `RecomputeCrop()` to `FilterScribeVM.cs`
3. Add localization strings to `FilterScribeModalLangProvider.cs`
4. Add crop UI section to `FilterScribeModal.xaml`
5. Build & verify no errors

## Verification
- `dotnet build` should succeed with no errors
