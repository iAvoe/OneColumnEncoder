# 1cenc Concepts & Features

## Design Philosophy

### Motivation

Video encoding is an optimization problem constrained by the impossible triangle of visual quality, encoding speed, and file size——There is no optimal solution. Beginners and novices can only build experience through trial-&-error, while even advanced users frequently make mistakes involving typos, file paths, text copy (selection) errors, or spec-compatibility failures between tools, which leads to the tedious, disciplinary verification step, otherwise, a slight oversight can force a start-over, wasting hours of time & computing resources. Regardless, these issues are not only troublesome, but also ruins the speed advantage brought by high-end hardware.

The video encoding ecosystem is splintered. Some tools are over-simplified, some be like nuclear power control panels, some are non-general solutions for specific problems, and some retain lots of outdated features due to decades of updates. Furthermore, there are UI/UX issues, such as requiring back-&-forth window switching, outdated defaults, and sometimes lack of visual feedbacks. These problems require users among all levels to spend a while on familiarizing. Conversely, a command-line interface with consistent interaction logic is often more efficient and user-friendly.

### Core Positioning

1cenc (OneColumnEncoder) is a Windows video encoding assistant built on .NET 9 / WPF. It does not attempt to dumb down video encoding into a black box. Instead, it automates common, repetitive, and error-prone steps while retaining full customization of external tools, scripts, encoder parameters, and command lines.

1cenc's design balances hardcore workflows with an intuitive UI — letting users quickly start encodes while still being able to inspect and intervene in the underlying logic when needed.

### Single-Column UI Interaction

1cenc uses a "single-column" main interface: tool import, upstream programs, encoders, dependencies, video sources, script sources, source validation, encoding settings, and start encoding are all laid out in workflow order on the main interface rather than hidden in nested menus.

Benefits:

- Common operations can be done directly on the main interface, reducing back-and-forth navigation into settings pages.
- Tools, sources, check status, and output settings are visible at the same time, making it easier to understand the current encoding pipeline.
- Selecting upstream programs, encoders, source files, and script sources can mostly be done in a single click or import — in contrast, opening a dropdown and selecting requires at least two clicks, two cursor movements, and smaller button areas that are harder to target.

The trade-off is a longer main interface that requires scrolling. To address this, the project provides `▶▶` / `▼▼` collapse mechanisms so users can collapse sections while keeping key status visible. First-time users are advised to expand all sections and read through the full layout before collapsing to their preference.

### Main-Interface-First Interaction

1cenc is designed so that encoding can begin without entering secondary configuration screens. After importing tools, selecting a source, analyzing it, and setting output, users can start encoding directly from the main interface.

Secondary windows primarily handle:

- Fine-tuning encoder parameters
- Parallelism / NUMA / thread settings
- Filter and script editing
- Clip sampling
- Encoding monitoring
- App settings and cleanup

#### Start Encode Button Blocking Conditions

Conditions that gray out the Start Encode button (making it unclickable) mainly come from states that would inevitably cause task failure, such as:

- Required tools not imported/selected: at minimum, an upstream program, an encoder, and ffprobe.
- No video source / queue / concat source imported.
- Source video corrupted to the point that ffprobe cannot open and analyze it — critical data missing.
- Upstream program and script source mismatch: e.g., `vspipe` requires `.vpy`, `avs2yuv` / `avs2pipemod` require `.avs`.
- `avs2pipemod` selected without `avisynth.dll` dependency (or vice versa — they must match).
- Output directory not writable, insufficient disk space, or other hardware/system issues.
- Output file already exists: the overwrite guard raises a warning on the pre-encoding checklist card (shown in orange), which grays out the Start Encode button. Users can manually bypass the checklist to unlock the button, but a cooldown-based secondary confirmation dialog (proportional to the overwritten file's size) will appear after clicking Start.

Some warnings in source validation and pre-encoding checklists can be bypassed. The principle: errors block inevitably failing or high-risk processes; warnings notify the user but leave the final choice to them.

### Low-Interference Colors and High-Visibility Status Colors

1cenc's normal UI elements use low-brightness, low-contrast colors to reduce visual strain during extended configuration and monitoring sessions. Status indicator colors use more recognizable traffic-light logic:

- Red: error, usually blocks further progress.
- Orange/Yellow: warning, should be addressed but can be bypassed.
- Cyan/Green: success, normal, conditions satisfied.

This design lets users quickly scan the main interface to determine whether the workflow is ready, rather than reading each check item individually.

### Mouse-Friendly Design

1cenc assumes users may understand encoding but may not want to memorize long command lines. The project strives to make all operations achievable with a mouse.

UI controls make extensive use of:

- Cards: representing tools, sources, settings, and status.
- Dropdown menus: selecting tool import types, language, encoding modes, etc.
- Sliders: configuring CRF, ABR, keyframe intervals, scaling ratios, thread counts, and other continuous or stepped parameters.
- Toggles / Checkboxes: controlling filters, third-party parameters, parallelism preferences, and other boolean options.
- Copyable text: displaying paths, commands, logs, analysis JSON, etc. — right-click context menu to open or copy.

### Command Display Mechanism

The final command is shown before starting an encode, and can be reviewed again in the encoding monitor — making it easy to inspect and reproduce.

---

## Encoding Modes

1cenc supports four main source routes: single mode, queue mode, concat mode, and Repart mode.

### Single Source Mode

Single source mode is the most common workflow: one video source or script source produces one output.

Available routes include:

- `ffmpeg` directly reads the video source and outputs a Y4M pipe.
- `vspipe` reads VapourSynth `.vpy` scripts.
- `avs2yuv` / `avs2pipemod` reads AviSynth `.avs` scripts.
- `OneLineShotArgs` reads SVFI-related source configuration for the SVFI route.

Single source mode supports clip sampling — users can extract a segment by time or frame number for quick subjective quality verification.

### Queue Mode

Queue mode is for "a batch of independent video sources producing a batch of independent outputs." Typical use cases include TV series, BDRips, and other already-segmented streams with consistent formats.

Queue mode imports a folder. After import, ffprobe analyzes all candidate sources and filters out streams with inconsistent formats based on compatibility rules.

#### Queue Filtering Strategies

1cenc provides two strategies:

1. Use the first video stream as reference, excluding files whose validation results differ too much.
2. Weighted vote then use the representative group as reference — suitable for BDMV directories mixing main features, intros, trailers, menu clips, etc.

#### Queue Mode Characteristics

- Number of source files equals number of output files; one-to-one automatic muxing.
- Tasks execute sequentially, not running multiple encode jobs in parallel.
- Queue sidebar shows Pending, Encoding, Completed, Failed, Interrupted states.
- Generates a batch of `.avs` / `.vpy` scripts for the queue.
- Supports filtering out short segments by minimum duration.
- Does not support clip sampling.
- Does not support `OneLineShotArgs` / SVFI route.

### Concat Mode

Concat mode is for "multiple video fragments spliced into a single output." Its primary use case is handling BDMV main features that have been split into multiple segments — i.e., multi-part sources or "playlist-based" discs.

Concat mode performs basic compatibility checks on import:

- At least two video files required.
- File extensions must match.
- Resolutions must match.
- Differences in codec, pixel format, validation checklist signature, and frame rate produce warnings.
- VFR and frame-rate signature differences are treated as warnings, directing users to apply VFR→CFR repair in the filter editor.

Concat mode generates the `ffmpeg concat demuxer` filelist and can produce concat `.avs` / `.vpy` scripts. The filter editor also allows reordering, removing fragments, and regenerating the filelist.

Audio handling in concat mode is a muxing step outside the video pipeline: generated AVS/VPY scripts handle video only; after encoding, ffmpeg attempts to copy audio streams from the concat filelist sources into the final MKV. For VFR sources or complex boundary fragments, audio duration may exceed video duration — it is recommended to check and handle audio after concat encoding.

### Repart Mode

Repart mode treats an ordered set of strictly matching CFR video streams as one virtual frame timeline, then manually repartitions that timeline into independent episode outputs. It handles both joined sources containing several episodes and fragmented sources where one episode spans several files.

The dedicated partition-style window provides input and output sidebars, a proportional allocation map, synchronized time/frame fields, unallocated gaps, and adjacent-output merging. The first implementation does not read chapters or MPLS and does not copy source audio, subtitles, chapters, or metadata. Every output is encoded independently and muxed as a video-only MKV.

#### Concat Then Split

Repart mode lands the "concatenate into one virtual frame timeline, then split per output range" commands per upstream tool as follows:

- **ffmpeg**: opens every source separately (`-i source1 -i source2 ...`), maps each source's `v:0` only, joins the streams in import order with the `concat` filter, then crops the current output range with frame-exact `trim=start_frame=first:end_frame=last+1` and regenerates a CFR PTS sequence from zero with `setpts`. For a single source the concat stage is omitted and the frame-exact `trim` is applied directly.
- **vspipe**: generates a splice script containing all sources (`core.std.Splice`) and slices with `-s {first} -e {last}` at the vspipe command line.
- **avs2yuv / avs2pipemod**: generates a splice script containing all sources (AviSynth `++` UnalignedSplice); avs2yuv slices with `-seek {first} -frames {count}`, avs2pipemod with `-trim={first},{last}`.

Slicing is based on the measured original frame numbers from the analysis stage, accumulated into global frame indices across sources, so output boundaries stay exact after concatenation. Each encoding start creates execution-specific concat filelists and virtual-source scripts, so a stale or reordered external script cannot invalidate the planned frame offsets.

#### Filters Apply Only to the New Trimmed Output

Repart mode has a hard constraint: **imported video sources are never modified.** Every filter (scaling, frame-rate / VFR→CFR repair, denoise, color conversion, etc.) may only act on the new source after it has been trimmed to an output range. The reasons:

- The analysis stage builds the virtual timeline from each imported source's original frame rate, resolution, and per-frame timestamps.
- If any source changes its frame rate or resolution before concatenation, the sources can no longer be joined reliably and already-planned output ranges shift as a whole.
- Therefore Source Reviser and Filter Scribe are unavailable for an active Repart plan; one-click script generation only emits the "concat + trim" skeleton and never rewrites the imported sources.

---

## Encoding Pipeline

### External-Encoder-First Design

ffmpeg includes built-in video encoders, but 1cenc deliberately does not use ffmpeg's built-in encoders as the primary encoding route. Instead, it treats ffmpeg / vspipe / avs2yuv / etc. as upstream decoding and filtering tools, and independent programs like x264 / x265 / SVT-AV1 as downstream encoders.

The formal encoding pipeline looks like this:

```text
Upstream program outputs Y4M -> Pipe -> External encoder -> Raw video stream -> Optional ffmpeg mux
```

Reasons for this design:

- Encourages using standalone or third-party modified encoders.
- Encourages using the latest encoder versions, as well as self-compiled builds (↑ quality, ↑ efficiency).
- Easier to leverage native encoder parameters and experimental options.
- More transparent for command-line encoders.
- Encoding commands can be displayed, copied, and reviewed directly.
- Encoders embedded in ffmpeg only show their version when encoding starts, requiring a dedicated command-line argument (inconvenient).

The trade-off is that adapting new encoders is more complex. Each new encoder requires handling tool import, version detection, parameter UI, presets, auto-parameter generation, output extension, and mux input format details.

### Auto-Parameter Generation

1cenc automatically supplements certain encoding parameters based on ffprobe analysis results, including:

- Total frame count: `--frames` for x264/x265, `-n` for SVT-AV1.
- Color matrix, transfer characteristics, primaries.
- Color range.
- Chroma location.
- Lookahead for x264/x265.
- `merange` and `subme` for x265.
- In concat mode, uses the sum of all fragment frame counts instead of only the first fragment's count.

The project does not expose every parameter as a UI control. Commonly used parameters are surfaced as sliders, presets, and toggles; more advanced or unstable options are left to the custom parameters text box.

#### Frame Count Policy

When total frame count is missing from video metadata, 1cenc leaves it unknown instead of synthesizing a value from duration and average frame rate. That avoids feeding guessed counts into encoder frame limits or the encoding monitor's progress display. In that case, frame-based progress is omitted.

### Output and Auto-Muxing

The encoder first outputs the corresponding raw video stream:

- x264: `.mp4`
- x265: `.hevc`
- SVT-AV1: `.ivf`

If ffmpeg has been imported, the project generates a mux command to combine the encoded video stream with audio, subtitles, chapters, and metadata from the source into an MKV. Single source and queue mode copy non-video streams from the original video; concat mode copies audio streams from the concat filelist.

Muxing is a controllable step — the mux command and status are visible in the encoding monitor window. For raw stream outputs like x265 / SVT-AV1, muxing makes it easier to add new streams or convert to other container formats, so it is checked by default.

## Source Analysis and Checklists

1cenc relies on ffprobe to read source video metadata. Source checks fall into two categories:

Hard issues:

- Metadata cannot be read.
- Not progressive scan.
- Bit depth exceeds supported range.
- 12-bit input (not currently supported by SVT-AV1).
- Other issues that would make parameter generation or encoding clearly unreliable.

Soft issues:

- VFR / non-constant frame rate.
- SAR not 1:1 / non-square pixels.
- Missing color matrix.
- Missing transfer characteristics.
- Missing primaries.
- Chroma subsampling / chroma location may be unsuitable for the current encoder.

Hard issues generally block encoding start; soft issues prompt the user to use the filter editor or confirm and bypass.

---

## Filter & Script Editor

The filter editor is an auxiliary module in 1cenc. It supports:

- One-click AviSynth(+) `.avs` script generation.
- One-click VapourSynth `.vpy` script generation.
- Batch script generation for queue mode.
- Multi-fragment concat script generation.
- ffmpeg filter parameter generation and hints.
- VFR→CFR conversion.
- SAR repair.
- Resolution scaling.
- Color space conversion.
- HDR→SDR conversion.
- hqdn3d denoise example.
- Subtitle burning example.
- Custom ffmpeg filter command-line parameters.
- Custom AviSynth(+), VapourSynth filter script lines.

For script sources, the project validates that the video path embedded in the script matches the current video source. Queue scripts are checked per file name and embedded path to prevent misalignment. Concat mode skips single-source path matching (since scripts naturally contain multiple source paths) and instead relies on concat import and analysis to ensure correctness.

Filter Scribe is unavailable in Repart mode: episode boundaries depend on the imported sources' original frame numbers, and any filter that changes a source's frame rate or resolution before trimming would break the virtual timeline. If filters are wanted, they may only act on the trimmed output range (see "Repart Mode / Filters Apply Only to the New Trimmed Output").

---

## Encoder Parameters & Preview

Encoder settings support CRF / ABR, presets, keyframe intervals, and some third-party parameter toggles for x264, x265, and SVT-AV1. Rather than exposing every encoder parameter as a form, the project builds presets aligned with each video encoder developer's design intent.

The project also provides a single-frame A/B preview:

- Extracts a frame from the source video at a specified position.
- Encodes the frame using ffmpeg's libx264 / libx265 / libsvtav1 / libvvenc for single-frame preview encoding.
- Decodes the result and compares it with the original side by side.
- Supports different display modes: raw display, low gamut → BT.709, WCG → BT.709, HDR → SDR, etc.
- If SSIMULACRA2.1 and Butteraugli tools are bundled, computes image quality scores.

The preview feature is for quickly observing parameter impacts. Quality depends on whether the video encoder's single-frame encoding is better or worse than inter-frame redundancy, so it is not equivalent to the formal encoding pipeline.

---

## Parallelism & Hardware Scheduling

1cenc relies heavily on Windows APIs for parallelism, making it explicitly a Windows-only tool rather than a cross-platform utility.

Parallelism settings include:

- Upstream NUMA node.
- Downstream encoder NUMA node.
- Whether to prefer physical cores.
- Encoder thread count limit.
- x264 thread parameters.
- x265 pools parameters.
- CPU Sets binding.

The project does not raise process priority. The design prioritizes keeping the system responsive, avoiding a situation where an encoder hang drags down the entire system.

Pre-encoding checks also include:

- Whether AC power is connected.
- Whether the output directory is writable.
- Whether sufficient disk space is available.
- Whether an output file would be overwritten.
- Whether the L-SMASH plugin required for the avs2yuv route is present.
- Whether current NUMA node CPU usage is high.

The overwrite check not only warns but also delays the confirmation button based on the overwritten file's size to reduce the chance of accidentally overwriting large files.

---

## Encoding Monitor

The encoding monitor window handles running upstream and downstream processes, piping upstream stdout to the encoder stdin. It simultaneously reads upstream and downstream stderr for log display and progress parsing.

Monitor features include:

- Upstream / downstream log split panes.
- Log folding to avoid repeated line flooding.
- Save upstream / downstream stderr.
- View encoding command.
- Progress percentage when a reliable total frame count exists.
- Written frame count when a reliable total frame count exists.
- Current output size.
- Estimated output size when progress tracking is available.
- Elapsed time, remaining time, estimated completion time when progress tracking is available.
- Upstream / downstream memory usage.
- Working set peak, Page Fault, memory pressure.
- Manually interrupt upstream or encoder.
- Window can only be closed after encoding completes.
- Queue mode shows each task's status.

Queue tasks execute sequentially; on failure, subsequent tasks stop. After user interruption, there is an option to stop remaining queue tasks.

---

## Tool Import & Portability

1cenc assumes no system paths are configured by default. Users can import external tool paths, and the project saves each tool's path, version, and file size.

Supported importable tools include:

- Upstream: `ffmpeg.exe`, `vspipe.exe`, `avs2yuv.exe`, `avs2pipemod.exe`, `one_line_shot_args.exe`
- Encoders: `x264.exe`, `x265.exe`, `svtav1encapp.exe`
- Analysis / Dependencies: `ffprobe.exe`, `avisynth.dll`

On first launch, the project attempts to auto-discover some tools, such as VapourSynth's `vspipe.exe` and Steam-distributed SVFI's `one_line_shot_args.exe`.

Distribution packages can bundle common encoders and analysis tools, but the project philosophy still encourages users to replace them with trusted newer or third-party modified versions.

---

## Configuration & Temporary Files

### App Configuration Directory

All major persistent configuration is stored in the `1cenc` folder under the application base directory:

- `appconfig.json`: App settings, language, overwrite confirmation behavior.
- `appdata.json`: Tool paths, versions, sizes, source paths, output directory, main interface collapse states, etc.
- `encodingconfig.json`: Encoder parameter configuration.
- `parallelismconfig.json`: Parallelism and NUMA settings.

### Queue Mode

Queue source analysis generates timestamped JSON files in the `1cenc` config directory:

- `source_queue_yyyyMMdd_HHmmss.json`: Accepted queue sources with their ffprobe JSON.
- `source_queue_excluded_yyyyMMdd_HHmmss.json`: Filtered or excluded sources.

The settings interface can clean `source_queue_*.json` files older than 7 days.

### Concat Mode

Concat mode generates in the `1cenc` config directory:

- `source_concat_filelist.txt`

This file uses ffmpeg concat demuxer format, recording the current concat fragment order. It is regenerated when users reorder or delete fragments in the filter editor.

### Script Files

The filter editor and one-click script generation produce files at user-chosen locations:

- `.avs`
- `.vpy`
- Occasionally `.txt` for ffmpeg filter / filelist content

Default script file names use the source file name. Queue mode generates identically named scripts for each source.

### LWLibavVideoSource / LWLibavSource Index

When AviSynth and VapourSynth scripts use L-SMASH / LWLibav for video reading, the underlying plugin may generate index files (`.lwi`) in the video source directory. This is decoder plugin behavior, not a 1cenc configuration file.

### Preview & Logs

Single-frame preview creates a `1cenc-image-preview-*` working directory under the system temp folder, which is cleaned up on window close.

When saving logs in the encoding monitor, the following files are written to the user-chosen output location:

- `upstream-stderr.txt`
- `downstream-stderr.txt`

---

## Current Limitations

- Windows only. The project depends on WPF, CPU Sets, NUMA, process and memory-related Windows APIs.
- Currently supports x264, x265, and SVT-AV1 as formal encoders.
- SVT-AV1 does not support 12-bit input.
- Queue, concat, and Repart modes do not support clip sampling.
- `OneLineShotArgs` / SVFI route does not support queue, concat, or Repart mode.
- Repart mode currently requires CFR, reliable per-frame timestamps, identical video-stream formats, manual output boundaries, and ffmpeg for video-only MKV output.
- Concat mode audio muxing is available, but complex VFR or boundary-abnormal sources may still require post-processing.
- The project currently has no comprehensive automated tests — it relies on functional validation and real long-queue testing.
