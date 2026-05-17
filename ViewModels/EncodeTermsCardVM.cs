using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class EncodeTermsCardVM : ValidationCardBaseVM
    {
        public EncodeTermsCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderM.GetEncodeChecklist1());
            FillCollection(Checklist2, ChecklistProviderM.GetEncodeChecklist2());
        }
    }
}
