using OneColumnEncoder.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// This ViewModel is for the Test SMTP, Confirm and Cancel buttons in the AppConfModal.
    /// This should be de-duplicated from the OpenAppConfButtonsVM in future.
    /// </summary>
    /// <param name="closeModalCmd">Cancel: close modal command</param>
    /// <param name="saveAppConfCmd">Confirm: save setting command, then close modal</param>
    public class SmtpConfirmCancelButtonsVM(CloseModalCmd closeModalCmd, SaveAppConfCmd saveAppConfCmd) : BaseVM
    {
        private string _b3_1Text = "Test SMTP"; // TODO
        public string B3_1Text
        {
            get => _b3_1Text;
            set => SetProperty(ref _b3_1Text, value);
        }

        private string _b3_2Text = "Cancel";
        public string B3_2Text
        {
            get => _b3_2Text;
            set => SetProperty(ref _b3_2Text, value);
        }

        private string _b3_3Text = "Confirm";
        public string B3_3Text
        {
            get => _b3_3Text;
            set => SetProperty(ref _b3_3Text, value);
        }
        // public TestSmtpCmd TestCmd { get; } = new TestSmtpCmd();
        public CloseModalCmd CancelCmd { get; } = closeModalCmd;
        public SaveAppConfCmd ConfirmCmd { get; } = saveAppConfCmd;
    }
}
