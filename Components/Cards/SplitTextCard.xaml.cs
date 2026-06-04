using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components.Cards
{
    public partial class SplitTextCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SplitTextCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LeftTextProperty =
            DependencyProperty.Register(
                nameof(LeftText),
                typeof(string),
                typeof(SplitTextCard),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RightTextProperty =
            DependencyProperty.Register(
                nameof(RightText),
                typeof(string),
                typeof(SplitTextCard),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SplitTextCard), new PropertyMetadata(true));

        public static readonly DependencyProperty TextBoxMinHeightProperty =
            DependencyProperty.Register(nameof(TextBoxMinHeight), typeof(double), typeof(SplitTextCard), new PropertyMetadata(120.0));

        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(SplitTextCard), new PropertyMetadata(12.0));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string LeftText
        {
            get => (string)GetValue(LeftTextProperty);
            set => SetValue(LeftTextProperty, value);
        }

        public string RightText
        {
            get => (string)GetValue(RightTextProperty);
            set => SetValue(RightTextProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public double TextBoxMinHeight
        {
            get => (double)GetValue(TextBoxMinHeightProperty);
            set => SetValue(TextBoxMinHeightProperty, value);
        }

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        public SplitTextCard()
        {
            InitializeComponent();
        }
    }
}
