using OneColumnEncoder.Commands;
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

        public MainVM()
        {
            /*
            _modelNav_S = new ModelNav_S();
            CentralVM = new Central_VM(_modelNav_S);

            _modelNav_S.CurrentViewModelChanged += ModelNav_S_CurrentViewModelChanged;
            */

            // 初始化下拉菜单数据
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