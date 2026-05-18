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
        private readonly AppDataM _appDataS;

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

        public MainVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages, AppDataM appDataS)
        {
            _appDataS = appDataS;
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

            SubToToolsChecklist();
        }

        protected override void Dispose()
        {
            ToolsImportCard.ToolImported -= OnToolImported;
            UnsubFromToolsChecklist();
            base.Dispose();
        }

        // Enable-disable Encoding Start buttons listening subscription and unsubscription
        private void SubToToolsChecklist()
        {
            foreach (var entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            ToolsImportCard.ToolsChecklist.CollectionChanged += OnToolsChecklistCollectionChanged;
            UpdateEncodingStartButtonsState();
        }
        private void UnsubFromToolsChecklist()
        {
            foreach (var entry in ToolsImportCard.ToolsChecklist)
                entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            ToolsImportCard.ToolsChecklist.CollectionChanged -= OnToolsChecklistCollectionChanged;
        }

        // Subscribe/unsubscribe to tools' PropertyChanged events
        private void OnToolsChecklistCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (ChecklistEntryVM entry in e.NewItems)
                    entry.PropertyChanged += OnChecklistEntryPropertyChanged;
            if (e.OldItems != null)
                foreach (ChecklistEntryVM entry in e.OldItems)
                    entry.PropertyChanged -= OnChecklistEntryPropertyChanged;
            UpdateEncodingStartButtonsState();
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
                .Where(entry => entry.Text == "FFMPEG" || entry.Text == "VSPipe" || entry.Text == "AVS2YUV" || entry.Text == "AVS2PipeMod" || entry.Text == "OneLineShotArgs")
                .Any(entry => entry.Status == StatusType.Success);
            bool atLeastOneEncoder = ToolsImportCard.ToolsChecklist
                .Where(entry => entry.Text == "x264" || entry.Text == "x265" || entry.Text == "SVT-AV1")
                .Any(entry => entry.Status == StatusType.Success);
            bool atLeastOneAnalytics = ToolsImportCard.ToolsChecklist
                .Where(entry => entry.Text == "FFProbe" || entry.Text == "AviSynth.dll (for Avs2PipeMod)")
                .Any(entry => entry.Status == StatusType.Success);
            EncodingStartButtons.B3_2IsEnabled = allToolsReady && atLeastOneUpstream && atLeastOneEncoder && atLeastOneAnalytics;
            EncodingStartButtons.B3_3IsEnabled = allToolsReady && atLeastOneUpstream && atLeastOneEncoder && atLeastOneAnalytics;
        }

        private void LoadToolsFromAppDataM()
        {
            var t = _appDataS.Tools;
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
            _appDataS.Save();

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
            var t = _appDataS.Tools;
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