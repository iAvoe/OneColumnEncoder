using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components.Cards
{
    public partial class CPUNodeCardGroup : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(CPUNodeCardGroup),
                new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public CPUNodeCardGroup()
        {
            InitializeComponent();
        }
    }
}
