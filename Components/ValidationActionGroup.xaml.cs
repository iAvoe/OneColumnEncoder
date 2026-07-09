using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    public partial class ValidationActionGroup : UserControl
    {
        public static readonly DependencyProperty CardMarginProperty =
            DependencyProperty.Register(
                nameof(CardMargin),
                typeof(Thickness),
                typeof(ValidationActionGroup),
                new PropertyMetadata(new Thickness(5, 5, 5, 0)));

        public static readonly DependencyProperty ButtonsMarginProperty =
            DependencyProperty.Register(
                nameof(ButtonsMargin),
                typeof(Thickness),
                typeof(ValidationActionGroup),
                new PropertyMetadata(new Thickness(10, 10, 10, 0)));

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(
                nameof(ButtonHeight),
                typeof(double),
                typeof(ValidationActionGroup),
                new PropertyMetadata(30.0));

        public static readonly DependencyProperty MiddleContentProperty =
            DependencyProperty.Register(
                nameof(MiddleContent),
                typeof(object),
                typeof(ValidationActionGroup),
                new PropertyMetadata(null));

        public Thickness CardMargin
        {
            get => (Thickness)GetValue(CardMarginProperty);
            set => SetValue(CardMarginProperty, value);
        }

        public Thickness ButtonsMargin
        {
            get => (Thickness)GetValue(ButtonsMarginProperty);
            set => SetValue(ButtonsMarginProperty, value);
        }

        public double ButtonHeight
        {
            get => (double)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        public object? MiddleContent
        {
            get => GetValue(MiddleContentProperty);
            set => SetValue(MiddleContentProperty, value);
        }

        public ValidationActionGroup()
        {
            InitializeComponent();
        }
    }
}
