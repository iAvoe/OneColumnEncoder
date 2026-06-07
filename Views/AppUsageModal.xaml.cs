using System.Windows;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder.Views
{
    public partial class AppUsageModal : AdaptiveWindow
    {
        public AppUsageModal()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
