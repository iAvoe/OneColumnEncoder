using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components.Cards
{
    public partial class MiniItemCard : UserControl
    {
        public MiniItemCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(MiniItemCard),
                new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsCancelProperty =
            DependencyProperty.Register(
                nameof(IsCancel),
                typeof(bool),
                typeof(MiniItemCard),
                new PropertyMetadata(false));

        public bool IsCancel
        {
            get => (bool)GetValue(IsCancelProperty);
            set => SetValue(IsCancelProperty, value);
        }

        public static readonly DependencyProperty ShowActionButtonsProperty =
            DependencyProperty.Register(
                nameof(ShowActionButtons),
                typeof(bool),
                typeof(MiniItemCard),
                new PropertyMetadata(true));

        public bool ShowActionButtons
        {
            get => (bool)GetValue(ShowActionButtonsProperty);
            set => SetValue(ShowActionButtonsProperty, value);
        }

        public static readonly DependencyProperty IsCompactTextProperty =
            DependencyProperty.Register(
                nameof(IsCompactText),
                typeof(bool),
                typeof(MiniItemCard),
                new PropertyMetadata(false));

        public bool IsCompactText
        {
            get => (bool)GetValue(IsCompactTextProperty);
            set => SetValue(IsCompactTextProperty, value);
        }

        public static readonly DependencyProperty PaletteIndexProperty =
            DependencyProperty.Register(
                nameof(PaletteIndex),
                typeof(int),
                typeof(MiniItemCard),
                new PropertyMetadata(0));

        public int PaletteIndex
        {
            get => (int)GetValue(PaletteIndexProperty);
            set => SetValue(PaletteIndexProperty, value);
        }
    }
}
