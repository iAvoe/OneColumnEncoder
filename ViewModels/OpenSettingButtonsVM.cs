using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    // Using TwoButtonGroup component
    public class OpenSettingButtonsVM : BaseVM
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

        /*
        TODO:
        public ICommand ReservedButton { get; }
        public ICommand OpenSettings { get; }
        */
    }
}
