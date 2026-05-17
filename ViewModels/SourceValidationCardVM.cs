using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class SourceValidationCardVM : ValidationCardBaseVM
    {
        public SourceValidationCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }
    }
}
