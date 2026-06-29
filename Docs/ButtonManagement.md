# Button Management

This document describes how to create, style, and wire up buttons in the OneColumnEncoder WPF application.

---

## Table of Contents

1. [Button Styles (Types)](#1-button-styles-types)
2. [Button Text](#2-button-text)
3. [Button Commands](#3-button-commands)
4. [Button Icons (SVG)](#4-button-icons-svg)
5. [Button Group Components](#5-button-group-components)
6. [Standalone Button Patterns](#6-standalone-button-patterns)
7. [ViewModel Wiring Examples](#7-viewmodel-wiring-examples)

---

## 1. Button Styles (Types)

All button styles are defined in `App.xaml` (based on `BaseButton` in `Views/Themes/Generic.xaml`). Five styles form a visual hierarchy:

| Style Key          | Default BG          | Default FG   | Hover BG → FG      | Usage                            |
|--------------------|---------------------|--------------|--------------------|----------------------------------|
| `NormalButton`     | `Background` (white)| `ButtonPrimary`| `ButtonPrimary` → `Background` | Secondary actions, left-aligned |
| `PrimaryButton`    | `ButtonPrimary`     | `Background` | `Background` → `ButtonPrimary` | Primary/confirm actions         |
| `SecondaryButton`  | `GlobalSecondary`   | `Background` | `Background` → `GlobalSecondary` | Secondary actions, center      |
| `TertiaryButton`   | `GlobalTertiary`    | `Background` | `Background` → `GlobalTertiary` | Tertiary actions (e.g. 5-btn groups) |
| `QuaternaryButton` | `GlobalQuaternary`  | `Background` | `Background` → `GlobalQuaternary` | Quaternary actions             |

Additionally, `LinkButton` is a borderless text-only button (blue underline on focus, red on cancel).

```xml
<!-- Example: applying a style -->
<Button Style="{StaticResource PrimaryButton}" />
```

**Default button height:** 40 (from `BaseButton`). Override via `Height` property.

---

## 2. Button Text

### 2.1 Static Text

```xml
<Button Content="Click Me" />
```

### 2.2 Data-Bound Text

Bind the `Content` property to a ViewModel string:

```xml
<Button Content="{Binding MyButtonText}" />
```

### 2.3 Text with Icon (StackPanel Content)

When a button needs both an icon and text, use explicit `StackPanel` content instead of the `Content` attribute:

```xml
<Button Command="{Binding MyCommand}"
        Style="{StaticResource PrimaryButton}">
    <StackPanel Orientation="Horizontal"
                VerticalAlignment="Center"
                HorizontalAlignment="Center">
        <!-- Icon Rectangle goes here (see §4) -->
        <TextBlock Text="{Binding MyButtonText}"
                   VerticalAlignment="Center"
                   Margin="5 0 0 0" />
    </StackPanel>
</Button>
```

---

## 3. Button Commands

### 3.1 Command from ViewModel

Bind `Command` to an `ICommand` property on the `DataContext`:

```xml
<Button Command="{Binding MyCommand}" />
```

### 3.2 Command from Ancestor (Button Groups)

In reusable button group components, commands are exposed as `DependencyProperty`s and bound with `RelativeSource`:

```xml
<Button Command="{Binding Button1Command, RelativeSource={RelativeSource AncestorType=UserControl}}" />
```

### 3.3 Creating Commands in ViewModels

Use the `ActionCmd` helper (a simple `ICommand` implementation):

```csharp
MyCommand = new ActionCmd(_ => ExecuteAction());
// With can-execute:
MyCommand = new ActionCmd(_ => ExecuteAction(), _ => CanExecuteAction());
```

### 3.4 Disabling Buttons

```xml
<!-- Direct IsEnabled binding -->
<Button IsEnabled="{Binding IsMyButtonEnabled}" />

<!-- Via button group component -->
<comps:TwoButtonGroup Button1IsEnabled="{Binding ...}" />
```

---

## 4. Button Icons (SVG)

SVG icons are provided by the `SvgIconProviderH` static class (`Helpers/SvgIconProviderH.cs`), which exposes `ImageSource` properties (e.g., `GamePlay`, `GameImport`, `GameSave`, `GameRefresh`, etc.).

### 4.1 Available Icons

| Property          | Description          |
|-------------------|----------------------|
| `GamePlay`        | Play triangle        |
| `GameImport`      | Download arrow       |
| `GameReplace`     | Replace/refresh      |
| `GameSave`        | Save/floppy          |
| `GameSetting`     | Gear/settings        |
| `GameRefresh`     | Refresh arrows       |
| `GamePaste`       | Clipboard            |
| `GameLocation`    | Map pin              |
| `GamePhone`       | Phone                |
| `GameDelete`      | Trash                |
| `GameInfo`        | Info circle          |
| `GameWaiting`     | Hourglass            |
| `GameWarning`     | Warning (two dots)   |
| `GameXMark`       | X/close              |
| `GameCorrectMark` | Checkmark            |
| `GlobeWarning`    | Globe with warning   |
| `GlobeError`      | Globe with error     |
| `GlobeSuccess`    | Globe with checkmark |
| `Troubleshoot`    | Wrench               |
| `AzureConsortium` | Blocks               |
| `AzureSearch`     | Magnifier            |

### 4.2 Icon Pattern (x:Static)

Used when the icon is fixed in XAML (not data-bound). Requires `xmlns:helpers` namespace:

```xml
<UserControl xmlns:helpers="clr-namespace:OneColumnEncoder.Helpers" ...>
```

```xml
<Rectangle Width="14"
           Height="14"
           VerticalAlignment="Center"
           Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}">
    <Rectangle.OpacityMask>
        <ImageBrush ImageSource="{x:Static helpers:SvgIconProviderH.GamePlay}"
                    Stretch="Uniform"
                    AlignmentX="Center"
                    AlignmentY="Center" />
    </Rectangle.OpacityMask>
</Rectangle>
```

The `Fill` binding to the Button's `Foreground` makes the icon color follow the button state automatically (including hover color changes).

### 4.3 Icon Pattern (Data-Bound)

Used in button group components where the icon comes from a `ButtonGroupVM` property:

```xml
<Rectangle Width="14"
           Height="14"
           Margin="0 0 4 0"
           VerticalAlignment="Center"
           Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}">
    <Rectangle.OpacityMask>
        <ImageBrush ImageSource="{Binding B2_1Icon}"
                    Stretch="Uniform" />
    </Rectangle.OpacityMask>
    <Rectangle.Style>
        <Style TargetType="Rectangle">
            <Style.Triggers>
                <DataTrigger Binding="{Binding B2_1Icon}"
                             Value="{x:Null}">
                    <Setter Property="Visibility" Value="Collapsed" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Rectangle.Style>
</Rectangle>
```

The `DataTrigger` hides the icon Rectangle when the bound `ImageSource` is `null`, allowing icon-only or text-only configurations.

### 4.4 Setting Icons from ViewModel

```csharp
myButtonGroup.B2_1Icon = SvgIconProviderH.GamePaste;
myButtonGroup.B3_3Icon = SvgIconProviderH.GamePlay;
```

---

## 5. Button Group Components

Three reusable components reduce boilerplate for common button layouts.

### 5.1 Component Overview

| Component        | File                         | Buttons | Styles Applied                     |
|------------------|------------------------------|---------|-------------------------------------|
| `TwoButtonGroup`   | `Components/TwoButtonGroup.xaml` | 2     | B1: `NormalButton`, B2: `PrimaryButton` |
| `ThreeButtonGroup` | `Components/ThreeButtonGroup.xaml` | 3 | B1: `NormalButton`, B2: `SecondaryButton`, B3: `PrimaryButton` |
| `FiveButtonGroup`  | `Components/FiveButtonGroup.xaml` | 5 | B1: `NormalButton` (icon), B2: `TertiaryButton`, B3: `SecondaryButton`, B4: `QuaternaryButton`, B5: `PrimaryButton` (icon) |

### 5.2 Dependency Properties

Each component exposes:

| Property             | Type        | Default | Description          |
|----------------------|-------------|---------|----------------------|
| `ButtonNCommand`     | `ICommand`  | `null`  | Command for button N |
| `ButtonNIsEnabled`   | `bool`      | `true`  | IsEnabled for button N |
| `ButtonHeight`       | `double`    | `40.0`  | Height of all buttons |

### 5.3 ViewModel Properties (ButtonGroupVM)

The `ButtonGroupVM` class (`ViewModels/ButtonGroupVM.cs`) provides text, icon, and command properties keyed by button count:

| Prefix | Text Properties        | Icon Properties      | Cmd Properties | IsEnabled Properties    |
|--------|------------------------|----------------------|----------------|-------------------------|
| B1_1   | `B1_1Text`             | —                    | `Cmd1`         | `B1_1IsEnabled`         |
| B2_*   | `B2_1Text`, `B2_2Text` | `B2_1Icon`, `B2_2Icon` | `Cmd1`, `Cmd2` | `B2_1IsEnabled`, `B2_2IsEnabled` |
| B3_*   | `B3_1Text`..`B3_3Text` | `B3_1Icon`..`B3_3Icon` | `Cmd1`..`Cmd3` | `B3_1IsEnabled`..`B3_3IsEnabled` |
| B5_*   | `B5_1Text`..`B5_5Text` | `B5_1Icon`, `B5_5Icon` (ends only) | `Cmd1`..`Cmd5` | `B5_1IsEnabled`..`B5_5IsEnabled` |

### 5.4 Factory Methods

```csharp
// Single button
ButtonGroupVM.CreatePrimaryButton(string text, ICommand? cmd = null)

// Two buttons
ButtonGroupVM.CreateTwoButton(string b1Text, string b2Text,
    ICommand? cmd1 = null, ICommand? cmd2 = null)

// Three buttons
ButtonGroupVM.CreateThreeButton(string b1Text, string b2Text, string b3Text,
    ICommand? cmd1 = null, ICommand? cmd2 = null, ICommand? cmd3 = null)

// Five buttons
ButtonGroupVM.CreateFiveButton(string b1Text, string b2Text, string b3Text,
    string b4Text, string b5Text,
    ICommand? cmd1 = null, ICommand? cmd2 = null, ICommand? cmd3 = null,
    ICommand? cmd4 = null, ICommand? cmd5 = null)
```

### 5.5 Usage in XAML

```xml
<!-- Bind DataContext to a ButtonGroupVM property, then map commands -->
<comps:ThreeButtonGroup Grid.Column="4"
                        DataContext="{Binding ZoomPresetButtons}"
                        ButtonHeight="30"
                        Width="330"
                        Button1Command="{Binding Cmd1}"
                        Button2Command="{Binding Cmd2}"
                        Button3Command="{Binding Cmd3}" />
```

Commands from the ViewModel are mapped via `Cmd1`/`Cmd2`/etc. to the component's dependency properties (`Button1Command`/`Button2Command`/etc.).

### 5.6 ButtonWidthConverter

Button group components use `ButtonWidthConverter` (`Converters/ButtonWidthConverter.cs`) to distribute width evenly:

```xml
Width="{Binding RelativeSource={RelativeSource AncestorType=UserControl},
                Path=ActualWidth,
                Converter={StaticResource ButtonWidthConverter},
                ConverterParameter='3,5'}"
```

Parameter format: `"{buttonCount},{gapSpacing}"` — e.g., `"3,5"` means 3 buttons with 5px gaps between them.

---

## 6. Standalone Button Patterns

### 6.1 Text-Only Button

```xml
<Button Content="{Binding MyText}"
        Command="{Binding MyCommand}"
        Height="30"
        Style="{StaticResource PrimaryButton}" />
```

### 6.2 Icon-Only Button

```xml
<Button Command="{Binding MyCommand}"
        Width="30" Height="30"
        Style="{StaticResource NormalButton}">
    <Rectangle Width="14" Height="14"
               Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}">
        <Rectangle.OpacityMask>
            <ImageBrush ImageSource="{x:Static helpers:SvgIconProviderH.GamePlay}"
                        Stretch="Uniform" />
        </Rectangle.OpacityMask>
    </Rectangle>
</Button>
```

### 6.3 Icon + Text Button (Most Common)

```xml
<Button Grid.Column="3"
        Command="{Binding PreviewCommand}"
        Height="30"
        HorizontalAlignment="Stretch"
        Style="{StaticResource PrimaryButton}"
        Background="Black">
    <StackPanel Orientation="Horizontal"
                VerticalAlignment="Center"
                HorizontalAlignment="Center">
        <Rectangle Width="14" Height="14"
                   VerticalAlignment="Center"
                   Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}">
            <Rectangle.OpacityMask>
                <ImageBrush ImageSource="{x:Static helpers:SvgIconProviderH.GamePlay}"
                            Stretch="Uniform" />
            </Rectangle.OpacityMask>
        </Rectangle>
        <TextBlock Text="{Binding PreviewButtonText}"
                   VerticalAlignment="Center"
                   Margin="5 0 0 0" />
    </StackPanel>
</Button>
```

Key points:
- Remove the `Content` attribute when using explicit inner content.
- Center both icon and text via `StackPanel` alignment.
- Bind `Rectangle.Fill` to `Button.Foreground` for automatic color matching.
- Namespace `xmlns:helpers="clr-namespace:OneColumnEncoder.Helpers"` must be declared on the root element.

### 6.4 Button with Click Event (Code-Behind)

```xml
<Button Content="--"
        Width="30" Height="30"
        Style="{StaticResource NormalButton}"
        Click="ZoomFineOut_Click" />
```

Use `Click` handlers sparingly; prefer command binding.

---

## 7. ViewModel Wiring Examples

### 7.1 Two Button Group with Icons

```csharp
// MainVM.cs
OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
    UICaptionProviderM.Buttons.UsageAndCompliance,
    UICaptionProviderM.Buttons.Settings,
    OpenUsages,          // ICommand
    OpenAppConf);        // ICommand
OpenAppConfButtons.B2_1Icon = SvgIconProviderH.GamePhone;
OpenAppConfButtons.B2_2Icon = SvgIconProviderH.GameSetting;
```

### 7.2 Three Button Group with Icons

```csharp
// MainVM.cs
EncStartButtons = ButtonGroupVM.CreateThreeButton(
    UICaptionProviderM.Buttons.ReEvaluate,
    UICaptionProviderM.Buttons.RunSample,
    UICaptionProviderM.Buttons.StartEncode,
    new ActionCmd(_ => ReEvaluateAllChecks()),
    SampleClip,    // ICommand
    StartEncode);  // ICommand
EncStartButtons.B3_1Icon = SvgIconProviderH.GameRefresh;
EncStartButtons.B3_2Icon = SvgIconProviderH.GameLocation;
EncStartButtons.B3_3Icon = SvgIconProviderH.GamePlay;
```

### 7.3 Standalone Button with Dynamic Text

```csharp
// ImgABPvVM.cs
public bool IsBusy
{
    set
    {
        PreviewButtonText = value
            ? Lang.CancelButtonText
            : Lang.PreviewButtonText;
    }
}

// XAML binds PreviewButtonText and PreviewCommand
```

### 7.4 Five Button Group with Icons

```csharp
ScriptExportButtons = ButtonGroupVM.CreateFiveButton(
    UILangProviderM.Current["SrcScribe.CopyFull"],
    UILangProviderM.Current["SrcScribe.CopyInOut"],
    UILangProviderM.Current["SrcScribe.SaveAsFile"],
    /* ... */);
ScriptExportButtons.B3_3Icon = SvgIconProviderH.GameSave;
```

---

## Quick Reference: Adding a New Button

1. **Choose a style:** `PrimaryButton`, `SecondaryButton`, `NormalButton`, etc.
2. **Set text:** Use `Content` attribute or `<StackPanel>` + `<TextBlock>`.
3. **Wire command:** Bind `Command` to an `ICommand` property.
4. **Add icon (optional):** Use `Rectangle` + `OpacityMask` + `ImageBrush` pattern.
5. **Add `xmlns:helpers`** if using `{x:Static helpers:SvgIconProviderH...}`.
6. **Create ViewModel property** for text/icon/command as needed.
