using OneColumnEncoder.ViewModels;
using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class EncodingMonitorModal : Window
    {
        public EncodingMonitorModal()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EncodingMonitorModalVM vm)
                vm.Start();
        }
    }
}
