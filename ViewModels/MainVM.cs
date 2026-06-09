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
        public InspectEncProblemsCmd InspectEncProblems { get; }
        public BypassEncChecklistCmd BypassEncChecklist { get; }
        public OpenSampleClipCmd SampleClip { get; }
        public StartEncCmd StartEncode { get; }
        public SelectToolCmd SelectTool { get; } // ItemCard select on click
        public ButtonGroupVM OpenAppConfButtons { get; } // OpenUsages & OpenAppConf
        public ButtonGroupVM ScriptScbButtons { get; } // OneClickScriptGen & OpenScriptScribe
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
        // Card UIs
        public ToolsImportCardVM ToolsImportCard { get; }
        public SourceCheckCardVM SrcValidationCard { get; } = new();
        public EncTermsCardVM EncTermsCard { get; } = new();
        public BestPracsSelfCheckCardVM BestPracticesCard { get; } = new();
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
        public static string SVFIClipDisabledHintText => UILangProviderM.Current["Hint.SVFIClipDisabled"];
        public static string AnalyzeNeedsSourceText => UILangProviderM.Current["Hint.AnalyzeNeedsSource"];
        public static string NumaCpuCheckHintText => UILangProviderM.Current["Hint.NumaCpuCheckTrigger"];

        // Disable UI when other modal opens
        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        private bool _svfiClipDisabledHintVisible;
        public bool SVFIClipDisabledHintVisible
        {
            get => _svfiClipDisabledHintVisible;
            set => SetProperty(ref _svfiClipDisabledHintVisible, value);
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
            VideoSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetVideoSrcImportDefs(), true, false);
            ScriptSrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetScriptSrcImportDefs(), true, false);
            EncSettingsZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetEncSettingsDefinitions(), enableRealCheck: false);
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
                string cachedOutputDirectory = NormalizeOutputDirectory(_appDataM.Encoding.OutputDirectory);
                outputSetting.PropertyChanged += OnOutputSettingPropertyChanged;
                // Must be set first because Path setter has Validate() call which clears VersionText
                if (string.IsNullOrWhiteSpace(outputSetting.P2TextData))
                    outputSetting.P2TextData = cachedOutputDirectory;
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
                modalNavS, () => GetCurrentVideoSourcePath(), ScriptSrcImportZone[0], ScriptSrcImportZone[1], OnSourceImported, () => _srcVideoAnalysis.RawJson);
            CopyRawAnalysis = new CopyRawAnalysisCmd(
                _srcVideoAnalysis, modalNavS);
            AnalyzeSrcVideo = new AnalyzeSrcVideoCmd(
                GetSelectedFfprobePath, GetSelectedVideoSourcePath, _srcVideoAnalysis, SrcValidationCard, modalNavS,
                OnSourceAnalysisCompleted,
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
            SampleClip = new OpenSampleClipCmd(modalNavS, BuildEncodingPipelineRequest, _srcVideoAnalysis);
            StartEncode = new StartEncCmd(BuildEncodingPipelineRequest, modalNavS, appConfM);

            // Buttons
            OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.UsageAndCompliance, UICaptionProviderM.Buttons.Settings, OpenUsages, OpenAppConf);
            OpenAppConfButtons.B2_1Icon = SvgIconProviderH.GamePhone;
            OpenAppConfButtons.B2_2Icon = SvgIconProviderH.GameSetting;
            ScriptScbButtons = ButtonGroupVM.CreateTwoButton( // UpdateScriptScbButtonsState()
                UICaptionProviderM.Buttons.OneClickScriptGen, UICaptionProviderM.Buttons.OpenScribeSrcScribe, OneClickScriptGen, OpenScriptScribe);
            AnalyzeSrcButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.CopyRawAnalysis, UICaptionProviderM.Buttons.AnalyzeSrcVideo, CopyRawAnalysis, AnalyzeSrcVideo);
            _isAnalyzeSrcButtonsReady = true;
            EncStartButtons = ButtonGroupVM.CreateThreeButton( // UpdateEncStartButtonsState()
                UICaptionProviderM.Buttons.ReEvaluate, UICaptionProviderM.Buttons.RunSample, UICaptionProviderM.Buttons.StartEncode,
                new ActionCmd(_ => ReEvaluateAllChecks()), SampleClip, StartEncode);
            EncStartButtons.B3_1Icon = SvgIconProviderH.GameRefresh;
            EncStartButtons.B3_2Icon = SvgIconProviderH.GameLocation;
            EncStartButtons.B3_3Icon = SvgIconProviderH.GamePlay;
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
            BestPracticesCard.Subtitle = UICaptionProviderM.Cards.BestPracticesSubtitle;

            SrcValidationCard.IsSvtav1SelectedFunc = () =>
                EncodersZone.Any(t => t.IsSelected
                    && ToolDefinitionProviderM.IsImportedTool(t.Name, "svtav1encapp.exe"));

            EncTermsCard.GetOutputDirectoryFunc = () =>
            {
                ToolItemCardVM? output = EncSettingsZone.FirstOrDefault(t =>
                    t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));
                return output?.P2TextData ?? string.Empty;
            };
            EncTermsCard.GetOutputFilePathFunc = () =>
            {
                ToolItemCardVM? output = EncSettingsZone.FirstOrDefault(t =>
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
                ToolItemCardVM? videoSrc = VideoSrcImportZone.FirstOrDefault(
                    t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
                return videoSrc?.P2TextData ?? string.Empty;
            };

            // Checklist subs, nav subs, overlay subs
            EncTermsCard.RunAllChecks();
            SubToImportedToolZones();
            AnalyticsZone.CollectionChanged += OnAnalyticsZoneCollectionChanged;
            RefreshImportedToolStates(); // initial state after loading
            RevertCancelledAutoSelection(UpstreamsZone);
            RevertCancelledAutoSelection(DependenciesZone);
            SubToToolsChecklist();
            UpdateScriptScbButtonsState(); // Initial state of script scribe buttons
            RefreshSelectedSourceStatus();
            UpdateAnalyzeSrcButtonsState();
            UpdateInspBypsChkButtonsState();
            UpdateInspBypsEncChkButtonsState();
            _modalNavS.CurrentViewModelChanged += OnModalStateChanged;
            IsOverlayVisible = _modalNavS.IsOpen;
            UILangProviderM.CurrentChanged += OnLanguageChanged;
            RefreshLanguage();
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
            ToolCompatibilityH.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                UpstreamsZone, ScriptSrcImportZone, RefreshSelectedSourceStatus);

            // Revert the selection only for IsCancel caused by "Auto Selection"
            if (autoSelected && sender is ObservableCollection<ToolItemCardVM> changedZone)
                RevertCancelledAutoSelection(changedZone);
        }

        private bool ApplyDefaultImportedToolSelection(ObservableCollection<ToolItemCardVM> zone)
        {
            bool autoSelected = ItemCardSelectionH.ApplyDefaultSelection(zone);
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
            ToolCompatibilityH.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                UpstreamsZone, ScriptSrcImportZone, RefreshSelectedSourceStatus);
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
            ToolCompatibilityH.RefreshDependencySelectionState(
                UpstreamsZone, DependenciesZone, UpdateEncStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(
                UpstreamsZone, ScriptSrcImportZone, RefreshSelectedSourceStatus);
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

            if (_modalNavS.CurrentModalVM is ScriptScribeVM modal)
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

            bool vspipeReady = UpstreamsZone.All(t =>
                !ToolDefinitionProviderM.IsImportedTool(t.Name, "vspipe.exe") || t.IsEnabled);

            bool toolsReady =
                UpstreamsZone.Count > 0 && EncodersZone.Count > 0 && HasImportedFfprobe() && vspipeReady;

            bool toolsPickedReady =
                ToolsImportCard.ToolsChecklist.Skip(3).All(e => !e.IsEnabled || e.Status == StatusType.Success);

            bool hasRawJson = !string.IsNullOrWhiteSpace(_srcVideoAnalysis.RawJson);

            // Checklist:
            // 1. Selected 
            // SVT-AV1 may eventually have 12bit support, but for now keep encoding start button disabled
            bool sourceValidationReady = SrcValidationCard.IsBypassed ||
                (hasRawJson &&
                SrcValidationCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                SrcValidationCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success));

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

            bool scriptSourceReady = true;
            ToolItemCardVM? selUpstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected);
            if (selUpstream != null)
            {
                string? exe = ToolCatalogProviderM.ResolveExeFromDisplayName(selUpstream.Name);
                if (exe is "vspipe.exe" or "avs2yuv.exe" or "avs2pipemod.exe" or "one_line_shot_args.exe")
                {
                    SourceFileKind expectedKind = exe switch
                    {
                        "vspipe.exe" => SourceFileKind.VapourSynthScript,
                        "avs2yuv.exe" or "avs2pipemod.exe" => SourceFileKind.AviSynthScript,
                        "one_line_shot_args.exe" => SourceFileKind.SvfiIni,
                        _ => SourceFileKind.Video
                    };
                    scriptSourceReady = ScriptSrcImportZone.Any(t =>
                        t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData) &&
                        ResolveSourceFileKind(t.Name) == expectedKind);
                }
            }

            bool allReady = toolsReady && toolsPickedReady && sourceValidationReady && encodeTermsReady && dependencyReady && scriptSourceReady;
            EncStartButtons.B3_2IsEnabled = allReady && !oneLineShotSelected;
            EncStartButtons.B3_3IsEnabled = allReady;
            SVFIClipDisabledHintVisible = oneLineShotSelected;
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
            foreach (ToolItemCardVM tool in EncSettingsZone) WireUpStaticClearCmd(tool);
            WireUpEncSettingsCmds();
            foreach (ObservableCollection<ToolItemCardVM> zone in AllImportedToolZones)
                foreach (ToolItemCardVM tool in zone) WireUpToolCmd(tool);
        }
        private void WireUpToolCmd(ToolItemCardVM item)
        {
            item.R1Command = new ReplaceToolCmd(
                item, _appDataM, _modalNavS, RefreshImportedToolStates);
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
            SourceFileKind kind = ResolveSourceFileKind(item.Name);
            item.R1Command = kind == SourceFileKind.Video
                ? new BrowseSourcePathCmd(item, kind, _appDataM, _modalNavS, OnVideoSourceImported)
                : new BrowseSourcePathCmd(item, kind, _appDataM, _modalNavS, OnVideoSourceImported, GetCurrentVideoSourcePath);
            item.R2Command = new ClearToolItemCmd(item, () => OnSourceCleared(kind));
            item.PropertyChanged += OnVideoSrcItemPropertyChanged;
        }
        private static void WireUpStaticClearCmd(ToolItemCardVM item) =>
            item.R2Command = new ClearToolItemCmd(item);
        private void WireUpEncSettingsCmds()
        {
            if (EncSettingsZone.Count > 1)
                EncSettingsZone[1].R1Command = new OpenParallelismConfCmd(_modalNavS, EncSettingsZone[1]);

            ToolItemCardVM? outputSetting = EncSettingsZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));

            if (outputSetting != null)
                outputSetting.R1Command = new OpenFilenameScribeCmd(_modalNavS, outputSetting);

            ToolItemCardVM? compressionParams = EncSettingsZone.FirstOrDefault(t =>
                t.Name.Equals(UILangProviderM.Current["Tool.Enc.EncParams"], StringComparison.OrdinalIgnoreCase));

            if (compressionParams != null)
            {
                compressionParams.R1Command = new OpenEncoderConfCmd(_modalNavS, compressionParams);
                EncoderConfVM.ApplySavedSettingsToCard(compressionParams);
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
                RefreshEncTermsState();
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

            if (kind == SourceFileKind.Video)
            {
                ToolItemCardVM? outputSetting = EncSettingsZone.FirstOrDefault(t =>
                    t.Name.Equals(UILangProviderM.Current["Tool.Enc.OutputSetting"], StringComparison.OrdinalIgnoreCase));
                if (outputSetting != null)
                    outputSetting.P1TextData = Path.GetFileNameWithoutExtension(filePath);

                foreach (ToolItemCardVM source in VideoSrcImportZone)
                    source.IsSelected = false;

                foreach (ToolItemCardVM script in ScriptSrcImportZone)
                {
                    script.P2TextData = string.Empty;
                    script.P1TextData = string.Empty;
                    script.IsSelected = false;
                }
                SaveSourcePath(SourceFileKind.AviSynthScript, string.Empty);
                SaveSourcePath(SourceFileKind.VapourSynthScript, string.Empty);
                SaveSourcePath(SourceFileKind.SvfiIni, string.Empty);
            }
            else
            {
                foreach (ToolItemCardVM source in ScriptSrcImportZone)
                    source.IsSelected = false;
            }

            item.IsSelected = true;
            _appDataM.Save();
            RefreshSelectedSourceStatus(resetAnalysis: kind == SourceFileKind.Video);
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
            ConfirmationVM vm = ConfirmationVM.CreateWarning(
                UILangProviderM.Current["ScriptGen.WindowTitle"],
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

        private void PromptRunSourceAnalysisAfterReplace()
        {
            if (!AnalyzeSrcVideo.CanExecute(null)) return;

            ConfirmationModal window = new();
            CloseModalCmd cancelCmd = new(window.Close);
            ConfirmationVM vm = ConfirmationVM.CreateWarning(
                UILangProviderM.Current["SrcAnalysis.WindowTitle"],
                UILangProviderM.Current["SrcAnalysis.RunAfterReplace"],
                cancelCmd,
                new ActionCmd(_ =>
                {
                    window.DialogResult = true;
                    window.Close();
                    _promptScriptGenAfterAnalysis = true;
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
                ToolsImportCard.ResetCompleteSourceAnalysisStatus();
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
            ToolsImportCard.ResetCompleteSourceAnalysisStatus();
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

        private EncodingPipelineRequest? BuildEncodingPipelineRequest()
        {
            ToolItemCardVM? upstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected && t.IsEnabled && !string.IsNullOrWhiteSpace(t.P2TextData));
            ToolItemCardVM? encoder = EncodersZone.FirstOrDefault(t => t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData));
            ToolItemCardVM? outputSetting = EncSettingsZone.FirstOrDefault(t =>
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
                    var (iniInputPath, iniTaskId) = EncodingPipelineH.ParseSvfiIni(svfiIniPath);
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
                SvfiTaskId: svfiTaskId);
        }

        private string GetSelectedSvfiIniPath()
        {
            ToolItemCardVM? svfiIni = ScriptSrcImportZone.FirstOrDefault(t =>
                t.IsSelected && !string.IsNullOrWhiteSpace(t.P2TextData) &&
                ResolveSourceFileKind(t.Name) == SourceFileKind.SvfiIni);
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

            ToolItemCardVM? source = ScriptSrcImportZone.FirstOrDefault(t =>
                ResolveSourceFileKind(t.Name) == kind && !string.IsNullOrWhiteSpace(t.P2TextData));
            return source?.P2TextData ?? string.Empty;
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
                await ToolVersionDetectH.DetectAndStoreVspipeY4mArgAsync(
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
            if (!hasVideoSource && !string.IsNullOrWhiteSpace(_appDataM.Tools.VideoSourcePath))
            {
                _appDataM.Tools.VideoSourcePath = string.Empty;
                _appDataM.Save();
            }

            LoadSourceItem(ScriptSrcImportZone[0], SourceFileKind.AviSynthScript, _appDataM.Tools.AvsSourcePath);
            LoadSourceItem(ScriptSrcImportZone[1], SourceFileKind.VapourSynthScript, _appDataM.Tools.VpySourcePath);
            LoadSourceItem(ScriptSrcImportZone[2], SourceFileKind.SvfiIni, _appDataM.Tools.SvfiSourcePath);

            ToolItemCardVM? selectedUpstream = UpstreamsZone.FirstOrDefault(t => t.IsSelected);
            if (selectedUpstream != null)
            {
                string? upstreamExe = ToolCatalogProviderM.ResolveExeFromDisplayName(selectedUpstream.Name);
                if (upstreamExe != null)
                {
                    foreach (ToolItemCardVM src in ScriptSrcImportZone)
                    {
                        if (string.IsNullOrWhiteSpace(src.P2TextData)) continue;

                        bool isMatch = (upstreamExe, ResolveSourceFileKind(src.Name)) switch
                        {
                            ("vspipe.exe", SourceFileKind.VapourSynthScript) => true,
                            ("avs2yuv.exe", SourceFileKind.AviSynthScript) => true,
                            ("avs2pipemod.exe", SourceFileKind.AviSynthScript) => true,
                            ("one_line_shot_args.exe", SourceFileKind.SvfiIni) => true,
                            _ => false
                        };

                        if (isMatch) src.IsSelected = true;
                    }
                }
            }

            RefreshEncSettingsState();
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
            OnPropertyChanged(nameof(SVFIClipDisabledHintText));
            OnPropertyChanged(nameof(AnalyzeNeedsSourceText));
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
            RefreshSourceZonePrimaryText(VideoSrcImportZone);
            ApplyDefinitionsToZone(ScriptSrcImportZone, ToolCatalogProviderM.GetScriptSrcImportDefs());
            RefreshSourceZonePrimaryText(ScriptSrcImportZone);
            ApplyDefinitionsToZone(EncSettingsZone, ToolCatalogProviderM.GetEncSettingsDefinitions());
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

            vspipe.IsEnabled = ToolVersionDetectH.HasValidVspipeY4mArg(
                _appDataM.Tools.VspipePath,
                _appDataM.Tools.VspipeY4mArg);
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
