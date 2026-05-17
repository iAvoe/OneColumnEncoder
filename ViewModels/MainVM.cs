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
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class MainVM : BaseVM
    {
        // Modal navigation, app settings, and tools data stores
        private readonly ModalNavS _modalNavS;
        private readonly AppConfS _appConfS;
        private readonly AppDataS _appDataS;

        public BaseVM? CurrentModalVM => _modalNavS.CurrentModalVM;
        public bool IsModalOpen => _modalNavS.IsOpen;
        public ICommand OpenAppConfCmd { get; }

        public ObservableCollection<EncItemVM> UpstreamsZone { get; } =
        [
            new EncItemVM(new EncItemM("FFMPEG")),
            new EncItemVM(new EncItemM("VSPipe")),
            new EncItemVM(new EncItemM("AVS2YUV")),
            new EncItemVM(new EncItemM("AVS2PipeMod")),
            new EncItemVM(new EncItemM("SVFI")),
        ];
        public ObservableCollection<EncItemVM> EncodersZone { get; } =
        [
            new EncItemVM(new EncItemM("x264")),
            new EncItemVM(new EncItemM("x265")),
            new EncItemVM(new EncItemM("SVT-AV1")),
        ];
        public ObservableCollection<EncItemVM> AnalyticsZone { get; } =
        [
            new EncItemVM(new EncItemM("FFProbe")),
            new EncItemVM(new EncItemM("AviSynth.dll (for Avs2PipeMod)")),
        ];
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
        public OpenSettingButtonsVM OpenSettingsButtonsVM { get; } = new();
        public EncodingStartButtonsVM EncodingStartButtonsVM { get; } = new();

        public ToolsImportCardVM ToolsImportCard { get; } = new ToolsImportCardVM();
        public SourceValidationCardVM SourceValidationCard { get; } = new SourceValidationCardVM();
        public EncodeTermsCardVM EncodeTermsCard { get; } = new EncodeTermsCardVM();
        public BestPracticesCardVM BestPracticesCard { get; } = new BestPracticesCardVM();

        public MainVM(ModalNavS modalNavS, AppConfS appConfS, AppDataS appDataS)
        {
            _modalNavS = modalNavS;
            _appConfS = appConfS;
            _appDataS = appDataS;

            // Subscribe to modal changes
            _modalNavS.CurrentViewModelChanged += ModalNavS_CurrentViewModelChanged;
            OpenAppConfCmd = new OpenAppConfCmd(_modalNavS);

            // Subscribe to settings changes to update card checks

            // Subscribe to tools data changes to update import cards

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