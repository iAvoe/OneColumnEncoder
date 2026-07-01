# Progress bar detection flow

1. **`StartEncCmd.Execute()`** builds the `EncodingPipelineRequest` and `EncodingPipelineCommand`, then opens the `EncodingMonitorModal`.
2. **`EncodingMonitorModal.OnLoaded()`** calls **`EncodingMonitorVM.Start()`**, which starts a `DispatcherTimer` (500ms tick) and launches the async encoding pipeline (`RunEncodingAsync`).
3. **`RunEncodingAsync`** spawns upstream (e.g., ffmpeg/vspipe) and downstream (encoder) processes, pipes stdout from upstream to encoder stdin, and concurrently reads **stderr from both processes** via `ReadStreamAsync`.
4. **`ReadStreamAsync`** processes stderr character-by-character, splits on `\r`/`\n`, and enqueues lines to a `ConcurrentQueue<ProcessLogEntry>`.
5. **`ProcessQueuedLogs`** (called by the timer) dequeues lines, calls **`AppendLogWithOverwrite`**, which calls **`UpdateProgressFromLogLine`**.
6. **`UpdateProgressFromLogLine`** uses `IsProgressLine` + `InferProgress` (percentage regex) and `TryParseEncoderFrame` (6 frame-extraction regexes) to update `ProgressValue` and `_writtenFrames`.
7. **`UpdateProgressDetails`** (called every 1 second by the timer) updates the current output file size.
8. **`UpdateFooterTimes`** estimates remaining time via linear extrapolation: `total = elapsed / (progress/100)`.
