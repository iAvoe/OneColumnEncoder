using OneColumnEncoder.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// This ViewModel is for the Confirm and Cancel buttons in the AppConfModal.
    /// This should be de-duplicated from the OpenAppConfButtonsVM in future.
    /// </summary>
    /// <param name="closeModalCmd">Cancel: close modal command</param>
    /// <param name="saveAppConfCmd">Confirm: save setting command, then close modal</param>
    public class ConfirmCancelButtonsVM(CloseModalCmd closeModalCmd, SaveAppConfCmd saveAppConfCmd) : BaseVM
    {
        private string _b2_1Text = "Cancel";
        public string B2_1Text
        {
            get => _b2_1Text;
            set => SetProperty(ref _b2_1Text, value);
        }

        private string _b2_2Text = "Confirm";
        public string B2_2Text
        {
            get => _b2_2Text;
            set => SetProperty(ref _b2_2Text, value);
        }
        // Cancel: close modal, Confirm: save settings, close modal
        public CloseModalCmd CancelCmd { get; } = closeModalCmd;
        public SaveAppConfCmd ConfirmCmd { get; } = saveAppConfCmd;
    }
}
