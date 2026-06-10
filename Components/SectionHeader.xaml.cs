using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Interaction logic for SectionHeader.xaml
    /// </summary>
    public partial class SectionHeader : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
                "HeaderText",
                typeof(string),
                typeof(SectionHeader),
                new PropertyMetadata(string.Empty));

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        public SectionHeader()
        {
            InitializeComponent();
        }
    }
}
