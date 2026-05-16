using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class BestPracticesCardVM : ValidationCardBaseVM
    {
        public BestPracticesCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderS.GetBestPracticeChecklist1());
            FillCollection(Checklist2, ChecklistProviderS.GetBestPracticeChecklist2());
        }
    }
}
