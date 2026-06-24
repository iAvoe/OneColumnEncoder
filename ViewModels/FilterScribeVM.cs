using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// Note:
    /// Users must manually copy/enter the desired filters into the free text box in the ffmpeg tab to be accepted.
    /// 
    /// File save & ItemCard write back logic created by MainVM as OnSourceImported,
    /// passed in via OpenFilterScribeCmd constructor as Action<>
    /// </summary>
    public class FilterScribeVM : BaseVM
    {
        private readonly ModalNavS _modalNavS;
        private readonly Func<string> _getSourcePath;
        private readonly Action _closeAction;
        private readonly ToolItemCardVM _avsItem;
        private readonly ToolItemCardVM _vpyItem;
        private readonly Func<SourceFileKind?> _getPreferredScriptSourceKind;
        private readonly Action<ToolItemCardVM, SourceFileKind, string> _afterImport;
        private readonly Action<string?> _applyFfmpegFilterArgs;
        private readonly Func<bool> _hasSourceValidationError;
        private readonly Func<bool> _hasSarRepairWarning;
        private readonly Func<bool>? _isQueueRoute;
        private readonly Func<string[]>? _getQueueFilePaths;
        private ColorSpaceAnalysisM _colorSpaceAnalysis = ColorSpaceConverterH.Analyze(null);
        public CloseModalCmd CloseCmd { get; }
        // 0: AVS, 1: VPY, 2: ffmpeg
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        // Avs/VpyPrefix becomes instance property to support dynamic fpsnum/fpsden
        // Avs/VpyPrefix2 is a guidance comment to keep
        #region Script text
        private string _baseAvsPrefix;
        public string AvsPrefix
        {
            get
            {
                if (_isFrameRateVariable && _avsEnableFpsParams && _frameRateNum > 0 && _frameRateDen > 0)
                    return $"LWLibavVideoSource(\"video file path\", fpsnum={_frameRateNum}, fpsden={_frameRateDen})";
                return _baseAvsPrefix;
            }
        }
        public static string AvsPrefix2 => UILangProviderM.Current["SrcScribe.AvsPrefix2"];
        private string _avsUserInput = "";
        public string AvsUserInput
        {
            get => _avsUserInput;
            set => SetProperty(ref _avsUserInput, value);
        }
        public static string AvsSuffix => UILangProviderM.Current["SrcScribe.AvsSuffix"];

        private string _baseVpyPrefix;
        public string VpyPrefix
        {
            get
            {
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
        public static string VpyPrefix2 => UILangProviderM.Current["SrcScribe.VpyPrefix2"];
        public static string VpySuffix => UILangProviderM.Current["SrcScribe.VpySuffix"];
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
                    OnPropertyChanged(nameof(HasSource));
                    OnPropertyChanged(nameof(IsScaleApplicable));
                    RecomputeTarget();
                }
            }
        }

        public bool IsScaleApplicable =>
            HasSource && ResolutionScaleH.IsScaleApplicable(SourceWidth, SourceHeight);

        public string ScaleNotApplicableText =>
            !HasSource
                ? UILangProviderM.Current["SrcScribe.NoVidSrcWarning"]
                : string.Format(UILangProviderM.Current["SrcScribe.ScaleNotApplicable"], 16);

        private int _scalePercent = 100;
        public int ScalePercent
        {
            get => _scalePercent;
            set
            {
                if (SetProperty(ref _scalePercent, value)) RecomputeTarget();
            }
        }

        public void CommitScale()
        {
            if (!IsScaleApplicable) return;
            // var w, h are discard values now
            var (_, _) = ResolutionScaleH.ComputeTargetDimensions(SourceWidth, SourceHeight, ScalePercent);
            OnPropertyChanged(nameof(TargetDisplay));
            OnPropertyChanged(nameof(FfmpegResizeFilter));
            OnPropertyChanged(nameof(FfmpegFpsScaleFilter));
            OnPropertyChanged(nameof(FfmpegFpsColorScaleFilter));
            OnPropertyChanged(nameof(FfmpegFullChainFilter));
            OnPropertyChanged(nameof(VapourSynthResizeFilter));
            OnPropertyChanged(nameof(AviSynthResizeFilter));
        }

        private int _targetWidth;
        public int TargetWidth => _targetWidth;

        private int _targetHeight;
        public int TargetHeight => _targetHeight;

        public string TargetDisplay => !HasSource ? "--" : $"{TargetWidth}x{TargetHeight}";

        private bool HasScaleFilter => IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight);

        private bool HasFpsFilter => IsFrameRateApplicable;

        private bool HasSarRepairFilter => _hasSarRepairWarning();

        private bool HasColorSpaceFilter =>
            !_hasSourceValidationError()
            && _colorSpaceAnalysis.IsApplicable
            && !RequiresManualColorSpacePeakNits
            && !string.IsNullOrWhiteSpace(_colorSpaceAnalysis.FfmpegColorFilter);

        private bool RequiresManualColorSpacePeakNits =>
            _colorSpaceAnalysis.Strategy is ColorSpaceStrategy.HdrToSdr or ColorSpaceStrategy.HighHdrToSdr;

        private string? ScaleFilterChain => HasScaleFilter ? $"scale={TargetWidth}:{TargetHeight}" : null;

        private string? FpsFilterChain => HasFpsFilter ? $"fps={_frameRateNum}/{_frameRateDen}" : null;

        private string? SarRepairFilterChain => HasSarRepairFilter ? "libplacebo=reset_sar=1" : null;

        private string? ColorSpaceFilterChain => HasColorSpaceFilter ? _colorSpaceAnalysis.FfmpegColorFilter : null;

        private bool IsColorSpaceStrategyShown(ColorSpaceStrategy strategy) =>
            !_hasSourceValidationError()
            && ColorSpaceConverterH.IsStrategyApplicable(strategy, _colorSpaceAnalysis.ColorPrimaries, _colorSpaceAnalysis.ColorTransfer)
            && !string.IsNullOrWhiteSpace(BuildColorSpaceStrategyFilterChain(strategy));

        public string FfmpegResizeFilter =>
            HasScaleFilter
                ? BuildFfmpegFilterArgs(includeSwsFlags: true, includeCsp709Flags: false, ScaleFilterChain)
                : "N/A";

        public string FfmpegFpsFilter =>
            HasFpsFilter
                ? BuildFfmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: false, FpsFilterChain)
                : "N/A";

        public string FfmpegSarRepairFilter =>
            HasSarRepairFilter
                ? "-filter:v \"libplacebo=reset_sar=1\""
                : "N/A";

        public static string VapourSynthHqdn3dDenoiseFilter =>
            UILangProviderM.Current["SrcScribe.FilterPlaceholder"];
        public static string AviSynthHqdn3dDenoiseFilter =>
            UILangProviderM.Current["SrcScribe.FilterPlaceholder"];
        public static string FfmpegHqdn3dDenoiseFilter =>
            UILangProviderM.Current["SrcScribe.FilterPlaceholder"];
        public static string VapourSynthSubtitleFilter =>
            UILangProviderM.Current["SrcScribe.FilterPlaceholder"];
        public static string AviSynthSubtitleFilter =>
            UILangProviderM.Current["SrcScribe.FilterPlaceholder"];
        public static string FfmpegSubtitleFilter =>
            UILangProviderM.Current["SrcScribe.FilterPlaceholder"];

        public string FfmpegFpsScaleFilter =>
            HasFpsFilter && HasScaleFilter
                ? BuildFfmpegFilterArgs(includeSwsFlags: true, includeCsp709Flags: false, FpsFilterChain, ScaleFilterChain)
                : "N/A";

        public string FfmpegLowToHighColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.LowToHigh);

        public string FfmpegHighToLowColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.HighToLow);

        public string FfmpegHdrToSdrColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.HdrToSdr);

        public string FfmpegHighHdrToLowSdrColorFilter => GetColorSpaceStrategyFilter(ColorSpaceStrategy.HighHdrToSdr);

        public string FfmpegFpsColorScaleFilter
        {
            get
            {
                string? color = ColorSpaceFilterChain;
                string? fps = FpsFilterChain;
                string? scale = ScaleFilterChain;
                if (color == null || fps == null || scale == null) return "N/A";
                return BuildFfmpegFilterArgs(includeSwsFlags: scale != null, includeCsp709Flags: color != null, fps, color, scale);
            }
        }

        public string FfmpegFullChainFilter
        {
            get
            {
                string? sar = SarRepairFilterChain;
                string? color = ColorSpaceFilterChain;
                string? fps = FpsFilterChain;
                string? scale = ScaleFilterChain;
                if (sar == null || color == null || fps == null || scale == null) return "N/A";
                return BuildFfmpegFilterArgs(includeSwsFlags: scale != null, includeCsp709Flags: color != null, fps, sar, color, scale);
            }
        }

        private string GeneratedFfmpegFilterArgs
        {
            get
            {
                bool hasSar = HasSarRepairFilter;
                bool hasColor = HasColorSpaceFilter;
                bool hasFps = HasFpsFilter;
                bool hasScale = HasScaleFilter;
                if (!hasSar && !hasColor && !hasFps && !hasScale) return string.Empty;
                if (hasSar && !hasColor && !hasFps && !hasScale) return BuildFfmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: false, SarRepairFilterChain);
                return BuildFfmpegFilterArgs(hasScale, hasColor, FpsFilterChain, SarRepairFilterChain, ColorSpaceFilterChain, ScaleFilterChain);
            }
        }

        private string GetColorSpaceStrategyFilter(ColorSpaceStrategy strategy) =>
            IsColorSpaceStrategyShown(strategy)
                ? BuildFfmpegFilterArgs(includeSwsFlags: false, includeCsp709Flags: true, BuildColorSpaceStrategyFilterChain(strategy))
                : "N/A";

        private string? BuildColorSpaceStrategyFilterChain(ColorSpaceStrategy strategy) =>
            ColorSpaceConverterH.BuildFfmpegFilter(
                strategy,
                _colorSpaceAnalysis.ColorMatrix,
                _colorSpaceAnalysis.ColorChromaLocation,
                _colorSpaceAnalysis.ColorPrimaries,
                _colorSpaceAnalysis.PixelFormat);

        private string BuildFfmpegFilterArgs(bool includeSwsFlags, bool includeCsp709Flags, params string?[] filters)
        {
            string filterChain = string.Join(",", filters.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
            if (string.IsNullOrWhiteSpace(filterChain)) return string.Empty;

            string filterArgs = filterChain.Contains(',', StringComparison.Ordinal)
                ? $"-filter:v \"{filterChain}\""
                : $"-filter:v {filterChain}";

            string csp709Flags = includeCsp709Flags
                ? " -color_primaries bt709 -color_trc bt709 -colorspace bt709"
                : string.Empty;

            string pixelFormatFlag = includeCsp709Flags && !string.IsNullOrWhiteSpace(_colorSpaceAnalysis.PixelFormat)
                ? $" -pix_fmt {_colorSpaceAnalysis.PixelFormat}"
                : string.Empty;

            string swsFlags = includeSwsFlags
                ? " -sws_flags bicubic+full_chroma_int+full_chroma_inp+accurate_rnd"
                : string.Empty;

            return $"{filterArgs}{csp709Flags}{pixelFormatFlag}{swsFlags}";
        }

        public string VapourSynthResizeFilter =>
            IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight)
                ? $"src = core.resize.Bicubic(src, {TargetWidth}, {TargetHeight})"
                : "N/A";

        public string AviSynthResizeFilter =>
            IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight)
                ? $"BicubicResize({TargetWidth}, {TargetHeight})"
                : "N/A";

        public static List<string> ScaleTickLabels =>
            ResolutionScaleH.GenerateTickLabels(10, 100, 5);

        private void RecomputeTarget()
        {
            if (!IsScaleApplicable) return;
            var (w, h) = ResolutionScaleH.ComputeTargetDimensions(SourceWidth, SourceHeight, ScalePercent);
            if (_targetWidth != w || _targetHeight != h)
            {
                _targetWidth = w;
                _targetHeight = h;
                OnPropertyChanged(nameof(TargetWidth));
                OnPropertyChanged(nameof(TargetHeight));
                OnPropertyChanged(nameof(TargetDisplay));
                OnPropertyChanged(nameof(FfmpegResizeFilter));
                OnPropertyChanged(nameof(FfmpegFpsScaleFilter));
                OnPropertyChanged(nameof(FfmpegFpsColorScaleFilter));
                OnPropertyChanged(nameof(FfmpegFullChainFilter));
                OnPropertyChanged(nameof(VapourSynthResizeFilter));
                OnPropertyChanged(nameof(AviSynthResizeFilter));
            }
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
        public string FfmpegFreeText
        {
            get => _ffmpegFreeText;
            set => SetProperty(ref _ffmpegFreeText, value);
        }
        #endregion

        #region UILang properties
        public static string FfmpegText => "ffmpeg";
        public static string VapourSynthText => "VS";
        public static string AviSynthText => "AVS(+)";
        public static string WindowTitle => UILangProviderM.FltScribeWindowTitle;
        public static string ScribeDescription1 => UILangProviderM.Current["SrcScribe.Description1"];
        public static string ScribeDescription2 => UILangProviderM.Current["SrcScribe.Description2"];
        public static string NoteText => UILangProviderM.Current["SrcScribe.NoteText"];
        public static string TabAvs => UILangProviderM.Current["SrcScribe.TabAvs"];
        public static string TabVpy => UILangProviderM.Current["SrcScribe.TabVpy"];
        public static string TabFfmpeg => UILangProviderM.Current["SrcScribe.TabFfmpeg"];
        public static string ResolutionScaleTitle => UILangProviderM.Current["SrcScribe.ResolutionScaleTitle"];
        public static string ScalePercentLabel => UILangProviderM.Current["SrcScribe.ScalePercentLabel"];
        public static string FfmpegFreeTextHint => UILangProviderM.Current["SrcScribe.FfmpegFreeTextHint"];
        public static string SarRepairTitle => UILangProviderM.Current["SrcScribe.SarRepairTitle"];
        public static string FrameRateConvertTitle => UILangProviderM.Current["SrcScribe.FrameRateConvertTitle"];
        public static string ColorSpaceConvertTitle => UILangProviderM.Current["SrcScribe.ColorSpaceConvertTitle"];
        public static string DenoiseTitle => UILangProviderM.Current["SrcScribe.DenoiseTitle"];
        public static string SubtitleBurnTitle => UILangProviderM.Current["SrcScribe.SubtitleBurnTitle"];
        public static string MultiFilterAssemblyTitle => UILangProviderM.Current["SrcScribe.MultiFilterAssemblyTitle"];
        public static string LowToHighColorFilterLabel => "NCG";
        public static string HighToLowColorFilterLabel => "WCG";
        public static string HdrToSdrColorFilterLabel => "HDR→SDR";
        public static string HighHdrToLowSdrColorFilterLabel => "H&W→SDR";
        public static string ColorSpacePeakNitsHint => UILangProviderM.Current["SrcScribe.ColorSpacePeakNitsHint"];
        #endregion

        public ButtonGroupVM ScriptExportButtons { get; private set; } = null!;
        public ButtonGroupVM FinishScribeButtons { get; private set; } = null!;

        public FilterScribeVM(
            ModalNavS modalNavS,
            Action closeAction,
            Func<string> getSourcePath,
            ToolItemCardVM avsItem,
            ToolItemCardVM vpyItem,
            Func<SourceFileKind?> getPreferredScriptSourceKind,
            Action<ToolItemCardVM, SourceFileKind, string> afterImport,
            Action<string?> applyFfmpegFilterArgs,
            Func<bool> hasSourceValidationError,
            Func<bool> hasSarRepairWarning,
            string? sourceFfprobeJson = null,
            Func<bool>? isQueueRoute = null,
            Func<string[]>? getQueueFilePaths = null)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            CloseCmd = new CloseModalCmd(closeAction);
            _getSourcePath = getSourcePath;
            _avsItem = avsItem;
            _vpyItem = vpyItem;
            _getPreferredScriptSourceKind = getPreferredScriptSourceKind;
            _afterImport = afterImport;
            _applyFfmpegFilterArgs = applyFfmpegFilterArgs;
            _hasSourceValidationError = hasSourceValidationError;
            _hasSarRepairWarning = hasSarRepairWarning;
            _isQueueRoute = isQueueRoute;
            _getQueueFilePaths = getQueueFilePaths;
            _baseAvsPrefix = UILangProviderM.Current["SrcScribe.AvsPrefix"];
            _baseVpyPrefix = UILangProviderM.Current["SrcScribe.VpyPrefix"];
            ParseColorSpaceInfo(sourceFfprobeJson);
            ParseSourceResolution(sourceFfprobeJson);
            ParseFrameRateInfo(sourceFfprobeJson);
            BuildButtonGroups();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
        }

        private void ParseColorSpaceInfo(string? sourceFfprobeJson)
        {
            _colorSpaceAnalysis = ColorSpaceConverterH.Analyze(sourceFfprobeJson);
            OnPropertyChanged(nameof(FfmpegLowToHighColorFilter));
            OnPropertyChanged(nameof(FfmpegHighToLowColorFilter));
            OnPropertyChanged(nameof(FfmpegHdrToSdrColorFilter));
            OnPropertyChanged(nameof(FfmpegHighHdrToLowSdrColorFilter));
            OnPropertyChanged(nameof(FfmpegFpsColorScaleFilter));
            OnPropertyChanged(nameof(FfmpegFullChainFilter));
        }

        public void RefreshGeneratedFfmpegFilters()
        {
            OnPropertyChanged(nameof(FfmpegSarRepairFilter));
            OnPropertyChanged(nameof(FfmpegFpsScaleFilter));
            OnPropertyChanged(nameof(FfmpegFpsColorScaleFilter));
            OnPropertyChanged(nameof(FfmpegFullChainFilter));
        }

        private void ParseSourceResolution(string? sourceFfprobeJson)
        {
            if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return;

            try
            {
                using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
                if (!document.RootElement.TryGetProperty("streams", out JsonElement streams)
                    || streams.ValueKind != JsonValueKind.Array)
                    return;

                foreach (JsonElement stream in streams.EnumerateArray())
                {
                    string? codecType = null;
                    if (stream.TryGetProperty("codec_type", out JsonElement ct))
                        codecType = ct.GetString();

                    if (codecType is null or "video")
                    {
                        if (stream.TryGetProperty("width", out JsonElement w) && w.TryGetInt32(out int width)
                            && stream.TryGetProperty("height", out JsonElement h) && h.TryGetInt32(out int height))
                        {
                            SourceWidth = width;
                            SourceHeight = height;
                        }
                        return;
                    }
                }
            }
            catch
            {
                // ignore parse errors
            }
        }

        private void ParseFrameRateInfo(string? sourceFfprobeJson)
        {
            if (string.IsNullOrWhiteSpace(sourceFfprobeJson)) return;

            try
            {
                using JsonDocument document = JsonDocument.Parse(sourceFfprobeJson);
                if (!FrameRateH.TryGetFirstVideoStream(document.RootElement, out JsonElement stream))
                    return;

                bool? isVfr = FrameRateH.IsVariableFrameRate(stream);
                _isFrameRateVariable = isVfr == true;

                if (_isFrameRateVariable)
                {
                    var r = FrameRateH.GetRFrameRate(stream);
                    if (r.HasValue)
                    {
                        _frameRateNum = r.Value.num;
                        _frameRateDen = r.Value.den;
                    }
                }

                OnPropertyChanged(nameof(IsFrameRateVariable));
                OnPropertyChanged(nameof(IsFrameRateApplicable));
                OnPropertyChanged(nameof(FrameRateNum));
                OnPropertyChanged(nameof(FrameRateDen));
                OnPropertyChanged(nameof(FfmpegFpsFilter));
                OnPropertyChanged(nameof(FfmpegFpsScaleFilter));
                OnPropertyChanged(nameof(FfmpegFpsColorScaleFilter));
                OnPropertyChanged(nameof(FfmpegFullChainFilter));
            }
            catch { } // ignore parse errors
        }

        private void BuildButtonGroups()
        {
            ScriptExportButtons = ButtonGroupVM.CreateThreeButton(
                UILangProviderM.Current["SrcScribe.CopyFull"],
                UILangProviderM.Current["SrcScribe.CopyInOut"],
                UILangProviderM.Current["SrcScribe.SaveAsFile"],
                new ActionCmd(_ => CopyFullScript()),
                new ActionCmd(_ => CopyInOutSection()),
                new ActionCmd(_ => SaveAsFile()));
            ScriptExportButtons.B3_3Icon = SvgIconProviderH.GameSave;

            FinishScribeButtons = ButtonGroupVM.CreateThreeButton(
                UILangProviderM.Current["SrcScribe.Cancel"],
                UILangProviderM.Current["SrcScribe.ApplyFfmpegOnly"],
                UILangProviderM.Current["SrcScribe.Confirm"],
                CloseCmd,
                new ActionCmd(_ => ApplyFfmpegFilterArgsOnly()),
                new ActionCmd(_ => SaveAndImportAll()));
        }

        #region ThreeButtonGroup: copy full, copy in-out, save as file
        private void CopyFullScript()
        {
            Clipboard.SetText(GetCurrentFullScript());
            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.FltScribeWindowTitle,
                UILangProviderM.Current["SrcScribe.CopiedFull"]).Execute(null);
        }
        private void CopyInOutSection()
        {
            string sourcePath = _getSourcePath();
            string inOutText = SelectedTabIndex switch
            {
                0 => ScriptTemplateH.BuildAvsInOutSection(sourcePath, AvsPrefix2, AvsSuffix,
                    _avsEnableFpsParams ? _frameRateNum : 0, _avsEnableFpsParams ? _frameRateDen : 0),
                1 => ScriptTemplateH.BuildVpyInOutSection(sourcePath, VpyPrefix2, VpySuffix,
                    _vpyEnableFpsParams ? _frameRateNum : 0, _vpyEnableFpsParams ? _frameRateDen : 0),
                _ => string.Empty
            };

            Clipboard.SetText(inOutText);
            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.FltScribeWindowTitle,
                UILangProviderM.Current["SrcScribe.CopiedSection"]).Execute(null);
        }
        private void SaveAsFile()
        {
            if (_isQueueRoute?.Invoke() == true)
            {
                ExecuteQueueSaveAsFile();
                return;
            }

            string sourcePath = _getSourcePath();
            int avsFpsnum = _avsEnableFpsParams ? _frameRateNum : 0;
            int avsFpsden = _avsEnableFpsParams ? _frameRateDen : 0;
            int vpyFpsnum = _vpyEnableFpsParams ? _frameRateNum : 0;
            int vpyFpsden = _vpyEnableFpsParams ? _frameRateDen : 0;
            string script = SelectedTabIndex switch
            {
                0 => ScriptTemplateH.BuildAvsExportScript(
                    sourcePath, AvsPrefix2, AvsSuffix, AvsUserInput, avsFpsnum, avsFpsden),
                1 => ScriptTemplateH.BuildVpyExportScript(
                    sourcePath, VpyPrefix2, VpySuffix, VpyUserInput, vpyFpsnum, vpyFpsden),
                _ => FfmpegFreeText
            };

            string filter = SelectedTabIndex switch
            {
                0 => UILangProviderM.Current["SrcScribe.FilterAvs"],
                1 => UILangProviderM.Current["SrcScribe.FilterVpy"],
                _ => "Text files (*.txt)|*.txt"
            };

            string extension = SelectedTabIndex switch
            {
                0 => ".avs",
                1 => ".vpy",
                _ => ".txt"
            };

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"],
                Filter = filter,
                FileName = GetScriptFileName(sourcePath, extension)
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

            if (TryWriteScript(dialog.FileName, script))
                ShowSavedMessage(dialog.FileName);
        }

        private void ExecuteQueueSaveAsFile()
        {
            string[] sourcePaths = _getQueueFilePaths?.Invoke() ?? [];
            if (sourcePaths.Length == 0) return;

            OpenFolderDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"]
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

            string directory = dialog.FolderName;
            int avsFpsnum = _avsEnableFpsParams ? _frameRateNum : 0;
            int avsFpsden = _avsEnableFpsParams ? _frameRateDen : 0;
            int vpyFpsnum = _vpyEnableFpsParams ? _frameRateNum : 0;
            int vpyFpsden = _vpyEnableFpsParams ? _frameRateDen : 0;
            List<string> savedPaths = [];

            foreach (string sourcePath in sourcePaths)
            {
                string baseName = Path.GetFileNameWithoutExtension(sourcePath);
                string avsPath = Path.Combine(directory, baseName + ".avs");
                string vpyPath = Path.Combine(directory, baseName + ".vpy");

                if (!TryWriteScript(avsPath, ScriptTemplateH.BuildAvsExportScript(
                        sourcePath, AvsPrefix2, AvsSuffix, AvsUserInput, avsFpsnum, avsFpsden)))
                    return;
                if (!TryWriteScript(vpyPath, ScriptTemplateH.BuildVpyExportScript(
                        sourcePath, VpyPrefix2, VpySuffix, VpyUserInput, vpyFpsnum, vpyFpsden)))
                    return;
                savedPaths.Add(avsPath);
                savedPaths.Add(vpyPath);
            }

            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.FltScribeWindowTitle,
                string.Format(UILangProviderM.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, savedPaths))).Execute(null);
        }

        private void ExecuteQueueSaveAndImport()
        {
            string[] sourcePaths = _getQueueFilePaths?.Invoke() ?? [];
            if (sourcePaths.Length == 0) return;

            OpenFolderDialog dialog = new()
            {
                Title = UILangProviderM.SavingScriptWindowTitle
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
                foreach (string sourcePath in sourcePaths)
                {
                    string baseName = Path.GetFileNameWithoutExtension(sourcePath);
                    string avsPath = Path.Combine(directory, baseName + ".avs");
                    string vpyPath = Path.Combine(directory, baseName + ".vpy");

                    File.WriteAllText(avsPath, ScriptTemplateH.BuildAvsExportScript(
                        sourcePath, AvsPrefix2, AvsSuffix, AvsUserInput, avsFpsnum, avsFpsden));
                    File.WriteAllText(vpyPath, ScriptTemplateH.BuildVpyExportScript(
                        sourcePath, VpyPrefix2, VpySuffix, VpyUserInput, vpyFpsnum, vpyFpsden));
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
            string[] avsFileNames = [.. savedPaths.Where(path => path.EndsWith(".avs", StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!)];
            string[] vpyFileNames = [.. savedPaths.Where(path => path.EndsWith(".vpy", StringComparison.OrdinalIgnoreCase))
                .Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!)];
            _avsItem.P2TextData = directory;
            _avsItem.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(avsFileNames);
            _avsItem.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(avsFileNames);
            _vpyItem.P2TextData = directory;
            _vpyItem.P1TextData = BrowseSourceQueueCmd.FormatQueueP1Text(vpyFileNames);
            _vpyItem.P1TooltipText = BrowseSourceQueueCmd.FormatQueueP1TooltipText(vpyFileNames);

            SelectPreferredScriptItem();

            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.FltScribeWindowTitle,
                string.Format(UILangProviderM.Current["ScriptGen.ScriptsSaved"], string.Join(Environment.NewLine, savedPaths))).Execute(null);
            _closeAction();
        }

        private void SaveAndImportAll()
        {
            ApplyFfmpegFilterArgs();

            if (_isQueueRoute?.Invoke() == true)
            {
                ExecuteQueueSaveAndImport();
                return;
            }

            string sourcePath = _getSourcePath();
            string avsScript = ScriptTemplateH.BuildAvsExportScript(
                sourcePath, AvsPrefix2, AvsSuffix, AvsUserInput,
                _avsEnableFpsParams ? _frameRateNum : 0, _avsEnableFpsParams ? _frameRateDen : 0);
            string vpyScript = ScriptTemplateH.BuildVpyExportScript(
                sourcePath, VpyPrefix2, VpySuffix, VpyUserInput,
                _vpyEnableFpsParams ? _frameRateNum : 0, _vpyEnableFpsParams ? _frameRateDen : 0);

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.SavingScriptWindowTitle,
                Filter = UILangProviderM.Current["SrcScribe.FilterAvs"],
                FileName = GetScriptFileName(sourcePath, ".avs")
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

            string avsPath = dialog.FileName;
            string directory = Path.GetDirectoryName(avsPath) ?? ".";
            string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");

            if (!TryWriteScripts(avsPath, avsScript, vpyPath, vpyScript)) return;

            SourceFileKind? preferredKind = _getPreferredScriptSourceKind();
            if (preferredKind == SourceFileKind.AviSynthScript)
            {
                ImportScript(_avsItem, SourceFileKind.AviSynthScript, avsPath);
            }
            else if (preferredKind == SourceFileKind.VapourSynthScript)
            {
                ImportScript(_vpyItem, SourceFileKind.VapourSynthScript, vpyPath);
            }
            else
            {
                ImportScript(_avsItem, SourceFileKind.AviSynthScript, avsPath);
                ImportScript(_vpyItem, SourceFileKind.VapourSynthScript, vpyPath);
            }

            SelectPreferredScriptItem();
            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.FltScribeWindowTitle,
                string.Format(UILangProviderM.Current["ScriptGen.ScriptsSaved"], $"{avsPath}\n{vpyPath}")).Execute(null);
            _closeAction();
        }

        private void ApplyFfmpegFilterArgsOnly()
        {
            ApplyFfmpegFilterArgs();
            _closeAction();
        }

        private void ApplyFfmpegFilterArgs()
        {
            _applyFfmpegFilterArgs(FfmpegFreeText.Trim());
        }

        private bool TryWriteScript(string path, string script)
        {
            try
            {
                File.WriteAllText(path, script);
                return true;
            }
            catch (Exception ex)
            {
                ShowSaveError(ex);
                return false;
            }
        }

        private static string GetScriptFileName(string sourcePath, string extension) =>
            Path.GetFileNameWithoutExtension(sourcePath) + extension;

        private bool TryWriteScripts(string avsPath, string avsScript, string vpyPath, string vpyScript)
        {
            try
            {
                File.WriteAllText(avsPath, avsScript);
                File.WriteAllText(vpyPath, vpyScript);
                return true;
            }
            catch (Exception ex)
            {
                ShowSaveError(ex);
                return false;
            }
        }

        private void ImportScript(ToolItemCardVM item, SourceFileKind kind, string path)
        {
            item.P2TextData = path;
            item.P1TextData = SourceFilePickerH.GetPrimaryText(kind, path);
            _afterImport(item, kind, path);
        }

        private void SelectPreferredScriptItem()
        {
            SourceFileKind? preferredKind = _getPreferredScriptSourceKind();
            if (preferredKind == null) return;

            ToolItemCardVM target = preferredKind == SourceFileKind.AviSynthScript ? _avsItem : _vpyItem;
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
                UILangProviderM.FltScribeWindowTitle,
                string.Format(UILangProviderM.Current["SrcScribe.FailedToSave"], ex.Message)).Execute(null);
        }

        private void ShowSavedMessage(string path)
        {
            new OpenSuccModalCmd(
                _modalNavS,
                UILangProviderM.FltScribeWindowTitle,
                string.Format(UILangProviderM.Current["SrcScribe.ScriptSaved"], path)).Execute(null);
        }

        private string GetCurrentFullScript()
        {
            string sourcePath = _getSourcePath();
            return SelectedTabIndex switch
            {
                0 => ScriptTemplateH.BuildAvsEditorScript(sourcePath, AvsPrefix2, AvsUserInput,
                    _avsEnableFpsParams ? _frameRateNum : 0, _avsEnableFpsParams ? _frameRateDen : 0),
                1 => ScriptTemplateH.BuildVpyEditorScript(sourcePath, VpyPrefix2, VpySuffix, VpyUserInput,
                    _vpyEnableFpsParams ? _frameRateNum : 0, _vpyEnableFpsParams ? _frameRateDen : 0),
                _ => FfmpegFreeText
            };
        }
        #endregion

        #region Language switching
        private void OnLanguageChanged()
        {
            _baseAvsPrefix = UILangProviderM.Current["SrcScribe.AvsPrefix"];
            _baseVpyPrefix = UILangProviderM.Current["SrcScribe.VpyPrefix"];

            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(ScribeDescription1));
            OnPropertyChanged(nameof(ScribeDescription2));
            OnPropertyChanged(nameof(NoteText));
            OnPropertyChanged(nameof(TabAvs));
            OnPropertyChanged(nameof(TabVpy));
            OnPropertyChanged(nameof(TabFfmpeg));
            OnPropertyChanged(nameof(AvsPrefix));
            OnPropertyChanged(nameof(AvsSuffix));
            OnPropertyChanged(nameof(VpyPrefix));
            OnPropertyChanged(nameof(VpySuffix));
            OnPropertyChanged(nameof(ResolutionScaleTitle));
            OnPropertyChanged(nameof(ScalePercentLabel));
            OnPropertyChanged(nameof(HasSource));
            OnPropertyChanged(nameof(ScaleNotApplicableText));
            OnPropertyChanged(nameof(TargetDisplay));
            OnPropertyChanged(nameof(FfmpegFreeTextHint));
            OnPropertyChanged(nameof(FfmpegText));
            OnPropertyChanged(nameof(SarRepairTitle));
            OnPropertyChanged(nameof(FfmpegSarRepairFilter));
            OnPropertyChanged(nameof(FfmpegHqdn3dDenoiseFilter));
            OnPropertyChanged(nameof(FfmpegSubtitleFilter));
            OnPropertyChanged(nameof(FfmpegFpsScaleFilter));
            OnPropertyChanged(nameof(FfmpegFpsColorScaleFilter));
            OnPropertyChanged(nameof(FfmpegFullChainFilter));
            OnPropertyChanged(nameof(VapourSynthText));
            OnPropertyChanged(nameof(AviSynthText));
            OnPropertyChanged(nameof(FrameRateConvertTitle));
            OnPropertyChanged(nameof(ColorSpaceConvertTitle));
            OnPropertyChanged(nameof(DenoiseTitle));
            OnPropertyChanged(nameof(SubtitleBurnTitle));
            OnPropertyChanged(nameof(MultiFilterAssemblyTitle));
            OnPropertyChanged(nameof(LowToHighColorFilterLabel));
            OnPropertyChanged(nameof(HighToLowColorFilterLabel));
            OnPropertyChanged(nameof(HdrToSdrColorFilterLabel));
            OnPropertyChanged(nameof(HighHdrToLowSdrColorFilterLabel));
            OnPropertyChanged(nameof(ColorSpacePeakNitsHint));
            OnPropertyChanged(nameof(FfmpegLowToHighColorFilter));
            OnPropertyChanged(nameof(FfmpegHighToLowColorFilter));
            OnPropertyChanged(nameof(FfmpegHdrToSdrColorFilter));
            OnPropertyChanged(nameof(FfmpegHighHdrToLowSdrColorFilter));
            OnPropertyChanged(nameof(AvsEnableFpsParamsLabel));
            OnPropertyChanged(nameof(VpyEnableFpsParamsLabel));

            BuildButtonGroups();
            OnPropertyChanged(nameof(ScriptExportButtons));
            OnPropertyChanged(nameof(FinishScribeButtons));
        }
        #endregion

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
