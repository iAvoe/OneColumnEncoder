using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class AppConfModal : Window
    {
        public AppConfModal()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}