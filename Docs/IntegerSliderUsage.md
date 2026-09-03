# IntegerSlider Usage

This document covers the reusable `IntegerSlider` control used across the app for integer-valued ranges.

## What it is

`IntegerSlider` is a WPF `UserControl` that pairs a label, a custom thumb, a floating value popup, and optional tick labels. It is used when the UI needs an integer range with visual position feedback, not a plain `Slider`.

## Main usages

`IntegerSlider` is used in these places:

- `Components/EncoderPreviewPanel.xaml` for preview position selection
- `Components/VpyPreviewPanel.xaml` for VapourSynth preview position selection
- `Components/EncoderRateControlPanel.xaml` for CRF and ABR controls
- `Components/EncoderCustomParamsPanel.xaml` for keyframe interval controls
- `Components/EncodingMonitor.xaml` for sample interval control
- `Views/SampleClipModal.xaml` for clip duration
- `Views/ParallelismConfModal.xaml` for thread count
- `Views/FilterScribeModal.xaml` for crop and scale values

## Basic binding pattern

```xml
<comps:IntegerSlider Grid.Column="1"
                     Minimum="0"
                     Maximum="{Binding MaxValue}"
                     Value="{Binding CurrentValue, Mode=TwoWay}"
                     TickLabels="{Binding Labels}"
                     TickCount="5"
                     LabelWidth="0"
                     HorizontalAlignment="Stretch" />
```

## Important properties

- `Minimum` and `Maximum` define the integer range.
- `Value` is the bound selection.
- `TickLabels` supplies the displayed labels under the track.
- `TickCount` keeps the track layout stable when labels are present.
- `Step` snaps movement to a fixed integer increment.
- `SnapToTicks` keeps the selected value aligned to ticks.
- `IsLogarithmic` switches the position math to a logarithmic scale.

## Preview panel behavior

In `ImgABPvVM`, the preview slider is initialized from source ffprobe data. When source stats are available, `PreviewPositionSeconds` starts near the middle of the available range; otherwise it starts at `0`.

## Implementation notes

- The thumb position is calculated from `Minimum`, `Maximum`, and `Value`.
- The popup and tick labels use the same range math, so they stay aligned with the thumb.
- `Maximum` and `TickCount` changes re-clamp the current value.
