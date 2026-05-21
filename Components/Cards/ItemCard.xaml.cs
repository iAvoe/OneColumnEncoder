using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Interaction logic for ItemCard.xaml
    /// </summary>
    public partial class ItemCard : UserControl
    {
        public ItemCard()
        {
            InitializeComponent();
        }

        // Register IsSelected depending property
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(ItemCard),
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
                typeof(ItemCard),
                new PropertyMetadata(false));

        public bool IsCancel
        {
            get => (bool)GetValue(IsCancelProperty);
            set => SetValue(IsCancelProperty, value);
        }
    }
}
