using OneColumnEncoder.Commands;
using OneColumnEncoder.Components;
using OneColumnEncoder.Models;
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
        private readonly AppConfM _appConfM; // Load settings to configure ChecklistEntryVM _isEnabled

        public OpenAppConfCmd OpenAppConf { get; }
        public OpenUsagesCmd OpenUsages { get; }

        public ObservableCollection<ToolItemVM> UpstreamsZone { get; }
        public ObservableCollection<ToolItemVM> EncodersZone { get; }
        public ObservableCollection<ToolItemVM> AnalyticsZone { get; }
        public ObservableCollection<ToolItemVM> SourceImportZone { get; } =
        [
            new ToolItemVM(new EncItemM("Video Source")),
            new ToolItemVM(new EncItemM("AviSynth .avs Source")),
            new ToolItemVM(new EncItemM("VapourSynth .vpy Source")),
            new ToolItemVM(new EncItemM("SVFI .ini Source")),
        ];
        public ObservableCollection<ToolItemVM> EncSettingsZone { get; } =
        [
            new ToolItemVM(new EncItemM("Output Setting")),
            new ToolItemVM(new EncItemM("Parallelism")),
            new ToolItemVM(new EncItemM("Rate Control Mechanism")),
            new ToolItemVM(new EncItemM("Base Parameters")),
            new ToolItemVM(new EncItemM("Custom Parameters")),
        ];
        public OpenAppConfButtonsVM OpenAppConfButtons { get; }
        public EncodeStartButtonsVM EncodingStartButtons { get; } = new();

        public ToolsImportCardVM ToolsImportCard { get; } = new ToolsImportCardVM();
        public SourceValidationCardVM SourceValidationCard { get; } = new SourceValidationCardVM();
        public EncodeTermsCardVM EncodeTermsCard { get; } = new EncodeTermsCardVM();
        public BestPracticesCardVM BestPracticesCard { get; } = new BestPracticesCardVM();

        public MainVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages, AppDataM appDataM, AppConfM appConfM)
        {
            _appDataM = appDataM;
            _appConfM = appConfM;
            OpenAppConf = openAppConf;
            OpenUsages = openUsages;

            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
            LoadToolsFromAppDataM();

            OpenAppConfButtons = new OpenAppConfButtonsVM(OpenAppConf, OpenUsages);

            ToolsImportCard.ToolImported += OnToolImported;

            ToolsImportCard.Name = "Import tools:";
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("No Selection"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("", true));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("ffmpeg.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("vspipe.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("avs2yuv.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("avs2pipemod.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("one_line_shot_args.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("", true));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("x264.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("x265.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("SvtAv1EncApp.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("", true));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("ffprobe.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("AviSynth.dll"));
            ToolsImportCard.ImportDropdown.SelectedItem =
                ToolsImportCard.ImportDropdown.Items[0];

            SourceValidationCard.Name = "Source Video Validation";
            SourceValidationCard.P1Name = "Severe (incompatible / corrupted)";
            SourceValidationCard.P3Name = "Moderate (affecting quality)";

            EncodeTermsCard.Name = "Encoding Prerequisites";
            EncodeTermsCard.P1Name = "Hardware";
            EncodeTermsCard.P3Name = "Software";

            BestPracticesCard.Name = "Best Practices";
            BestPracticesCard.P1Name = "Hardware (self check)";
            BestPracticesCard.P3Name = "Software (self check)";

            InitializeChecklistEntryStates();
            SubToToolsChecklist();
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

            var cl1 = EncodeTermsCard.Checklist1;
            if (cl1.Count >= 1) cl1[0].IsEnabled = g.OffGrid;
            if (cl1.Count >= 2) cl1[1].IsEnabled = g.InsufficientRAM;
            if (cl1.Count >= 3) cl1[2].IsEnabled = g.InsufficientDiskSpace;

            var cl2 = EncodeTermsCard.Checklist2;
            if (cl2.Count >= 1) cl2[0].IsEnabled = g.OSFileNameInvalid;
            if (cl2.Count >= 2) cl2[1].IsEnabled = g.FTPFileNameInvalid;
            if (cl2.Count >= 3) cl2[2].IsEnabled = g.NoWritePermission;
            if (cl2.Count >= 4) cl2[3].IsEnabled = g.IsOverwriting;
        }

        // Enable-disable Encoding Start buttons listening subscription and unsubscription
        private void SubToToolsChecklist()
        {
            foreach (var entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in SourceValidationCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in SourceValidationCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in EncodeTermsCard.Checklist1)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            foreach (var entry in EncodeTermsCard.Checklist2)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            UpdateEncodingStartButtonsState();
        }
        private void UnsubFromToolsChecklist()
        {
            foreach (var entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in SourceValidationCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in SourceValidationCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in EncodeTermsCard.Checklist1)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            foreach (var entry in EncodeTermsCard.Checklist2)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
        }

        // Enable-disable Encoding Start button, i.e., if PC changed from battery power to plugged in
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
                .Where(entry => entry.Text is "FFMPEG" or "VSPipe" or "AVS2YUV" or "AVS2PipeMod" or "OneLineShotArgs")
                .Any(entry => entry.Status == StatusType.Success);
            bool atLeastOneEncoder = ToolsImportCard.ToolsChecklist
                .Where(entry => entry.Text is "x264" or "x265" or "SVT-AV1")
                .Any(entry => entry.Status == StatusType.Success);
            bool atLeastOneAnalytics = ToolsImportCard.ToolsChecklist
                .Where(entry => entry.Text is "FFProbe" or "AviSynth.dll (for Avs2PipeMod)")
                .Any(entry => entry.Status == StatusType.Success);

            bool sourceValidationReady =
                SourceValidationCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                SourceValidationCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);
            bool encodeTermsReady =
                EncodeTermsCard.Checklist1.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success) &&
                EncodeTermsCard.Checklist2.Where(e => e.IsEnabled).All(e => e.Status == StatusType.Success);

            EncodingStartButtons.B3_2IsEnabled =
                allToolsReady && atLeastOneUpstream && atLeastOneEncoder && atLeastOneAnalytics && sourceValidationReady && encodeTermsReady;
            EncodingStartButtons.B3_3IsEnabled =
                allToolsReady && atLeastOneUpstream && atLeastOneEncoder && atLeastOneAnalytics && sourceValidationReady && encodeTermsReady;
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

        private static void AddTool(string? path, string? version, string displayName, ObservableCollection<ToolItemVM> zone)
        {
            if (string.IsNullOrEmpty(path)) return;
            var item = new ToolItemVM(new EncItemM(displayName))
            {
                Path = path,
                VersionText = version ?? "",
                P1Name = "Version",
                P2Name = "Path",
                R1Text = "Edit",
                R2Text = "Clear"
            };
            zone.Add(item);
        }

        private void OnToolImported(string toolName)
        {
            var zone = ZoneForTool(toolName);
            if (zone == null) return;

            var displayName = Path.GetFileNameWithoutExtension(toolName);

            // Save to AppDataM and persist
            SetToolPath(toolName, displayName);
            _appDataM.Save();

            var item = new ToolItemVM(new EncItemM(displayName))
            {
                P1Name = "Version",
                P2Name = "Path",
                R1Text = "Edit",
                R2Text = "Clear"
            };
            zone.Add(item);
        }

        private void SetToolPath(string toolName, string displayName)
        {
            // Maps tool exe name to AppDataM.Importables properties
            // TODO: Replace placeholder path with actual resolved path when ImportToolAsync is implemented
            var t = _appDataM.Tools;
            switch (toolName.ToLowerInvariant())
            {
                case "ffmpeg.exe":              t.FfmpegPath = displayName; break;
                case "vspipe.exe":              t.VspipePath = displayName; break;
                case "avs2yuv.exe":             t.Avs2yuvPath = displayName; break;
                case "avs2pipemod.exe":         t.Avs2pipemodPath = displayName; break;
                case "one_line_shot_args.exe":  t.OneLineShotArgsPath = displayName; break;
                case "x264.exe":                t.X264Path = displayName; break;
                case "x265.exe":                t.X265Path = displayName; break;
                case "svtav1encapp.exe":        t.SvtAv1Path = displayName; break;
                case "ffprobe.exe":             t.FfprobePath = displayName; break;
                case "avisynth.dll":            t.AviSynthDllPath = displayName; break;
            }
        }

        private ObservableCollection<ToolItemVM>? ZoneForTool(string toolName)
        {
            return toolName.ToLowerInvariant() switch
            {
                "ffmpeg.exe" or "vspipe.exe" or "avs2yuv.exe" or "avs2pipemod.exe"
                    or "one_line_shot_args.exe" => UpstreamsZone,
                "x264.exe" or "x265.exe" or "svtav1encapp.exe" => EncodersZone,
                "ffprobe.exe" or "avisynth.dll" => AnalyticsZone,
                _ => null,
            };
        }

    }
}