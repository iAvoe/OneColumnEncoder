# OneColumnEncoder

[中文版](README.md)

A next-gen smart video encoding utility based on .NET 9/WPF, revolves around "tool & encoder orchestrating, source analyzing, encode customizing, parallelism tailoring, modern GUI monitoring, encode interrupting, and auto-multiplexing."

The current code works but lacks testing, which is releasing as beta.

<p align="center"><img src="WireframeMockups/logo.png" alt="Logo" width="200"></p>

## Gallery

This software supports multiple languages, but English text screenshots are used here to reduce the number of images. Some UI elements or text in the images may be outdated, but the overall layout and functional area divisions remain applicable. Please refer to the actual version you are using.

1. Main Interface: Tools area, Source Import area, Analysis area, Checklist, Encoding Settings area, and Startup area
2. Script Editor: AVS/VPY editing area, video scaling and VFR to CFR command generation
3. Encoding Settings: CRF/ABR parameters, custom presets, and 3rd party encoder arguments
4. Parallelism Settings: NUMA nodes, CPU Sets, and advanced thread limit
5. Clip Sampler: Time/Frame Number selection, conversion, and basic preview
6. Encoding Monitoring: Logs, progress, resource usage, interrupt control, as well as auto-multiplexing
7. Warning Modal and File Overwrite Protection

<p align="center"><img src="WireframeMockups/1-Main-Page.png" alt="Main Window" width="600"><br>
<img src="WireframeMockups/2-Script-Scribe.png" alt="Script Scribe Window" width="500"><br>
<img src="WireframeMockups/3-Encoder-Setting.png" alt="Encoder Setting Window" width="350"><br>
<img src="WireframeMockups/4-Parallelism-Setting.png" alt="Parallelism Setting Window" width="400"><br>
<img src="WireframeMockups/5-Clip-Sampler.png" alt="Clip Sampler Window" width="400"><br>
<img src="WireframeMockups/6-Encoding-Monitor.png" alt="Encoding Monitor Window" width="650"><br>
<img src="WireframeMockups/7-Warning-Modal-OW-Guard.png" alt="Warning Modal & Overwrite Protection" width="350"><br></p>

## System Requirements

- Windows 10/11 x64
  - Recommended version: 1809/21H2 (LTSC) or higher; minimum: 1607
- .NET 9 Desktop Runtime
  - Download: [Microsoft Official Website](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

<p align="center"><img src="WireframeMockups/Actual-Binary-Link-Position.png" alt="Actual link is on the right side" width="600"></p>

**Supported pipe upstream programs (decoding and filtering tools)**:
- ffmpeg
- vspipe (supports API 3.0 and 4.0 automatic recognition)
- avs2yuv
- avs2pipemod
- SVFI

**Supported pipe downstream programs (encoders)**:
- x264
- x265
- SVT-AV1

> Minimum requirement is one upstream program + one downstream program.

## Icon usage

- Azure icons: [azureicons.com](https://www.azureicons.com)
- Game icon pack by NiewBie: [GitHub/Niewbie](https://github.com/Nieobie/Game-Icon-Pack)

---

## Validation Status

**OS**：
- All tests are currently validated on Windows 10 22H2
- Lacking validation for Windows 11, but it should work fine...

**Hardware**：
- Core i5 7600X (4C4T)
- Ryzen 9 9900X (2CCD 12C24T)
- EPYC 7R13 (6CCD 48C96T)
- Lacking validation for Intel 12th\~14th generation and Ultra 200\~300 series heterogeneous CPUs

## Localization Status

- **Supported:** English, Simplified Chinese, Traditional Chinese
- To provide a translation, please fork this repository, add a new language entrie in `Models/XxxLangProviderM`, and submit a pull request
  - Translation of the README is not required, but it would be great if you can do that

---

## Support me

Its not esay to develop these tools. If this software helped, please consider sponsoring or promoting it.

<p align="center"><img src="WireframeMockups/bmc_qr.png" alt="Support me -_-"><br><img src="WireframeMockups/pp_tip_qr.png" alt="Pls support =_="></p>

## Project Status

The content below organizes the implementation status based on the current project status, marking the completion status of major and sub-modules, classified as:

- Done: Implemented and has been integrated into the main workflow or current UI.
- Unverified: Complete implementation exists, but has not yet been actually tested due to environment or external service limitations.
- Incomplete: UI, models, or some logic exist, but the behavior is incomplete, or some configurations have not yet been consumed.
- Not Started: Only placeholders, manifests, fields, or old code exist; currently, there is no actual functionality or it is not integrated into the main workflow.

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
- The source inspection card has parsed and displayed progressive, bit depth, frame rate, SAR, color metadata, chroma, and other inspection items
- Viewing source inspection issues, refreshing the checklist status, and manually bypassing them have been integrated into the main workflow

#### Pre-coding Inspection

- The pre-coding inspection card has implemented hardware and software inspection items
- Includes checks on AC power, output directory, disk space, write permissions, output file overwrite, and AviSynth / L-SMASH
- Viewing pre-encoding issues, re-evaluating, and manually bypassing them have been integrated into the start button status

#### Encoding Parameter Configuration

- Encoding presets, keyframe intervals, and some third-party parameter switches have been implemented and persisted
- The encoding settings card displays a summary of the current encoding parameters
- The encoding pipeline generates corresponding encoder parameters based on the current configuration

#### Encoding Command Generation and Startup

- Y4M pipeline command generation, supporting output from various upstream tools to x264/x265/SVT-AV1
- Command generation automatically supplements parameters such as frame number, color, range, chroma, and lookahead based on ffprobe information
- A parameter confirmation window pops up before starting encoding; after confirmation, the encoding monitoring window appears

#### Sampling Clips

- Supports selecting clips by time & frame number, and supports timestamp-to-frame-number conversion
- Sampling segments will open the encoding monitoring process in sample mode
- SVFI / OneLineShotArgs currently do not support sampling segments; a disabled message is displayed on the main interface

#### Encoding Monitoring and Process Execution

- Supports starting upstream and encoder processes and passing upstream stdout pipes to encoder stdin
- Supports reading upstream/downstream stderr, log folding, saving logs, viewing encoding commands, and adjusting log fontsize
- Supports encoding progress, number of written frames, current/estimated output size, time elapsed, remaining time, and completion time estimation
- Supports memory usage, working set peak, Page Fault, memory pressure, and memory range statistics
- Supports interrupting upstream or encoder processes; the window can only be closed after encoding is complete

#### Parallelism Basic Capabilities

- NUMA node enumeration, CPU topology reading, CPU Sets allocation, and encoder thread limit are already implemented
- Parallel settings can be saved and applied to the upstream/encoder processes started by encoding monitoring
  - The encoder thread count is passed to the x264/x265 parameter; SVT-AV1 currently does not generate thread parameters

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

- `EncoderConfM.CustomParams` will be saved and appended to the final encoding command by `EncodingPipelineH`
- The "Custom Parameters" area is no longer a summary of third-party switches, but instead directly reads and writes a free text parameter
- The parameter coverage for x264/x265/STV-AV1 is still limited, but it is usable

#### Scriptwriter

- The scriptwriter window, AVS/VPY editing area, copying complete scripts, and copying input/output fragments
- "Save As" is implemented
- "Confirm" has implemented script saving and backfilling
  - To simplify code, it saves both AVS & VPY scripts simultaneously, same as the one-click generate button

#### Main Interface Best Practices Checklist

- `BestPracsSelfCheckCardVM` is a self-check reference card and does not participate in the start-encoding blocking conditions
- No `RunAllChecks()`, no `IsBypassed`, no Inspect/Bypass buttons
- Marked as "Advisory — not blocking" on the UI

### Application Settings → File Overwrite

The Overwrite setting will append an overwrite confirmation pop-up if the output file already exists after displaying and confirming the compression command, and delay enabling the confirmation button according to the size of the overwritten file

---

## Unverified

### Intel 12~14th Gen & Ultra 200~300 Series CPU Utilization Verification

No CPU available for testing, but it should not fail catastrophically
- This software uses CPU Sets to bind encoding processes to physical cores, which means it should be compatible and not fail catastrophically

---

## Not Started

None currently

---

## Dead Ends

### P-Core / E-Core Optimization

This feature cannot be implemented due to the need to modify the upstream program and encoder source code
- P-Core / E-Core related checkboxes in the UI are disabled
- This is really CPU manufacturers' task

### Large Pages Implementation

This feature cannot be implemented due to the need to modify the upstream program and encoder source code

---

## Main Source Code Locations

- `Commands/`: User operation commands, modal window opening and closing, save loading, and encoding startup entry point
- `Helpers/`: Encoding pipeline, ffprobe analysis, tool detection, script templates, filename validation, CPU/NUMA/permissions, and other auxiliary logic
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

- Confirm encoding commands before starting encode, and file overwriting：`Commands/StartEncCmd.cs`
- Sample clip confirmation before starting encode：`ViewModels/SampleClipVM.cs`
- View encoding commands in the encoding monitor：`ViewModels/EncodingMonitorVM.cs`
- Copy/save results after script generation：`ViewModels/ScriptScribeVM.cs`、`Commands/SaveLoad/OneClickScriptGenCmd.cs`
- Source analysis and check results：`Commands/AnalyzeSrcVideoCmd.cs`、`Commands/CopyRawAnalysisCmd.cs`、`Commands/InspectEncProblemsCmd.cs`、`Commands/InspectSrcProblemsCmd.cs`
- Secondary confirmation when importing tools/selecting files：`Commands/ImportToolCmd.cs`、`Helpers/SourceFilePickerH.cs`
