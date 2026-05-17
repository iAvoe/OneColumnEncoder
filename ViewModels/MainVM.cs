using OneColumnEncoder.Commands;
using OneColumnEncoder.Components;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        // Modal navigation, app settings, and tools data stores
        private readonly ModalNavS _modalNavS;
        private readonly AppConfS _appConfS;
        private readonly AppDataS _appDataS;
        
        // Nav Commands
        public OpenUsagesCmd OpenUsages { get; }
        public OpenAppConfCmd OpenAppConf { get; }
        public CloseModalCmd CloseModal { get; }
        public SaveAppConfCmd SaveAppConf { get; }

        public BaseVM? CurrentModalVM => _modalNavS.CurrentModalVM;
        public bool IsModalOpen => _modalNavS.IsOpen;

        public ObservableCollection<EncItemVM> UpstreamsZone { get; }
        public ObservableCollection<EncItemVM> EncodersZone { get; }
        public ObservableCollection<EncItemVM> AnalyticsZone { get; }
        public ObservableCollection<EncItemVM> SourceImportZone { get; } =
        [
            new EncItemVM(new EncItemM("Video Source")),
            new EncItemVM(new EncItemM("AviSynth .avs Source")),
            new EncItemVM(new EncItemM("VapourSynth .vpy Source")),
            new EncItemVM(new EncItemM("SVFI .ini Source")),
        ];
        public ObservableCollection<EncItemVM> EncSettingsZone { get; } =
        [
            new EncItemVM(new EncItemM("Output Setting")),
            new EncItemVM(new EncItemM("Parallelism")),
            new EncItemVM(new EncItemM("Rate Control Mechanism")),
            new EncItemVM(new EncItemM("Base Parameters")),
            new EncItemVM(new EncItemM("Custom Parameters")),
        ];
        public OpenAppConfButtonsVM OpenAppConfButtons { get; }
        public EncodingStartButtonsVM EncodingStartButtons { get; } = new(); // TODO in future

        public ToolsImportCardVM ToolsImportCard { get; } = new ToolsImportCardVM();
        public SourceValidationCardVM SourceValidationCard { get; } = new SourceValidationCardVM();
        public EncodeTermsCardVM EncodeTermsCard { get; } = new EncodeTermsCardVM();
        public BestPracticesCardVM BestPracticesCard { get; } = new BestPracticesCardVM();

        public MainVM(ModalNavS modalNavS, AppConfS appConfS, AppDataS appDataS)
        {
            // Store initialized in App.xaml.cs and passed down to ensure SSOT
            _modalNavS = modalNavS;
            _appConfS = appConfS;
            _appDataS = appDataS;

            // Build zone collections from persisted AppDataS
            UpstreamsZone = [];
            EncodersZone = [];
            AnalyticsZone = [];
            LoadToolsFromAppDataS();

            _modalNavS.CurrentViewModelChanged += ModalNavS_CurrentViewModelChanged;
            OpenAppConf = new OpenAppConfCmd(_modalNavS, _appConfS);
            OpenUsages = new OpenUsagesCmd(_modalNavS);
            OpenAppConfButtons = new OpenAppConfButtonsVM(OpenAppConf, OpenUsages);

            CloseModal = new CloseModalCmd(_modalNavS);
            SaveAppConf = new SaveAppConfCmd(_appConfS, _modalNavS);
            ToolsImportCard.ToolImported += OnToolImported;

            // Initialize import card
            ToolsImportCard.Name = "Import tools:";
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("No Selection"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("", true)); // Separator line
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("ffmpeg.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("vspipe.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("avs2yuv.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("avs2pipemod.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("one_line_shot_args.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("", true)); // Separator line
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("x264.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("x265.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("SvtAv1EncApp.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("", true));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("ffprobe.exe"));
            ToolsImportCard.ImportDropdown.Items.Add(new DropdownItemM("AviSynth.dll"));
            ToolsImportCard.ImportDropdown.SelectedItem =
                ToolsImportCard.ImportDropdown.Items[0];

            // Check lists are within model, no need to set here
            SourceValidationCard.Name = "Source Video Validation";
            SourceValidationCard.P1Name = "Severe (incompatible / corrupted)";
            SourceValidationCard.P3Name = "Moderate (affecting quality)";

            EncodeTermsCard.Name = "Encoding Prerequisites";
            EncodeTermsCard.P1Name = "Hardware";
            EncodeTermsCard.P3Name = "Software";

            BestPracticesCard.Name = "Best Practices";
            BestPracticesCard.P1Name = "Hardware (self check)";
            BestPracticesCard.P3Name = "Software (self check)";
        }

        protected override void Dispose()
        {
            _modalNavS.CurrentViewModelChanged -= ModalNavS_CurrentViewModelChanged;
            ToolsImportCard.ToolImported -= OnToolImported;
            base.Dispose();
        }

        // Update modal-related properties on nav change
        private void ModalNavS_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalVM));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        private void LoadToolsFromAppDataS()
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

        private static void AddTool(string? path, string? version, string displayName, ObservableCollection<EncItemVM> zone)
        {
            if (string.IsNullOrEmpty(path)) return;
            var item = new EncItemVM(new EncItemM(displayName))
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

            // Save to AppDataS and persist
            SetToolPath(toolName, displayName);
            _appDataS.Save();

            var item = new EncItemVM(new EncItemM(displayName))
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
            // Maps tool exe name to AppDataS.Importables properties
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

        private ObservableCollection<EncItemVM>? ZoneForTool(string toolName)
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