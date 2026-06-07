using OneColumnEncoder.ViewModels;
using System.Windows;
using OneColumnEncoder.Helpers;

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
            if (DataContext is EncodingMonitorModalVM vm)
                vm.Start();
        }
    }
}
