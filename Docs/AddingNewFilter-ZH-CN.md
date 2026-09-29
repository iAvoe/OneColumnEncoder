# 添加新滤镜：给 Agent 的实现指南

本文以当前仓库代码为准，说明如何在 OneColumnEncoder 的 Filter Scribe 中添加一个可插入的滤镜。目标是让 Agent 先判断滤镜属于哪条现有路径，再只修改必要文件。

## 0. 先记住当前架构

项目目前**没有**一个对所有滤镜通用的 `Strategy -> Converter -> ViewModel` 框架：

- 色彩空间转换是特例，使用 `Models/Analysis/ColorSpaceAnalysisM.cs`、`FFmpeg/ColorSpaceConverter.cs` 和 `FilterScribeVM` 的专用路径。
- 旋转、翻转、裁切、缩放、去色带、降噪、字幕烧录等普通功能，大多直接在 `ViewModels/FilterScribeVM.cs` 中生成字符串。
- UI 位于 `Views/FilterScribeModal.xaml`，通过 `CommandParameter` 把生成的字符串传给 VM 中的插入命令。
- `Views/FilterScribeModal.xaml.cs` 只处理窗口行为、文本框右键菜单、FFmpeg 单行输入等，不负责滤镜生成。
- `FilterScribe` 中的滤镜不是持久化设置。AVS/VPY 会在确认时保存为脚本；FFmpeg 文本通过回调应用到当前会话。

因此，**普通新滤镜通常不需要新增 Model、enum 或 `ColorSpaceConverter` 分支**。只有滤镜需要根据源媒体元数据分类，或者确实属于色彩空间转换时，才进入后面的专项路径。

## 1. 代码地图与调用链

| 责任 | 文件 | 说明 |
| --- | --- | --- |
| 打开编辑器 | `Commands/OpenClose/OpenFilterScribeCmd.cs` | 收集源路径、ffprobe JSON、工具路径和回调，创建 `FilterScribeVM` |
| 滤镜状态、生成和插入 | `ViewModels/FilterScribeVM.cs` | 三个后端的字符串、可用性、参数、链合并、预览和确认 |
| UI 布局与绑定 | `Views/FilterScribeModal.xaml` | AVS、VapourSynth、FFmpeg 三个 Tab，以及滤镜区域 |
| UI 事件 | `Views/FilterScribeModal.xaml.cs` | 只处理窗口和输入框行为 |
| 文本资源 | `Models/Lang/FilterScribeModalLangProvider.cs` | `SrcScribe.*` 键；窗口标题保持英文常量 |
| FFmpeg 通用参数 | `FFmpeg/FfmpegFilterArgs.cs` | 将多个滤镜合成 `-filter:v`，可附加色彩、像素格式和 swscale 参数 |
| 色彩空间分析和转换 | `FFmpeg/ColorSpaceConverter.cs` | 读取 ffprobe 的色彩元数据，分类并构造色彩空间滤镜 |
| 像素格式规则 | `FFmpeg/FFProbePixelFormatRules.cs` | 位深、YUV 采样类型、YUV 输出格式等规则 |
| 源分析结果 | `Models/Analysis/VideoAnalysisM.cs` | 保存代表性 ffprobe JSON；队列/拼接/重分集使用代表源驱动 Filter Scribe |
| 脚本模板 | `ScriptGeneration/ScriptTemplate.cs` | 生成 AVS/VPY 源头、预览和导出脚本 |
| 脚本保存 | `Persistence/FilterScribeScriptPersistence.cs` | 同时写出 `.avs` 和 `.vpy` |
| 插件路径 | `Persistence/BundledToolPathResolver.cs` | 从配置目录解析插件文件夹，找不到时回退到程序目录 |

运行链路如下：

```text
MainVM / OpenFilterScribeCmd
    -> FilterScribeVM(source ffprobe JSON, callbacks)
    -> ParseColorSpaceInfo / ParseSourceResolution / ParseFrameRateInfo
    -> FilterScribeModal.xaml bindings
    -> InsertAvsFilterCommand / InsertVpyFilterCommand / InsertFFmpegFilterCommand
    -> AvsUserInput / VpyUserInput / FFmpegFreeText
    -> ApplyFFmpegFilterArgs 或 ScriptTemplate + 脚本保存
```

主界面的检查项状态改变时，`MainVM` 会调用 `FilterScribeVM.RefreshGeneratedFFmpegFilters()` 刷新生成结果。源分析数据在 `FilterScribeVM` 构造时解析；依赖元数据的滤镜必须加入对应的属性通知，否则 UI 可能仍显示旧字符串。

## 2. 开始编码前必须作出的判断

先明确以下结果，不要直接复制色彩空间代码：

1. 支持哪些后端：FFmpeg、VapourSynth、AviSynth。三者不必强行同时支持。
2. 滤镜输出是完整 FFmpeg 参数，还是只有 filter chain；AVS/VS 是否需要多行加载插件和前后处理。
3. 是否依赖源的 `pix_fmt`、位深、分辨率、帧率、色彩元数据、SAR 或 progressive 状态。
4. 是否有用户参数；参数是本次会话临时值，还是需要保存到 `AppConfM` 等持久化模型。
5. 是否需要第三方 DLL，DLL 是否有 x86 和 x64 版本，脚本是否必须显式加载。
6. 滤镜插入顺序是否有要求，尤其是 FFmpeg `libplacebo` 合并逻辑和 native filter 插入位置。

建议先写一张后端矩阵：

```text
滤镜：NewFilter
FFmpeg：-filter:v "newfilter=..."，是否需要额外 -pix_fmt？
VapourSynth：src = core.xxx.Filter(src, ...)，是否需要 LoadPlugin？
AviSynth：NewFilter(...)，是否需要 LoadPlugin？
可用条件：无 / pix_fmt / bit depth / color metadata / resolution / user input
插入顺序：普通追加 / 必须在 libplacebo 前 / 必须在某一步之后
```

## 3. 普通滤镜的最小实现路径

适用于不需要色彩空间分类的滤镜，例如一个固定参数的后处理滤镜。

### 3.1 在 `FilterScribeVM` 增加生成属性

普通滤镜的构造位置是 `ViewModels/FilterScribeVM.cs`。现有写法可以按后端分别提供：

```csharp
public static string AviSynthNewFilter => "NewFilter(src, strength=1.0)";
public static string VapourSynthNewFilter =>
    "src = core.newplugin.Filter(src, strength=1.0)";
public static string FFmpegNewFilter =>
    "-filter:v \"newfilter=strength=1.0\"";

public static string AviSynthNewFilterDisplay => AviSynthNewFilter;
public static string VapourSynthNewFilterDisplay => VapourSynthNewFilter;
public static string FFmpegNewFilterDisplay => "newfilter=strength=1.0";

public static bool CanInsertAviSynthNewFilter => true;
public static bool CanInsertVapourSynthNewFilter => true;
public static bool CanInsertFFmpegNewFilter => true;
```

实际属性应根据输入条件使用实例属性，而不是无条件返回 `true`。显示文本和插入参数可以是同一个属性，也可以像 FFmpeg 现有实现一样分成：

- 完整命令：例如 `FFmpegResizeFilter`，用于插入。
- 只显示 chain：例如 `FFmpegResizeFilterDisplay`，用于只读 TextBox。

无效时返回 `null` 或 `LangProviderBase.NAText`。现有的 `IsUsableColorSpaceFilter` 和各 `CanInsert...` 属性会据此禁用按钮。

### 3.2 参数改变时刷新所有相关属性

如果滤镜的输出由滑块、文本框或源分析决定，setter 中必须触发相应的 `OnPropertyChanged`。至少要覆盖：

- 插入属性，例如 `FFmpegNewFilter`；
- 显示属性，例如 `FFmpegNewFilterDisplay`；
- 可插入属性，例如 `CanInsertFFmpegNewFilter`；
- 依赖该滤镜的组合示例或其他链属性。

如果依赖源 ffprobe 数据，在 `ParseColorSpaceInfo`、`ParseSourceResolution` 或 `ParseFrameRateInfo` 中加入刷新；如果依赖检查项状态，在 `RefreshGeneratedFFmpegFilters()` 中加入刷新。不要只修改 getter 而忘记通知 UI。

### 3.3 使用现有插入命令

构造函数中现有命令定义如下：

```csharp
InsertAvsFilterCommand = new ActionCmd(
    filter => AppendScriptFilter(ref _avsUserInput, filter as string, nameof(AvsUserInput)));
InsertVpyFilterCommand = new ActionCmd(
    filter => AppendScriptFilter(ref _vpyUserInput, filter as string, nameof(VpyUserInput)));
InsertFFmpegFilterCommand = new ActionCmd(
    filter => AppendFFmpegFilter(filter as string));
InsertFFmpegFlipFilterCommand = new ActionCmd(
    filter => AppendFFmpegNativeFilter(filter as string));
```

通常直接复用这些命令，不要为一个普通滤镜新建 Command。

AVS/VS 的 `AppendScriptFilter`：

- 忽略空字符串和含 `N/A` 的字符串；
- 多次点击时按换行追加到 `AvsUserInput` 或 `VpyUserInput`；
- AVS 普通调用不需要 `src =`；
- VapourSynth 通常需要将结果重新赋给 `src`。

FFmpeg 的 `AppendFFmpegFilter`：

- 接受 `-filter:v` 或 `-vf`，也能处理不能拆分的原始文本；
- 普通 video filter 会被解析为 chain 并用逗号合并；
- `-filter_complex` 会以空格追加，不能当作普通 video chain；
- 含 `libplacebo` 的新命令会尝试合并到现有 `libplacebo` 参数中；
- 后缀参数会尽量保留。

只有确定新滤镜必须位于现有 `libplacebo` 之前时，才考虑 `InsertFFmpegFlipFilterCommand` 对应的 `AppendFFmpegNativeFilter`。该命令是目前翻转功能的特殊顺序处理，不是所有滤镜的默认入口。

### 3.4 在 XAML 增加一个 UI 区域

在 `Views/FilterScribeModal.xaml` 的 Filter generator `ScrollViewer` 内增加区域。最小模式是：标题、按当前 Tab 显示的插入按钮、只读生成文本。

```xml
<StackPanel Visibility="{Binding IsVpyTabSelected, Converter={StaticResource BoolToVisibility}}">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        <TextBlock Text="{Binding NewFilterTitle}"
                   Style="{StaticResource H3}" />
        <Button Grid.Column="1"
                Command="{Binding InsertVpyFilterCommand}"
                CommandParameter="{Binding VapourSynthNewFilter}"
                IsEnabled="{Binding CanInsertVapourSynthNewFilter}"
                Style="{StaticResource AppendLinkButton}" />
    </Grid>
    <TextBox Text="{Binding VapourSynthNewFilterDisplay, Mode=OneWay}"
             IsReadOnly="True"
             BorderThickness="0"
             FontFamily="{DynamicResource CodeFont}" />
</StackPanel>
```

按后端复制按钮和显示框时，必须检查：

- AVS 使用 `InsertAvsFilterCommand` 和 AVS 属性；
- VS 使用 `InsertVpyFilterCommand` 和 VS 属性；
- FFmpeg 使用 `InsertFFmpegFilterCommand` 和完整 FFmpeg 参数；
- 只支持某个后端时，对其他 Tab 不创建按钮，或明确显示 `N/A`；
- 需要条件禁用时绑定 `CanInsert...`，不要仅隐藏生成文本；
- 有参数输入时，确保输入控件不覆盖预览按钮和代码框。

现有 UI 的 `AppendLinkButton` 使用 `+` 作为插入操作；保持这个样式和当前区域的列布局。窗口高度和脚本编辑区已经固定，不要为单个滤镜随意修改窗口结构。

### 3.5 增加语言资源

需要显示给用户的标题、说明、提示和错误文字应加入 `Models/Lang/FilterScribeModalLangProvider.cs`，然后在 VM 中通过 `FilterScribeModalLangProvider.Current["SrcScribe...."]` 暴露给 XAML。

至少检查 `en`、`zh-cn`、`zh-tw` 三组字典。其他语言通常由英文字典复制得到，但不要以此为理由硬编码新英文字符串。窗口标题 `FilterScribeModalLangProvider.WindowTitle` 按项目约定保持英文。

当前 XAML 中仍有一些历史遗留的硬编码技术标签，例如 `HLG→709` 和 `DOVI HDR→709`。新增功能不要继续扩大这种做法；如果修改相关区域，优先把新增用户可见文本放进语言 Provider。

## 4. FFmpeg 专项规则

### 4.1 什么时候修改 `FfmpegFilterArgs`

`FFmpeg/FfmpegFilterArgs.cs` 的唯一公共 API 是：

```csharp
FFMpegFilterArgs.Build(
    includeSwsFlags,
    includeCsp709Flags,
    pixelFormat,
    filters);
```

它会：

1. 删除空 filter 并用逗号连接；
2. 生成 `-filter:v ...`；
3. 在 `includeCsp709Flags` 时附加 `-color_primaries bt709 -color_trc bt709 -colorspace bt709`；
4. 在同一开关下按传入 pixel format 附加 `-pix_fmt`；
5. 在 `includeSwsFlags` 时附加固定的 `-sws_flags`。

普通 FFmpeg filter 不需要修改这个文件。只有新功能需要新的公共输出参数，或者应复用这些组合规则时，才从 VM 调用它；不要为了新增一个滤镜而把专用参数塞进通用 builder。

### 4.2 FFmpeg 插入顺序

`FilterScribeVM.AppendFFmpegFilter` 会对现有 `FFmpegFreeText` 做合并。添加滤镜前要定义它与下列滤镜的顺序：

- `fps`；
- `libplacebo`，包括色彩转换、SAR 修复、去色带、放大；
- `scale`；
- native filter，例如 `hflip`/`vflip`；
- `-filter_complex`，例如字幕烧录。

不要假设“按钮点击顺序”就是最终正确顺序。若滤镜必须在特定节点前后，应该在 VM 中提供专门的合并逻辑，并为该逻辑写出可验证的输入/输出例子。

`FFmpegFreeText` 是 session-only 文本：确认时由 `_applyFFmpegFilterArgs(FFmpegFreeText.Trim())` 回传给 `MainVM`。它不是 `AppConfM`，也不会自动写入设置文件。

## 5. 依赖输入元数据的滤镜

先复用已有分析字段，不要在新滤镜中重新解析 JSON。

### 5.1 当前可用的数据

`ColorSpaceConverter.Analyze` 从 ffprobe 第一个视频流读取：

```text
color_primaries -> ColorPrimaries
color_transfer  -> ColorTransfer
color_space     -> ColorMatrix
chroma_location -> ColorChromaLocation
pix_fmt         -> PixelFormat
帧率            -> FrameRate
side_data_list  -> HasDolbyVision
```

`FilterScribeVM.ParseColorSpaceInfo` 另外读取：

- `FFProbeSrcVal.ReadBitDepthFromJson` -> `_sourceBitDepth`；
- `FFProbeSrcVal.Analyze(...).IsProgressive` -> `_sourceIsProgressive`。

分辨率在 `ParseSourceResolution` 中进入 `SourceWidth`/`SourceHeight`，VFR 信息在 `ParseFrameRateInfo` 中进入 `_frameRateNum`/`_frameRateDen`。

### 5.2 根据色彩空间分类

只有新增功能本身是色彩空间转换，或者确实需要共享色彩空间策略时，才修改：

1. `Models/Analysis/ColorSpaceAnalysisM.cs`：新增 `ColorSpaceStrategy` 值，并在 `IsApplicable` 中决定它是否是可转换策略。
2. `ColorSpaceConverter.Classify`：从 primaries、transfer、DoVi 元数据推导策略。
3. `ColorSpaceConverter.IsStrategyApplicable`：严格限制输入适用范围；不适用应返回 `false`。
4. `BuildFFmpegFilter`、`BuildVapourSynthFilter`、`BuildAviSynthFilter`：为各后端构造实际字符串；某后端不支持就返回 `null`。
5. `CreateResult`：确认策略、三个后端 filter、显示名和 description 都被填充。
6. `FilterScribeVM`：增加三个后端的显示属性、`CanInsert...` 属性、组合链和 `RefreshColorSpaceFilters` 通知。
7. `FilterScribeModal.xaml`：增加该策略的按钮和显示行。

`IsColorSpaceStrategyShown` 的现有语义很重要：它同时检查源验证状态、`IsStrategyApplicable`，以及至少有一个后端能产生可用滤镜。不要只在 XAML 中写一个永远可点击的按钮。

当前内置策略包括：

```text
NativeBt709
LowToHigh
HighToLow
HdrToSdr
HighHdrToSdr
HlgToSdr
DoviSdrTo709
DoviHdrToSdr
```

`NativeBt709` 表示无需转换，不要把它当成需要插入的 filter；`Unknown` 也不应自动生成颜色转换。

### 5.3 依赖像素格式或色度采样

使用 `FFProbePixelFormatRules.GetChromaSubsampling()`，不要只使用旧的 `GetChromaSubsamplingDepth()`。当前枚举为：

```text
Unknown / Yuv420 / Yuv422 / Yuv444 / Other
```

当前规则识别 `yuv*`、`nv12`/`nv16`、`p010`/`p016`、`p210`/`p216` 等格式，但新增格式时要检查：

- `GetChromaSubsampling`；
- `IsYuv`；
- `GetBitDepth`；
- `GetYuv420PixelFormat`；
- 如影响缩放或裁切，再检查 `GetResolutionScaleStep` 和相关规则。

需要色度位置时使用 `ColorChromaLocation`，通过 VM 中的 `NormalizeChromaLocation` 映射到后端语法。未知位置必须禁用需要显式输入位置的转换；不要用猜测值替代缺失 metadata。对于 YUV444，当前代码不会机械添加 `cplace_in`/`ChromaInPlacement`，新增功能应保持同样原则。

**当前实现的事实检查：** `FilterScribeVM` 有 `FFmpegChroma422Filter` 和 `FFmpegChroma420Filter` 两个属性，但它们最终都调用 `BuildFFmpegChromaFilter`；该 helper 当前通过 `GetYuv420PixelFormat` 得到输出格式。因此，新增或修改色度转换时不能仅凭属性名假定 FFmpeg 的 `422` 分支一定输出 YUV422，必须检查并测试实际生成的 `-pix_fmt`。如果要修复这个行为，应把“目标采样类型”明确作为 helper 参数，而不是只复制现有 bool 参数。

### 5.4 位深和前后转换

只有插件或滤镜确实要求固定位深时才增加位深包装。当前色彩空间的 VS/AVS 路径是：

```text
输入
 -> fmtconv 转 16 bit
 -> libplacebo
 -> 恢复源位深，支持 8/10/12/14/16/32，其他回落 16
```

对应实现是 `GetVapourSynthColorSpaceFilterChain`、`GetAviSynthColorSpaceFilterChain` 和 `GetPlaceboOutputBitDepth`。普通滤镜不要复制这段逻辑，否则可能改变输出格式或增加无意义的转换。

FFmpeg 的目标像素格式另由 `FFProbePixelFormatRules.GetYuv420PixelFormat` 等规则决定，也不要把 VS/AVS 的位深策略直接套到 FFmpeg。

## 6. 用户参数和校验

如果新滤镜需要用户输入，例如峰值亮度或设备 ID：

1. 在 VM 增加字段和公开属性；setter 使用 `SetProperty`。
2. 使用 `PreviewTextInput` 和 `DataObject.Pasting` 阻止明显非法输入。
3. 增加 `Has...` 或 `Is...Valid`，将它接到 `CanInsert...`。
4. 在参数改变时刷新所有生成和显示属性。
5. 将占位符替换放在生成链的最后一步，不要在 XAML 中替换滤镜字符串。
6. 如果只是本次滤镜会话需要，保持 session-only；只有用户明确需要跨会话保存时，才修改 `AppConfM`、对应 persistence 和设置 UI。

现有 `ColorSpacePeakNits` 是参考实现：输入绑定到 `SettingsListing`，用 invariant culture 解析，生成字符串中的 `<nits>` 由 `ReplaceColorSpacePeakNits` 替换。`SettingsListing` 不是通用配置注册表；当前 VM 是手动创建 `AppConfItem` 和 `TextBox`。新增参数时沿用这个模式即可，不要假设存在通用滤镜设置 API。

## 7. 第三方插件和 DLL

### 7.1 判断是否真的需要新增 DLL

如果使用已经随程序提供且脚本已经加载的插件，不要重复加载或重复打包。当前插件文件位于：

```text
cicd/1cenc/x64-AVS-VS-plugins/
cicd/1cenc/x86-AVS-VS-plugins/
```

来源记录在 `cicd/1cenc/Plugin-URLs.txt`。当前代码使用 `BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")` 或按进程位数选择 x86/x64 目录；不要硬编码某台机器的绝对路径。

### 7.2 在脚本中加载插件

当前项目没有统一的“插件注册表”。实际模式有两种：

- 在生成的滤镜字符串前直接加入 `core.std.LoadPlugin(...)` 或 `LoadPlugin(...)`，例如 ASharp、fmtconv 和 VS-zipcl。
- 为可独立插入的加载命令提供 VM 属性，例如 `VapourSynthPlaceboLoadCommand`、`AviSynthPlaceboLoadCommand`，再在 XAML 放一个插入按钮。

VapourSynth 和 AviSynth 的加载语法不同，插件函数名称和 DLL 位数也可能不同。必须分别验证 AVS/VS 脚本；FFmpeg 内置 filter 不需要 DLL 加载。

如果新增 DLL：

1. 确认许可证和来源，更新 `Plugin-URLs.txt`。
2. 按实际支持情况放入 x86、x64 或两者目录。
3. 使用 `BundledToolPathResolver` 解析路径。
4. 在脚本生成结果中保证加载发生在首次调用插件之前。
5. 检查发布/打包流程是否会带上新文件；`.csproj` 当前不会自动把 `cicd` 下的 DLL 作为 WPF 资源嵌入。

## 8. 预览、保存和模式差异

### 8.1 预览

AVS/VS 预览使用 `ScriptTemplate.BuildAvsPreviewScript` 和 `BuildVpyPreviewScript`，内容来自 `AvsUserInput`/`VpyUserInput`。如果滤镜依赖：

- 插件加载顺序；
- `src` 必须重新赋值；
- 位深转换；
- 特殊输出节点；

就必须通过 A/B Preview 实际运行，而不能只检查文本框字符串。FFmpeg 预览从 `FFmpegFreeText` 提取 video filter 并调用 `ffmpeg`。

### 8.2 保存和导入

普通滤镜无需修改保存层：

- 单源模式用 `ScriptTemplate.BuildAvsExportScript` / `BuildVpyExportScript`；
- 拼接模式用对应的 `BuildConcat...ExportScript`；
- `FilterScribeScriptPersistence.TryWriteScripts` 同时写出 AVS 和 VPY；
- `MainVM` 的 `_afterImport` 回调负责写回源卡片。

除非新增功能改变脚本头、脚本尾或拼接源结构，否则不要修改 `ScriptTemplate`。

### 8.3 Queue、Concat、Repart

Filter Scribe 的代表性源 JSON来自 `VideoAnalysisM.RawJson`。队列、拼接和重分集会用代表源/第一段源驱动生成器；这意味着依赖源格式的滤镜不能假装每个片段都相同。若滤镜要求多源逐个检查，需要修改对应 route 的分析或验证流程，而不是只改 Filter Scribe UI。

## 9. 建议的修改顺序

按下面顺序实施，便于 Agent 在每一步编译和定位问题：

1. 先决定后端矩阵、输入条件、输出格式和插入顺序。
2. 先在 `FilterScribeVM` 写出静态字符串或 helper，并定义 `CanInsert...`。
3. 若依赖 metadata，再接入已有分析字段和刷新通知。
4. 若是色彩空间策略，按第 5.2 节完整修改 enum、分类、三个 builder、VM 和 UI。
5. 若需要插件，添加路径解析和脚本加载，并更新 DLL 来源/打包文件。
6. 在 XAML 增加按钮、`CommandParameter`、`IsEnabled` 和只读显示框。
7. 在 `FilterScribeModalLangProvider` 增加用户可见文字。
8. 运行构建，再用三种后端实际打开 Filter Scribe 和预览。

## 10. 验证清单

仓库当前没有独立的 `Tests` 测试项目，因此新增滤镜至少要做以下手工验证；若未来加入测试项目，应优先把纯字符串构造和元数据分类抽成可测方法。

### 静态检查

- [ ] 所有 XAML Binding 名称都对应 VM 中的 public property/command。
- [ ] 所有动态属性在输入或源分析改变时收到 `OnPropertyChanged`。
- [ ] 无效条件返回 `null`/`N/A`，并且按钮被禁用。
- [ ] AVS 使用正确的调用语法，VS 保持 `src`，FFmpeg 使用正确的完整参数形式。
- [ ] 没有为了普通滤镜无条件修改 `ColorSpaceStrategy` 或 `ColorSpaceConverter`。
- [ ] 新增用户可见文字没有硬编码在 VM 或 XAML 中。

### 运行时

- [ ] 无源分析时按钮状态正确，不能因为默认值生成可执行滤镜。
- [ ] 正常输入能生成预期字符串。
- [ ] 不支持的后端显示 `N/A` 或没有可用按钮。
- [ ] 缺失或未知 metadata 时不会猜测关键参数。
- [ ] 连续点击两次不会丢失已有链，也不会错误地重复插件加载。
- [ ] 与 `fps`、SAR 修复、`libplacebo`、`scale`、native filter 和 `-filter_complex` 组合时顺序正确。
- [ ] AVS 预览能运行，VPY 预览能运行，FFmpeg 预览能运行。
- [ ] 依赖 DLL 时，加载命令在首次调用前，并且 x86/x64 路径正确。
- [ ] 确认后单源、Queue/Concat/Repart 的保存或应用行为没有回归。
- [ ] 语言切换后新标题、提示和 N/A 文本正常。

### 构建

在仓库根目录执行：

```powershell
dotnet build OneColumnEncoder.sln --configuration Debug
```

项目是 `net9.0-windows` WPF，构建环境需要 Windows 和可用的 .NET 9 SDK。构建成功只能证明 C#/XAML 能编译，不能替代后端脚本和实际视频验证。

## 11. 最小变更模板

对一个无 metadata 依赖、只支持 FFmpeg 的普通滤镜，通常只需：

```text
ViewModels/FilterScribeVM.cs
  1. NewFilter 属性
  2. NewFilterDisplay 属性（必要时）
  3. CanInsertNewFilter 属性（必要时）

Views/FilterScribeModal.xaml
  4. 标题、插入按钮、只读显示框

Models/Lang/FilterScribeModalLangProvider.cs
  5. 标题/提示语言键

验证
  6. dotnet build
  7. FFmpeg filter scribe 插入、预览和确认
```

如果是 AVS/VS 普通滤镜，增加相应的两个后端属性和按钮；如果支持用户参数、插件或输入格式判断，再分别从第 5、6、7 节增加分支。不要把这些专项步骤扩展成所有滤镜的固定要求。
