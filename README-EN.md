# OneColumnEncoder

[中文版](.\README.md)

OneColumnEncoder is a next-gen video encoding auxiliary tool based on .NET 9/WPF. The current project works but lacks testing, and the main workflow revolves around "importing upstream tools and encoders, importing video or script sources, analyzing source video, generating encoding commands, and starting and monitoring the encoding process."

This document organizes the implementation status based on the current code structure, marking the completion status of major and sub-modules. The classifications are as follows:

- Completed: Actual implementation exists and has been integrated into the main workflow or current UI.
- Unverified: Complete implementation exists, but has not yet been actually tested due to environment or external service limitations.
- Incomplete: UI, models, or some logic exist, but the behavior is incomplete, or some configurations have not yet been consumed.
- Not Started: Only placeholders, manifests, fields, or old code exist; currently, there is no actual functionality or it is not integrated into the main workflow.

## System Requirements

- Windows 10/11 x64
- .NET 9 Desktop Runtime
  - Download Link: [Microsoft Official Website](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

---

## Completed

### Application Framework and Main Interface

- WPF app startup, main window, and main interface layout have been implemented, with entry points in `App.xaml.cs`, `MainWindow.xaml`, and `Views/MainUI.xaml`.
- `MainVM` is responsible for the main interface module orchestration, including the tool area, source import area, analysis area, checklist, coding settings area, and startup area.
- Modal window navigation, masking states, close commands, and basic command models have been implemented.
- Multi-language switching mechanisms have been integrated into the main interface, cards, buttons, and modal window text.

### Tool Import and Selection

- Importing, replacing, deleting, and selecting external tools.
- Defined & supported upstream tools include `ffmpeg.exe`, `vspipe.exe`, `avs2yuv.exe`, `avs2pipemod.exe`, and `one_line_shot_args.exe`.
- Defined & supported encoders include `x264.exe`, `x265.exe`, and `svtav1encapp.exe`.
- Defined & supported analytics and dependencies include `ffprobe.exe` and `avisynth.dll`.
- Tool version detection, filename verification, tool partitioning, default selection, and dependency/source compatibility refresh are implemented.

### Source Import and Script Generation

- Importing regular video sources, AviSynth scripts, VapourSynth scripts, and SVFI ini sources.
- Supports persistent source file paths and refilling existing source files upon startup.
- One-click generation of AviSynth/VapourSynth scripts is implemented, writing `.avs` and `.vpy` files and refilling them into the script source card.
- Linkage between source type and upstream tool selection has been implemented. For example, `vspipe.exe` corresponds to `.vpy`, and `avs2yuv.exe` / `avs2pipemod.exe` corresponds to `.avs`.

### ffprobe Source Analysis and Inspection

- JSON analysis of `ffprobe` has been implemented, and the original analysis results can be copied.
- The source inspection card has parsed and displayed progressive, bit depth, frame rate, SAR, color metadata, chroma, and other inspection items.
- Viewing source inspection issues, refreshing the checklist status, and manually bypassing them have been integrated into the main workflow.

### Pre-coding Inspection

- The pre-coding inspection card has implemented hardware and software inspection items.
- Includes checks on AC power, output directory, disk space, write permissions, output file overwrite, and AviSynth / L-SMASH.
- Viewing pre-coding issues, re-evaluating them, and manually bypassing them have been integrated into the start button status.

### Encoding Parameter Configuration

- Basic CRF/ABR parameter configuration for x264/x265/STVT-AV1 has been implemented.
- Encoding presets, keyframe intervals, and some third-party parameter switches have been implemented and persisted.
- The encoding settings card displays a summary of the current encoding parameters.
- The encoding pipeline generates corresponding encoder parameters based on the current configuration.

### Encoding Command Generation and Startup

- Y4M pipeline command generation has been implemented, supporting output from various upstream tools to x264/x265/STVT-AV1.
- Command generation automatically supplements parameters such as frame number, color, range, chroma, and lookahead based on ffprobe information.
- A command confirmation window pops up before starting encoding; after confirmation, the encoding monitoring window appears.

### Sampling Clips

- The sampling clip modal window has been implemented.
- Supports selecting clips by time or frame number, and supports time-to-frame-number conversion.
- Sampling segments will open the encoding monitoring process in sample mode.
- SVFI / OneLineShotArgs currently do not support sampling segments; a disabled message is displayed on the main interface.

### Encoding Monitoring and Process Execution

- The encoding monitoring window now supports actual process startup.
- Supports starting upstream and encoder processes and passing upstream stdout pipes to encoder stdin.
- Supports reading upstream/downstream stderr, log folding, saving logs, viewing encoding commands, and adjusting log font size.
- Supports encoding progress, number of written frames, current/estimated output size, time elapsed, remaining time, and completion time estimation.
- Supports memory usage, working set peak, Page Fault, memory pressure, and memory range statistics.
- Supports interrupting upstream or encoder processes; the window can only be closed after encoding is complete.

### Parallel Basic Capabilities

- NUMA node enumeration, CPU topology reading, CPU Sets allocation, and encoder thread limit are already implemented.
- Parallel settings can be saved and applied to the upstream/encoder processes started by encoding monitoring. - The encoder thread count is passed to the x264/x265 parameter; SVT-AV1 currently does not generate thread parameters.

### Application Configuration and Persistence

- The basic logic for saving/loading JSON for application configuration, tool path, source path, encoding parameters, and parallel parameters has been implemented.
- SMTP configuration, test emails, encoding completion/failure notifications, and user idle time thresholds have been implemented.
- Language configuration has been integrated for saving and loading.

### Output Filename Tool

- The output filename/directory setting modal window has been implemented.
- Supports path preview, clipboard, illegal characters, reserved names, and length checks.
- Output settings cards will be populated upon confirmation.

### UI Components

- Cards, button groups, dropdown menus, settings containers, checklists, integer sliders, fragment range selectors, memory range bars, and column text components have been implemented.
- The currently used one-way UI converter meets existing binding requirements.

### Encoding Parameter Configuration Details

- `EncoderConfM.CustomParams` will be saved and appended to the final encoding command by `EncodingPipelineH`.
- The "Custom Parameters" area is no longer a summary of third-party switches, but instead directly reads and writes a free text parameter.
- The parameter coverage for x264/x265/STV-AV1 is still limited, but it is usable.

### Scriptwriter

- The scriptwriter window, AVS/VPY editing area, copying complete scripts, and copying input/output fragments have been implemented.
- "Save As" is implemented.
- "Confirm" has implemented script saving and backfilling (to simplify the logic, it saves both AVS and VPY scripts simultaneously, similar to the one-click generate button).

### Main Interface Best Practices Checklist

- `BestPracsSelfCheckCardVM` is a self-check reference card and does not participate in the start-encoding blocking conditions
- No `RunAllChecks()`, no `IsBypassed`, no Inspect/Bypass buttons.
- Marked as "for reference only" on the UI via the `Subtitle` attribute ("Advisory — not blocking").
- It is an independent design and not an incomplete blocking checklist.

### Application Settings → File Overwrite

- The Overwrite setting will append an overwrite confirmation pop-up if the output file already exists after displaying and confirming the compression command, and delay enabling the confirmation button according to the size of the overwritten file.

---

## Unverified

### SMTP Settings

- SMTP Configuration, test emails, encoding completion/failure notifications, and user idle detection logic have all been implemented, but an SMTP server has not yet been set up for actual testing.

---

## Not Started

None currently

---
## Dead End

### P-Core / E-Core Optimization

- P-Core / E-Core related checkboxes in the UI are disabled.
- Since this implementation only works after modifying the encoder source code, and considering the maintenance cost of third-party modified encoders
- This requires users modify the source code of encoders and compile it themselves to work, which's really what CISC & RISC CPU manufacturers' task
- Further evaluation is needed on whether to retain the switches and text in the UI, or to remove the relevant UI elements to avoid misleading users.
- Language resources are still marked as `TODO`.
- Saved fields exist, but there is no actual scheduling or parameter generation logic.

### Large Pages Implementation

- This feature cannot be implemented due to the need to modify the upstream program and encoder source code.

---

## Main Source Code Locations

- `Commands/`: User operation commands, modal window opening and closing, save loading, and encoding startup entry point

- `Helpers/`: Encoding pipeline, ffprobe analysis, tool detection, script templates, filename validation, CPU/NUMA/permissions/SMTP, and other auxiliary logic

- `Models/`: Configuration models, tool definitions, language resources, checklists, and data DTOs

- `ViewModels/`: Main interface, modal window, and card state management

- `Views/`: WPF windows and interface XAML

- `Components/`: Reusable UI controls

- `Converters/`: WPF binding converters

### Testing and Engineering

- No unit test, integration test, or automated UI test projects are currently observed.

- The README does not yet include instructions for building, running, preparing dependencies, and typical workflows (although these are provided in the usage instructions window / AppUsageModal).