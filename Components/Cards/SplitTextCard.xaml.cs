using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;

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
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(SplitTextCard), new PropertyMetadata(true));

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

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        private ScrollViewer? _leftScrollViewer;
        private ScrollViewer? _rightScrollViewer;

        public SplitTextCard()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _leftScrollViewer = FindScrollViewer(LeftTextBox);
            _rightScrollViewer = FindScrollViewer(RightTextBox);
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject visual)
        {
            if (visual is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                ScrollViewer? child = FindScrollViewer(VisualTreeHelper.GetChild(visual, i));
                if (child != null) return child;
            }
            return null;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            ScrollViewer? sv = textBox == LeftTextBox ? _leftScrollViewer : _rightScrollViewer;
            if (sv == null) return;

            bool isAtBottom = Math.Abs(sv.VerticalOffset + sv.ViewportHeight - sv.ExtentHeight) < 1d;
            if (isAtBottom)
            {
                Dispatcher.BeginInvoke(sv.ScrollToEnd, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
