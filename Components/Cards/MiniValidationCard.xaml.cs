using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public static readonly DependencyProperty ToggleCommandProperty =
            DependencyProperty.Register(
                nameof(ToggleCommand),
                typeof(ICommand),
                typeof(MiniValidationCard),
                new PropertyMetadata(null));

        public ICommand? ToggleCommand
        {
            get => (ICommand?)GetValue(ToggleCommandProperty);
            set => SetValue(ToggleCommandProperty, value);
        }

        public static readonly DependencyProperty ToggleTagProperty =
            DependencyProperty.Register(
                nameof(ToggleTag),
                typeof(bool),
                typeof(MiniValidationCard),
                new PropertyMetadata(false));

        public bool ToggleTag
        {
            get => (bool)GetValue(ToggleTagProperty);
            set => SetValue(ToggleTagProperty, value);
        }

        public static readonly DependencyProperty ToggleToolTipProperty =
            DependencyProperty.Register(
                nameof(ToggleToolTip),
                typeof(object),
                typeof(MiniValidationCard),
                new PropertyMetadata(null));

        public object? ToggleToolTip
        {
            get => GetValue(ToggleToolTipProperty);
            set => SetValue(ToggleToolTipProperty, value);
        }
    }
}
