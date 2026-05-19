using OneColumnEncoder.Commands;
using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Components;
using OneColumnEncoder.Models;
using OneColumnEncoder.ViewModels.Cards;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        private readonly AppDataM _appDataM;
        private readonly AppConfM _appConfM;

        public OpenAppConfCmd OpenAppConf { get; }
        public OpenUsagesCmd OpenUsages { get; }

        public ObservableCollection<ToolItemVM> UpstreamsZone { get; }
        public ObservableCollection<ToolItemVM> EncodersZone { get; }
        public ObservableCollection<ToolItemVM> AnalyticsZone { get; }
        public ObservableCollection<ToolItemVM> SrcImportZone { get; }
        public ObservableCollection<ToolItemVM> EncSettingsZone { get; }
        public ButtonGroupVM OpenAppConfButtons { get; }
        public ButtonGroupVM EncStartButtons { get; }

        public ToolsImportCardVM ToolsImportCard { get; } = new();
        public SourceValidationCardVM SrcValidationCard { get; } = new();
        public EncTermsCardVM EncTermsCard { get; } = new();
        public BestPracticesCardVM BestPracticesCard { get; } = new();

        public MainVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages, AppDataM appDataM, AppConfM appConfM)
        {
            _appDataM = appDataM;
            _appConfM = appConfM;
            OpenAppConf = openAppConf;
            OpenUsages = openUsages;

            SrcImportZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetSourceImportDefinitions());
            EncSettingsZone = LoadZoneFromDefinitions(ToolCatalogProviderM.GetEncSettingsDefinitions());

            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
            LoadToolsFromAppDataM();
            WireUpZoneDeleteCmds();

            OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
                UICaptionProviderM.Buttons.UsageAndCompliance,
                UICaptionProviderM.Buttons.Settings,
                OpenUsages,
                OpenAppConf);
            EncStartButtons = ButtonGroupVM.CreateThreeButton(
                UICaptionProviderM.Buttons.ReEvaluate,
                UICaptionProviderM.Buttons.RunSample,
                UICaptionProviderM.Buttons.StartEncode);

            ToolsImportCard.ToolImported += OnToolImported;

            ToolsImportCard.Name = UICaptionProviderM.Cards.ToolsImport;
            foreach (var item in ToolCatalogProviderM.GetImportDropdownItems())
                ToolsImportCard.ImportDropdown.Items.Add(item);
            ToolsImportCard.ImportDropdown.SelectedItem =
                ToolsImportCard.ImportDropdown.Items[0];

            SrcValidationCard.Name = UICaptionProviderM.Cards.SourceValidation;
            SrcValidationCard.P1Name = UICaptionProviderM.Cards.SourceSevere;
            SrcValidationCard.P3Name = UICaptionProviderM.Cards.SourceModerate;

            EncTermsCard.Name = UICaptionProviderM.Cards.EncPrerequisites;
            EncTermsCard.P1Name = UICaptionProviderM.Cards.EncHardware;
            EncTermsCard.P3Name = UICaptionProviderM.Cards.EncSoftware;

            BestPracticesCard.Name = UICaptionProviderM.Cards.BestPractices;
            BestPracticesCard.P1Name = UICaptionProviderM.Cards.BestHardware;
            BestPracticesCard.P3Name = UICaptionProviderM.Cards.BestSoftware;

            InitializeChecklistEntryStates();
            SubToToolsChecklist();
        }

        private static ObservableCollection<ToolItemVM> LoadZoneFromDefinitions(List<ToolDefinitionM> defs)
        {
            var zone = new ObservableCollection<ToolItemVM>();
            foreach (var def in defs)
            {
                var item = new ToolItemVM(new EncItemM(def.DisplayName))
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

        protected override void Dispose()
        {
            ToolsImportCard.ToolImported -= OnToolImported;
            UnsubFromToolsChecklist();
            base.Dispose();
        }

        private void InitializeChecklistEntryStates()
        {
            AppConfM.GeneralSettings g = _appConfM.General;

            var cl1 = EncTermsCard.Checklist1;
            if (cl1.Count >= 1) cl1[0].IsEnabled = g.OffGrid;
            if (cl1.Count >= 2) cl1[1].IsEnabled = g.InsufficientRAM;
            if (cl1.Count >= 3) cl1[2].IsEnabled = g.InsufficientDiskSpace;

            var cl2 = EncTermsCard.Checklist2;
            if (cl2.Count >= 1) cl2[0].IsEnabled = g.OSFileNameInvalid;
            if (cl2.Count >= 2) cl2[1].IsEnabled = g.FTPFileNameInvalid;
            if (cl2.Count >= 3) cl2[2].IsEnabled = g.NoWritePermission;
            if (cl2.Count >= 4) cl2[3].IsEnabled = g.IsOverwriting;
        }

        private void SubToToolsChecklist()
        {
            foreach (var entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in SrcValidationCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in SrcValidationCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in EncTermsCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in EncTermsCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            UpdateEncodingStartButtonsState();
        }
        private void UnsubFromToolsChecklist()
        {
            foreach (var entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in SrcValidationCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in SrcValidationCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in EncTermsCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in EncTermsCard.Checklist2)
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

        private void WireUpZoneDeleteCmds()
        {
            foreach (var item in SrcImportZone)
                WireUpDeleteCmd(item, SrcImportZone);
            foreach (var item in EncSettingsZone)
                WireUpDeleteCmd(item, EncSettingsZone);
            foreach (var item in AnalyticsZone)
                WireUpDeleteCmd(item, AnalyticsZone);
        }

        private void WireUpDeleteCmd(ToolItemVM item, ObservableCollection<ToolItemVM> zone)
        {
            item.R2Command =
                new ActionCmd(_ => { zone.Remove(item); OnToolDeleted(item.Name); });
        }

        private ObservableCollection<ToolItemVM> GetZoneForTool(ToolZone zone) => zone switch
        {
            ToolZone.Upstream => UpstreamsZone,
            ToolZone.Encoder => EncodersZone,
            ToolZone.Analytics => AnalyticsZone,
            _ => throw new ArgumentException("Invalid tool zone")
        };

        private void AddOrUpdateTool(string defKey, string? path, string? version)
        {
            if (!ToolDefinitionProviderM.ToolDefinitions.TryGetValue(defKey, out var def)) return;
            if (def.Zone == null || string.IsNullOrEmpty(path)) return;

            var zone = GetZoneForTool(def.Zone.Value);
            var existing = zone.FirstOrDefault(i => i.Name == def.DisplayName);
            if (existing != null) zone.Remove(existing);

            var item = new ToolItemVM(new EncItemM(def.DisplayName))
            {
                Path = path,
                VersionText = version ?? "",
                P1Name = def.P1Name,
                P2Name = def.P2Name ?? "",
                R1Text = def.R1Text,
                R2Text = def.R2Text
            };
            WireUpDeleteCmd(item, zone);
            zone.Add(item);
        }

        private void LoadToolsFromAppDataM()
        {
            AppDataM.Importables t = _appDataM.Tools;
            foreach (var (defKey, def) in ToolDefinitionProviderM.ToolDefinitions)
            {
                if (def.Zone == null || def.ExeName == null) continue;

                var (path, version) = def.ExeName switch
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

        private void OnToolImported(string exeName, string filePath)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByExeName(exeName);
            if (def == null || def.Zone == null) return;

            var defKey = ToolDefinitionProviderM.ToolDefinitions
                .FirstOrDefault(kvp => kvp.Value == def).Key;
            if (defKey == null) return;

            ToolCatalogProviderM.TrySetPath(exeName, _appDataM.Tools, filePath);
            _appDataM.Save();

            AddOrUpdateTool(defKey, filePath, null);
        }

        private void OnToolDeleted(string toolName)
        {
            ToolDefinitionM? def = ToolDefinitionProviderM.GetByDisplayName(toolName);
            if (def == null || def.ExeName == null) return;

            ToolCatalogProviderM.TrySetPath(def.ExeName, _appDataM.Tools, string.Empty);
            ToolCatalogProviderM.TrySetVersion(def.ExeName, _appDataM.Tools, string.Empty);
            _appDataM.Save();
        }
    }
}
