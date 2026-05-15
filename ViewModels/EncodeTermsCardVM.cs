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
    public class EncodeTermsCardVM : ValidationCardBaseVM
    {
        public EncodeTermsCardVM()
        {
            FillCollection(Checklist1, ChecklistProviderS.GetEncodeChecklist1());
            FillCollection(Checklist2, ChecklistProviderS.GetEncodeChecklist2());
        }
    }
}
