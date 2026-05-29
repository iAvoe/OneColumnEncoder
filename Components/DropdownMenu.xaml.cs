using OneColumnEncoder.ViewModels;
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
    /// Interaction logic for DropdownMenu.xaml
    /// </summary>
    public partial class DropdownMenu : UserControl
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(System.Collections.IEnumerable), typeof(DropdownMenu));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(DropdownMenu), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public System.Collections.IEnumerable Items
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public DropdownMenu()
        {
            InitializeComponent();
        }
    }
}
