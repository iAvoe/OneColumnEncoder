using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ImageSource? B3_3Icon { get; set; }

        private string _b3_1Text = "";
        public string B3_1Text { get => _b3_1Text; set => SetProperty(ref _b3_1Text, value); }
        private string _b3_2Text = "";
        public string B3_2Text { get => _b3_2Text; set => SetProperty(ref _b3_2Text, value); }
        private string _b3_3Text = "";
        public string B3_3Text { get => _b3_3Text; set => SetProperty(ref _b3_3Text, value); }

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

        // Button commands
        public ICommand? Cmd1 { get; set; }
        public ICommand? Cmd2 { get; set; }
        public ICommand? Cmd3 { get; set; }

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
    }
}