# Progress bar detection flow

The monitor no longer fabricates `nb_frames` or guesses a total frame count from `duration × avg_frame_rate` when source metadata does not already provide one. That fallback could make the displayed progress look reliable while still being derived from incomplete data, and it could also feed guessed counts into frame-limited encoder arguments. When the total frame count is unknown, the progress UI stays unavailable instead of trying to show a possibly wrong percentage.

1. **`StartEncCmd.Execute()`** builds the `EncodingPipelineRequest` and `EncodingPipelineCommand`, then opens the `EncodingMonitorModal`.
2. **`EncodingMonitorModal.OnLoaded()`** calls **`EncodingMonitorVM.Start()`**, which starts a `DispatcherTimer` (500ms tick) and launches the async encoding pipeline (`RunEncodingAsync`).
3. **`RunEncodingAsync`** spawns upstream (e.g., ffmpeg/vspipe) and downstream (encoder) processes, pipes stdout from upstream to encoder stdin, and concurrently reads **stderr from both processes** via `ReadStreamAsync`.
4. **`ReadStreamAsync`** processes stderr character-by-character, splits on `\r`/`\n`, and enqueues lines to a `ConcurrentQueue<ProcessLogEntry>`.
5. **`ProcessQueuedLogs`** (called by the timer) dequeues lines, calls **`AppendLogWithOverwrite`**, which calls **`UpdateProgressFromLogLine`**.
6. **`UpdateProgressFromLogLine`** only runs when a reliable total frame count is available. It uses `IsProgressLine` + `InferProgress` (percentage regex) and `TryParseEncoderFrame` (6 frame-extraction regexes) to update `ProgressValue` and `_writtenFrames`.
7. **`UpdateProgressDetails`** (called every 1 second by the timer) updates the current output file size.
8. **`UpdateFooterTimes`** estimates remaining time via linear extrapolation only when progress tracking is available: `total = elapsed / (progress/100)`.
