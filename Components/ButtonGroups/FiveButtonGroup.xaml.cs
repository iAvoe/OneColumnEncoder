namespace OneColumnEncoder.Components
{
    public partial class FiveButtonGroup : UserControl
    {
        public static readonly DependencyProperty Button1CommandProperty =
            DependencyProperty.Register(nameof(Button1Command), typeof(ICommand), typeof(FiveButtonGroup));

        public static readonly DependencyProperty Button2CommandProperty =
            DependencyProperty.Register(nameof(Button2Command), typeof(ICommand), typeof(FiveButtonGroup));

        public static readonly DependencyProperty Button3CommandProperty =
            DependencyProperty.Register(nameof(Button3Command), typeof(ICommand), typeof(FiveButtonGroup));

        public static readonly DependencyProperty Button4CommandProperty =
            DependencyProperty.Register(nameof(Button4Command), typeof(ICommand), typeof(FiveButtonGroup));

        public static readonly DependencyProperty Button5CommandProperty =
            DependencyProperty.Register(nameof(Button5Command), typeof(ICommand), typeof(FiveButtonGroup));

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(FiveButtonGroup), new PropertyMetadata(40.0));

        public static readonly DependencyProperty Button1IsEnabledProperty =
            DependencyProperty.Register(nameof(Button1IsEnabled), typeof(bool), typeof(FiveButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button2IsEnabledProperty =
            DependencyProperty.Register(nameof(Button2IsEnabled), typeof(bool), typeof(FiveButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button3IsEnabledProperty =
            DependencyProperty.Register(nameof(Button3IsEnabled), typeof(bool), typeof(FiveButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button4IsEnabledProperty =
            DependencyProperty.Register(nameof(Button4IsEnabled), typeof(bool), typeof(FiveButtonGroup), new PropertyMetadata(true));

        public static readonly DependencyProperty Button5IsEnabledProperty =
            DependencyProperty.Register(nameof(Button5IsEnabled), typeof(bool), typeof(FiveButtonGroup), new PropertyMetadata(true));

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
        public ICommand Button4Command
        {
            get => (ICommand)GetValue(Button4CommandProperty);
            set => SetValue(Button4CommandProperty, value);
        }
        public ICommand Button5Command
        {
            get => (ICommand)GetValue(Button5CommandProperty);
            set => SetValue(Button5CommandProperty, value);
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
        public bool Button4IsEnabled
        {
            get => (bool)GetValue(Button4IsEnabledProperty);
            set => SetValue(Button4IsEnabledProperty, value);
        }
        public bool Button5IsEnabled
        {
            get => (bool)GetValue(Button5IsEnabledProperty);
            set => SetValue(Button5IsEnabledProperty, value);
        }

        public FiveButtonGroup()
        {
            InitializeComponent();
        }
    }
}
