using OneColumnEncoder.Models.Analysis;
using OneColumnEncoder.ScriptGeneration;
using System.IO;

namespace OneColumnEncoder.ViewModels;

/// <summary>
/// Note:
/// Users must manually copy/enter the desired filters into the free text box in the ffmpeg tab to be accepted.
/// 
/// File save & ItemCard write back logic created by MainVM as OnSrcImported,
/// passed in via OpenFilterScribeCmd constructor as Action<>
/// </summary>
public class FilterScribeVM : BaseVM
{
    private readonly ModalNavS _modalNavS;
    private readonly Func<string> _getsrcPath;
    private readonly Action _closeAction;
    private readonly ToolItemCardVM _avsItem;
    private readonly ToolItemCardVM _vpyItem;
    private readonly Func<SrcFileKind?> _getPreferredScriptSrcKind;
    private readonly Func<string?> _getSelectedUpstreamExeName;
    private readonly Action<ToolItemCardVM, SrcFileKind, string> _afterImport;
    private readonly Action<string?> _applyFFmpegFilterArgs;
    private readonly Func<SrcRevisionRequest, string?> _sourceReviser;
    private readonly string _sourceFfprobeJson;
    private readonly Func<bool> _hasSourceValidationError;
    private readonly Func<bool> _hasSarRepairWarning;
    private readonly Func<bool>? _isQueueRoute;
    private readonly Func<string[]>? _getQueueFilePaths;
    private readonly Func<bool>? _isConcatRoute;
    private readonly Func<string[]>? _getConcatFilePaths;
    private readonly Func<bool>? _isRepartRoute;
    private readonly Action<string?, string?>? _applyScriptFilters;
    private readonly string? _vspipePath;
    private readonly string? _vspipeY4mArg;
    private readonly Func<long>? _getTotalFrames;
    private const int DisplayConcatPathMaxLength = 90;
    private ColorSpaceAnalysisM _colorSpaceAnalysis = ColorSpaceConverter.Analyze(null);
    private int _sourceBitDepth;
    private bool _hasSourceAnalysis;
    private bool _sourceIsProgressive = true;
    public CloseModalCmd CloseCmd { get; }
    public bool IsConcatMode => _isConcatRoute?.Invoke() == true || IsRepartMode;
    public bool IsRepartMode => _isRepartRoute?.Invoke() == true;
    public bool HasSourceAnalysis => _hasSourceAnalysis;
    // 0: AVS, 1: VS, 2: ffmpeg
    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value))
            {
                OnPropertyChanged(nameof(IsAvsTabSelected));
                OnPropertyChanged(nameof(IsVpyTabSelected));
                OnPropertyChanged(nameof(IsFFmpegTabSelected));
                OnPropertyChanged(nameof(IsDenoiseSectionVisible));
            }
        }
    }
    public bool IsAvsTabSelected => _selectedTabIndex == 0;
    public bool IsVpyTabSelected => _selectedTabIndex == 1;
    public bool IsFFmpegTabSelected => _selectedTabIndex == 2;
    public bool IsDenoiseSectionVisible => IsFFmpegTabSelected || IsAvsTabSelected;

    // Avs/VpyPrefix becomes instance property to support dynamic fpsnum/fpsden
    // Avs/VpyPrefix2 is a guidance comment to keep
    #region Script text
    private string _baseAvsPrefix;
    public string AvsPrefix
    {
        get
        {
            if (IsConcatMode)
            {
                int fpsnum = _isFrameRateVariable && _avsEnableFpsParams ? _frameRateNum : 0;
                int fpsden = _isFrameRateVariable && _avsEnableFpsParams ? _frameRateDen : 0;
                string[] paths = GetDisplayConcatFilePaths();
                return paths.Length > 1
                    ? ScriptTemplate.BuildConcatAvsSourceHeader(paths, fpsnum, fpsden)
                    : string.Empty;
            }
            if (_isFrameRateVariable && _avsEnableFpsParams && _frameRateNum > 0 && _frameRateDen > 0)
                return $"LWLibavVideoSource(\"video file path\", fpsnum={_frameRateNum}, fpsden={_frameRateDen})";
            return _baseAvsPrefix;
        }
    }
    public static string AvsPrefix2 => FilterScribeModalLangProvider.Current["SrcScribe.AvsPrefix2"];
    private string _avsUserInput = "";
    public string AvsUserInput
    {
        get => _avsUserInput;
        set => SetProperty(ref _avsUserInput, value);
    }
    public static string AvsSuffix => FilterScribeModalLangProvider.Current["SrcScribe.AvsSuffix"];

    private string _baseVpyPrefix;
    public string VpyPrefix
    {
        get
        {
            if (IsConcatMode)
            {
                int fpsnum = _isFrameRateVariable && _vpyEnableFpsParams ? _frameRateNum : 0;
                int fpsden = _isFrameRateVariable && _vpyEnableFpsParams ? _frameRateDen : 0;
                string[] paths = GetDisplayConcatFilePaths();
                return paths.Length > 1
                    ? ScriptTemplate.BuildConcatVpySourceHeader(paths, fpsnum, fpsden)
                    : string.Empty;
            }
            if (_isFrameRateVariable && _vpyEnableFpsParams && _frameRateNum > 0 && _frameRateDen > 0)
                return $"import vapoursynth as vs\r\ncore = vs.core\r\nsrc = core.lsmas.LWLibavSource(source=r\"video file path\", fpsnum={_frameRateNum}, fpsden={_frameRateDen})";
            return _baseVpyPrefix;
        }
    }
    private string _vpyUserInput = "";
    public string VpyUserInput
    {
        get => _vpyUserInput;
        set => SetProperty(ref _vpyUserInput, value);
    }
    public static string VpyPrefix2 => FilterScribeModalLangProvider.Current["SrcScribe.VpyPrefix2"];
    public static string VpySuffix => FilterScribeModalLangProvider.Current["SrcScribe.VpySuffix"];
    public static FilterScribeModalLangProvider Lang => FilterScribeModalLangProvider.Current;
    #endregion

    #region Resolution scaling
    public bool HasSource => SourceWidth > 0 && SourceHeight > 0;

    private int _sourceWidth;
    public int SourceWidth
    {
        get => _sourceWidth;
        set
        {
            if (SetProperty(ref _sourceWidth, value))
            {
                OnPropertyChanged(nameof(HasSource));
                OnPropertyChanged(nameof(IsScaleApplicable));
                OnPropertyChanged(nameof(ScaleHeightMaximum));
                OnPropertyChanged(nameof(ScaleStep));
                OnPropertyChanged(nameof(ScaleTickLabels));
                RecomputeCrop();
                OnPropertyChanged(nameof(AviSynthAssRenderFilter));
                RecomputeTarget();
            }
        }
    }

    private int _sourceHeight;
    public int SourceHeight
    {
        get => _sourceHeight;
        set
        {
            if (SetProperty(ref _sourceHeight, value))
            {
                _scaleHeight = ResolutionScale.MaximumTargetHeight(value);
                OnPropertyChanged(nameof(ScaleHeight));
                OnPropertyChanged(nameof(HasSource));
                OnPropertyChanged(nameof(IsScaleApplicable));
                OnPropertyChanged(nameof(ScaleHeightMaximum));
                OnPropertyChanged(nameof(ScaleStep));
                OnPropertyChanged(nameof(ScaleTickLabels));
                RecomputeCrop();
                OnPropertyChanged(nameof(AviSynthAssRenderFilter));
                RecomputeTarget();
            }
        }
    }

    private int ScaleSourceWidth => HasCropFilter ? CropWidth : SourceWidth;
    private int ScaleSourceHeight => HasCropFilter ? CropHeight : SourceHeight;

    public bool IsScaleApplicable =>
        HasSource && ResolutionScale.IsScaleApplicable(ScaleSourceWidth, ScaleSourceHeight);

    public bool IsCropSectionVisible => HasSource;

    private int _cropWidth;
    public int CropWidth
    {
        get => _cropWidth;
        set
        {
            // int mod = CropWidthStep;
            int clamped = Math.Clamp(value, CropWidthMinimum, CropWidthMaximum);
            if (SetProperty(ref _cropWidth, clamped))
            {
                OnPropertyChanged(nameof(CropTargetDisplay));
                OnPropertyChanged(nameof(HasCropFilter));
                OnPropertyChanged(nameof(FFmpegCropFilter));
                OnPropertyChanged(nameof(VapourSynthCropFilter));
                OnPropertyChanged(nameof(AviSynthCropFilter));
                OnPropertyChanged(nameof(CanInsertAviSynthCropFilter));
                OnPropertyChanged(nameof(CanInsertVapourSynthCropFilter));
                OnPropertyChanged(nameof(CanInsertFFmpegCropFilter));
                RefreshScaleForCropChange();
            }
        }
    }

    private int _cropHeight;
    public int CropHeight
    {
        get => _cropHeight;
        set
        {
            int clamped = Math.Clamp(value, CropHeightMinimum, CropHeightMaximum);
            if (SetProperty(ref _cropHeight, clamped))
            {
                OnPropertyChanged(nameof(CropTargetDisplay));
                OnPropertyChanged(nameof(HasCropFilter));
                OnPropertyChanged(nameof(FFmpegCropFilter));
                OnPropertyChanged(nameof(VapourSynthCropFilter));
                OnPropertyChanged(nameof(AviSynthCropFilter));
                OnPropertyChanged(nameof(CanInsertAviSynthCropFilter));
                OnPropertyChanged(nameof(CanInsertVapourSynthCropFilter));
                OnPropertyChanged(nameof(CanInsertFFmpegCropFilter));
                RefreshScaleForCropChange();
            }
        }
    }

    public static int CropWidthMinimum => 120; // Arbitrary but small enough, if user needs lower, just edit manually in textbox
    public int CropWidthMaximum => HasSource ? SourceWidth : CropWidthMinimum;
    public int CropWidthStep => CropCalculator.GetWidthMod(_colorSpaceAnalysis.PixelFormat);
    public List<string> CropWidthTickLabels =>
        GenerateCropTickLabels(CropWidthMinimum, CropWidthMaximum, 5);

    public static int CropHeightMinimum => 120;  // Arbitrary but small enough
    public int CropHeightMaximum => HasSource ? SourceHeight : CropHeightMinimum;
    public int CropHeightStep => CropCalculator.GetHeightMod(_colorSpaceAnalysis.PixelFormat, _sourceIsProgressive);
    public List<string> CropHeightTickLabels =>
        GenerateCropTickLabels(CropHeightMinimum, CropHeightMaximum, 5);

    public bool HasCropFilter => HasSource && (_cropWidth != SourceWidth || _cropHeight != SourceHeight);

    public string CropTargetDisplay => !HasSource ? "--" : $"{CropWidth}x{CropHeight}";

    public string ScaleNotApplicableText =>
        !HasSource
            ? FilterScribeModalLangProvider.Current["SrcScribe.NoVidSrcWarning"]
            : string.Format(FilterScribeModalLangProvider.Current["SrcScribe.ScaleNotApplicable"], 16);

    private int _scaleHeight;
    public int ScaleHeight
    {
        get => _scaleHeight;
        set
        {
            int clamped = Math.Clamp(value, ScaleHeightMinimum, ScaleHeightMaximum);
            if (SetProperty(ref _scaleHeight, clamped)) RecomputeTarget();
        }
    }

    public static int ScaleHeightMinimum => ResolutionScale.MinimumTargetHeight;
    public int ScaleHeightMaximum => HasSource
        ? ResolutionScale.MaximumTargetHeight(ScaleSourceHeight)
        : ResolutionScale.MinimumTargetHeight;

    public int ScaleStep => FFProbePixelFormatRules.GetResolutionScaleStep(_colorSpaceAnalysis.PixelFormat);

    public void CommitScale()
    {
        if (!IsScaleApplicable) return;
        // var w, h are discard values now
        RecomputeTarget();
        OnPropertyChanged(nameof(TargetDisplay));
        OnPropertyChanged(nameof(FFmpegResizeFilter));
        OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(FFmpegHqdn3dFullChainFilter));
        OnPropertyChanged(nameof(VapourSynthResizeFilter));
        OnPropertyChanged(nameof(AviSynthResizeFilter));
    }

    private int _targetWidth;
    public int TargetWidth => _targetWidth;

    private int _targetHeight;
    public int TargetHeight => _targetHeight;

    public string TargetDisplay => !HasSource ? "--" : $"{TargetWidth}x{TargetHeight}";

    private FFProbeAspectRatio _sourceAspectRatio = FFProbeAspectRatioResolver.Resolve((string?)null);
    private string SourceDar => _sourceAspectRatio.Dar.ToString();
    private string SourceSar => _sourceAspectRatio.Sar.ToString();

    private bool HasScaleFilter => IsScaleApplicable && (TargetWidth != ScaleSourceWidth || TargetHeight != ScaleSourceHeight);

    private bool HasFpsFilter => IsFrameRateApplicable;

    private bool HasSarRepairFilter => _hasSarRepairWarning();

    private bool HasColorSpaceFilter =>
        !_hasSourceValidationError()
        && _colorSpaceAnalysis.IsApplicable
        && !RequiresManualColorSpacePeakNits
        && !string.IsNullOrWhiteSpace(_colorSpaceAnalysis.FFmpegColorFilter);

    private bool RequiresManualColorSpacePeakNits =>
        _colorSpaceAnalysis.Strategy is ColorSpaceStrategy.HdrToSdr or ColorSpaceStrategy.HighHdrToSdr;

    private string? ScaleFilterChain => HasScaleFilter ? $"scale={TargetWidth}:{TargetHeight}" : null;

    private string? FpsFilterChain => HasFpsFilter ? $"fps={_frameRateNum}/{_frameRateDen}" : null;

    private string? SarRepairFilterChain => HasSarRepairFilter ? "libplacebo=reset_sar=1" : null;

    private string? ColorSpaceFilterChain => HasColorSpaceFilter ? _colorSpaceAnalysis.FFmpegColorFilter : null;

    private string? CropFilterChain => HasCropFilter ? $"crop={CropWidth}:{CropHeight}:0:0" : null;

    private bool IsColorSpaceStrategyShown(ColorSpaceStrategy strategy) =>
        !_hasSourceValidationError()
        && ColorSpaceConverter.IsStrategyApplicable(strategy, _colorSpaceAnalysis.ColorPrimaries, _colorSpaceAnalysis.ColorTransfer)
        && !string.IsNullOrWhiteSpace(BuildColorSpaceStrategyFilterChain(strategy));

    public string FFmpegResizeFilter =>
        HasScaleFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: true, includeCsp709Flags: false, ScaleFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegCropFilter =>
        HasCropFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: true, includeCsp709Flags: false, CropFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegFpsFilter =>
        HasFpsFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: false, FpsFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegSarRepairFilter =>
        HasSarRepairFilter
            ? "-filter:v \"libplacebo=reset_sar=1\""
            : LangProviderBase.NAText;

    public static string AviSynthHqdn3dDenoiseFilter => "hqdn3d(src)";
    public static string FFmpegHqdn3dDenoiseFilter => "-filter:v \"hqdn3d\"";
    public string AviSynthAssRenderFilter =>
        "SupTitle(src, \"x:\\path\\to\\DVD_BDMV.sup\", forcedOnly=false)\r\n" +
        "assrender(src, \"x:\\path\\to\\subtitle.ass\", scale=1.0, frame_width=" +
        FormatAviSynthAssRenderDimension(SourceWidth, "width") +
        ", frame_height=" +
        FormatAviSynthAssRenderDimension(SourceHeight, "height") +
        ", dar=" + SourceDar +
        ", sar=" + SourceSar +
        ")";
    public static string VapourSynthSubtitleFilter =>
        "src = core.sub.ImageFile(src, file=r\"X:\\path\\to\\DVD_BDMV.sup\", gray=False)\r\n" +
        "src = core.sub.TextFile(src, file=r\"X:\\path\\to\\subtitle.ass\", fontdir=r\"Y:\\dir\\of\\fonts\")";
    public string VapourSynthVszipclFilter
    {
        get
        {
            if (!LibImportProviderM.IsOpenCLAvailable) return $"{LangProviderBase.NAText} (!OpenCL)";

            // isSrcYuvRGBOrGray
            if (!FFProbePixelFormatRules.IsYuvRgbOrGray(_colorSpaceAnalysis.PixelFormat))
                return $"{LangProviderBase.NAText} (!YUV/RGB/Gray Colorspace)";

            // vszipcl only supports 8 (int), 16 (int, half), 32 (float)
            int targetBpp = _sourceBitDepth switch
            {
                8 or 16 or 32 => 0,
                > 0 and < 8 => 8,
                > 8 and < 16 => 16,
                > 16 => 32,
                _ => 0
            };

            string pluginsDirectory = BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins");
            string vszipclDllPath = Path.Combine(pluginsDirectory, "vszipcl.dll");
            string fmtconvDllPath = Path.Combine(pluginsDirectory, "fmtconv.dll");

            string loadPlugins = $"core.std.LoadPlugin(r\"{vszipclDllPath}\")";
            string vszipclCalls =
                "src = core.vszipcl.Deband(src, dither_algo=0, device_id=0, num_streams=2)\r\n" +
                "src = core.vszipcl.NLMeans(src, d=1, a=2, s=4, h=1.2, wmode=0, wref=1.0, device_id=0, num_streams=2)\r\n" +
                "src = core.vszipcl.GaussBlur(src, device_id=0, num_streams=2)";
            string convIn = targetBpp == 0
                ? ""
                : $"core.std.LoadPlugin(r\"{fmtconvDllPath}\")\r\n" +
                $"src = core.fmtc.bitdepth(src, bits={targetBpp})\r\n";
            string convOut = targetBpp == 0
                ? ""
                : $"src = core.fmtc.bitdepth(src, bits={_sourceBitDepth})\r\n";

            return $"{loadPlugins}\r\n{convIn}{vszipclCalls}\r\n{convOut}".TrimEnd();
        }
    }

    public static string VapourSynthVszipclTitle => FilterScribeModalLangProvider.Current["SrcScribe.VszipclTitle"];
    public static string VapourSynthVszipclPreviewHint => FilterScribeModalLangProvider.Current["SrcScribe.VszipclPreviewHint"];
    public static string VapourSynthVszipclDeviceHint => FilterScribeModalLangProvider.Current["SrcScribe.VszipclDeviceHint"];
    public bool VapourSynthVszipclHasFmtconv => _sourceBitDepth != 8 && _sourceBitDepth != 16 && _sourceBitDepth != 32;
    public string VapourSynthVszipclFmtconvHint => string.Format(FilterScribeModalLangProvider.Current["SrcScribe.VszipclFmtconvHint"], _sourceBitDepth);
    public static string FFmpegSubtitleFilter =>
        "-filter_complex \"ass='X\\:/path/to/subtitle.ass':fontsdir='Y\\:/dir/of/fonts'\"";

    private static string FormatAviSynthAssRenderDimension(int value, string name) =>
        value > 0 ? value.ToString() : $"<ffprobe {name}>";

    public string FFmpegFpsScaleFilter =>
        HasFpsFilter && HasScaleFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: true, includeCsp709Flags: false, FpsFilterChain, ScaleFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegLowToHighColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.LowToHigh);

    public string FFmpegHighToLowColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.HighToLow);

    public string FFmpegHdrToSdrColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.HdrToSdr);

    public string FFmpegHighHdrToLowSdrColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.HighHdrToSdr);

    public string FFmpegFpsColorScaleFilter
    {
        get
        {
            string? color = ColorSpaceFilterChain;
            string? fps = FpsFilterChain;
            string? scale = ScaleFilterChain;
            if (color == null || fps == null || scale == null) return LangProviderBase.NAText;
            return BuildFFmpegFilterArgs(includeSwsFlags: scale != null, includeCsp709Flags: color != null, fps, color, scale);
        }
    }

    public string FFmpegFullChainFilter
    {
        get
        {
            string? sar = SarRepairFilterChain;
            string? color = ColorSpaceFilterChain;
            string? fps = FpsFilterChain;
            string? scale = ScaleFilterChain;
            if (sar == null || color == null || fps == null || scale == null) return LangProviderBase.NAText;
            return BuildFFmpegFilterArgs(includeSwsFlags: scale != null, includeCsp709Flags: color != null, fps, sar, color, scale);
        }
    }

    public string FFmpegHqdn3dFullChainFilter
    {
        get
        {
            string? sar = SarRepairFilterChain;
            string? color = ColorSpaceFilterChain;
            string? fps = FpsFilterChain;
            string? scale = ScaleFilterChain;
            if (sar == null || color == null || fps == null || scale == null) return LangProviderBase.NAText;
            return BuildFFmpegFilterArgs(includeSwsFlags: scale != null, includeCsp709Flags: color != null, "hqdn3d", fps, sar, color, scale);
        }
    }

    public bool CanInsertFFmpegFpsFilter => HasFpsFilter;
    public bool CanInsertFFmpegSarRepairFilter => HasSarRepairFilter;
    public bool CanInsertFFmpegResizeFilter => HasScaleFilter;
    public bool CanInsertFFmpegLowToHighColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.LowToHigh);
    public bool CanInsertFFmpegHighToLowColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.HighToLow);
    public bool CanInsertFFmpegHdrToSdrColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.HdrToSdr);
    public bool CanInsertFFmpegHighHdrToLowSdrColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.HighHdrToSdr);

    private string GetColorSpaceStrategyFilter(ColorSpaceStrategy strategy) =>
        IsColorSpaceStrategyShown(strategy)
            ? BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: true, BuildColorSpaceStrategyFilterChain(strategy))
            : LangProviderBase.NAText;

    private string? BuildColorSpaceStrategyFilterChain(ColorSpaceStrategy strategy) =>
        ColorSpaceConverter.BuildFFmpegFilter(
            strategy,
            _colorSpaceAnalysis.ColorMatrix,
            _colorSpaceAnalysis.ColorChromaLocation,
            _colorSpaceAnalysis.ColorPrimaries,
            _colorSpaceAnalysis.PixelFormat);

    private string BuildFFmpegFilterArgs(bool includeSwsFlags, bool includeCsp709Flags, params string?[] filters)
    {
        return FFMpegFilterArgs.Build(includeSwsFlags, includeCsp709Flags, _colorSpaceAnalysis.PixelFormat, filters);
    }

    public string VapourSynthResizeFilter =>
        IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight)
            ? $"src = core.resize.Bicubic(src, {TargetWidth}, {TargetHeight})"
            : LangProviderBase.NAText;

    public string VapourSynthCropFilter =>
        HasCropFilter
            ? $"src = core.std.CropAbs(src, {CropWidth}, {CropHeight})"
            : LangProviderBase.NAText;

    public string AviSynthResizeFilter =>
        IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight)
            ? $"BicubicResize({TargetWidth}, {TargetHeight})"
            : LangProviderBase.NAText;

    public string AviSynthCropFilter =>
        HasCropFilter
            ? $"Crop(0, 0, {CropWidth}, {CropHeight})"
            : LangProviderBase.NAText;

    public bool CanInsertAviSynthResizeFilter => HasScaleFilter;
    public bool CanInsertVapourSynthResizeFilter => HasScaleFilter;
    public bool CanInsertAviSynthCropFilter => HasCropFilter;
    public bool CanInsertVapourSynthCropFilter => HasCropFilter;
    public bool CanInsertFFmpegCropFilter => HasCropFilter;
    public bool CanInsertVapourSynthVszipclFilter => CanUseVapourSynthVszipcl;

    private bool CanUseVapourSynthVszipcl =>
        LibImportProviderM.IsOpenCLAvailable
        && FFProbePixelFormatRules.IsYuvRgbOrGray(_colorSpaceAnalysis.PixelFormat);

    public List<string> ScaleTickLabels =>
        ResolutionScale.GenerateHeightTickLabels(ScaleHeightMinimum, ScaleHeightMaximum, 5);

    private void RecomputeTarget()
    {
        if (!IsScaleApplicable) return;

        int targetHeight = ScaleHeight > 0 ? ScaleHeight : ScaleHeightMaximum;
        var (w, h) = ResolutionScale.ComputeTargetDimensionsFromHeight(ScaleSourceWidth, ScaleSourceHeight, targetHeight);

        if (_targetWidth != w || _targetHeight != h)
        {
            _targetWidth = w;
            _targetHeight = h;
            OnPropertyChanged(nameof(TargetWidth));
            OnPropertyChanged(nameof(TargetHeight));
            OnPropertyChanged(nameof(TargetDisplay));
            OnPropertyChanged(nameof(FFmpegResizeFilter));
            OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
            OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
            OnPropertyChanged(nameof(FFmpegFullChainFilter));
            OnPropertyChanged(nameof(FFmpegHqdn3dFullChainFilter));
            OnPropertyChanged(nameof(CanInsertAviSynthResizeFilter));
            OnPropertyChanged(nameof(CanInsertVapourSynthResizeFilter));
            OnPropertyChanged(nameof(CanInsertFFmpegResizeFilter));
            OnPropertyChanged(nameof(VapourSynthResizeFilter));
            OnPropertyChanged(nameof(AviSynthResizeFilter));
        }
    }

    private void RefreshScaleForCropChange()
    {
        int maximum = ScaleHeightMaximum;
        if (_scaleHeight > maximum)
        {
            _scaleHeight = maximum;
            OnPropertyChanged(nameof(ScaleHeight));
        }

        OnPropertyChanged(nameof(IsScaleApplicable));
        OnPropertyChanged(nameof(ScaleHeightMaximum));
        OnPropertyChanged(nameof(ScaleTickLabels));
        OnPropertyChanged(nameof(TargetDisplay));
        OnPropertyChanged(nameof(FFmpegResizeFilter));
        OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(FFmpegHqdn3dFullChainFilter));
        OnPropertyChanged(nameof(CanInsertAviSynthResizeFilter));
        OnPropertyChanged(nameof(CanInsertVapourSynthResizeFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegResizeFilter));
        OnPropertyChanged(nameof(VapourSynthResizeFilter));
        OnPropertyChanged(nameof(AviSynthResizeFilter));
        RecomputeTarget();
    }

    private void RecomputeCrop()
    {
        OnPropertyChanged(nameof(CropWidthMinimum));
        OnPropertyChanged(nameof(CropWidthMaximum));
        OnPropertyChanged(nameof(CropWidthStep));
        OnPropertyChanged(nameof(CropWidthTickLabels));
        OnPropertyChanged(nameof(CropHeightMinimum));
        OnPropertyChanged(nameof(CropHeightMaximum));
        OnPropertyChanged(nameof(CropHeightStep));
        OnPropertyChanged(nameof(CropHeightTickLabels));

        _cropWidth = SourceWidth;
        _cropHeight = SourceHeight;
                OnPropertyChanged(nameof(CropWidth));
                OnPropertyChanged(nameof(CropHeight));
                OnPropertyChanged(nameof(CropTargetDisplay));
                OnPropertyChanged(nameof(HasCropFilter));
        OnPropertyChanged(nameof(FFmpegCropFilter));
        OnPropertyChanged(nameof(VapourSynthCropFilter));
        OnPropertyChanged(nameof(AviSynthCropFilter));
        OnPropertyChanged(nameof(CanInsertAviSynthCropFilter));
        OnPropertyChanged(nameof(CanInsertVapourSynthCropFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegCropFilter));
    }

    private static string DescribeCropMod(int mod) =>
        mod <= 1 ? FilterScribeModalLangProvider.Current["SrcScribe.CropNoRestriction"] : $"mod-{mod}";

    private static List<string> GenerateCropTickLabels(int min, int max, int count)
    {
        List<string> labels = [];
        if (count <= 1 || max <= min)
        {
            labels.Add(min.ToString(CultureInfo.InvariantCulture));
            return labels;
        }

        for (int i = 0; i < count; i++)
        {
            int value = min + (max - min) * i / (count - 1);
            labels.Add(value.ToString(CultureInfo.InvariantCulture));
        }
        return labels;
    }
    #endregion

    #region VFR -> CFR conversion
    private bool _isFrameRateVariable;
    private int _frameRateNum;
    private int _frameRateDen;
    private bool _avsEnableFpsParams;
    private bool _vpyEnableFpsParams;

    public bool IsFrameRateVariable => _isFrameRateVariable;
    public bool IsFrameRateApplicable => HasSource && _isFrameRateVariable;

    public int FrameRateNum => _frameRateNum;
    public int FrameRateDen => _frameRateDen;

    public bool AvsEnableFpsParams
    {
        get => _avsEnableFpsParams;
        set
        {
            if (SetProperty(ref _avsEnableFpsParams, value))
            {
                OnPropertyChanged(nameof(AvsPrefix));
            }
        }
    }

    public bool VpyEnableFpsParams
    {
        get => _vpyEnableFpsParams;
        set
        {
            if (SetProperty(ref _vpyEnableFpsParams, value))
            {
                OnPropertyChanged(nameof(VpyPrefix));
            }
        }
    }

    public static string AvsEnableFpsParamsLabel => "LWLibavVideoSource VFR\u2192CFR";
    public static string VpyEnableFpsParamsLabel => "LWLibavSource VFR\u2192CFR";

    #endregion

    #region ffmpeg FreeText (session only)
    private string _ffmpegFreeText = "";
    public string FFmpegFreeText
    {
        get => _ffmpegFreeText;
        set => SetProperty(ref _ffmpegFreeText, value);
    }

    private void AppendScriptFilter(ref string target, string? filter, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal))
            return;

        target = string.IsNullOrEmpty(target)
            ? filter
            : target.TrimEnd('\r', '\n') + "\r\n" + filter;

        OnPropertyChanged(propertyName);
    }

    private void AppendFFmpegFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal))
            return;

        if (filter.StartsWith("-filter_complex", StringComparison.OrdinalIgnoreCase))
        {
            FFmpegFreeText = string.IsNullOrWhiteSpace(FFmpegFreeText)
                ? filter.Trim()
                : $"{FFmpegFreeText.Trim()} {filter.Trim()}";
            return;
        }

        if (!TrySplitVideoFilterArgs(filter, out string generatedChain, out string generatedSuffix))
        {
            FFmpegFreeText = string.IsNullOrWhiteSpace(FFmpegFreeText)
                ? filter.Trim()
                : $"{FFmpegFreeText.Trim()} {filter.Trim()}";
            return;
        }

        if (!TrySplitVideoFilterArgs(FFmpegFreeText, out string currentChain, out string currentSuffix))
        {
            FFmpegFreeText = string.IsNullOrWhiteSpace(FFmpegFreeText)
                ? filter.Trim()
                : $"-filter:v \"{FFmpegFreeText.Trim()},{generatedChain}\"{generatedSuffix}";
            return;
        }

        string suffix = string.IsNullOrWhiteSpace(currentSuffix) ? generatedSuffix : currentSuffix;
        FFmpegFreeText = $"-filter:v \"{currentChain},{generatedChain}\"{suffix}".Trim();
    }

    private static bool TrySplitVideoFilterArgs(string? value, out string chain, out string suffix)
    {
        chain = string.Empty;
        suffix = string.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;

        int optionIndex = value.IndexOf("-filter:v", StringComparison.OrdinalIgnoreCase);
        if (optionIndex < 0) return false;

        int contentStart = optionIndex + "-filter:v".Length;
        while (contentStart < value.Length && char.IsWhiteSpace(value[contentStart])) contentStart++;
        if (contentStart >= value.Length) return false;

        char quote = value[contentStart] is '"' or '\'' ? value[contentStart++] : '\0';
        int contentEnd = quote == '\0'
            ? value.IndexOf(' ', contentStart)
            : value.IndexOf(quote, contentStart);
        if (contentEnd < 0) contentEnd = value.Length;

        chain = value[contentStart..contentEnd].Trim();
        suffix = value[(contentEnd + (quote == '\0' ? 0 : 1))..];
        return !string.IsNullOrWhiteSpace(chain);
    }

    public string FFmpegConcatFileList => IsConcatMode
        ? ScriptTemplate.BuildConcatFfmpegFileList(GetDisplayConcatFilePaths())
        : string.Empty;
    #endregion

    #region UILang properties
    public static string WindowTitle => FilterScribeModalLangProvider.WindowTitle;
    public static string VFRCFRTitle => "VFR→CFR";
    public static string LowToHighColorFilterLabel => "NCG";
    public static string HighToLowColorFilterLabel => "WCG";
    public static string HdrToSdrColorFilterLabel => "HDR→SDR";
    public static string HighHdrToLowSdrColorFilterLabel => "H&W→SDR";
    public static string ScribeDescription => FilterScribeModalLangProvider.Current["SrcScribe.Description"];
    public static string NoteText => FilterScribeModalLangProvider.Current["SrcScribe.NoteText"];
    public static string TabAvs => LangProviderBase.AviSynth;
    public static string TabVpy => LangProviderBase.VapourSynth;
    public static string TabFFmpeg => LangProviderBase.FFmpeg;
    public static string ResolutionScaleTitle => FilterScribeModalLangProvider.Current["SrcScribe.ResolutionScaleTitle"];
    public static string ScaleHeightLabel => FilterScribeModalLangProvider.Current["SrcScribe.ScaleHeightLabel"];
    public static string FFmpegFreeTextHint => FilterScribeModalLangProvider.Current["SrcScribe.FFmpegFreeTextHint"];
    public static string SarRepairTitle => FilterScribeModalLangProvider.Current["SrcScribe.SarRepairTitle"];
    public static string ColorSpaceConvertTitle => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpaceConvertTitle"];
    public static string DenoiseTitle => FilterScribeModalLangProvider.Current["SrcScribe.DenoiseTitle"];
    public static string ScaleHint => FilterScribeModalLangProvider.Current["SrcScribe.ScaleHint"];
    public static string SubtitleBurnTitle => FilterScribeModalLangProvider.Current["SrcScribe.SubtitleBurnTitle"];
    public static string MultiFilterAssemblyTitle => FilterScribeModalLangProvider.Current["SrcScribe.MultiFilterAssemblyTitle"];
    public static string CropTitle => FilterScribeModalLangProvider.Current["SrcScribe.CropTitle"];
    public static string CropNoRestriction => FilterScribeModalLangProvider.Current["SrcScribe.CropNoRestriction"];
    public static string ColorSpacePeakNitsHint => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpacePeakNitsHint"];
    #endregion

    public ButtonGroupVM FinishScribeButtons { get; private set; } = null!;
    public ActionCmd OpenVpyPreviewCommand { get; }
    public ActionCmd InsertAvsFilterCommand { get; }
    public ActionCmd InsertVpyFilterCommand { get; }
    public ActionCmd InsertFFmpegFilterCommand { get; }
    public ActionCmd InsertAvsCropFilterCommand { get; }
    public ActionCmd InsertVpyCropFilterCommand { get; }
    public ActionCmd InsertFFmpegCropFilterCommand { get; }
    public bool CanOpenVpyPreview => GetVpyPreviewsrcPaths().Length > 0;

    public FilterScribeVM(
        ModalNavS modalNavS,
        Action closeAction,
        Func<string> getsrcPath,
        ToolItemCardVM avsItem,
        ToolItemCardVM vpyItem,
        Func<SrcFileKind?> getPreferredScriptSrcKind,
        Func<string?> getSelectedUpstreamExeName,
        Action<ToolItemCardVM, SrcFileKind, string> afterImport,
        Action<string?> applyFFmpegFilterArgs,
        Func<bool> hasSourceValidationError,
        Func<bool> hasSarRepairWarning,
        string? sourceFfprobeJson = null,
        Func<SrcRevisionRequest, string?>? reviseSource = null,
        Func<bool>? isQueueRoute = null,
        Func<string[]>? getQueueFilePaths = null,
        Func<bool>? isConcatRoute = null,
        Func<string[]>? getConcatFilePaths = null,
        Func<bool>? isRepartRoute = null,
        Action<string?, string?>? applyScriptFilters = null,
        string? vspipePath = null,
        string? vspipeY4mArg = null,
        Func<long>? getTotalFrames = null)
    {
        _modalNavS = modalNavS;
        _closeAction = closeAction;
        CloseCmd = new CloseModalCmd(closeAction);
        _getsrcPath = getsrcPath;
        _avsItem = avsItem;
        _vpyItem = vpyItem;
        _getPreferredScriptSrcKind = getPreferredScriptSrcKind;
        _getSelectedUpstreamExeName = getSelectedUpstreamExeName;
        _afterImport = afterImport;
        _applyFFmpegFilterArgs = applyFFmpegFilterArgs;
        _sourceReviser = reviseSource ?? (_ => null);
        _sourceFfprobeJson = sourceFfprobeJson ?? string.Empty;
        _hasSourceValidationError = hasSourceValidationError;
        _hasSarRepairWarning = hasSarRepairWarning;
        _isQueueRoute = isQueueRoute;
        _getQueueFilePaths = getQueueFilePaths;
        _isConcatRoute = isConcatRoute;
        _getConcatFilePaths = getConcatFilePaths;
        _isRepartRoute = isRepartRoute;
        _applyScriptFilters = applyScriptFilters;
        _vspipePath = vspipePath;
        _vspipeY4mArg = vspipeY4mArg;
        _getTotalFrames = getTotalFrames;
        _baseAvsPrefix = FilterScribeModalLangProvider.Current["SrcScribe.AvsPrefix"];
        _baseVpyPrefix = FilterScribeModalLangProvider.Current["SrcScribe.VpyPrefix"];
        _hasSourceAnalysis = !string.IsNullOrWhiteSpace(sourceFfprobeJson);
        OpenVpyPreviewCommand = new ActionCmd(_ => OpenVpyPreview(), _ => CanOpenVpyPreview);
        InsertAvsFilterCommand = new ActionCmd(filter => AppendScriptFilter(ref _avsUserInput, filter as string, nameof(AvsUserInput)));
        InsertVpyFilterCommand = new ActionCmd(filter => AppendScriptFilter(ref _vpyUserInput, filter as string, nameof(VpyUserInput)));
        InsertFFmpegFilterCommand = new ActionCmd(filter => AppendFFmpegFilter(filter as string));
        InsertAvsCropFilterCommand = new ActionCmd(filter => InsertCropFilter(filter as string, value => AppendScriptFilter(ref _avsUserInput, value, nameof(AvsUserInput))));
        InsertVpyCropFilterCommand = new ActionCmd(filter => InsertCropFilter(filter as string, value => AppendScriptFilter(ref _vpyUserInput, value, nameof(VpyUserInput))));
        InsertFFmpegCropFilterCommand = new ActionCmd(filter => InsertCropFilter(filter as string, AppendFFmpegFilter));
        ParseColorSpaceInfo(sourceFfprobeJson);
        ParseSourceResolution(sourceFfprobeJson);
        ParseFrameRateInfo(sourceFfprobeJson);
        BuildButtonGroups();
        SelectedTabIndex = GetInitialTabIndex(_getSelectedUpstreamExeName());
        UILangProvider.CurrentChanged += OnLanguageChanged;
    }

    private static int GetInitialTabIndex(string? upstreamExeName) => upstreamExeName?.ToLowerInvariant() switch
    {
        "ffmpeg.exe" => 2,
        "vspipe.exe" => 1,
        "avs2yuv.exe" or "avs2pipemod.exe" => 0,
        _ => 0
    };

    #region Concat Source Queries
    private string[] GetCurrentConcatFilePaths() =>
        IsConcatMode ? _getConcatFilePaths?.Invoke() ?? [] : [];

    private string[] GetDisplayConcatFilePaths()
    {
        string[] paths = GetCurrentConcatFilePaths();
        string[] displayPaths = new string[paths.Length];
        for (int i = 0; i < paths.Length; i++)
            displayPaths[i] = ShortenDisplayPath(paths[i]);
        return displayPaths;
    }

    private static string ShortenDisplayPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= DisplayConcatPathMaxLength)
            return path;

        const string prefix = "...";
        int tailLength = DisplayConcatPathMaxLength - prefix.Length;
        return string.Concat(prefix, path.AsSpan(path.Length - tailLength, tailLength));
    }
    #endregion

    private void ParseColorSpaceInfo(string? sourceFfprobeJson)
    {
        _sourceBitDepth = FFProbeSrcVal.ReadBitDepthFromJson(sourceFfprobeJson);
        _colorSpaceAnalysis = ColorSpaceConverter.Analyze(sourceFfprobeJson);
        _sourceIsProgressive = string.IsNullOrWhiteSpace(sourceFfprobeJson) || FFProbeSrcVal.Analyze(sourceFfprobeJson).IsProgressive;
        OnPropertyChanged(nameof(FFmpegLowToHighColorFilter));
        OnPropertyChanged(nameof(FFmpegHighToLowColorFilter));
        OnPropertyChanged(nameof(FFmpegHdrToSdrColorFilter));
        OnPropertyChanged(nameof(FFmpegHighHdrToLowSdrColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegLowToHighColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegHighToLowColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegHdrToSdrColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegHighHdrToLowSdrColorFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(VapourSynthVszipclFilter));
        OnPropertyChanged(nameof(VapourSynthVszipclHasFmtconv));
        OnPropertyChanged(nameof(VapourSynthVszipclFmtconvHint));
        RecomputeCrop();
    }

    public void RefreshGeneratedFFmpegFilters()
    {
        OnPropertyChanged(nameof(FFmpegSarRepairFilter));
        OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(FFmpegCropFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegFpsFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegSarRepairFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegResizeFilter));
    }

    private void ParseSourceResolution(string? sourceFfprobeJson)
    {
        _sourceAspectRatio = FFProbeAspectRatioResolver.Resolve(sourceFfprobeJson);
        var resolution = FFProbeSrcResolution.Read(sourceFfprobeJson);
        if (resolution.HasValue)
        {
            SourceWidth = resolution.Value.width;
            SourceHeight = resolution.Value.height;
        }
        RecomputeCrop();
        OnPropertyChanged(nameof(AviSynthAssRenderFilter));
    }

    private void ParseFrameRateInfo(string? sourceFfprobeJson)
    {
        var info = FrameRate.GetVariableFrameRateInfo(sourceFfprobeJson);
        if (!info.HasValue) return;

        _isFrameRateVariable = info.Value.isVariable;
        if (_isFrameRateVariable)
        {
            _frameRateNum = info.Value.num;
            _frameRateDen = info.Value.den;
        }

        OnPropertyChanged(nameof(IsFrameRateVariable));
        OnPropertyChanged(nameof(IsFrameRateApplicable));
        OnPropertyChanged(nameof(FrameRateNum));
        OnPropertyChanged(nameof(FrameRateDen));
        OnPropertyChanged(nameof(FFmpegFpsFilter));
        OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
    }

    private void BuildButtonGroups()
    {
        FinishScribeButtons = ButtonGroupVM.CreateThreeButton(
            FilterScribeModalLangProvider.Current["SrcScribe.Cancel"],
            FilterScribeModalLangProvider.Current["SrcScribe.ApplyFFmpegOnly"],
            FilterScribeModalLangProvider.Current["SrcScribe.Confirm"],
            CloseCmd,
            new ActionCmd(_ => ApplyFFmpegFilterArgsOnly()),
            new ActionCmd(_ => SaveAndImportAll()));

        UpdateFinishButtonState();
    }

    public void SetSourceAnalysisState(bool hasSourceAnalysis)
    {
        if (SetProperty(ref _hasSourceAnalysis, hasSourceAnalysis))
            UpdateFinishButtonState();
    }

    private void UpdateFinishButtonState()
    {
        if (FinishScribeButtons == null) return;

        FinishScribeButtons.B3_2IsEnabled = _hasSourceAnalysis;
        FinishScribeButtons.B3_3IsEnabled = _hasSourceAnalysis;
    }

    private void ExecuteQueueSaveAndImport()
    {
        string[] srcPaths = _getQueueFilePaths?.Invoke() ?? [];
        if (srcPaths.Length == 0 || string.IsNullOrWhiteSpace(srcPaths[0])) return;

        OpenFolderDialog dialog = new()
        {
            Title = FilterScribeModalLangProvider.SavingScriptWindowTitle
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

        string directory = dialog.FolderName;
        int avsFpsnum = _avsEnableFpsParams ? _frameRateNum : 0;
        int avsFpsden = _avsEnableFpsParams ? _frameRateDen : 0;
        int vpyFpsnum = _vpyEnableFpsParams ? _frameRateNum : 0;
        int vpyFpsden = _vpyEnableFpsParams ? _frameRateDen : 0;
        List<string> savedPaths = [];

        try
        {
            foreach (string srcPath in srcPaths)
            {
                string baseName = Path.GetFileNameWithoutExtension(srcPath);
                string avsPath = Path.Combine(directory, baseName + ".avs");
                string vpyPath = Path.Combine(directory, baseName + ".vpy");

                File.WriteAllText(avsPath, ScriptTemplate.BuildAvsExportScript(
                    srcPath, AvsPrefix2, AvsSuffix, AvsUserInput, avsFpsnum, avsFpsden));
                File.WriteAllText(vpyPath, ScriptTemplate.BuildVpyExportScript(
                    srcPath, VpyPrefix2, VpySuffix, VpyUserInput, vpyFpsnum, vpyFpsden));
                savedPaths.Add(avsPath);
                savedPaths.Add(vpyPath);
            }
        }
        catch (Exception ex)
        {
            ShowSaveError(ex);
            return;
        }

        // Extract saved script file names for card display and hover tooltip
        string[] avsFileNames = [.. savedPaths.Where(path => path
            .EndsWith(".avs", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];
        string[] vpyFileNames = [.. savedPaths.Where(path => path
            .EndsWith(".vpy", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)];

        _avsItem.P2TextData = directory;
        _avsItem.P1TextData = BrowseCmdBase.FormatQueueP1Text(avsFileNames);
        _avsItem.P1TooltipText = BrowseCmdBase.FormatQueueP1TooltipText(avsFileNames);
        _vpyItem.P2TextData = directory;
        _vpyItem.P1TextData = BrowseCmdBase.FormatQueueP1Text(vpyFileNames);
        _vpyItem.P1TooltipText = BrowseCmdBase.FormatQueueP1TooltipText(vpyFileNames);

        SelectPreferredScriptItem();

        new OpenSuccModalCmd(
            _modalNavS,
            FilterScribeModalLangProvider.WindowTitle,
            string.Format(UILangProvider.Current["ScriptGen.ScriptsSaved"],
            string.Join(Environment.NewLine, savedPaths))).Execute(null);
        _closeAction();
    }

    private void SaveAndImportAll()
    {
        if (!IsRepartMode && !ShowSourceReviserModal()) return;

        ApplyFFmpegFilterArgs();

        if (IsRepartMode)
            _applyScriptFilters?.Invoke(AvsUserInput, VpyUserInput);

        if (_isQueueRoute?.Invoke() == true)
        {
            ExecuteQueueSaveAndImport();
            return;
        }

        if (IsConcatMode)
        {
            ExecuteConcatSaveAndImport();
            return;
        }

        string srcPath = _getsrcPath();
        string avsScript = ScriptTemplate.BuildAvsExportScript(
            srcPath, AvsPrefix2, AvsSuffix, AvsUserInput,
            _avsEnableFpsParams ? _frameRateNum : 0, _avsEnableFpsParams ? _frameRateDen : 0);
        string vpyScript = ScriptTemplate.BuildVpyExportScript(
            srcPath, VpyPrefix2, VpySuffix, VpyUserInput,
            _vpyEnableFpsParams ? _frameRateNum : 0, _vpyEnableFpsParams ? _frameRateDen : 0);

        SaveFileDialog dialog = new()
        {
            Title = FilterScribeModalLangProvider.SavingScriptWindowTitle,
            Filter = FilterScribeModalLangProvider.Current["SrcScribe.FilterAvs"],
            FileName = FilterScribeScriptPersistence.GetScriptFileName(srcPath, ".avs")
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

        string avsPath = dialog.FileName;
        string directory = Path.GetDirectoryName(avsPath) ?? ".";
        string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");

        if (!FilterScribeScriptPersistence.TryWriteScripts(avsPath, avsScript, vpyPath, vpyScript, ShowSaveError)) return;

        SrcFileKind? preferredKind = _getPreferredScriptSrcKind();
        if (preferredKind == SrcFileKind.AviSynthScript)
        {
            ImportScript(_avsItem, SrcFileKind.AviSynthScript, avsPath);
        }
        else if (preferredKind == SrcFileKind.VapourSynthScript)
        {
            ImportScript(_vpyItem, SrcFileKind.VapourSynthScript, vpyPath);
        }
        else
        {
            ImportScript(_avsItem, SrcFileKind.AviSynthScript, avsPath);
            ImportScript(_vpyItem, SrcFileKind.VapourSynthScript, vpyPath);
        }

        SelectPreferredScriptItem();
        new OpenSuccModalCmd(
            _modalNavS,
            FilterScribeModalLangProvider.WindowTitle,
            string.Format(UILangProvider.Current["ScriptGen.ScriptsSaved"], $"{avsPath}\n{vpyPath}")).Execute(null);
        _closeAction();
    }

    private void ExecuteConcatSaveAndImport()
    {
        string[] concatPaths = GetCurrentConcatFilePaths();
        if (!EnsureConcatSourceCount(concatPaths)) return;

        int avsFpsnum = _isFrameRateVariable && _avsEnableFpsParams ? _frameRateNum : 0;
        int avsFpsden = _isFrameRateVariable && _avsEnableFpsParams ? _frameRateDen : 0;
        int vpyFpsnum = _isFrameRateVariable && _vpyEnableFpsParams ? _frameRateNum : 0;
        int vpyFpsden = _isFrameRateVariable && _vpyEnableFpsParams ? _frameRateDen : 0;
        string avsScript = ScriptTemplate.BuildConcatAvsExportScript(
            concatPaths,
            AvsPrefix2,
            AvsSuffix,
            AvsUserInput,
            avsFpsnum,
            avsFpsden);
        string vpyScript = ScriptTemplate.BuildConcatVpyExportScript(
            concatPaths,
            VpyPrefix2,
            VpySuffix,
            VpyUserInput,
            vpyFpsnum,
            vpyFpsden);

        SaveFileDialog dialog = new()
        {
            Title = FilterScribeModalLangProvider.SavingScriptWindowTitle,
            Filter = FilterScribeModalLangProvider.Current["SrcScribe.FilterAvs"],
            FileName = BrowseSrcQueueCmd.FormatConcatFileName(concatPaths) + "_concat.avs"
        };

        if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

        string avsPath = dialog.FileName;
        string directory = Path.GetDirectoryName(avsPath) ?? ".";
        string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");

        if (!FilterScribeScriptPersistence.TryWriteScripts(avsPath, avsScript, vpyPath, vpyScript, ShowSaveError)) return;

        SrcFileKind? preferredKind = _getPreferredScriptSrcKind();
        if (preferredKind == SrcFileKind.AviSynthScript)
        {
            ImportScript(_avsItem, SrcFileKind.AviSynthScript, avsPath);
        }
        else if (preferredKind == SrcFileKind.VapourSynthScript)
        {
            ImportScript(_vpyItem, SrcFileKind.VapourSynthScript, vpyPath);
        }
        else
        {
            ImportScript(_avsItem, SrcFileKind.AviSynthScript, avsPath);
            ImportScript(_vpyItem, SrcFileKind.VapourSynthScript, vpyPath);
        }

        SelectPreferredScriptItem();
        new OpenSuccModalCmd(
            _modalNavS,
            FilterScribeModalLangProvider.WindowTitle,
            string.Format(UILangProvider.Current["ScriptGen.ScriptsSaved"], $"{avsPath}\n{vpyPath}")).Execute(null);
        _closeAction();
    }

    private bool EnsureConcatSourceCount(string[] concatPaths)
    {
        if (concatPaths.Length > 1) return true;

        new OpenErrModalCmd(
            _modalNavS,
            FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSourcesTitle"],
            FilterScribeModalLangProvider.Current["SrcScribe.ConcatNeedMultipleSources"]).Execute(null);
        return false;
    }

    private void ApplyFFmpegFilterArgsOnly()
    {
        if (!IsRepartMode && !ShowSourceReviserModal()) return;

        ApplyFFmpegFilterArgs();
        _closeAction();
    }

    private void ApplyFFmpegFilterArgs() =>
        _applyFFmpegFilterArgs(FFmpegFreeText.Trim());

    private bool ShowSourceReviserModal()
    {
        var (suggestedWidth, suggestedHeight) = GetSuggestedOutputResolution();
        return ShowSourceReviserModal(suggestedWidth, suggestedHeight);
    }

    private bool ShowSourceReviserModal(int suggestedWidth, int suggestedHeight)
    {
        SrcReviserModal window = new();
        SrcReviserVM vm = new(
            _modalNavS,
            window.Close,
            result => window.DialogResult = result,
            _sourceReviser,
            SourceWidth,
            SourceHeight,
            suggestedWidth,
            suggestedHeight,
            HasCropFilter ? CropWidth : 0,
            HasCropFilter ? CropHeight : 0);

        window.DataContext = vm;
        window.Owner = Application.Current.Windows
            .OfType<FilterScribeModal>()
            .FirstOrDefault(w => ReferenceEquals(w.DataContext, this))
            ?? Application.Current.MainWindow;
        window.Closed += (_, _) => _modalNavS.Close();
        _modalNavS.CurrentModalVM = vm;

        bool confirmed = window.ShowDialog() == true;
        if (confirmed)
        {
            SourceWidth = vm.ResolutionWidth;
            SourceHeight = vm.ResolutionHeight;
        }
        return confirmed;
    }

    private bool InsertCropFilter(string? filter, Action<string?> insertAction)
    {
        if (!HasCropFilter || string.IsNullOrWhiteSpace(filter) || filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal))
            return false;

        insertAction(filter);
        return true;
    }

    private (int width, int height) GetSuggestedOutputResolution()
    {
        if (HasScaleFilter && TargetWidth > 0 && TargetHeight > 0)
            return (TargetWidth, TargetHeight);

        return HasSource ? (SourceWidth, SourceHeight) : (0, 0);
    }

    private void ImportScript(ToolItemCardVM item, SrcFileKind kind, string path)
    {
        item.P2TextData = path;
        item.P1TextData = SrcFilePicker.GetPrimaryText(kind, path);
        _afterImport(item, kind, path);
    }

    private void SelectPreferredScriptItem()
    {
        SrcFileKind? preferredKind = _getPreferredScriptSrcKind();
        if (preferredKind == null) return;

        ToolItemCardVM target = preferredKind == SrcFileKind.AviSynthScript ? _avsItem : _vpyItem;
        if (target == _avsItem)
            _vpyItem.IsSelected = false;
        else
            _avsItem.IsSelected = false;

        if (target.IsEnabled && !string.IsNullOrWhiteSpace(target.P2TextData)) target.IsSelected = true;
    }

    private void ShowSaveError(Exception ex)
    {
        new OpenErrModalCmd(
            _modalNavS,
            FilterScribeModalLangProvider.WindowTitle,
            string.Format(FilterScribeModalLangProvider.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
    }

    #region VapourSynth Preview
    private void OpenVpyPreview()
    {
        if (!CanOpenVpyPreview) return;

        var existingWindow = Application.Current.Windows
            .OfType<VpyPreviewDialog>()
            .FirstOrDefault();

        if (existingWindow != null)
        {
            existingWindow.Activate();
            return;
        }

        if (string.IsNullOrWhiteSpace(_vspipePath) || !File.Exists(_vspipePath))
        {
            new OpenErrModalCmd(
                _modalNavS,
                VpyPreviewLangProvider.WindowTitle,
                "!vspipe.exe").Execute(null);
            return;
        }

        if (string.IsNullOrWhiteSpace(_vspipeY4mArg))
        {
            new OpenErrModalCmd(
                _modalNavS,
                VpyPreviewLangProvider.WindowTitle,
                "!vspipe Y4M args").Execute(null);
            return;
        }

        string srcPath = GetVpyPreviewsrcPath();
        if (string.IsNullOrWhiteSpace(srcPath))
        {
            new OpenErrModalCmd(
                _modalNavS,
                VpyPreviewLangProvider.WindowTitle,
                "!source").Execute(null);
            return;
        }

        int fpsnum = _isFrameRateVariable && _vpyEnableFpsParams ? _frameRateNum : 0;
        int fpsden = _isFrameRateVariable && _vpyEnableFpsParams ? _frameRateDen : 0;
        string script = ScriptTemplate.BuildVpyPreviewScript(srcPath, VpyUserInput, fpsnum, fpsden);

        long total = _getTotalFrames?.Invoke() ?? 0;
        int frameCount = (int)Math.Min(total > 0 ? total : 1, int.MaxValue);

        string[] previewsrcPaths = GetVpyPreviewsrcPaths();
        string buildScript(string path) => ScriptTemplate.BuildVpyPreviewScript(path, VpyUserInput, fpsnum, fpsden);

        var previewVm = new VpyPreviewVM(
            _modalNavS,
            _vspipePath,
            _vspipeY4mArg,
            script,
            srcPath,
            frameCount,
            buildPreviewScript: buildScript,
            queueFilePaths: previewsrcPaths);

        var ownerWindow = Application.Current.Windows
            .OfType<FilterScribeModal>()
            .FirstOrDefault(w => ReferenceEquals(w.DataContext, this));
        VpyPreviewDialog window = new(previewVm, _modalNavS, ownerWindow);

        if (ownerWindow != null)
            PositionVpyPreviewWindow(ownerWindow, window);

        window.Show();
    }

    private static void PositionVpyPreviewWindow(Window ownerWindow, Window previewWindow)
    {
        Rect workArea = SystemParameters.WorkArea;
        double ownerWidth = GetWindowWidth(ownerWindow);
        double previewWidth = GetWindowWidth(previewWindow);
        double previewLeft = workArea.Left + ownerWidth;
        double availablePreviewWidth = workArea.Right - previewLeft;

        ownerWindow.Left = workArea.Left;

        if (availablePreviewWidth > 0 && previewWidth > availablePreviewWidth)
            previewWindow.Width = availablePreviewWidth;

        previewWindow.Left = previewLeft;
        previewWindow.Top = Math.Max(workArea.Top, ownerWindow.Top);
        previewWindow.WindowStartupLocation = WindowStartupLocation.Manual;
    }

    private static double GetWindowWidth(Window window) =>
        !double.IsNaN(window.Width) && window.Width > 0
            ? window.Width
            : window.ActualWidth;

    private string GetVpyPreviewsrcPath() =>
        GetVpyPreviewsrcPaths().FirstOrDefault() ?? string.Empty;

    private string[] GetVpyPreviewsrcPaths()
    {
        if (_isQueueRoute?.Invoke() == true)
            return [.. (_getQueueFilePaths?.Invoke() ?? []).Where(path => !string.IsNullOrWhiteSpace(path))];

        if (IsConcatMode)
            return [.. GetCurrentConcatFilePaths().Where(path => !string.IsNullOrWhiteSpace(path))];

        string srcPath = _getsrcPath();
        return string.IsNullOrWhiteSpace(srcPath) ? [] : [srcPath];
    }

    #endregion

    #region Language switching
    private void OnLanguageChanged()
    {
        _baseAvsPrefix = FilterScribeModalLangProvider.Current["SrcScribe.AvsPrefix"];
        _baseVpyPrefix = FilterScribeModalLangProvider.Current["SrcScribe.VpyPrefix"];

        OnPropertyChanged(nameof(ScribeDescription));
        OnPropertyChanged(nameof(NoteText));
        OnPropertyChanged(nameof(AvsPrefix));
        OnPropertyChanged(nameof(AvsSuffix));
        OnPropertyChanged(nameof(VpyPrefix));
        OnPropertyChanged(nameof(VpySuffix));
        OnPropertyChanged(nameof(Lang));
        OnPropertyChanged(nameof(FFmpegConcatFileList));
        OnPropertyChanged(nameof(CropTitle));
        OnPropertyChanged(nameof(CropTargetDisplay));
        OnPropertyChanged(nameof(HasCropFilter));
        OnPropertyChanged(nameof(FFmpegCropFilter));
        OnPropertyChanged(nameof(VapourSynthCropFilter));
        OnPropertyChanged(nameof(AviSynthCropFilter));
        OnPropertyChanged(nameof(CropWidthMinimum));
        OnPropertyChanged(nameof(CropWidthMaximum));
        OnPropertyChanged(nameof(CropHeightMinimum));
        OnPropertyChanged(nameof(CropHeightMaximum));
        OnPropertyChanged(nameof(CropWidthTickLabels));
        OnPropertyChanged(nameof(CropHeightTickLabels));
        OnPropertyChanged(nameof(ResolutionScaleTitle));
        OnPropertyChanged(nameof(ScaleHeightLabel));
        OnPropertyChanged(nameof(HasSource));
        OnPropertyChanged(nameof(ScaleNotApplicableText));
        OnPropertyChanged(nameof(TargetDisplay));
        OnPropertyChanged(nameof(FFmpegFreeTextHint));
        OnPropertyChanged(nameof(SarRepairTitle));
        OnPropertyChanged(nameof(FFmpegSarRepairFilter));
        OnPropertyChanged(nameof(FFmpegHqdn3dDenoiseFilter));
        OnPropertyChanged(nameof(FFmpegSubtitleFilter));
        OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(VapourSynthVszipclTitle));
        OnPropertyChanged(nameof(VapourSynthVszipclPreviewHint));
        OnPropertyChanged(nameof(VapourSynthVszipclDeviceHint));
        OnPropertyChanged(nameof(VapourSynthVszipclFmtconvHint));
        OnPropertyChanged(nameof(ColorSpaceConvertTitle));
        OnPropertyChanged(nameof(DenoiseTitle));
        OnPropertyChanged(nameof(ScaleHint));
        OnPropertyChanged(nameof(SubtitleBurnTitle));
        OnPropertyChanged(nameof(MultiFilterAssemblyTitle));
        OnPropertyChanged(nameof(LowToHighColorFilterLabel));
        OnPropertyChanged(nameof(HighToLowColorFilterLabel));
        OnPropertyChanged(nameof(HdrToSdrColorFilterLabel));
        OnPropertyChanged(nameof(HighHdrToLowSdrColorFilterLabel));
        OnPropertyChanged(nameof(ColorSpacePeakNitsHint));
        OnPropertyChanged(nameof(FFmpegLowToHighColorFilter));
        OnPropertyChanged(nameof(FFmpegHighToLowColorFilter));
        OnPropertyChanged(nameof(FFmpegHdrToSdrColorFilter));
        OnPropertyChanged(nameof(FFmpegHighHdrToLowSdrColorFilter));
        OnPropertyChanged(nameof(AvsEnableFpsParamsLabel));
        OnPropertyChanged(nameof(VpyEnableFpsParamsLabel));
        OnPropertyChanged(nameof(IsConcatMode));
        OnPropertyChanged(nameof(CanOpenVpyPreview));
        OpenVpyPreviewCommand.OnCanExecuteChanged();

        BuildButtonGroups();
        OnPropertyChanged(nameof(FinishScribeButtons));
    }
    #endregion

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
