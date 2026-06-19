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
    }
}
