using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels.Cards
{
    public class BestPracticesCardVM : ValidationCardBaseVM
    {
        public BestPracticesCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetBestPracticeChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetBestPracticeChecklist2());
        }
    }
}
