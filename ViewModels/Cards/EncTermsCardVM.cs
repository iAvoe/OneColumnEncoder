using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class EncTermsCardVM : ValidationCardBaseVM
    {
        public EncTermsCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
        }

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
        }
    }
}
