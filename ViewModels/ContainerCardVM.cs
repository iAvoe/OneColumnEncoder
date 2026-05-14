using OneColumnEncoder.CommonMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class ContainerCardVM : BaseVM
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public DropdownMenuVM ImportDropdown { get; } = new();

        public ObservableCollection<ChecklistEntryVM> ToolsChecklist { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist2 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> EncodeChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> EncodeChecklist2 { get; } = [];
        public ICommand ImportCommand { get; }

        public ContainerCardVM()
        {

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
    }
}
