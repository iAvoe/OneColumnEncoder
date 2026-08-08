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

        public static readonly DependencyProperty Button2HighlightProperty =
            DependencyProperty.Register(nameof(Button2Highlight), typeof(bool), typeof(TwoButtonGroup), new PropertyMetadata(false));

        public static readonly DependencyProperty Button2StrikethroughProperty =
            DependencyProperty.Register(nameof(Button2Strikethrough), typeof(bool), typeof(TwoButtonGroup), new PropertyMetadata(false));

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

        public bool Button2Highlight
        {
            get => (bool)GetValue(Button2HighlightProperty);
            set => SetValue(Button2HighlightProperty, value);
        }

        public bool Button2Strikethrough
        {
            get => (bool)GetValue(Button2StrikethroughProperty);
            set => SetValue(Button2StrikethroughProperty, value);
        }

        public TwoButtonGroup()
        {
            InitializeComponent();
        }
    }
}
