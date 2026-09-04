using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace OneColumnEncoder.Views;

public partial class EncodingMonitorModal : AdaptiveWindow
{
    private const uint ScClose = 0xF060;
    private const uint MfByCommand = 0x00000000;
    private const uint MfGrayed = 0x00000001;
    private const int DefaultWidth = 1080;
    private const int SidebarWidth = 310;
    private const int SplitterWidth = 2;
    private const int DefaultWidthWithSidebar = DefaultWidth + SidebarWidth + SplitterWidth;
    private const int MinWidthDefault = 860;
    private const int MinWidthSidebar = MinWidthDefault + SidebarWidth + SplitterWidth;

    private EncodingMonitorVM? _subscribedVm;
    private QueueSidebarVM? _subscribedQueueSidebar;

    public EncodingMonitorModal()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SourceInitialized += OnSrcInitialized;
        Closing += OnClosing;
        Closed += OnClosed;
    }

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DrawMenuBar(IntPtr hWnd);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is EncodingMonitorVM vm)
        {
            if (_subscribedVm != vm)
            {
                DetachViewModelEvents();
                _subscribedVm = vm;
                _subscribedQueueSidebar = vm.QueueSidebar;
                vm.QueueSidebar.PropertyChanged += OnQueueSidebarPropertyChanged;
                vm.PropertyChanged += OnViewModelPropertyChanged;
            }

            SyncSidebarWidth(vm.QueueSidebar.IsVisible);
            UpdateSystemCloseButton(vm.IsWindowCloseEnabled);
            vm.Start();
        }
    }

    private void OnSrcInitialized(object? sender, EventArgs e)
    {
        if (DataContext is EncodingMonitorVM vm)
            UpdateSystemCloseButton(vm.IsWindowCloseEnabled);
    }

    private void OnQueueSidebarPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QueueSidebarVM.IsVisible))
        {
            if (sender is QueueSidebarVM sidebar)
            {
                SyncSidebarWidth(sidebar.IsVisible);
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EncodingMonitorVM.IsWindowCloseEnabled)) return;
        if (sender is EncodingMonitorVM vm)
            UpdateSystemCloseButton(vm.IsWindowCloseEnabled);
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is EncodingMonitorVM vm && !vm.IsWindowCloseEnabled)
            e.Cancel = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        DetachViewModelEvents();
        Loaded -= OnLoaded;
        SourceInitialized -= OnSrcInitialized;
        Closing -= OnClosing;
        Closed -= OnClosed;
    }

    private void DetachViewModelEvents()
    {
        if (_subscribedQueueSidebar != null)
            _subscribedQueueSidebar.PropertyChanged -= OnQueueSidebarPropertyChanged;
        if (_subscribedVm != null)
            _subscribedVm.PropertyChanged -= OnViewModelPropertyChanged;

        _subscribedQueueSidebar = null;
        _subscribedVm = null;
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

    private void SyncSidebarWidth(bool isSidebarVisible)
    {
        if (isSidebarVisible) ExpandForSidebar();
        else CollapseSidebar();
    }

    private void ExpandForSidebar()
    {
        MinWidth = MinWidthSidebar;
        if (Width < DefaultWidthWithSidebar) Width = DefaultWidthWithSidebar;
    }

    private void CollapseSidebar()
    {
        MinWidth = MinWidthDefault;
        if (Width > DefaultWidth) Width = DefaultWidth;
    }
}
