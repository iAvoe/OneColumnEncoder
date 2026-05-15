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
    public class SourceValidationCardVM : BaseVM
    {
        private string _name = string.Empty;
        public string Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _p1Name = string.Empty;
        public string P1Name {
            get => _p1Name;
            set => SetProperty(ref _p1Name, value);
        }

        private string _p3Name = string.Empty;
        public string P3Name {
            get => _p3Name;
            set => SetProperty(ref _p3Name, value);
        }

        public ObservableCollection<ChecklistEntryVM> SourceChecklist1 { get; } = [];
        public ObservableCollection<ChecklistEntryVM> SourceChecklist2 { get; } = [];

        public SourceValidationCardVM()
        {
            FillCollection(SourceChecklist1, ChecklistProviderS.GetSourceChecklist1());
            FillCollection(SourceChecklist2, ChecklistProviderS.GetSourceChecklist2());
        }
    }
}
