using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Helpers;
using OneColumnEncoder.Components;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels.Cards;
using OneColumnEncoder.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using OneColumnEncoder.Commands.SaveLoad;

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly AppDataM _appDataM;
        private readonly AppConfM _appConfM;
        private readonly ModalNavS _modalNavS;
        private readonly VideoAnalysisM _srcVideoAnalysis = new();

        // Groups of Card or other element UIs
        public ObservableCollection<ToolItemCardVM> UpstreamsZone { get; }
        public ObservableCollection<ToolItemCardVM> EncodersZone { get; }
        public ObservableCollection<ToolItemCardVM> AnalyticsZone { get; } // A-D separated for dual single-select
        public ObservableCollection<ToolItemCardVM> DependenciesZone { get; }
        public ObservableCollection<ToolItemCardVM> VideoSrcImportZone { get; } // V-S separated for dual single-select
        public ObservableCollection<ToolItemCardVM> ScriptSrcImportZone { get; }
        public ObservableCollection<ToolItemCardVM> EncSettingsZone { get; }
        // Cmds and buttons
        public OpenUsagesCmd OpenUsages { get; }
        public OpenAppConfCmd OpenAppConf { get; }
        public OneClickScriptGenCmd OneClickScriptGen { get; }
        public OpenScriptScribeCmd OpenScriptScribe { get; }
        public CopyRawAnalysisCmd CopyRawAnalysis { get; } // Copy (ffprobe JSON) to clipboard
        public AnalyzeSrcVideoCmd AnalyzeSrcVideo { get; } // Maybe add mediaInfo analysis in future, but ffprobe alone will do
        public InspectSrcProblemsCmd InspectSrcProblems { get; }
        public BypsSrcChecklistCmd BypassSrcChecklist { get; }
        public SelectToolCmd SelectTool { get; } // ItemCard select on click
        public ButtonGroupVM OpenAppConfButtons { get; } // OpenUsages & OpenAppConf
        public ButtonGroupVM ScriptScbButtons { get; } // OneClickScriptGen & OpenScriptScribe
        public ButtonGroupVM AnalyzeSrcButtons { get; } // AnalyzeSrcVideo & CopyRawAnalysis
        public ButtonGroupVM InspBypsChkButtons { get; } // InspectSrcProbelms & BypsSrcChecklist
        public ButtonGroupVM EncStartButtons { get; }
        // Button guards
        private readonly bool _isAnalyzeSrcButtonsReady;
        private readonly bool _isInspBypsChkButtonsReady;
        private readonly bool _isEncStartButtonsReady;
        // Card UIs
        public ToolsImportCardVM ToolsImportCard { get; }
        public SourceCheckCardVM SrcValidationCard { get; } = new();
        public EncTermsCardVM EncTermsCard { get; } = new();
        public BestPracsCardVM BestPracticesCard { get; } = new();
        // Section header texts
        public static string SectionImportTools => UILangProviderM.Current["Section.ImportTools"];
        public static string SectionSelectUpstream => UILangProviderM.Current["Section.SelectUpstream"];
        public static string SectionSelectEncoder => UILangProviderM.Current["Section.SelectEncoder"];
        public static string SectionSelectAnalytics => UILangProviderM.Current["Section.SelectAnalytics"];
        public static string SectionSelectDependencies => UILangProviderM.Current["Section.SelectDependencies"];
        public static string SectionImportSource => UILangProviderM.Current["Section.ImportSource"];
        public static string SectionAnalysisResults => UILangProviderM.Current["Section.AnalysisResults"];
        public static string SectionEncodingConfigs => UILangProviderM.Current["Section.EncodingConfigs"];
        public static string SectionStartEncoding => UILangProviderM.Current["Section.StartEncoding"];

        // Disable UI when other modal opens
        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        private ObservableCollection<ToolItemCardVM>[] AllImportedToolZones =>
            [UpstreamsZone, EncodersZone, AnalyticsZone, DependenciesZone];

        #region Constructor
        public MainVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages, AppDataM appDataM, AppConfM appConfM, ModalNavS modalNavS)
        {
            // Tools data, Settings data, Modal Navigation, Open Settings Command
            _appDataM = appDataM;
            _appConfM = appConfM;
            _modalNavS = modalNavS;
            OpenAppConf = openAppConf;
            OpenUsages = openUsages;
            SelectTool = new SelectToolCmd(this);

            ToolsImportCard = new ToolsImportCardVM(modalNavS);
            VideoSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetVideoSrcImportDefs(), true);
            ScriptSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetScriptSrcImportDefs(), true);
            EncSettingsZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetEncSettingsDefinitions());
            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
            DependenciesZone = [];
            LoadToolsFromAppDataM();
            LoadSourcesFromAppDataM();
            WireUpZoneDeleteCmds();

            // Set default values for output setting in EncSettingsZone
            ToolItemCardVM? outputSetting = EncSettingsZone.FirstOrDefault(t => t.Name.Equals(
                UILangProviderM.Current["Tool.Enc.OutputSetting"],
                StringComparison.OrdinalIgnoreCase));

            // Set P2Text to desktop, then P1Text to file name
            if (outputSetting != null)
            {
                // Must be set first because Path setter has Validate() call which clears VersionText
                if (string.IsNullOrWhiteSpace(outputSetting.P2TextData))
                    outputSetting.P2TextData = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (string.IsNullOrWhiteSpace(outputSetting.P1TextData))
                    outputSetting.P1TextData = "1cenc output";
            }

            // Load saved parallelism settings onto the card
            ToolItemCardVM? parallelismCard = EncSettingsZone.FirstOrDefault(t => t.Name.Equals(
                UILangProviderM.Current["Tool.Enc.Parallelism"],
                StringComparison.OrdinalIgnoreCase));
            if (parallelismCard != null)
                ParallelismConfVM.ApplySavedSettingsToCard(parallelismCard);

            // Commands
            OneClickScriptGen = new OneClickScriptGenCmd(
                () => GetCurrentVideoSourcePath(), ScriptSrcImportZone[0], ScriptSrcImportZone[1], modalNavS);
            OpenScriptScribe = new OpenScriptScribeCmd(
                modalNavS, () => GetCurrentVideoSourcePath());
            CopyRawAnalysis = new CopyRawAnalysisCmd(
                _srcVideoAnalysis, modalNavS);
            AnalyzeSrcVideo = new AnalyzeSrcVideoCmd(
                GetSelectedFfprobePath, GetSelectedVideoSourcePath, _srcVideoAnalysis, SrcValidationCard, modalNavS,
                () =>
                { // On source analysis complete
                    UpdateAnalyzeSrcButtonsState();
                    UpdateEncStartButtonsState();
                });
            InspectSrcProblems = new InspectSrcProblemsCmd(
                _srcVideoAnalysis, SrcValidationCard, modalNavS);
            BypassSrcChecklist = new BypsSrcChecklistCmd(
                SrcValidationCard,
                () => !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson),
                UpdateEncStartButtonsState);

            // Buttons
            OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.UsageAndCompliance, UICaptionProviderM.Buttons.Settings, OpenUsages, OpenAppConf);
            OpenAppConfButtons.B2_2Icon = SvgIconProviderH.GameSetting;
            ScriptScbButtons = ButtonGroupVM.CreateTwoButton( // UpdateScriptScbButtonsState()
                UICaptionProviderM.Buttons.OneClickScriptGen, UICaptionProviderM.Buttons.OpenScribeSrcScribe, OneClickScriptGen, OpenScriptScribe);
            AnalyzeSrcButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.CopyRawAnalysis, UICaptionProviderM.Buttons.AnalyzeSrcVideo, CopyRawAnalysis, AnalyzeSrcVideo);
            _isAnalyzeSrcButtonsReady = true;
            EncStartButtons = ButtonGroupVM.CreateThreeButton( // UpdateEncStartButtonsState()
                UICaptionProviderM.Buttons.ReEvaluate, UICaptionProviderM.Buttons.RunSample, UICaptionProviderM.Buttons.StartEncode);
            _isEncStartButtonsReady = true;
            InspBypsChkButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.InspectSrcProbelms, UICaptionProviderM.Buttons.BypassSrcChecklist, InspectSrcProblems, BypassSrcChecklist);
            _isInspBypsChkButtonsReady = true;

            // Import dropdown menu and behavior
            ToolsImportCard.ToolImported += OnToolImported;
            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;

            foreach (DropdownItemM item in ToolCatalogProviderM.GetImportDropdownItems())
                ToolsImportCard.ImportDropdown.Items.Add(item);
            ToolsImportCard.ImportDropdown.SelectedItem = ToolsImportCard.ImportDropdown.Items[0];

            // Other validations or simply lists for Start Encode button
            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceIncompatOrCorrupted;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SrcQualityIssues;
            EncTermsCard.Name = UICaptionProviderM.Cards.EncPrerequisites;
            EncTermsCard.P1Name = UICaptionProviderM.Cards.EncHardware;
            EncTermsCard.P3Name = UICaptionProviderM.Cards.EncSoftware;
            BestPracticesCard.Name = UICaptionProviderM.Cards.BestPractices;
            BestPracticesCard.P1Name = UICaptionProviderM.Cards.BestHardware;
            BestPracticesCard.P3Name = UICaptionProviderM.Cards.BestSoftware;

            SrcValidationCard.IsSvtav1SelectedFunc = () =>
                EncodersZone.Any(t => t.IsSelected
                    && ToolDefinitionProviderM.IsImportedTool(t.Name, "svtav1encapp.exe"));

            // Checklist item settings, checklist subs, nav subs, overlay subs
            InitializeChecklistEntryStates();
            OpenAppConf.OnAfterClose += InitializeChecklistEntryStates;
            SubToImportedToolZones();
            AnalyticsZone.CollectionChanged += OnAnalyticsZoneCollectionChanged;
            RefreshUpstreamToolState(); // initial state after loading
            SubToToolsChecklist();
            UpdateScriptScbButtonsState(); // Initial state of script scribe buttons
            RefreshSelectedSourceStatus();
            UpdateAnalyzeSrcButtonsState();
            UpdateInspBypsChkButtonsState();
            _modalNavS.CurrentViewModelChanged += OnModalStateChanged;
            IsOverlayVisible = _modalNavS.IsOpen;
            UILangProviderM.CurrentChanged += OnLanguageChanged;
            RefreshLanguage();
        }
        #endregion

        #region Zone Initialization
        private static ObservableCollection<ToolItemCardVM> LoadZoneFromDefinitions(List<ToolDefinitionM> defs, bool useAutoAddReplaceText = false)
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
                    UseAutoAddReplaceText = useAutoAddReplaceText
                };
                item.R2Command = new RemoveZoneItemCmd(item, zone);
                zone.Add(item);
            }
            return zone;
        }

        // Read settings regarding disabling checklist items
        private void InitializeChecklistEntryStates()
        {
            AppConfM.GeneralSettings g = _appConfM.General;
            ObservableCollection<ChecklistEntryVM> cl1 = EncTermsCard.Checklist1;
            if (cl1.Count >= 1) cl1[0].IsEnabled = g.OffGrid;
            if (cl1.Count >= 2) cl1[1].IsEnabled = g.InsufficientRAM;
            if (cl1.Count >= 3) cl1[2].IsEnabled = g.InsufficientDiskSpace;

            ObservableCollection<ChecklistEntryVM> cl2 = EncTermsCard.Checklist2;
            if (cl2.Count >= 1) cl2[0].IsEnabled = g.NoWritePermission;
            if (cl2.Count >= 2) cl2[1].IsEnabled = g.IsOverwriting;
        }
        #endregion

        #region Imported Zone Event Handling
        private void SubToImportedToolZones()
        {
            foreach (var zone in AllImportedToolZones)
                zone.CollectionChanged += OnImportedToolZoneCollectionChanged;
            RefreshImportedToolsChecklist();
        }
        private void UnsubFromImportedToolZones()
        {
            foreach (var zone in AllImportedToolZones)
                zone.CollectionChanged -= OnImportedToolZoneCollectionChanged;
        }
        private void OnAnalyticsZoneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAnalyzeSrcButtonsState();
            UpdateInspBypsChkButtonsState();
        }
        private void OnImportedToolZoneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<ToolItemCardVM> zone)
                ApplyDefaultImportedToolSelection(zone);

            if (sender == EncodersZone)
                SrcValidationCard.RefreshSvtav1BitDepthStatus();

            RefreshUpstreamToolState();
            RefreshImportedToolsChecklist();
            ToolCompatibilityH.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                UpstreamsZone, ScriptSrcImportZone, RefreshSelectedSourceStatus);
        }

        private void ApplyDefaultImportedToolSelection(ObservableCollection<ToolItemCardVM> zone)
        {
            ItemCardSelectionH.ApplyDefaultSelection(zone);
            RefreshImportedToolPickedStatus(zone);
        }

        private void RefreshUpstreamToolState()
        {
            ToolItemCardVM? avs2pipemod =
                UpstreamsZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            if (avs2pipemod == null) return;

            if (!HasImportedAviSynthDll())
            {
                avs2pipemod.IsSelected = false;
                avs2pipemod.IsEnabled = false;
            }
            else avs2pipemod.IsEnabled = true;

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
            RefreshImportedToolsChecklist();
            ToolCompatibilityH.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                UpstreamsZone, ScriptSrcImportZone, RefreshSelectedSourceStatus);
        }

        private void RefreshEncSettingsState()
        {
            bool hasVideoSource = CanRunSourceAnalysis();
            foreach (ToolItemCardVM item in EncSettingsZone)
                item.IsEnabled = hasVideoSource;
        }

        private bool HasImportedFfprobe() =>
            !string.IsNullOrWhiteSpace(_appDataM.Tools.FfprobePath);
        private bool HasImportedAviSynthDll() =>
            !string.IsNullOrWhiteSpace(_appDataM.Tools.AviSynthDllPath);

        #endregion

        #region Validation Checklists

        private void SubToToolsChecklist()
        {
            foreach (ChecklistEntryVM entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            UpdateEncStartButtonsState();
        }
        private void UnsubFromToolsChecklist()
        {
            foreach (ChecklistEntryVM entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in SrcValidationCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (ChecklistEntryVM entry in EncTermsCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
        }
        private void OnChecklistEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChecklistEntryVM.Status))
                UpdateEncStartButtonsState();
        }
        #endregion

        #region Button state updates
        public void UpdateScriptScbButtonsState()
        {
            bool hasVideoSrc = VideoSrcImportZone.Any(t => !string.IsNullOrWhiteSpace(t.P2TextData));
            ScriptScbButtons.B2_2IsEnabled = hasVideoSrc;

            if (_modalNavS.CurrentModalVM is ScriptScribeModalVM modal)
            {
                modal.ScriptExportButtons.B3_1IsEnabled = hasVideoSrc;
                modal.ScriptExportButtons.B3_2IsEnabled = hasVideoSrc;
                modal.ScriptExportButtons.B3_3IsEnabled = hasVideoSrc;
            }

            OneClickScriptGen.OnCanExecuteChanged();
        }
        public void UpdateEncStartButtonsState()
        {
            if (!_isEncStartButtonsReady) return;

            bool toolsReady =
                UpstreamsZone.Count > 0 && EncodersZone.Count > 0 && HasImportedFfprobe();

            bool toolsPickedReady =
                ToolsImportCard.ToolsChecklist.Skip(3).All(e => !e.IsEnabled || e.Status == StatusType.Success);

            bool hasRawJson = !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson);

            bool sourceValidationReady = SrcValidationCard.IsBypassed ||
                (hasRawJson &&
                SrcValidationCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                SrcValidationCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success));
            bool encodeTermsReady =
                EncTermsCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                EncTermsCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);
            bool avsSelected = UpstreamsZone.Any(
                t => t.IsSelected && ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            bool aviSelected = DependenciesZone.Any(
                t => t.IsSelected && ToolDefinitionProviderM.IsImportedTool(t.Name, "avisynth.dll"));
            bool dependencyReady = avsSelected == aviSelected;
            
            // TODO: SVT-AV1 does not suport 12bit source

            bool allReady = toolsReady && toolsPickedReady && sourceValidationReady && encodeTermsReady && dependencyReady;
            EncStartButtons.B3_2IsEnabled = allReady;
            EncStartButtons.B3_3IsEnabled = allReady;
        }
        #endregion

        #region Command Wiring (Bind R1-R2)
        private void WireUpZoneDeleteCmds()
        {
            foreach (ToolItemCardVM tool in VideoSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemCardVM tool in ScriptSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemCardVM tool in EncSettingsZone) WireUpStaticClearCmd(tool);
            WireUpEncSettingsCmds();
            foreach (var zone in AllImportedToolZones)
                foreach (ToolItemCardVM tool in zone) WireUpToolCmd(tool);
        }
        private void WireUpToolCmd(ToolItemCardVM item)
        {
            item.R1Command = new ReplaceToolCmd(
                item, _appDataM, _modalNavS, RefreshImportedToolStates);
            item.R2Command = new DeleteToolCmd(
                item, GetZoneForTool(ToolDefinitionProviderM.ResolveToolZone(item.Name)), _appDataM);

            ToolZone zone = ToolDefinitionProviderM.ResolveToolZone(item.Name);
            if (zone == ToolZone.Encoder)
                item.PropertyChanged += OnEncoderItemPropertyChanged;
            if (zone == ToolZone.Analytics)
                item.PropertyChanged += OnAnalyticsItemPropertyChanged;
        }
        private void WireUpSourceCmd(ToolItemCardVM item)
        {
            SourceFileKind kind = ResolveSourceFileKind(item.Name);
            item.R1Command = new BrowseSourcePathCmd(item, kind, _appDataM, _modalNavS, OnSourceImported);
            item.R2Command = new ClearToolItemCmd(item, () => OnSourceCleared(kind));
            item.PropertyChanged += OnVideoSrcItemPropertyChanged;
        }
        private static void WireUpStaticClearCmd(ToolItemCardVM item)
        {
            item.R2Command = new ClearToolItemCmd(item);
        }
        private void WireUpEncSettingsCmds()
        {
            if (EncSettingsZone.Count > 1)
                EncSettingsZone[1].R1Command = new OpenParallelismConfCmd(_modalNavS, EncSettingsZone[1]);

            ToolItemCardVM? outputSetting = EncSettingsZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));

            if (outputSetting != null)
                outputSetting.R1Command = new OpenFilenameScribeCmd(_modalNavS, outputSetting);

            ToolItemCardVM? compressionParams = EncSettingsZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.CompressionParams"], StringComparison.OrdinalIgnoreCase));

            if (compressionParams != null)
            {
                compressionParams.R1Command = new OpenEncoderConfCmd(_modalNavS, compressionParams);
                compressionParams.SetEncodingSummary("策略基础参数: CRF18-22-33 | 通用-通用-极致画质", "最大关键帧间距: 11-9-11 秒");
            }
        }
        private void OnVideoSrcItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolItemCardVM) return;
            if (e.PropertyName == nameof(ToolItemCardVM.P2TextData))
                UpdateScriptScbButtonsState();
            if (e.PropertyName is nameof(ToolItemCardVM.P2TextData) or nameof(ToolItemCardVM.IsSelected))
            {
                UpdateAnalyzeSrcButtonsState();
                UpdateInspBypsChkButtonsState();
            }
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
            ItemCardSelectionH.HandleItemCardClick(
                clickedTool,
                UpstreamsZone, EncodersZone, AnalyticsZone, DependenciesZone,
                VideoSrcImportZone, ScriptSrcImportZone,
                ToolsImportCard,
                RefreshSelectedSourceStatusAfterSourceSelection,
                UpdateEncStartButtonsState,
                RefreshSelectedSourceStatus);
        }

        private void RefreshToolPickedStatus(ToolZone toolZone, ObservableCollection<ToolItemCardVM> itemZone) =>
            ItemCardSelectionH.RefreshToolPickedStatus(ToolsImportCard, toolZone, itemZone);

        private void RefreshImportedToolPickedStatus(ObservableCollection<ToolItemCardVM> itemZone)
        {
            if (itemZone == UpstreamsZone)
                RefreshToolPickedStatus(ToolZone.Upstream, itemZone);
            else if (itemZone == EncodersZone)
                RefreshToolPickedStatus(ToolZone.Encoder, itemZone);
            else if (itemZone == AnalyticsZone)
                RefreshToolPickedStatus(ToolZone.Analytics, itemZone);
        }

        private void OnSourceImported(ToolItemCardVM item, SourceFileKind kind, string filePath)
        {
            SaveSourcePath(kind, filePath);

            foreach (ToolItemCardVM source in VideoSrcImportZone)
                source.IsSelected = false;
            foreach (ToolItemCardVM source in ScriptSrcImportZone)
                source.IsSelected = false;

            item.IsSelected = true;
            _appDataM.Save();
            RefreshSelectedSourceStatus(resetAnalysis: true);
        }
        private void OnSourceCleared(SourceFileKind kind)
        {
            SaveSourcePath(kind, string.Empty);
            _appDataM.Save();
            RefreshSelectedSourceStatus(
                resetAnalysis: kind == SourceFileKind.Video || !VideoSrcImportZone.Any(t => t.IsSelected));
        }

        private void SaveSourcePath(SourceFileKind kind, string filePath)
        {
            switch (kind)
            {
                case SourceFileKind.Video:
                    _appDataM.Tools.VideoSourcePath = filePath;
                    break;
                case SourceFileKind.AviSynthScript:
                    _appDataM.Tools.AvsSourcePath = filePath;
                    break;
                case SourceFileKind.VapourSynthScript:
                    _appDataM.Tools.VpySourcePath = filePath;
                    break;
                case SourceFileKind.SvfiIni:
                    _appDataM.Tools.SvfiSourcePath = filePath;
                    break;
            }
        }
        public void RefreshSelectedSourceStatus() =>
            RefreshSelectedSourceStatus(resetAnalysis: false);

        public void RefreshSelectedSourceStatus(bool resetAnalysis)
        {
            bool anySelected =
                VideoSrcImportZone.Any(t => t.IsSelected) ||
                ScriptSrcImportZone.Any(t => t.IsSelected);
            if (resetAnalysis)
            {
                _srcVideoAnalysis.Clear();
                SrcValidationCard.ResetAnalysisStatus(anySelected);
            }
            else
            {
                SrcValidationCard.SetSourcePickedStatus(anySelected);
            }

            UpdateScriptScbButtonsState();
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
            if (!hasRawJson && SrcValidationCard.IsBypassed)
                SrcValidationCard.SetBypassed(false);

            InspBypsChkButtons.B2_1IsEnabled = hasRawJson;
            InspBypsChkButtons.B2_2IsEnabled = hasRawJson;
            InspectSrcProblems.OnCanExecuteChanged();
            BypassSrcChecklist.OnCanExecuteChanged();
        }
        private string GetCurrentVideoSourcePath()
        {
            ToolItemCardVM? videoSrc = VideoSrcImportZone.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.P2TextData));
            return videoSrc?.P2TextData ?? string.Empty;
        }

        private bool CanRunSourceAnalysis() =>
            VideoSrcImportZone.Any(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData)) &&
            AnalyticsZone.Any(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));

        private bool IsCurrentAnalysisFor(string sourcePath, string ffprobePath) =>
            !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson) &&
            string.Equals(_srcVideoAnalysis.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(_srcVideoAnalysis.FfprobePath, ffprobePath, StringComparison.OrdinalIgnoreCase);

        private void ResetAnalysisIfStale()
        {
            if (IsCurrentAnalysisFor(GetSelectedVideoSourcePath(), GetSelectedFfprobePath())) return;

            bool anySelected = VideoSrcImportZone.Any(t => t.IsSelected) || ScriptSrcImportZone.Any(t => t.IsSelected);
            _srcVideoAnalysis.Clear();
            SrcValidationCard.ResetAnalysisStatus(anySelected);
        }

        private string GetSelectedVideoSourcePath()
        {
            ToolItemCardVM? videoSrc = VideoSrcImportZone.FirstOrDefault(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
            return videoSrc?.P2TextData ?? string.Empty;
        }

        private string GetSelectedFfprobePath()
        {
            ToolItemCardVM? ffprobe = AnalyticsZone.FirstOrDefault(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
            return ffprobe?.P2TextData ?? string.Empty;
        }

        private static SourceFileKind ResolveSourceFileKind(string displayName)
        {
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.VideoSource"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.Video;
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.AviSynth"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.AviSynthScript;
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.VapourSynth"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.VapourSynthScript;
            if (displayName.Equals(UILangProviderM.Current["Tool.Source.Svfi"], StringComparison.OrdinalIgnoreCase))
                return SourceFileKind.SvfiIni;

            throw new ArgumentException($"Unknown source type: {displayName}");
        }
        #endregion

        #region Zone Helpers

        private ObservableCollection<ToolItemCardVM> GetZoneForTool(ToolZone zone) => zone switch
        {
            ToolZone.Upstream => UpstreamsZone,
            ToolZone.Encoder => EncodersZone,
            ToolZone.Analytics => AnalyticsZone,
            ToolZone.Dependencies => DependenciesZone,
            _ => throw new ArgumentException("Invalid tool zone")
        };

        #endregion

        #region Loading or adding other persistent data

        /// <summary>
        /// Add new tool configuration or update an existing one if duplicated
        /// </summary>
        /// <param name="defKey">Unique key for a tool</param>
        /// <param name="filePath">Tool filePath</param>
        /// <param name="version">Tool version string, collected from ToolVersionDetector.cs</param>
        private void AddOrUpdateTool(string defKey, string? filePath, string? version)
        {
            if (!ToolDefinitionProviderM.ToolDefs.TryGetValue(defKey, out ToolDefinitionM? def)) return;
            if (def.Zone == null || string.IsNullOrEmpty(filePath)) return;

            // Find zone belonging, duplication check, duplication handling (replace)
            ObservableCollection<ToolItemCardVM> zone = GetZoneForTool(def.Zone.Value);
            ToolItemCardVM? existing = zone.FirstOrDefault(i => i.Name == def.DisplayName);
            if (existing != null) zone.Remove(existing);

            ToolItemCardVM item = new(new EncItemM(def.DisplayName))
            {
                P2TextData = filePath,
                P1TextData = version ?? string.Empty,
                P1Name = def.P1Name,
                P2Name = def.P2Name ?? string.Empty,
                R1Text = def.R1Text,
                R2Text = def.R2Text
            };
            WireUpToolCmd(item);
            zone.Add(item);

            ApplyDefaultImportedToolSelection(zone);
        }
        private void LoadToolsFromAppDataM()
        {
            AppDataM.Importables t = _appDataM.Tools;
            foreach ((string defKey, ToolDefinitionM def) in ToolDefinitionProviderM.ToolDefs)
            {
                if (def.Zone == null || def.ExeName == null) continue;

                (string? path, string? version) = def.ExeName switch
                {
                    "ffmpeg.exe" => (t.FfmpegPath, t.FfmpegVer),
                    "vspipe.exe" => (t.VspipePath, t.VspipeVer),
                    "avs2yuv.exe" => (t.Avs2yuvPath, t.Avs2yuvVer),
                    "avs2pipemod.exe" => (t.Avs2pipemodPath, t.Avs2pipemodVer),
                    "one_line_shot_args.exe" => (t.OneLineShotArgsPath, t.OneLineShotArgsVer),
                    "x264.exe" => (t.X264Path, t.X264Ver),
                    "x265.exe" => (t.X265Path, t.X265Ver),
                    "svtav1encapp.exe" => (t.SvtAv1Path, t.SvtAv1Ver),
                    "ffprobe.exe" => (t.FfprobePath, t.FfprobeVer),
                    "avisynth.dll" => (t.AviSynthDllPath, t.AviSynthDllVer),
                    _ => (null, null)
                };

                if (!string.IsNullOrEmpty(path)) AddOrUpdateTool(defKey, path, version);
            }
        }
        private async Task OnToolImported(string exeName, string filePath, string? version)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByExeName(exeName);
            if (def == null || def.Zone == null) return;

            string defKey = ToolDefinitionProviderM.ToolDefs
                .FirstOrDefault(kvp => kvp.Value == def).Key;
            if (defKey == null) return;

            ToolCatalogProviderM.TrySetPath(exeName, _appDataM.Tools, filePath);
            ToolCatalogProviderM.TrySetVersion(exeName, _appDataM.Tools, version ?? string.Empty);

            if (exeName.Equals("vspipe.exe", StringComparison.OrdinalIgnoreCase))
            {
                string? y4mArg = await ToolVersionDetectH.DetectVspipeY4mArgAsync(filePath);
                _appDataM.Tools.VspipeY4mArg = y4mArg;
            }

            _appDataM.Save();
            AddOrUpdateTool(defKey, filePath, version);
        }

        private void LoadSourcesFromAppDataM()
        {
            bool hasVideoSource = LoadSourceItem(VideoSrcImportZone[0], SourceFileKind.Video, _appDataM.Tools.VideoSourcePath);
            VideoSrcImportZone[0].IsSelected = hasVideoSource;
            if (!hasVideoSource && !string.IsNullOrWhiteSpace(_appDataM.Tools.VideoSourcePath))
            {
                _appDataM.Tools.VideoSourcePath = string.Empty;
                _appDataM.Save();
            }

            LoadSourceItem(ScriptSrcImportZone[0], SourceFileKind.AviSynthScript, _appDataM.Tools.AvsSourcePath);
            LoadSourceItem(ScriptSrcImportZone[1], SourceFileKind.VapourSynthScript, _appDataM.Tools.VpySourcePath);
            LoadSourceItem(ScriptSrcImportZone[2], SourceFileKind.SvfiIni, _appDataM.Tools.SvfiSourcePath);

            RefreshEncSettingsState();
        }
        private static bool LoadSourceItem(ToolItemCardVM item, SourceFileKind kind, string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;

            item.P2TextData = path;
            item.P1TextData = SourceFilePickerH.GetPrimaryText(kind, path);
            return true;
        }


        #endregion

        private void OnModalStateChanged() { IsOverlayVisible = _modalNavS.IsOpen; }

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
            OnPropertyChanged(nameof(SectionImportTools));
            OnPropertyChanged(nameof(SectionSelectUpstream));
            OnPropertyChanged(nameof(SectionSelectEncoder));
            OnPropertyChanged(nameof(SectionSelectAnalytics));
            OnPropertyChanged(nameof(SectionSelectDependencies));
            OnPropertyChanged(nameof(SectionImportSource));
            OnPropertyChanged(nameof(SectionAnalysisResults));
            OnPropertyChanged(nameof(SectionEncodingConfigs));
            OnPropertyChanged(nameof(SectionStartEncoding));
        }
        private void RefreshButtonCaptions()
        {
            OpenAppConfButtons.B2_1Text = UICaptionProviderM.Buttons.UsageAndCompliance;
            OpenAppConfButtons.B2_2Text = UICaptionProviderM.Buttons.Settings;
            ScriptScbButtons.B2_1Text = UICaptionProviderM.Buttons.OneClickScriptGen;
            ScriptScbButtons.B2_2Text = UICaptionProviderM.Buttons.OpenScribeSrcScribe;
            EncStartButtons.B3_1Text = UICaptionProviderM.Buttons.ReEvaluate;
            EncStartButtons.B3_2Text = UICaptionProviderM.Buttons.RunSample;
            EncStartButtons.B3_3Text = UICaptionProviderM.Buttons.StartEncode;
            AnalyzeSrcButtons.B2_1Text = UICaptionProviderM.Buttons.CopyRawAnalysis;
            AnalyzeSrcButtons.B2_2Text = UICaptionProviderM.Buttons.AnalyzeSrcVideo;
            InspBypsChkButtons.B2_1Text = UICaptionProviderM.Buttons.InspectSrcProbelms;
            InspBypsChkButtons.B2_2Text = UICaptionProviderM.Buttons.BypassSrcChecklist;
        }
        private void RefreshCardsLanguage()
        {
            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;
            ToolsImportCard.RefreshLanguage();

            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceIncompatOrCorrupted;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SrcQualityIssues;
            SrcValidationCard.RefreshLanguage();

            EncTermsCard.Name = UICaptionProviderM.Cards.EncPrerequisites;
            EncTermsCard.P1Name = UICaptionProviderM.Cards.EncHardware;
            EncTermsCard.P3Name = UICaptionProviderM.Cards.EncSoftware;
            EncTermsCard.RefreshLanguage();

            BestPracticesCard.Name = UICaptionProviderM.Cards.BestPractices;
            BestPracticesCard.P1Name = UICaptionProviderM.Cards.BestHardware;
            BestPracticesCard.P3Name = UICaptionProviderM.Cards.BestSoftware;
            BestPracticesCard.RefreshLanguage();
        }
        private void RefreshZoneLanguage()
        {
            ApplyDefinitionsToZone(VideoSrcImportZone, ToolCatalogProviderM.GetVideoSrcImportDefs());
            RefreshSourceZonePrimaryText(VideoSrcImportZone);
            ApplyDefinitionsToZone(ScriptSrcImportZone, ToolCatalogProviderM.GetScriptSrcImportDefs());
            RefreshSourceZonePrimaryText(ScriptSrcImportZone);
            ApplyDefinitionsToZone(EncSettingsZone, ToolCatalogProviderM.GetEncSettingsDefinitions());
            WireUpEncSettingsCmds();
            foreach (var zone in AllImportedToolZones)
                ApplyImportedToolDefs(zone);
        }
        private static void RefreshSourceZonePrimaryText(ObservableCollection<ToolItemCardVM> zone)
        {
            foreach (ToolItemCardVM item in zone)
            {
                if (string.IsNullOrWhiteSpace(item.P2TextData)) continue;

                SourceFileKind fileKind = ResolveSourceFileKind(item.Name);
                item.P1TextData = SourceFilePickerH.GetPrimaryText(fileKind, item.P2TextData);
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

        #endregion

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            _modalNavS.CurrentViewModelChanged -= OnModalStateChanged;
            ToolsImportCard.ToolImported -= OnToolImported;
            AnalyticsZone.CollectionChanged -= OnAnalyticsZoneCollectionChanged;
            ToolsImportCard.Dispose();
            UnsubFromImportedToolZones();
            UnsubFromToolsChecklist();
            base.Dispose();
        }
    }
}
