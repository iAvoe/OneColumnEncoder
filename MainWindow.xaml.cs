using OneColumnEncoder.UI;
using System.ComponentModel;
using System.Reflection;
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
            Title = $"1cenc (Beta——Commit {GetGitCommitCount()} {GetGitCommitShortHash()})";
            Closing += OnClosing;
            Closed += OnClosed;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private static string GetGitCommitCount()
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => attribute.Key == "GitCommitCount")
                ?.Value ?? "0";
        }

        private static string GetGitCommitShortHash()
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => attribute.Key == "GitCommitShortHash")
                ?.Value ?? "unknown";
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                TriggerNumaCpuCheck();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e) => TriggerNumaCpuCheck();

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
                window.Close();

            if (Application.Current.Windows.OfType<Window>().Any(window => window != this))
            {
                e.Cancel = true;
                return;
            }

            // Clear modal navigation state so no stale VM lingers
            if (Application.Current is App app) app._modalNavM.CloseAll();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            if (DataContext is IDisposable disposable) disposable.Dispose();

            DataContext = null;
            Closing -= OnClosing;
            Closed -= OnClosed;
            PreviewMouseDown -= OnPreviewMouseDown;
            PreviewKeyDown -= OnPreviewKeyDown;
        }
    }
}
