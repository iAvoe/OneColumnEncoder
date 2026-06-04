using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    public partial class HeatMapGrid : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(IEnumerable), typeof(HeatMapGrid), new PropertyMetadata(null));

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(HeatMapGrid), new PropertyMetadata(32));

        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register(nameof(CellSize), typeof(double), typeof(HeatMapGrid), new PropertyMetadata(15.0));

        public IEnumerable? Items
        {
            get => (IEnumerable?)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public double CellSize
        {
            get => (double)GetValue(CellSizeProperty);
            set => SetValue(CellSizeProperty, value);
        }

        public HeatMapGrid()
        {
            InitializeComponent();
        }
    }
}
