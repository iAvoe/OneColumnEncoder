using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class AppConfWindow : Window
    {
        public AppConfWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}