using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class OpenUsagesCmd(ModalNavS modelNavS) : BaseCmd
    {
        private readonly ModalNavS _modelNavS = modelNavS;
        public override void Execute(object? parameter)
        {
            _modelNavS.CurrentModalVM = new UsageComplianceVM(_modelNavS);
        }
    }
}
