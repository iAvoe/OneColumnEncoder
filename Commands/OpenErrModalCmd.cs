using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    class OpenErrModalCmd(ModalNavS modalNavS) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;  
        public override void Execute(object? parameter)
        {
            /*
            var window = ErrModal();
            window.DataContext = new ErrVM(_modalNavS, window.Close);
            window.ShowDialog();
            */
            throw new NotImplementedException();
        }
    }
}
