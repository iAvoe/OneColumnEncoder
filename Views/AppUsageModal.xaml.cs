using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class AppUsageModal : Window
    {
        public AppUsageModal()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}