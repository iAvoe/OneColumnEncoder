using OneColumnEncoder.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands.OpenClose
{
    /// <summary>
    /// Close modal in both navigation store and window (if applicable)
    /// Usage¡ªconstructor: CloseCmd = new CloseModalCmd(modalNavS, closeAction);
    /// </summary>
    /// <param name="modalNavS"></param>
    /// <param name="closeAction"></param>
    public class CloseModalCmd(ModalNavS modalNavS, Action closeAction) : BaseCmd
    {
        private readonly ModalNavS _modalNavS = modalNavS;
        public override void Execute(object? parameter)
        {
            closeAction();
            _modalNavS.Close();
        }
    }
}
