using System.ComponentModel;
using System.Linq;
using System.Windows;
using OneColumnEncoder.Helpers;

namespace OneColumnEncoder
{
    public partial class MainWindow : AdaptiveWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
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
