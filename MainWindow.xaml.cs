using OneColumnEncoder.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace OneColumnEncoder
{
    public partial class MainWindow : AdaptiveWindow
    {
        private DateTime _lastNumaCpuTrigger = DateTime.MinValue;
        private static readonly TimeSpan NumaCpuTriggerInterval = TimeSpan.FromMilliseconds(500);

        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                TriggerNumaCpuCheck();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            TriggerNumaCpuCheck();
        }

        private void TriggerNumaCpuCheck()
        {
            if (DateTime.UtcNow - _lastNumaCpuTrigger < NumaCpuTriggerInterval)
                return;
            _lastNumaCpuTrigger = DateTime.UtcNow;

            if (DataContext is ViewModels.MainVM vm)
                vm.RefreshNumaCpuCheck();
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            // Close all child windows before the main window closes
            foreach (Window? window in Application.Current.Windows.OfType<Window>().Except([this]).ToArray())
            {
                window.Close();
            }

            // Clear modal navigation state so no stale VM lingers
            if (Application.Current is App app)
            {
                app._modalNavM.Close();
            }
        }
    }
}
