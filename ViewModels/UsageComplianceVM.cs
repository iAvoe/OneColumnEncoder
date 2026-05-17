using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    public class UsageComplianceVM : BaseVM
    {
        private ModalNavS modelNavS;

        public UsageComplianceVM(ModalNavS modelNavS)
        {
            this.modelNavS = modelNavS;
        }
    }
}
