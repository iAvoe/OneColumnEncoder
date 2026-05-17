using OneColumnEncoder.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    // Using TwoButtonGroup component
    public class OpenAppConfButtonsVM(OpenAppConfCmd openAppConf, OpenUsagesCmd openUsages) : BaseVM
    {
        private string _b2_1Text = "Usage & Compliance";
        public string B2_1Text
        {
            get => _b2_1Text;
            set => SetProperty(ref _b2_1Text, value);
        }

        private string _b2_2Text = "Settings";
        public string B2_2Text
        {
            get => _b2_2Text;
            set => SetProperty(ref _b2_2Text, value);
        }

        public OpenUsagesCmd OpenUsage { get; } = openUsages;
        public OpenAppConfCmd OpenAppConf { get; } = openAppConf;
    }
}
