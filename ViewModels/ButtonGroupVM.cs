using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// Centralized ViewModel for button groups in modals, to reduce code duplication.
    /// </summary>
    public class ButtonGroupVM : BaseVM
    {
        private string _b1_1Text = "";
        public string B1_1Text { get => _b1_1Text; set => SetProperty(ref _b1_1Text, value); }

        private string _b2_1Text = "";
        public string B2_1Text { get => _b2_1Text; set => SetProperty(ref _b2_1Text, value); }
        private string _b2_2Text = "";
        public string B2_2Text { get => _b2_2Text; set => SetProperty(ref _b2_2Text, value); }

        public ImageSource? B2_1Icon { get; set; }
        public ImageSource? B2_2Icon { get; set; }
        public ImageSource? B3_1Icon { get; set; }
        public ImageSource? B3_2Icon { get; set; }
        public ImageSource? B3_3Icon { get; set; }
        public ImageSource? B5_1Icon { get; set; }
        public ImageSource? B5_2Icon { get; set; }
        public ImageSource? B5_3Icon { get; set; }
        public ImageSource? B5_4Icon { get; set; }
        public ImageSource? B5_5Icon { get; set; }

        private string _b3_1Text = "";
        public string B3_1Text { get => _b3_1Text; set => SetProperty(ref _b3_1Text, value); }
        private string _b3_2Text = "";
        public string B3_2Text { get => _b3_2Text; set => SetProperty(ref _b3_2Text, value); }
        private string _b3_3Text = "";
        public string B3_3Text { get => _b3_3Text; set => SetProperty(ref _b3_3Text, value); }

        private string _b5_1Text = "";
        public string B5_1Text { get => _b5_1Text; set => SetProperty(ref _b5_1Text, value); }
        private string _b5_2Text = "";
        public string B5_2Text { get => _b5_2Text; set => SetProperty(ref _b5_2Text, value); }
        private string _b5_3Text = "";
        public string B5_3Text { get => _b5_3Text; set => SetProperty(ref _b5_3Text, value); }
        private string _b5_4Text = "";
        public string B5_4Text { get => _b5_4Text; set => SetProperty(ref _b5_4Text, value); }
        private string _b5_5Text = "";
        public string B5_5Text { get => _b5_5Text; set => SetProperty(ref _b5_5Text, value); }

        // Button states
        private bool _b1_1IsEnabled = true;
        public bool B1_1IsEnabled
        {
            get => _b1_1IsEnabled;
            set => SetProperty(ref _b1_1IsEnabled, value);
        }

        private bool _b2_1IsEnabled = true;
        public bool B2_1IsEnabled
        {
            get => _b2_1IsEnabled;
            set => SetProperty(ref _b2_1IsEnabled, value);
        }
        private bool _b2_2IsEnabled = true;
        public bool B2_2IsEnabled
        {
            get => _b2_2IsEnabled;
            set => SetProperty(ref _b2_2IsEnabled, value);
        }
        private bool _b2_2Highlight;
        public bool B2_2Highlight
        {
            get => _b2_2Highlight;
            set => SetProperty(ref _b2_2Highlight, value);
        }
        private bool _b2_2Strikethrough;
        public bool B2_2Strikethrough
        {
            get => _b2_2Strikethrough;
            set => SetProperty(ref _b2_2Strikethrough, value);
        }
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

        private bool _b5_1IsEnabled = true;
        public bool B5_1IsEnabled
        {
            get => _b5_1IsEnabled;
            set => SetProperty(ref _b5_1IsEnabled, value);
        }
        private bool _b5_2IsEnabled = true;
        public bool B5_2IsEnabled
        {
            get => _b5_2IsEnabled;
            set => SetProperty(ref _b5_2IsEnabled, value);
        }
        private bool _b5_3IsEnabled = true;
        public bool B5_3IsEnabled
        {
            get => _b5_3IsEnabled;
            set => SetProperty(ref _b5_3IsEnabled, value);
        }
        private bool _b5_4IsEnabled = true;
        public bool B5_4IsEnabled
        {
            get => _b5_4IsEnabled;
            set => SetProperty(ref _b5_4IsEnabled, value);
        }
        private bool _b5_5IsEnabled = true;
        public bool B5_5IsEnabled
        {
            get => _b5_5IsEnabled;
            set => SetProperty(ref _b5_5IsEnabled, value);
        }

        // Button commands
        private ICommand? _cmd1;
        public ICommand? Cmd1 { get => _cmd1; set => SetProperty(ref _cmd1, value); }
        private ICommand? _cmd2;
        public ICommand? Cmd2 { get => _cmd2; set => SetProperty(ref _cmd2, value); }
        private ICommand? _cmd3;
        public ICommand? Cmd3 { get => _cmd3; set => SetProperty(ref _cmd3, value); }
        public ICommand? Cmd4 { get; set; }
        public ICommand? Cmd5 { get; set; }

        public static ButtonGroupVM CreatePrimaryButton(string text, ICommand? cmd = null)
        {
            return new ButtonGroupVM
            {
                B1_1Text = text,
                Cmd1 = cmd,
            };
        }
        public static ButtonGroupVM CreateTwoButton(string b1Text, string b2Text, ICommand? cmd1 = null, ICommand? cmd2 = null)
        {
            return new ButtonGroupVM
            {
                B2_1Text = b1Text,
                B2_2Text = b2Text,
                Cmd1 = cmd1,
                Cmd2 = cmd2
            };
        }
        public static ButtonGroupVM CreateThreeButton(string b1Text, string b2Text, string b3Text, ICommand? cmd1 = null, ICommand? cmd2 = null, ICommand? cmd3 = null)
        {
            return new ButtonGroupVM
            {
                B3_1Text = b1Text,
                B3_2Text = b2Text,
                B3_3Text = b3Text,
                Cmd1 = cmd1,
                Cmd2 = cmd2,
                Cmd3 = cmd3
            };
        }
        public static ButtonGroupVM CreateFiveButton(string b1Text, string b2Text, string b3Text, string b4Text, string b5Text, ICommand? cmd1 = null, ICommand? cmd2 = null, ICommand? cmd3 = null, ICommand? cmd4 = null, ICommand? cmd5 = null)
        {
            return new ButtonGroupVM
            {
                B5_1Text = b1Text,
                B5_2Text = b2Text,
                B5_3Text = b3Text,
                B5_4Text = b4Text,
                B5_5Text = b5Text,
                Cmd1 = cmd1,
                Cmd2 = cmd2,
                Cmd3 = cmd3,
                Cmd4 = cmd4,
                Cmd5 = cmd5
            };
        }
    }
}
