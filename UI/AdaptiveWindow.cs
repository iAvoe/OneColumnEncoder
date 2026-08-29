using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OneColumnEncoder.UI;

public partial class AdaptiveWindow : Window
{
    private const int MonitorDefaultToNearest = 2;

    public AdaptiveWindow() => Loaded += OnLoaded;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TextElement.SetFontFamily(this, AppFontProvider.UiFont);
        Dispatcher.BeginInvoke(ApplyAdaptiveBounds, DispatcherPriority.Loaded);
    }

    private void ApplyAdaptiveBounds()
    {
        Rect workArea = GetCurrentMonitorWorkArea();
        double maxHeight = double.IsInfinity(MaxHeight)
            ? workArea.Height
            : Math.Min(MaxHeight, workArea.Height);

        MaxHeight = maxHeight;

        double windowHeight = GetCurrentHeight();
        double windowWidth = GetCurrentWidth();

        if (windowHeight > maxHeight)
        {
            Height = maxHeight;
            windowHeight = maxHeight;
        }

        if (Top < workArea.Top) Top = workArea.Top;

        if (Top + windowHeight > workArea.Bottom)
            Top = Math.Max(workArea.Top, workArea.Bottom - windowHeight);

        if (Left < workArea.Left)
            Left = workArea.Left;

        if (Left + windowWidth > workArea.Right)
            Left = Math.Max(workArea.Left, workArea.Right - windowWidth);
    }

    private double GetCurrentHeight()
    {
        if (!double.IsNaN(Height)) return Height;
        return ActualHeight;
    }

    private double GetCurrentWidth()
    {
        if (!double.IsNaN(Width)) return Width;
        return ActualWidth;
    }

    /// <summary>
    /// Prevent auto-positioned window from going out bounds
    /// </summary>
    /// <returns>Rectangle object representing monitor</returns>
    /// <remarks>May not work on secondary or more monitor, testing looks ok though</remarks>
    private Rect GetCurrentMonitorWorkArea()
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        IntPtr monitor = MonitorFromWindow(handle, MonitorDefaultToNearest);

        if (monitor == IntPtr.Zero)
            return SystemParameters.WorkArea;

        MONITORINFO monitorInfo = new() { cbSize = Marshal.SizeOf<MONITORINFO>() };

        if (!GetMonitorInfo(monitor, ref monitorInfo))
            return SystemParameters.WorkArea;

        Matrix transformFromDevice =
            PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
            ?? Matrix.Identity;
        Point topLeft =
            transformFromDevice.Transform(new Point(monitorInfo.rcWork.Left, monitorInfo.rcWork.Top));
        Point bottomRight =
            transformFromDevice.Transform(new Point(monitorInfo.rcWork.Right, monitorInfo.rcWork.Bottom));

        return new Rect(topLeft, bottomRight);
    }

    [LibraryImport("user32.dll")]
    private static partial IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
