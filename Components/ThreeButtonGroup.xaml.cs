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

        public ThreeButtonGroup()
        {
            InitializeComponent();
        }
    }
}
