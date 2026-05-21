using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class SourceValidationCardVM : ValidationCardBaseVM
    {
        private const int SourcePickedChecklistIdx = 0; 
        public SourceValidationCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }

        public void SetSourcePickedStatus(bool isPicked)
        {
            if (SourcePickedChecklistIdx >= 0 && SourcePickedChecklistIdx < Checklist1.Count)
                Checklist1[SourcePickedChecklistIdx].Status = isPicked ? StatusType.Success : StatusType.Error;
        }

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetSourceChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetSourceChecklist2());
        }
    }
}
