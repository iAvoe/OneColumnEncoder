using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Commands.SaveLoad;
using OneColumnEncoder.ToolManagement;
using OneColumnEncoder.UI;
using OneColumnEncoder.FileManagement;
using OneColumnEncoder.Validation;
using OneColumnEncoder.QueueManagement;
using OneColumnEncoder.Pipeline;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly AppDataM _appDataM;
        private readonly AppConfM _appConfM;
        private readonly ModalNavS _modalNavS;
        private readonly VideoAnalysisM _srcVideoAnalysis = new();
        private readonly ToolItemCardVM? _outputSettingCard;
        private readonly VideoSourceQueueState _videoSourceQueue;
        private string _scriptScribeFfmpegFilterArgs = string.Empty;
        private bool _isMiniUpstreamsZone;
        private bool _isMiniEncodersZone;
        private bool _isMiniAnalyticsZone;
        private bool _isMiniDependenciesZone;
        private bool _isMiniVideoSrcImportZone;
        private bool _isMiniScriptSrcImportZone;
        private bool _isMiniEncodingConfZone;
        private bool _showBestPracticesCard;
        private string _upstreamsZoneSelectedPath = string.Empty;
        private string _encodersZoneSelectedPath = string.Empty;
        private string _analyticsZoneSelectedPath = string.Empty;
        private string _dependenciesZoneSelectedPath = string.Empty;
        private string _videoSrcImportZoneSelectedPath = string.Empty;
        private string _activeScriptSrcImportZoneSelectedPath = string.Empty;
        private string _encodingConfZoneSelectedPath = string.Empty;

        // Groups of Card or other element UIs
        public ObservableCollection<ToolItemCardVM> UpstreamsZone { get; }
        public ObservableCollection<ToolItemCardVM> EncodersZone { get; }
        public ObservableCollection<ToolItemCardVM> AnalyticsZone { get; } // A-D separated for dual single-select
        public ObservableCollection<ToolItemCardVM> DependenciesZone { get; }
        public ObservableCollection<ToolItemCardVM> VideoSrcImportZone { get; } // V-S separated for dual single-select
        public ObservableCollection<ToolItemCardVM> ScriptSrcImportZone { get; }
        public ObservableCollection<ToolItemCardVM> QueueScriptSrcImportZone { get; }
        public ObservableCollection<ToolItemCardVM> EncodingConfZone { get; }
        private ObservableCollection<ToolItemCardVM> _activeScriptSrcImportZone = null!;
        public ObservableCollection<ToolItemCardVM> ActiveScriptSrcImportZone
        {
            get => _activeScriptSrcImportZone;
            private set
            {
                ObservableCollection<ToolItemCardVM>? previousZone = _activeScriptSrcImportZone;
                if (!SetProperty(ref _activeScriptSrcImportZone, value)) return;

                // The first assignment happens before all startup state is ready; only rewire after a previous zone exists.
                if (previousZone != null)
                {
                    UnsubZoneItemsCollectionChanged(previousZone);
                    SubZoneItemsCollectionChanged(value);
                    RefreshAllZoneSelectedPaths();
                }
            }
        }
        // Cmds and buttons
        public OpenUsagesCmd OpenUsages { get; }
        public OpenAppConfCmd OpenAppConf { get; }
        public ActionCmd ToggleMiniUpstreamsZoneCmd { get; }
        public ActionCmd ToggleMiniEncodersZoneCmd { get; }
        public ActionCmd ToggleMiniAnalyticsZoneCmd { get; }
        public ActionCmd ToggleMiniDependenciesZoneCmd { get; }
        public ActionCmd ToggleMiniVideoSrcImportZoneCmd { get; }
        public ActionCmd ToggleMiniScriptSrcImportZoneCmd { get; }
        public ActionCmd ToggleMiniEncodingConfZoneCmd { get; }
        public ActionCmd ToggleShowBestPracticesCardCmd { get; }
        public OneClickScriptGenCmd OneClickScriptGen { get; }
        public OpenFilterScribeCmd OpenFilterScribe { get; }
        public CopyRawAnalysisCmd CopyRawAnalysis { get; } // Copy (ffprobe JSON) to clipboard
        public AnalyzeSrcVideoCmd AnalyzeSrcVideo { get; } // Maybe add mediaInfo analysis in future, but ffprobe alone will do
        public InspectSrcProblemsCmd InspectSrcProblems { get; }
        public BypsSrcChecklistCmd BypassSrcChecklist { get; }
        public InspectEncProblemsCmd InspectEncProblems { get; }
        public BypassEncChecklistCmd BypassEncChecklist { get; }
        public OpenSampleClipCmd SampleClip { get; }
        public StartEncCmd StartEncode { get; }
        public SelectToolCmd SelectTool { get; } // ItemCard select on click
        public ButtonGroupVM OpenAppConfButtons { get; } // OpenUsages & OpenAppConf
        public ButtonGroupVM FilterScbButtons { get; } // OneClickScriptGen & OpenFilterScribe
        public ButtonGroupVM AnalyzeSrcButtons { get; } // AnalyzeSrcVideo & CopyRawAnalysis
        public ButtonGroupVM InspBypsChkButtons { get; } // InspectSrcProbelms & BypsSrcChecklist
        public ButtonGroupVM InspBypsEncChkButtons { get; } // InspectEncPreProblems & BypassEncChecklist
        public ButtonGroupVM EncStartButtons { get; }
        // Button guards
        private readonly bool _isAnalyzeSrcButtonsReady;
        private readonly bool _isInspBypsChkButtonsReady;
        private readonly bool _isInspBypsEncChkButtonsReady;
        private readonly bool _isEncStartButtonsReady;
        private bool _importedToolZonesSubscribed;
        private bool _promptScriptGenAfterAnalysis;
        // Checklist Card UIs
        public ToolsImportCardVM ToolsImportCard { get; }
        public SourceCheckCardVM SrcValidationCard { get; } = new();
        public QueueSrcFilterCardVM QueueSrcFilterCard { get; } = new();
        public EncTermsCardVM EncTermsCard { get; } = new();
        public BestPracsSelfCheckCardVM BestPracticesCard { get; } = new();
        private SourceCheckCardVM _activeSrcValidationCard = null!;
        public SourceCheckCardVM ActiveSrcValidationCard
        {
            get => _activeSrcValidationCard;
            private set => SetProperty(ref _activeSrcValidationCard, value);
        }

        // SectionHeader refreshes this dynamic lookup when the global language changes.
        private readonly LocalizedTextLookup _sectionTexts = new(new Dictionary<string, Func<string>>
        {
            ["SelectUpstream"] = () => UICaptionProviderM.Sections.SelectUpstream,
            ["SelectEncoder"] = () => UICaptionProviderM.Sections.SelectEncoder,
            ["SelectAnalytics"] = () => UICaptionProviderM.Sections.SelectAnalytics,
            ["SelectDependencies"] = () => UICaptionProviderM.Sections.SelectDependencies,
            ["ImportSource"] = () => UICaptionProviderM.Sections.ImportSource,
            ["EncodingConfigs"] = () => UICaptionProviderM.Sections.EncodingConfigs,
            ["StartEncoding"] = () => UICaptionProviderM.Sections.StartEncoding
        });

        public LocalizedTextLookup SectionTexts => _sectionTexts;

        public static string SVFIClipDisabledHintText => UICaptionProviderM.Hints.SVFIClipDisabled;
        public static string AnalyzeNeedsSourceText => UICaptionProviderM.Hints.AnalyzeNeedsSource;
        public static string NumaCpuCheckHintText => UICaptionProviderM.Hints.NumaCpuCheckTrigger;

        public sealed class LocalizedTextLookup(IReadOnlyDictionary<string, Func<string>> getters)
        {
            private readonly IReadOnlyDictionary<string, Func<string>> _getters = getters;

            public string this[string key] => _getters.TryGetValue(key, out Func<string>? getter)
                ? getter()
                : key;
        }
        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }
        private bool _isEncoding;
        public bool IsEncoding
        {
            get => _isEncoding;
            set => SetProperty(ref _isEncoding, value);
        }
        // Hide SVFI hint when unselected
        private bool _svfiClipDisabledHintVisible;
        public bool SVFIClipDisabledHintVisible
        {
            get => _svfiClipDisabledHintVisible;
            set => SetProperty(ref _svfiClipDisabledHintVisible, value);
        }

        public bool IsMiniUpstreamsZone
        {
            get => _isMiniUpstreamsZone;
            set => SetProperty(ref _isMiniUpstreamsZone, value);
        }

        public string ToggleMiniUpstreamsZoneText =>
            IsMiniUpstreamsZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public bool IsMiniEncodersZone
        {
            get => _isMiniEncodersZone;
            set => SetProperty(ref _isMiniEncodersZone, value);
        }

        public bool IsMiniAnalyticsZone
        {
            get => _isMiniAnalyticsZone;
            set => SetProperty(ref _isMiniAnalyticsZone, value);
        }

        public bool IsMiniDependenciesZone
        {
            get => _isMiniDependenciesZone;
            set => SetProperty(ref _isMiniDependenciesZone, value);
        }

        public bool IsMiniVideoSrcImportZone
        {
            get => _isMiniVideoSrcImportZone;
            set => SetProperty(ref _isMiniVideoSrcImportZone, value);
        }

        public bool IsMiniScriptSrcImportZone
        {
            get => _isMiniScriptSrcImportZone;
            set => SetProperty(ref _isMiniScriptSrcImportZone, value);
        }

        public bool IsMiniEncodingConfZone
        {
            get => _isMiniEncodingConfZone;
            set => SetProperty(ref _isMiniEncodingConfZone, value);
        }

        public bool ShowBestPracticesCard
        {
            get => _showBestPracticesCard;
            set => SetProperty(ref _showBestPracticesCard, value);
        }

        public string ToggleMiniEncodersZoneText =>
            IsMiniEncodersZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string ToggleMiniAnalyticsZoneText =>
            IsMiniAnalyticsZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string ToggleMiniDependenciesZoneText =>
            IsMiniDependenciesZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string ToggleMiniVideoSrcImportZoneText =>
            IsMiniVideoSrcImportZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string ToggleMiniScriptSrcImportZoneText =>
            IsMiniScriptSrcImportZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string ToggleMiniEncodingConfZoneText =>
            IsMiniEncodingConfZone
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string ToggleShowBestPracticesCardText =>
            ShowBestPracticesCard
                ? UILangProviderM.Current["Expand"]
                : UILangProviderM.Current["Collapse"];

        public string UpstreamsZoneSelectedPath
        {
            get => _upstreamsZoneSelectedPath;
            set => SetProperty(ref _upstreamsZoneSelectedPath, value);
        }

        public string EncodersZoneSelectedPath
        {
            get => _encodersZoneSelectedPath;
            set => SetProperty(ref _encodersZoneSelectedPath, value);
        }

        public string AnalyticsZoneSelectedPath
        {
            get => _analyticsZoneSelectedPath;
            set => SetProperty(ref _analyticsZoneSelectedPath, value);
        }

        public string DependenciesZoneSelectedPath
        {
            get => _dependenciesZoneSelectedPath;
            set => SetProperty(ref _dependenciesZoneSelectedPath, value);
        }

        public string VideoSrcImportZoneSelectedPath
        {
            get => _videoSrcImportZoneSelectedPath;
            set => SetProperty(ref _videoSrcImportZoneSelectedPath, value);
        }

        public string ActiveScriptSrcImportZoneSelectedPath
        {
            get => _activeScriptSrcImportZoneSelectedPath;
            set => SetProperty(ref _activeScriptSrcImportZoneSelectedPath, value);
        }

        public string EncodingConfZoneSelectedPath
        {
            get => _encodingConfZoneSelectedPath;
            set => SetProperty(ref _encodingConfZoneSelectedPath, value);
        }

        private ObservableCollection<ToolItemCardVM>[] AllImportedToolZones =>
            [UpstreamsZone, EncodersZone, AnalyticsZone, DependenciesZone];

        #region Constructor
        public MainVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages, AppDataM appDataM, AppConfM appConfM, ModalNavS modalNavS)
        {
            // Capture persistent models and restore UI collapse state before commands bind to them.
            _appDataM = appDataM;
            _appConfM = appConfM;
            _modalNavS = modalNavS;
            _isMiniUpstreamsZone = _appDataM.IsMiniUpstreamsZone ?? false;
            _isMiniEncodersZone = _appDataM.IsMiniEncodersZone ?? false;
            _isMiniAnalyticsZone = _appDataM.IsMiniAnalyticsZone ?? false;
            _isMiniDependenciesZone = _appDataM.IsMiniDependenciesZone ?? false;
            _isMiniVideoSrcImportZone = _appDataM.IsMiniVideoSrcImportZone ?? false;
            _isMiniScriptSrcImportZone = _appDataM.IsMiniScriptSrcImportZone ?? false;
            _isMiniEncodingConfZone = _appDataM.IsMiniEncodingConfZone ?? false;
            _showBestPracticesCard = _appDataM.ShowBestPracticesCard ?? false;
            OpenAppConf = openAppConf;
            OpenUsages = openUsages;

            // Build simple commands first because later UI groups reference them directly.
            SelectTool = new SelectToolCmd(this);
            ToggleMiniUpstreamsZoneCmd = new ActionCmd(_ =>
            {
                IsMiniUpstreamsZone = !IsMiniUpstreamsZone;
                _appDataM.IsMiniUpstreamsZone = IsMiniUpstreamsZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniUpstreamsZoneText));
            });
            ToggleMiniEncodersZoneCmd = new ActionCmd(_ =>
            {
                IsMiniEncodersZone = !IsMiniEncodersZone;
                _appDataM.IsMiniEncodersZone = IsMiniEncodersZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniEncodersZoneText));
            });
            ToggleMiniAnalyticsZoneCmd = new ActionCmd(_ =>
            {
                IsMiniAnalyticsZone = !IsMiniAnalyticsZone;
                _appDataM.IsMiniAnalyticsZone = IsMiniAnalyticsZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniAnalyticsZoneText));
            });
            ToggleMiniDependenciesZoneCmd = new ActionCmd(_ =>
            {
                IsMiniDependenciesZone = !IsMiniDependenciesZone;
                _appDataM.IsMiniDependenciesZone = IsMiniDependenciesZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniDependenciesZoneText));
            });
            ToggleMiniVideoSrcImportZoneCmd = new ActionCmd(_ =>
            {
                IsMiniVideoSrcImportZone = !IsMiniVideoSrcImportZone;
                _appDataM.IsMiniVideoSrcImportZone = IsMiniVideoSrcImportZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniVideoSrcImportZoneText));
            });
            ToggleMiniScriptSrcImportZoneCmd = new ActionCmd(_ =>
            {
                IsMiniScriptSrcImportZone = !IsMiniScriptSrcImportZone;
                _appDataM.IsMiniScriptSrcImportZone = IsMiniScriptSrcImportZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniScriptSrcImportZoneText));
            });
            ToggleMiniEncodingConfZoneCmd = new ActionCmd(_ =>
            {
                IsMiniEncodingConfZone = !IsMiniEncodingConfZone;
                _appDataM.IsMiniEncodingConfZone = IsMiniEncodingConfZone;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleMiniEncodingConfZoneText));
            });
            ToggleShowBestPracticesCardCmd = new ActionCmd(_ =>
            {
                ShowBestPracticesCard = !ShowBestPracticesCard;
                _appDataM.ShowBestPracticesCard = ShowBestPracticesCard;
                _appDataM.Save();
                OnPropertyChanged(nameof(ToggleShowBestPracticesCardText));
            });
            ActiveSrcValidationCard = SrcValidationCard;

            // Create static card zones, then restore imported tools and cached sources.
            ToolsImportCard = new ToolsImportCardVM(modalNavS);
            VideoSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetVideoSrcImportDefs(), true, false);
            _videoSourceQueue = new(VideoSrcImportZone);
            ScriptSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetScriptSrcImportDefs(), true, false);
            QueueScriptSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetScriptSrcImportQueueDefs(), false, false);
            ActiveScriptSrcImportZone = ScriptSrcImportZone;
            EncodingConfZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetEncSettingsDefinitions(), enableRealCheck: false);
            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
            DependenciesZone = [];
            LoadToolsFromAppDataM();
            LoadSourcesFromAppDataM();
            WireUpZoneDeleteCmds();

            // Restore encoding cards after source import so output defaults can sync with source state.
            ToolItemCardVM? outputSetting = EncodingConfZone.FirstOrDefault(t => t.Name.Equals(
                UILangProviderM.Current["Tool.Enc.OutputSetting"],
                StringComparison.OrdinalIgnoreCase));
            _outputSettingCard = outputSetting;

            // Set P2Text to desktop, then P1Text to file name
            if (outputSetting != null)
            {
                string cachedOutputDirectory = NormalizeOutputDirectory(_appDataM.Encoding.OutputDirectory);
                outputSetting.PropertyChanged += OnOutputSettingPropertyChanged;
                outputSetting.InitializeOutputSetting(cachedOutputDirectory);
            }

            // Load saved parallelism settings onto the card
            ToolItemCardVM? parallelismCard = EncodingConfZone.FirstOrDefault(t => t.Name.Equals(
                UILangProviderM.Current["Tool.Enc.Parallelism"],
                StringComparison.OrdinalIgnoreCase));
            if (parallelismCard != null)
                ParallelismConfVM.ApplySavedSettingsToCard(parallelismCard);

            // Build workflow commands after zones exist so delegates can resolve current selections lazily.
            OneClickScriptGen = new OneClickScriptGenCmd(
                () => GetCurrentVideoSourcePath(),
                () => ActiveScriptSrcImportZone[0],
                () => ActiveScriptSrcImportZone[1],
                UpstreamsZone,
                modalNavS,
                IsQueueRouteActive,
                GetCurrentQueueFilePaths);
            OpenFilterScribe = new OpenFilterScribeCmd(
                modalNavS,
                () => GetCurrentVideoSourcePath(),
                () => ActiveScriptSrcImportZone[0],
                () => ActiveScriptSrcImportZone[1],
                () => SourceFileKindResolver.GetPreferredScriptSourceKind(UpstreamsZone),
                OnSourceImported,
                args => _scriptScribeFfmpegFilterArgs = args ?? string.Empty,
                () => SrcValidationCard.Checklist1.Any(
                    e => e.IsEnabled && e.Status == StatusType.Error),
                () => SrcValidationCard.Checklist2.Count > 1
                    && SrcValidationCard.Checklist2[1].Status == StatusType.Warning,
                () => _srcVideoAnalysis.RawJson,
                () => UpstreamsZone.Any(
                    t => t.IsSelected &&
                    ToolDefinitionProviderM.IsImportedTool(t.Name, "one_line_shot_args.exe")),
                IsQueueRouteActive,
                GetCurrentQueueFilePaths);
            CopyRawAnalysis = new CopyRawAnalysisCmd(
                _srcVideoAnalysis, modalNavS, IsQueueRouteActive);
            AnalyzeSrcVideo = new AnalyzeSrcVideoCmd(
                GetSelectedFfprobePath,
                GetSelectedVideoSourcePath,
                _srcVideoAnalysis,
                () => ActiveSrcValidationCard,
                modalNavS,
                IsQueueRouteActive,
                GetCurrentQueueFilePaths,
                OnSourceQueueAccepted,
                OnSourceAnalysisCompleted,
                () =>
                { // On source analysis complete
                    UpdateAnalyzeSrcButtonsState();
                    UpdateEncStartButtonsState();
                });
            InspectSrcProblems = new InspectSrcProblemsCmd(
                _srcVideoAnalysis, () => ActiveSrcValidationCard, modalNavS);
            BypassSrcChecklist = new BypsSrcChecklistCmd(
                () => ActiveSrcValidationCard,
                () => !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson),
                UpdateEncStartButtonsState);
            SampleClip = new OpenSampleClipCmd(
                modalNavS,
                BuildEncodingPipelineRequest,
                _srcVideoAnalysis,
                IsQueueRouteActive);
            StartEncode = new StartEncCmd(
                BuildEncodingPipelineRequest,
                modalNavS,
                appConfM,
                IsQueueRouteActive,
                GetCurrentQueueJsonPath,
                BuildQueueEncodingPipelineRequests,
                IsQueueRouteSupported);

            // Build button groups after commands so initial CanExecute refreshes have valid targets.
            OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.UsageAndCompliance, UICaptionProviderM.Buttons.Settings, OpenUsages, OpenAppConf);
            OpenAppConfButtons.B2_1Icon = SvgIconProvider.GamePhone;
            OpenAppConfButtons.B2_2Icon = SvgIconProvider.GameSetting;
            FilterScbButtons = ButtonGroupVM.CreateTwoButton( // UpdateFilterScbButtonsState()
                UICaptionProviderM.Buttons.OneClickScriptGen, UICaptionProviderM.Buttons.OpenScribeSrcScribe, OneClickScriptGen, OpenFilterScribe);
            AnalyzeSrcButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.CopyRawAnalysis, UICaptionProviderM.Buttons.AnalyzeSrcVideo, CopyRawAnalysis, AnalyzeSrcVideo);
            _isAnalyzeSrcButtonsReady = true;
            EncStartButtons = ButtonGroupVM.CreateThreeButton( // UpdateEncStartButtonsState()
                UICaptionProviderM.Buttons.ReEvaluate, UICaptionProviderM.Buttons.RunSample, UICaptionProviderM.Buttons.StartEncode,
                new ActionCmd(_ => ReEvaluateAllChecks()), SampleClip, StartEncode);
            EncStartButtons.B3_1Icon = SvgIconProvider.GameRefresh;
            EncStartButtons.B3_2Icon = SvgIconProvider.GameLocation;
            EncStartButtons.B3_3Icon = SvgIconProvider.GamePlay;
            _isEncStartButtonsReady = true;
            InspBypsChkButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.InspectSrcProbelms, UICaptionProviderM.Buttons.BypassSrcChecklist, InspectSrcProblems, BypassSrcChecklist);
            _isInspBypsChkButtonsReady = true;

            InspectEncProblems = new InspectEncProblemsCmd(EncTermsCard, modalNavS);
            BypassEncChecklist = new BypassEncChecklistCmd(EncTermsCard, UpdateEncStartButtonsState);
            InspBypsEncChkButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.InspectEncPreProblems, UICaptionProviderM.Buttons.BypassEncChecklist, InspectEncProblems, BypassEncChecklist);
            _isInspBypsEncChkButtonsReady = true;

            // Import dropdown menu and behavior
            ToolsImportCard.ToolImported += OnToolImported;
            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;

            foreach (DropdownItemM item in ToolCatalogProviderM.GetImportDropdownItems())
                ToolsImportCard.ImportDropdown.Items.Add(item);
            ToolsImportCard.ImportDropdown.SelectedItem = ToolsImportCard.ImportDropdown.Items[0];

            // Configure validation cards and deferred getters used by checks and encoding requests.
            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceIncompatOrCorrupted;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SrcQualityIssues;
            QueueSrcFilterCard.RefreshLanguage();
            EncTermsCard.Name = UICaptionProviderM.Cards.EncPrerequisites;
            EncTermsCard.P1Name = UICaptionProviderM.Cards.EncHardware;
            EncTermsCard.P3Name = UICaptionProviderM.Cards.EncSoftware;
            BestPracticesCard.Name = UICaptionProviderM.Cards.BestPractices;
            BestPracticesCard.P1Name = UICaptionProviderM.Cards.BestHardware;
            BestPracticesCard.P3Name = UICaptionProviderM.Cards.BestSoftware;
            BestPracticesCard.Subtitle = UICaptionProviderM.Cards.BestPracticesSubtitle;

            SrcValidationCard.IsSvtav1SelectedFunc = () =>
                EncodersZone.Any(t => t.IsSelected
                    && ToolDefinitionProviderM.IsImportedTool(t.Name, "svtav1encapp.exe"));
            QueueSrcFilterCard.IsSvtav1SelectedFunc = SrcValidationCard.IsSvtav1SelectedFunc;

            EncTermsCard.GetOutputDirectoryFunc = () =>
            {
                ToolItemCardVM? output = EncodingConfZone.FirstOrDefault(t =>
                    t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));
                return output?.P2TextData ?? string.Empty;
            };
            EncTermsCard.GetOutputFilePathFunc = () =>
            {
                ToolItemCardVM? output = EncodingConfZone.FirstOrDefault(t =>
                    t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));
                if (output is null || string.IsNullOrWhiteSpace(output.P2TextData) || string.IsNullOrWhiteSpace(output.P1TextData))
                    return string.Empty;

                return Path.Combine(output.P2TextData, output.P1TextData);
            };
            EncTermsCard.IsAvs2yuvSelectedFunc = () =>
                UpstreamsZone.Any(t => t.IsSelected
                    && ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2yuv.exe"));
            EncTermsCard.GetAviSynthDllPathFunc = () =>
            {
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                return string.IsNullOrWhiteSpace(programFilesX86)
                    ? string.Empty
                    : Path.Combine(programFilesX86, "AviSynth+", "plugins64+");
            };
            EncTermsCard.GetSourceVideoFilePathFunc = () =>
            {
                return GetSelectedVideoSourcePath();
            };

            // Run final state refreshes after all cards, commands, and subscriptions are ready.
            EncTermsCard.RunAllChecks();
            SyncOutputFilenameWithVideoSource();
            SubToImportedToolZones();
            AnalyticsZone.CollectionChanged += OnAnalyticsZoneCollectionChanged;
            RefreshImportedToolStates(); // initial state after loading
            RevertCancelledAutoSelection(UpstreamsZone);
            RevertCancelledAutoSelection(DependenciesZone);
            SubToToolsChecklist();
            UpdateFilterScbButtonsState(); // Initial state of script scribe buttons
            RefreshSelectedSourceStatus();
            UpdateAnalyzeSrcButtonsState();
            UpdateInspBypsChkButtonsState();
            UpdateInspBypsEncChkButtonsState();
            _modalNavS.CurrentViewModelChanged += OnModalStateChanged;
            IsOverlayVisible = _modalNavS.IsOpen;
            UILangProviderM.CurrentChanged += OnLanguageChanged;
            RefreshLanguage();
            SubToAllZoneItemChanges();
            RefreshAllZoneSelectedPaths();
            _ = Application.Current.Dispatcher.InvokeAsync(async () => await TryAutoImportToolsOnStartupAsync());
        }
        #endregion

        #region Startup Auto Tool Import

        private async Task TryAutoImportToolsOnStartupAsync()
        {
            if (!_appConfM.IsFirstLaunch) return;

            try
            {
                IReadOnlyList<AutoToolImport.Candidate> candidates =
                    await AutoToolImport.FindImportableToolsAsync(_appDataM.Tools);

                if (candidates.Count == 0)
                {
                    ShowAutoImportInfo(
                        UILangProviderM.Current["AutoImport.Title"],
                        UILangProviderM.Current["AutoImport.NotFoundMessage"]);
                    return;
                }

                if (!ShowAutoImportConfirmation(candidates)) return;

                foreach (AutoToolImport.Candidate candidate in candidates)
                {
                    await OnToolImported(candidate.ExeName, candidate.FilePath, candidate.Version);
                }
            }
            finally
            {
                _appConfM.IsFirstLaunch = false;
                _appConfM.Save();
            }
        }

        private bool ShowAutoImportConfirmation(IReadOnlyList<AutoToolImport.Candidate> candidates)
        {
            string itemText = string.Join(Environment.NewLine, candidates.Select(candidate => string.Format(
                UILangProviderM.Current["AutoImport.ItemFormat"],
                candidate.ExeName,
                candidate.Version,
                candidate.FilePath)));
            string message = string.Format(UILangProviderM.Current["AutoImport.FoundMessage"], itemText);

            ConfirmationModal window = new();
            ConfirmationVM vm = ConfirmationVM.CreateInfo(
                UILangProviderM.Current["AutoImport.Title"],
                message,
                new ActionCmd(_ => { window.DialogResult = false; window.Close(); }),
                new ActionCmd(_ => { window.DialogResult = true; window.Close(); }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            return window.ShowDialog() == true;
        }

        private void ShowAutoImportInfo(string title, string message)
        {
            ConfirmationModal window = new();
            CloseModalCmd closeCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateInfo(title, message, closeCmd, closeCmd);

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        #endregion

        // Zone Initialization
        private static ObservableCollection<ToolItemCardVM> LoadZoneFromDefinitions(
            List<ToolDefinitionM> defs,
            bool useAutoAddReplaceText = false,
            bool enableRealCheck = true)
        {
            ObservableCollection<ToolItemCardVM> zone = [];
            foreach (ToolDefinitionM def in defs)
            {
                ToolItemCardVM item = new(new EncItemM(def.DisplayName))
                {
                    R1Text = def.R1Text,
                    R2Text = def.R2Text,
                    P1Name = def.P1Name,
                    P2Name = def.P2Name ?? "",
                    UseAutoAddReplaceText = useAutoAddReplaceText,
                    EnableRealCheck = enableRealCheck
                };
                item.R2Command = new RemoveZoneItemCmd(item, zone);
                zone.Add(item);
            }
            return zone;
        }

        #region Imported Zone Event Handling
        private void SubToImportedToolZones()
        {
            foreach (ObservableCollection<ToolItemCardVM> zone in AllImportedToolZones)
                zone.CollectionChanged += OnImportedToolZoneCollectionChanged;
            _importedToolZonesSubscribed = true;
            RefreshImportedToolsChecklist();
        }
        private void UnsubFromImportedToolZones()
        {
            foreach (ObservableCollection<ToolItemCardVM> zone in AllImportedToolZones)
                zone.CollectionChanged -= OnImportedToolZoneCollectionChanged;
            _importedToolZonesSubscribed = false;
        }
        private void OnAnalyticsZoneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAnalyzeSrcButtonsState();
            UpdateInspBypsChkButtonsState();
        }

        // When tools are added or removed in imported zones, re-apply default selection logic,
        // also refresh states of related buttons and checklists
        private void OnImportedToolZoneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            bool autoSelected = false;
            if (sender is ObservableCollection<ToolItemCardVM> zone)
                autoSelected = ApplyDefaultImportedToolSelection(zone);

            if (sender == EncodersZone)
                SrcValidationCard.RefreshSvtav1BitDepthStatus();

            if (sender == UpstreamsZone)
                RefreshEncTermsState();

            RefreshUpstreamToolState();
            RefreshVspipeAvailability();
            RefreshImportedToolsChecklist();
            // Only mark IsCancel for auto-selected items, user manual selection won't be reverted
            ToolCompatibility.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibility.RefreshSourceSelectionState(
                UpstreamsZone, ActiveScriptSrcImportZone, () => RefreshSelectedSourceStatus());
            ToolCompatibility.RefreshVideoSourceSelectionState(
                UpstreamsZone, VideoSrcImportZone);

            // Revert the selection for IsCancel caused by "Auto Selection".
            // Must revert both zones because RefreshDependencySelectionState can set
            // IsCancel on items in either UpstreamsZone or DependenciesZone regardless
            // of which zone triggered the change.
            if (autoSelected)
            {
                RevertCancelledAutoSelection(UpstreamsZone);
                RevertCancelledAutoSelection(DependenciesZone);
            }
        }

        private bool ApplyDefaultImportedToolSelection(ObservableCollection<ToolItemCardVM> zone)
        {
            bool autoSelected = ItemCardSelection.ApplyDefaultSelection(zone);
            RefreshImportedToolPickedStatus(zone);
            return autoSelected;
        }

        private void RevertCancelledAutoSelection(ObservableCollection<ToolItemCardVM> zone)
        {
            bool reverted = false;
            foreach (ToolItemCardVM item in zone.Where(t => t.IsCancel))
            {
                item.IsSelected = false;
                item.IsCancel = false;
                reverted = true;
            }

            if (!reverted) return;

            RefreshImportedToolPickedStatus(zone);
            ToolCompatibility.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibility.RefreshSourceSelectionState(
                UpstreamsZone, ActiveScriptSrcImportZone, () => RefreshSelectedSourceStatus());
            ToolCompatibility.RefreshVideoSourceSelectionState(
                UpstreamsZone, VideoSrcImportZone);
        }

        private void RefreshUpstreamToolState()
        {
            ToolItemCardVM? avs2pipemod = UpstreamsZone.FirstOrDefault(
                t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            if (avs2pipemod == null) return;

            if (!HasImportedAviSynthDll()) avs2pipemod.IsSelected = false;
            // avs2pipemod.IsEnabled = false; // This prevents delete button to work, not feasible
            // else avs2pipemod.IsEnabled = true;

            RefreshToolPickedStatus(ToolZone.Upstream, UpstreamsZone);
        }

        private void RefreshImportedToolsChecklist()
        {
            ToolsImportCard.RefreshToolsChecklist(
                hasUpstreamTool: UpstreamsZone.Count > 0,
                hasEncoderTool: EncodersZone.Count > 0,
                hasFfprobe: HasImportedFfprobe());
        }

        private void RefreshImportedToolStates()
        {
            RefreshUpstreamToolState();
            RefreshVspipeAvailability();
            RefreshImportedToolsChecklist();
            RefreshEncTermsState();
            ToolCompatibility.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibility.RefreshSourceSelectionState(
                UpstreamsZone, ActiveScriptSrcImportZone, () => RefreshSelectedSourceStatus());
            ToolCompatibility.RefreshVideoSourceSelectionState(
                UpstreamsZone, VideoSrcImportZone);
        }

        private void RefreshEncTermsState()
        {
            bool isAvs2yuvSelected = UpstreamsZone.Any(t => t.IsSelected
                && ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2yuv.exe"));

            EncTermsCard.SetLsmashCheckEnabled(isAvs2yuvSelected);
            EncTermsCard.RunAllChecks();
        }

        private void RefreshEncSettingsState()
        {
            bool hasAnySource = BothSourceSelected();
            foreach (ToolItemCardVM item in EncodingConfZone)
                item.IsEnabled = hasAnySource;
        }

        private void RefreshScriptSourceEnabledState()
        {
            if (IsQueueRouteActive()) return;

            bool hasVideoSource = !string.IsNullOrWhiteSpace(GetCurrentVideoSourcePath());
            if (hasVideoSource) return;

            foreach (ToolItemCardVM item in ScriptSrcImportZone)
            {
                item.IsSelected = false;
                item.IsEnabled = false;
            }
        }

        private void SyncOutputFilenameWithVideoSource(string? filePath = null)
        {
            ToolItemCardVM? outputSetting = EncodingConfZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));
            if (outputSetting == null) return;

            string? sourcePath = filePath;
            if (string.IsNullOrWhiteSpace(sourcePath))
                sourcePath = GetSelectedVideoSourcePath();

            if (string.IsNullOrWhiteSpace(sourcePath)) return;

            outputSetting.RefreshOutputSetting(false, _modalNavS, sourcePath);
        }

        private bool HasImportedFfprobe() =>
            !string.IsNullOrWhiteSpace(_appDataM.Tools.FfprobePath);
        private bool HasImportedAviSynthDll() =>
            !string.IsNullOrWhiteSpace(_appDataM.Tools.AviSynthDllPath);
        #endregion

        #region Validation Checklists
        private void SubToToolsChecklist()
        {
            foreach (ChecklistEntryVM entry in ToolsImportCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in ToolsImportCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in QueueSrcFilterCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in QueueSrcFilterCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            UpdateEncStartButtonsState();
        }
        private void UnsubFromToolsChecklist()
        {
            foreach (ChecklistEntryVM entry in ToolsImportCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in ToolsImportCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in QueueSrcFilterCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in QueueSrcFilterCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
        }
        private void OnChecklistEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChecklistEntryVM.Status))
            {
                UpdateEncStartButtonsState();
                if (_modalNavS.CurrentModalVM is FilterScribeVM modal)
                    modal.RefreshGeneratedFfmpegFilters();
            }
        }
        #endregion

        #region Button state updates
        public void UpdateFilterScbButtonsState()
        {
            bool oneLineShotSelected = UpstreamsZone.Any(
                t => t.IsSelected &&
                ToolDefinitionProviderM.IsImportedTool(t.Name, "one_line_shot_args.exe"));

            bool hasVideoSrc = HasSelectedVideoSource();

            if (oneLineShotSelected)
            {
                FilterScbButtons.B2_1IsEnabled = false;
                FilterScbButtons.B2_2IsEnabled = false;
            }
            else
            {
                FilterScbButtons.B2_1IsEnabled = true;
                FilterScbButtons.B2_2IsEnabled = hasVideoSrc;
            }

            if (_modalNavS.CurrentModalVM is FilterScribeVM modal)
            {
                modal.ScriptExportButtons.B3_1IsEnabled = !oneLineShotSelected && hasVideoSrc;
                modal.ScriptExportButtons.B3_2IsEnabled = !oneLineShotSelected && hasVideoSrc;
                modal.ScriptExportButtons.B3_3IsEnabled = !oneLineShotSelected && hasVideoSrc;
            }

            OneClickScriptGen.OnCanExecuteChanged();
        }
        public void UpdateEncStartButtonsState()
        {
            if (!_isEncStartButtonsReady) return;

            bool vspipeReady = UpstreamsZone.All(t =>
                !ToolDefinitionProviderM.IsImportedTool(t.Name, "vspipe.exe") || t.IsEnabled);

            bool toolsReady =
                UpstreamsZone.Count > 0 && EncodersZone.Count > 0 && HasImportedFfprobe() && vspipeReady;

            bool toolsChecklistReady =
                ToolsImportCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                ToolsImportCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);

            bool hasRawJson = !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson);

            // Checklist:
            // 1. Selected 
            // SVT-AV1 may eventually have 12bit support, but for now keep encoding start button disabled
            SourceCheckCardVM activeSrcCard = ActiveSrcValidationCard;
            bool sourceValidationReady = activeSrcCard.IsBypassed ||
                (hasRawJson &&
                activeSrcCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                activeSrcCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success));

            bool encodeTermsReady = EncTermsCard.IsBypassed ||
                EncTermsCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                EncTermsCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);
            bool avsSelected = UpstreamsZone.Any(
                t => t.IsSelected && ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            bool aviSelected = DependenciesZone.Any(
                t => t.IsSelected && ToolDefinitionProviderM.IsImportedTool(t.Name, "avisynth.dll"));
            bool dependencyReady = avsSelected == aviSelected;

            // SVFI currently doesn't support clipping, and its not really built with basic editing in design principle,
            // disable clip sampling if SVFI is selected as upstream to avoid confusion
            bool oneLineShotSelected = UpstreamsZone.Any(
                t => t.IsSelected &&
                ToolDefinitionProviderM.IsImportedTool(t.Name, "one_line_shot_args.exe"));

            bool allReady = toolsReady && toolsChecklistReady && sourceValidationReady && encodeTermsReady && dependencyReady;
            EncStartButtons.B3_2IsEnabled = allReady && !oneLineShotSelected && !IsQueueRouteActive();
            EncStartButtons.B3_3IsEnabled = allReady;
            SVFIClipDisabledHintVisible = oneLineShotSelected;
        }

        private void RefreshToolSourceChecklistStatus()
        {
            bool hasVideoSource = HasSelectedVideoSource();
            ToolsImportCard.SetVideoSourcePickedStatus(hasVideoSource);

            SourceFileKind? expectedKind = GetExpectedScriptSourceKindForSelectedUpstream();
            bool scriptSourcePicked = expectedKind == null || IsScriptSourceSelected(expectedKind.Value);
            ToolsImportCard.SetScriptSourcePickedStatus(expectedKind != null, scriptSourcePicked);
        }

        private void ReEvaluateAllChecks()
        {
            EncTermsCard.RunAllChecks();
            UpdateEncStartButtonsState();
        }

        public void RefreshNumaCpuCheck()
        {
            EncTermsCard.RunAllChecks();
            UpdateEncStartButtonsState();
        }
        #endregion

        #region Command Wiring (Bind R1-R2)
        private void WireUpZoneDeleteCmds()
        {
            foreach (ToolItemCardVM tool in VideoSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemCardVM tool in ScriptSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemCardVM tool in QueueScriptSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemCardVM tool in EncodingConfZone) WireUpStaticClearCmd(tool);
            WireUpEncSettingsCmds();
            foreach (ObservableCollection<ToolItemCardVM> zone in AllImportedToolZones)
                foreach (ToolItemCardVM tool in zone) WireUpToolCmd(tool);
        }
        private void WireUpToolCmd(ToolItemCardVM item)
        {
            item.R1Command = new ReplaceToolCmd(
                item, _appDataM, _modalNavS, () =>
                {
                    RefreshImportedToolStates();
                    // After replace, the consistency check may have re-set IsCancel.
                    // Revert both zones so the user sees a clean state.
                    RevertCancelledAutoSelection(UpstreamsZone);
                    RevertCancelledAutoSelection(DependenciesZone);
                });
            item.R2Command = new DeleteToolCmd(
                item, GetZoneForTool(ToolDefinitionProviderM.ResolveToolZone(item.Name)), _appDataM);

            ToolZone zone = ToolDefinitionProviderM.ResolveToolZone(item.Name);
            if (zone == ToolZone.Upstream)
                item.PropertyChanged += OnUpstreamItemPropertyChanged;
            if (zone == ToolZone.Encoder)
                item.PropertyChanged += OnEncoderItemPropertyChanged;
            if (zone == ToolZone.Analytics)
                item.PropertyChanged += OnAnalyticsItemPropertyChanged;
        }
        private void WireUpSourceCmd(ToolItemCardVM item)
        {
            if (IsVideoSourceQueueItem(item))
            {
                item.R1Command = new BrowseSourceQueueCmd(item, _modalNavS, OnSourceQueueImported);
                item.R2Command = new ClearToolItemCmd(item, () => OnSourceQueueCleared(item));
                item.PropertyChanged += OnVideoSrcItemPropertyChanged;
                return;
            }

            SourceFileKind kind = SourceFileKindResolver.ResolveSourceFileKind(item.Name);
            if (QueueScriptSrcImportZone.Contains(item))
            {
                item.R1Command = new BrowseSourceScriptQueueCmd(item, kind, OnSourceScriptQueueImported, GetCurrentSourceImportPath);
                item.R2Command = new ClearToolItemCmd(item, () => OnSourceScriptQueueCleared(item));
            }
            else
            {
                item.R1Command = kind == SourceFileKind.Video
                    ? new BrowseSourcePathCmd(item, kind, _appDataM, _modalNavS, OnVideoSourceImported)
                    : new BrowseSourcePathCmd(item, kind, _appDataM, _modalNavS, OnVideoSourceImported, GetCurrentSourceImportPath);
                item.R2Command = new ClearToolItemCmd(item, () => OnSourceCleared(kind));
            }
            item.PropertyChanged += OnVideoSrcItemPropertyChanged;
        }
        private static void WireUpStaticClearCmd(ToolItemCardVM item) =>
            item.R2Command = new ClearToolItemCmd(item);

        private void RefreshOutputSettingCommand(ToolItemCardVM? outputSetting = null)
        {
            outputSetting ??= _outputSettingCard;
            if (outputSetting == null) return;

            outputSetting.RefreshOutputSetting(IsQueueRouteActive(), _modalNavS, GetSelectedVideoSourcePath());
        }

        private void WireUpEncSettingsCmds()
        {
            if (EncodingConfZone.Count > 1)
                EncodingConfZone[1].R1Command = new OpenParallelismConfCmd(_modalNavS, EncodingConfZone[1]);

            ToolItemCardVM? outputSetting = EncodingConfZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));

            if (outputSetting != null)
                RefreshOutputSettingCommand(outputSetting);

            ToolItemCardVM? compressionParams = EncodingConfZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.EncParams"], StringComparison.OrdinalIgnoreCase));

            if (compressionParams != null)
            {
                compressionParams.R1Command = new OpenEncoderConfCmd(
                    _modalNavS,
                    compressionParams,
                    () => _appDataM.Tools.FfmpegPath,
                    GetPreviewSourceVideoPath,
                    () => _srcVideoAnalysis.RawJson);
                EncoderConfVM.ApplySavedSettingsToCard(compressionParams);
            }
        }

        private string GetPreviewSourceVideoPath() =>
            IsQueueRouteActive()
                ? GetCurrentQueueFilePaths().FirstOrDefault() ?? string.Empty
                : GetSelectedVideoSourcePath();
        private void OnVideoSrcItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolItemCardVM) return;
            if (e.PropertyName is nameof(ToolItemCardVM.P2TextData) or nameof(ToolItemCardVM.IsSelected))
                RefreshSelectedSourceStatus(resetAnalysis: false);
        }

        private void OnOutputSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolItemCardVM outputSetting) return;
            if (e.PropertyName != nameof(ToolItemCardVM.P2TextData)) return;

            _appDataM.Encoding.OutputDirectory = NormalizeOutputDirectory(outputSetting.P2TextData);
            _appDataM.Save();
            EncTermsCard.RunAllChecks();
            UpdateEncStartButtonsState();
        }
        private void OnEncoderItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolItemCardVM) return;
            if (e.PropertyName == nameof(ToolItemCardVM.IsSelected))
            {
                SrcValidationCard.RefreshSvtav1BitDepthStatus();
                UpdateEncStartButtonsState();
            }
        }

        private void OnUpstreamItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolItemCardVM) return;
            if (e.PropertyName == nameof(ToolItemCardVM.IsSelected))
            {
                RefreshToolSourceChecklistStatus();
                RefreshEncTermsState();
            }
        }

        private void OnAnalyticsItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolItemCardVM) return;
            if (e.PropertyName is nameof(ToolItemCardVM.P2TextData) or nameof(ToolItemCardVM.IsSelected))
            {
                ResetAnalysisIfStale();
                UpdateAnalyzeSrcButtonsState();
                UpdateInspBypsChkButtonsState();
                UpdateEncStartButtonsState();
            }
        }

        public void SelectItemCard(ToolItemCardVM clickedTool)
        {
            if (ShouldRunPrimaryCardCommandOnClick(clickedTool))
            {
                if (clickedTool.R1Command?.CanExecute(null) == true)
                    clickedTool.R1Command.Execute(null);
                return;
            }

            ItemCardSelection.HandleItemCardClick(
                clickedTool,
                UpstreamsZone, EncodersZone, AnalyticsZone, DependenciesZone,
                VideoSrcImportZone, ActiveScriptSrcImportZone,
                ToolsImportCard,
                RefreshSelectedSourceStatusAfterSourceSelection,
                UpdateEncStartButtonsState,
                () => RefreshSelectedSourceStatus());
        }

        private bool ShouldRunPrimaryCardCommandOnClick(ToolItemCardVM clickedTool) =>
            VideoSrcImportZone.Contains(clickedTool) ||
            ScriptSrcImportZone.Contains(clickedTool) ||
            QueueScriptSrcImportZone.Contains(clickedTool) ||
            EncodingConfZone.Contains(clickedTool);

        private void RefreshToolPickedStatus(ToolZone toolZone, ObservableCollection<ToolItemCardVM> itemZone) =>
            ItemCardSelection.RefreshToolPickedStatus(ToolsImportCard, toolZone, itemZone);

        private void RefreshImportedToolPickedStatus(ObservableCollection<ToolItemCardVM> itemZone)
        {
            if (itemZone == UpstreamsZone)
                RefreshToolPickedStatus(ToolZone.Upstream, itemZone);
            else if (itemZone == EncodersZone)
                RefreshToolPickedStatus(ToolZone.Encoder, itemZone);
            else if (itemZone == AnalyticsZone)
                RefreshToolPickedStatus(ToolZone.Analytics, itemZone);
        }

        // File save & ItemCard write back logic after FilterScribeModal completes
        private void OnSourceImported(ToolItemCardVM item, SourceFileKind kind, string filePath)
        {
            if (kind != SourceFileKind.Video)
            {
                string? error = ValidateSingleScriptImport(kind, filePath);
                if (error != null)
                {
                    ClearSourceItem(item);
                    SaveSourcePath(kind, string.Empty);
                    _appDataM.Save();
                    new OpenErrModalCmd(_modalNavS, UILangProviderM.Current["Warn.SourceCheck"], error).Execute(null);
                    RefreshSelectedSourceStatus(resetAnalysis: false);
                    return;
                }
            }

            SaveSourcePath(kind, filePath);

            if (kind == SourceFileKind.Video)
            {
                SyncOutputFilenameWithVideoSource(filePath);

                foreach (ToolItemCardVM source in VideoSrcImportZone)
                    source.IsSelected = false;

                ClearScriptSourceZone(ScriptSrcImportZone);
                ClearScriptSourceZone(QueueScriptSrcImportZone);
                SaveSourcePath(SourceFileKind.AviSynthScript, string.Empty);
                SaveSourcePath(SourceFileKind.VapourSynthScript, string.Empty);
                SaveSourcePath(SourceFileKind.SvfiIni, string.Empty);
            }
            else
            {
                if (ShouldSelectImportedScriptSource(kind))
                {
                    foreach (ToolItemCardVM source in ActiveScriptSrcImportZone)
                        source.IsSelected = false;
                }
            }

            bool shouldSelectImportedSource = kind == SourceFileKind.Video || ShouldSelectImportedScriptSource(kind);
            if (item.IsEnabled && shouldSelectImportedSource) item.IsSelected = true;
            _appDataM.Save();
            RefreshSelectedSourceStatus(resetAnalysis: kind == SourceFileKind.Video);
        }

        private bool ShouldSelectImportedScriptSource(SourceFileKind kind)
        {
            SourceFileKind? preferredKind = SourceFileKindResolver.GetPreferredScriptSourceKind(UpstreamsZone);
            return preferredKind == null || kind == preferredKind.Value;
        }

        private string? ValidateSingleScriptImport(SourceFileKind kind, string filePath)
        {
            string videoPath = GetCurrentVideoSourcePath();
            ScriptSourceValidationIssue? issue = ScriptSourceValidation.ValidateSingle(kind, filePath, videoPath);
            return issue == null ? null : FormatScriptImportIssues([issue]);
        }

        private void OnVideoSourceImported(ToolItemCardVM item, SourceFileKind kind, string filePath, bool wasReplaced)
        {
            OnSourceImported(item, kind, filePath);

            if (kind == SourceFileKind.Video && wasReplaced)
            {
                PromptRunSourceAnalysisAfterReplace();
            }
        }

        private bool HasGeneratableScriptUpstream() =>
            UpstreamsZone.Any(t => t.IsSelected &&
                !string.IsNullOrWhiteSpace(t.P2TextData) &&
                (ToolDefinitionProviderM.IsImportedTool(t.Name, "vspipe.exe") ||
                 ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2yuv.exe") ||
                 ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe")));

        private void PromptScriptGenerationAfterReplace()
        {
            if (!HasGeneratableScriptUpstream()) return;
            if (!OneClickScriptGen.CanExecute(null)) return;

            ConfirmationModal window = new();
            CloseModalCmd cancelCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateInfo(
                UILangProviderM.ScriptGenWindowTitle,
                UILangProviderM.Current["ScriptGen.RunAfterReplace"],
                cancelCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    if (OneClickScriptGen.CanExecute(null))
                        OneClickScriptGen.Execute(null);
                    _appDataM.Save();
                }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }

        private void OnSourceAnalysisCompleted(bool isSuccess)
        {
            ToolsImportCard.SetCompleteSourceAnalysisStatus(isSuccess);

            if (!isSuccess)
            {
                _promptScriptGenAfterAnalysis = false;
                return;
            }

            if (_promptScriptGenAfterAnalysis)
            {
                _promptScriptGenAfterAnalysis = false;
                PromptScriptGenerationAfterReplace();
            }
        }

        private void PromptRunSourceAnalysisAfterReplace(bool promptScriptGenAfterAnalysis = true)
        {
            if (!AnalyzeSrcVideo.CanExecute(null)) return;

            ConfirmationModal window = new();
            CloseModalCmd cancelCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateInfo(
                UILangProviderM.SrcAnalysisWindowTitle,
                UILangProviderM.Current["SrcAnalysis.RunAfterReplace"],
                cancelCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    _promptScriptGenAfterAnalysis = promptScriptGenAfterAnalysis;
                    if (AnalyzeSrcVideo.CanExecute(null))
                        AnalyzeSrcVideo.Execute(null);
                }));

            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            window.Closed += (_, _) => _modalNavS.Close();
            _modalNavS.CurrentModalVM = vm;
            window.ShowDialog();
        }
        private void OnSourceCleared(SourceFileKind kind)
        {
            SaveSourcePath(kind, string.Empty);
            _appDataM.Save();
            RefreshSelectedSourceStatus(
                resetAnalysis: kind == SourceFileKind.Video || !HasSelectedVideoSource());
        }

        private void OnSourceQueueImported(ToolItemCardVM item, string folderPath, string[] filePaths)
        {
            _videoSourceQueue.ApplyImportedFiles(item, filePaths);

            foreach (ToolItemCardVM source in VideoSrcImportZone)
                source.IsSelected = false;

            ClearScriptSourceZone(ScriptSrcImportZone);
            ClearScriptSourceZone(QueueScriptSrcImportZone);

            SaveSourcePath(SourceFileKind.Video, string.Empty);
            SaveSourcePath(SourceFileKind.AviSynthScript, string.Empty);
            SaveSourcePath(SourceFileKind.VapourSynthScript, string.Empty);
            SaveSourcePath(SourceFileKind.SvfiIni, string.Empty);

            if (filePaths.Length > 0)
                item.IsSelected = true;
            _appDataM.Save();
            RefreshSelectedSourceStatus(resetAnalysis: true);
            if (filePaths.Length > 0)
                PromptRunSourceAnalysisAfterReplace(promptScriptGenAfterAnalysis: false);
        }

        private void OnSourceQueueCleared(ToolItemCardVM item)
        {
            // The queue card label is restored by VideoSourceQueueState.
            // This handler only clears the stored files and refreshes selection state.
            _videoSourceQueue.Clear(item);
            RefreshSelectedSourceStatus(resetAnalysis: !HasSelectedVideoSource());
        }

        private void OnSourceScriptQueueImported(ToolItemCardVM item, SourceFileKind kind, string folderPath, string[] filePaths)
        {
            string? error = ValidateScriptQueueImport(kind, filePaths);
            if (error != null)
            {
                ClearSourceItem(item);
                new OpenErrModalCmd(_modalNavS, UILangProviderM.Current["Warn.SourceCheck"], error).Execute(null);
                RefreshSelectedSourceStatus(resetAnalysis: false);
                return;
            }

            foreach (ToolItemCardVM source in ActiveScriptSrcImportZone)
                source.IsSelected = false;

            if (filePaths.Length > 0)
                item.IsSelected = true;

            RefreshSelectedSourceStatus(resetAnalysis: false);
        }

        private string? ValidateScriptQueueImport(SourceFileKind kind, string[] filePaths)
        {
            string[] videoPaths = GetCurrentQueueFilePaths();
            if (videoPaths.Length == 0) return null;

            IReadOnlyList<ScriptSourceValidationIssue> issues =
                ScriptSourceValidation.ValidateQueue(kind, filePaths, videoPaths);
            return issues.Count == 0 ? null : FormatScriptImportIssues(issues);
        }

        private static string FormatScriptImportIssues(IReadOnlyList<ScriptSourceValidationIssue> issues)
        {
            const int maxDetails = 5;
            List<string> details = [];
            int unmatchedCount = issues.Count(issue => issue.Kind == ScriptSourceValidationIssueKind.NoMatchingVideoSource);
            int mismatchCount = issues.Count - unmatchedCount;

            foreach (ScriptSourceValidationIssue issue in issues.Take(maxDetails))
            {
                string scriptName = Path.GetFileName(issue.ScriptPath);
                string detail = issue.Kind switch
                {
                    ScriptSourceValidationIssueKind.NoMatchingVideoSource =>
                        string.Format(UILangProviderM.Current["ScriptQueueImport.DetailNoMatch"], scriptName),
                    ScriptSourceValidationIssueKind.UnreadableScript =>
                        string.Format(UILangProviderM.Current["ScriptQueueImport.DetailUnreadable"], scriptName),
                    _ => string.Format(
                        UILangProviderM.Current["ScriptQueueImport.DetailMismatch"],
                        scriptName,
                        issue.EmbeddedPath ?? string.Empty,
                        issue.ExpectedPath ?? string.Empty)
                };
                details.Add(detail);
            }

            int omitted = issues.Count - Math.Min(issues.Count, maxDetails);
            string msg = string.Format(UILangProviderM.Current["ScriptQueueImport.RejectedPrefix"], unmatchedCount, mismatchCount);
            msg += "\n\n" + UILangProviderM.Current["ScriptQueueImport.DetailsHeader"] + "\n" + string.Join("\n", details);
            if (omitted > 0) msg += "\n" + string.Format(UILangProviderM.Current["ScriptQueueImport.MoreCount"], omitted);
            return msg;
        }

        private void OnSourceScriptQueueCleared(ToolItemCardVM item)
        {
            item.IsSelected = false;
            RefreshSelectedSourceStatus(resetAnalysis: !HasSelectedVideoSource());
        }

        private static void ClearScriptSourceZone(IEnumerable<ToolItemCardVM> zone)
        {
            foreach (ToolItemCardVM script in zone)
                ClearSourceItem(script);
        }

        private static void ClearSourceItem(ToolItemCardVM item)
        {
            item.P2TextData = string.Empty;
            item.P1TextData = string.Empty;
            item.P1TooltipText = null; // Reset tooltip to fall back to P1TextData
            item.IsSelected = false;
        }

        private void OnSourceQueueAccepted(string[] acceptedFilePaths, string queueJsonPath)
        {
            _videoSourceQueue.ApplyAcceptedFiles(acceptedFilePaths);
            RefreshSelectedSourceStatus(resetAnalysis: false);
        }

        private void SaveSourcePath(SourceFileKind kind, string filePath)
        {
            if (kind == SourceFileKind.Video)
                _appDataM.Tools.VideoSourcePath = filePath;
        }
        public void RefreshSelectedSourceStatus(bool resetAnalysis = false)
        {
            RefreshActiveSourceRoute();
            SelectMatchingScriptSourceForSelectedUpstream();
            bool anySelected =
                VideoSrcImportZone.Any(t => t.IsSelected) ||
                ActiveScriptSrcImportZone.Any(t => t.IsSelected);
            if (resetAnalysis)
            {
                _srcVideoAnalysis.Clear();
                ActiveSrcValidationCard.ResetAnalysisStatus();
                ToolsImportCard.ResetCompleteSourceAnalysisStatus();
            }

            // Keep import-zone HintPanel paths in sync after any source change
            RefreshAllZoneSelectedPaths();
            RefreshToolSourceChecklistStatus();
            UpdateFilterScbButtonsState();
            UpdateAnalyzeSrcButtonsState();
            UpdateEncStartButtonsState();
        }

        public void RefreshSelectedSourceStatusAfterSourceSelection()
        {
            ResetAnalysisIfStale();
            RefreshSelectedSourceStatus();
        }
        public void UpdateAnalyzeSrcButtonsState()
        {
            if (!_isAnalyzeSrcButtonsReady) return;

            bool hasVideoSource = CanRunSourceAnalysis();

            RefreshEncSettingsState();
            AnalyzeSrcButtons.B2_2IsEnabled = hasVideoSource;
            AnalyzeSrcButtons.B2_1IsEnabled = !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson);
            CopyRawAnalysis.OnCanExecuteChanged();
            AnalyzeSrcVideo.OnCanExecuteChanged();
            UpdateInspBypsChkButtonsState();
        }
        public void UpdateInspBypsChkButtonsState()
        {
            if (!_isInspBypsChkButtonsReady) return;

            bool hasRawJson = !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson);
            if (!hasRawJson && ActiveSrcValidationCard.IsBypassed)
                ActiveSrcValidationCard.SetBypassed(false);

            InspBypsChkButtons.B2_1IsEnabled = hasRawJson;
            InspBypsChkButtons.B2_2IsEnabled = hasRawJson;
            InspectSrcProblems.OnCanExecuteChanged();
            BypassSrcChecklist.OnCanExecuteChanged();
        }
        public void UpdateInspBypsEncChkButtonsState()
        {
            if (!_isInspBypsEncChkButtonsReady) return;

            InspBypsEncChkButtons.B2_1IsEnabled = true;
            InspBypsEncChkButtons.B2_2IsEnabled = true;
            InspectEncProblems.OnCanExecuteChanged();
            BypassEncChecklist.OnCanExecuteChanged();
        }
        private string GetCurrentVideoSourcePath()
        {
            ToolItemCardVM? videoSrc = VideoSrcImportZone.FirstOrDefault(t => !IsVideoSourceQueueItem(t) && !string.IsNullOrWhiteSpace(t.P2TextData));
            return videoSrc?.P2TextData ?? string.Empty;
        }

        private string GetCurrentSourceImportPath()
        {
            if (IsQueueRouteActive())
            {
                ToolItemCardVM? queueSrc = VideoSrcImportZone.FirstOrDefault(t => IsVideoSourceQueueItem(t) && !string.IsNullOrWhiteSpace(t.P2TextData));
                return queueSrc?.P2TextData ?? string.Empty;
            }

            return GetCurrentVideoSourcePath();
        }

        private bool CanRunSourceAnalysis() =>
            HasSelectedVideoSource() &&
            AnalyticsZone.Any(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));

        private bool IsCurrentAnalysisFor(string sourcePath, string ffprobePath) =>
            !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson) &&
            string.Equals(_srcVideoAnalysis.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_srcVideoAnalysis.FfprobePath, ffprobePath, StringComparison.OrdinalIgnoreCase);

        private void ResetAnalysisIfStale()
        {
            if (IsCurrentAnalysisFor(GetSelectedVideoSourcePath(), GetSelectedFfprobePath())) return;

            _srcVideoAnalysis.Clear();
            ActiveSrcValidationCard.ResetAnalysisStatus();
            ToolsImportCard.ResetCompleteSourceAnalysisStatus();
        }

        private void RefreshActiveSourceRoute()
        {
            bool queueActive = IsQueueRouteActive();
            ActiveSrcValidationCard = queueActive
                ? QueueSrcFilterCard
                : SrcValidationCard;
            ActiveScriptSrcImportZone = queueActive
                ? QueueScriptSrcImportZone
                : ScriptSrcImportZone;
            ToolCompatibility.RefreshSourceSelectionState(
                UpstreamsZone, ActiveScriptSrcImportZone, () => { });
            RefreshScriptSourceEnabledState();
            ToolCompatibility.RefreshVideoSourceSelectionState(
                UpstreamsZone, VideoSrcImportZone);
            RefreshOutputSettingCommand();

            if (_outputSettingCard != null)
            {
                if (queueActive)
                {
                    _outputSettingCard.RefreshOutputSetting(true, _modalNavS);
                }
                else
                {
                    SyncOutputFilenameWithVideoSource();
                }
            }
        }

        private bool IsQueueRouteActive() =>
            _videoSourceQueue.IsActive;

        private string[] GetCurrentQueueFilePaths() =>
            _videoSourceQueue.CurrentFilePaths;

        private string GetCurrentQueueJsonPath() =>
            QueueSrcFilterCard.QueueJsonPath;

        private bool IsQueueRouteSupported()
        {
            ToolItemCardVM? upstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected && t.IsEnabled && !string.IsNullOrWhiteSpace(t.P2TextData));
            string? upstreamExeName = upstream == null
                ? null
                : ToolCatalogProviderM.ResolveExeFromDisplayName(upstream.Name);
            return SourceFileKindResolver.IsQueueRouteSupportedUpstream(upstreamExeName);
        }

        private string GetSelectedVideoSourcePath()
        {
            ToolItemCardVM? videoSrc = GetSelectedSingleVideoSource();
            return videoSrc?.P2TextData ?? string.Empty;
        }

        private ToolItemCardVM? GetSelectedSingleVideoSource() =>
            VideoSrcImportZone.FirstOrDefault(t =>
                !IsVideoSourceQueueItem(t) &&
                t.IsSelected &&
                !string.IsNullOrWhiteSpace(t.P2TextData));

        private bool HasSelectedVideoSource() =>
            IsQueueRouteActive()
                ? GetCurrentQueueFilePaths().Length > 0
                : GetSelectedSingleVideoSource() != null;

        private bool BothSourceSelected() =>
            HasSelectedVideoSource() && HasRequiredScriptSourceSelected();

        private bool HasRequiredScriptSourceSelected()
        {
            SourceFileKind? expectedKind = GetExpectedScriptSourceKindForSelectedUpstream();
            return expectedKind == null || IsScriptSourceSelected(expectedKind.Value);
        }

        private void SelectMatchingScriptSourceForSelectedUpstream()
        {
            SourceFileKind? expectedKind = GetExpectedScriptSourceKindForSelectedUpstream();
            if (expectedKind == null) return;

            ToolItemCardVM? target = ActiveScriptSrcImportZone.FirstOrDefault(t =>
                t.IsEnabled &&
                !string.IsNullOrWhiteSpace(t.P2TextData) &&
                SourceFileKindResolver.ResolveSourceFileKind(t.Name) == expectedKind.Value);
            if (target == null) return;

            foreach (ToolItemCardVM source in ActiveScriptSrcImportZone)
            {
                bool shouldSelect = source == target;
                if (source.IsSelected != shouldSelect)
                    source.IsSelected = shouldSelect;
            }
        }

        private bool IsScriptSourceSelected(SourceFileKind expectedKind) =>
            ActiveScriptSrcImportZone.Any(t =>
                t.IsSelected &&
                !string.IsNullOrWhiteSpace(t.P2TextData) &&
                SourceFileKindResolver.ResolveSourceFileKind(t.Name) == expectedKind);

        private SourceFileKind? GetExpectedScriptSourceKindForSelectedUpstream()
        {
            ToolItemCardVM? selectedUpstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected);
            string? exe = selectedUpstream == null
                ? null
                : ToolCatalogProviderM.ResolveExeFromDisplayName(selectedUpstream.Name);

            return exe switch
            {
                "vspipe.exe" => SourceFileKind.VapourSynthScript,
                "avs2yuv.exe" or "avs2pipemod.exe" => SourceFileKind.AviSynthScript,
                "one_line_shot_args.exe" => SourceFileKind.SvfiIni,
                _ => null
            };
        }

        private string GetSelectedFfprobePath()
        {
            ToolItemCardVM? ffprobe = AnalyticsZone.FirstOrDefault(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
            return ffprobe?.P2TextData ?? string.Empty;
        }

        private EncodingPipelineRequest? BuildEncodingPipelineRequest()
        {
            if (!BothSourceSelected()) return null;

            ToolItemCardVM? upstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected && t.IsEnabled && !string.IsNullOrWhiteSpace(t.P2TextData));
            ToolItemCardVM? encoder = EncodersZone.FirstOrDefault(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
            ToolItemCardVM? outputSetting = EncodingConfZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));

            if (upstream == null || encoder == null || outputSetting == null) return null;

            string? upstreamExeName = ToolCatalogProviderM.ResolveExeFromDisplayName(upstream.Name);
            string? encoderExeName = ToolCatalogProviderM.ResolveExeFromDisplayName(encoder.Name);
            if (string.IsNullOrWhiteSpace(upstreamExeName) || string.IsNullOrWhiteSpace(encoderExeName)) return null;

            string upstreamInputPath = GetUpstreamInputPath(upstreamExeName);
            string sourceVideoPath = GetSelectedVideoSourcePath();
            string? svfiIniPath = null;
            string? svfiTaskId = null;

            if (upstreamExeName.Equals("one_line_shot_args.exe", StringComparison.OrdinalIgnoreCase))
            {
                svfiIniPath = GetSelectedSvfiIniPath();
                if (!string.IsNullOrWhiteSpace(svfiIniPath))
                {
                    var (iniInputPath, iniTaskId) = EncodingPipeline.ParseSvfiIni(svfiIniPath);
                    if (!string.IsNullOrWhiteSpace(iniInputPath))
                        upstreamInputPath = iniInputPath;
                    svfiTaskId = iniTaskId;
                }
            }

            if (string.IsNullOrWhiteSpace(upstreamInputPath) || string.IsNullOrWhiteSpace(outputSetting.P2TextData)) return null;

            return new EncodingPipelineRequest(
                upstreamExeName,
                upstream.P2TextData,
                upstreamInputPath,
                encoderExeName,
                encoder.P2TextData,
                _appDataM.Tools.FfmpegPath,
                sourceVideoPath,
                Path.Combine(outputSetting.P2TextData, outputSetting.P1TextData ?? string.Empty),
                EncoderConfM.Load(),
                _appDataM.Tools.VspipeY4mArg,
                SourceFfprobeJson: _srcVideoAnalysis.RawJson,
                ParallelismConf: ParallelismConfM.LoadEffective(),
                SvfiIniPath: svfiIniPath,
                SvfiTaskId: svfiTaskId,
                FfmpegFilterArgs: _scriptScribeFfmpegFilterArgs);
        }

        private EncodingPipelineRequest[]? BuildQueueEncodingPipelineRequests(string[] sourcePaths)
        {
            if (!BothSourceSelected()) return null;

            ToolItemCardVM? upstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected && t.IsEnabled && !string.IsNullOrWhiteSpace(t.P2TextData));
            ToolItemCardVM? encoder = EncodersZone.FirstOrDefault(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
            ToolItemCardVM? outputSetting = EncodingConfZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));

            if (upstream == null || encoder == null || outputSetting == null) return null;

            string? upstreamExeName = ToolCatalogProviderM.ResolveExeFromDisplayName(upstream.Name);
            string? encoderExeName = ToolCatalogProviderM.ResolveExeFromDisplayName(encoder.Name);
            if (string.IsNullOrWhiteSpace(upstreamExeName) || string.IsNullOrWhiteSpace(encoderExeName)) return null;
            if (string.IsNullOrWhiteSpace(outputSetting.P2TextData)) return null;

            EncoderConfM encoderConf = EncoderConfM.Load();
            ParallelismConfM parallelismConf = ParallelismConfM.LoadEffective();
            string outputDirectory = outputSetting.P2TextData;
            Dictionary<string, string> queueFfprobeJsonByPath = LoadQueueFfprobeJsonByPath();
            string GetSourceFfprobeJson(string sourcePath) =>
                queueFfprobeJsonByPath.TryGetValue(sourcePath, out string? rawJson)
                    ? rawJson
                    : _srcVideoAnalysis.RawJson;

            string? scriptDir = null;
            SourceFileKind scriptKind = SourceFileKind.Video;
            if (upstreamExeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase))
            {
                scriptKind = SourceFileKind.VapourSynthScript;
                ToolItemCardVM? vpyItem = ActiveScriptSrcImportZone.FirstOrDefault(t =>
                    t.IsSelected && SourceFileKindResolver.ResolveSourceFileKind(t.Name) == scriptKind && !string.IsNullOrWhiteSpace(t.P2TextData));
                if (vpyItem == null) return null;
                scriptDir = vpyItem.P2TextData;
            }
            else if (upstreamExeName.Equals("avs2yuv.exe", StringComparison.OrdinalIgnoreCase) ||
                     upstreamExeName.Equals("avs2pipemod.exe", StringComparison.OrdinalIgnoreCase))
            {
                scriptKind = SourceFileKind.AviSynthScript;
                ToolItemCardVM? avsItem = ActiveScriptSrcImportZone.FirstOrDefault(t =>
                    t.IsSelected && SourceFileKindResolver.ResolveSourceFileKind(t.Name) == scriptKind && !string.IsNullOrWhiteSpace(t.P2TextData));
                if (avsItem == null) return null;
                scriptDir = avsItem.P2TextData;
            }

            if (scriptDir != null)
            {
                string ext = scriptKind == SourceFileKind.VapourSynthScript ? ".vpy" : ".avs";
                const int maxListed = 5;
                int missingCount = 0;
                List<string> missingFiles = [];
                int mismatchCount = 0;
                List<string> mismatchFiles = [];
                foreach (string sourcePath in sourcePaths)
                {
                    string scriptPath = Path.Combine(scriptDir, Path.GetFileNameWithoutExtension(sourcePath) + ext);
                    if (!File.Exists(scriptPath))
                    {
                        missingCount++;
                        if (missingFiles.Count < maxListed)
                            missingFiles.Add(Path.GetFileName(scriptPath)!);
                    }
                    else
                    {
                        string? embeddedPath = ScriptSourceValidation.ExtractScriptSourcePath(scriptPath, ext);
                        if (embeddedPath == null)
                        {
                            mismatchCount++;
                            if (mismatchFiles.Count < maxListed)
                                mismatchFiles.Add($"{Path.GetFileName(scriptPath)} (unrecognized script format)");
                        }
                        else
                        {
                            string normalizedEmbedded = Path.GetFullPath(embeddedPath);
                            string normalizedSource = Path.GetFullPath(sourcePath);
                            if (!string.Equals(normalizedEmbedded, normalizedSource, StringComparison.OrdinalIgnoreCase))
                            {
                                mismatchCount++;
                                if (mismatchFiles.Count < maxListed)
                                    mismatchFiles.Add($"{Path.GetFileName(scriptPath)} (refers \"{embeddedPath}\", paired \"{sourcePath}\")");
                            }
                        }
                    }
                }
                if (missingCount > 0)
                {
                    string missingList = string.Join(", ", missingFiles);
                    string omitted = missingCount > maxListed
                        ? $" ({missingCount - maxListed} more)"
                        : string.Empty;
                    throw new InvalidOperationException(
                        $"Script count mismatch: {missingCount} of {sourcePaths.Length} scripts are missing for the selected upstream. Missing: {missingList}{omitted}");
                }
                if (mismatchCount > 0)
                {
                    string mismatchList = string.Join("; ", mismatchFiles);
                    string omitted = mismatchCount > maxListed
                        ? $" ({mismatchCount - maxListed} more)"
                        : string.Empty;
                    throw new InvalidOperationException(
                        $"Script source mismatch: {mismatchCount} script(s) reference a different video source. Details: {mismatchList}{omitted}");
                }
            }

            return [.. sourcePaths.Select(sourcePath =>
            {
                string inputPath = sourcePath;
                if (scriptDir != null)
                {
                    string ext = scriptKind == SourceFileKind.VapourSynthScript ? ".vpy" : ".avs";
                    inputPath = Path.Combine(scriptDir, Path.GetFileNameWithoutExtension(sourcePath) + ext);
                }

                return new EncodingPipelineRequest(
                    upstreamExeName,
                    upstream.P2TextData,
                    inputPath,
                    encoderExeName,
                    encoder.P2TextData,
                    _appDataM.Tools.FfmpegPath,
                    sourcePath,
                    Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourcePath)),
                    encoderConf,
                    _appDataM.Tools.VspipeY4mArg,
                    SourceFfprobeJson: GetSourceFfprobeJson(sourcePath),
                    ParallelismConf: parallelismConf,
                    FfmpegFilterArgs: _scriptScribeFfmpegFilterArgs);
            })];
        }

        private Dictionary<string, string> LoadQueueFfprobeJsonByPath()
        {
            string queueJsonPath = GetCurrentQueueJsonPath();
            if (string.IsNullOrWhiteSpace(queueJsonPath) || !File.Exists(queueJsonPath)) return [];

            try
            {
                string json = File.ReadAllText(queueJsonPath);
                QueueSourceData? data = JsonSerializer.Deserialize<QueueSourceData>(json);
                return data?.Entries
                    .Where(entry => !string.IsNullOrWhiteSpace(entry.FilePath) && entry.FfprobeJson.ValueKind != JsonValueKind.Undefined)
                    .GroupBy(entry => entry.FilePath, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First().FfprobeJson.GetRawText(),
                        StringComparer.OrdinalIgnoreCase) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private string GetSelectedSvfiIniPath()
        {
            ToolItemCardVM? svfiIni = ActiveScriptSrcImportZone.FirstOrDefault(t =>
                t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData) &&
                SourceFileKindResolver.ResolveSourceFileKind(t.Name) == SourceFileKind.SvfiIni);
            return svfiIni?.P2TextData ?? string.Empty;
        }

        private string GetUpstreamInputPath(string upstreamExeName)
        {
            if (upstreamExeName.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase) ||
                upstreamExeName.Equals("one_line_shot_args.exe", StringComparison.OrdinalIgnoreCase))
                return GetSelectedVideoSourcePath();

            SourceFileKind kind = upstreamExeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase)
                ? SourceFileKind.VapourSynthScript
                : SourceFileKind.AviSynthScript;

            ToolItemCardVM? source = ActiveScriptSrcImportZone.FirstOrDefault(t =>
                SourceFileKindResolver.ResolveSourceFileKind(t.Name) == kind && !string.IsNullOrWhiteSpace(t.P2TextData));
            return source?.P2TextData ?? string.Empty;
        }

        private bool IsVideoSourceQueueItem(ToolItemCardVM item) =>
            _videoSourceQueue.IsQueueItem(item);

        private sealed class QueueSourceData
        {
            public List<QueueSourceEntry> Entries { get; set; } = [];
        }

        private sealed class QueueSourceEntry
        {
            public string FilePath { get; set; } = string.Empty;
            public JsonElement FfprobeJson { get; set; }
        }

        #endregion

        #region Zone Helpers

        private static int GetToolOrderIndex(string displayName)
        {
            int i = 0;
            foreach (var kvp in ToolDefinitionProviderM.ToolDefs)
            {
                if (kvp.Value.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    return i;
                i++;
            }
            return int.MaxValue;
        }

        private ObservableCollection<ToolItemCardVM> GetZoneForTool(ToolZone zone) => zone switch
        {
            ToolZone.Upstream => UpstreamsZone,
            ToolZone.Encoder => EncodersZone,
            ToolZone.Analytics => AnalyticsZone,
            ToolZone.Dependencies => DependenciesZone,
            _ => throw new ArgumentException("Invalid tool zone")
        };

        #endregion

        #region Hint Panel Subscription & Refresh
        private void SubToAllZoneItemChanges()
        {
            SubZoneItemsCollectionChanged(UpstreamsZone);
            SubZoneItemsCollectionChanged(EncodersZone);
            SubZoneItemsCollectionChanged(AnalyticsZone);
            SubZoneItemsCollectionChanged(DependenciesZone);
            SubZoneItemsCollectionChanged(VideoSrcImportZone);
        }

        private void SubZoneItemsCollectionChanged(ObservableCollection<ToolItemCardVM> zone)
        {
            foreach (ToolItemCardVM item in zone)
                item.PropertyChanged += OnAnyToolCardPropertyChanged;
            zone.CollectionChanged += OnAnyZoneCollectionChanged;
        }

        private void UnsubZoneItemsCollectionChanged(ObservableCollection<ToolItemCardVM> zone)
        {
            foreach (ToolItemCardVM item in zone)
                item.PropertyChanged -= OnAnyToolCardPropertyChanged;
            zone.CollectionChanged -= OnAnyZoneCollectionChanged;
        }

        private void OnAnyToolCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ToolItemCardVM.IsSelected) or nameof(ToolItemCardVM.P2TextData))
                RefreshAllZoneSelectedPaths();
        }

        private void OnAnyZoneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (ToolItemCardVM item in e.OldItems)
                    item.PropertyChanged -= OnAnyToolCardPropertyChanged;
            if (e.NewItems != null)
                foreach (ToolItemCardVM item in e.NewItems)
                    item.PropertyChanged += OnAnyToolCardPropertyChanged;
            RefreshAllZoneSelectedPaths();
        }

        private void RefreshAllZoneSelectedPaths()
        {
            UpstreamsZoneSelectedPath = GetZoneSelectedPath(UpstreamsZone);
            EncodersZoneSelectedPath = GetZoneSelectedPath(EncodersZone);
            AnalyticsZoneSelectedPath = GetZoneSelectedPath(AnalyticsZone);
            DependenciesZoneSelectedPath = GetZoneSelectedPath(DependenciesZone);
            EncodingConfZoneSelectedPath = GetOutputSettingPath();
            // Import zones don't require selection — show the first item that has a path
            VideoSrcImportZoneSelectedPath = GetFirstSourcePath(VideoSrcImportZone);
            ActiveScriptSrcImportZoneSelectedPath = GetFirstSourcePath(ActiveScriptSrcImportZone);
        }

        private static string GetZoneSelectedPath(ObservableCollection<ToolItemCardVM>? zone)
        {
            ToolItemCardVM? selected = zone?.FirstOrDefault(t => t.IsSelected);
            if (selected == null || string.IsNullOrWhiteSpace(selected.P2TextData))
                return "!Path";
            return selected.P2TextData;
        }

        // Returns P2TextData of the first item with a non-empty path, regardless of selection.
        // Source items in the same zone are typically in the same directory, so the first path is sufficient.
        private string GetOutputSettingPath()
        {
            ToolItemCardVM? outputSetting = EncodingConfZone.FirstOrDefault();
            return outputSetting?.P2TextData ?? "!Path";
        }

        private static string GetFirstSourcePath(ObservableCollection<ToolItemCardVM>? zone)
        {
            ToolItemCardVM? first = zone?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.P2TextData));
            return first?.P2TextData ?? "!Path";
        }
        #endregion

        #region Loading or adding other persistent data
        private void AddOrUpdateTool(string defKey, string? filePath, string? version, long? fileSize = null)
        {
            if (!ToolDefinitionProviderM.ToolDefs.TryGetValue(defKey, out ToolDefinitionM? def)) return;
            if (def.Zone == null || string.IsNullOrEmpty(filePath)) return;

            ObservableCollection<ToolItemCardVM> zone = GetZoneForTool(def.Zone.Value);
            ToolItemCardVM? existing = zone.FirstOrDefault(i => i.Name == def.DisplayName);
            if (existing != null) zone.Remove(existing);

            ToolItemCardVM item = new(new EncItemM(def.DisplayName))
            {
                P1Name = def.P1Name,
                P2Name = def.P2Name ?? string.Empty,
                R1Text = def.R1Text,
                R2Text = def.R2Text
            };
            item.SetStoredFingerprint(fileSize);
            item.P2TextData = filePath;
            item.P1TextData = version ?? string.Empty;
            WireUpToolCmd(item);

            int insertIndex = zone.Count;
            int newOrder = GetToolOrderIndex(item.Name);
            for (int i = 0; i < zone.Count; i++)
            {
                if (newOrder < GetToolOrderIndex(zone[i].Name))
                {
                    insertIndex = i;
                    break;
                }
            }
            // This will trigger CollectionChanged, at which point the default selection,
            // dependency refresh, and IsCancel check will be executed.
            zone.Insert(insertIndex, item);

            // Try to auto-select when there is only 1 item,
            // but if IsCancel triggers, revert selection
            if (!_importedToolZonesSubscribed)
                ApplyDefaultImportedToolSelection(zone);
        }
        private void LoadToolsFromAppDataM()
        {
            AppDataM.Importables t = _appDataM.Tools;
            foreach ((string defKey, ToolDefinitionM def) in ToolDefinitionProviderM.ToolDefs)
            {
                if (def.Zone == null || def.ExeName == null) continue;

                (string? path, string? version, long? size) = def.ExeName switch
                {
                    "ffmpeg.exe" => (t.FfmpegPath, t.FfmpegVer, t.FfmpegSize),
                    "vspipe.exe" => (t.VspipePath, t.VspipeVer, t.VspipeSize),
                    "avs2yuv.exe" => (t.Avs2yuvPath, t.Avs2yuvVer, t.Avs2yuvSize),
                    "avs2pipemod.exe" => (t.Avs2pipemodPath, t.Avs2pipemodVer, t.Avs2pipemodSize),
                    "one_line_shot_args.exe" => (t.OneLineShotArgsPath, t.OneLineShotArgsVer, t.OneLineShotArgsSize),
                    "x264.exe" => (t.X264Path, t.X264Ver, t.X264Size),
                    "x265.exe" => (t.X265Path, t.X265Ver, t.X265Size),
                    "svtav1encapp.exe" => (t.SvtAv1Path, t.SvtAv1Ver, t.SvtAv1Size),
                    "ffprobe.exe" => (t.FfprobePath, t.FfprobeVer, t.FfprobeSize),
                    "avisynth.dll" => (t.AviSynthDllPath, t.AviSynthDllVer, t.AviSynthDllSize),
                    _ => (null, null, null)
                };

                if (!string.IsNullOrEmpty(path)) AddOrUpdateTool(defKey, path, version, size);
            }
        }
        private async Task OnToolImported(string exeName, string filePath, string? version)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByExeName(exeName);
            if (def == null || def.Zone == null) return;

            string defKey = ToolDefinitionProviderM.ToolDefs
                .FirstOrDefault(kvp => kvp.Value == def).Key;
            if (defKey == null) return;

            long? fileSize = ToolCatalogProviderM.GetFileSize(filePath);
            ToolCatalogProviderM.TrySetPath(exeName, _appDataM.Tools, filePath);
            ToolCatalogProviderM.TrySetVersion(exeName, _appDataM.Tools, version ?? string.Empty);
            ToolCatalogProviderM.TrySetSize(exeName, _appDataM.Tools, fileSize);

            if (exeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase))
            {
                await ToolVersionDetect.DetectAndStoreVspipeY4mArgAsync(
                    exeName,
                    filePath,
                    y4mArg => _appDataM.Tools.VspipeY4mArg = y4mArg);
            }

            _appDataM.Save();
            AddOrUpdateTool(defKey, filePath, version, fileSize);
        }

        private void LoadSourcesFromAppDataM()
        {
            bool hasVideoSource = LoadSourceItem(VideoSrcImportZone[0], SourceFileKind.Video, _appDataM.Tools.VideoSourcePath);
            VideoSrcImportZone[0].IsSelected = hasVideoSource;
            // Use non-selection lookup so the path shows even before an explicit selection occurs
            VideoSrcImportZoneSelectedPath = GetFirstSourcePath(VideoSrcImportZone);
            if (!hasVideoSource && !string.IsNullOrWhiteSpace(_appDataM.Tools.VideoSourcePath))
            {
                _appDataM.Tools.VideoSourcePath = string.Empty;
                _appDataM.Save();
            }

            // Script sources are not cached: without a selected upstream at startup,
            // the matching script ItemCard cannot be selected reliably and may leave the UI in an invalid state.
            ClearLegacyCachedScriptSources();

            RefreshEncSettingsState();
        }

        private void ClearLegacyCachedScriptSources()
        {
            if (string.IsNullOrWhiteSpace(_appDataM.Tools.AvsSourcePath) &&
                string.IsNullOrWhiteSpace(_appDataM.Tools.VpySourcePath) &&
                string.IsNullOrWhiteSpace(_appDataM.Tools.SvfiSourcePath))
                return;

            _appDataM.Tools.AvsSourcePath = string.Empty;
            _appDataM.Tools.VpySourcePath = string.Empty;
            _appDataM.Tools.SvfiSourcePath = string.Empty;
            _appDataM.Save();
        }

        private static string NormalizeOutputDirectory(string? path)
        {
            string fallbackDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (string.IsNullOrWhiteSpace(path)) return fallbackDirectory;
            if (Directory.Exists(path)) return path;

            string? directory = Path.GetDirectoryName(path);
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)
                ? directory
                : fallbackDirectory;
        }
        private static bool LoadSourceItem(ToolItemCardVM item, SourceFileKind kind, string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;

            item.P2TextData = path;
            item.P1TextData = SourceFilePicker.GetPrimaryText(kind, path);
            return true;
        }


        #endregion

        private void OnModalStateChanged()
        {
            IsOverlayVisible = _modalNavS.IsOpen;
            bool isCurrentlyEncoding = _modalNavS.CurrentModalVM is EncodingMonitorVM;

            if (isCurrentlyEncoding && !_isEncoding)
            {
                IsEncoding = true;
                Application.Current.MainWindow?.Hide();
            }
            else if (!isCurrentlyEncoding && _isEncoding)
            {
                IsEncoding = false;
                if (Application.Current.MainWindow is { Visibility: Visibility.Hidden } mw)
                    mw.Show();
            }
        }

        #region Language Switching
        private void OnLanguageChanged() { RefreshLanguage(); }
        private void RefreshLanguage()
        {
            RefreshSectionHeaders();
            RefreshButtonCaptions();
            RefreshCardsLanguage();
            RefreshZoneLanguage();
        }
        private void RefreshSectionHeaders()
        {
            OnPropertyChanged(nameof(SVFIClipDisabledHintText));
            OnPropertyChanged(nameof(AnalyzeNeedsSourceText));
            OnPropertyChanged(nameof(NumaCpuCheckHintText));
        }
        private void RefreshButtonCaptions()
        {
            OpenAppConfButtons.B2_1Text = UICaptionProviderM.Buttons.UsageAndCompliance;
            OpenAppConfButtons.B2_2Text = UICaptionProviderM.Buttons.Settings;
            FilterScbButtons.B2_1Text = UICaptionProviderM.Buttons.OneClickScriptGen;
            FilterScbButtons.B2_2Text = UICaptionProviderM.Buttons.OpenScribeSrcScribe;
            OnPropertyChanged(nameof(ToggleMiniUpstreamsZoneText));
            OnPropertyChanged(nameof(ToggleMiniEncodersZoneText));
            OnPropertyChanged(nameof(ToggleMiniAnalyticsZoneText));
            OnPropertyChanged(nameof(ToggleMiniDependenciesZoneText));
            OnPropertyChanged(nameof(ToggleMiniVideoSrcImportZoneText));
            OnPropertyChanged(nameof(ToggleMiniScriptSrcImportZoneText));
            OnPropertyChanged(nameof(ToggleMiniEncodingConfZoneText));
            OnPropertyChanged(nameof(ToggleShowBestPracticesCardText));
            EncStartButtons.B3_1Text = UICaptionProviderM.Buttons.ReEvaluate;
            EncStartButtons.B3_2Text = UICaptionProviderM.Buttons.RunSample;
            EncStartButtons.B3_3Text = UICaptionProviderM.Buttons.StartEncode;
            AnalyzeSrcButtons.B2_1Text = UICaptionProviderM.Buttons.CopyRawAnalysis;
            AnalyzeSrcButtons.B2_2Text = UICaptionProviderM.Buttons.AnalyzeSrcVideo;
            InspBypsChkButtons.B2_1Text = UICaptionProviderM.Buttons.InspectSrcProbelms;
            InspBypsChkButtons.B2_2Text = UICaptionProviderM.Buttons.BypassSrcChecklist;
            InspBypsEncChkButtons.B2_1Text = UICaptionProviderM.Buttons.InspectEncPreProblems;
            InspBypsEncChkButtons.B2_2Text = UICaptionProviderM.Buttons.BypassEncChecklist;
        }
        private void RefreshCardsLanguage()
        {
            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;
            ToolsImportCard.RefreshLanguage();

            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceIncompatOrCorrupted;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SrcQualityIssues;
            SrcValidationCard.RefreshLanguage();
            QueueSrcFilterCard.RefreshLanguage();

            EncTermsCard.Name = UICaptionProviderM.Cards.EncPrerequisites;
            EncTermsCard.P1Name = UICaptionProviderM.Cards.EncHardware;
            EncTermsCard.P3Name = UICaptionProviderM.Cards.EncSoftware;
            EncTermsCard.RefreshLanguage();

            BestPracticesCard.Name = UICaptionProviderM.Cards.BestPractices;
            BestPracticesCard.P1Name = UICaptionProviderM.Cards.BestHardware;
            BestPracticesCard.P3Name = UICaptionProviderM.Cards.BestSoftware;
            BestPracticesCard.Subtitle = UICaptionProviderM.Cards.BestPracticesSubtitle;
            BestPracticesCard.RefreshLanguage();
        }
        private void RefreshZoneLanguage()
        {
            ApplyDefinitionsToZone(VideoSrcImportZone, ToolCatalogProviderM.GetVideoSrcImportDefs());
            RefreshSourceQueueLanguage();
            RefreshSourceZonePrimaryText(VideoSrcImportZone);
            ApplyDefinitionsToZone(ScriptSrcImportZone, ToolCatalogProviderM.GetScriptSrcImportDefs());
            RefreshSourceZonePrimaryText(ScriptSrcImportZone);
            ApplyDefinitionsToZone(QueueScriptSrcImportZone, ToolCatalogProviderM.GetScriptSrcImportQueueDefs());
            RefreshScriptQueuePrimaryText();
            ApplyDefinitionsToZone(EncodingConfZone, ToolCatalogProviderM.GetEncSettingsDefinitions());
            WireUpEncSettingsCmds();
            foreach (ObservableCollection<ToolItemCardVM> zone in AllImportedToolZones)
                ApplyImportedToolDefs(zone);
            RefreshVspipeAvailability();
        }

        private void RefreshVspipeAvailability()
        {
            ToolItemCardVM? vspipe = UpstreamsZone.FirstOrDefault(t =>
                ToolDefinitionProviderM.IsImportedTool(t.Name, "vspipe.exe"));
            if (vspipe == null) return;

            vspipe.IsEnabled = ToolVersionDetect.HasValidVspipeY4mArg(
                _appDataM.Tools.VspipePath,
                _appDataM.Tools.VspipeY4mArg);
        }
        private void RefreshSourceQueueLanguage()
        {
            _videoSourceQueue.RefreshLanguage();
        }

        private void RefreshSourceZonePrimaryText(ObservableCollection<ToolItemCardVM> zone)
        {
            if (zone == null) return;

            foreach (ToolItemCardVM item in zone)
            {
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.P2TextData)) continue;

                if (IsVideoSourceQueueItem(item)) continue;

                SourceFileKind fileKind = SourceFileKindResolver.ResolveSourceFileKind(item.Name);
                item.P1TextData = SourceFilePicker.GetPrimaryText(fileKind, item.P2TextData);
            }
        }

        private void RefreshScriptQueuePrimaryText()
        {
            if (QueueScriptSrcImportZone == null) return;

            foreach (ToolItemCardVM item in QueueScriptSrcImportZone)
            {
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.P2TextData)) continue;
                SourceFileKind fileKind = SourceFileKindResolver.ResolveSourceFileKind(item.Name);
                string[] filePaths = SourceFilePicker.GetSourceFilesInFolder(item.P2TextData, fileKind);
                item.P1TextData = VideoSourceQueue.GetQueueP1Text(
                    [.. filePaths.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!)]);
            }
        }

        private static void ApplyDefinitionsToZone(ObservableCollection<ToolItemCardVM> zone, List<ToolDefinitionM> definitions)
        {
            for (int i = 0; (i < definitions.Count && i < zone.Count); i++)
            {
                zone[i].ApplyDefinition(definitions[i]);
                zone[i].RefreshLanguage();
            }
        }
        private static void ApplyImportedToolDefs(ObservableCollection<ToolItemCardVM> zone)
        {
            foreach (ToolItemCardVM item in zone)
            {
                ToolDefinitionM? definition =
                    ToolDefinitionProviderM.GetByDisplayName(item.Name);
                if (definition != null) item.ApplyDefinition(definition);
                item.RefreshLanguage();
            }
        }

        private void UnwireUpZoneDeleteCmds()
        {
            foreach (ToolItemCardVM tool in VideoSrcImportZone) UnwireSourceCmd(tool);
            foreach (ToolItemCardVM tool in ScriptSrcImportZone) UnwireSourceCmd(tool);
            foreach (ToolItemCardVM tool in QueueScriptSrcImportZone) UnwireSourceCmd(tool);
            foreach (ToolItemCardVM tool in EncodingConfZone) UnwireStaticClearCmd(tool);
            foreach (ObservableCollection<ToolItemCardVM> zone in AllImportedToolZones)
                foreach (ToolItemCardVM tool in zone) UnwireToolCmd(tool);
        }

        private void UnwireToolCmd(ToolItemCardVM item)
        {
            item.PropertyChanged -= OnUpstreamItemPropertyChanged;
            item.PropertyChanged -= OnEncoderItemPropertyChanged;
            item.PropertyChanged -= OnAnalyticsItemPropertyChanged;
            item.R1Command = null;
            item.R2Command = null;
        }

        private void UnwireSourceCmd(ToolItemCardVM item)
        {
            item.PropertyChanged -= OnVideoSrcItemPropertyChanged;
            item.R1Command = null;
            item.R2Command = null;
        }

        private static void UnwireStaticClearCmd(ToolItemCardVM item) =>
            item.R2Command = null;
        #endregion

        public override void Dispose()
        {
            // Release retained event handlers and command references so the VM can be collected.
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            _modalNavS.CurrentViewModelChanged -= OnModalStateChanged;
            ToolsImportCard.ToolImported -= OnToolImported;
            AnalyticsZone.CollectionChanged -= OnAnalyticsZoneCollectionChanged;
            if (_outputSettingCard != null)
                _outputSettingCard.PropertyChanged -= OnOutputSettingPropertyChanged;
            UnwireUpZoneDeleteCmds();
            ToolsImportCard.Dispose();
            UnsubFromImportedToolZones();
            UnsubFromToolsChecklist();
            UnsubZoneItemsCollectionChanged(UpstreamsZone);
            UnsubZoneItemsCollectionChanged(EncodersZone);
            UnsubZoneItemsCollectionChanged(AnalyticsZone);
            UnsubZoneItemsCollectionChanged(DependenciesZone);
            UnsubZoneItemsCollectionChanged(VideoSrcImportZone);
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
