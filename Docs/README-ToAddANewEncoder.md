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

1. Register it in `Models/ToolDefinitionProviderM.cs` and make sure the executable name is included in import/version flows.
2. Add persistent settings to `Models/EncoderConfM.cs`.
3. Add preset rows to `Models/EncoderPresetsM.cs`.
4. Update `ViewModels/EncoderConfVM.cs` and `Views/EncoderConfModal.xaml` for the UI.
5. Extend `Pipeline/EncodingPipeline.cs` for base params, auto params, custom params, and parallelism params.
6. Add version parsing in `ToolManagement/ToolVersionDetect.cs`.
7. Update localization text in `Models/Lang/EncoderConfLangProvider.cs` and related language providers.

### Exceptions

If there is no Keyframe interval / max GOP size, CRF control or anything that breaks the UI pattern, leave a textbox to indicate it, ever fake a control.

### Parallelism

- Thread count and NUMA affinity must be derived from `ParallelismConfM.DownstreamNodeId` / `.EncoderThreadCount` / `.PreferPhysicalCores`.
- Use `CpuSets.ClampThreadCountForNode()` to get the effective thread cap for the selected node.
- Add the resulting flags in `EncodingPipeline.BuildParallelismEncoderParams()` via a new switch branch.
- If the encoder does not support thread/affinity control, leave the existing scaffolding returning `string.Empty`.

## Current pipeline

- The app uses Y4M piping into the encoder, then muxes when needed.
- `ffprobe` JSON is used for source analysis and auto-parameter generation.
- Auto params currently cover frame count and color metadata for all supported encoders.
- `x264` also gets `--rc-lookahead` and `--threads`.
- `x265` also gets `--rc-lookahead`, `--merange`, `--subme`, and `--pools`.
- `svtav1encapp` currently gets frame count plus color metadata and chroma location handling.
- Custom third-party toggles are filtered per encoder before command generation.

### Plan a new pipeline

1. Search and read the video encoder's parameter documentation, see what parameters can be derived from ffprobe JSON
2. Do a ffprobe run on a typical video
    - `ffprobe -i <source> -select_streams v:0 -v error -hide_banner -show_streams -show_frames -read_intervals "%+#1" -of json`
    - Or, run this app, do an ffprobe video analysis, click the 'Copy Raw JSON' button to have it
3. See what parameters are outside of Y4M pipe deliverables, and should be adjusted based on FPS, resolution, etc.
4. It's common for developers to restrict their video encoder can truely perform, lift these restrictions by increasing parameter intensity, but not maxxing them out, which would be way too slow
5. If there is a good 3rd-party modified video encoder which implemented extra parameters, add it into the 3rd party parameter toggle section, but keep it simple, not comprehensive

### CRF integer slider alignment

The slider is the user's primary quality control. It must be aligned so the ticks feel comparable to existing encoders.

1. Pick a test set covering animation, film/IRL, grainy, low/high motion, and noisy clips.
2. Freeze all non-target settings: keyframe, parallelism, toggles, custom text.
3. Sweep the encoder's CRF range and encode each test clip.
4. Encode the same clips with x264 / x265 / SVT-AV1 at their current slider defaults.
5. Compare outputs subjectively and build a mapping from quality tier to slider value:
   - `Lossless / UHQ / HQ / Streaming / Default`
6. Limit the slider to a useful sub-range and label ticks accordingly.
7. If the encoder has a logarithmic or non-linear scale, enable the slider's logarithmic mode in XAML instead of trying to linearize it manually.

## UI notes

- CRF and ABR are presented as fixed integer sliders, not free-form scales.
    - Slider range and ticks should adhere to arbitrary VQA quality ranking noted as `Lossless/UHQ/HQ/Streaming/Default`
    - Don't expose the entire CRF range since it usually has over 50 values in integer, which shrinks the slider by values that nobody uses, and worsens the UX
- Keyframe controls are shown in seconds in the UI and converted in the pipeline.
    - Slider range and ticks should adhere to arbitrary decoding difficulty noted as `Eco./Multi-track Edit|Mid.|Hard & Mid. Compression|Extreme & High Comp.`
- Presets are grouped by content/use case, not by a universal quality ranking.
    - Presets should adhere to encoder's parameter design philosophy
    - i.e., If an encoder hides most parameters behind its preset parameter, then use a content-unaware simple approach, like quality-first, compression-first, and vice-versa
- Parallelism is edited in a separate modal and can affect the encoder command line.
    - Check for thread count, CPU affinity, and NUMA-related parameters

### Edge cases

Integer slider has a logrithm mode, use it to combat inconsistent parameter value progressions

## Checks after adding support

- Import the tool and confirm version detection works.
- Open encoder settings and verify sliders, presets, and toggles render correctly.
- Use the "Copy RAW JSON" button and check against commandlines displayed in a Conformation pop-up when clicking on "Start Encode" button
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
