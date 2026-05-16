using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OneColumnEncoder.Commands;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class AppConfVM : BaseVM
    {
        public ICommand CloseCmd { get; }

        public AppConfVM(ModalNavS modalNavS)
        {
            CloseCmd = new CloseModalCmd(modalNavS);
        }
    }
}
