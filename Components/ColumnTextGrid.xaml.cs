namespace OneColumnEncoder.Components
{
    public partial class ColumnTextGrid : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(
                nameof(Items),
                typeof(ObservableCollection<ColumnTextItemM>),
                typeof(ColumnTextGrid),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(
                nameof(ColumnCount),
                typeof(int),
                typeof(ColumnTextGrid),
                new PropertyMetadata(1));

        public ObservableCollection<ColumnTextItemM>? Items
        {
            get => (ObservableCollection<ColumnTextItemM>?)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public int ColumnCount
        {
            get => (int)GetValue(ColumnCountProperty);
            set => SetValue(ColumnCountProperty, value);
        }

        public ColumnTextGrid()
        {
            InitializeComponent();
        }
    }
}
