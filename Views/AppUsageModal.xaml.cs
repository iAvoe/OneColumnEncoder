using OneColumnEncoder.Helpers;
using System.Windows;

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
