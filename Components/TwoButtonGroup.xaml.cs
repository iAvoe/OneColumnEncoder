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

        public TwoButtonGroup()
        {
            InitializeComponent();
        }
    }
}
