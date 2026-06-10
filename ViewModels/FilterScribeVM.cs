using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.ViewModels.Cards;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// Note:
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
        private readonly Action<ToolItemCardVM, SourceFileKind, string> _afterImport;
        private readonly Action<string?> _applyFfmpegFilterArgs;
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

        public bool IsScaleApplicable => HasSource && ResolutionScaleH.IsScaleApplicable(SourceWidth, SourceHeight);

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
                if (SetProperty(ref _scalePercent, value))
                    RecomputeTarget();
            }
        }

        public void CommitScale()
        {
            if (!IsScaleApplicable) return;
            // var w, h are discard values for now
            var (_, _) = ResolutionScaleH.ComputeTargetDimensions(SourceWidth, SourceHeight, ScalePercent);
            OnPropertyChanged(nameof(TargetDisplay));
            OnPropertyChanged(nameof(FfmpegResizeFilter));
            OnPropertyChanged(nameof(FfmpegCombinedFilter));
            OnPropertyChanged(nameof(VapourSynthResizeFilter));
            OnPropertyChanged(nameof(AviSynthResizeFilter));
        }

        private int _targetWidth;
        public int TargetWidth => _targetWidth;

        private int _targetHeight;
        public int TargetHeight => _targetHeight;

        public string TargetDisplay => !HasSource ? "--" : $"{TargetWidth}x{TargetHeight}";

        public string FfmpegResizeFilter =>
            IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight)
                ? $"-filter:v scale={TargetWidth}:{TargetHeight} -sws_flags bicubic+full_chroma_int+full_chroma_inp+accurate_rnd"
                : "N/A";

        public string FfmpegFpsFilter =>
            IsFrameRateApplicable
                ? $"-filter:v fps={_frameRateNum}/{_frameRateDen}"
                : "N/A";

        public string FfmpegCombinedFilter =>
            IsFrameRateApplicable && IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight)
                ? $"-filter:v \"fps={_frameRateNum}/{_frameRateDen},scale={TargetWidth}:{TargetHeight}\" -sws_flags bicubic+full_chroma_int+full_chroma_inp+accurate_rnd"
                : "N/A";

        private string GeneratedFfmpegFilterArgs
        {
            get
            {
                bool hasFps = IsFrameRateApplicable;
                bool hasScale = IsScaleApplicable && (TargetWidth != SourceWidth || TargetHeight != SourceHeight);
                if (!hasFps && !hasScale) return string.Empty;
                if (hasFps && hasScale)
                {
                    string filterChain = $"fps={_frameRateNum}/{_frameRateDen},scale={TargetWidth}:{TargetHeight}";
                    return $"-filter:v \"{filterChain}\" -sws_flags bicubic+full_chroma_int+full_chroma_inp+accurate_rnd";
                }
                if (hasFps)
                    return $"-filter:v fps={_frameRateNum}/{_frameRateDen}";
                return $"-filter:v scale={TargetWidth}:{TargetHeight} -sws_flags bicubic+full_chroma_int+full_chroma_inp+accurate_rnd";
            }
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
                OnPropertyChanged(nameof(FfmpegCombinedFilter));
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
        public static string WindowTitle => "1cenc Script Generator";
        public static string ScribeDescription1 => UILangProviderM.Current["SrcScribe.Description1"];
        public static string ScribeDescription2 => UILangProviderM.Current["SrcScribe.Description2"];
        public static string NoteText => UILangProviderM.Current["SrcScribe.NoteText"];
        public static string TabAvs => UILangProviderM.Current["SrcScribe.TabAvs"];
        public static string TabVpy => UILangProviderM.Current["SrcScribe.TabVpy"];
        public static string TabFfmpeg => UILangProviderM.Current["SrcScribe.TabFfmpeg"];
        public static string ResolutionScaleTitle => UILangProviderM.Current["SrcScribe.ResolutionScaleTitle"];
        public static string ScalePercentLabel => UILangProviderM.Current["SrcScribe.ScalePercentLabel"];
        public static string FfmpegFreeTextHint => UILangProviderM.Current["SrcScribe.FfmpegFreeTextHint"];
        public static string FfmpegAutoFilter => "ffmpeg";
        public static string VapourSynthAutoFilter => "VS";
        public static string AviSynthAutoFilter => "AVS(+)";
        public static string FrameRateConvertTitle => UILangProviderM.Current["SrcScribe.FrameRateConvertTitle"];
        #endregion

        public ButtonGroupVM ScriptExportButtons { get; private set; } = null!;
        public ButtonGroupVM FinishScribeButtons { get; private set; } = null!;

        public FilterScribeVM(
            ModalNavS modalNavS,
            Action closeAction,
            Func<string> getSourcePath,
            ToolItemCardVM avsItem,
            ToolItemCardVM vpyItem,
            Action<ToolItemCardVM, SourceFileKind, string> afterImport,
            Action<string?> applyFfmpegFilterArgs,
            string? sourceFfprobeJson = null)
        {
            _modalNavS = modalNavS;
            _closeAction = closeAction;
            CloseCmd = new CloseModalCmd(closeAction);
            _getSourcePath = getSourcePath;
            _avsItem = avsItem;
            _vpyItem = vpyItem;
            _afterImport = afterImport;
            _applyFfmpegFilterArgs = applyFfmpegFilterArgs;
            _baseAvsPrefix = UILangProviderM.Current["SrcScribe.AvsPrefix"];
            _baseVpyPrefix = UILangProviderM.Current["SrcScribe.VpyPrefix"];
            ParseSourceResolution(sourceFfprobeJson);
            ParseFrameRateInfo(sourceFfprobeJson);
            BuildButtonGroups();
            UILangProviderM.CurrentChanged += OnLanguageChanged;
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
                OnPropertyChanged(nameof(FfmpegCombinedFilter));
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
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
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
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                UILangProviderM.Current["SrcScribe.CopiedSection"]).Execute(null);
        }
        private void SaveAsFile()
        {
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

        private void SaveAndImportAll()
        {
            ApplyFfmpegFilterArgs();

            string sourcePath = _getSourcePath();
            string avsScript = ScriptTemplateH.BuildAvsExportScript(
                sourcePath, AvsPrefix2, AvsSuffix, AvsUserInput,
                _avsEnableFpsParams ? _frameRateNum : 0, _avsEnableFpsParams ? _frameRateDen : 0);
            string vpyScript = ScriptTemplateH.BuildVpyExportScript(
                sourcePath, VpyPrefix2, VpySuffix, VpyUserInput,
                _vpyEnableFpsParams ? _frameRateNum : 0, _vpyEnableFpsParams ? _frameRateDen : 0);

            SaveFileDialog dialog = new()
            {
                Title = UILangProviderM.Current["SrcScribe.SavingWindowTitle"],
                Filter = UILangProviderM.Current["SrcScribe.FilterAvs"],
                FileName = GetScriptFileName(sourcePath, ".avs")
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true) return;

            string avsPath = dialog.FileName;
            string directory = Path.GetDirectoryName(avsPath) ?? ".";
            string vpyPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(avsPath) + ".vpy");

            if (!TryWriteScripts(avsPath, avsScript, vpyPath, vpyScript)) return;

            ImportScript(_avsItem, SourceFileKind.AviSynthScript, avsPath);
            ImportScript(_vpyItem, SourceFileKind.VapourSynthScript, vpyPath);
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Scripts saved:\n{avsPath}\n{vpyPath}").Execute(null);
            _closeAction();
        }

        private void ApplyFfmpegFilterArgsOnly()
        {
            ApplyFfmpegFilterArgs();
            _closeAction();
        }

        private void ApplyFfmpegFilterArgs()
        {
            string generated = GeneratedFfmpegFilterArgs;
            string freeText = FfmpegFreeText.Trim();
            string args = string.Join(" ", new[] { generated, freeText }.Where(s => !string.IsNullOrWhiteSpace(s)));
            _applyFfmpegFilterArgs(args);
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

        private void ShowSaveError(Exception ex)
        {
            new OpenErrModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Failed to save scripts: {ex.Message}").Execute(null);
        }

        private void ShowSavedMessage(string path)
        {
            new OpenInfoModalCmd(
                _modalNavS,
                UILangProviderM.Current["SrcScribe.WindowTitle"],
                $"Script saved:\n{path}").Execute(null);
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
            OnPropertyChanged(nameof(FfmpegAutoFilter));
            OnPropertyChanged(nameof(FfmpegCombinedFilter));
            OnPropertyChanged(nameof(VapourSynthAutoFilter));
            OnPropertyChanged(nameof(AviSynthAutoFilter));
            OnPropertyChanged(nameof(FrameRateConvertTitle));
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
        }
    }
}
