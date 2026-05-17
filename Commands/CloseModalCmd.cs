using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class CloseModalCmd(ModalNavS modalNavS) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;

        public override void Execute(object? parameter)
        {
            _modalNavS.Close();
        }
    }
}
