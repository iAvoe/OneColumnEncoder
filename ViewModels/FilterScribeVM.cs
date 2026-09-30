using OneColumnEncoder.Components;
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
    private readonly Func<string?>? _getAvs2yuvPath;
    private readonly Func<string?>? _getAvs2pipemodPath;
    private readonly Func<string?>? _getFfmpegPath;
    private readonly string? _sourceFfprobeJson;
    private const int DisplayConcatPathMaxLength = 90;
    private ColorSpaceAnalysisM _colorSpaceAnalysis = ColorSpaceConverter.Analyze(null);
    private int _sourceBitDepth;
    private bool _hasSourceAnalysis;
    private bool _sourceIsProgressive = true;
    private string _colorSpacePeakNits = string.Empty;
    private string _vszipclDeviceId = "0";
    private readonly System.Windows.Threading.DispatcherTimer _cropRefreshTimer;
    private bool _cropRefreshPending;
    public CloseModalCmd CloseCmd { get; }
    public ObservableCollection<AppConfItem> SettingsListing { get; } = [];
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
                NotifyTabDependentProperties();
        }
    }
    public bool IsAvsTabSelected => _selectedTabIndex == 0;
    public bool IsVpyTabSelected => _selectedTabIndex == 1;
    public bool IsFFmpegTabSelected => _selectedTabIndex == 2;
    public bool IsScriptTabSelected => IsAvsTabSelected || IsVpyTabSelected;
    public bool IsAvsOrFFmpegTabSelected => IsAvsTabSelected || IsFFmpegTabSelected;
    public bool IsDenoiseSectionVisible => IsFFmpegTabSelected || IsAvsTabSelected;
    public bool IsBlurSharpenSectionVisible => IsAvsTabSelected || IsVpyTabSelected;

    public string SelectedRotateFilterDisplay =>
        GetSelectedTabFilter(AvsRotateFilterDisplay, VpyRotateFilterDisplay, FFmpegRotateFilterDisplay);

    public string SelectedScriptFlipFilterDisplay =>
        GetSelectedTabFilter($"{AvsHFlipFilter}\r\n{AvsVFlipFilter}",
            $"{VpyHFlipFilter}\r\n{VpyVFlipFilter}",
            LangProviderBase.NAText);

    public string SelectedCropFilterDisplay =>
        GetSelectedTabFilter(AviSynthCropFilter, VapourSynthCropFilter, FFmpegCropFilterDisplay);

    public string SelectedResizeFilterDisplay =>
        GetSelectedTabFilter(AviSynthResizeFilter, VapourSynthResizeFilter, FFmpegResizeFilterDisplay);

    public string SelectedChroma422FilterDisplay =>
        GetSelectedTabFilter(AviSynthChroma422Filter, VapourSynthChroma422Filter, FFmpegChroma422Filter);

    public string SelectedChroma420FilterDisplay =>
        GetSelectedTabFilter(AviSynthChroma420Filter, VapourSynthChroma420Filter, FFmpegChroma420Filter);

    public string SelectedBlurSharpenFilterDisplay =>
        GetSelectedTabFilter(AviSynthBlurSharpenFilter, VapourSynthBlurSharpenFilter, LangProviderBase.NAText);

    public string SelectedDenoiseFilterDisplay =>
        GetSelectedTabFilter(AviSynthHqdn3dDenoiseFilter, LangProviderBase.NAText, FFmpegHqdn3dDenoiseFilterDisplay);

    public string SelectedSubtitleFilterDisplay =>
        GetSelectedTabFilter(AviSynthAssRenderFilter, VapourSynthSubtitleFilter, FFmpegSubtitleFilter);

    public string SelectedPlaceboLoadCommand =>
        GetSelectedTabFilter(AviSynthPlaceboLoadCommand, VapourSynthPlaceboLoadCommand, LangProviderBase.NAText);

    public string SelectedHlgTargetLabel => IsVpyTabSelected ? "HLG→SDR" : "HLG→709";
    public static string SelectedDoviSdrTargetLabel => "DOVI SDR→709";
    public string SelectedDoviHdrTargetLabel => IsVpyTabSelected ? "DOVI HDR→SDR709" : "DOVI HDR→709";

    public string SelectedLowToHighColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthLowToHighColorFilterDisplay, VapourSynthLowToHighColorFilterDisplay, FFmpegLowToHighColorFilterDisplay);

    public string SelectedHighToLowColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthHighToLowColorFilterDisplay, VapourSynthHighToLowColorFilterDisplay, FFmpegHighToLowColorFilterDisplay);

    public string SelectedHdrToSdrColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthHdrToSdrColorFilterDisplay, VapourSynthHdrToSdrColorFilterDisplay, FFmpegHdrToSdrColorFilterDisplay);

    public string SelectedHighHdrToLowSdrColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthHighHdrToLowSdrColorFilterDisplay, VapourSynthHighHdrToLowSdrColorFilterDisplay, FFmpegHighHdrToLowSdrColorFilterDisplay);

    public string SelectedHlgToSdrColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthHlgToSdrColorFilterDisplay, VapourSynthHlgToSdrColorFilterDisplay, FFmpegHlgToSdrColorFilterDisplay);

    public string SelectedDoviSdrTo709ColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthDoviSdrTo709ColorFilterDisplay, VapourSynthDoviSdrTo709ColorFilterDisplay, FFmpegDoviSdrTo709ColorFilterDisplay);

    public string SelectedDoviHdrToSdrColorFilterDisplay =>
        GetSelectedTabFilter(AviSynthDoviHdrToSdrColorFilterDisplay, VapourSynthDoviHdrToSdrColorFilterDisplay, FFmpegDoviHdrToSdrColorFilterDisplay);

    private string GetSelectedTabFilter(string avsFilter, string vpyFilter, string ffmpegFilter) =>
        _selectedTabIndex switch
        {
            0 => avsFilter,
            1 => vpyFilter,
            2 => ffmpegFilter,
            _ => LangProviderBase.NAText
        };

    private void NotifyTabDependentProperties()
    {
        NotifyProperties(
            nameof(IsAvsTabSelected),
            nameof(IsVpyTabSelected),
            nameof(IsFFmpegTabSelected),
            nameof(IsScriptTabSelected),
            nameof(IsAvsOrFFmpegTabSelected),
            nameof(IsDenoiseSectionVisible),
            nameof(IsBlurSharpenSectionVisible),
            nameof(SelectedRotateFilterDisplay),
            nameof(SelectedScriptFlipFilterDisplay),
            nameof(SelectedCropFilterDisplay),
            nameof(SelectedResizeFilterDisplay),
            nameof(SelectedChroma422FilterDisplay),
            nameof(SelectedChroma420FilterDisplay),
            nameof(SelectedBlurSharpenFilterDisplay),
            nameof(SelectedDenoiseFilterDisplay),
            nameof(SelectedSubtitleFilterDisplay),
            nameof(SelectedPlaceboLoadCommand),
            nameof(SelectedResolutionPlaceboLoadCommand),
            nameof(SelectedHlgTargetLabel),
            nameof(SelectedDoviSdrTargetLabel),
            nameof(SelectedDoviHdrTargetLabel),
            nameof(SelectedLowToHighColorFilterDisplay),
            nameof(SelectedHighToLowColorFilterDisplay),
            nameof(SelectedHdrToSdrColorFilterDisplay),
            nameof(SelectedHighHdrToLowSdrColorFilterDisplay),
            nameof(SelectedHlgToSdrColorFilterDisplay),
            nameof(SelectedDoviSdrTo709ColorFilterDisplay),
            nameof(SelectedDoviHdrToSdrColorFilterDisplay),
            nameof(CanClearFilters));
        ClearFiltersCommand?.OnCanExecuteChanged();
    }

    private void NotifyProperties(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
            OnPropertyChanged(propertyName);
    }

    private void NotifySourceDimensionProperties()
    {
        NotifyProperties(
            nameof(HasSource),
            nameof(IsScaleApplicable),
            nameof(ResolutionWidthMinimum),
            nameof(ResolutionWidthMaximum),
            nameof(ResolutionHeightMinimum),
            nameof(ResolutionHeightMaximum),
            nameof(ResolutionWidthTickLabels),
            nameof(ResolutionHeightTickLabels),
            nameof(ResolutionStep),
            nameof(ScaleNotApplicableText));
    }

    private void NotifyResolutionFilterProperties()
    {
        NotifyProperties(
            nameof(IsResolutionUpscale),
            nameof(IsResolutionShrink),
            nameof(IsResolutionAspectLocked),
            nameof(ResolutionModeLabel),
            nameof(ResolutionWidth),
            nameof(ResolutionHeight),
            nameof(ResolutionWidthMinimum),
            nameof(ResolutionWidthMaximum),
            nameof(ResolutionHeightMinimum),
            nameof(ResolutionHeightMaximum),
            nameof(ResolutionWidthTickLabels),
            nameof(ResolutionHeightTickLabels),
            nameof(ResolutionTargetDisplay),
            nameof(TargetDisplay),
            nameof(FFmpegResizeFilter),
            nameof(FFmpegResizeFilterDisplay),
            nameof(FFmpegUpscaleFilter),
            nameof(FFmpegUpscaleFilterDisplay),
            nameof(VapourSynthResizeFilter),
            nameof(AviSynthResizeFilter),
            nameof(VapourSynthResizeFilterWithImport),
            nameof(AviSynthResizeFilterWithImport),
            nameof(SelectedResolutionPlaceboLoadCommand),
            nameof(CanInsertAviSynthResizeFilter),
            nameof(CanInsertVapourSynthResizeFilter),
            nameof(CanInsertFFmpegResizeFilter),
            nameof(CanInsertFFmpegUpscaleFilter),
            nameof(UpscaleTargetDisplay),
            nameof(SelectedResizeFilterDisplay),
            nameof(VapourSynthResolutionPlaceboLoadCommand),
            nameof(AviSynthResolutionPlaceboLoadCommand));
    }

    private void NotifyCropFilterProperties()
    {
        NotifyProperties(
            nameof(CropTargetDisplay),
            nameof(HasCropFilter),
            nameof(FFmpegCropFilter),
            nameof(FFmpegCropFilterDisplay),
            nameof(VapourSynthCropFilter),
            nameof(AviSynthCropFilter),
            nameof(SelectedCropFilterDisplay),
            nameof(CanInsertAviSynthCropFilter),
            nameof(CanInsertVapourSynthCropFilter),
            nameof(CanInsertFFmpegCropFilter));
    }

    private void NotifyUpscaleFilterProperties()
    {
        NotifyProperties(
            nameof(UpscaleTargetDisplay),
            nameof(FFmpegUpscaleFilter),
            nameof(FFmpegUpscaleFilterDisplay),
            nameof(CanInsertFFmpegUpscaleFilter));
    }

    private void NotifyScaleFilterProperties()
    {
        NotifyProperties(
            nameof(TargetDisplay),
            nameof(FFmpegResizeFilter),
            nameof(FFmpegResizeFilterDisplay),
            nameof(FFmpegFpsScaleFilter),
            nameof(FFmpegFpsColorScaleFilter),
            nameof(FFmpegFullChainFilter),
            nameof(FFmpegHqdn3dFullChainFilter),
            nameof(CanInsertAviSynthResizeFilter),
            nameof(CanInsertVapourSynthResizeFilter),
             nameof(CanInsertFFmpegResizeFilter),
             nameof(VapourSynthResizeFilter),
             nameof(AviSynthResizeFilter),
             nameof(VapourSynthResizeFilterWithImport),
             nameof(AviSynthResizeFilterWithImport),
             nameof(SelectedResolutionPlaceboLoadCommand),
             nameof(SelectedResizeFilterDisplay));
    }

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
        set
        {
            if (SetProperty(ref _avsUserInput, value))
                OnFilterInputChanged();
        }
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
        set
        {
            if (SetProperty(ref _vpyUserInput, value))
                OnFilterInputChanged();
        }
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
                NotifySourceDimensionProperties();
                RecomputeCrop();
                OnPropertyChanged(nameof(AviSynthAssRenderFilter));
                OnPropertyChanged(nameof(SelectedSubtitleFilterDisplay));
                RecomputeTarget();
                NotifyScaleFilterProperties();
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
                NotifySourceDimensionProperties();
                RecomputeCrop();
                OnPropertyChanged(nameof(AviSynthAssRenderFilter));
                OnPropertyChanged(nameof(SelectedSubtitleFilterDisplay));
                RecomputeTarget();
                NotifyScaleFilterProperties();
            }
        }
    }

    private int ScaleSourceWidth => HasCropFilter ? CropWidth : SourceWidth;
    private int ScaleSourceHeight => HasCropFilter ? CropHeight : SourceHeight;

    public bool IsScaleApplicable => HasSource;

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
                NotifyCropFilterProperties();
                ScheduleCropRefresh();
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
                NotifyCropFilterProperties();
                ScheduleCropRefresh();
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

    private bool _isResolutionUpscale;
    public bool IsResolutionUpscale
    {
        get => _isResolutionUpscale;
        set
        {
            if (!SetProperty(ref _isResolutionUpscale, value)) return;
            ResetResolutionTarget();
            NotifyResolutionFilterProperties();
        }
    }

    public bool IsResolutionShrink => !IsResolutionUpscale;

    private bool _isResolutionAspectLocked = true;
    public bool IsResolutionAspectLocked
    {
        get => _isResolutionAspectLocked;
        set
        {
            if (!SetProperty(ref _isResolutionAspectLocked, value)) return;
            if (value) SetResolutionHeight(_resolutionHeight);
            NotifyResolutionFilterProperties();
        }
    }

    private int _resolutionWidth;
    public int ResolutionWidth
    {
        get => _resolutionWidth;
        set
        {
            int clamped = Math.Clamp(value, ResolutionWidthMinimum, ResolutionWidthMaximum);
            if (!SetProperty(ref _resolutionWidth, clamped)) return;
            if (IsResolutionAspectLocked)
                SetResolutionHeightFromWidth(clamped);
            NotifyResolutionFilterProperties();
        }
    }

    private int _resolutionHeight;
    public int ResolutionHeight
    {
        get => _resolutionHeight;
        set => SetResolutionHeight(value);
    }

    private void SetResolutionHeight(int value)
    {
        int clamped = Math.Clamp(value, ResolutionHeightMinimum, ResolutionHeightMaximum);
        if (!SetProperty(ref _resolutionHeight, clamped)) return;
        if (IsResolutionAspectLocked)
            SetResolutionWidthFromHeight(clamped);
        NotifyResolutionFilterProperties();
    }

    private void SetResolutionHeightFromWidth(int width)
    {
        if (!HasSource || ScaleSourceWidth <= 0) return;
        int height = ResolutionScale.EnsureValid((int)Math.Round(width * (double)ScaleSourceHeight / ScaleSourceWidth));
        SetProperty(ref _resolutionHeight, Math.Clamp(height, ResolutionHeightMinimum, ResolutionHeightMaximum), nameof(ResolutionHeight));
    }

    private void SetResolutionWidthFromHeight(int height)
    {
        if (!HasSource || ScaleSourceHeight <= 0) return;
        int width = ResolutionScale.EnsureValid((int)Math.Round(height * (double)ScaleSourceWidth / ScaleSourceHeight));
        SetProperty(ref _resolutionWidth, Math.Clamp(width, ResolutionWidthMinimum, ResolutionWidthMaximum), nameof(ResolutionWidth));
    }

    public int ResolutionWidthMinimum => ResolutionMinimum(ScaleSourceWidth);
    public int ResolutionWidthMaximum => ResolutionMaximum(ScaleSourceWidth);
    public int ResolutionHeightMinimum => ResolutionMinimum(ScaleSourceHeight);
    public int ResolutionHeightMaximum => ResolutionMaximum(ScaleSourceHeight);
    public int ResolutionStep => FFProbePixelFormatRules.GetResolutionScaleStep(_colorSpaceAnalysis.PixelFormat);
    public int ScaleStep => ResolutionStep;

    public List<string> ResolutionWidthTickLabels =>
        ResolutionScale.GenerateHeightTickLabels(ResolutionWidthMinimum, ResolutionWidthMaximum, 5);
    public List<string> ResolutionHeightTickLabels =>
        ResolutionScale.GenerateHeightTickLabels(ResolutionHeightMinimum, ResolutionHeightMaximum, 5);
    public List<string> ScaleTickLabels => ResolutionHeightTickLabels;

    private int ResolutionMinimum(int sourceDimension) => IsResolutionUpscale
        ? ResolutionScale.EnsureValid(sourceDimension)
        : ResolutionScale.MinimumTargetHeight;

    private int ResolutionMaximum(int sourceDimension) => !HasSource
        ? ResolutionScale.MinimumTargetHeight
        : IsResolutionUpscale
            ? ResolutionScale.EnsureValid((int)Math.Min(int.MaxValue, (long)sourceDimension * 4))
            : ResolutionScale.EnsureValid(sourceDimension);

    public string ResolutionTargetDisplay => !HasSource ? "--" : $"{ResolutionWidth}x{ResolutionHeight}";
    public string UpscaleTargetDisplay => ResolutionTargetDisplay;

    private bool HasResolutionFilter => HasSource
        && (ResolutionWidth != ScaleSourceWidth || ResolutionHeight != ScaleSourceHeight);

    private bool HasScaleFilter => HasResolutionFilter;
    private bool HasUpscaleFilter => IsResolutionUpscale && HasResolutionFilter;
    public bool HasUpscaleOutput => HasUpscaleFilter;
    public int UpscaleTargetWidth => HasUpscaleFilter ? ResolutionWidth : 0;
    public int UpscaleTargetHeight => HasUpscaleFilter ? ResolutionHeight : 0;

    // These aliases keep the source reviser and existing filter-chain callers on the unified target.
    public int TargetWidth => ResolutionWidth;
    public int TargetHeight => ResolutionHeight;
    public string TargetDisplay => ResolutionTargetDisplay;

    public void CommitScale()
    {
        if (!IsScaleApplicable) return;
        RecomputeTarget();
        NotifyScaleFilterProperties();
    }

    private void ResetResolutionTarget()
    {
        int width = IsResolutionUpscale
            ? ResolutionWidthMinimum
            : ResolutionWidthMaximum;
        int height = IsResolutionUpscale
            ? ResolutionHeightMinimum
            : ResolutionHeightMaximum;

        _resolutionWidth = width;
        _resolutionHeight = height;
        if (IsResolutionAspectLocked && HasSource)
            SetResolutionWidthFromHeight(height);

        OnPropertyChanged(nameof(ResolutionWidth));
        OnPropertyChanged(nameof(ResolutionHeight));
        NotifyResolutionFilterProperties();
    }

    private FFProbeAspectRatio _sourceAspectRatio = FFProbeAspectRatioResolver.Resolve((string?)null);
    private string SourceDar => _sourceAspectRatio.Dar.ToString();
    private string SourceSar => _sourceAspectRatio.Sar.ToString();

    private bool HasFpsFilter => IsFrameRateApplicable;

    private bool HasSarRepairFilter => _hasSarRepairWarning();

    private bool HasColorSpaceFilter =>
        !_hasSourceValidationError()
        && _colorSpaceAnalysis.IsApplicable
        && !string.IsNullOrWhiteSpace(_colorSpaceAnalysis.FFmpegColorFilter);

    private bool RequiresManualColorSpacePeakNits =>
        _colorSpaceAnalysis.Strategy is ColorSpaceStrategy.HdrToSdr
            or ColorSpaceStrategy.HighHdrToSdr
            or ColorSpaceStrategy.HlgToSdr
            or ColorSpaceStrategy.DoviHdrToSdr;

    public string ColorSpacePeakNits
    {
        get => _colorSpacePeakNits;
        set
        {
            if (!SetProperty(ref _colorSpacePeakNits, value)) return;

            OnPropertyChanged(nameof(HasColorSpacePeakNits));
            RefreshColorSpaceFilters();
        }
    }

    public bool HasColorSpacePeakNits =>
        double.TryParse(ColorSpacePeakNits, NumberStyles.Float, CultureInfo.InvariantCulture, out double peakNits)
        && double.IsFinite(peakNits)
        && peakNits > 0;

    public bool IsColorSpacePeakNitsVisible => RequiresManualColorSpacePeakNits;

    private static bool RequiresColorSpacePeakNits(ColorSpaceStrategy strategy) =>
        strategy is ColorSpaceStrategy.HdrToSdr
            or ColorSpaceStrategy.HighHdrToSdr
            or ColorSpaceStrategy.HlgToSdr
            or ColorSpaceStrategy.DoviHdrToSdr;

    // public bool HasResizeAspectRatioLock => IsResolutionAspectLocked;

    private string? ResolutionFilterChain => HasResolutionFilter
        ? $"libplacebo=w={ResolutionWidth}:h={ResolutionHeight}{(IsResolutionAspectLocked
            ? ":force_original_aspect_ratio=decrease"
            : string.Empty)}:normalize_sar=true:upscaler=spline36:downscaler=spline36:antiringing=0.1"
        : null;

    private string? FpsFilterChain => HasFpsFilter ? $"fps={_frameRateNum}/{_frameRateDen}" : null;

    private string? SarRepairFilterChain => HasSarRepairFilter ? "libplacebo=reset_sar=1" : null;

    private string? ColorSpaceFilterChain => HasColorSpaceFilter
        ? GetColorSpaceStrategyFilterChain(_colorSpaceAnalysis.Strategy)
        : null;

    private string? CropFilterChain => HasCropFilter ? $"crop={CropWidth}:{CropHeight}:0:0" : null;

    private bool IsColorSpaceStrategyShown(ColorSpaceStrategy strategy) =>
        !_hasSourceValidationError()
        && ColorSpaceConverter.IsStrategyApplicable(
            strategy,
            _colorSpaceAnalysis.ColorPrimaries,
            _colorSpaceAnalysis.ColorTransfer,
            _colorSpaceAnalysis.HasDolbyVision)
        && (!string.IsNullOrWhiteSpace(BuildColorSpaceStrategyFilterChain(strategy))
            || !string.IsNullOrWhiteSpace(ColorSpaceConverter.BuildVapourSynthFilter(
                strategy,
                _colorSpaceAnalysis.ColorTransfer))
            || IsUsableColorSpaceFilter(ColorSpaceConverter.BuildAviSynthFilter(
                strategy,
                _colorSpaceAnalysis.ColorTransfer,
                _colorSpaceAnalysis.ColorPrimaries,
                _colorSpaceAnalysis.ColorMatrix,
                _colorSpaceAnalysis.FrameRate)));

    private static bool IsUsableColorSpaceFilter(string? filter) =>
        !string.IsNullOrWhiteSpace(filter)
        && !filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal);

    public string FFmpegResizeFilter =>
        ResolutionFilterChain is string filter
            ? $"-filter:v \"{filter}\""
            : LangProviderBase.NAText;

    public string FFmpegResizeFilterDisplay =>
        ResolutionFilterChain ?? LangProviderBase.NAText;

    public string FFmpegCropFilter =>
        HasCropFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: true, includeCsp709Flags: false, CropFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegCropFilterDisplay =>
        CropFilterChain ?? LangProviderBase.NAText;

    public string FFmpegFpsFilter =>
        HasFpsFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: false, FpsFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegFpsFilterDisplay =>
        FpsFilterChain ?? LangProviderBase.NAText;

    public string FFmpegSarRepairFilter =>
        HasSarRepairFilter
            ? "-filter:v \"libplacebo=reset_sar=1\""
            : LangProviderBase.NAText;

    public string FFmpegSarRepairFilterDisplay =>
        SarRepairFilterChain ?? LangProviderBase.NAText;

    public static string FFmpegDebandFilter =>
        "-filter:v \"libplacebo=deband=true:deband_iterations=3:deband_radius=8:deband_threshold=6\"";

    public static string FFmpegDebandFilterDisplay =>
        "libplacebo=deband=true:deband_iterations=3:deband_radius=8:deband_threshold=6";

    public string FFmpegRotateFilter =>
        RotateMode > 0
            ? $"-filter:v \"libplacebo=rotate={RotateMode}\""
            : LangProviderBase.NAText;

    public string FFmpegRotateFilterDisplay =>
        RotateMode > 0 ? $"libplacebo=rotate={RotateMode}" : LangProviderBase.NAText;

    public string AvsRotateFilter =>
        RotateMode switch
        {
            1 => "TurnRight(src)",
            2 => "Turn180(src)",
            3 => "TurnLeft(src)",
            _ => LangProviderBase.NAText
        };

    public string AvsRotateFilterDisplay => AvsRotateFilter;

    public string VpyRotateFilter =>
        RotateMode switch
        {
            1 => "src = core.std.FlipHorizontal(core.std.Transpose(src))",
            2 => "src = core.std.Turn180(src)",
            3 => "src = core.std.FlipVertical(core.std.Transpose(src))",
            _ => LangProviderBase.NAText
        };

    public string VpyRotateFilterDisplay => VpyRotateFilter;

    public string FFmpegUpscaleFilter => HasUpscaleFilter ? FFmpegResizeFilter : LangProviderBase.NAText;
    public string FFmpegUpscaleFilterDisplay => HasUpscaleFilter ? FFmpegResizeFilterDisplay : LangProviderBase.NAText;

    public string FFmpegFlipFilter
    {
        get
        {
            if (!HorizontalFlipEnabled && !VerticalFlipEnabled)
                return LangProviderBase.NAText;

            string chain = HorizontalFlipEnabled && VerticalFlipEnabled
                ? "hflip,vflip"
                : HorizontalFlipEnabled ? "hflip" : "vflip";
            return $"-filter:v \"{chain}\"";
        }
    }

    public string FFmpegFlipFilterDisplay =>
        !HorizontalFlipEnabled && !VerticalFlipEnabled
            ? LangProviderBase.NAText
            : HorizontalFlipEnabled && VerticalFlipEnabled
                ? "hflip,vflip"
                : HorizontalFlipEnabled ? "hflip" : "vflip";

    public static string AvsHFlipFilter => "FlipHorizontal(src)";
    public static string AvsVFlipFilter => "FlipVertical(src)";
    public static string VpyHFlipFilter => "src = core.std.FlipHorizontal(src)";
    public static string VpyVFlipFilter => "src = core.std.FlipVertical(src)";

    private void NotifyRotateFilterProperties()
    {
        NotifyProperties(
            nameof(RotateDisplay),
            nameof(FFmpegRotateFilter),
            nameof(FFmpegRotateFilterDisplay),
            nameof(CanInsertFFmpegRotateFilter),
            nameof(AvsRotateFilter),
            nameof(AvsRotateFilterDisplay),
            nameof(CanInsertAvsRotateFilter),
            nameof(VpyRotateFilter),
            nameof(VpyRotateFilterDisplay),
            nameof(CanInsertVpyRotateFilter),
            nameof(SelectedRotateFilterDisplay));
    }

    private void NotifyFlipFilterProperties()
    {
        NotifyProperties(
            nameof(FFmpegFlipFilter),
            nameof(FFmpegFlipFilterDisplay),
            nameof(CanInsertFFmpegFlipFilter));
    }

    public static bool DebandEnabled => true;

    private int _rotateMode;
    public int RotateMode
    {
        get => _rotateMode;
        set
        {
            int clamped = Math.Clamp(value, 0, 3);
            if (SetProperty(ref _rotateMode, clamped))
                NotifyRotateFilterProperties();
        }
    }

    public static List<string> RotateTickLabels => ["0", "1", "2", "3"];
    public string RotateDisplay => (RotateMode * 90).ToString(CultureInfo.InvariantCulture);

    private bool _horizontalFlipEnabled;
    public bool HorizontalFlipEnabled
    {
        get => _horizontalFlipEnabled;
        set
        {
            if (SetProperty(ref _horizontalFlipEnabled, value))
                NotifyFlipFilterProperties();
        }
    }

    private bool _verticalFlipEnabled;
    public bool VerticalFlipEnabled
    {
        get => _verticalFlipEnabled;
        set
        {
            if (SetProperty(ref _verticalFlipEnabled, value))
                NotifyFlipFilterProperties();
        }
    }

    public static bool CanInsertFFmpegDebandFilter => DebandEnabled;
    public bool CanInsertFFmpegRotateFilter => RotateMode > 0;
    public bool CanInsertFFmpegUpscaleFilter => HasUpscaleOutput;
    public bool CanInsertFFmpegFlipFilter => HorizontalFlipEnabled || VerticalFlipEnabled;

    public bool CanInsertAvsRotateFilter => RotateMode > 0;
    public bool CanInsertVpyRotateFilter => RotateMode > 0;

    public static bool CanInsertAvsHFlipFilter => true;
    public static bool CanInsertAvsVFlipFilter => true;
    public static bool CanInsertVpyHFlipFilter => true;
    public static bool CanInsertVpyVFlipFilter => true;

    public static string AviSynthHqdn3dDenoiseFilter => "hqdn3d(src)";
    public static string FFmpegHqdn3dDenoiseFilter => "-filter:v \"hqdn3d\"";
    public static string FFmpegHqdn3dDenoiseFilterDisplay => "hqdn3d";
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
    public string VSZipCLFilter
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

            string pluginsDir = BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins");
            string vszipclDllPath = Path.Combine(pluginsDir, "vszipcl.dll");
            string fmtconvDllPath = Path.Combine(pluginsDir, "fmtconv.dll");

            string loadVszipcl = $"core.std.LoadPlugin(r\"{vszipclDllPath}\")";
            int deviceId = int.TryParse(_vszipclDeviceId, out int parsed) && parsed >= 0 ? parsed : 0;
            string vszipclCalls =
                $"src = core.vszipcl.Deband(src, dither_algo=0, device_id={deviceId}, num_streams=2)\r\n" +
                $"src = core.vszipcl.NLMeans(src, d=1, a=2, s=4, h=1.2, wmode=0, wref=1.0, device_id={deviceId}, num_streams=2)\r\n" +
                $"src = core.vszipcl.GaussBlur(src, device_id={deviceId}, num_streams=2)";
            string convIn = targetBpp == 0
                ? ""
                : $"core.std.LoadPlugin(r\"{fmtconvDllPath}\")\r\n" +
                $"src = core.fmtc.bitdepth(src, bits={targetBpp})\r\n";
            string convOut = targetBpp == 0
                ? ""
                : $"src = core.fmtc.bitdepth(src, bits={_sourceBitDepth})\r\n";


            return $"{loadVszipcl}\r\n{convIn}{vszipclCalls}\r\n{convOut}".TrimEnd();
        }
    }

    public ObservableCollection<AppConfItem> VSZipCLSettingsListing { get; } = [];

    public string VSZipCLDeviceId
    {
        get => _vszipclDeviceId;
        set
        {
            if (!SetProperty(ref _vszipclDeviceId, value)) return;
            OnPropertyChanged(nameof(VSZipCLFilter));
        }
    }

    public static string VSZipCLTitle =>
        FilterScribeModalLangProvider.Current["SrcScribe.VszipclTitle"];
    public static string VSZipCLPreviewHint =>
        FilterScribeModalLangProvider.Current["SrcScribe.VszipclPreviewHint"];
    public static string VSZipCLDeviceHint =>
        FilterScribeModalLangProvider.Current["SrcScribe.VszipclDeviceHint"];
    public bool VSZipCLHasFmtconv =>
        _sourceBitDepth != 8 && _sourceBitDepth != 16 && _sourceBitDepth != 32;
    public string VSZipCLFmtconvHint =>
        string.Format(FilterScribeModalLangProvider.Current["SrcScribe.VszipclFmtconvHint"], _sourceBitDepth);

    public static string BlurSharpenTitle =>
        FilterScribeModalLangProvider.Current["SrcScribe.BlurSharpenTitle"];
    public static string AviSynthBlurFilter => "Blur(1.0, 1.0)";
    public static string VapourSynthBlurFilter => "src = core.std.BoxBlur(src, hradius=1, vradius=1, hpasses=1, vpasses=1)";
    public static string AviSynthAsharpFilter
    {
        get
        {
            string pluginsDir = Environment.Is64BitProcess
                ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
                : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
            string asharpPath = Path.Combine(pluginsDir, "ASharp.dll");
            return $"LoadPlugin(\"{asharpPath}\")\r\nASharp(T=2.0, D=4.0, B=2.0, hqbf=true)";
        }
    }

    public static string VapourSynthASharpFilter
    {
        get
        {
            string pluginsDir = Environment.Is64BitProcess
                ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
                : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
            string asharpPath = Path.Combine(pluginsDir, "libasharp.dll");

            return $"core.std.LoadPlugin(r\"{asharpPath}\")\r\n" +
                   "src = core.asharp.ASharp(src, t=2.0, d=4.0, b=2.0, hqbf=true)"; // lowercase
        }
    }
    public static string AviSynthBlurSharpenFilter =>
        $"{AviSynthBlurFilter}\r\n{AviSynthAsharpFilter}";
    public static string VapourSynthBlurSharpenFilter =>
        $"{VapourSynthBlurFilter}\r\n{VapourSynthASharpFilter}";

    public static string FFmpegSubtitleFilter =>
        "-filter_complex \"ass='X\\:/path/to/subtitle.ass':fontsdir='Y\\:/dir/of/fonts'\"";

    private static string FormatAviSynthAssRenderDimension(int value, string name) =>
        value > 0 ? value.ToString() : $"<ffprobe {name}>";

    public string FFmpegFpsScaleFilter =>
        HasFpsFilter && HasScaleFilter
            ? BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: false, FpsFilterChain, ResolutionFilterChain)
            : LangProviderBase.NAText;

    public string FFmpegLowToHighColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.LowToHigh);
    public string FFmpegLowToHighColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.LowToHigh) ?? LangProviderBase.NAText;
    public string FFmpegHighToLowColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.HighToLow);
    public string FFmpegHighToLowColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.HighToLow) ?? LangProviderBase.NAText;
    public string FFmpegHdrToSdrColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.HdrToSdr);
    public string FFmpegHdrToSdrColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.HdrToSdr) ?? LangProviderBase.NAText;
    public string FFmpegHighHdrToLowSdrColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.HighHdrToSdr);
    public string FFmpegHighHdrToLowSdrColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.HighHdrToSdr) ?? LangProviderBase.NAText;
    public string FFmpegHlgToSdrColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.HlgToSdr);
    public string FFmpegHlgToSdrColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.HlgToSdr) ?? LangProviderBase.NAText;
    public string FFmpegDoviSdrTo709ColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.DoviSdrTo709);
    public string FFmpegDoviSdrTo709ColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.DoviSdrTo709) ?? LangProviderBase.NAText;
    public string FFmpegDoviHdrToSdrColorFilter =>
        GetColorSpaceStrategyFilter(ColorSpaceStrategy.DoviHdrToSdr);
    public string FFmpegDoviHdrToSdrColorFilterDisplay =>
        GetColorSpaceStrategyFilterChain(ColorSpaceStrategy.DoviHdrToSdr) ?? LangProviderBase.NAText;

    // VapourSynth color space filters
    public string VapourSynthLowToHighColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.LowToHigh) ?? LangProviderBase.NAText;
    public string VapourSynthHighToLowColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.HighToLow) ?? LangProviderBase.NAText;
    public string VapourSynthHdrToSdrColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.HdrToSdr) ?? LangProviderBase.NAText;
    public string VapourSynthHighHdrToLowSdrColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.HighHdrToSdr) ?? LangProviderBase.NAText;
    public string VapourSynthHlgToSdrColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.HlgToSdr) ?? LangProviderBase.NAText;
    public string VapourSynthDoviSdrTo709ColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.DoviSdrTo709) ?? LangProviderBase.NAText;
    public string VapourSynthDoviHdrToSdrColorFilterDisplay =>
        GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy.DoviHdrToSdr) ?? LangProviderBase.NAText;

    // AviSynth color space filters
    public string AviSynthLowToHighColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.LowToHigh) ?? LangProviderBase.NAText;
    public string AviSynthHighToLowColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.HighToLow) ?? LangProviderBase.NAText;
    public string AviSynthHdrToSdrColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.HdrToSdr) ?? LangProviderBase.NAText;
    public string AviSynthHighHdrToLowSdrColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.HighHdrToSdr) ?? LangProviderBase.NAText;
    public string AviSynthHlgToSdrColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.HlgToSdr) ?? LangProviderBase.NAText;
    public string AviSynthDoviSdrTo709ColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.DoviSdrTo709) ?? LangProviderBase.NAText;
    public string AviSynthDoviHdrToSdrColorFilterDisplay =>
        GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy.DoviHdrToSdr) ?? LangProviderBase.NAText;
    public string AviSynthChroma422Filter => BuildAviSynthChromaFilter("422") ?? LangProviderBase.NAText;
    public string AviSynthChroma420Filter => BuildAviSynthChromaFilter("420") ?? LangProviderBase.NAText;
    public string VapourSynthChroma422Filter => BuildVapourSynthChromaFilter("422") ?? LangProviderBase.NAText;
    public string VapourSynthChroma420Filter => BuildVapourSynthChromaFilter("420") ?? LangProviderBase.NAText;
    public string FFmpegChroma422Filter => BuildFFmpegChromaFilter(allowYuv422Source: false) ?? LangProviderBase.NAText;
    public string FFmpegChroma420Filter => BuildFFmpegChromaFilter(allowYuv422Source: true) ?? LangProviderBase.NAText;

    // LoadPlugin commands for color space filters
    public static string VapourSynthPlaceboLoadCommand => BuildVapourSynthPlaceboLoadCommand();
    public static string AviSynthPlaceboLoadCommand => BuildAviSynthPlaceboLoadCommand();

    public bool CanInsertAviSynthChroma422Filter => CanUseChromaSubsampling(ChromaSubsampling.Yuv444);
    public bool CanInsertAviSynthChroma420Filter => CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
        || CanUseChromaSubsampling(ChromaSubsampling.Yuv422, requiresInputLocation: true);
    public bool CanInsertVapourSynthChroma422Filter => CanUseChromaSubsampling(ChromaSubsampling.Yuv444);
    public bool CanInsertVapourSynthChroma420Filter => CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
        || CanUseChromaSubsampling(ChromaSubsampling.Yuv422, requiresInputLocation: true);
    public bool CanInsertFFmpegChroma422Filter => CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
        && FFProbePixelFormatRules.GetYuv420PixelFormat(_sourceBitDepth) != null;
    public bool CanInsertFFmpegChroma420Filter => (CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
        || CanUseChromaSubsampling(ChromaSubsampling.Yuv422, requiresInputLocation: true))
        && FFProbePixelFormatRules.GetYuv420PixelFormat(_sourceBitDepth) != null;

    private ChromaSubsampling SourceChromaSubsampling =>
        FFProbePixelFormatRules.GetChromaSubsampling(_colorSpaceAnalysis.PixelFormat);

    private string? SourceChromaLocation => NormalizeChromaLocation(_colorSpaceAnalysis.ColorChromaLocation);

    private bool CanUseChromaSubsampling(ChromaSubsampling source) =>
        !_hasSourceValidationError()
        && SourceChromaSubsampling == source;

    private bool CanUseChromaSubsampling(ChromaSubsampling source, bool requiresInputLocation) =>
        CanUseChromaSubsampling(source)
        && (!requiresInputLocation || SourceChromaLocation != null);

    private string? BuildAviSynthChromaFilter(string target)
    {
        ChromaSubsampling source = SourceChromaSubsampling;
        bool canConvert = target == "422"
            ? CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
            : CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
                || CanUseChromaSubsampling(ChromaSubsampling.Yuv422, requiresInputLocation: true);
        if (!canConvert) return null;

        string inputPlacement = source == ChromaSubsampling.Yuv444
            ? string.Empty
            : $"ChromaInPlacement=\"{SourceChromaLocation}\", ";
        return $"ConvertToYUV{target}({inputPlacement}ChromaOutPlacement=\"left\", Chromaresample=\"spline36\")";
    }

    private string? BuildVapourSynthChromaFilter(string target)
    {
        ChromaSubsampling source = SourceChromaSubsampling;
        bool canConvert = target == "422"
            ? CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
            : CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
                || CanUseChromaSubsampling(ChromaSubsampling.Yuv422, requiresInputLocation: true);
        if (!canConvert) return null;

        string inputPlacement = source == ChromaSubsampling.Yuv444
            ? string.Empty
            : $", cplace_in=\"{SourceChromaLocation}\"";
        string pluginsDir = Environment.Is64BitProcess
            ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
            : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
        string fmtconvPath = Path.Combine(pluginsDir, "fmtconv.dll");
        return $"core.std.LoadPlugin(r\"{fmtconvPath}\")\r\n" +
               $"src = core.fmtc.resample(src, css=\"{target}\", kernel=\"spline36\", cplace=\"left\"{inputPlacement})";
    }

    /// <summary>
    /// Create ffmpeg YUV444→YUV422, YUV444→YUV420, YUV422→YUV420 filter string
    /// </summary>
    /// <param name="allowYuv422Source">
    /// Block if the input chroma location is unknown
    /// </param>
    /// <remarks>libplacebo (is not designed) to handle chroma resizing</remarks>
    /// <returns></returns>
    private string? BuildFFmpegChromaFilter(bool allowYuv422Source)
    {
        if (!CanUseChromaSubsampling(ChromaSubsampling.Yuv444)
            && (!allowYuv422Source || !CanUseChromaSubsampling(ChromaSubsampling.Yuv422)))
            return null;
        // Mandatory blocking since the source cannot be converted accurately, applies to FFmpeg-AVS-VS
        if (SourceChromaSubsampling == ChromaSubsampling.Yuv422 && SourceChromaLocation == null)
            return null;

        string? outputFormat = FFProbePixelFormatRules.GetYuv420PixelFormat(_sourceBitDepth);
        if (outputFormat == null) return null;

        string chromaInPart = SourceChromaLocation != null
            ? $":in_chroma_loc={SourceChromaLocation}"
            : string.Empty;
        string filter = $"scale=flags=spline+accurate_rnd+full_chroma_int{chromaInPart}:out_chroma_loc=left";
        string args = BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: false, filter);
        return $"{args} -pix_fmt {outputFormat}";
    }

    private static string? NormalizeChromaLocation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        return value.Trim().ToLowerInvariant() switch
        {
            "left" => "left",
            "center" => "center",
            "topleft" or "top_left" => "top_left",
            "top" => "top",
            "bottomleft" or "bottom_left" => "bottom_left",
            "bottom" => "bottom",
            "dv" => "dv",
            _ => null
        };
    }

    public string FFmpegFpsColorScaleFilter
    {
        get
        {
            string? color = ColorSpaceFilterChain;
            string? fps = FpsFilterChain;
            string? scale = ResolutionFilterChain;
            if (color == null || fps == null || scale == null) return LangProviderBase.NAText;
            return BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: color != null, fps, color, scale);
        }
    }

    public string FFmpegFullChainFilter
    {
        get
        {
            string? sar = SarRepairFilterChain;
            string? color = ColorSpaceFilterChain;
            string? fps = FpsFilterChain;
            string? scale = ResolutionFilterChain;
            if (sar == null || color == null || fps == null || scale == null) return LangProviderBase.NAText;
            return BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: color != null, fps, sar, color, scale);
        }
    }

    public string FFmpegHqdn3dFullChainFilter
    {
        get
        {
            string? sar = SarRepairFilterChain;
            string? color = ColorSpaceFilterChain;
            string? fps = FpsFilterChain;
            string? scale = ResolutionFilterChain;
            if (sar == null || color == null || fps == null || scale == null) return LangProviderBase.NAText;
            return BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: color != null, "hqdn3d", fps, sar, color, scale);
        }
    }

    public bool CanInsertFFmpegFpsFilter => HasFpsFilter;
    public bool CanInsertFFmpegSarRepairFilter => HasSarRepairFilter;
    public bool CanInsertFFmpegResizeFilter => HasScaleFilter;
    public bool CanInsertFFmpegLowToHighColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.LowToHigh);
    public bool CanInsertFFmpegHighToLowColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.HighToLow);
    public bool CanInsertFFmpegHdrToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HdrToSdr);
    public bool CanInsertFFmpegHighHdrToLowSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HighHdrToSdr);
    public bool CanInsertFFmpegHlgToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HlgToSdr);
    public bool CanInsertFFmpegDoviSdrTo709ColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.DoviSdrTo709);
    public bool CanInsertFFmpegDoviHdrToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.DoviHdrToSdr);

    public bool CanInsertVapourSynthLowToHighColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.LowToHigh);
    public bool CanInsertVapourSynthHighToLowColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.HighToLow);
    public bool CanInsertVapourSynthHdrToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HdrToSdr) && HasColorSpacePeakNits;
    public bool CanInsertVapourSynthHighHdrToLowSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HighHdrToSdr) && HasColorSpacePeakNits;
    public bool CanInsertVapourSynthHlgToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HlgToSdr) && HasColorSpacePeakNits;
    public bool CanInsertVapourSynthDoviSdrTo709ColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.DoviSdrTo709);
    public bool CanInsertVapourSynthDoviHdrToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.DoviHdrToSdr) && HasColorSpacePeakNits;

    public bool CanInsertAviSynthLowToHighColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.LowToHigh);
    public bool CanInsertAviSynthHighToLowColorFilter => IsColorSpaceStrategyShown(ColorSpaceStrategy.HighToLow);
    public bool CanInsertAviSynthHdrToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HdrToSdr) && HasColorSpacePeakNits;
    public bool CanInsertAviSynthHighHdrToLowSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HighHdrToSdr) && HasColorSpacePeakNits;
    public bool CanInsertAviSynthHlgToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.HlgToSdr) && HasColorSpacePeakNits;
    public bool CanInsertAviSynthDoviSdrTo709ColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.DoviSdrTo709);
    public bool CanInsertAviSynthDoviHdrToSdrColorFilter =>
        IsColorSpaceStrategyShown(ColorSpaceStrategy.DoviHdrToSdr) && HasColorSpacePeakNits;

    private string GetColorSpaceStrategyFilter(ColorSpaceStrategy strategy) =>
        IsColorSpaceStrategyShown(strategy)
            ? BuildFFmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: true, BuildColorSpaceStrategyFilterChain(strategy))
            : LangProviderBase.NAText;

    private string? GetColorSpaceStrategyFilterChain(ColorSpaceStrategy strategy) =>
        IsColorSpaceStrategyShown(strategy)
            ? BuildColorSpaceStrategyFilterChain(strategy)
            : null;

    private static string? BuildColorSpaceStrategyFilterChain(ColorSpaceStrategy strategy) =>
        ColorSpaceConverter.BuildFFmpegFilter(strategy);

    private string? GetVapourSynthColorSpaceFilterChain(ColorSpaceStrategy strategy)
    {
        if (!IsColorSpaceStrategyShown(strategy)) return null;

        string? filter = ColorSpaceConverter.BuildVapourSynthFilter(
            strategy,
            _colorSpaceAnalysis.ColorTransfer);

        if (filter == null) return null;

        filter = ReplaceColorSpacePeakNits(filter, strategy);
        int outputBitDepth = GetPlaceboOutputBitDepth();
        return $"src = core.fmtc.bitdepth(src, bits=16)\r\n{filter}\r\nsrc = core.fmtc.bitdepth(src, bits={outputBitDepth})";
    }

    private string? GetAviSynthColorSpaceFilterChain(ColorSpaceStrategy strategy)
    {
        if (!IsColorSpaceStrategyShown(strategy)) return null;

        string? filter = ColorSpaceConverter.BuildAviSynthFilter(
            strategy,
            _colorSpaceAnalysis.ColorTransfer,
            _colorSpaceAnalysis.ColorPrimaries,
            _colorSpaceAnalysis.ColorMatrix,
            _colorSpaceAnalysis.FrameRate);

        if (filter == null) return null;

        // Undeterminable Bt601 standard: report "N/A" as-is, without the bit depth wrapper
        if (filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal)) return filter;

        filter = ReplaceColorSpacePeakNits(filter, strategy);
        int outputBitDepth = GetPlaceboOutputBitDepth();
        // Warning: AviSynth does not use "src =" pattern, it is VapourSynth specific
        return $"fmtc_bitdepth(bits=16)\r\n{filter}\r\nfmtc_bitdepth(bits={outputBitDepth})";
    }

    private string ReplaceColorSpacePeakNits(string filter, ColorSpaceStrategy strategy)
    {
        if (!RequiresColorSpacePeakNits(strategy)
            || !HasColorSpacePeakNits
            || !double.TryParse(ColorSpacePeakNits, NumberStyles.Float, CultureInfo.InvariantCulture, out double peakNits))
            return filter;

        return filter.Replace("<nits>", peakNits.ToString("G", CultureInfo.InvariantCulture));
    }

    private int GetPlaceboOutputBitDepth() =>
        _sourceBitDepth is 8 or 10 or 12 or 14 or 16 or 32 ? _sourceBitDepth : 16;

    private static string BuildVapourSynthPlaceboLoadCommand()
    {
        string pluginsDir = Environment.Is64BitProcess
            ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
            : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
        string placeboPath = Path.Combine(pluginsDir, "libvs_placebo.dll");
        string fmtconvPath = Path.Combine(pluginsDir, "fmtconv.dll");
        return $"core.std.LoadPlugin(r\"{placeboPath}\")\r\n" +
               $"core.std.LoadPlugin(r\"{fmtconvPath}\")";
    }

    private static string BuildAviSynthPlaceboLoadCommand()
    {
        string pluginsDir = Environment.Is64BitProcess
            ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
            : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
        string placeboPath = Path.Combine(pluginsDir, "libplacebo_Render.dll");
        string fmtconvPath = Path.Combine(pluginsDir, "fmtconv.dll");
        return $"LoadPlugin(\"{placeboPath}\")\r\n" +
               $"LoadPlugin(\"{fmtconvPath}\")";
    }

    private void RefreshColorSpaceFilters()
    {
        NotifyProperties(
            nameof(ColorSpaceFilterChain),
            nameof(FFmpegLowToHighColorFilter),
            nameof(FFmpegLowToHighColorFilterDisplay),
            nameof(FFmpegHighToLowColorFilter),
            nameof(FFmpegHighToLowColorFilterDisplay),
            nameof(FFmpegHdrToSdrColorFilter),
            nameof(FFmpegHdrToSdrColorFilterDisplay),
            nameof(FFmpegHighHdrToLowSdrColorFilter),
            nameof(FFmpegHighHdrToLowSdrColorFilterDisplay),
            nameof(FFmpegHlgToSdrColorFilter),
            nameof(FFmpegHlgToSdrColorFilterDisplay),
            nameof(FFmpegDoviSdrTo709ColorFilter),
            nameof(FFmpegDoviSdrTo709ColorFilterDisplay),
            nameof(FFmpegDoviHdrToSdrColorFilter),
            nameof(FFmpegDoviHdrToSdrColorFilterDisplay),
            nameof(VapourSynthLowToHighColorFilterDisplay),
            nameof(VapourSynthHighToLowColorFilterDisplay),
            nameof(VapourSynthHdrToSdrColorFilterDisplay),
            nameof(VapourSynthHighHdrToLowSdrColorFilterDisplay),
            nameof(VapourSynthHlgToSdrColorFilterDisplay),
            nameof(VapourSynthDoviSdrTo709ColorFilterDisplay),
            nameof(VapourSynthDoviHdrToSdrColorFilterDisplay),
            nameof(AviSynthLowToHighColorFilterDisplay),
            nameof(AviSynthHighToLowColorFilterDisplay),
            nameof(AviSynthHdrToSdrColorFilterDisplay),
            nameof(AviSynthHighHdrToLowSdrColorFilterDisplay),
            nameof(AviSynthHlgToSdrColorFilterDisplay),
            nameof(AviSynthDoviSdrTo709ColorFilterDisplay),
            nameof(AviSynthDoviHdrToSdrColorFilterDisplay),
            nameof(SelectedLowToHighColorFilterDisplay),
            nameof(SelectedHighToLowColorFilterDisplay),
            nameof(SelectedHdrToSdrColorFilterDisplay),
            nameof(SelectedHighHdrToLowSdrColorFilterDisplay),
            nameof(SelectedHlgToSdrColorFilterDisplay),
            nameof(SelectedDoviSdrTo709ColorFilterDisplay),
            nameof(SelectedDoviHdrToSdrColorFilterDisplay),
            nameof(VapourSynthPlaceboLoadCommand),
            nameof(AviSynthPlaceboLoadCommand),
            nameof(AviSynthChroma422Filter),
            nameof(AviSynthChroma420Filter),
            nameof(VapourSynthChroma422Filter),
            nameof(VapourSynthChroma420Filter),
            nameof(FFmpegChroma422Filter),
            nameof(FFmpegChroma420Filter),
            nameof(SelectedChroma422FilterDisplay),
            nameof(SelectedChroma420FilterDisplay),
            nameof(CanInsertAviSynthChroma422Filter),
            nameof(CanInsertAviSynthChroma420Filter),
            nameof(CanInsertVapourSynthChroma422Filter),
            nameof(CanInsertVapourSynthChroma420Filter),
            nameof(CanInsertFFmpegChroma422Filter),
            nameof(CanInsertFFmpegChroma420Filter),
            nameof(FFmpegFpsColorScaleFilter),
            nameof(FFmpegFullChainFilter),
            nameof(FFmpegHqdn3dFullChainFilter),
            nameof(CanInsertFFmpegLowToHighColorFilter),
            nameof(CanInsertFFmpegHighToLowColorFilter),
            nameof(CanInsertFFmpegHdrToSdrColorFilter),
            nameof(CanInsertFFmpegHighHdrToLowSdrColorFilter),
            nameof(CanInsertFFmpegHlgToSdrColorFilter),
            nameof(CanInsertFFmpegDoviSdrTo709ColorFilter),
            nameof(CanInsertFFmpegDoviHdrToSdrColorFilter),
            nameof(CanInsertVapourSynthLowToHighColorFilter),
            nameof(CanInsertVapourSynthHighToLowColorFilter),
            nameof(CanInsertVapourSynthHdrToSdrColorFilter),
            nameof(CanInsertVapourSynthHighHdrToLowSdrColorFilter),
            nameof(CanInsertVapourSynthHlgToSdrColorFilter),
            nameof(CanInsertVapourSynthDoviSdrTo709ColorFilter),
            nameof(CanInsertVapourSynthDoviHdrToSdrColorFilter),
            nameof(CanInsertAviSynthLowToHighColorFilter),
            nameof(CanInsertAviSynthHighToLowColorFilter),
            nameof(CanInsertAviSynthHdrToSdrColorFilter),
            nameof(CanInsertAviSynthHighHdrToLowSdrColorFilter),
            nameof(CanInsertAviSynthHlgToSdrColorFilter),
            nameof(CanInsertAviSynthDoviSdrTo709ColorFilter),
            nameof(CanInsertAviSynthDoviHdrToSdrColorFilter));
    }

    private string BuildFFmpegFilterArgs(bool includeSwsFlags, bool includeCsp709Flags, params string?[] filters)
    {
        return FFMpegFilterArgs.Build(includeSwsFlags, includeCsp709Flags, _colorSpaceAnalysis.PixelFormat, filters);
    }

    private bool NeedsResolutionBitDepthWrapper =>
        _sourceBitDepth > 0 && _sourceBitDepth is not (8 or 16 or 32);

    private int ResolutionWorkingBitDepth => _sourceBitDepth switch
    {
        > 0 and < 8 => 8,
        > 8 and < 16 => 16,
        > 16 => 32,
        _ => _sourceBitDepth
    };

    private string BuildVapourSynthResolutionFilter()
    {
        string filter =
            $"src = core.placebo.Resample(src, width={ResolutionWidth}, height={ResolutionHeight}, filter=\"spline36\", antiring=0.1)";
        if (!NeedsResolutionBitDepthWrapper) return filter;

        return $"src = core.fmtc.bitdepth(src, bits={ResolutionWorkingBitDepth})\r\n" +
               filter +
               $"\r\nsrc = core.fmtc.bitdepth(src, bits={_sourceBitDepth})";
    }

    private string BuildAviSynthResolutionFilter()
    {
        string filter =
            $"libplacebo_Render(width={ResolutionWidth}, height={ResolutionHeight}, aspect_mode=\"fit\", upscaler=\"spline36\", downscaler=\"spline36\", antiringing_strength=0.1)";
        if (!NeedsResolutionBitDepthWrapper) return filter;

        return $"fmtc_bitdepth(bits={ResolutionWorkingBitDepth})\r\n" +
               filter +
               $"\r\nfmtc_bitdepth(bits={_sourceBitDepth})";
    }

    public string VapourSynthResolutionPlaceboLoadCommand => BuildVapourSynthResolutionPlaceboLoadCommand();
    public string AviSynthResolutionPlaceboLoadCommand => BuildAviSynthResolutionPlaceboLoadCommand();

    public string SelectedResolutionPlaceboLoadCommand =>
        GetSelectedTabFilter(AviSynthResolutionPlaceboLoadCommand, VapourSynthResolutionPlaceboLoadCommand, LangProviderBase.NAText);

    private string BuildVapourSynthResolutionPlaceboLoadCommand()
    {
        string pluginsDir = Environment.Is64BitProcess
            ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
            : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
        string placeboPath = Path.Combine(pluginsDir, "libvs_placebo.dll");
        string command = $"core.std.LoadPlugin(r\"{placeboPath}\")";
        if (NeedsResolutionBitDepthWrapper)
            command += $"\r\ncore.std.LoadPlugin(r\"{Path.Combine(pluginsDir, "fmtconv.dll")}\")";
        return command;
    }

    private string BuildAviSynthResolutionPlaceboLoadCommand()
    {
        string pluginsDir = Environment.Is64BitProcess
            ? BundledToolPathResolver.ResolveFolder("x64-AVS-VS-plugins")
            : BundledToolPathResolver.ResolveFolder("x86-AVS-VS-plugins");
        string command = $"LoadPlugin(\"{Path.Combine(pluginsDir, "libplacebo_Render.dll")}\")";
        if (NeedsResolutionBitDepthWrapper)
            command += $"\r\nLoadPlugin(\"{Path.Combine(pluginsDir, "fmtconv.dll")}\")";
        return command;
    }

    public string VapourSynthResizeFilter =>
        HasScaleFilter ? BuildVapourSynthResolutionFilter() : LangProviderBase.NAText;

    public string VapourSynthResizeFilterWithImport =>
        HasScaleFilter
            ? $"{VapourSynthResolutionPlaceboLoadCommand}\r\n{VapourSynthResizeFilter}"
            : LangProviderBase.NAText;

    public string VapourSynthCropFilter =>
        HasCropFilter
            ? $"src = core.std.CropAbs(src, {CropWidth}, {CropHeight})"
            : LangProviderBase.NAText;

    public string AviSynthResizeFilter =>
        HasScaleFilter ? BuildAviSynthResolutionFilter() : LangProviderBase.NAText;

    public string AviSynthResizeFilterWithImport =>
        HasScaleFilter
            ? $"{AviSynthResolutionPlaceboLoadCommand}\r\n{AviSynthResizeFilter}"
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
    public bool CanInsertVSZipCLFilter => CanUseVSZipCL;

    private bool CanUseVSZipCL =>
        LibImportProviderM.IsOpenCLAvailable
        && FFProbePixelFormatRules.IsYuvRgbOrGray(_colorSpaceAnalysis.PixelFormat);

    private void RecomputeTarget()
    {
        ResetResolutionTarget();
        NotifyScaleFilterProperties();
    }

    private void ScheduleCropRefresh()
    {
        _cropRefreshPending = true;
        _cropRefreshTimer.Stop();
        _cropRefreshTimer.Start();
    }

    private void FlushPendingCropRefresh()
    {
        if (!_cropRefreshPending)
            return;

        _cropRefreshPending = false;
        _cropRefreshTimer.Stop();
        RefreshScaleForCropChange();
    }

    private void RefreshScaleForCropChange()
    {
        OnPropertyChanged(nameof(IsScaleApplicable));
        OnPropertyChanged(nameof(ResolutionWidthMinimum));
        OnPropertyChanged(nameof(ResolutionWidthMaximum));
        OnPropertyChanged(nameof(ResolutionHeightMinimum));
        OnPropertyChanged(nameof(ResolutionHeightMaximum));
        OnPropertyChanged(nameof(ResolutionWidthTickLabels));
        OnPropertyChanged(nameof(ResolutionHeightTickLabels));
        ResetResolutionTarget();
        NotifyScaleFilterProperties();
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
        NotifyCropFilterProperties();
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
        set
        {
            if (SetProperty(ref _ffmpegFreeText, value))
                OnFilterInputChanged();
        }
    }

    public bool CanClearFilters =>
        !string.IsNullOrEmpty(_selectedTabIndex switch
        {
            0 => _avsUserInput,
            1 => _vpyUserInput,
            2 => _ffmpegFreeText,
            _ => string.Empty
        });

    public static string ClearFiltersText => UILangProvider.Current["Clear"];

    private void OnFilterInputChanged()
    {
        OnPropertyChanged(nameof(CanClearFilters));
        ClearFiltersCommand?.OnCanExecuteChanged();
    }

    private void ClearFilters()
    {
        switch (_selectedTabIndex)
        {
            case 0:
                AvsUserInput = string.Empty;
                break;
            case 1:
                VpyUserInput = string.Empty;
                break;
            case 2:
                FFmpegFreeText = string.Empty;
                break;
        }
    }

    private void AppendScriptFilter(ref string target, string? filter, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal))
            return;

        target = string.IsNullOrEmpty(target)
            ? filter
            : target.TrimEnd('\r', '\n') + "\r\n" + filter;

        OnPropertyChanged(propertyName);
        OnFilterInputChanged();
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

        if (ContainsLibplaceboFilter(generatedChain))
        {
            AppendFFmpegLibplaceboChain(generatedChain, generatedSuffix, filter);
            return;
        }

        AppendFFmpegChain(generatedChain, generatedSuffix, filter);
    }

    private void AppendFFmpegNativeFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Contains(LangProviderBase.NAText, StringComparison.Ordinal))
            return;

        if (!TrySplitVideoFilterArgs(filter, out string generatedChain, out string generatedSuffix))
        {
            AppendFFmpegFilter(filter);
            return;
        }

        if (!TrySplitVideoFilterArgs(FFmpegFreeText, out string currentChain, out string currentSuffix))
        {
            AppendFFmpegChain(generatedChain, generatedSuffix, filter);
            return;
        }

        List<string> currentFilters = SplitFilterChain(currentChain);
        int libplaceboIndex = currentFilters.FindIndex(IsLibplaceboFilter);
        if (libplaceboIndex < 0)
        {
            AppendFFmpegChain(generatedChain, generatedSuffix, filter);
            return;
        }

        currentFilters.InsertRange(libplaceboIndex, SplitFilterChain(generatedChain));
        string suffix = string.IsNullOrWhiteSpace(currentSuffix) ? generatedSuffix : currentSuffix;
        FFmpegFreeText = BuildVideoFilterArgs(string.Join(",", currentFilters), suffix);
    }

    private void AppendFFmpegLibplaceboChain(string generatedChain, string generatedSuffix, string originalFilter)
    {
        if (!TrySplitVideoFilterArgs(FFmpegFreeText, out string currentChain, out string currentSuffix))
        {
            FFmpegFreeText = string.IsNullOrWhiteSpace(FFmpegFreeText)
                ? originalFilter.Trim()
                : $"-filter:v \"{FFmpegFreeText.Trim()},{generatedChain}\"{generatedSuffix}";
            return;
        }

        List<string> filters = SplitFilterChain(currentChain);
        int libplaceboIndex = filters.FindIndex(IsLibplaceboFilter);
        if (libplaceboIndex < 0)
        {
            filters.Add(generatedChain);
        }
        else
        {
            string payload = string.Join(":", SplitFilterChain(generatedChain)
                .Where(IsLibplaceboFilter)
                .Select(GetLibplaceboPayload)
                .Where(payload => !string.IsNullOrWhiteSpace(payload)));

            if (!string.IsNullOrWhiteSpace(payload))
            {
                List<string> merged = [];
                for (int i = 0; i < filters.Count; i++)
                {
                    if (!IsLibplaceboFilter(filters[i]))
                    {
                        merged.Add(filters[i]);
                        continue;
                    }

                    if (i == libplaceboIndex)
                    {
                        string currentPayload = GetLibplaceboPayload(filters[i]);
                        merged.Add(string.IsNullOrWhiteSpace(currentPayload)
                            ? $"libplacebo={payload}"
                            : $"libplacebo={currentPayload}:{payload}");
                    }
                }
                filters = merged;
            }
        }

        string suffix = string.IsNullOrWhiteSpace(currentSuffix) ? generatedSuffix : currentSuffix;
        FFmpegFreeText = BuildVideoFilterArgs(string.Join(",", filters), suffix);
    }

    private void AppendFFmpegChain(string generatedChain, string generatedSuffix, string originalFilter)
    {
        if (!TrySplitVideoFilterArgs(FFmpegFreeText, out string currentChain, out string currentSuffix))
        {
            FFmpegFreeText = string.IsNullOrWhiteSpace(FFmpegFreeText)
                ? originalFilter.Trim()
                : $"-filter:v \"{FFmpegFreeText.Trim()},{generatedChain}\"{generatedSuffix}";
            return;
        }

        string suffix = string.IsNullOrWhiteSpace(currentSuffix) ? generatedSuffix : currentSuffix;
        FFmpegFreeText = $"-filter:v \"{currentChain},{generatedChain}\"{suffix}".Trim();
    }

    private static List<string> SplitFilterChain(string chain) =>
        [.. chain.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    private static bool ContainsLibplaceboFilter(string chain) =>
        SplitFilterChain(chain).Any(IsLibplaceboFilter);

    private static bool IsLibplaceboFilter(string filter) =>
        filter.StartsWith("libplacebo", StringComparison.OrdinalIgnoreCase)
        && (filter.Length == "libplacebo".Length || filter["libplacebo".Length] == '=');

    private static string GetLibplaceboPayload(string filter)
    {
        int equalsIndex = filter.IndexOf('=');
        return equalsIndex < 0 ? string.Empty : filter[(equalsIndex + 1)..].Trim();
    }

    private static string BuildVideoFilterArgs(string chain, string suffix) =>
        $"-filter:v \"{chain}\"{suffix}".Trim();

    private static bool TrySplitVideoFilterArgs(string? value, out string chain, out string suffix)
    {
        chain = string.Empty;
        suffix = string.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;

        int optionIndex = value.IndexOf("-filter:v", StringComparison.OrdinalIgnoreCase);
        string option = "-filter:v";
        if (optionIndex < 0)
        {
            optionIndex = value.IndexOf("-vf", StringComparison.OrdinalIgnoreCase);
            option = "-vf";
        }
        if (optionIndex < 0) return false;

        int contentStart = optionIndex + option.Length;
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

    public static string ScribeDescription => FilterScribeModalLangProvider.Current["SrcScribe.Description"];
    public static string NoteText => FilterScribeModalLangProvider.Current["SrcScribe.NoteText"];
    public static string TabAvs => LangProviderBase.AviSynth;
    public static string TabVpy => LangProviderBase.VapourSynth;
    public static string TabFFmpeg => LangProviderBase.FFmpeg;
    public static string ResizeTitle => FilterScribeModalLangProvider.Current["Resize"];
    public static string Downscale => FilterScribeModalLangProvider.Current["Downscale"];
    public string ResolutionModeLabel => IsResolutionUpscale
        ? LangProviderBase.MoveDownText
        : LangProviderBase.MoveUpText;
    public static string ResolutionLockAspectLabel => FilterScribeModalLangProvider.Current["SrcScribe.ResolutionLockAspect"];
    public static string FFmpegFreeTextHint => FilterScribeModalLangProvider.Current["SrcScribe.FFmpegFreeTextHint"];
    public static string SarRepairTitle => FilterScribeModalLangProvider.Current["SrcScribe.SarRepairTitle"];
    public static string DebandTitle => FilterScribeModalLangProvider.Current["SrcScribe.DebandTitle"];
    public static string RotateTitle => FilterScribeModalLangProvider.Current["SrcScribe.RotateTitle"];
    public static string Upscale => FilterScribeModalLangProvider.Current["Upscale"];
    public static string FlipTitle => FilterScribeModalLangProvider.Current["SrcScribe.FlipTitle"];
    public static string HorizontalFlipLabel => FilterScribeModalLangProvider.Current["SrcScribe.HorizontalFlipLabel"];
    public static string VerticalFlipLabel => FilterScribeModalLangProvider.Current["SrcScribe.VerticalFlipLabel"];
    public static string ColorSpaceConvertTitle => FilterScribeModalLangProvider.Current["SrcScribe.ColorSpaceConvertTitle"];
    public static string ChromaSubsamplingTitle => FilterScribeModalLangProvider.Current["SrcScribe.ChromaSubsamplingTitle"];
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
    public ActionCmd OpenAvsPreviewCommand { get; }
    public ActionCmd OpenFfmpegPreviewCommand { get; }
    public ActionCmd InsertAvsFilterCommand { get; }
    public ActionCmd InsertVpyFilterCommand { get; }
    public ActionCmd InsertFFmpegFilterCommand { get; }
    public ActionCmd InsertFFmpegFlipFilterCommand { get; }
    public ActionCmd InsertAvsCropFilterCommand { get; }
    public ActionCmd InsertVpyCropFilterCommand { get; }
    public ActionCmd InsertFFmpegCropFilterCommand { get; }
    public ActionCmd ClearFiltersCommand { get; }
    public bool CanOpenVpyPreview => GetVpyPreviewsrcPaths().Length > 0;
    public bool CanOpenAvsPreview => GetAvsPreviewToolPath() != null && GetVpyPreviewsrcPaths().Length > 0;
    public bool CanOpenFfmpegPreview => GetFfmpegPreviewToolPath() != null && GetVpyPreviewsrcPaths().Length > 0;

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
        Func<long>? getTotalFrames = null,
        Func<string?>? getAvs2yuvPath = null,
        Func<string?>? getAvs2pipemodPath = null,
        Func<string?>? getFfmpegPath = null)
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
        _getAvs2yuvPath = getAvs2yuvPath;
        _getAvs2pipemodPath = getAvs2pipemodPath;
        _getFfmpegPath = getFfmpegPath;
        _sourceFfprobeJson = sourceFfprobeJson;
        _baseAvsPrefix = FilterScribeModalLangProvider.Current["SrcScribe.AvsPrefix"];
        _baseVpyPrefix = FilterScribeModalLangProvider.Current["SrcScribe.VpyPrefix"];
        _hasSourceAnalysis = !string.IsNullOrWhiteSpace(sourceFfprobeJson);
        _cropRefreshTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(40)
        };
        _cropRefreshTimer.Tick += (_, _) => FlushPendingCropRefresh();
        BuildColorSpaceSettingsListing();
        BuildVSZipCLSettingsListing();
        OpenVpyPreviewCommand = new ActionCmd(_ => OpenVpyPreview(), _ => CanOpenVpyPreview);
        OpenAvsPreviewCommand = new ActionCmd(_ => OpenAvsPreview(), _ => CanOpenAvsPreview);
        OpenFfmpegPreviewCommand = new ActionCmd(_ => OpenFfmpegPreview(), _ => CanOpenFfmpegPreview);
        InsertAvsFilterCommand = new ActionCmd(filter => AppendScriptFilter(ref _avsUserInput, filter as string, nameof(AvsUserInput)));
        InsertVpyFilterCommand = new ActionCmd(filter => AppendScriptFilter(ref _vpyUserInput, filter as string, nameof(VpyUserInput)));
        InsertFFmpegFilterCommand = new ActionCmd(filter => AppendFFmpegFilter(filter as string));
        InsertFFmpegFlipFilterCommand = new ActionCmd(filter => AppendFFmpegNativeFilter(filter as string));
        InsertAvsCropFilterCommand = new ActionCmd(filter => InsertCropFilter(filter as string, value => AppendScriptFilter(ref _avsUserInput, value, nameof(AvsUserInput))));
        InsertVpyCropFilterCommand = new ActionCmd(filter => InsertCropFilter(filter as string, value => AppendScriptFilter(ref _vpyUserInput, value, nameof(VpyUserInput))));
        InsertFFmpegCropFilterCommand = new ActionCmd(filter => InsertCropFilter(filter as string, AppendFFmpegFilter));
        ClearFiltersCommand = new ActionCmd(_ => ClearFilters(), _ => CanClearFilters);
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
        OnPropertyChanged(nameof(IsColorSpacePeakNitsVisible));
        OnPropertyChanged(nameof(FFmpegLowToHighColorFilter));
        OnPropertyChanged(nameof(FFmpegLowToHighColorFilterDisplay));
        OnPropertyChanged(nameof(FFmpegHighToLowColorFilter));
        OnPropertyChanged(nameof(FFmpegHighToLowColorFilterDisplay));
        OnPropertyChanged(nameof(FFmpegHdrToSdrColorFilter));
        OnPropertyChanged(nameof(FFmpegHdrToSdrColorFilterDisplay));
        OnPropertyChanged(nameof(FFmpegHighHdrToLowSdrColorFilter));
        OnPropertyChanged(nameof(FFmpegHighHdrToLowSdrColorFilterDisplay));
        OnPropertyChanged(nameof(AviSynthChroma422Filter));
        OnPropertyChanged(nameof(AviSynthChroma420Filter));
        OnPropertyChanged(nameof(VapourSynthChroma422Filter));
        OnPropertyChanged(nameof(VapourSynthChroma420Filter));
        OnPropertyChanged(nameof(FFmpegChroma422Filter));
        OnPropertyChanged(nameof(FFmpegChroma420Filter));
        OnPropertyChanged(nameof(CanInsertAviSynthChroma422Filter));
        OnPropertyChanged(nameof(CanInsertAviSynthChroma420Filter));
        OnPropertyChanged(nameof(CanInsertVapourSynthChroma422Filter));
        OnPropertyChanged(nameof(CanInsertVapourSynthChroma420Filter));
        OnPropertyChanged(nameof(CanInsertFFmpegChroma422Filter));
        OnPropertyChanged(nameof(CanInsertFFmpegChroma420Filter));
        OnPropertyChanged(nameof(CanInsertFFmpegLowToHighColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegHighToLowColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegHdrToSdrColorFilter));
        OnPropertyChanged(nameof(CanInsertFFmpegHighHdrToLowSdrColorFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(VSZipCLFilter));
        OnPropertyChanged(nameof(VSZipCLHasFmtconv));
        OnPropertyChanged(nameof(VSZipCLFmtconvHint));
        RecomputeCrop();
    }

    private void BuildColorSpaceSettingsListing()
    {
        TextBox textBox = new()
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        textBox.PreviewTextInput += PeakNitsPreviewTextInput;
        DataObject.AddPastingHandler(textBox, PeakNitsPasting);
        textBox.SetBinding(
            TextBox.TextProperty,
            new Binding(nameof(ColorSpacePeakNits))
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        SettingsListing.Add(new AppConfItem
        {
            Text = "HDR peak",
            Content = textBox
        });
    }

    private void BuildVSZipCLSettingsListing()
    {
        TextBox textBox = new()
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        textBox.PreviewTextInput += VSZipCLDeviceIdPreviewTextInput;
        DataObject.AddPastingHandler(textBox, VSZipCLDeviceIdPasting);
        textBox.SetBinding(
            TextBox.TextProperty,
            new Binding(nameof(VSZipCLDeviceId))
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        VSZipCLSettingsListing.Add(new AppConfItem
        {
            Text = "OpenCL Device ID",
            Content = textBox
        });
    }

    private static void PeakNitsPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        e.Handled = !IsValidPeakNitsText(textBox, e.Text);
    }

    private static void PeakNitsPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        string? pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string
            ?? e.DataObject.GetData(DataFormats.Text) as string;
        if (pastedText is null || !IsValidPeakNitsText(textBox, pastedText))
            e.CancelCommand();
    }

    private static bool IsValidPeakNitsText(TextBox textBox, string insertedText)
    {
        if (insertedText.Any(character => !char.IsDigit(character) && character != '.'))
            return false;

        string proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, insertedText);
        return proposedText.Count(character => character == '.') <= 1;
    }

    private static void VSZipCLDeviceIdPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        e.Handled = !IsValidVSZipCLDeviceIdText(textBox, e.Text);
    }

    private static void VSZipCLDeviceIdPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        string? pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string
            ?? e.DataObject.GetData(DataFormats.Text) as string;
        if (pastedText is null || !IsValidVSZipCLDeviceIdText(textBox, pastedText))
            e.CancelCommand();
    }

    private static bool IsValidVSZipCLDeviceIdText(TextBox textBox, string insertedText)
    {
        if (insertedText.Any(character => !char.IsDigit(character)))
            return false;

        string proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, insertedText);
        return proposedText.Length > 0 && proposedText.All(char.IsDigit);
    }

    public void RefreshGeneratedFFmpegFilters()
    {
        FlushPendingCropRefresh();
        OnPropertyChanged(nameof(FFmpegSarRepairFilter));
        OnPropertyChanged(nameof(FFmpegSarRepairFilterDisplay));
        OnPropertyChanged(nameof(FFmpegDebandFilter));
        OnPropertyChanged(nameof(FFmpegDebandFilterDisplay));
        OnPropertyChanged(nameof(FFmpegRotateFilter));
        OnPropertyChanged(nameof(FFmpegRotateFilterDisplay));
        OnPropertyChanged(nameof(FFmpegUpscaleFilter));
        OnPropertyChanged(nameof(FFmpegUpscaleFilterDisplay));
        OnPropertyChanged(nameof(FFmpegFlipFilter));
        OnPropertyChanged(nameof(FFmpegFlipFilterDisplay));
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
        OnPropertyChanged(nameof(FFmpegFpsFilterDisplay));
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

    private void ApplyFFmpegFilterArgs()
    {
        FlushPendingCropRefresh();
        _applyFFmpegFilterArgs(FFmpegFreeText.Trim());
    }

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
            HasCropFilter ? CropHeight : 0,
            HasUpscaleOutput ? UpscaleTargetWidth : 0,
            HasUpscaleOutput ? UpscaleTargetHeight : 0,
            RotateMode % 2 == 1);

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

    #region AviSynth Preview
    private void OpenAvsPreview()
    {
        string? toolPath = GetAvsPreviewToolPath();
        if (!CanOpenAvsPreview || toolPath == null) return;

        var existingWindow = Application.Current.Windows
            .OfType<AvsPreviewerDialog>()
            .FirstOrDefault();
        if (existingWindow != null)
        {
            existingWindow.Activate();
            return;
        }

        string srcPath = GetVpyPreviewsrcPath();
        int fpsnum = _isFrameRateVariable && _avsEnableFpsParams ? _frameRateNum : 0;
        int fpsden = _isFrameRateVariable && _avsEnableFpsParams ? _frameRateDen : 0;
        string script = ScriptTemplate.BuildAvsPreviewScript(srcPath, AvsUserInput, fpsnum, fpsden);
        long total = _getTotalFrames?.Invoke() ?? 0;
        int frameCount = (int)Math.Min(total > 0 ? total : 1, int.MaxValue);
        string[] previewSourcePaths = GetVpyPreviewsrcPaths();
        string[] previewToolPaths = GetAvsPreviewToolPaths(toolPath);
        string buildScript(string path) => ScriptTemplate.BuildAvsPreviewScript(path, AvsUserInput, fpsnum, fpsden);
        string buildSourceScript(string path) => ScriptTemplate.BuildAvsSourceLine(path, fpsnum, fpsden);

        AvsPreviewerVM previewVm = new(
            _modalNavS,
            toolPath,
            script,
            srcPath,
            frameCount,
            buildScript,
            previewSourcePaths,
            buildSourceScript,
            previewToolPaths);

        Window? ownerWindow = Application.Current.Windows
            .OfType<FilterScribeModal>()
            .FirstOrDefault(w => ReferenceEquals(w.DataContext, this));
        AvsPreviewerDialog window = new(previewVm, _modalNavS, ownerWindow);
        if (ownerWindow != null)
            PositionVpyPreviewWindow(ownerWindow, window);
        window.Show();
    }

    private string? GetAvsPreviewToolPath()
    {
        string? selected = _getSelectedUpstreamExeName();
        string? selectedPath = selected?.Equals("avs2pipemod.exe", StringComparison.OrdinalIgnoreCase) == true
            ? _getAvs2pipemodPath?.Invoke()
            : selected?.Equals("avs2yuv.exe", StringComparison.OrdinalIgnoreCase) == true
                ? _getAvs2yuvPath?.Invoke()
                : null;
        if (!string.IsNullOrWhiteSpace(selectedPath) && File.Exists(selectedPath)) return selectedPath;

        string? avs2yuvPath = _getAvs2yuvPath?.Invoke();
        if (!string.IsNullOrWhiteSpace(avs2yuvPath) && File.Exists(avs2yuvPath)) return avs2yuvPath;
        string? avs2pipemodPath = _getAvs2pipemodPath?.Invoke();
        return !string.IsNullOrWhiteSpace(avs2pipemodPath) && File.Exists(avs2pipemodPath)
            ? avs2pipemodPath
            : null;
    }

    private string[] GetAvsPreviewToolPaths(string? preferredPath = null)
    {
        List<string> paths = [];
        AddToolPath(preferredPath);
        AddToolPath(_getAvs2yuvPath?.Invoke());
        AddToolPath(_getAvs2pipemodPath?.Invoke());
        return [.. paths];

        void AddToolPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            if (paths.Any(existing => existing.Equals(path, StringComparison.OrdinalIgnoreCase))) return;
            paths.Add(path);
        }
    }

    #endregion

    #region FFmpeg Preview
    private void OpenFfmpegPreview()
    {
        string? ffmpegPath = GetFfmpegPreviewToolPath();
        if (!CanOpenFfmpegPreview || ffmpegPath == null) return;

        FFmpegPreviewerDialog? existingWindow = Application.Current.Windows
            .OfType<FFmpegPreviewerDialog>()
            .FirstOrDefault();
        if (existingWindow != null)
        {
            existingWindow.Activate();
            return;
        }

        string srcPath = GetVpyPreviewsrcPath();
        if (string.IsNullOrWhiteSpace(srcPath)) return;

        long total = _getTotalFrames?.Invoke() ?? 0;
        int frameCount = (int)Math.Min(total > 0 ? total : 1, int.MaxValue);
        string[] previewSourcePaths = GetVpyPreviewsrcPaths();

        FFmpegPreviewerVM previewVm = new(
            _modalNavS,
            ffmpegPath,
            () => FFmpegFreeText,
            srcPath,
            frameCount,
            _sourceFfprobeJson,
            previewSourcePaths);

        Window? ownerWindow = Application.Current.Windows
            .OfType<FilterScribeModal>()
            .FirstOrDefault(w => ReferenceEquals(w.DataContext, this));
        FFmpegPreviewerDialog window = new(previewVm, _modalNavS, ownerWindow);
        if (ownerWindow != null)
            PositionVpyPreviewWindow(ownerWindow, window);
        window.Show();
    }

    private string? GetFfmpegPreviewToolPath()
    {
        string? ffmpegPath = _getFfmpegPath?.Invoke();
        return !string.IsNullOrWhiteSpace(ffmpegPath) && File.Exists(ffmpegPath)
            ? ffmpegPath
            : null;
    }

    #endregion

    #region VapourSynth Preview
    private void OpenVpyPreview()
    {
        if (!CanOpenVpyPreview) return;

        var existingWindow = Application.Current.Windows
            .OfType<VpyPreviewerDialog>()
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
                VpyPreviewerLangProvider.WindowTitle,
                "!vspipe.exe").Execute(null);
            return;
        }

        if (string.IsNullOrWhiteSpace(_vspipeY4mArg))
        {
            new OpenErrModalCmd(
                _modalNavS,
                VpyPreviewerLangProvider.WindowTitle,
                "!vspipe Y4M args").Execute(null);
            return;
        }

        string srcPath = GetVpyPreviewsrcPath();
        if (string.IsNullOrWhiteSpace(srcPath))
        {
            new OpenErrModalCmd(
                _modalNavS,
                VpyPreviewerLangProvider.WindowTitle,
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

        var previewVm = new VpyPreviewerVM(
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
        VpyPreviewerDialog window = new(previewVm, _modalNavS, ownerWindow);

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
        OnPropertyChanged(nameof(ResizeTitle));
        OnPropertyChanged(nameof(Downscale));
        OnPropertyChanged(nameof(ResolutionModeLabel));
        OnPropertyChanged(nameof(ResolutionLockAspectLabel));
        OnPropertyChanged(nameof(HasSource));
        OnPropertyChanged(nameof(ScaleNotApplicableText));
        OnPropertyChanged(nameof(TargetDisplay));
        OnPropertyChanged(nameof(FFmpegFreeTextHint));
        OnPropertyChanged(nameof(SarRepairTitle));
        OnPropertyChanged(nameof(DebandTitle));
        OnPropertyChanged(nameof(RotateTitle));
        OnPropertyChanged(nameof(Upscale));
        OnPropertyChanged(nameof(FlipTitle));
        OnPropertyChanged(nameof(HorizontalFlipLabel));
        OnPropertyChanged(nameof(VerticalFlipLabel));
        OnPropertyChanged(nameof(FFmpegSarRepairFilter));
        OnPropertyChanged(nameof(FFmpegDebandFilter));
        OnPropertyChanged(nameof(FFmpegRotateFilter));
        OnPropertyChanged(nameof(FFmpegUpscaleFilter));
        OnPropertyChanged(nameof(FFmpegFlipFilter));
        OnPropertyChanged(nameof(FFmpegHqdn3dDenoiseFilter));
        OnPropertyChanged(nameof(FFmpegSubtitleFilter));
        OnPropertyChanged(nameof(FFmpegFpsScaleFilter));
        OnPropertyChanged(nameof(FFmpegFpsColorScaleFilter));
        OnPropertyChanged(nameof(FFmpegFullChainFilter));
        OnPropertyChanged(nameof(VSZipCLTitle));
        OnPropertyChanged(nameof(VSZipCLPreviewHint));
        OnPropertyChanged(nameof(VSZipCLDeviceHint));
        OnPropertyChanged(nameof(VSZipCLFmtconvHint));
        OnPropertyChanged(nameof(ColorSpaceConvertTitle));
        OnPropertyChanged(nameof(DenoiseTitle));
        OnPropertyChanged(nameof(ScaleHint));
        OnPropertyChanged(nameof(SubtitleBurnTitle));
        OnPropertyChanged(nameof(MultiFilterAssemblyTitle));
        OnPropertyChanged(nameof(BlurSharpenTitle));
        OnPropertyChanged(nameof(AviSynthBlurSharpenFilter));
        OnPropertyChanged(nameof(VapourSynthBlurSharpenFilter));
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
        OnPropertyChanged(nameof(CanOpenAvsPreview));
        OpenAvsPreviewCommand.OnCanExecuteChanged();
        OnPropertyChanged(nameof(CanOpenFfmpegPreview));
        OpenFfmpegPreviewCommand.OnCanExecuteChanged();
        OnPropertyChanged(nameof(ClearFiltersText));
        OnPropertyChanged(nameof(CanClearFilters));
        ClearFiltersCommand.OnCanExecuteChanged();

        BuildButtonGroups();
        OnPropertyChanged(nameof(FinishScribeButtons));
    }
    #endregion

    public override void Dispose()
    {
        UILangProvider.CurrentChanged -= OnLanguageChanged;
        _cropRefreshTimer.Stop();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
