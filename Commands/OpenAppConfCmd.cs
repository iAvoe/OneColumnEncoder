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
        private readonly AppConfS _appConfS;
        public OpenAppConfCmd(ModalNavS modalNavS, AppConfS appConfS)
        {
            _modalNavS = modalNavS;
            _appConfS = appConfS;
        }
        public override void Execute(object? parameter)
        {
            _modalNavS.CurrentModalVM = new AppConfVM(_modalNavS, _appConfS);
        }
    }
}
