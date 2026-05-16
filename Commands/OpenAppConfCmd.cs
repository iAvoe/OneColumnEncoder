using OneColumnEncoder.Stores;
using OneColumnEncoder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class OpenAppConfCmd : BaseCmd
    {
        private readonly ModalNavS _modalNavS;
        public OpenAppConfCmd(ModalNavS modalNavS)
        {
            _modalNavS = modalNavS;
        }
        public override void Execute(object? parameter)
        {
            _modalNavS.CurrentModalVM = new AppConfVM();
        }
    }
}
