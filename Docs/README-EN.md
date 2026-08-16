# OneColumnEncoder

A next-gen smart video encoding utility based on .NET 9/WPF, revolves around “tool & encoder orchestrating, source analyzing, encode customizing, parallelism tailoring, modern GUI monitoring, encode interrupting, and auto-multiplexing”.

<p align="center"><img src="../WireframeMockups/logo.png" alt="Logo" width="200"></p>

## Featuring:

**Source & Environment Validation**
- Validate source video integrity from ffprobe readings
- Verify current environment & configuration correctness
- Providing source checklist with Success, Warning & Error status types
- Providing bypass feature to continue on error

**Advanced Parallelism**
- Physical Core & NUMA Binding on top of Thread limiting

**Encode Parameter Setting**
- Customize via tuned UI controls for customized parameter configurations
- Providing encoder A/B comparison previewer

**Video Filtering**
- Providing FFMPEG-VS-AVS filter editor
- Auto-generating basic AviSynth & VapourSynth scripts (Decode→Y4M)
- Auto-generating filters for FFMPEG-VS-AVS:
  - VFR→CFR
  - HDR→SDR (WCG)
  - Color space conversion
  - SAR restoration
  - Resize
  - Descale
- Providing additional OpenCL filters for VapourSynth
  - Denoise
  - Deband
  - Gaussian Blur
- Providing VapourSynth A/B comparison previewer

**Managed Automation**
- Auto-generating video & audio encoding commandlines
- Advanced encoding modes supported:
  - **Queue Mode**: Encoding and filtering for a list of video sources
  - **Concat Mode**: Concatenating and filtering a group of video sources
  - **Repart Mode**: Re-slicing, filtering and encoding of a group video sources to correct episode partition structure

**Editable Blu-Ray Chapter Import**
- Parse and build your list from PLAYLIST folder of the BDMV structure
- Wired to Queue Mode and Repart Mode

**Clip Sampling**
- Providing FFMPEG-VS-AVS sampling
- Providing GUI to clip Time or Frame# range easily

**Encode Monitoring**
- Monitor memory usage details of along with encode ongoing
- View upstream & downstream (encoder) logs separately
- Selectively interrupt upsteam or downstream (encoder) processes

**Overwrite Protection**
- Fool-proof Start-Encode cooldown based on file size to be overwritten

## Gallery

This software supports multiple languages, but English text screenshots are used here to reduce the number of images. Some UI elements or text in the images may be outdated, but the overall layout and functional area divisions remain applicable. Please refer to the actual version you are using.

<p align="center">
<img src="../WireframeMockups/1-Main-Page-v4.png" alt="Main Window" width="600"/></br>
<sup><i>Main Page</i></sup></br>
<img src="../WireframeMockups/2-Script-Scribe-v4.png" alt="Script Scribe Window" width="1200" /></br>
<sup><i>Script Scribe (Editor)</i></sup></br>
<img src="../WireframeMockups/3-Encoder-Settings.png" alt="Encoder Setting Window" width="900" /></br>
<sup><i>Encoding Settings</i></sup></br>
<img src="../WireframeMockups/4-Parallelism-Setting-v5.png" alt="Parallelism Setting Window" width="400" /></br>
<sup><i>Parallelism Settings</i></sup></br>
<img src="../WireframeMockups/5-Clip-Sampler.png" alt="Clip Sampler Window" width="400" /></br>
<sup><i>Clip Sampler</i></sup></br>
<img src="../WireframeMockups/6-Encoding-Monitor.png" alt="Encoding Monitor Window" width="650" /></br>
<sup><i>Encoding Monitor</i></sup></br>
<img src="../WireframeMockups/7-Warning-Modal-OW-Guard.png" alt="Warning Modal & Overwrite Protection" width="350" /></br>
<sup><i>Warning Modal & Overwrite Protection</i></sup></br>
<img src="../WireframeMockups/8-BD-Playlist-Selector-v2.png" alt="Blu-Ray Playlist Selector" width="500" /></br>
<sup><i>Blu-Ray Playlist Selector</i></sup></br>
<img src="../WireframeMockups/9-Episode-Repartition-Editor-v3.png" alt="Repartition Configurator" width="650" /></br>
<sup><i>Episode Repartition Configurator</i></sup></br>
<img src="../WireframeMockups/10-Settings.png" alt="Settings" width="400" /></br>
<sup><i>Program Settings</i></sup></br>
</p>

## System Requirements

- Windows 10/11 x64
  - Recommended version: 1809/21H2 (LTSC) or higher; minimum: 1607

### Download OneColumnEncoder

- [GitHub Releases](https://github.com/iAvoe/OneColumnEncoder/releases)

### Download Encoding Tools

Follow this tutorial to get tool tailored for your use case:
- [Encoding Tools Download Tutorial](https://github.com/iAvoe/encoding-tools-download-tutorial)

If must TLDR; you may also use the tools included in download package (Not recommended but it works)

**Supported pipe upstream programs (decoding and filtering tools)**:
- ffmpeg
- vspipe (supports API 3.0 and 4.0 automatic recognition)
- avs2yuv
- avs2pipemod
- SVFI

**Supported pipe downstream programs (encoders)**:
- x264 core 165 or newer
- x265 v4.2 or newer
- SVT-AV1 v4.1 or newer

> Choose only the latest version of encoders to get the best performance (speed, quality, compression), plus less chances of memory leaks

## Icon usage

- Azure icons: [azureicons.com](https://www.azureicons.com)
- Game icon pack by NiewBie: [GitHub/Niewbie](https://github.com/Nieobie/Game-Icon-Pack)

## NuGet package usage

- Blu-Ray playlist parser by [tautcony](https://github.com/tautcony): [ChapterTool Core](https://github.com/tautcony/ChapterTool/pkgs/nuget/ChapterTool.Core)

---

## Validation Status

**OS Validated**：
- Windows 10 22H2
- Windows 11 25H2 (thanks to [Lofu](https://github.com/Ronifue))

**CPUs Validated**：
- Core i5 7600k (4C4T)
- Ryzen 9 9900X (2CCD 12C24T)
- EPYC 7R13 (6CCD 48C96T)
- Intel i7 14700K (thanks to Whithost)

> This includes L3 cache size and CCD-aware grouping detection

**Long Queue Validation**
- 30+ 4k videos and x265 encoding was successful (3 days uninterrupted)

---

## Support me

Its not esay to develop these tools. If this software helped, please consider sponsoring or promoting it.

<p align="center"><img src="../WireframeMockups/bmc_qr.png" alt="Support me -_-"><br><img src="../WireframeMockups/pp_tip_qr.png" alt="Pls support =_="></p>

## Project Status

### Done

#### Application Framework and Main Interface

- App startup, main window, and main interface layout have been implemented, with entry points in `App.xaml.cs`, `MainWindow.xaml`, and `Views/MainUI.xaml`
- `MainVM` is responsible for main interface module orchestration, including the tool area, source import area, analysis area, checklist, coding settings area, and startup area
- Modal window navigation, masking states, close commands, and basic command models have been implemented
- Multi-language switching mechanism has been integrated into the main interface, cards, buttons, and modal window text

#### Tool Import and Selection

- Importing, replacing, deleting, and selecting external tools.
- Defined & supported upstream tools include `ffmpeg.exe`, `vspipe.exe`, `avs2yuv.exe`, `avs2pipemod.exe`, and `one_line_shot_args.exe`
- Defined & supported encoders include `x264.exe`, `x265.exe`, and `svtav1encapp.exe`
- Defined & supported analytics and dependencies include `ffprobe.exe` and `avisynth.dll`
- Tool version detection, filename verification, tool partitioning, default selection, and dependency/source compatibility refresh are implemented

#### Source Import and Script Generation

- Importing regular video sources, AviSynth scripts, VapourSynth scripts, and SVFI ini sources
- Supports persistent source file paths and refilling existing source files upon startup
- One-click generation of AviSynth/VapourSynth scripts is implemented, writing `.avs` and `.vpy` files and refilling them into the script source card
- Linkage between source type and upstream tool selection has been implemented
  - For example, `vspipe.exe` corresponds to `.vpy`, and `avs2yuv.exe` / `avs2pipemod.exe` corresponds to `.avs`

#### ffprobe Source Analysis and Inspection

- JSON analysis of `ffprobe` has been implemented, and the original analysis results can be copied
- The source inspection card has parsed and displayed progressive, bit depth, frame rate, SAR, color metadata, chroma, HDR / Dolby Vision source info, and other inspection items
- Viewing source inspection issues, refreshing the checklist status, and manually bypassing them have been integrated into the main workflow

#### Pre-coding Inspection

- The pre-coding inspection card has implemented hardware and software inspection items
- Includes checks on AC power, output directory, disk space, write permissions, output file overwrite, and AviSynth / L-SMASH
- Viewing pre-encoding issues, re-evaluating, and manually bypassing them have been integrated into the start button status

#### Encoding Parameter Configuration

- Encoding presets, keyframe intervals, and some third-party parameter switches have been implemented and persisted
- The encoding settings card displays a summary of the current encoding parameters
- The encoding pipeline generates corresponding encoder parameters based on the current configuration
- ffprobe-derived auto parameters now also cover HDR mastering display and content light level metadata when present, and the source inspection subtitle surfaces HDR / Dolby Vision source info

#### Encoding Command Generation and Startup

- Y4M pipeline command generation, supporting output from various upstream tools to x264/x265/SVT-AV1
- Command generation derives parameters such as color, range, chroma, and lookahead from ffprobe information; frame count is only used when it is already reliable
- A parameter confirmation window pops up before starting encoding; after confirmation, the encoding monitoring window appears

#### Sampling Clips

- Supports selecting clips by time & frame number, and supports timestamp-to-frame-number conversion
- Sampling segments will open the encoding monitoring process in sample mode
- SVFI / OneLineShotArgs currently do not support sampling segments; a disabled message is displayed on the main interface

#### Encoding Monitoring and Process Execution

- Supports starting upstream and encoder processes and passing upstream stdout pipes to encoder stdin
- Supports reading upstream/downstream stderr, log folding, saving logs, viewing encoding commands, and adjusting log fontsize
- Supports encoding progress when a reliable total frame count exists, written frame count, current/estimated output size, time elapsed, remaining time, and completion time estimation
- Supports memory usage, working set peak, Page Fault, memory pressure, and memory range statistics
- Supports interrupting upstream or encoder processes; the window can only be closed after encoding is complete

#### Parallelism Basic Capabilities

- NUMA node enumeration, CPU topology reading, CPU Sets allocation, and encoder thread limit are already implemented
- Parallel settings can be saved and applied to the upstream/encoder processes started by encoding monitoring
  - The encoder thread count is passed to the x264/x265 parameter; SVT-AV1 currently does not generate thread parameters

#### Multiple Instances (Run multiple instances)

- A single 1cenc instance can only bind the downstream encoder threads to one NUMA node at a time. If you have more encode jobs than one instance can handle and the machine still has spare NUMA nodes, start as many program instances as available NUMA nodes, then let each instance take a portion of the queue so every node stays busy.
- The instances are easy to tell apart: just look at the `PID xxxxx` text in the title bar or taskbar button. A PID is the process's temporary identifier, unique while it is running and not reused across instances.

#### Application Configuration and Persistence

- The basic logic for saving/loading JSON for application configuration, tool path, source path, encoding parameters, and parallel parameters has been implemented
- Language configuration has been integrated for saving and loading

#### Output Filename Tool

- Supports path preview, clipboard, illegal characters, reserved names, and length checks
- Output settings cards will be populated upon confirmation

#### UI Components

- Cards, button groups, dropdown menus, settings containers, checklists, integer sliders, fragment range selectors, memory range bars, and column text components have been implemented
- The currently used one-way UI converter meets existing binding requirements

#### Encoding Parameter Configuration Details

- `EncoderConfM.CustomParams` will be saved and appended to the final encoding command by `EncodingPipeline`
- The "Custom Parameters" area is no longer a summary of third-party switches, but instead directly reads and writes a free text parameter
- The parameter coverage for x264/x265/STV-AV1 is still limited, but it is usable

#### Scriptwriter

- The scriptwriter window, AVS/VPY editing area, copying complete scripts, and copying input/output fragments
- "Save As" is implemented
- "Confirm" has implemented script saving and backfilling
  - To simplify, it saves both AVS & VPY scripts simultaneously, same as the one-click generate button

#### Main Interface Best Practices Checklist

- `BestPracsSelfCheckCardVM` is a self-check reference card and does not participate in the start-encoding blocking conditions
- No `RunAllChecks()`, no `IsBypassed`, no Inspect/Bypass buttons
- Marked as "Advisory — not blocking" on the UI

#### Application Settings → File Overwrite

The Overwrite setting will append an overwrite confirmation pop-up if the output file already exists after displaying and confirming the compression command, and delay enabling the confirmation button according to the size of the overwritten file

#### Queue Mode

Implemented and verified, usability confirmed

#### Concat Mode

Implemented and verified, usability confirmed

#### Filter Scribe (Script Editor) Extended Features

Implemented and verified, usability confirmed

#### Repart Mode

Implemented and verified, usability confirmed

---

### Unverified

##### None currently

---

### Not Started

##### None currently

---

### Dead Ends

#### P-Core / E-Core Optimization

This feature cannot be implemented due to the need to modify the upstream program and encoder source code
- P-Core / E-Core related checkboxes in the UI are disabled
- This is really CPU manufacturers' task

### Large Pages Implementation

This feature cannot be implemented due to the need to modify the upstream program and encoder source code

#### Automatic Precise Keyframe Marker (qpfile)

Implementation failed due to excessive complexity and encoding time addition
- A better scene-change detection function than the video encoder's built-in method must be used
- A slower scene-change detection function will significantly increase overall encoding task duration (+50~150%)
- The final expected compression ratio & image quality improvement are only about 5% (compared to compression results without this function).

---

## Main Source Code Locations

- `Commands/`: User operation commands, modal window opening and closing, save loading, and encoding startup entry point
- `Pipeline/`: Encoding command-line building, frame counts, and muxing
- `FFmpeg/`: ffprobe analysis and FFmpeg process/argument helpers
- `ToolManagement/`: Tool registration, version detection, and compatibility rules
- `ScriptGeneration/`: AVS/VPY script templates
- `CPU/` / `Hardware/`: CPU/NUMA topology, thread sets, and OpenCL detection
- `Persistence/`: JSON save/load base classes
- `FileManagement/`: Source path routing, file pickers, and output path helpers
- `Models/`: Configuration models, tool definitions, language resources, checklists, and data DTOs
- `ViewModels/`: Main interface, modal window, and card state management
- `Views/`: WPF windows and interface XAML
- `Components/`: Reusable UI controls
- `Converters/`: WPF binding converters

### Testing and Engineering

- No unit test, integration test, or automated UI test projects are there
  - Some attempts were declared as not ready due to .Net 9.0 being new
- README has not yet included instructions for building, running, preparing dependencies, and typical workflows (although these are provided in the usage instructions window / AppUsageModal)

---

## Confirm action window (ConfirmationModal) Popup locations

- Confirm encoding commands before starting encode, and file overwriting:`Commands/StartEncCmd.cs`
- Sample clip confirmation before starting encode:`ViewModels/SampleClipVM.cs`
- View encoding commands in the encoding monitor:`ViewModels/EncodingMonitorVM.cs`
- Copy/save results after script generation:`ViewModels/ScriptScribeVM.cs`、`Commands/SaveLoad/OneClickScriptGenCmd.cs`
- Source analysis and check results:`Commands/AnalyzeSrcVideoCmd.cs`、`Commands/CopyRawAnalysisCmd.cs`、`Commands/InspectEncProblemsCmd.cs`、`Commands/InspectSrcProblemsCmd.cs`
- Secondary confirmation when importing tools/selecting files:`Commands/ImportToolCmd.cs`、`FileManagement/SrcFilePicker.cs`

## Settings Storage Location

All persistent configuration data is stored as **JSON files** under `{Application Base Directory}\1cenc\`:

| File | Contents |
|------|----------|
| `appconfig.json` | App settings (overwrite config, language selection) |
| `appdata.json` | Tool paths/versions/sizes, source video paths, output directory |
| `encodingconfig.json` | Encoder parameters (CRF/ABR, keyframe, presets, custom params for x264/x265/SVT-AV1) |
| `parallelismconfig.json` | Parallelism settings (NUMA node IDs, CPU preferences, thread count) |

**Persistence base class:** `Persistence\SaveLoadBase.cs` — all configuration models inherit from `SaveLoadBase<T>` which provides JSON serialization/deserialization via `Save()` / `Load()`.

**Other persisted data (user-selected paths, not in `\1cenc\`):**
- Generated script files (`.avs` / `.vpy` / `.txt`) via `ViewModels\FilterScribeVM.cs` and `Commands\SaveLoad\OneClickScriptGenCmd.cs`
- Stderr log files (`upstream-stderr.txt`, `downstream-stderr.txt`) to the output directory via `ViewModels\EncodingMonitorVM.cs`

---

## Disclaimer

## Limition of Liability

The developer shall not be liable for any direct, indirect, incidental, special, or consequential damages (including, but not limited to, loss of business profits, business interruption, computer system damage, data loss, and damage to goodwill) arising from the use or inability to use this software, even if advised of the possibility of such damages. Users assume all risks associated with the use of this software.

### Risks of Hardware Damage

Video compression is a long-duration, sustained, high-load CPU computing task. Under these conditions, the following factors, among others, may cause hardware damage:

- Improper heatsink installation, unstable overclocking, or excessively high voltage settings may lead to accelerated processor aging, electrical short circuits, or other hardware failures.
- Extreme computational loads may cause system unresponsiveness, blue-screen crashes, resulting in data corruption or loss.

### Protective Measures Provided by This Software

1. **x265 Stress Test Preset**: The software includes an x265 stress test preset for verifying system stability. However, the actual load of this test depends on the content complexity of the input video. Use a test video consistent with the target compression task as the source file for accurate validation.
    - This test can be more brutal & relentless than traditional stress-testing tools like Prime95, running it carries inherent risks. Run this test only while actively monitoring system temperature and status, and save all files before starting the test.
2. **No Process Priority Escalation**: This software does not raise the process priority of encoding tasks. This ensures the operating system and other programs remain responsive even under extreme encoder loads.
3. **File Overwrite Protection**: If an output file already exists before encoding begins, a confirmation window will appear. To prevent accidental data loss, the confirmation button's activation is delayed proportionally to the size of the file being overwritten.

### Recommended Protective Measures for Users

1. **Use Reliable Cooling Equipment**: Standard cooling fans may degrade rapidly under the wear and tear of prolonged, full-speed operation. Ensure the use of quality & heavy-duty cooling.
2. **Configure Stable Overclocking Strategies**: CPU & memory overclocking should be tuned for stable, long-term, high-load operation rather than short-term peak performance.
3. **Use an Uninterruptible Power Supply (UPS)**: Sudden power outages during high-load operations pose severe risks to hardware. A UPS provides critical buffer time to save data and shut down the system.
4. **Monitor Ambient Humidity**: High humidity can lead to electrical short circuits, especially under high-load, long-duration operation. Ensure the computer is in a dry environment.

---

### Why no Linux support / Why use WPF?

This software relies heavily on proprietary Windows APIs, which form the foundation of its core functionality; therefore, it is natively bound to the Windows platform:

1. **WPF Presentation Layer**: WPF has no official Linux support.
2. **Windows Kernel APIs (P/Invoke calls to `kernel32.dll`)**:
   - **CPU Sets**: Binds encoding processes to specific physical cores, avoiding hyper-threaded virtual cores.
   - **NUMA Topology**: Enumerates and specifies the NUMA nodes used by the encoder, ensuring that the visualization presented to the user is consistent with what the encoder sees.
   - **Process Enumeration**: Used for tracking sub-processes during encoding monitoring.
   - **Power State**: Checks whether the system is running on AC power before starting encoding.
   - **Memory Information**: Retrieves total physical memory and estimates allocation based on NUMA node proportions.
3. **`psapi.dll`**: Provides working set and memory pressure statistics for the encoding monitoring feature.

These APIs cover critical areas such as parallel scheduling, hardware detection, process monitoring, and pre-encoding checks—functionalities that cannot be replaced by cross-platform UI frameworks.

Since the backend is already locked to Windows APIs, choosing WPF became the natural decision. It provides native Windows desktop integration (including features to prevent window overflow), a mature MVVM data-binding ecosystem, and requires no browser kernels or third-party dependencies. While cross-platform frameworks solve UI portability, they cannot resolve underlying API incompatibilities; instead, they would only add an extra layer of abstraction cost and testing overhead.

In summary, the best approach for other platforms is to reimplement the project's logic using the corresponding native technology stack of that platform. Because the full source code is available and Agent-assisted programming tools exist—and this project has adopted the Apache 2.0 license to lower the barrier to entry—the difficulty of redevelopment has been significantly reduced.
