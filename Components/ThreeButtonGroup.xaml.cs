using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Interaction logic for ThreeButtonGroup.xaml
    /// </summary>
    public partial class ThreeButtonGroup : UserControl
    {
        public static readonly DependencyProperty Button1CommandProperty =
            DependencyProperty.Register(nameof(Button1Command), typeof(ICommand), typeof(ThreeButtonGroup));

        public static readonly DependencyProperty Button2CommandProperty =
            DependencyProperty.Register(nameof(Button2Command), typeof(ICommand), typeof(ThreeButtonGroup));

        public static readonly DependencyProperty Button3CommandProperty =
            DependencyProperty.Register(nameof(Button3Command), typeof(ICommand), typeof(ThreeButtonGroup));

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(ThreeButtonGroup), new PropertyMetadata(40.0));

        public static readonly DependencyProperty Button1IsEnabledProperty =
            DependencyProperty.Register(nameof(Button1IsEnabled), typeof(bool), typeof(ThreeButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button2IsEnabledProperty =
            DependencyProperty.Register(nameof(Button2IsEnabled), typeof(bool), typeof(ThreeButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button3IsEnabledProperty =
            DependencyProperty.Register(nameof(Button3IsEnabled), typeof(bool), typeof(ThreeButtonGroup), new PropertyMetadata(true));

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
        public ICommand Button3Command
        {
            get => (ICommand)GetValue(Button3CommandProperty);
            set => SetValue(Button3CommandProperty, value);
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

        public bool Button3IsEnabled
        {
            get => (bool)GetValue(Button3IsEnabledProperty);
            set => SetValue(Button3IsEnabledProperty, value);
        }

        public ThreeButtonGroup()
        {
            InitializeComponent();
        }
    }
}
