using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands.OpenClose
{
    public class OpenWarnModalCmd(ModalNavS modalNavS) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        public override void Execute(object? parameter)
        {
            /*
            var window = WarnModal();
            window.DataContext = new WarnVM(_modalNavS, window.Close);
            window.ShowDialog();
            */
            throw new NotImplementedException();
        }
    }
}
