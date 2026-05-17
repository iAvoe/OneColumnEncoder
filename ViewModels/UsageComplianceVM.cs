using OneColumnEncoder.Commands;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class UsageComplianceVM : BaseVM
    {
        public ICommand CloseCmd { get; set; }

        public UsageComplianceVM(ModalNavS modelNavS)
        {
            CloseCmd = new CloseModalCmd(modelNavS);
        }
    }
}
