# Purged Designs

This document lists features that have been cancelled. Maybe salvaged in future, or never.

## "Do You Know" Banner Reel During Encoding

Originally planned for EncodingMonitor as a banner that rotates video encoding facts every few seconds.

It was cancelled for several reasons: to maximize free RAM and VRAM, minimize UI updates, and keep the UI straightforward, facts maybe outdated or wrong, etc.

## Export AviSynth/VapourSynth Script

This originally existed in FilterScribe (the video filter editor) as a TwoButtonGroup/ThreeButtonGroup below the filter textbox. The design intention was to let users save another copy of the AviSynth/VapourSynth script somewhere else.

It was removed because it broke the up→down UI flow (the Cancel/Save ThreeButtonGroup at the bottom of the editor), and user testing revealed that it caused confusion when "Export script" and "Save script" coexisted.

## Descale Filter

This filter used to be placed under a switch tab that served as an alternative to the resize filter, within teh FilterScribe.

It was removed because the infrastructure to detect which upscale filter and what amount of magnification were originally used for the source video was lacking. Without that, the descale filter essentially became the "worst possible resize filter"——it always needs a prefected parameter input.

Which upscale filter and magnification were used can be found by an exhaustive search, using VapourSynth, OpenCV, pyzo, or a dedicated computer-vision AI trained to determine the values. Perhaps 1cenc can eventually have this capability in the future.

For now, a simple resize filter is enough.

## IVTC

Implementing this requires new UIs, such as a "FrameWover" that allows users to see all combinations of deinterlace-pulldown attempts (which can roughly be categorized by type and phase) and then pick the right one. Multiple UI elements would need to be invented and tuned, many changes to the codebase would be required, and a great deal of testing would be needed.

Implementing an IVTC system would also require disabling most features of 1cenc. Encoding would become lossless only, and video filtering would be disabled (the "Open Filter Scribe" button would be swapped for an "Open Frame Wover" button). Essentially, it would be a different mode.

If this system is eventually released, it should result a major version number change.