using OneColumnEncoder.Commands;
using OneColumnEncoder.Components;
using OneColumnEncoder.Models;
using OneColumnEncoder.Stores;
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
        private readonly ModalNavS _modalNavS;
        private readonly AppConfS _appConfS;
        // Expose current modal VM and open state for binding
        public BaseVM? CurrentModalVM => _modalNavS.CurrentModalVM;
        public bool IsModalOpen => _modalNavS.IsOpen;

        public ObservableCollection<EncItemVM> UpstreamsZone { get; set; }
        public ObservableCollection<EncItemVM> EncodersZone { get; set; }
        public ObservableCollection<EncItemVM> AnalyticsZone { get; set; }
        public ObservableCollection<EncItemVM> SourceImportZone { get; set; }
        public ObservableCollection<EncItemVM> EncSettingsZone { get; set; }
        // ? EncStartingZone
        public OpenSettingButtonsVM OpenSettingsButtonsVM { get; set; }
        public EncodingStartButtonsVM EncodingStartButtonsVM { get; set; }

        // Centralized view modal that contains all sub-VMs
        // public Central_VM CentralVM { get; }

        public ToolsImportCardVM ToolsImportCard { get; } = new ToolsImportCardVM();
        public SourceValidationCardVM SourceValidationCard { get; } = new SourceValidationCardVM();
        public EncodeTermsCardVM EncodePermissionCard { get; } = new EncodeTermsCardVM();

        public MainVM(ModalNavS modalNavS, AppConfS appConfS)
        {
            _modalNavS = modalNavS;
            _appConfS = appConfS;
            // Subscribe to modal changes
            _modalNavS.CurrentViewModelChanged += ModalNavS_CurrentViewModelChanged;

            OpenSettingsButtonsVM = new OpenSettingButtonsVM();

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
            EncodePermissionCard.Name = "Encoding Prerequisites";
            EncodePermissionCard.P1Name = "Hardware";
            EncodePermissionCard.P3Name = "Software";

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
                new EncItemVM(new EncItemM("Parallelism")),
                new EncItemVM(new EncItemM("Rate Control Mechanism")),
                new EncItemVM(new EncItemM("Base Parameters")),
                new EncItemVM(new EncItemM("Custom Parameters")),
            ];
            // TODO: EncStartingZone = [];
            EncodingStartButtonsVM = new EncodingStartButtonsVM();

        }

        protected override void Dispose()
        {
            _modalNavS.CurrentViewModelChanged -= ModalNavS_CurrentViewModelChanged;
            base.Dispose();
        }

        // Update modal-related properties on nav change
        private void ModalNavS_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalVM));
            OnPropertyChanged(nameof(IsModalOpen));
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