using OneColumnEncoder.Commands;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class UsageComplianceVM : BaseVM
    {
        public CloseModalCmd? CloseCmd { get; }
        public UsageComplianceVM(ModalNavS modelNavS, Action closeAction)
        {
            CloseCmd = new CloseModalCmd(modelNavS, closeAction);
        }
    }
}
