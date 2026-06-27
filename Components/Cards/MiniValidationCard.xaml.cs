using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components.Cards
{
    public partial class MiniValidationCard : UserControl
    {
        public MiniValidationCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PaletteIndexProperty =
            DependencyProperty.Register(
                nameof(PaletteIndex),
                typeof(int),
                typeof(MiniValidationCard),
                new PropertyMetadata(0));

        public int PaletteIndex
        {
            get => (int)GetValue(PaletteIndexProperty);
            set => SetValue(PaletteIndexProperty, value);
        }

        public static readonly DependencyProperty IsCompactTextProperty =
            DependencyProperty.Register(
                nameof(IsCompactText),
                typeof(bool),
                typeof(MiniValidationCard),
                new PropertyMetadata(false));

        public bool IsCompactText
        {
            get => (bool)GetValue(IsCompactTextProperty);
            set => SetValue(IsCompactTextProperty, value);
        }
    }
}
