using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Threading;

namespace OneColumnEncoder.Components
{
    public partial class ImgABPvViewer : UserControl
    {
        private bool _isPanning;
        private bool _isDraggingSplit;
        private Point _lastPanPoint;
        private double _splitRatio = 0.5d;
        private double _zoom = 1d;
        private double _offsetX;
        private double _offsetY;
        private ImgABPvVM? _subscribedVm;
        private bool _isFitQueued;

        public ImgABPvViewer()
        {
            InitializeComponent();
            Loaded += (_, _) => ApplyView();
            DataContextChanged += OnDataContextChanged;
            Unloaded += (_, _) => UnsubscribeViewModel();
        }

        public void ZoomIn() => SetZoom(_zoom + 0.01d, GetViewportCenter());
        public void ZoomOut() => SetZoom(_zoom - 0.01d, GetViewportCenter());
        public void ZoomFineIn() => SetZoom(_zoom + 0.1d, GetViewportCenter());
        public void ZoomFineOut() => SetZoom(_zoom - 0.1d, GetViewportCenter());
        public void Fit() => FitImage();
        public void SetActualSize() => SetZoom(1d, GetViewportCenter());
        public void SetDoubleSize() => SetZoom(2d, GetViewportCenter());

        private ImgABPvVM? ViewModel => DataContext as ImgABPvVM;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeViewModel();

            _subscribedVm = e.NewValue as ImgABPvVM;
            if (_subscribedVm != null)
                _subscribedVm.PropertyChanged += OnViewModelPropertyChanged;

            ApplyView();
        }

        private void UnsubscribeViewModel()
        {
            if (_subscribedVm != null)
                _subscribedVm.PropertyChanged -= OnViewModelPropertyChanged;
            _subscribedVm = null;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ImgABPvVM.SourceImage) or nameof(ImgABPvVM.EncodedImage))
            {
                QueueFitImage();
            }
        }

        private void Viewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSplitVisual();
            if (ViewModel?.IsFitMode == true)
                FitImage();
            else
                ApplyView();
        }

        private void Viewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel?.IsBusy == true) return;
            Point mousePoint = e.GetPosition(Viewport);
            SetZoom(_zoom * (e.Delta > 0 ? 1.15d : 1d / 1.15d), mousePoint);
            e.Handled = true;
        }

        private void Viewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingSplit) return;
            Focus();
            _isPanning = true;
            _lastPanPoint = e.GetPosition(Viewport);
            Cursor = Cursors.SizeAll;
            CaptureMouse();
        }

        private void Viewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isPanning) return;
            _isPanning = false;
            Cursor = Cursors.Arrow;
            ReleaseMouseCapture();
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning) return;
            Point current = e.GetPosition(Viewport);
            _offsetX += current.X - _lastPanPoint.X;
            _offsetY += current.Y - _lastPanPoint.Y;
            _lastPanPoint = current;
            ViewModel?.SetFitMode(false);
            ApplyView();
        }

        private void SplitHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSplit = true;
            SplitHandle.CaptureMouse();
            e.Handled = true;
        }

        private void SplitHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingSplit) return;
            _isDraggingSplit = false;
            SplitHandle.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void SplitHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingSplit) return;
            Point point = e.GetPosition(Viewport);
            double imageWidth = GetImagePixelWidth() * _zoom;
            if (imageWidth > 0d)
                _splitRatio = Math.Max(0d, Math.Min(1d, (point.X - _offsetX) / imageWidth));
            else
                _splitRatio = Math.Max(0d, Math.Min(1d, point.X / Math.Max(1d, Viewport.ActualWidth)));
            UpdateSplitVisual();
            e.Handled = true;
        }

        private void SetZoom(double zoom, Point anchor)
        {
            zoom = Math.Max(0.05d, Math.Min(16d, zoom));
            if (Math.Abs(zoom - _zoom) < 0.0001d) return;

            double imageX = (anchor.X - _offsetX) / _zoom;
            double imageY = (anchor.Y - _offsetY) / _zoom;
            _zoom = zoom;
            _offsetX = anchor.X - imageX * _zoom;
            _offsetY = anchor.Y - imageY * _zoom;
            ClampOffsetToViewport();
            ViewModel?.SetFitMode(false);
            ApplyView();
        }

        private void FitImage()
        {
            _isFitQueued = false;
            UpdateLayout();
            SyncImageLayoutSize(SourceImage);
            SyncImageLayoutSize(EncodedImage);

            BitmapSource? bitmap = SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource;
            double width = Viewport.ActualWidth;
            double height = Viewport.ActualHeight;
            if (bitmap == null || width <= 0 || height <= 0)
            {
                ApplyView();
                return;
            }

            double fitZoom = Math.Min(width / bitmap.PixelWidth, height / bitmap.PixelHeight);
            if (double.IsNaN(fitZoom) || double.IsInfinity(fitZoom) || fitZoom <= 0)
                fitZoom = 1d;

            _zoom = Math.Max(0.01d, fitZoom);
            _offsetX = (width - bitmap.PixelWidth * _zoom) / 2d;
            _offsetY = (height - bitmap.PixelHeight * _zoom) / 2d;
            ViewModel?.SetFitMode(true);
            ApplyView();
        }

        private void QueueFitImage()
        {
            if (_isFitQueued) return;
            _isFitQueued = true;
            Dispatcher.BeginInvoke(FitImage, DispatcherPriority.ContextIdle);
        }

        private Point GetViewportCenter() =>
            new(Math.Max(0d, Viewport.ActualWidth) / 2d, Math.Max(0d, Viewport.ActualHeight) / 2d);

        private void ApplyView()
        {
            ClampOffsetToViewport();
            SyncImageLayoutSize(SourceImage);
            SyncImageLayoutSize(EncodedImage);
            Canvas.SetLeft(SourceImage, _offsetX);
            Canvas.SetTop(SourceImage, _offsetY);
            Canvas.SetLeft(EncodedImage, _offsetX);
            Canvas.SetTop(EncodedImage, _offsetY);
            ViewModel?.SetZoomPercent((int)Math.Round(_zoom * 100d));
            UpdateSplitVisual();
        }

        private void SyncImageLayoutSize(Image image)
        {
            if (image.Source is not BitmapSource bitmap) return;
            image.Width = Math.Max(1d, bitmap.PixelWidth * _zoom);
            image.Height = Math.Max(1d, bitmap.PixelHeight * _zoom);
        }

        private void ClampOffsetToViewport()
        {
            BitmapSource? bitmap = GetActiveBitmap();
            if (bitmap == null) return;

            double viewportWidth = Math.Max(0d, Viewport.ActualWidth);
            double viewportHeight = Math.Max(0d, Viewport.ActualHeight);
            double imageWidth = bitmap.PixelWidth * _zoom;
            double imageHeight = bitmap.PixelHeight * _zoom;

            _offsetX = ClampOffset(_offsetX, imageWidth, viewportWidth);
            _offsetY = ClampOffset(_offsetY, imageHeight, viewportHeight);
        }

        private static double ClampOffset(double offset, double contentSize, double viewportSize)
        {
            if (contentSize <= 0d || viewportSize <= 0d) return offset;
            if (contentSize <= viewportSize)
                return (viewportSize - contentSize) / 2d;

            double minOffset = viewportSize - contentSize;
            double maxOffset = 0d;
            return Math.Max(minOffset, Math.Min(maxOffset, offset));
        }

        private BitmapSource? GetActiveBitmap() => SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource;

        private double GetImagePixelWidth() =>
            (SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource)?.PixelWidth ?? 0d;

        private double GetImagePixelHeight() =>
            (SourceImage.Source as BitmapSource ?? EncodedImage.Source as BitmapSource)?.PixelHeight ?? 0d;

        private void UpdateSplitVisual()
        {
            double viewportWidth = Math.Max(0d, Viewport.ActualWidth);
            double imageWidth = GetImagePixelWidth() * _zoom;
            double splitX = imageWidth > 0d
                ? _offsetX + imageWidth * _splitRatio
                : viewportWidth * _splitRatio;
            splitX = Math.Max(0d, Math.Min(viewportWidth, splitX));
            SourceLayer.Width = splitX;
            SourceLayer.Height = Math.Max(0d, Viewport.ActualHeight);
            SplitHandle.Height = Math.Max(0d, Viewport.ActualHeight);
            Canvas.SetLeft(SplitHandle, Math.Max(0d, splitX - SplitHandle.Width / 2d));
            Canvas.SetTop(SplitHandle, 0d);
        }
    }
}
