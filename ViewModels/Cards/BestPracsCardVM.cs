using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class BestPracsCardVM : ValidationCardBaseVM
    {
        public BestPracsCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetBestPracticeChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetBestPracticeChecklist2());
        }

        public void RefreshLanguage()
        {
            RefreshChecklist(Checklist1, ChecklistProviderM.GetBestPracticeChecklist1());
            RefreshChecklist(Checklist2, ChecklistProviderM.GetBestPracticeChecklist2());
        }
    }
}