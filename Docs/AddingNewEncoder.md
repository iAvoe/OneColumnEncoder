# Add a New Encoder

## What exists now

- Supported encoder entries are `x264.exe`, `x265.exe`, and `svtav1encapp.exe`.
- `ToolDefinitionProviderM` is the source of truth for encoder registration and import labels.
- `EncoderConfM`, `EncoderConfVM`, and `EncoderConfModal.xaml` hold encoder-specific CRF, ABR, preset, keyframe, and toggle UI.
- `EncoderPresetsM` contains the preset tables and third-party toggle definitions.
- `EncodingPipeline` builds the command line, applies `ffprobe`-based auto params, and adds parallelism settings.
- `ToolVersionDetect` handles version detection for importable tools.
- `ParallelismConfM` and `ParallelismConfVM` control NUMA/thread settings for the downstream encoder.

## How a new encoder is wired

1. Register it in `Models/ToolDefinitionProviderM.cs`, include the executable in import/version flows.
2. Add persistent settings to `Models/EncoderConfM.cs`.
3. Add preset rows to `Models/EncoderPresetsM.cs`.
4. Update `ViewModels/EncoderConfVM.cs` and `Views/EncoderConfModal.xaml` for UI.
5. Extend `Pipeline/EncodingPipeline.cs` for base, auto, custom, and parallelism params.
6. Add version parsing in `ToolManagement/ToolVersionDetect.cs`.
7. Update localization in `Models/Lang/EncoderConfLangProvider.cs` and related providers.

### Exceptions

If there is no Keyframe interval / max GOP size, CRF control or anything that breaks the UI pattern, leave a textbox to indicate it, ever fake a control.

### Parallelism

- Derive thread count and NUMA affinity from `ParallelismConfM.DownstreamNodeId` / `.EncoderThreadCount` / `.PreferPhysicalCores`.
- Use `CpuSets.ClampThreadCountForNode()` for the effective thread cap on the selected node.
- Add the resulting flags in `EncodingPipeline.BuildParallelismEncoderParams()` via a new switch branch.
- If the encoder lacks thread/affinity support, leave the existing scaffolding returning `string.Empty`.

## Current pipeline

- The app uses Y4M piping into the encoder, then muxes when needed.
- `ffprobe` JSON is used for source analysis and auto-parameter generation.
- Auto params currently cover frame count, color metadata, range, chroma, lookahead, and HDR metadata for all supported encoders.
- HDR10 mastering display / content light level are read from stream side data when available, otherwise from the first decoded frame side data (`-show_frames -read_intervals "%+#1"`).
- `x264` also gets `--rc-lookahead` and `--threads`.
- `x265` also gets `--rc-lookahead`, `--merange`, `--subme`, and `--pools`.
- `svtav1encapp` currently gets frame count plus color metadata, chroma location handling, and HDR mastering-display / content-light metadata.
- Custom third-party toggles are filtered per encoder before command generation.

### Plan a new pipeline

1. Read the encoder's parameter docs, note which params can be derived from ffprobe JSON.
2. Run ffprobe on a typical video:
    - `ffprobe -i <source> -select_streams v:0 -v error -hide_banner -show_streams -show_frames -read_intervals "%+#1" -of json`
    - Or use this app's ffprobe analysis and click 'Show Raw JSON'.
3. Identify parameters outside Y4M pipe deliverables that depend on FPS, resolution, etc.
4. Developers often restrict encoder capabilities — lift those limits by increasing parameter intensity, but not maxing them out (too slow).
5. For good 3rd-party encoder forks with extra parameters, add them to the 3rd-party toggle section, keeping it simple.

### CRF integer slider alignment

The slider is the user's primary quality control. Align ticks to feel comparable to existing encoders.

1. Pick a test set: animation, film/IRL, grainy, low/high motion, noisy clips.
2. Freeze all non-target settings: keyframe, parallelism, toggles, custom text.
3. Sweep the encoder's CRF range and encode each test clip.
4. Encode the same clips with x264 / x265 / SVT-AV1 at their current defaults.
5. Compare outputs and map quality tiers to slider values: `Lossless / UHQ / HQ / Streaming / Default`.
6. Limit the slider to a useful sub-range, label ticks accordingly.
7. For logarithmic or non-linear scales, enable the slider's logarithmic mode in XAML instead of linearizing manually.

## UI notes

- CRF and ABR use fixed integer sliders, not free-form scales.
    - Ticks follow quality tiers: `Lossless/UHQ/HQ/Streaming/Default`
    - Don't expose the full CRF range (usually 50+ values) — it shrinks the slider and worsens UX.
- Keyframe controls show seconds in UI, converted in the pipeline.
    - Ticks follow decoding difficulty: `Eco./Multi-track Edit|Mid.|Hard & Mid. Compression|Extreme & High Comp.`
- Presets group by content/use case, not universal quality ranking.
    - Follow the encoder's design philosophy — if most params are hidden behind presets, use a content-unaware approach (quality-first, compression-first, etc.)
- Parallelism edits live in a separate modal and affect the encoder command line.
    - Check for thread count, CPU affinity, and NUMA parameters.

### Edge cases

Integer slider has a logarithmic mode — use it for inconsistent parameter value progressions.

## Checks after adding support

- Import the tool and confirm version detection works.
- Open encoder settings and verify sliders, presets, and toggles render correctly.
- Use the "Show RAW JSON" button and check against commandlines displayed in a Conformation pop-up when clicking on "Start Encode" button
- Confirm parallelism values land in the final encoder command.
- Run a short encode and check the monitor and mux output.

## How to code with quality

- Follow the existing MVVM pattern, create or modify Helpers, Converters, Commands to modularize
- Keep encoder-specific knobs out of presets if they are experimental or optional.

## Minimal checklist

- [ ] Tool registered in `ToolDefinitionProviderM` + `ToolCatalogProviderM`
- [ ] Persistent settings added to `EncoderConfM`
- [ ] Presets added to `EncoderPresetsM`
- [ ] UI bindings done in `EncoderConfVM` / `EncoderConfModal.xaml`
- [ ] Command generation extended in `EncodingPipeline` (base, auto, custom, parallelism)
- [ ] Version parsing added to `ToolVersionDetect`
- [ ] Localized text added for encoder name, presets, hints
- [ ] CRF slider mapped against arbitrary VQA quality tiers
- [ ] Parallelism flags derived from `ParallelismConfM` and added to command
- [ ] Version import, encoder settings, encode run all verified end-to-end
