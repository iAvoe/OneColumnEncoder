using OneColumnEncoder.Helpers;
using OneColumnEncoder.ViewModels;
using System.Windows;

namespace OneColumnEncoder.Views
{
    public partial class EncodingMonitorModal : AdaptiveWindow
    {
        public EncodingMonitorModal()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EncodingMonitorVM vm)
                vm.Start();
        }
    }
}
