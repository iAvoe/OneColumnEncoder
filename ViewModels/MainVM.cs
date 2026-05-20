using Microsoft.Win32;
using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
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
        public ObservableCollection<ToolItemVM> AnalyticsZone { get; }
        public ObservableCollection<ToolItemVM> SrcImportZone { get; }
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

        // Prevent UI responding during settings or confirmation modal is opening
        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        public MainVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages, AppDataM appDataM, AppConfM appConfM, ModalNavS modalNavS)
        {
            // Tools data, Settings data, Modal Navigation, Open Settings Command
            _appDataM = appDataM;
            _appConfM = appConfM;
            _modalNavS = modalNavS;
            OpenAppConf = openAppConf;
            OpenUsages = openUsages;
            SelectTool = new SelectToolCmd(this);

            // SrcImportZone Card
            ToolsImportCard = new ToolsImportCardVM(modalNavS);

            // Initialize main UI zones
            SrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetSourceImportDefinitions());
            EncSettingsZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetEncSettingsDefinitions());
            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
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
            SubToToolsChecklist();
            _modalNavS.CurrentViewModelChanged += OnModalStateChanged;
            IsOverlayVisible = _modalNavS.IsOpen;
        }

        // SrcImportZone and EncSettingsZone creation by initialization and import
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
                item.R2Command = new ActionCmd(_ => zone.Remove(item));
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

        #region Encoding Start button states
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
        private void UpdateEncodingStartButtonsState()
        {
            bool allToolsReady =
                ToolsImportCard.ToolsChecklist.All(entry => entry.Status == StatusType.Success);
            bool atLeastOneUpstream = ToolsImportCard.ToolsChecklist
                .Where(entry => ToolCatalogProviderM.UpstreamDisplayNames.Contains(entry.Text))
                .Any(entry => entry.Status == StatusType.Success);
            bool atLeastOneEncoder = ToolsImportCard.ToolsChecklist
                .Where(entry => ToolCatalogProviderM.EncoderDisplayNames.Contains(entry.Text))
                .Any(entry => entry.Status == StatusType.Success);
            bool atLeastOneAnalytics = ToolsImportCard.ToolsChecklist
                .Where(entry => ToolCatalogProviderM.AnalyticsDisplayNames.Contains(entry.Text))
                .Any(entry => entry.Status == StatusType.Success);

            bool sourceValidationReady =
                SrcValidationCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                SrcValidationCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);
            bool encodeTermsReady =
                EncTermsCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                EncTermsCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);

            EncStartButtons.B3_2IsEnabled =
                allToolsReady && atLeastOneUpstream && atLeastOneEncoder && atLeastOneAnalytics && sourceValidationReady && encodeTermsReady;
            EncStartButtons.B3_3IsEnabled =
                allToolsReady && atLeastOneUpstream && atLeastOneEncoder && atLeastOneAnalytics && sourceValidationReady && encodeTermsReady;
        }
        #endregion

        // Bind delete/replace tool commands to UI
        private void WireUpZoneDeleteCmds()
        {
            foreach (ToolItemVM item in SrcImportZone)
            {
                WireUpDeleteCmd(item, SrcImportZone);
                WireUpSrcReplaceCmd(item);
            }
            foreach (ToolItemVM item in EncSettingsZone)
                WireUpDeleteCmd(item, EncSettingsZone);
            foreach (ToolItemVM item in UpstreamsZone)
                WireUpToolCmd(item);
            foreach (ToolItemVM item in EncodersZone)
                WireUpToolCmd(item);
            foreach (ToolItemVM item in AnalyticsZone)
                WireUpToolCmd(item);
        }
        private void WireUpDeleteCmd(ToolItemVM item, ObservableCollection<ToolItemVM> zone)
        {
            item.R2Command = new DeleteToolCmd(item, zone, _appDataM);
        }
        private void WireUpToolCmd(ToolItemVM item)
        {
            item.R1Command = new ReplaceToolCmd(item, _appDataM, _modalNavS);
            item.R2Command = new DeleteToolCmd(item, GetZoneForTool(ResolveToolZone(item.Name)), _appDataM);
        }
        private void WireUpSrcReplaceCmd(ToolItemVM item)
        {
            item.R1Command = new ActionCmd(_ =>
            {
                OpenFileDialog dialog = new()
                {
                    Filter = "All files (*.*)|*.*",
                    Title = $"Select {item.Name}",
                    CheckFileExists = true,
                    CheckPathExists = true
                };
                if (dialog.ShowDialog() == true)
                    item.Path = dialog.FileName;
            });
        }
        private static ToolZone ResolveToolZone(string displayName)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(displayName);
            return def?.Zone ?? throw new ArgumentException($"Unknown tool: {displayName}");
        }

        // Check which zone a tool belongs to, and overwrite a tool if duplicate import happens
        private ObservableCollection<ToolItemVM> GetZoneForTool(ToolZone zone) => zone switch
        {
            ToolZone.Upstream => UpstreamsZone,
            ToolZone.Encoder => EncodersZone,
            ToolZone.Analytics => AnalyticsZone,
            _ => throw new ArgumentException("Invalid tool zone")
        };
        private void AddOrUpdateTool(string defKey, string? path, string? version)
        {
            if (!ToolDefinitionProviderM.ToolDefinitions.TryGetValue(defKey, out ToolDefinitionM? def)) return;
            if (def.Zone == null || string.IsNullOrEmpty(path)) return;

            ObservableCollection<ToolItemVM> zone = GetZoneForTool(def.Zone.Value);
            ToolItemVM? existing = zone.FirstOrDefault(i => i.Name == def.DisplayName);
            if (existing != null) zone.Remove(existing);

            ToolItemVM item = new(new EncItemM(def.DisplayName))
            {
                Path = path,
                // VersionText = version ?? "", // version variable functionality is not coded yet, this only overwrites default to ""
                P1Name = def.P1Name,
                P2Name = def.P2Name ?? "",
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
            foreach ((string defKey, ToolDefinitionM def) in ToolDefinitionProviderM.ToolDefinitions)
            {
                if (def.Zone == null || def.ExeName == null) continue;

                (string? path, string? version) = def.ExeName switch
                {
                    "ffmpeg.exe" => (t.FfmpegPath, t.FfmpegVer),
                    "vspipe.exe" => (t.VspipePath, t.VspipeVer),
                    "avs2yuv.exe" => (t.Avs2yuvPath, t.Avs2yuvVer),
                    "avs2pipemod.exe" => (t.Avs2pipemodPath, t.Avs2pipemodVer),
                    "one_line_shot_args.exe" => (t.OneLineShotArgsPath, null),
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

        // Import tool: Validate tool, create card UI & app data
        private void OnToolImported(string exeName, string filePath)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByExeName(exeName);
            if (def == null || def.Zone == null) return;

            string defKey = ToolDefinitionProviderM.ToolDefinitions
                .FirstOrDefault(kvp => kvp.Value == def).Key;
            if (defKey == null) return;

            ToolCatalogProviderM.TrySetPath(exeName, _appDataM.Tools, filePath);
            _appDataM.Save();

            AddOrUpdateTool(defKey, filePath, null);
        }

        // Navigated to other modal windows
        private void OnModalStateChanged() { IsOverlayVisible = _modalNavS.IsOpen; }

        public override void Dispose()
        {
            _modalNavS.CurrentViewModelChanged -= OnModalStateChanged;
            ToolsImportCard.ToolImported -= OnToolImported;
            ToolsImportCard.Dispose();
            UnsubFromToolsChecklist();
            base.Dispose();
        }
    }
}
