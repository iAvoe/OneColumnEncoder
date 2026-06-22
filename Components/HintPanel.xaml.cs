using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OneColumnEncoder.Components
{
    public partial class HintPanel : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HintPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty HintBrushProperty =
            DependencyProperty.Register(nameof(HintBrush), typeof(Brush), typeof(HintPanel),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Brush HintBrush
        {
            get => (Brush)GetValue(HintBrushProperty);
            set => SetValue(HintBrushProperty, value);
        }

        public HintPanel()
        {
            InitializeComponent();
            FontSize = 9.0;
            if (ReadLocalValue(HintBrushProperty) == DependencyProperty.UnsetValue)
                SetCurrentValue(HintBrushProperty, TryFindResource("GlobalTertiary") as Brush);
        }

        private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Text))
                Clipboard.SetText(Text);
        }
    }
}
