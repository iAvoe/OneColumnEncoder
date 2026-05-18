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
        private readonly AppConfM _appConfM; // Load settings to configure ChecklistEntryVM _isEnabled

        public OpenAppConfCmd OpenAppConf { get; }
        public OpenUsagesCmd OpenUsages { get; }

        public ObservableCollection<ToolItemVM> UpstreamsZone { get; }
        public ObservableCollection<ToolItemVM> EncodersZone { get; }
        public ObservableCollection<ToolItemVM> AnalyticsZone { get; }
        public ObservableCollection<ToolItemVM> SrcImportZone { get; } =
        [
            new ToolItemVM(new EncItemM("Video Source"))
            {
                R1Text = "Replace",
                R2Text = "Clear",
                P1Name = "Name",
                P2Name = "Path"
            },
            new ToolItemVM(new EncItemM("AviSynth .avs Source"))
            {
                R1Text = "Replace",
                R2Text = "Clear",
                P1Name = "Mode",
                P2Name = "Path"
            },
            new ToolItemVM(new EncItemM("VapourSynth .vpy Source"))
            {
                R1Text = "Replace",
                R2Text = "Clear",
                P1Name = "Mode",
                P2Name = "Path"
            },
            new ToolItemVM(new EncItemM("SVFI .ini Source"))
            {
                R1Text = "Replace",
                R2Text = "Clear",
                P1Name = "Mode",
                P2Name = "Path"
            },
        ];
        public ObservableCollection<ToolItemVM> EncSettingsZone { get; } =
        [
            new ToolItemVM(new EncItemM("Output Setting"))
            {
                R1Text = "Edit",
                R2Text = "Clear",
                P1Name = "File name w/out extension",
                P2Name = "Path"
            },
            new ToolItemVM(new EncItemM("Parallelism"))
            {
                R1Text = "Edit",
                R2Text = "Clear",
                P1Name = "CPU-RAM Nodes",
                P2Name = "Threads"
            },
            new ToolItemVM(new EncItemM("Rate Control Mechanism"))
            {
                R1Text = "Edit",
                R2Text = "Clear",
                P1Name = "Mode",
                P2Name = "Value"
            },
            new ToolItemVM(new EncItemM("Base Parameters"))
            {
                R1Text = "Edit",
                R2Text = "Clear",
                P1Name = "Stratagem",
            },
            new ToolItemVM(new EncItemM("Custom Parameters"))
            {
                R1Text = "Edit",
                R2Text = "Clear",
                P1Name = "Maximum keyframe gap",
                P2Name = "Other custom params",
            },
        ];
        public ButtonGroupVM OpenAppConfButtons { get; }
        public ButtonGroupVM EncStartButtons { get; }

        public ToolsImportCardVM ToolsImportCard { get; } = new ToolsImportCardVM();
        public SourceValidationCardVM SrcValidationCard { get; } = new SourceValidationCardVM();
        public EncTermsCardVM EncTermsCard { get; } = new EncTermsCardVM();
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

            OpenAppConfButtons = ButtonGroupVM.CreateTwoButton(
                "Usage & Compliance",
                "⚙️ Settings",
                OpenUsages,
                OpenAppConf);
            EncStartButtons = ButtonGroupVM.CreateThreeButton(
                "Re-Evaluate",
                "Run a Sample",
                "Start Encode");
            // TODO: ReEvaluate Cmd
            // TODO: Sample Clip Cmd
            // TODO: Start Encode Cmd
            
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

            SrcValidationCard.Name = "Source Video Validation";
            SrcValidationCard.P1Name = "Severe (incompatible / corrupted)";
            SrcValidationCard.P3Name = "Moderate (affecting quality)";

            EncTermsCard.Name = "Encoding Prerequisites";
            EncTermsCard.P1Name = "Hardware";
            EncTermsCard.P3Name = "Software";

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

        // Enable-disable Encoding Start buttons listening subscription and unsubscription
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
                R1Text = "Replace",
                R2Text = "Delete"
            };
            zone.Add(item);
        }

        private void OnToolImported(string toolName, string filePath)
        {
            var zone = ZoneForTool(toolName);
            if (zone == null) return;

            var displayName = Path.GetFileNameWithoutExtension(toolName);

            // Save to AppDataM and persist
            SetToolPath(toolName, filePath);
            _appDataM.Save();

            var item = new ToolItemVM(new EncItemM(displayName))
            {
                Path = filePath,
                P1Name = "Version",
                P2Name = "Path",
                R1Text = "Edit",
                R2Text = "Clear"
            };
            zone.Add(item);
        }

        private void SetToolPath(string toolName, string filePath)
        {
            // Maps tool exe name to AppDataM.Importables properties
            var t = _appDataM.Tools;
            switch (toolName.ToLowerInvariant())
            {
                case "ffmpeg.exe":              t.FfmpegPath = filePath; break;
                case "vspipe.exe":              t.VspipePath = filePath; break;
                case "avs2yuv.exe":             t.Avs2yuvPath = filePath; break;
                case "avs2pipemod.exe":         t.Avs2pipemodPath = filePath; break;
                case "one_line_shot_args.exe":  t.OneLineShotArgsPath = filePath; break;
                case "x264.exe":                t.X264Path = filePath; break;
                case "x265.exe":                t.X265Path = filePath; break;
                case "svtav1encapp.exe":        t.SvtAv1Path = filePath; break;
                case "ffprobe.exe":             t.FfprobePath = filePath; break;
                case "avisynth.dll":            t.AviSynthDllPath = filePath; break;
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