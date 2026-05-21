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

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly AppDataM _appDataM;
        private readonly AppConfM _appConfM;
        private readonly ModalNavS _modalNavS;

        // Groups of Card or other element UIs
        public ObservableCollection<ToolItemVM> UpstreamsZone { get; }
        public ObservableCollection<ToolItemVM> EncodersZone { get; }
        public ObservableCollection<ToolItemVM> AnalyticsZone { get; } // A-D separated for dual single-select
        public ObservableCollection<ToolItemVM> DependenciesZone { get; }
        public ObservableCollection<ToolItemVM> VideoSrcImportZone { get; } // V-S separated for dual single-select
        public ObservableCollection<ToolItemVM> ScriptSrcImportZone { get; }
        public ObservableCollection<ToolItemVM> EncSettingsZone { get; }
        // Buttons
        public ButtonGroupVM OpenAppConfButtons { get; }
        public ButtonGroupVM EncStartButtons { get; }
        // Commands
        public OpenAppConfCmd OpenAppConf { get; }
        public OpenUsagesCmd OpenUsages { get; }
        public SelectToolCmd SelectTool { get; }
        // Card UIs
        public ToolsImportCardVM ToolsImportCard { get; }
        public SourceValidationCardVM SrcValidationCard { get; } = new();
        public EncTermsCardVM EncTermsCard { get; } = new();
        public BestPracticesCardVM BestPracticesCard { get; } = new();

        // Section header texts (for MainUI.xaml binding)
        public string SectionImportTools => UILangProviderM.Current["Section.ImportTools"];
        public string SectionSelectUpstream => UILangProviderM.Current["Section.SelectUpstream"];
        public string SectionSelectEncoder => UILangProviderM.Current["Section.SelectEncoder"];
        public string SectionSelectAnalytics => UILangProviderM.Current["Section.SelectAnalytics"];
        public string SectionImportSource => UILangProviderM.Current["Section.ImportSource"];
        public string SectionAnalysisResults => UILangProviderM.Current["Section.AnalysisResults"];
        public string SectionEncodingConfigs => UILangProviderM.Current["Section.EncodingConfigs"];
        public string SectionStartEncoding => UILangProviderM.Current["Section.StartEncoding"];

        // Prevent UI responding during settings or confirmation modal is opening
        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        private ObservableCollection<ToolItemVM>[] AllImportedToolZones =>
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
            VideoSrcImportZone =
                LoadZoneFromDefinitions(ToolCatalogProviderM.GetVideoSrcImportDefs());
            ScriptSrcImportZone =
                LoadZoneFromDefinitions(ToolCatalogProviderM.GetScriptSrcImportDefs());
            EncSettingsZone =
                LoadZoneFromDefinitions(ToolCatalogProviderM.GetEncSettingsDefinitions());
            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
            DependenciesZone = [];
            LoadToolsFromAppDataM();
            WireUpZoneDeleteCmds();

            // Buttons
            OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.UsageAndCompliance,
                UICaptionProviderM.Buttons.Settings,
                OpenUsages,
                OpenAppConf);
            EncStartButtons = ButtonGroupVM.CreateThreeButton(
                UICaptionProviderM.Buttons.ReEvaluate,
                UICaptionProviderM.Buttons.RunSample,
                UICaptionProviderM.Buttons.StartEncode);

            // Import dropdown menu and behavior
            ToolsImportCard.ToolImported += OnToolImported;
            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;

            foreach (DropdownItemM item in ToolCatalogProviderM.GetImportDropdownItems())
                ToolsImportCard.ImportDropdown.Items.Add(item);
            ToolsImportCard.ImportDropdown.SelectedItem =
                ToolsImportCard.ImportDropdown.Items[0];

            // Other validations or simply lists for Start Encode button
            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceSevere;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SourceModerate;
            EncTermsCard.Name = UICaptionProviderM.Cards.EncPrerequisites;
            EncTermsCard.P1Name = UICaptionProviderM.Cards.EncHardware;
            EncTermsCard.P3Name = UICaptionProviderM.Cards.EncSoftware;
            BestPracticesCard.Name = UICaptionProviderM.Cards.BestPractices;
            BestPracticesCard.P1Name = UICaptionProviderM.Cards.BestHardware;
            BestPracticesCard.P3Name = UICaptionProviderM.Cards.BestSoftware;

            // Checklist item settings, checklist subs, nav subs, overlay subs
            InitializeChecklistEntryStates();
            SubToImportedToolZones();
            RefreshUpstreamToolState(); // initial state after loading
            SubToToolsChecklist();
            _modalNavS.CurrentViewModelChanged += OnModalStateChanged;
            IsOverlayVisible = _modalNavS.IsOpen;
            UILangProviderM.CurrentChanged += OnLanguageChanged;
            RefreshLanguage();
        }

        #endregion

        #region Zone Initialization

        private static ObservableCollection<ToolItemVM> LoadZoneFromDefinitions(List<ToolDefinitionM> defs)
        {
            ObservableCollection<ToolItemVM> zone = [];
            foreach (ToolDefinitionM def in defs)
            {
                ToolItemVM item = new(new EncItemM(def.DisplayName))
                {
                    R1Text = def.R1Text,
                    R2Text = def.R2Text,
                    P1Name = def.P1Name,
                    P2Name = def.P2Name ?? ""
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
            if (cl2.Count >= 1) cl2[0].IsEnabled = g.OSFileNameInvalid;
            if (cl2.Count >= 2) cl2[1].IsEnabled = g.FTPFileNameInvalid;
            if (cl2.Count >= 3) cl2[2].IsEnabled = g.NoWritePermission;
            if (cl2.Count >= 4) cl2[3].IsEnabled = g.IsOverwriting;
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
        private void OnImportedToolZoneCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshUpstreamToolState();
            RefreshImportedToolsChecklist();
            ToolCompatibilityH.RefreshDependencySelectionState(UpstreamsZone, DependenciesZone, UpdateEncodingStartButtonsState);
            ToolCompatibilityH.RefreshSourceSelectionState(UpstreamsZone, ScriptSrcImportZone, RefreshSelectedSourceStatus);
        }
        private void RefreshUpstreamToolState()
        {
            ToolItemVM? avs2pipemod = UpstreamsZone.FirstOrDefault(t => ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            if (avs2pipemod == null) return;

            if (!HasImportedAviSynthDll())
            {
                avs2pipemod.IsSelected = false;
                avs2pipemod.IsEnabled = false;
            }
            else avs2pipemod.IsEnabled = true;
        }

        private void RefreshImportedToolsChecklist()
        {
            ToolsImportCard.RefreshToolsChecklist(
                hasUpstreamTool: UpstreamsZone.Count > 0,
                hasEncoderTool: EncodersZone.Count > 0,
                hasFfprobe: HasImportedFfprobe());
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
            UpdateEncodingStartButtonsState();
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
                UpdateEncodingStartButtonsState();
        }
        public void UpdateEncodingStartButtonsState()
        {
            bool toolsReady =
                UpstreamsZone.Count > 0 &&
                EncodersZone.Count > 0 &&
                HasImportedFfprobe();

            bool toolsPickedReady =
                ToolsImportCard.ToolsChecklist
                    .Skip(3)
                    .All(e => !e.IsEnabled || e.Status == StatusType.Success);

            bool sourcePickedReady =
                !SrcValidationCard.Checklist1[0].IsEnabled || SrcValidationCard.Checklist1[0].Status == StatusType.Success;

            bool sourceValidationReady =
                SrcValidationCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                SrcValidationCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);
            bool encodeTermsReady =
                EncTermsCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                EncTermsCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);
            bool avsSelected = UpstreamsZone.Any(t => t.IsSelected && ToolDefinitionProviderM.IsImportedTool(t.Name, "avs2pipemod.exe"));
            bool aviSelected = DependenciesZone.Any(t => t.IsSelected && ToolDefinitionProviderM.IsImportedTool(t.Name, "avisynth.dll"));
            bool dependencyReady = avsSelected == aviSelected;

            bool allReady = toolsReady && toolsPickedReady && sourcePickedReady && sourceValidationReady && encodeTermsReady && dependencyReady;
            EncStartButtons.B3_2IsEnabled = allReady;
            EncStartButtons.B3_3IsEnabled = allReady;
        }

        #endregion

        #region Command Wiring (Bind R1-R2)
        private void WireUpZoneDeleteCmds()
        {
            foreach (ToolItemVM tool in VideoSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemVM tool in ScriptSrcImportZone) WireUpSourceCmd(tool);
            foreach (ToolItemVM tool in EncSettingsZone) WireUpStaticClearCmd(tool);
            foreach (var zone in AllImportedToolZones)
                foreach (ToolItemVM tool in zone) WireUpToolCmd(tool);
        }
        private void WireUpToolCmd(ToolItemVM item)
        {
            item.R1Command =
                new ReplaceToolCmd(item, _appDataM, _modalNavS);
            item.R2Command =
                new DeleteToolCmd(item, GetZoneForTool(ToolDefinitionProviderM.ResolveToolZone(item.Name)), _appDataM);
        }
        private void WireUpSourceCmd(ToolItemVM item)
        {
            item.R1Command =
                new BrowseSourcePathCmd(item, ResolveSourceFileKind(item.Name), _appDataM);
            item.R2Command =
                new ClearToolItemCmd(item, RefreshSelectedSourceStatus);
        }
        private void WireUpStaticClearCmd(ToolItemVM item)
        {
            item.R2Command = new ClearToolItemCmd(item);
        }
        public void RefreshSelectedSourceStatus()
        {
            bool anySelected = VideoSrcImportZone.Any(t => t.IsSelected) || ScriptSrcImportZone.Any(t => t.IsSelected);
            SrcValidationCard.SetSourcePickedStatus(anySelected);
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

        private ObservableCollection<ToolItemVM> GetZoneForTool(ToolZone zone) => zone switch
        {
            ToolZone.Upstream => UpstreamsZone,
            ToolZone.Encoder => EncodersZone,
            ToolZone.Analytics => AnalyticsZone,
            ToolZone.Dependencies => DependenciesZone,
            _ => throw new ArgumentException("Invalid tool zone")
        };

        #endregion

        #region Tool CRUD & Import

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
            ObservableCollection<ToolItemVM> zone = GetZoneForTool(def.Zone.Value);
            ToolItemVM? existing = zone.FirstOrDefault(i => i.Name == def.DisplayName);
            if (existing != null) zone.Remove(existing);

            ToolItemVM item = new(new EncItemM(def.DisplayName))
            {
                Path = filePath,
                VersionText = version ?? string.Empty,
                P1Name = def.P1Name,
                P2Name = def.P2Name ?? string.Empty,
                R1Text = def.R1Text,
                R2Text = def.R2Text
            };
            WireUpToolCmd(item);
            zone.Add(item);
        }

        // Load tool from app data
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
                    "avisynth.dll" => (t.AviSynthDllPath, null),
                    _ => (null, null)
                };

                if (!string.IsNullOrEmpty(path))
                    AddOrUpdateTool(defKey, path, version);
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

        #endregion

        #region Modal Navigation

        private void OnModalStateChanged() { IsOverlayVisible = _modalNavS.IsOpen; }

        #endregion

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
            OnPropertyChanged(nameof(SectionImportSource));
            OnPropertyChanged(nameof(SectionAnalysisResults));
            OnPropertyChanged(nameof(SectionEncodingConfigs));
            OnPropertyChanged(nameof(SectionStartEncoding));
        }
        private void RefreshButtonCaptions()
        {
            OpenAppConfButtons.B2_1Text = UICaptionProviderM.Buttons.UsageAndCompliance;
            OpenAppConfButtons.B2_2Text = UICaptionProviderM.Buttons.Settings;
            EncStartButtons.B3_1Text = UICaptionProviderM.Buttons.ReEvaluate;
            EncStartButtons.B3_2Text = UICaptionProviderM.Buttons.RunSample;
            EncStartButtons.B3_3Text = UICaptionProviderM.Buttons.StartEncode;
        }
        private void RefreshCardsLanguage()
        {
            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;
            ToolsImportCard.RefreshLanguage();

            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceSevere;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SourceModerate;
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
            foreach (var zone in AllImportedToolZones)
                ApplyImportedToolDefs(zone);
        }
        private static void RefreshSourceZonePrimaryText(ObservableCollection<ToolItemVM> zone)
        {
            foreach (ToolItemVM item in zone)
            {
                if (string.IsNullOrWhiteSpace(item.Path)) continue;

                SourceFileKind fileKind = ResolveSourceFileKind(item.Name);
                item.VersionText = SourceFilePickerH.GetPrimaryText(fileKind, item.Path);
            }
        }
        private static void ApplyDefinitionsToZone(ObservableCollection<ToolItemVM> zone, List<ToolDefinitionM> definitions)
        {
            for (int i = 0; (i < definitions.Count && i < zone.Count); i++)
            {
                zone[i].ApplyDefinition(definitions[i]);
                zone[i].RefreshLanguage();
            }
        }
        private static void ApplyImportedToolDefs(ObservableCollection<ToolItemVM> zone)
        {
            foreach (ToolItemVM item in zone)
            {
                ToolDefinitionM? definition =
                    ToolDefinitionProviderM.GetByDisplayName(item.Name);
                if (definition != null) item.ApplyDefinition(definition);
                item.RefreshLanguage();
            }
        }
        #endregion

        #region Dispose

        public override void Dispose()
        {
            UILangProviderM.CurrentChanged -= OnLanguageChanged;
            _modalNavS.CurrentViewModelChanged -= OnModalStateChanged;
            ToolsImportCard.ToolImported -= OnToolImported;
            ToolsImportCard.Dispose();
            UnsubFromImportedToolZones();
            UnsubFromToolsChecklist();
            base.Dispose();
        }

        #endregion
    }
}