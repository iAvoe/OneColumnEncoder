using OneColumnEncoder.Helpers;
using OneColumnEncoder.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace OneColumnEncoder.Views
{
    public partial class ImageABPreviewModal : AdaptiveWindow
    {
        private const uint ScClose = 0xF060;
        private const uint MfByCommand = 0x00000000;
        private const uint MfGrayed = 0x00000001;
        private bool _allowClose;

        public ImageABPreviewModal()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SourceInitialized += OnSourceInitialized;
            Closing += OnClosing;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ImageABPreviewVM vm) return;

            vm.ZoomPresetButtons.Cmd1 = new OneColumnEncoder.Commands.ActionCmd(_ => PreviewViewer.Fit());
            vm.ZoomPresetButtons.Cmd2 = new OneColumnEncoder.Commands.ActionCmd(_ => PreviewViewer.SetActualSize());
            vm.ZoomPresetButtons.Cmd3 = new OneColumnEncoder.Commands.ActionCmd(_ => PreviewViewer.SetDoubleSize());
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DrawMenuBar(IntPtr hWnd);

        public void CloseFromOwner()
        {
            _allowClose = true;
            Close();
        }

        private void OnSourceInitialized(object? sender, EventArgs e) => UpdateSystemCloseButton(false);

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                return;
            }

            if (DataContext is ImageABPreviewVM vm)
                vm.Dispose();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;
            SourceInitialized -= OnSourceInitialized;
            Closing -= OnClosing;
            Closed -= OnClosed;
        }

        private void UpdateSystemCloseButton(bool isEnabled)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero) return;

            IntPtr menu = GetSystemMenu(handle, false);
            if (menu == IntPtr.Zero) return;

            EnableMenuItem(menu, ScClose, MfByCommand | (isEnabled ? MfByCommand : MfGrayed));
            DrawMenuBar(handle);
        }

        private void ZoomFineOut_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomFineOut();
        private void ZoomOut_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomOut();
        private void ZoomIn_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomIn();
        private void ZoomFineIn_Click(object sender, RoutedEventArgs e) => PreviewViewer.ZoomFineIn();
        private void Fit_Click(object sender, RoutedEventArgs e) => PreviewViewer.Fit();
        private void ActualSize_Click(object sender, RoutedEventArgs e) => PreviewViewer.SetActualSize();
        private void DoubleSize_Click(object sender, RoutedEventArgs e) => PreviewViewer.SetDoubleSize();
    }
}
