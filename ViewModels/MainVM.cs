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

        private void LoadToolsFromAppDataM()
        {
            var t = _appDataM.Tools;
            AddTool(t.FfmpegPath, t.FfmpegVer, "FFMPEG", UpstreamsZone);
            AddTool(t.VspipePath, t.VspipeVer, "VSPipe", UpstreamsZone);
            AddTool(t.Avs2yuvPath, t.Avs2yuvVer, "AVS2YUV", UpstreamsZone);
            AddTool(t.Avs2pipemodPath, t.Avs2pipemodVer, "AVS2PipeMod", UpstreamsZone);
            AddTool(t.OneLineShotArgsPath, null, "OneLineShotArgs", UpstreamsZone);
            AddTool(t.X264Path, t.X264Ver, "x264", EncodersZone);
            AddTool(t.X265Path, t.X265Ver, "x265", EncodersZone);
            AddTool(t.SvtAv1Path, t.SvtAv1Ver, "SVT-AV1", EncodersZone);
            AddTool(t.FfprobePath, t.FfprobeVer, "FFProbe", AnalyticsZone);
            AddTool(t.AviSynthDllPath, null, "AviSynth.dll (for Avs2PipeMod)", AnalyticsZone);
        }

        private void AddTool(string? path, string? version, string displayName, ObservableCollection<ToolItemVM> zone)
        {
            if (string.IsNullOrEmpty(path)) return;
            var item = new ToolItemVM(new EncItemM(displayName))
            {
                Path = path,
                VersionText = version ?? "",
                P1Name = "Version",
                P2Name = "Path",
                R1Text = "Replace",
                R2Text = "Delete"
            };
            WireUpDeleteCmd(item, zone);
            zone.Add(item);
        }

        private void OnToolImported(string toolName, string filePath)
        {
            var resolved = ToolCatalogProviderM.ResolveExe(toolName);
            if (resolved == null) return;

            var zone = resolved.Value.Zone switch
            {
                ToolZone.Upstream => UpstreamsZone,
                ToolZone.Encoder => EncodersZone,
                ToolZone.Analytics => AnalyticsZone,
                _ => null,
            };
            if (zone == null) return;

            ToolCatalogProviderM.TrySetPath(toolName, _appDataM.Tools, filePath);
            _appDataM.Save();

            var item = new ToolItemVM(new EncItemM(resolved.Value.DisplayName))
            {
                Path = filePath,
                P1Name = "Version",
                P2Name = "Path",
                R1Text = "Edit",
                R2Text = "Clear"
            };
            WireUpDeleteCmd(item, zone);
            zone.Add(item);
        }
        private void OnToolDeleted(string toolName)
        {
            var resolved = ToolCatalogProviderM.ResolveExe(toolName);
            if (resolved == null) return;
            ToolCatalogProviderM.TrySetPath(toolName, _appDataM.Tools, string.Empty);
            ToolCatalogProviderM.TrySetVersion(toolName, _appDataM.Tools, string.Empty);
            _appDataM.Save();
        }
    }
}
