using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class CloseModalCmd : BaseCmd
    {
        private readonly ModalNavS _modalNavS;
        public CloseModalCmd(ModalNavS modalNavS)
        {
            _modalNavS = modalNavS;
        }
        public override void Execute(object? parameter)
        {
            _modalNavS.Close();
        }
    }
}
