using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneColumnEncoder.ViewModels
{
    // Using ThreeButtonGroup component
    public class EncodingStartButtonsVM : BaseVM
    {
        private string _b3_1Text = "Re-Evaluate";
        public string B3_1Text
        {
            get => _b3_1Text;
            set => SetProperty(ref _b3_1Text, value);
        }

        private string _b3_2Text = "Run a Sample";
        public string B3_2Text
        {
            get => _b3_2Text;
            set => SetProperty(ref _b3_2Text, value);
        }

        private string _b3_3Text = "Start Encode";
        public string B3_3Text
        {
            get => _b3_3Text;
            set => SetProperty(ref _b3_3Text, value);
        }

        /*
        TODO:
        public ICommand ReEvaluate { get; }
        public ICommand SampleClip { get; }
        public ICommand StartEncode { get; }
        */
    }
}
