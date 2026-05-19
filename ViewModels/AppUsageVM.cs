using OneColumnEncoder.Commands.OpenClose;
using OneColumnEncoder.Stores;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    public class AppUsageVM : BaseVM
    {
        public CloseModalCmd? CloseCmd { get; }
        public string B1_1Text { get; } = "Close";
        public AppUsageVM(ModalNavS modelNavS, Action closeAction)
        {
            CloseCmd = new CloseModalCmd(modelNavS, closeAction);
        }
    }
}
