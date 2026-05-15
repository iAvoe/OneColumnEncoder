using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class ValidationCardBaseVM : BaseVM
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _p1Name = string.Empty;
        public string P1Name
        {
            get => _p1Name;
            set => SetProperty(ref _p1Name, value);
        }

        private string _p3Name = string.Empty;
        public string P3Name
        {
            get => _p3Name;
            set => SetProperty(ref _p3Name, value);
        }

        // P2, P4
        public ObservableCollection<ChecklistEntryVM> Checklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> Checklist2 { get; } = [];
    }
}
