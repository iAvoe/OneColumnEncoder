using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OneColumnEncoder.Components
{
    public partial class TwoButtonGroup : UserControl
    {
        public static readonly DependencyProperty Button1CommandProperty =
            DependencyProperty.Register(nameof(Button1Command), typeof(ICommand), typeof(TwoButtonGroup));

        public static readonly DependencyProperty Button2CommandProperty =
            DependencyProperty.Register(nameof(Button2Command), typeof(ICommand), typeof(TwoButtonGroup));

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(TwoButtonGroup), new PropertyMetadata(40.0));

        public static readonly DependencyProperty Button1IsEnabledProperty =
            DependencyProperty.Register(nameof(Button1IsEnabled), typeof(bool), typeof(TwoButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button2IsEnabledProperty =
            DependencyProperty.Register(nameof(Button2IsEnabled), typeof(bool), typeof(TwoButtonGroup), new PropertyMetadata(true));

        public ICommand Button1Command
        {
            get => (ICommand)GetValue(Button1CommandProperty);
            set => SetValue(Button1CommandProperty, value);
        }

        public ICommand Button2Command
        {
            get => (ICommand)GetValue(Button2CommandProperty);
            set => SetValue(Button2CommandProperty, value);
        }

        public double ButtonHeight
        {
            get => (double)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        public bool Button1IsEnabled
        {
            get => (bool)GetValue(Button1IsEnabledProperty);
            set => SetValue(Button1IsEnabledProperty, value);
        }

        public bool Button2IsEnabled
        {
            get => (bool)GetValue(Button2IsEnabledProperty);
            set => SetValue(Button2IsEnabledProperty, value);
        }

        public TwoButtonGroup()
        {
            InitializeComponent();
        }
    }
}
