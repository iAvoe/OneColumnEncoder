using OneColumnEncoder.Commands;
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

        public ICommand ReEvaluate { get; } = new ReEvaluateCmd();
        public ICommand SampleClip { get; } = new SampleClipCmd();
        public ICommand StartEncode { get; } = new StartEncodeCmd();

        private bool _b3_1IsEnabled = true;
        public bool B3_1IsEnabled
        {
            get => _b3_1IsEnabled;
            set => SetProperty(ref _b3_1IsEnabled, value);
        }

        private bool _b3_2IsEnabled = true;
        public bool B3_2IsEnabled
        {
            get => _b3_2IsEnabled;
            set => SetProperty(ref _b3_2IsEnabled, value);
        }

        private bool _b3_3IsEnabled = true;
        public bool B3_3IsEnabled
        {
            get => _b3_3IsEnabled;
            set => SetProperty(ref _b3_3IsEnabled, value);
        }
    }
}
