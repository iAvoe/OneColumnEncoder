using OneColumnEncoder.Commands;
using OneColumnEncoder.CommonMethods;
using OneColumnEncoder.Components;
using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        // private readonly ModelNav_S _modelNav_S;
        // public Base_VM? CurrentModalVM => _modelNav_S.CurrentModalVM;
        // public bool IsModalOpen => _modelNav_S.IsOpen;
        public ObservableCollection<EncItemVM> UpstreamsZone { get; set; }
        public ObservableCollection<EncItemVM> EncodersZone { get; set; }
        public ObservableCollection<EncItemVM> AnalyticsZone { get; set; }
        public ObservableCollection<EncItemVM> SourceImportZone { get; set; }
        public ObservableCollection<EncItemVM> EncSettingsZone { get; set; }
        // ? EncStartingZone
        public EncodingStartButtonsVM EncodingStartButtons_VM { get; set; }

        // Centralized view modal that contains all sub-VMs
        // public Central_VM CentralVM { get; }

        // Dropdown
        public DropdownMenuVM ImportDropdown { get; } = new();

        // Checklists
        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist2 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> EncodeChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> EncodeChecklist2 { get; } = [];

        public MainVM()
        {
            /*
            _modelNav_S = new ModelNav_S();
            CentralVM = new Central_VM(_modelNav_S);

            _modelNav_S.CurrentViewModelChanged += ModelNav_S_CurrentViewModelChanged;
            */

            // Initialize dropdown
            ImportDropdown.Items.Add(new DropdownItemM("No Selection"));
            ImportDropdown.Items.Add(new DropdownItemM("", true)); // Separator line
            ImportDropdown.Items.Add(new DropdownItemM("ffmpeg.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("vspipe.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("avs2yuv.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("avs2pipemod.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("one_line_shot_args.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("", true)); // Separator line
            ImportDropdown.Items.Add(new DropdownItemM("x264.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("x265.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("SvtAv1EncApp.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("", true));
            ImportDropdown.Items.Add(new DropdownItemM("ffprobe.exe"));
            ImportDropdown.Items.Add(new DropdownItemM("AviSynth.dll"));
            ImportDropdown.SelectedItem = ImportDropdown.Items[0];
            ImportDropdown.SelectionChangedCommand = new SelectDropdownCmd();

            // TODO: ToolsImportZone = [];
            UpstreamsZone =
            [
                new EncItemVM(new EncItemM("FFMPEG")),
                new EncItemVM(new EncItemM("VSPipe")),
                new EncItemVM(new EncItemM("AVS2YUV")),
                new EncItemVM(new EncItemM("AVS2PipeMod")),
                new EncItemVM(new EncItemM("SVFI")),
            ];
            EncodersZone =
            [
                new EncItemVM(new EncItemM("x264")),
                new EncItemVM(new EncItemM("x265")),
                new EncItemVM(new EncItemM("SVT-AV1")),
            ];
            AnalyticsZone =
            [
                new EncItemVM(new EncItemM("FFProbe")),
                new EncItemVM(new EncItemM("AviSynth.dll (for Avs2PipeMod)")),
            ];
            SourceImportZone =
            [
                new EncItemVM(new EncItemM("Video Source")),
                new EncItemVM(new EncItemM("AviSynth .avs Source")),
                new EncItemVM(new EncItemM("VapourSynth .vpy Source")),
                new EncItemVM(new EncItemM("SVFI .ini Source")),
            ];
            // TODO: AnalyticResultsZone = [];
            EncSettingsZone =
            [
                new EncItemVM(new EncItemM("Output Setting")),
                new EncItemVM(new EncItemM("Threading Setting")),
                new EncItemVM(new EncItemM("Rate Controls")),
                new EncItemVM(new EncItemM("Base Parameters")),
                new EncItemVM(new EncItemM("Custom Parameters")),
            ];
            // TODO: EncStartingZone = [];
            EncodingStartButtons_VM = new EncodingStartButtonsVM();

            // Fill checklists
            ToolsChecklist.Add(new ChecklistEntryVM
            {
                Text = "One upsream program available",
                Status = StatusType.Error
            });
            ToolsChecklist.Add(new ChecklistEntryVM
            {
                Text = "One downstream program available",
                Status = StatusType.Error
            });
            ToolsChecklist.Add(new ChecklistEntryVM
            {
                Text = "One analysis program available",
                Status = StatusType.Error
            });

            // SourceChecklist1: Severe
            SourceChecklist1.Add(new ChecklistEntryVM
            {
                Text = "Metadata is readable",
                Status = StatusType.Waiting
            });
            SourceChecklist1.Add(new ChecklistEntryVM
            {
                Text = "Progressive video frame / not interlated (SVT-AV1 req.)",
                Status = StatusType.Waiting
            });
            SourceChecklist1.Add(new ChecklistEntryVM
            {
                Text = "Bit-depth is less than 12 (SVT-AV1 req.)",
                Status = StatusType.Waiting
            });

            // SourceChecklist2: General
            SourceChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Framerate is constant / not variable",
                Status = StatusType.Waiting
            });
            SourceChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Square pixel aspect ratio / 1:1 sar",
                Status = StatusType.Waiting
            });
            SourceChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Color matrix matadata is normal",
                Status = StatusType.Waiting
            });
            SourceChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Transfer characteristics matadata is normal",
                Status = StatusType.Waiting
            });
            SourceChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Color primaries metadata is normal",
                Status = StatusType.Waiting
            });
            SourceChecklist2.Add(new ChecklistEntryVM
            {
                Text = "←/↖ chroma sample location (SVT-AV1 req.)",
                Status = StatusType.Waiting
            });

            // EncodeChecklist1: Hardware Conditions
            EncodeChecklist1.Add(new ChecklistEntryVM
            {
                Text = "Using power adapter or battery level above 30%",
                Status = StatusType.Waiting
            });
            EncodeChecklist1.Add(new ChecklistEntryVM
            {
                Text = "Sufficient available RAM",
                Status = StatusType.Waiting
            });
            EncodeChecklist1.Add(new ChecklistEntryVM
            {
                Text = "Sufficient available disk space",
                Status = StatusType.Waiting
            });

            // EncodeChecklist2: Software Conditions
            EncodeChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Output filename is valid for OS",
                Status = StatusType.Waiting
            });
            EncodeChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Output filename is valid for FTP (optional)",
                Status = StatusType.Waiting
            });
            EncodeChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Output format is compatible with the downstream program",
                Status = StatusType.Waiting
            });
            EncodeChecklist2.Add(new ChecklistEntryVM
            {
                Text = "Output file does not overwrite existing files",
                Status = StatusType.Waiting
            });
        }

        /*
        protected override void Dispose()
        {
            _modelNav_S.CurrentViewModelChanged -= ModelNav_S_CurrentViewModelChanged;
            base.Dispose();
        }
        
        private void ModelNavStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalVM));
            OnPropertyChanged(nameof(IsModalOpen));
        }
        */

    }
}