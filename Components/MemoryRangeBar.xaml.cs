using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    public partial class MemoryRangeBar : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IEnumerable), typeof(MemoryRangeBar), new PropertyMetadata(null));

        public IEnumerable? Items
        {
            get => (IEnumerable?)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public MemoryRangeBar()
        {
            InitializeComponent();
        }
    }
}
